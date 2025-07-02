namespace Silhouette;

public class ICorProfilerInfo2 : ICorProfilerInfo, ICorProfilerInfoFactory<ICorProfilerInfo2>
{
    private readonly NativeObjects.ICorProfilerInfo2Invoker _impl;

    public ICorProfilerInfo2(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo2 ICorProfilerInfoFactory<ICorProfilerInfo2>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo2>.Guid => Interfaces.ICorProfilerInfo2.Guid;

    public unsafe HResult DoStackSnapshot(ThreadId thread, delegate* unmanaged[Stdcall]<FunctionId, nint, COR_PRF_FRAME_INFO, uint, byte*, void*, HResult> callback, COR_PRF_SNAPSHOT_INFO infoFlags, void* clientData, byte* context, uint contextSize)
    {
        return _impl.DoStackSnapshot(thread, callback, (uint)infoFlags, clientData, context, contextSize);
    }

    public unsafe HResult SetEnterLeaveFunctionHooks2(void* pFuncEnter, void* pFuncLeave, void* pFuncTailcall)
    {
        return _impl.SetEnterLeaveFunctionHooks2(pFuncEnter, pFuncLeave, pFuncTailcall);
    }

    public unsafe HResult<FunctionInfo> GetFunctionInfo2(FunctionId funcId, COR_PRF_FRAME_INFO frameInfo, ReadOnlySpan<ClassId> typeArgs, out uint nbTypeArgs)
    {
        fixed (ClassId* pTypeArgs = typeArgs)
        {
            var result = _impl.GetFunctionInfo2(funcId, frameInfo, out var classId, out var moduleId, out var token, (uint)typeArgs.Length, out nbTypeArgs, pTypeArgs);
            return new(result, new(classId, moduleId, token));
        }
    }

    public unsafe HResult GetFunctionInfo2(FunctionId funcId, COR_PRF_FRAME_INFO frameInfo, out ClassId pClassId, out ModuleId pModuleId, out MdToken pToken, uint cTypeArgs, out uint pcTypeArgs, ClassId* typeArgs)
    {
        return _impl.GetFunctionInfo2(funcId, frameInfo, out pClassId, out pModuleId, out pToken, cTypeArgs, out pcTypeArgs, typeArgs);
    }

    public HResult<StringLayout> GetStringLayout()
    {
        var result = _impl.GetStringLayout(out var pBufferLengthOffset, out var pStringLengthOffset, out var pBufferOffset);
        return new(result, new(pBufferLengthOffset, pStringLengthOffset, pBufferOffset));
    }

    public unsafe HResult<uint> GetClassLayout(ClassId classID, Span<COR_FIELD_OFFSET> fieldOffsets, out uint nbFieldOffsets)
    {
        fixed (COR_FIELD_OFFSET* pFieldOffsets = fieldOffsets)
        {
            var result = _impl.GetClassLayout(classID, pFieldOffsets, (uint)fieldOffsets.Length, out nbFieldOffsets, out var classSize);
            return new(result, classSize);
        }
    }

    public unsafe HResult<ClassIdInfo2> GetClassIDInfo2(ClassId classId, ReadOnlySpan<ClassId> typeArgs, out uint numTypeArgs)
    {
        fixed (ClassId* pTypeArgs = typeArgs)
        {
            var result = _impl.GetClassIDInfo2(classId, out var moduleId, out var typeDefToken, out var parentClassId, (uint)typeArgs.Length, out numTypeArgs, pTypeArgs);
            return new(result, new(moduleId, typeDefToken, parentClassId));
        }
    }

    public unsafe HResult GetCodeInfo2(FunctionId functionID, Span<COR_PRF_CODE_INFO> codeInfos, out uint nbCodeInfos)
    {
        fixed (COR_PRF_CODE_INFO* pCodeInfos = codeInfos)
        {
            return _impl.GetCodeInfo2(functionID, (uint)codeInfos.Length, out nbCodeInfos, pCodeInfos);
        }
    }

    public unsafe HResult<ClassId> GetClassFromTokenAndTypeArgs(ModuleId moduleID, MdTypeDef typeDef, ReadOnlySpan<ClassId> typeArgs)
    {
        fixed (ClassId* pTypeArgs = typeArgs)
        {
            var result = _impl.GetClassFromTokenAndTypeArgs(moduleID, typeDef, (uint)typeArgs.Length, pTypeArgs, out var classId);
            return new(result, classId);
        }
    }

    public unsafe HResult<FunctionId> GetFunctionFromTokenAndTypeArgs(ModuleId moduleID, MdMethodDef funcDef, ClassId classId, ReadOnlySpan<ClassId> typeArgs)
    {
        fixed (ClassId* pTypeArgs = typeArgs)
        {
            var result = _impl.GetFunctionFromTokenAndTypeArgs(moduleID, funcDef, classId, (uint)typeArgs.Length, pTypeArgs, out var functionId);
            return new(result, functionId);
        }
    }

    public HResult<INativeEnumerator<ObjectId>> EnumModuleFrozenObjects(ModuleId moduleID)
    {
        var result = _impl.EnumModuleFrozenObjects(moduleID, out var pEnum);
        return new(result, new(pEnum));
    }

    public unsafe HResult<nint> GetArrayObjectInfo(ObjectId objectId, Span<uint> dimensionSizes, Span<int> dimensionLowerBounds)
    {
        if (dimensionSizes.Length != dimensionLowerBounds.Length)
        {
            throw new ArgumentException("The length of the dimension sizes and dimension lower bounds must be equal.");
        }

        fixed (uint* pDimensionSizes = dimensionSizes)
        fixed (int* pDimensionLowerBounds = dimensionLowerBounds)
        {
            var result = _impl.GetArrayObjectInfo(objectId, (uint)dimensionSizes.Length, pDimensionSizes, pDimensionLowerBounds, out var ppData);
            return new(result, (nint)ppData);
        }
    }

    public HResult<uint> GetBoxClassLayout(ClassId classId)
    {
        var result = _impl.GetBoxClassLayout(classId, out var bufferOffset);
        return new(result, bufferOffset);
    }

    public HResult<AppDomainId> GetThreadAppDomain(ThreadId threadId)
    {
        var result = _impl.GetThreadAppDomain(threadId, out var appDomainId);
        return new(result, appDomainId);
    }

    public unsafe HResult<nint> GetRVAStaticAddress(ClassId classId, MdFieldDef fieldToken)
    {
        var result = _impl.GetRVAStaticAddress(classId, fieldToken, out var address);
        return new(result, (nint)address);
    }

    public unsafe HResult<nint> GetAppDomainStaticAddress(ClassId classId, MdFieldDef fieldToken, AppDomainId appDomainId)
    {
        var result = _impl.GetAppDomainStaticAddress(classId, fieldToken, appDomainId, out var address);
        return new(result, (nint)address);
    }

    public unsafe HResult<nint> GetThreadStaticAddress(ClassId classId, MdFieldDef fieldToken, ThreadId threadId)
    {
        var result = _impl.GetThreadStaticAddress(classId, fieldToken, threadId, out var address);
        return new(result, (nint)address);
    }

    public unsafe HResult<nint> GetContextStaticAddress(ClassId classId, MdFieldDef fieldToken, ContextId contextId)
    {
        var result = _impl.GetContextStaticAddress(classId, fieldToken, contextId, out var address);
        return new(result, (nint)address);
    }

    public HResult<COR_PRF_STATIC_TYPE> GetStaticFieldInfo(ClassId classId, MdFieldDef fieldToken)
    {
        var result = _impl.GetStaticFieldInfo(classId, fieldToken, out var fieldInfo);
        return new(result, fieldInfo);
    }

    public unsafe HResult GetGenerationBounds(Span<COR_PRF_GC_GENERATION_RANGE> ranges, out uint nbObjectRanges)
    {
        fixed (COR_PRF_GC_GENERATION_RANGE* pObjectRanges = ranges)
        {
            return _impl.GetGenerationBounds((uint)ranges.Length, out nbObjectRanges, pObjectRanges);
        }
    }

    public HResult<COR_PRF_GC_GENERATION_RANGE> GetObjectGeneration(ObjectId objectId)
    {
        var result = _impl.GetObjectGeneration(objectId, out var range);
        return new(result, range);
    }

    public HResult<COR_PRF_EX_CLAUSE_INFO> GetNotifiedExceptionClauseInfo()
    {
        var result = _impl.GetNotifiedExceptionClauseInfo(out var info);
        return new(result, info);
    }
}