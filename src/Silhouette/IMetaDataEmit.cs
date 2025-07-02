using NativeObjects;

namespace Silhouette;

public unsafe class IMetaDataEmit : Interfaces.IUnknown
{
    private readonly IMetaDataEmitInvoker _impl;

    public IMetaDataEmit(nint ptr)
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

    public HResult SetModuleProps(string name)
    {
        fixed (char* szName = name)
        {
            return _impl.SetModuleProps(szName);
        }
    }

    public HResult Save(string file, uint saveFlags)
    {
        fixed (char* szFile = file)
        {
            return _impl.Save(szFile, saveFlags);
        }
    }

    public HResult SaveToStream(IntPtr pIStream, uint saveFlags)
    {
        return _impl.SaveToStream(pIStream, saveFlags);
    }

    public HResult<uint> GetSaveSize(CorSaveSize save)
    {
        var result = _impl.GetSaveSize(save, out var saveSize);
        return new(result, saveSize);
    }

    public HResult<MdTypeDef> DefineTypeDef(string name, uint typeDefFlags, MdToken extends, ReadOnlySpan<MdToken> implements)
    {
        fixed (char* szTypeDef = name)
        fixed (MdToken* rtkImplements = implements)
        {
            var result = _impl.DefineTypeDef(szTypeDef, typeDefFlags, extends, rtkImplements, out var td);
            return new(result, td);
        }
    }

    public HResult<MdTypeDef> DefineNestedType(string name, uint typeDefFlags, MdToken extends, ReadOnlySpan<MdToken> implements, MdTypeDef encloser)
    {
        fixed (char* szTypeDef = name)
        fixed (MdToken* rtkImplements = implements)
        {
            var result = _impl.DefineNestedType(szTypeDef, typeDefFlags, extends, rtkImplements, encloser, out var td);
            return new(result, td);
        }
    }

    public HResult SetHandler(IntPtr pUnk)
    {
        return _impl.SetHandler(pUnk);
    }

    public HResult<MdMethodDef> DefineMethod(MdTypeDef td, string name, uint methodFlags, ReadOnlySpan<byte> sigBlob, uint codeRVA, uint implFlags)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = sigBlob)
        {
            var result = _impl.DefineMethod(td, szName, methodFlags, (nint)pvSigBlob, (uint)sigBlob.Length, codeRVA, implFlags, out var md);
            return new(result, md);
        }
    }

    public HResult DefineMethodImpl(MdTypeDef td, MdToken body, MdToken decl)
    {
        return _impl.DefineMethodImpl(td, body, decl);
    }

    public HResult<MdTypeRef> DefineTypeRefByName(MdToken resolutionScope, string name)
    {
        fixed (char* szName = name)
        {
            var result = _impl.DefineTypeRefByName(resolutionScope, szName, out var tr);
            return new(result, tr);
        }
    }

    public HResult<MdTypeRef> DefineImportType(IntPtr assemImport, IntPtr hashValue, uint hashValueLen, IntPtr import, MdTypeDef tdImport, IntPtr assemEmit)
    {
        var result = _impl.DefineImportType(assemImport, hashValue, hashValueLen, import, tdImport, assemEmit, out var tr);
        return new(result, tr);
    }

    public HResult<MdMemberRef> DefineMemberRef(MdToken tkImport, string name, ReadOnlySpan<byte> sigBlob)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = sigBlob)
        {
            var result = _impl.DefineMemberRef(tkImport, szName, (nint)pvSigBlob, (uint)sigBlob.Length, out var mr);
            return new(result, mr);
        }
    }

    public HResult<MdMemberRef> DefineImportMember(IntPtr assemImport, ReadOnlySpan<byte> hash, IntPtr import, MdToken mbMember, IntPtr assemEmit, MdToken tkParent)
    {
        fixed (byte* hashValue = hash)
        {
            var result = _impl.DefineImportMember(assemImport, (IntPtr)hashValue, (uint)hash.Length, import, mbMember, assemEmit, tkParent, out var mr);
            return new(result, mr);
        }
    }

    public HResult<MdEvent> DefineEvent(MdTypeDef td, string name, uint eventFlags, MdToken eventType, MdMethodDef addOn, MdMethodDef removeOn, MdMethodDef fire, ReadOnlySpan<MdMethodDef> otherMethods)
    {
        fixed (char* szEvent = name)
        fixed (MdMethodDef* rmdOtherMethods = otherMethods)
        {
            var result = _impl.DefineEvent(td, szEvent, eventFlags, eventType, addOn, removeOn, fire, rmdOtherMethods, out var ev);
            return new(result, ev);
        }
    }

    public HResult SetClassLayout(MdTypeDef td, uint packSize, ReadOnlySpan<COR_FIELD_OFFSET> fieldOffsets, uint classSize)
    {
        fixed (COR_FIELD_OFFSET* rFieldOffsets = fieldOffsets)
        {
            return _impl.SetClassLayout(td, packSize, rFieldOffsets, classSize);
        }
    }

    public HResult DeleteClassLayout(MdTypeDef td)
    {
        return _impl.DeleteClassLayout(td);
    }

    public HResult SetFieldMarshal(MdToken tk, ReadOnlySpan<byte> nativeType)
    {
        fixed (byte* nativeTypePtr = nativeType)
        {
            return _impl.SetFieldMarshal(tk, (nint)nativeTypePtr, (uint)nativeType.Length);
        }
    }

    public HResult DeleteFieldMarshal(MdToken tk)
    {
        return _impl.DeleteFieldMarshal(tk);
    }

    public HResult<MdPermission> DefinePermissionSet(MdToken tk, uint action, ReadOnlySpan<byte> permission)
    {
        fixed (byte* permissionPtr = permission)
        {
            var result = _impl.DefinePermissionSet(tk, action, (nint)permissionPtr, (uint)permission.Length, out var pm);
            return new(result, pm);
        }
    }

    public HResult SetRVA(MdMethodDef md, uint rva)
    {
        return _impl.SetRVA(md, rva);
    }

    public HResult<MdSignature> GetTokenFromSig(ReadOnlySpan<byte> sig)
    {
        fixed (byte* sigPtr = sig)
        {
            var result = _impl.GetTokenFromSig((nint)sigPtr, (uint)sig.Length, out var msig);
            return new(result, msig);
        }
    }

    public HResult<MdModuleRef> DefineModuleRef(string name)
    {
        fixed (char* szName = name)
        {
            var result = _impl.DefineModuleRef(szName, out var mur);
            return new(result, mur);
        }
    }

    public HResult SetParent(MdMemberRef mr, MdToken tk)
    {
        return _impl.SetParent(mr, tk);
    }

    public HResult<MdTypeSpec> GetTokenFromTypeSpec(ReadOnlySpan<byte> sig)
    {
        fixed (byte* sigPtr = sig)
        {
            var result = _impl.GetTokenFromTypeSpec((nint)sigPtr, (uint)sig.Length, out var ts);
            return new(result, ts);
        }
    }

    public HResult SaveToMemory(Span<byte> data)
    {
        fixed (byte* dataPtr = data)
        {
            return _impl.SaveToMemory((nint)dataPtr, (uint)data.Length);
        }
    }

    public HResult<MdString> DefineUserString(string value)
    {
        fixed (char* szString = value)
        {
            var result = _impl.DefineUserString(szString, (uint)value.Length, out var stk);
            return new(result, stk);
        }
    }

    public HResult DeleteToken(MdToken tkObj)
    {
        return _impl.DeleteToken(tkObj);
    }

    public HResult SetMethodProps(MdMethodDef md, uint methodFlags, uint codeRVA, uint implFlags)
    {
        return _impl.SetMethodProps(md, methodFlags, codeRVA, implFlags);
    }

    public HResult SetTypeDefProps(MdTypeDef td, uint typeDefFlags, MdToken extends, ReadOnlySpan<MdToken> implements)
    {
        fixed (MdToken* rtkImplements = implements)
        {
            return _impl.SetTypeDefProps(td, typeDefFlags, extends, rtkImplements);
        }
    }

    public HResult SetEventProps(MdEvent ev, uint eventFlags, MdToken eventType, MdMethodDef addOn, MdMethodDef removeOn, MdMethodDef fire, ReadOnlySpan<MdMethodDef> otherMethods)
    {
        fixed (MdMethodDef* rmdOtherMethods = otherMethods)
        {
            return _impl.SetEventProps(ev, eventFlags, eventType, addOn, removeOn, fire, rmdOtherMethods);
        }
    }

    public HResult<MdPermission> SetPermissionSetProps(MdToken tk, uint action, ReadOnlySpan<byte> permission)
    {
        fixed (byte* permissionPtr = permission)
        {
            var result = _impl.SetPermissionSetProps(tk, action, (nint)permissionPtr, (uint)permission.Length, out var pm);
            return new(result, pm);
        }
    }

    public HResult DefinePinvokeMap(MdToken tk, uint mappingFlags, string importName, MdModuleRef importDll)
    {
        fixed (char* szImportName = importName)
        {
            return _impl.DefinePinvokeMap(tk, mappingFlags, szImportName, importDll);
        }
    }

    public HResult SetPinvokeMap(MdToken tk, uint mappingFlags, string importName, MdModuleRef importDll)
    {
        fixed (char* szImportName = importName)
        {
            return _impl.SetPinvokeMap(tk, mappingFlags, szImportName, importDll);
        }
    }

    public HResult DeletePinvokeMap(MdToken tk)
    {
        return _impl.DeletePinvokeMap(tk);
    }

    public HResult<MdCustomAttribute> DefineCustomAttribute(MdToken owner, MdToken ctor, ReadOnlySpan<byte> customAttribute)
    {
        fixed (byte* customAttributePtr = customAttribute)
        {
            var result = _impl.DefineCustomAttribute(owner, ctor, (nint)customAttributePtr, (uint)customAttribute.Length, out var cv);
            return new(result, cv);
        }
    }

    public HResult SetCustomAttributeValue(MdCustomAttribute cv, ReadOnlySpan<byte> customAttribute)
    {
        fixed (byte* customAttributePtr = customAttribute)
        {
            return _impl.SetCustomAttributeValue(cv, (IntPtr)customAttributePtr, (uint)customAttribute.Length);
        }
    }

    public HResult<MdFieldDef> DefineField(MdTypeDef td, string name, uint fieldFlags, ReadOnlySpan<byte> sigBlob, uint cPlusTypeFlag, ReadOnlySpan<char> value)
    {
        fixed (char* szName = name)
        fixed (byte* pvSigBlob = sigBlob)
        fixed (char* valuePtr = value)
        {
            var result = _impl.DefineField(td, szName, fieldFlags, (nint)pvSigBlob, (uint)sigBlob.Length, cPlusTypeFlag, (nint)valuePtr, (uint)value.Length, out var fd);
            return new(result, fd);
        }
    }

    public HResult<MdProperty> DefineProperty(MdTypeDef td, string name, uint propFlags, ReadOnlySpan<byte> sig, uint cPlusTypeFlag, ReadOnlySpan<char> value, MdMethodDef setter, MdMethodDef getter, ReadOnlySpan<MdMethodDef> otherMethods)
    {
        fixed (char* szProperty = name)
        fixed (MdMethodDef* rmdOtherMethods = otherMethods)
        fixed (byte* sigPtr = sig)
        fixed (char* valuePtr = value)
        {
            var result = _impl.DefineProperty(td, szProperty, propFlags, (IntPtr)sigPtr, (uint)sig.Length, cPlusTypeFlag, (IntPtr)valuePtr, (uint)value.Length, setter, getter, rmdOtherMethods, out var prop);
            return new(result, prop);
        }
    }

    public HResult<MdParamDef> DefineParam(MdMethodDef md, uint paramSeq, string name, uint paramFlags, uint cPlusTypeFlag, ReadOnlySpan<char> value)
    {
        fixed (char* szName = name)
        fixed (char* valuePtr = value)
        {
            var result = _impl.DefineParam(md, paramSeq, szName, paramFlags, cPlusTypeFlag, (IntPtr)valuePtr, (uint)value.Length, out var pd);
            return new(result, pd);
        }
    }

    public HResult SetFieldProps(MdFieldDef fd, uint fieldFlags, uint cPlusTypeFlag, ReadOnlySpan<char> value)
    {
        fixed (char* valuePtr = value)
        {
            return _impl.SetFieldProps(fd, fieldFlags, cPlusTypeFlag, (IntPtr)valuePtr, (uint)value.Length);
        }
    }

    public HResult SetPropertyProps(MdProperty pr, uint propFlags, uint cPlusTypeFlag, ReadOnlySpan<char> value, MdMethodDef setter, MdMethodDef getter, ReadOnlySpan<MdMethodDef> otherMethods)
    {
        fixed (MdMethodDef* rmdOtherMethods = otherMethods)
        fixed (char* valuePtr = value)
        {
            return _impl.SetPropertyProps(pr, propFlags, cPlusTypeFlag, (IntPtr)valuePtr, (uint)value.Length, setter, getter, rmdOtherMethods);
        }
    }

    public HResult SetParamProps(MdParamDef pd, string name, uint paramFlags, uint cPlusTypeFlag, ReadOnlySpan<char> value)
    {
        fixed (char* szName = name)
        fixed (char* valuePtr = value)
        {
            return _impl.SetParamProps(pd, szName, paramFlags, cPlusTypeFlag, (IntPtr)valuePtr, (uint)value.Length);
        }
    }

    public HResult<uint> DefineSecurityAttributeSet(MdToken tkObj, ReadOnlySpan<COR_SECATTR> secAttrs)
    {
        fixed (COR_SECATTR* rSecAttrs = secAttrs)
        {
            var result = _impl.DefineSecurityAttributeSet(tkObj, rSecAttrs, (uint)secAttrs.Length, out var errorAttr);
            return new(result, errorAttr);
        }
    }

    public HResult ApplyEditAndContinue(IntPtr import)
    {
        return _impl.ApplyEditAndContinue(import);
    }

    public HResult<uint> TranslateSigWithScope(IntPtr assemImport, ReadOnlySpan<byte> hash, IntPtr import, ReadOnlySpan<byte> sig, IntPtr assemEmit, IntPtr emit, Span<byte> translatedSig)
    {
        fixed (byte* hashValue = hash)
        fixed (byte* sigBlob = sig)
        fixed (byte* translatedSigPtr = translatedSig)
        {
            var result = _impl.TranslateSigWithScope(assemImport, (IntPtr)hashValue, (uint)hash.Length, import, (IntPtr)sigBlob, (uint)sig.Length, assemEmit, emit, (IntPtr)translatedSigPtr, (uint)translatedSig.Length, out var translatedSigLen);
            return new(result, translatedSigLen);
        }
    }

    public HResult SetMethodImplFlags(MdMethodDef md, uint implFlags)
    {
        return _impl.SetMethodImplFlags(md, implFlags);
    }

    public HResult SetFieldRVA(MdFieldDef fd, uint rva)
    {
        return _impl.SetFieldRVA(fd, rva);
    }

    public HResult Merge(IntPtr import, IntPtr hostMapToken, IntPtr handler)
    {
        return _impl.Merge(import, hostMapToken, handler);
    }

    public HResult MergeEnd()
    {
        return _impl.MergeEnd();
    }
}