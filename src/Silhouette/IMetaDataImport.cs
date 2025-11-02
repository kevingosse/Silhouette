using NativeObjects;
using System.Runtime.CompilerServices;

namespace Silhouette;

public class IMetaDataImport : Interfaces.IUnknown
{
    private readonly IMetaDataImportInvoker _impl;

    public IMetaDataImport(nint ptr)
    {
        _impl = new(ptr);
    }

    public HResult QueryInterface(in Guid guid, out nint ptr)
    {
        return _impl.QueryInterface(in guid, out ptr);
    }

    public int AddRef()
    {
        return _impl.AddRef();
    }

    public int Release()
    {
        return _impl.Release();
    }

    public void CloseEnum(HCORENUM hEnum)
    {
        _impl.CloseEnum(hEnum);
    }

    public HResult<uint> CountEnum(HCORENUM hEnum)
    {
        var result = _impl.CountEnum(hEnum, out var pulCount);
        return new(result, pulCount);
    }

    public HResult ResetEnum(HCORENUM hEnum, uint ulPos)
    {
        return _impl.ResetEnum(hEnum, ulPos);
    }

    public unsafe HResult EnumTypeDefs(ref HCORENUM hEnum, Span<MdTypeDef> typeDefs, out uint nbTypeDefs)
    {
        fixed (MdTypeDef* rTypeDefs = typeDefs)
        {
            return _impl.EnumTypeDefs((HCORENUM*)Unsafe.AsPointer(ref hEnum), rTypeDefs, (uint)typeDefs.Length, out nbTypeDefs);
        }
    }

    public unsafe HResult EnumInterfaceImpls(ref HCORENUM hEnum, MdTypeDef td, Span<MdInterfaceImpl> interfaceImpls, out uint pcImpls)
    {
        fixed (MdInterfaceImpl* rImpls = interfaceImpls)
        {
            return _impl.EnumInterfaceImpls((HCORENUM*)Unsafe.AsPointer(ref hEnum), td, rImpls, (uint)interfaceImpls.Length, out pcImpls);
        }
    }

    public unsafe HResult EnumTypeRefs(ref HCORENUM hEnum, Span<MdTypeRef> typeRefs, out uint nbTypeRefs)
    {
        fixed (MdTypeRef* rTypeRefs = typeRefs)
        {
            return _impl.EnumTypeRefs((HCORENUM*)Unsafe.AsPointer(ref hEnum), rTypeRefs, (uint)typeRefs.Length, out nbTypeRefs);
        }
    }

    public unsafe HResult<MdTypeDef> FindTypeDefByName(string typeDef, MdToken tkEnclosingClass)
    {
        fixed (char* szTypeDef = typeDef)
        {
            var result = _impl.FindTypeDefByName(szTypeDef, tkEnclosingClass, out var td);
            return new(result, td);
        }
    }

    public unsafe HResult<Guid> GetScopeProps(Span<char> name, out uint nameLength)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetScopeProps(szName, (uint)name.Length, out nameLength, out var mvid);
            return new(result, mvid);
        }
    }

    public unsafe HResult<ScopeProps> GetScopeProps()
    {
        var (result, _) = GetScopeProps([], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        (result, var mvid) = GetScopeProps(buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(buffer.WithoutNullTerminator(), mvid));
    }

    public HResult<MdModule> GetModuleFromScope()
    {
        var result = _impl.GetModuleFromScope(out var md);
        return new(result, md);
    }

    public unsafe HResult<TypeDefPropsWithName> GetTypeDefProps(MdTypeDef typeDef)
    {
        var (result, _) = GetTypeDefProps(typeDef, [], out var length);

        if (!result.IsOK)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var typeDefProps) = GetTypeDefProps(typeDef, buffer, out _);
        
        return new(result, new(buffer.WithoutNullTerminator(), (CorTypeAttr)typeDefProps.TypeDefFlags, typeDefProps.Extends));
    }

    public unsafe HResult<TypeDefProps> GetTypeDefProps(MdTypeDef typeDef, Span<char> typeName, out uint typeNameLength)
    {
        fixed (char* szTypeDef = typeName)
        {
            var result = _impl.GetTypeDefProps(typeDef, szTypeDef, (uint)typeName.Length, out typeNameLength, out var typeDefFlags, out var extends);                
            return new(result, new(typeDefFlags, extends));
        }
    }

    public HResult<InterfaceImplProps> GetInterfaceImplProps(MdInterfaceImpl interfaceImpl)
    {
        var result = _impl.GetInterfaceImplProps(interfaceImpl, out var pClass, out var ptkIface);
        return new(result, new(pClass, ptkIface));
    }

    public unsafe HResult<TypeRefProps> GetTypeRefProps(MdTypeRef typeRef)
    {
        var (result, _) = GetTypeRefProps(typeRef, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> name = stackalloc char[(int)length];

        (result, var resolutionScope) = GetTypeRefProps(typeRef, name, out _);

        return new(result, new(name.WithoutNullTerminator(), resolutionScope));
    }

    public unsafe HResult<MdToken> GetTypeRefProps(MdTypeRef typeRef, Span<char> name, out uint nameLength)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetTypeRefProps(typeRef, out var ptkResolutionScope, szName, (uint)name.Length, out nameLength);
            return new(result, ptkResolutionScope);
        }
    }

    public HResult<ResolvedTypeRef> ResolveTypeRef(MdTypeRef typeRef, in Guid riid)
    {
        var result = _impl.ResolveTypeRef(typeRef, in riid, out var iScope, out var typeDef);
        return new(result, new(iScope, typeDef));
    }

    public unsafe HResult EnumMembers(ref HCORENUM hEnum, MdTypeDef cl, Span<MdToken> members, out uint nbMembers)
    {
        fixed (MdToken* rMembers = members)
        {
            return _impl.EnumMembers((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, rMembers, (uint)members.Length, out nbMembers);
        }
    }

    public unsafe HResult EnumMembersWithName(ref HCORENUM hEnum, MdTypeDef cl, string name, Span<MdToken> members, out uint nbMembers)
    {
        fixed (char* szName = name)
        fixed (MdToken* rMembers = members)
        {
            return _impl.EnumMembersWithName((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, szName, rMembers, (uint)members.Length, out nbMembers);
        }
    }

    public unsafe HResult EnumMethods(ref HCORENUM hEnum, MdTypeDef cl, Span<MdMethodDef> methods, out uint nbMethods)
    {
        fixed (MdMethodDef* rMethods = methods)
        {
            return _impl.EnumMethods((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, rMethods, (uint)methods.Length, out nbMethods);
        }
    }

    public unsafe HResult EnumMethodsWithName(ref HCORENUM hEnum, MdTypeDef cl, string name, Span<MdMethodDef> methods, out uint nbMethods)
    {
        fixed (char* szName = name)
        fixed (MdMethodDef* rMethods = methods)
        {
            return _impl.EnumMethodsWithName((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, szName, rMethods, (uint)methods.Length, out nbMethods);
        }
    }

    public unsafe HResult EnumFields(ref HCORENUM hEnum, MdTypeDef cl, Span<MdFieldDef> fields, out uint nbFields)
    {
        fixed (MdFieldDef* rFields = fields)
        {
            return _impl.EnumFields((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, rFields, (uint)fields.Length, out nbFields);
        }
    }

    public unsafe HResult EnumFieldsWithName(ref HCORENUM hEnum, MdTypeDef cl, string szName, Span<MdFieldDef> fields, out uint nbTokens)
    {
        fixed (char* name = szName)
        fixed (MdFieldDef* rFields = fields)
        {
            return _impl.EnumFieldsWithName((HCORENUM*)Unsafe.AsPointer(ref hEnum), cl, name, rFields, (uint)fields.Length, out nbTokens);
        }
    }

    public unsafe HResult EnumParams(ref HCORENUM hEnum, MdMethodDef mb, Span<MdParamDef> parameters, out uint nbParameters)
    {
        fixed (MdParamDef* rParams = parameters)
        {
            return _impl.EnumParams((HCORENUM*)Unsafe.AsPointer(ref hEnum), mb, rParams, (uint)parameters.Length, out nbParameters);
        }
    }

    public unsafe HResult EnumMemberRefs(ref HCORENUM hEnum, MdToken tkParent, Span<MdMemberRef> memberRefs, out uint nbMembers)
    {
        fixed (MdMemberRef* rMemberRefs = memberRefs)
        {
            return _impl.EnumMemberRefs((HCORENUM*)Unsafe.AsPointer(ref hEnum), tkParent, rMemberRefs, (uint)memberRefs.Length, out nbMembers);
        }
    }

    public unsafe HResult EnumMethodImpls(ref HCORENUM hEnum, MdTypeDef td, Span<MdToken> methodBodies, Span<MdToken> methodDeclarations, out uint nbMethods)
    {
        if (methodBodies.Length != methodDeclarations.Length)
        {
            throw new ArgumentException("methodBodies and methodDeclarations must have the same length");
        }

        fixed (MdToken* rMethodBody = methodBodies)
        fixed (MdToken* rMethodDecl = methodDeclarations)
        {
            return _impl.EnumMethodImpls((HCORENUM*)Unsafe.AsPointer(ref hEnum), td, rMethodBody, rMethodDecl, (uint)methodBodies.Length, out nbMethods);
        }
    }

    public unsafe HResult EnumPermissionSets(ref HCORENUM hEnum, MdToken tk, int dwActions, Span<MdPermission> permissions, out uint nbPermissions)
    {
        fixed (MdPermission* rPermission = permissions)
        {
            return _impl.EnumPermissionSets((HCORENUM*)Unsafe.AsPointer(ref hEnum), tk, dwActions, rPermission, (uint)permissions.Length, out nbPermissions);
        }
    }

    public unsafe HResult<MdToken> FindMember(MdTypeDef td, string name, ReadOnlySpan<byte> signature)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = signature)
        {
            var result = _impl.FindMember(td, szName, pvSigBlob, (uint)signature.Length, out var pmb);
            return new(result, pmb);
        }
    }

    public unsafe HResult<MdMethodDef> FindMethod(MdTypeDef td, string name, ReadOnlySpan<byte> signature)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = signature)
        {
            var result = _impl.FindMethod(td, szName, pvSigBlob, (uint)signature.Length, out var pmb);
            return new(result, pmb);
        }
    }

    public unsafe HResult<MdFieldDef> FindField(MdTypeDef td, string name, ReadOnlySpan<byte> signature)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = signature)
        {
            var result = _impl.FindField(td, szName, pvSigBlob, (uint)signature.Length, out var pmb);
            return new(result, pmb);
        }
    }

    public unsafe HResult<MdMemberRef> FindMemberRef(MdTypeRef td, string name, ReadOnlySpan<byte> signature)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = signature)
        {
            var result = _impl.FindMemberRef(td, szName, pvSigBlob, (uint)signature.Length, out var pmr);
            return new(result, pmr);
        }
    }

    public unsafe HResult<MethodPropsWithName> GetMethodProps(MdMethodDef methodDef)
    {
        var (result, _) = GetMethodProps(methodDef, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> name = stackalloc char[(int)length];

        (result, var methodProps) = GetMethodProps(methodDef, name, out _);

        return new(result, new(name.WithoutNullTerminator(), methodProps.Class, methodProps.Attributes, methodProps.Signature, methodProps.RVA, methodProps.ImplementationFlags));
    }

    public unsafe HResult<MethodProps> GetMethodProps(MdMethodDef mb, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetMethodProps(mb, out var pClass, c, (uint)name.Length, out nameLength, out var attributes, out var signature, out var signatureLength, out var rva, out var implementationFlags);
            return new(result, new(pClass, attributes, new((nint)signature, (int)signatureLength), rva, implementationFlags));
        }
    }

    public unsafe HResult<MemberRefPropsWithName> GetMemberRefProps(MdMemberRef memberRef)
    {
        var (result, _) = GetMemberRefProps(memberRef, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> name = stackalloc char[(int)length];

        (result, var memberRefProps) = GetMemberRefProps(memberRef, name, out _);
        return new(result, new(name.WithoutNullTerminator(), memberRefProps.Token, memberRefProps.Signature));
    }

    public unsafe HResult<MemberRefProps> GetMemberRefProps(MdMemberRef memberRef, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetMemberRefProps(memberRef, out var token, c, (uint)name.Length, out nameLength, out var signature, out var signatureLength);
            return new(result, new(token, new((nint)signature, (int)signatureLength)));
        }
    }

    public unsafe HResult EnumProperties(ref HCORENUM hEnum, MdTypeDef typeDef, Span<MdProperty> properties, out uint nbProperties)
    {
        fixed (MdProperty* rProperties = properties)
        {
            return _impl.EnumProperties((HCORENUM*)Unsafe.AsPointer(ref hEnum), typeDef, rProperties, (uint)properties.Length, out nbProperties);
        }
    }

    public unsafe HResult EnumEvents(ref HCORENUM hEnum, MdTypeDef td, Span<MdEvent> events, out uint nbEvents)
    {
        fixed (MdEvent* rEvents = events)
        {
            return _impl.EnumEvents((HCORENUM*)Unsafe.AsPointer(ref hEnum), td, rEvents, (uint)events.Length, out nbEvents);
        }
    }

    public unsafe HResult<EventProps> GetEventProps(MdEvent ev, Span<char> name, out uint nameLength, Span<MdMethodDef> otherMethods, out uint otherMethodsLength)
    {
        fixed (char* c = name)
        fixed (MdMethodDef* rOtherMethods = otherMethods)
        {
            var result = _impl.GetEventProps(ev, out var td, c, (uint)name.Length, out nameLength, out var eventFlags, out var eventType, out var addOn, out var removeOn, out var fire, rOtherMethods, (uint)otherMethods.Length, out otherMethodsLength);
            return new(result, new(td, eventFlags, eventType, addOn, removeOn, fire));
        }
    }

    public unsafe HResult EnumMethodSemantics(ref HCORENUM hEnum, MdMethodDef mb, Span<MdToken> eventProperties, out uint eventPropertiesLength)
    {
        fixed (MdToken* rEventProp = eventProperties)
        {
            return _impl.EnumMethodSemantics((HCORENUM*)Unsafe.AsPointer(ref hEnum), mb, rEventProp, (uint)eventProperties.Length, out eventPropertiesLength);
        }
    }

    public HResult<CorMethodSemanticsAttr> GetMethodSemantics(MdMethodDef mb, MdToken tkEventProp)
    {
        var result = _impl.GetMethodSemantics(mb, tkEventProp, out var semanticsFlags);
        return new(result, (CorMethodSemanticsAttr)semanticsFlags);
    }

    public unsafe HResult<ClassLayout> GetClassLayout(MdTypeDef td, Span<COR_FIELD_OFFSET> fieldOffsets, out uint nbFieldOffsets)
    {
        fixed (COR_FIELD_OFFSET* rFieldOffsets = fieldOffsets)
        {
            var result = _impl.GetClassLayout(td, out var packSize, rFieldOffsets, (uint)fieldOffsets.Length, out nbFieldOffsets, out var classSize);
            return new(result, new(packSize, classSize));
        }
    }

    public HResult<NativePointer<byte>> GetFieldMarshal(MdToken token)
    {
        var result = _impl.GetFieldMarshal(token, out var signature, out var length);
        return new(result, new(signature, (int)length));
    }

    public HResult<MetadataRva> GetRVA(MdToken token)
    {
        var result = _impl.GetRVA(token, out var rva, out var flags);
        return new(result, new(rva, (CorMethodImpl)flags));
    }

    public HResult<PermissionSetProps> GetPermissionSetProps(MdPermission permissionToken)
    {
        var result = _impl.GetPermissionSetProps(permissionToken, out var action, out var permission, out var length);
        return new(result, new(action, new(permission, (int)length)));
    }

    public HResult<NativePointer<byte>> GetSigFromToken(MdSignature signatureToken)
    {
        var result = _impl.GetSigFromToken(signatureToken, out var signature, out var length);
        return new(result, new(signature, (int)length));
    }

    public unsafe HResult GetModuleRefProps(MdModuleRef moduleRef, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            return _impl.GetModuleRefProps(moduleRef, c, (uint)name.Length, out nameLength);
        }
    }

    public unsafe HResult<string> GetModuleRefProps(MdModuleRef moduleRef)
    {
        var result = GetModuleRefProps(moduleRef, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        result = GetModuleRefProps(moduleRef, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, buffer.WithoutNullTerminator());
    }

    public unsafe HResult EnumModuleRefs(ref HCORENUM hEnum, Span<MdModuleRef> moduleRefs, out uint nbModuleRefs)
    {
        fixed (MdModuleRef* rModuleRefs = moduleRefs)
        {
            return _impl.EnumModuleRefs((HCORENUM*)Unsafe.AsPointer(ref hEnum), rModuleRefs, (uint)moduleRefs.Length, out nbModuleRefs);
        }
    }

    public HResult<NativePointer<byte>> GetTypeSpecFromToken(MdTypeSpec typespec)
    {
        var result = _impl.GetTypeSpecFromToken(typespec, out var signature, out var length);
        return new(result, new(signature, (int)length));
    }

    public HResult<nint> GetNameFromToken(MdToken token)
    {
        var result = _impl.GetNameFromToken(token, out var ptr);
        return new(result, ptr);
    }

    public unsafe HResult EnumUnresolvedMethods(ref HCORENUM hEnum, Span<MdToken> methods, out uint nbMethods)
    {
        fixed (MdToken* rMethods = methods)
        {
            return _impl.EnumUnresolvedMethods((HCORENUM*)Unsafe.AsPointer(ref hEnum), rMethods, (uint)methods.Length, out nbMethods);
        }
    }

    public unsafe HResult GetUserString(MdString token, Span<char> str, out uint stringLength)
    {
        fixed (char* c = str)
        {
            return _impl.GetUserString(token, c, (uint)str.Length, out stringLength);
        }
    }

    public unsafe HResult<string> GetUserString(MdString token)
    {
        var result = GetUserString(token, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        result = GetUserString(token, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, buffer.WithoutNullTerminator());
    }

    public unsafe HResult<PInvokeMap> GetPinvokeMap(MdToken token, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetPinvokeMap(token, out var mappingFlags, c, (uint)name.Length, out nameLength, out var dll);
            return new(result, new((CorPinvokeMap)mappingFlags, dll));
        }
    }

    public unsafe HResult<PInvokeMapWithName> GetPinvokeMap(MdToken token)
    {
        var (result, _) = GetPinvokeMap(token, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var pinvokeMap) = GetPinvokeMap(token, buffer, out _);

        return new(result, new(buffer.WithoutNullTerminator(), pinvokeMap.Flags, pinvokeMap.ImportDll));
    }

    public unsafe HResult EnumSignatures(ref HCORENUM hEnum, Span<MdSignature> signatures, out uint nbSignatures)
    {
        fixed (MdSignature* rSignatures = signatures)
        {
            return _impl.EnumSignatures((HCORENUM*)Unsafe.AsPointer(ref hEnum), rSignatures, (uint)signatures.Length, out nbSignatures);
        }
    }

    public unsafe HResult EnumTypeSpecs(ref HCORENUM hEnum, Span<MdTypeSpec> typeSpecs, out uint nbTypeSpecs)
    {
        fixed (MdTypeSpec* rTypeSpecs = typeSpecs)
        {
            return _impl.EnumTypeSpecs((HCORENUM*)Unsafe.AsPointer(ref hEnum), rTypeSpecs, (uint)typeSpecs.Length, out nbTypeSpecs);
        }
    }

    public unsafe HResult EnumUserStrings(ref HCORENUM hEnum, Span<MdString> strings, out uint nbStrings)
    {
        fixed (MdString* rStrings = strings)
        {
            return _impl.EnumUserStrings((HCORENUM*)Unsafe.AsPointer(ref hEnum), rStrings, (uint)strings.Length, out nbStrings);
        }
    }

    public HResult<MdParamDef> GetParamForMethodIndex(MdMethodDef token, uint index)
    {
        var result = _impl.GetParamForMethodIndex(token, index, out var paramDef);
        return new(result, paramDef);
    }

    public unsafe HResult EnumCustomAttributes(ref HCORENUM hEnum, MdToken tk, MdToken tkType, Span<MdCustomAttribute> customAttributes, out uint nbCustomAttributes)
    {
        fixed (MdCustomAttribute* rCustomAttributes = customAttributes)
        {
            return _impl.EnumCustomAttributes((HCORENUM*)Unsafe.AsPointer(ref hEnum), tk, tkType, rCustomAttributes, (uint)customAttributes.Length, out nbCustomAttributes);
        }
    }

    public HResult<CustomAttributeProps> GetCustomAttributeProps(MdCustomAttribute token)
    {
        var result = _impl.GetCustomAttributeProps(token, out var obj, out var type, out var ptr, out var length);
        return new(result, new(obj, type, new(ptr, (int)length)));
    }

    public unsafe HResult<MdTypeRef> FindTypeRef(MdToken resolutionScope, string name)
    {
        fixed (char* c = name)
        {
            var result = _impl.FindTypeRef(resolutionScope, c, out var ptr);
            return new(result, ptr);
        }
    }

    public unsafe HResult<MemberProps> GetMemberProps(MdToken token, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetMemberProps(token, out var pClass, c, (uint)name.Length, out nameLength, out var attributes, out var signature, out var signatureLength, out var rva, out var implFlags, out var cPlusTypeFlag, out var value, out var valueLength);
            return new(result, new(pClass, attributes, new(signature, (int)signatureLength), rva, implFlags, (CorElementTypes)cPlusTypeFlag, new(value, (int)valueLength)));
        }
    }

    public unsafe HResult<MemberPropsWithName> GetMemberProps(MdToken token)
    {
        var (result, _) = GetMemberProps(token, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var memberProps) = GetMemberProps(token, buffer, out _);

        return new(result, new(buffer.WithoutNullTerminator(), memberProps.Class, memberProps.Attributes, memberProps.Signature, memberProps.CodeRva, memberProps.ImplementationFlags, memberProps.CPlusTypeFlag, memberProps.Value));
    }

    public unsafe HResult<FieldProps> GetFieldProps(MdFieldDef token, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetFieldProps(token, out var pClass, c, (uint)name.Length, out nameLength, out var attributes, out var signature, out var signatureLength, out var cPlusTypeFlag, out var value, out var valueLength);
            return new(result, new(pClass, attributes, new(signature, (int)signatureLength), (CorElementTypes)cPlusTypeFlag, new(value, (int)valueLength)));
        }
    }

    public unsafe HResult<FieldPropsWithName> GetFieldProps(MdFieldDef token)
    {
        var (result, _) = GetFieldProps(token, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var fieldProps) = GetFieldProps(token, buffer, out _);

        return new(result, new(buffer.WithoutNullTerminator(), fieldProps.Class, fieldProps.Attributes, fieldProps.Signature, fieldProps.CPlusTypeFlag, fieldProps.Value));
    }

    public unsafe HResult<PropertyProps> GetPropertyProps(MdProperty prop, Span<char> name, Span<MdMethodDef> otherMethods, out uint nameLength, out uint nbOtherMethods)
    {
        fixed (char* c = name)
        fixed (MdMethodDef* rOtherMethods = otherMethods)
        {
            var result = _impl.GetPropertyProps(prop, out var pClass, c, (uint)name.Length, out nameLength, out var attributes, out var signature, out var signatureLength, out var cPlusTypeFlag, out var value, out var valueLength, out var setter, out var getter, rOtherMethods, (uint)otherMethods.Length, out nbOtherMethods);
            return new(result, new(pClass, attributes, new(signature, (int)signatureLength), (CorElementTypes)cPlusTypeFlag, new(value, (int)valueLength), setter, getter));
        }
    }

    public unsafe HResult<ParamProps> GetParamProps(MdParamDef token, Span<char> name, out uint nameLength)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetParamProps(token, out var pmd, out var pulSequence, c, (uint)name.Length, out nameLength, out var pdwAttr, out var pdwCPlusTypeFlag, out var ppValue, out var pcchValue);
            return new(result, new(pmd, pulSequence, (CorParamAttr)pdwAttr, (CorElementTypes)pdwCPlusTypeFlag, new(ppValue, (int)pcchValue)));
        }
    }

    public unsafe HResult<ParamPropsWithName> GetParamProps(MdParamDef token)
    {
        var (result, _) = GetParamProps(token, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var paramProps) = GetParamProps(token, buffer, out _);

        return new(result, new(buffer.WithoutNullTerminator(), paramProps.Method, paramProps.Index, paramProps.Attributes, paramProps.CPlusTypeFlag, paramProps.Value));
    }

    public unsafe HResult<NativePointer<byte>> GetCustomAttributeByName(MdToken token, string name)
    {
        fixed (char* c = name)
        {
            var result = _impl.GetCustomAttributeByName(token, c, out var data, out var length);
            return new(result, new(data, (int)length));
        }
    }

    public bool IsValidToken(MdToken token)
    {
        return _impl.IsValidToken(token) != 0;
    }

    public HResult<MdTypeDef> GetNestedClassProps(MdTypeDef nestedClass)
    {
        var result = _impl.GetNestedClassProps(nestedClass, out var enclosingClass);
        return new(result, enclosingClass);
    }

    public unsafe HResult<uint> GetNativeCallConvFromSig(Span<byte> signature)
    {
        fixed (byte* pvSig = signature)
        {
            var result = _impl.GetNativeCallConvFromSig(pvSig, (uint)signature.Length, out var pCallConv);
            return new(result, pCallConv);
        }
    }

    public HResult<bool> IsGlobal(MdToken token)
    {
        var result = _impl.IsGlobal(token, out var global);
        return new(result, global != 0);
    }
}