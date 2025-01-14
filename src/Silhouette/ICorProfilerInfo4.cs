namespace Silhouette;

public class ICorProfilerInfo4 : ICorProfilerInfo3
{
    private NativeObjects.ICorProfilerInfo4Invoker _impl;

    public ICorProfilerInfo4(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    public HResult<INativeEnumerator<ThreadId>> EnumThreads()
    {
        var result = _impl.EnumThreads(out var pEnum);
        return new(result, new(pEnum));
    }

    public HResult InitializeCurrentThread()
    {
        return _impl.InitializeCurrentThread();
    }

    public unsafe HResult RequestReJIT(ReadOnlySpan<ModuleId> moduleIds, ReadOnlySpan<MdMethodDef> methodIds)
    {
        if (moduleIds.Length != methodIds.Length)
        {
            throw new ArgumentException("moduleIds and methodIds must have the same length.");
        }

        fixed (ModuleId* pModuleIds = moduleIds)
        fixed (MdMethodDef* pMethodIds = methodIds)
        {
            return _impl.RequestReJIT((uint)moduleIds.Length, pModuleIds, pMethodIds);
        }
    }

    public unsafe HResult RequestRevert(ReadOnlySpan<ModuleId> moduleIds, ReadOnlySpan<MdMethodDef> methodIds, Span<HResult> status)
    {
        if (moduleIds.Length != methodIds.Length || moduleIds.Length != status.Length)
        {
            throw new ArgumentException("moduleIds, methodIds, and status must have the same length.");
        }

        fixed (ModuleId* moduleIdsPtr = moduleIds)
        fixed (MdMethodDef* methodIdsPtr = methodIds)
        fixed (HResult* statusPtr = status)
        {
            return _impl.RequestRevert((uint)moduleIds.Length, moduleIdsPtr, methodIdsPtr, statusPtr);
        }
    }

    public unsafe HResult GetCodeInfo3(FunctionId functionID, ReJITId reJitId, Span<COR_PRF_CODE_INFO> codeInfos, out uint nbCodeInfos)
    {
        fixed (COR_PRF_CODE_INFO* pCodeInfos = codeInfos)
        {
            return _impl.GetCodeInfo3(functionID, reJitId, (uint)codeInfos.Length, out nbCodeInfos, pCodeInfos);
        }
    }

    public unsafe HResult<FunctionFromIP> GetFunctionFromIP2(nint ip)
    {
        var result = _impl.GetFunctionFromIP2(ip, out var functionId, out var reJitId);
        return new(result, new(functionId, reJitId));
    }

    public unsafe HResult GetReJITIDs(FunctionId functionId, Span<ReJITId> reJitIds, out uint nbReJitIds)
    {
        fixed (ReJITId* pReJitIds = reJitIds)
        {
            return _impl.GetReJITIDs(functionId, (uint)reJitIds.Length, out nbReJitIds, pReJitIds);
        }
    }

    public unsafe HResult GetILToNativeMapping2(FunctionId functionId, ReJITId reJitId, Span<COR_DEBUG_IL_TO_NATIVE_MAP> map, out uint mapLength)
    {
        fixed (COR_DEBUG_IL_TO_NATIVE_MAP* pMap = map)
        {
            return _impl.GetILToNativeMapping2(functionId, reJitId, (uint)map.Length, out mapLength, pMap);
        }
    }

    public HResult<nint> EnumJITedFunctions2()
    {
        var result = _impl.EnumJITedFunctions2(out var pEnum);
        return new(result, pEnum);
    }

    public HResult<nint> GetObjectSize2(ObjectId objectId)
    {
        var result = _impl.GetObjectSize2(objectId, out var pcSize);
        return new(result, pcSize);
    }
}