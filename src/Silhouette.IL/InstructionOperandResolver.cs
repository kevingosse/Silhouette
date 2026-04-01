using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.MD;
using dnlib.DotNet.Writer;
using dnlib.IO;

namespace Silhouette.IL;

public sealed class InstructionOperandResolver : IInstructionOperandResolver, IDisposable, ISignatureReaderHelper, ITokenProvider, ISignatureWriterHelper
{
    private ComPtr<IMetaDataImport2> _metaDataImport;
    private ComPtr<IMetaDataEmit> _metaDataEmit;
    private CorLibTypes _corLibTypes;

    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private readonly ModuleId _moduleId;

    public InstructionOperandResolver(ModuleId moduleId, ICorProfilerInfo3 corProfilerInfo)
    {
        _moduleId = moduleId;
        _corProfilerInfo = corProfilerInfo;
    }

    public CorLibTypes CorLibTypes
    {
        get
        {
            _corLibTypes ??= CorLibTypes.Create(MetaDataImport, _corProfilerInfo, _moduleId).ThrowIfFailed();
            return _corLibTypes;
        }
    }

    private ComPtr<IMetaDataImport2> MetaDataImport
    {
        get
        {
            _metaDataImport ??= _corProfilerInfo.GetModuleMetaDataImport2(_moduleId, CorOpenFlags.ofRead)
                .ThrowIfFailed()
                .Wrap();

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

        IMDTokenProvider result = MDToken.ToTable(token) switch
        {
            //Table.Module => ResolveModule(rid),
            Table.TypeRef => ResolveTypeRef(token, gpContext),
            Table.TypeDef => ResolveTypeDef(token, gpContext),
            Table.Field => ResolveField(token),
            Table.Method => ResolveMethod(token),
            //Table.Param => ResolveParam(rid),
            //Table.InterfaceImpl => ResolveInterfaceImpl(rid, gpContext),
            Table.MemberRef => ResolveMemberRef(token, gpContext),
            //Table.Constant => ResolveConstant(rid),
            //Table.DeclSecurity => ResolveDeclSecurity(rid),
            //Table.ClassLayout => ResolveClassLayout(rid),
            Table.StandAloneSig => ResolveStandAloneSig(token, gpContext),
            //Table.Event => ResolveEvent(rid),
            //Table.Property => ResolveProperty(rid),
            //Table.ModuleRef => ResolveModuleRef(rid),
            Table.TypeSpec => ResolveTypeSpec(token, gpContext),
            //Table.ImplMap => ResolveImplMap(rid),
            //Table.Assembly => ResolveAssembly(rid),
            //Table.AssemblyRef => ResolveAssemblyRef(rid),
            //Table.File => ResolveFile(rid),
            //Table.ExportedType => ResolveExportedType(rid),
            //Table.ManifestResource => ResolveManifestResource(rid),
            //Table.GenericParam => ResolveGenericParam(rid),
            Table.MethodSpec => ResolveMethodSpec(token, gpContext),
            //Table.GenericParamConstraint => ResolveGenericParamConstraint(rid, gpContext),
            _ => null
        };

        if (result == null)
        {
            Console.WriteLine($"Unsupported token: {token:x2} - ({MDToken.ToTable(token)})");
        }

        return result;
    }

    private TypeSpecUser ResolveTypeSpec(uint token, GenericParamContext _)
    {
        var signature = MetaDataImport.Value.GetTypeSpecFromToken(new((int)token)).ThrowIfFailed();
        var typeSig = ReadTypeSignature(signature);

        return new TypeSpecUser(typeSig) { Rid = MDToken.ToRID(token) };
    }

    private FieldDefUser ResolveField(uint token)
    {
        var props = MetaDataImport.Value.GetFieldProps(new((int)token)).ThrowIfFailed();
        var sig = ReadSignature(props.Signature);

        return new FieldDefUser(props.Name, (FieldSig)sig, (FieldAttributes)props.Attributes)
        {
            Rid = MDToken.ToRID(token)
        };
    }

    private MethodSpecUser ResolveMethodSpec(uint token, GenericParamContext gpContext)
    {
        var props = MetaDataImport.Value.GetMethodSpecProps(new MdMethodSpec((int)token)).ThrowIfFailed();

        var parentToken = (uint)props.Parent.Value;

        IMethodDefOrRef parent = MDToken.ToTable(parentToken) switch
        {
            Table.Method => ResolveMethod(parentToken),
            Table.MemberRef => ResolveMemberRef(parentToken, gpContext),
            _ => null
        };

        var sig = ReadSignature(props.Signature);
        return new MethodSpecUser(parent, (GenericInstMethodSig)sig) { Rid = MDToken.ToRID(token) };
    }

    private unsafe StandAloneSigUser ResolveStandAloneSig(uint token, GenericParamContext _)
    {
        var signature = MetaDataImport.Value.GetSigFromToken(new((int)token)).ThrowIfFailed();

        var dataStream = DataStreamFactory.Create((byte*)signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)signature.Length);

        var sig = SignatureReader.ReadSig(this, CorLibTypes, dataReader);

        return sig switch
        {
            LocalSig localSig => new StandAloneSigUser(localSig) { Rid = MDToken.ToRID(token) },
            MethodSig methodSig => new StandAloneSigUser(methodSig) { Rid = MDToken.ToRID(token) },
            _ => null
        };
    }

    private MethodDefUser ResolveMethod(uint token)
    {
        var props = MetaDataImport.Value.GetMethodProps(new((int)token)).ThrowIfFailed();
        var sig = ReadSignature(props.Signature);

        var methodDef = new MethodDefUser(props.Name, (MethodSig)sig, (MethodImplAttributes)props.ImplementationFlags)
        {
            Rid = MDToken.ToRID(token)
        };

        return methodDef;
    }

    private unsafe CallingConventionSig ReadSignature(NativePointer<byte> signature)
    {
        var dataStream = DataStreamFactory.Create((byte*)signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)signature.Length);

        return SignatureReader.ReadSig(this, CorLibTypes, dataReader);
    }

    private unsafe TypeSig ReadTypeSignature(NativePointer<byte> signature)
    {
        var dataStream = DataStreamFactory.Create((byte*)signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)signature.Length);

        return SignatureReader.ReadTypeSig(this, CorLibTypes, dataReader);
    }

    private MyMemberRef ResolveMemberRef(uint token, GenericParamContext gpContext)
    {
        var props = MetaDataImport.Value.GetMemberRefProps(new MdMemberRef((int)token)).ThrowIfFailed();

        IMemberRefParent parent = null;

        if (MDToken.ToTable(props.Token.Value) == Table.TypeRef)
        {
            parent = ResolveTypeRef((uint)props.Token.Value, gpContext);
        }

        var sig = ReadSignature(props.Signature);
        return new MyMemberRef(props.Name, MDToken.ToRID(token), sig, parent);
    }

    private TypeDefUser ResolveTypeDef(uint tokenValue, GenericParamContext _)
    {
        var typeDefProps = MetaDataImport.Value.GetTypeDefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeDefUser(new(typeDefProps.TypeName)) { Rid = MDToken.ToRID(tokenValue) };
    }

    private TypeRefUser ResolveTypeRef(uint tokenValue, GenericParamContext _)
    {
        var typeRefProps = MetaDataImport.Value.GetTypeRefProps(new((int)tokenValue)).ThrowIfFailed();
        return new TypeRefUser(new ModuleDefUser(new("TypeRef-ModuleDefUser")), new(typeRefProps.TypeName)) { Rid = MDToken.ToRID(tokenValue) };
    }

#pragma warning disable IDE0003 // Qualifier 'this.' is redundant
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

    public List<Parameter> ReadParameters(MdMethodDef methodDef)
    {
        var props = MetaDataImport.Value.GetMethodProps(methodDef).ThrowIfFailed();
        var sig = (MethodSig)ReadSignature(props.Signature);

        var parameters = new List<Parameter>();

        int paramIndex = 0;

        if (sig.HasThis)
        {
            parameters.Add(new Parameter(paramIndex, Parameter.HIDDEN_THIS_METHOD_SIG_INDEX));
            paramIndex++;
        }

        foreach (var paramType in sig.Params)
        {
            var methodSigIndex = sig.HasThis ? paramIndex - 1 : paramIndex;
            parameters.Add(new Parameter(paramIndex, methodSigIndex, paramType));
            paramIndex++;
        }

        return parameters;
    }

    public string ReadUserString(uint token)
    {
        return MetaDataImport.Value.GetUserString(new((int)token)).ThrowIfFailed();
    }

    public void Dispose()
    {
        _metaDataImport?.Dispose();
        _metaDataEmit?.Dispose();
        _corLibTypes?.Dispose();
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

        throw new NotImplementedException("ConvertRTInternalAddress");
    }

    public void Error(string message) => throw new InvalidOperationException(message);

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
                return memberRef.MDToken;

            case MethodDef methodDef:
                return methodDef.MDToken;

            case TypeDef typeDef:
                return typeDef.MDToken;

            case TypeRef typeRef:
                return typeRef.MDToken;

            case TypeSpec typeSpec:
                return typeSpec.MDToken;

            case FieldDef fieldDef:
                return fieldDef.MDToken;

            case MethodSpec methodSpec:
                return methodSpec.MDToken;

            case MethodSig methodSig:
                var sigBlob = SignatureWriter.Write(this, methodSig);
                return new MDToken(MetaDataEmit.Value.GetTokenFromSig(sigBlob).ThrowIfFailed().Value);

            default:
                throw new NotImplementedException($"Method not implemented for argument type {o.GetType()}");
        }
    }

    public MDToken GetToken(IList<TypeSig> locals, uint origToken)
    {
        if (locals == null || locals.Count == 0)
        {
            return new MDToken(origToken);
        }

        var sigBlob = SignatureWriter.Write(this, new LocalSig(locals));
        var token = MetaDataEmit.Value.GetTokenFromSig(sigBlob).ThrowIfFailed();
        return new MDToken(token.Value);
    }

    public uint ToEncodedToken(ITypeDefOrRef typeDefOrRef)
    {
        if (!CodedToken.TypeDefOrRef.Encode(typeDefOrRef.MDToken, out var encodedToken))
        {
            Error($"Can't encode TypeDefOrRef token 0x{typeDefOrRef.MDToken.Raw:X8}");
            return 0;
        }

        return encodedToken;
    }
}