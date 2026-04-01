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

    public ICorLibTypes CorLibTypes
    {
        get
        {
            _corLibTypes ??= IL.CorLibTypes.Create(MetaDataImport, _corProfilerInfo, _moduleId).ThrowIfFailed();
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

    public IMDTokenProvider ResolveToken(uint token, GenericParamContext _)
    {
        if (token == 0)
        {
            return null;
        }

        IMDTokenProvider result = MDToken.ToTable(token) switch
        {
            Table.TypeRef => ResolveTypeRef(new MdTypeRef(new((int)token))),
            Table.TypeDef => ResolveTypeDef(new MdTypeDef(new((int)token))),
            Table.Field => ResolveField(new MdFieldDef((int)token)),
            Table.Method => ResolveMethod(new MdMethodDef((int)token)),
            Table.MemberRef => ResolveMemberRef(new MdMemberRef((int)token)),
            Table.StandAloneSig => ResolveStandAloneSig(new MdSignature(new((int)token))),
            Table.TypeSpec => ResolveTypeSpec(new MdTypeSpec(new((int)token))),
            Table.MethodSpec => ResolveMethodSpec(new MdMethodSpec(new((int)token))),
            _ => null
        };

        if (result == null)
        {
            Error($"Unsupported token: 0x{token:x8} - ({MDToken.ToTable(token)})");
        }

        return result;
    }

    public TypeSpec ResolveTypeSpec(MdTypeSpec token)
    {
        var signature = MetaDataImport.Value.GetTypeSpecFromToken(new(token.Token.Value)).ThrowIfFailed();
        var typeSig = ReadTypeSignature(signature);

        return new TypeSpecUser(typeSig) { Rid = MDToken.ToRID((uint)token.Token.Value) };
    }

    public TypeSpec GetTypeSpec(TypeSig typeSig)
    {
        var sigBlob = SignatureWriter.Write(this, typeSig);
        var mdToken = MetaDataEmit.Value.GetTokenFromTypeSpec(sigBlob).ThrowIfFailed();
        return ResolveTypeSpec(mdToken);
    }

    public IField ResolveField(MdFieldDef token)
    {
        var props = MetaDataImport.Value.GetFieldProps(new(token.Value)).ThrowIfFailed();
        var sig = ReadSignature(props.Signature);

        return new FieldDefUser(props.Name, (FieldSig)sig, (FieldAttributes)props.Attributes)
        {
            Rid = MDToken.ToRID((uint)token.Value)
        };
    }

    public IField GetField(TypeDef declaringType, string name)
    {
        var token = MetaDataImport.Value.FindField(new MdTypeDef(new((int)declaringType.MDToken.Raw)), name, default).ThrowIfFailed();
        return ResolveField(token);
    }

    public MethodSpec ResolveMethodSpec(MdMethodSpec token)
    {
        var props = MetaDataImport.Value.GetMethodSpecProps(token).ThrowIfFailed();
        var parentToken = (uint)props.Parent.Value;

        var parent = MDToken.ToTable(parentToken) switch
        {
            Table.Method => (IMethodDefOrRef)ResolveMethod(new MdMethodDef((int)parentToken)),
            Table.MemberRef => ResolveMemberRef(new MdMemberRef((int)parentToken)),
            _ => null
        };

        var sig = ReadSignature(props.Signature);
        return new MethodSpecUser(parent, (GenericInstMethodSig)sig) { Rid = MDToken.ToRID((uint)token.Token.Value) };
    }

    public unsafe StandAloneSig ResolveStandAloneSig(MdSignature token)
    {
        var signature = MetaDataImport.Value.GetSigFromToken(new(token.Token.Value)).ThrowIfFailed();

        var dataStream = DataStreamFactory.Create((byte*)signature.Ptr);
        var dataReader = new DataReader(dataStream, 0, (uint)signature.Length);

        var sig = SignatureReader.ReadSig(this, CorLibTypes, dataReader);

        return new StandAloneSigUser
        {
            Signature = sig,
            Rid = MDToken.ToRID((uint)token.Token.Value)
        };
    }

    public StandAloneSig GetStandAloneSig(CallingConventionSig signature)
    {
        var sigBlob = SignatureWriter.Write(this, signature);
        var mdToken = MetaDataEmit.Value.GetTokenFromSig(sigBlob).ThrowIfFailed();
        return ResolveStandAloneSig(mdToken);
    }

    public IMethod ResolveMethod(MdMethodDef token)
    {
        var props = MetaDataImport.Value.GetMethodProps(new(token.Value)).ThrowIfFailed();
        var sig = ReadSignature(props.Signature);

        return new MethodDefUser(props.Name, (MethodSig)sig, (MethodImplAttributes)props.ImplementationFlags)
        {
            Rid = MDToken.ToRID((uint)token.Value)
        };
    }

    public IMethod GetMethod(TypeDef declaringType, string name)
    {
        var token = MetaDataImport.Value.FindMethod(new MdTypeDef(new((int)declaringType.MDToken.Raw)), name, default).ThrowIfFailed();
        return ResolveMethod(token);
    }

    public MemberRef ResolveMemberRef(MdMemberRef token)
    {
        var props = MetaDataImport.Value.GetMemberRefProps(token).ThrowIfFailed();

        IMemberRefParent parent = null;

        if (MDToken.ToTable(props.Token.Value) == Table.TypeRef)
        {
            parent = ResolveTypeRef(new MdTypeRef(new(props.Token.Value)));
        }

        return new MemberRefUser(null, props.Name)
        {
            Signature = ReadSignature(props.Signature),
            Class = parent,
            Rid = MDToken.ToRID((uint)token.Value)
        };
    }

    public MemberRef GetMemberRef(ITypeDefOrRef parent, string name, CallingConventionSig signature)
    {
        var sigBlob = SignatureWriter.Write(this, signature);
        var mdToken = MetaDataEmit.Value.DefineMemberRef(new MdToken((int)parent.MDToken.Raw), name, sigBlob).ThrowIfFailed();
        return ResolveMemberRef(mdToken);
    }

    public TypeDef ResolveTypeDef(MdTypeDef token)
    {
        var typeDefProps = MetaDataImport.Value.GetTypeDefProps(new(token.Token.Value)).ThrowIfFailed();
        return new TypeDefUser(new(typeDefProps.TypeName)) { Rid = MDToken.ToRID((uint)token.Token.Value) };
    }

    public TypeDef GetTypeDef(string name, TypeDef enclosingType = null)
    {
        var enclosingToken = enclosingType != null ? new MdToken((int)enclosingType.MDToken.Raw) : default;
        var token = MetaDataImport.Value.FindTypeDefByName(name, enclosingToken).ThrowIfFailed();
        return ResolveTypeDef(token);
    }

    public TypeRef ResolveTypeRef(MdTypeRef token)
    {
        var typeRefProps = MetaDataImport.Value.GetTypeRefProps(new(token.Token.Value)).ThrowIfFailed();
        return new TypeRefUser(new ModuleDefUser(new("TypeRef-ModuleDefUser")), new(typeRefProps.TypeName)) { Rid = MDToken.ToRID((uint)token.Token.Value) };
    }

    public TypeRef GetTypeRef(IResolutionScope resolutionScope, string name)
    {
        var scopeToken = new MdToken((int)resolutionScope.MDToken.Raw);
        var (hr, existing) = MetaDataImport.Value.FindTypeRef(scopeToken, name);

        if (hr)
        {
            return ResolveTypeRef(existing);
        }

        var token = MetaDataEmit.Value.DefineTypeRefByName(scopeToken, name).ThrowIfFailed();
        return ResolveTypeRef(new MdTypeRef(token.Token));
    }

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
                return ResolveTypeRef(new MdTypeRef(new((int)token.Raw)));

            case Table.TypeDef:
                return ResolveTypeDef(new MdTypeDef(new((int)token.Raw)));

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
}
