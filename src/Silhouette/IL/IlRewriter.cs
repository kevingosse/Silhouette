using dnlib.DotNet.Emit;
using dnlib.DotNet;
using dnlib.DotNet.MD;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;
public class IlRewriter
{
    private readonly ICorProfilerInfo3 _corProfilerInfo;

    public IlRewriter(ICorProfilerInfo3 corProfilerInfo)
    {
        _corProfilerInfo = corProfilerInfo;
    }

    public unsafe bool Import(IntPtr method, ModuleId module, MdMethodDef methodDefToken)
    {
        try
        {
            using var metadataImport = _corProfilerInfo
                .GetModuleMetaDataImport(module, CorOpenFlags.ofRead | CorOpenFlags.ofWrite)
                .ThrowIfFailed()
                .Wrap();

            var dataStream = DataStreamFactory.Create((byte*)method);
            var dataReader = new DataReader(dataStream, 0, uint.MaxValue);

            var resolver = new InstructionOperandResolver(metadataImport, _corProfilerInfo);

            var parameters = new List<Parameter>();

            // IInstructionOperandResolver
            var bodyReader = new MethodBodyReader(resolver, dataReader, parameters);

            if (!bodyReader.Read())
            {
                return false;
            }

            var body = bodyReader.CreateCilBody();

            Console.WriteLine("Dumping body");

            foreach (var instruction in body.Instructions)
            {
                Console.WriteLine(instruction);
            }

            var writer = new MethodBodyWriter(resolver, body);
            writer.Write();

            var bodyBytes = writer.Code;

            Console.WriteLine($"Wrote {bodyBytes} bytes");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error importing method: {ex.Message}");
            return false;
        }

        return true;
    }
}

public class InstructionOperandResolver : IInstructionOperandResolver, IDisposable, ISignatureReaderHelper, ITokenProvider
{
    private readonly ComPtr<IMetaDataImport> _metadataImport;
    private readonly ICorProfilerInfo3 _corProfilerInfo;

    public InstructionOperandResolver(ComPtr<IMetaDataImport> metadataImport, ICorProfilerInfo3 corProfilerInfo)
    {
        _metadataImport = metadataImport.Copy();
        _corProfilerInfo = corProfilerInfo;
    }

    public IMDTokenProvider ResolveToken(uint token, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveToken({token:x2}, {gpContext.Type}, {gpContext.Method})");

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
        Console.WriteLine($"ResolveMethod({token:x2})");
        
        var props = _metadataImport.Value.GetMethodProps(new((int)token)).ThrowIfFailed();

        var dataStream = DataStreamFactory.Create((byte*)props.Signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)props.Signature.Length);

        var (result, corLibTypes) = CorLibTypes.Create(_metadataImport, _corProfilerInfo);

        if (!result)
        {
            Console.WriteLine($"Failed to create CorLibTypes: {result}");
            return null;
        }

        using var _ = corLibTypes;

        var sig = SignatureReader.ReadSig(this, corLibTypes, dataReader);

        return new MethodDefUser(props.Name, (MethodSig)sig, (MethodImplAttributes)props.ImplementationFlags);
    }

    private unsafe MemberRef ResolveMemberRef(uint token, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveMemberRef({token:x2}, {gpContext.Type}, {gpContext.Method})");
        var memberRef = _metadataImport.Value.GetMemberRefProps(new((int)token)).ThrowIfFailed();

        var moduleDef = new ModuleDefUser();

        var props = _metadataImport.Value.GetMemberRefProps(new MdMemberRef((int)token)).ThrowIfFailed();

        Console.WriteLine($"Parent: {props.Token.Value.ToString("x2")}");

        IMemberRefParent parent = null;

        if (MDToken.ToTable(props.Token.Value) == Table.TypeRef)
        {
            Console.WriteLine("Parent is TypeRef.");
            parent = ResolveTypeRef((uint)props.Token.Value, gpContext);
        }
        else
        {
            Console.WriteLine(MDToken.ToTable(props.Token.Value));
        }

        var dataStream = DataStreamFactory.Create((byte*)props.Signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)props.Signature.Length);

        var (result, corLibTypes) = CorLibTypes.Create(_metadataImport, _corProfilerInfo);

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
        Console.WriteLine($"ResolveTypeDef({tokenValue:x2}, {gpContext.Type}, {gpContext.Method})");
        var typeDefProps = _metadataImport.Value.GetTypeDefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeDefUser(new(typeDefProps.TypeName));
    }

    private TypeRef ResolveTypeRef(uint tokenValue, GenericParamContext gpContext)
    {
        Console.WriteLine($"ResolveTypeRef({tokenValue:x2}, {gpContext.Type}, {gpContext.Method})");
        var typeRefProps = _metadataImport.Value.GetTypeRefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeRefUser(new ModuleDefUser(new("TypeRef-ModuleDefUser")), new(typeRefProps.TypeName));
    }

    //private IMemberRefParent ResolveFieldLayout(uint tokenValue, GenericParamContext gpContext)
    //{
    //    Console.WriteLine($"ResolveFieldLayout({tokenValue:x2}, {gpContext.Type}, {gpContext.Method})");
    //    var props = _metadataImport.Value.GetFieldProps(new((int)tokenValue)).ThrowIfFailed();

    //    var @class = props.Class;

    //    var typeProps = _metadataImport.Value.GetTypeDefProps(@class).ThrowIfFailed();

    //    return new TypeDefUser(typeProps.TypeName);
    //}

    private class MyMemberRef : MemberRef
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
        Console.WriteLine($"ReadUserString({token:x2})");

        var str = _metadataImport.Value.GetUserString(new((int)token)).ThrowIfFailed();
        Console.WriteLine($"Returning {str}");
        return str;
    }

    public void Dispose()
    {
        _metadataImport.Dispose();
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
        if (o is string s)
        {
            
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
        Console.WriteLine($"ResolveTypeSig({name}, {elementType})");
        var (result, typeDef) = _corLibMetadataImport.Value.FindTypeDefByName(name, default);
        if (!result)
        {
            Console.WriteLine($"Failed to find type definition for {name}: {result}");
            return default;
        }
        return new(new TypeDefUser(name), elementType);
    }

}