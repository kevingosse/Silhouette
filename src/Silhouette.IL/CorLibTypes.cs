using dnlib.DotNet;

namespace Silhouette.IL;

public class CorLibTypes : ICorLibTypes, IDisposable
{
    private readonly ComPtr<IMetaDataImport2> _metadataImport;
    private readonly ComPtr<IMetaDataImport2> _corLibMetadataImport;

    public static HResult<CorLibTypes> Create(ComPtr<IMetaDataImport2> metadataImport, ICorProfilerInfo3 corProfilerInfo)
    {
        var (result, corLib) = FindCorLib(corProfilerInfo);

        if (!result)
        {
            return result;
        }

        using var corLibPtr = corLib.Wrap();

        return new CorLibTypes(metadataImport, corLibPtr);
    }

    private CorLibTypes(ComPtr<IMetaDataImport2> metadataImport, ComPtr<IMetaDataImport2> corLibMetadataImport)
    {
        _metadataImport = metadataImport.Copy();
        _corLibMetadataImport = corLibMetadataImport.Copy();
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
        Console.WriteLine($"GetTypeRef({@namespace}, {name})");
        throw new NotImplementedException();
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
    public AssemblyRef AssemblyRef
    {
        get
        {
            Console.WriteLine("AssemblyRef requested, returning null.");
            return null;
        }
    }

    private CorLibTypeSig ResolveTypeSig(string name, ElementType elementType)
    {
        var (result, _) = _corLibMetadataImport.Value.FindTypeDefByName(name, default);
        if (!result)
        {
            Console.WriteLine($"Failed to find type definition for {name}: {result}");
            return default;
        }
        return new(new TypeDefUser(name), elementType);
    }

}