using dnlib.DotNet;

namespace Silhouette.IL;

internal class CorLibTypes : ICorLibTypes, IDisposable
{
    private readonly ComPtr<IMetaDataImport2> _metadataImport;
    private readonly ComPtr<IMetaDataImport2> _corLibMetadataImport;
    private readonly ModuleId _moduleId;
    private readonly ICorProfilerInfo3 _corProfilerInfo;

    internal static HResult<CorLibTypes> Create(ComPtr<IMetaDataImport2> metadataImport, ICorProfilerInfo3 corProfilerInfo, ModuleId moduleId)
    {
        var (result, corLib) = FindCorLib(corProfilerInfo);

        if (!result)
        {
            return result;
        }

        using var corLibPtr = corLib.Wrap();

        return new CorLibTypes(metadataImport, corLibPtr, moduleId, corProfilerInfo);
    }

    private CorLibTypes(ComPtr<IMetaDataImport2> metadataImport, ComPtr<IMetaDataImport2> corLibMetadataImport, ModuleId moduleId, ICorProfilerInfo3 corProfilerInfo)
    {
        _metadataImport = metadataImport.Copy();
        _corLibMetadataImport = corLibMetadataImport.Copy();
        _moduleId = moduleId;
        _corProfilerInfo = corProfilerInfo;
    }

    private static HResult<IMetaDataImport2> FindCorLib(ICorProfilerInfo3 corProfilerInfo)
    {
        var (result, moduleEnumerator) = corProfilerInfo.EnumModules();

        if (!result)
        {
            Console.WriteLine($"Failed to enumerate modules: {result}");
            return result;
        }

        using var _ = moduleEnumerator;

        foreach (var module in moduleEnumerator.AsEnumerable())
        {
            (result, var props) = corProfilerInfo.GetModuleInfo(module);

            if (!result)
            {
                continue;
            }

            var moduleName = Path.GetFileNameWithoutExtension(props.ModuleName);

            if ("mscorlib".Equals(moduleName, StringComparison.OrdinalIgnoreCase)
                || "System.Private.CoreLib".Equals(moduleName, StringComparison.OrdinalIgnoreCase))
            {
                // TODO: use ofWrite only if needed
                // TODO: double check if this is the correct module
                return corProfilerInfo.GetModuleMetaDataImport2(module, CorOpenFlags.ofRead | CorOpenFlags.ofWrite);
            }
        }

        return new(HResult.E_FAIL, default);
    }

    public void Dispose()
    {
        _metadataImport.Dispose();
    }

    public TypeRef GetTypeRef(string @namespace, string name)
    {
        var fullName = string.IsNullOrEmpty(@namespace) ? name : $"{@namespace}.{name}";

        // Try to find an existing TypeRef first
        var existing = FindTypeRef(fullName);
        if (existing != null)
        {
            return existing;
        }

        // Create a new one via IMetaDataEmit
        var asmRef = AssemblyRef;
        if (asmRef == null)
        {
            return null;
        }

        using var metaDataEmit = _corProfilerInfo.GetModuleMetaDataEmit(_moduleId, CorOpenFlags.ofRead | CorOpenFlags.ofWrite)
            .ThrowIfFailed()
            .Wrap();

        var typeRef = metaDataEmit.Value.DefineTypeRefByName(
            new MdToken((int)asmRef.MDToken.Raw), fullName).ThrowIfFailed();

        return new TypeRefUser(null, @namespace, name) { Rid = MDToken.ToRID((uint)typeRef.Value) };
    }

    public CorLibTypeSig Void => new(new TypeDefUser("void"), ElementType.Void);
    public CorLibTypeSig Boolean => ResolveTypeSig("System.Boolean", ElementType.Boolean);
    public CorLibTypeSig Char => ResolveTypeSig("System.Char", ElementType.Char);
    public CorLibTypeSig SByte => ResolveTypeSig("System.SByte", ElementType.I1);
    public CorLibTypeSig Byte => ResolveTypeSig("System.Byte", ElementType.U1);
    public CorLibTypeSig Int16 => ResolveTypeSig("System.Int16", ElementType.I2);
    public CorLibTypeSig UInt16 => ResolveTypeSig("System.UInt16", ElementType.U2);
    public CorLibTypeSig Int32 => ResolveTypeSig("System.Int32", ElementType.I4);
    public CorLibTypeSig UInt32 => ResolveTypeSig("System.UInt32", ElementType.U4);
    public CorLibTypeSig Int64 => ResolveTypeSig("System.Int64", ElementType.I8);
    public CorLibTypeSig UInt64 => ResolveTypeSig("System.UInt64", ElementType.U8);
    public CorLibTypeSig Single => ResolveTypeSig("System.Single", ElementType.R4);
    public CorLibTypeSig Double => ResolveTypeSig("System.Double", ElementType.R8);
    public CorLibTypeSig String => ResolveTypeSig("System.String", ElementType.String);
    public CorLibTypeSig TypedReference => ResolveTypeSig("System.TypedReference", ElementType.TypedByRef);
    public CorLibTypeSig IntPtr => ResolveTypeSig("System.IntPtr", ElementType.I);
    public CorLibTypeSig UIntPtr => ResolveTypeSig("System.UIntPtr", ElementType.U);
    public CorLibTypeSig Object => ResolveTypeSig("System.Object", ElementType.Object);
    public AssemblyRef AssemblyRef => _assemblyRef ??= FindCorLibAssemblyRef();

    private AssemblyRef _assemblyRef;

    // TODO: Replace enumeration with IMetaDataAssemblyImport.FindAssemblyRef or similar direct lookup
    private AssemblyRef FindCorLibAssemblyRef()
    {
        HCORENUM hEnum = default;
        Span<MdTypeRef> typeRefs = stackalloc MdTypeRef[50];

        try
        {
            while (_metadataImport.Value.EnumTypeRefs(ref hEnum, typeRefs, out var count) && count > 0)
            {
                for (int i = 0; i < (int)count; i++)
                {
                    var (hr, props) = _metadataImport.Value.GetTypeRefProps(typeRefs[i]);

                    if (hr && props.TypeName == "System.Object")
                    {
                        return new AssemblyRefUser("corlib") { Rid = MDToken.ToRID((uint)props.ResolutionScope.Value) };
                    }
                }
            }
        }
        finally
        {
            _metadataImport.Value.CloseEnum(hEnum);
        }

        return null;
    }

    /// <summary>
    /// Finds the TypeRef token for a corlib type in the target module.
    /// Returns null if the type is not referenced by the target module.
    /// </summary>
    private TypeRef FindTypeRef(string fullName)
    {
        var asmRef = AssemblyRef;

        if (asmRef == null)
        {
            return null;
        }

        var (hr, typeRef) = _metadataImport.Value.FindTypeRef(new MdToken((int)asmRef.MDToken.Raw), fullName);

        if (!hr)
        {
            return null;
        }

        var dot = fullName.LastIndexOf('.');
        var ns = dot >= 0 ? fullName[..dot] : "";
        var typeName = dot >= 0 ? fullName[(dot + 1)..] : fullName;
        return new TypeRefUser(null, ns, typeName) { Rid = MDToken.ToRID((uint)typeRef.Value) };
    }

    private CorLibTypeSig ResolveTypeSig(string name, ElementType elementType)
    {
        var (result, _) = _corLibMetadataImport.Value.FindTypeDefByName(name, default);
        if (!result)
        {
            Console.WriteLine($"Failed to find type definition for {name}: {result}");
            return default;
        }

        var typeRef = FindTypeRef(name);
        if (typeRef != null)
        {
            return new(typeRef, elementType);
        }

        return new(new TypeDefUser(name), elementType);
    }

}