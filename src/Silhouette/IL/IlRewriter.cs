using dnlib.DotNet.Emit;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;
public class IlRewriter : IDisposable
{
    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private ModuleId _moduleId;
    private MdMethodDef _methodDefToken;
    private InstructionOperandResolver _instructionOperandResolver;

    public IlRewriter(ICorProfilerInfo3 corProfilerInfo)
    {
        _corProfilerInfo = corProfilerInfo;
    }

    public CilBody Body { get; private set; }

    public unsafe void Import(FunctionId functionId)
    {
        var functionInfo = _corProfilerInfo.GetFunctionInfo(functionId).ThrowIfFailed();
        var functionBody = _corProfilerInfo.GetILFunctionBody(functionInfo.ModuleId, new(functionInfo.Token)).ThrowIfFailed();

        _moduleId = functionInfo.ModuleId;
        _methodDefToken = new MdMethodDef(functionInfo.Token);

        var dataStream = DataStreamFactory.Create((byte*)functionBody.MethodHeader);
        var dataReader = new DataReader(dataStream, 0, uint.MaxValue);
        
        _instructionOperandResolver = new InstructionOperandResolver(functionInfo.ModuleId, _corProfilerInfo);

        var parameters = new List<Parameter>();

        var bodyReader = new MethodBodyReader(_instructionOperandResolver, dataReader, parameters);

        if (!bodyReader.Read())
        {
            throw new InvalidOperationException("Failed to read method body.");
        }

        Body = bodyReader.CreateCilBody();
    }

    public unsafe void Export()
    {
        var writer = new MethodBodyWriter(_instructionOperandResolver, Body);
        writer.Write();

        var bodyBytes = writer.Code;

        var malloc = _corProfilerInfo.GetILFunctionBodyAllocator(_moduleId).ThrowIfFailed();

        var bodyPtr = malloc.Alloc((uint)bodyBytes.Length);
        bodyBytes.AsSpan().CopyTo(new Span<byte>((void*)bodyPtr, bodyBytes.Length));

        _corProfilerInfo.SetILFunctionBody(_moduleId, _methodDefToken, bodyPtr).ThrowIfFailed();
    }

    public void Dispose()
    {
        _instructionOperandResolver?.Dispose();
    }
}

public class InstructionOperandResolver : IInstructionOperandResolver, IDisposable, ISignatureReaderHelper, ITokenProvider
{
    private ComPtr<IMetaDataImport> _metaDataImport;
    private ComPtr<IMetaDataEmit> _metaDataEmit;
    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private readonly ModuleId _moduleId;

    public InstructionOperandResolver(ModuleId moduleId, ICorProfilerInfo3 corProfilerInfo)
    {
        _moduleId = moduleId;
        _corProfilerInfo = corProfilerInfo;
    }

    private ComPtr<IMetaDataImport> MetaDataImport
    {
        get
        {
            if (_metaDataImport == null)
            {
                _metaDataImport = _corProfilerInfo.GetModuleMetaDataImport(_moduleId, CorOpenFlags.ofRead)
                    .ThrowIfFailed()
                    .Wrap();
            }

            return _metaDataImport;
        }
    }

    private ComPtr<IMetaDataEmit> MetaDataEmit
    {
        get
        {
            if (_metaDataEmit == null)
            {
                var metadataEmitPtr = _corProfilerInfo.GetModuleMetaData(_moduleId, CorOpenFlags.ofRead | CorOpenFlags.ofWrite, Interfaces.IMetaDataEmit.Guid)
                    .ThrowIfFailed();
                _metaDataEmit = new IMetaDataEmit(metadataEmitPtr).Wrap();
            }

            return _metaDataEmit;
        }
    }

    public IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext)
    {
        if (token == 0)
        {
            return null;
        }

        //switch (MDToken.ToTable(token))
        //{
        //    case Table.Module:
        //        return new ModuleDefUser(_metadataImport.Value.GetModule(new((int)token)).ThrowIfFailed());
        //    case Table.TypeDef:
        //        return new TypeDefUser(_metadataImport.Value.GetTypeDef(new((int)token)).ThrowIfFailed());
        //    case Table.MethodDef:
        //        return new MethodDefUser(_metadataImport.Value.GetMethodDef(new((int)token)).ThrowIfFailed());
        //    case Table.Field:
        //        return new FieldDefUser(_metadataImport.Value.GetField(new((int)token)).ThrowIfFailed());
        //    case Table.Param:
        //        return new ParamDefUser(_metadataImport.Value.GetParam(new((int)token)).ThrowIfFailed());
        //    case Table.MemberRef:
        //        return new MemberRefUser(_metadataImport.Value.GetMemberRef(new((int)token)).ThrowIfFailed());
        //    case Table.String:
        //        return _metadataImport.Value.GetString(new((int)token)).ThrowIfFailed();
        //    case Table.UserString:
        //        return ReadUserString(token);
        //    default:
        //        throw new NotSupportedException($"Token type {MDToken.ToTable(token)} is not supported.");
        //}

        var result = MDToken.ToTable(token) switch
        {
            //Table.Module => ResolveModule(rid),
            //Table.TypeRef => ResolveTypeRef(rid),
            //Table.TypeDef => ResolveTypeDef(rid),
            //Table.Field => ResolveField(rid),
            Table.Method => ResolveMethod(token),
            //Table.Param => ResolveParam(rid),
            //Table.InterfaceImpl => ResolveInterfaceImpl(rid, gpContext),
            Table.MemberRef => ResolveMemberRef(token, gpContext),
            //Table.Constant => ResolveConstant(rid),
            //Table.DeclSecurity => ResolveDeclSecurity(rid),
            //Table.ClassLayout => ResolveClassLayout(rid),
            //Table.StandAloneSig => ResolveStandAloneSig(rid, gpContext),
            //Table.Event => ResolveEvent(rid),
            //Table.Property => ResolveProperty(rid),
            //Table.ModuleRef => ResolveModuleRef(rid),
            //Table.TypeSpec => ResolveTypeSpec(rid, gpContext),
            //Table.ImplMap => ResolveImplMap(rid),
            //Table.Assembly => ResolveAssembly(rid),
            //Table.AssemblyRef => ResolveAssemblyRef(rid),
            //Table.File => ResolveFile(rid),
            //Table.ExportedType => ResolveExportedType(rid),
            //Table.ManifestResource => ResolveManifestResource(rid),
            //Table.GenericParam => ResolveGenericParam(rid),
            //Table.MethodSpec => ResolveMethodSpec(rid, gpContext),
            //Table.GenericParamConstraint => ResolveGenericParamConstraint(rid, gpContext),
            _ => null,
        };

        if (result == null)
        {
            Console.WriteLine($"Unsupported token: {token:x2}");
        }

        return result;
    }

    private unsafe IMethodDefOrRef ResolveMethod(uint token)
    {
        var props = MetaDataImport.Value.GetMethodProps(new((int)token)).ThrowIfFailed();

        var dataStream = DataStreamFactory.Create((byte*)props.Signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)props.Signature.Length);

        var (result, corLibTypes) = CorLibTypes.Create(MetaDataImport, _corProfilerInfo);

        if (!result)
        {
            Console.WriteLine($"Failed to create CorLibTypes: {result}");
            return null;
        }

        using var _ = corLibTypes;

        var sig = SignatureReader.ReadSig(this, corLibTypes, dataReader);

        var methodDef = new MethodDefUser(props.Name, (MethodSig)sig, (MethodImplAttributes)props.ImplementationFlags);
        methodDef.Rid = MDToken.ToRID(token);
        return methodDef;
    }

    private unsafe MemberRef ResolveMemberRef(uint token, GenericParamContext gpContext)
    {
        var props = _metaDataImport.Value.GetMemberRefProps(new MdMemberRef((int)token)).ThrowIfFailed();

        IMemberRefParent parent = null;

        if (MDToken.ToTable(props.Token.Value) == Table.TypeRef)
        {
            parent = ResolveTypeRef((uint)props.Token.Value, gpContext);
        }

        var dataStream = DataStreamFactory.Create((byte*)props.Signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)props.Signature.Length);

        var (result, corLibTypes) = CorLibTypes.Create(MetaDataImport, _corProfilerInfo);

        if (!result)
        {
            Console.WriteLine($"Failed to create CorLibTypes: {result}");
            return null;
        }

        using var _ = corLibTypes;

        var sig = SignatureReader.ReadSig(this, corLibTypes, dataReader);

        return new MyMemberRef(props.Name, MDToken.ToRID(token), sig, parent);
    }

    private TypeDef ResolveTypeDef(uint tokenValue, GenericParamContext gpContext)
    {
        var typeDefProps = MetaDataImport.Value.GetTypeDefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeDefUser(new(typeDefProps.TypeName));
    }

    private TypeRef ResolveTypeRef(uint tokenValue, GenericParamContext gpContext)
    {
        var typeRefProps = MetaDataImport.Value.GetTypeRefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeRefUser(new ModuleDefUser(new("TypeRef-ModuleDefUser")), new(typeRefProps.TypeName));
    }

    internal class MyMemberRef : MemberRef
    {
        public MyMemberRef(string name, uint rid, CallingConventionSig sig, IMemberRefParent parent)
        {
            this.name = name;
            this.rid = rid;
            this.module = new ModuleDefUser(new("MyModule"));
            this.@class = parent;
            //var sig = MethodSig.CreateStatic(new ClassSig(new TypeDefUser(new("TypeDefArg"))));
            //SignatureReader.ReadSig()
            this.signature = sig;
        }

        public override string ToString()
        {
            return "MyMemberRef";
        }
    }

    public string ReadUserString(uint token)
    {
        return MetaDataImport.Value.GetUserString(new((int)token)).ThrowIfFailed();
    }

    public void Dispose()
    {
        MetaDataImport.Dispose();
    }

    public ITypeDefOrRef ResolveTypeDefOrRef(uint codedToken, GenericParamContext gpContext)
    {
        var token = CodedToken.TypeDefOrRef.Decode2(codedToken);

        if (token.Table == Table.TypeRef)
        {
            return ResolveTypeRef(token.Raw, gpContext);
        }

        if (token.Table == Table.TypeDef)
        {
            return ResolveTypeDef(token.Raw, gpContext);
        }

        Console.WriteLine($"Unsupported token type: {token.Table}");
        throw new NotSupportedException($"Unsupported token type: {token.Table}");
    }

    public TypeSig ConvertRTInternalAddress(IntPtr address)
    {
        Console.WriteLine($"ISignatureReaderHelper.ConvertRTInternalAddress({address})");

        throw new NotImplementedException();
    }

    public void Error(string message)
    {
        Console.WriteLine($"ITokenProvider.Error({message})");
        throw new NotImplementedException();
    }

    public MDToken GetToken(object o)
    {
        if (o is string str)
        {
            // Look if the string already exists
            HCORENUM hEnum = default;
            Span<MdString> strings = stackalloc MdString[10];

            try
            {
                while (MetaDataImport.Value.EnumUserStrings(ref hEnum, strings, out var nbStrings)
                       && nbStrings > 0)
                {
                    foreach (var stringToken in strings)
                    {
                        var value = MetaDataImport.Value.GetUserString(stringToken).ThrowIfFailed();

                        if (value == str)
                        {
                            return new MDToken(stringToken.Value);
                        }
                    }
                }
            }
            finally
            {
                MetaDataImport.Value.CloseEnum(hEnum);
            }

            // This is a new string, add it
            return new(MetaDataEmit.Value.DefineUserString(str).ThrowIfFailed().Value);
        }

        if (o is MemberRef memberRef)
        {
            // TODO: Might need to emit or something
            return memberRef.MDToken;
        }

        if (o is MethodDef methodDef)
        {
            // TODO: Might need to emit or something
            return methodDef.MDToken;
        }

        Console.WriteLine($"ITokenProvider.GetToken({o}) - {o.GetType()}");
        Console.WriteLine(Environment.StackTrace);
        throw new NotImplementedException();
    }

    public MDToken GetToken(IList<TypeSig> locals, uint origToken)
    {
        Console.WriteLine($"ITokenProvider.GetToken({locals}, {origToken})");
        throw new NotImplementedException();
    }
}

public class CorLibTypes : ICorLibTypes, IDisposable
{
    private readonly ComPtr<IMetaDataImport> _metadataImport;
    private readonly ComPtr<IMetaDataImport> _corLibMetadataImport;

    public static HResult<CorLibTypes> Create(ComPtr<IMetaDataImport> metadataImport, ICorProfilerInfo3 corProfilerInfo)
    {
        var (result, corLib) = FindCorLib(corProfilerInfo);

        if (!result)
        {
            return result;
        }

        using var corLibPtr = corLib.Wrap();

        return new CorLibTypes(metadataImport, corLibPtr);
    }

    private CorLibTypes(ComPtr<IMetaDataImport> metadataImport, ComPtr<IMetaDataImport> corLibMetadataImport)
    {
        _metadataImport = metadataImport.Copy();
        _corLibMetadataImport = corLibMetadataImport.Copy();
    }

    private static HResult<IMetaDataImport> FindCorLib(ICorProfilerInfo3 corProfilerInfo)
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
                return corProfilerInfo.GetModuleMetaDataImport(module, CorOpenFlags.ofRead | CorOpenFlags.ofWrite);
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
        var (result, typeDef) = _corLibMetadataImport.Value.FindTypeDefByName(name, default);
        if (!result)
        {
            Console.WriteLine($"Failed to find type definition for {name}: {result}");
            return default;
        }
        return new(new TypeDefUser(name), elementType);
    }

}