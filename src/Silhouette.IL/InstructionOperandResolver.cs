using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.IO;

namespace Silhouette.IL;

public sealed class InstructionOperandResolver : IInstructionOperandResolver, IDisposable, ISignatureReaderHelper, ITokenProvider
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
            _ => null
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

        var methodDef = new MethodDefUser(props.Name, (MethodSig)sig, (MethodImplAttributes)props.ImplementationFlags)
        {
            Rid = MDToken.ToRID(token)
        };

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

#pragma warning disable IDE0003: Qualifier 'this.' is redundant
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
#pragma warning restore IDE0003

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

        switch (token.Table)
        {
            case Table.TypeRef:
                return ResolveTypeRef(token.Raw, gpContext);

            case Table.TypeDef:
                return ResolveTypeDef(token.Raw, gpContext);

            default:
                Console.WriteLine($"Unsupported token type: {token.Table}");
                throw new NotSupportedException($"Unsupported token type: {token.Table}");
        }
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
        switch (o)
        {
            case string str:
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
            case MemberRef memberRef:
                // TODO: Might need to emit or something
                return memberRef.MDToken;
            case MethodDef methodDef:
                // TODO: Might need to emit or something
                return methodDef.MDToken;

            default:
                Console.WriteLine($"ITokenProvider.GetToken({o}) - {o.GetType()}");
                Console.WriteLine(Environment.StackTrace);
                throw new NotImplementedException();
        }
    }

    public MDToken GetToken(IList<TypeSig> locals, uint origToken)
    {
        Console.WriteLine($"ITokenProvider.GetToken({locals}, {origToken})");
        throw new NotImplementedException();
    }
}