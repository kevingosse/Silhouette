namespace Silhouette;

public class ICorProfilerInfo3 : ICorProfilerInfo2
{
    private NativeObjects.ICorProfilerInfo3Invoker _impl;

    public ICorProfilerInfo3(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    public unsafe HResult<nint> EnumJITedFunctions()
    {
        var result = _impl.EnumJITedFunctions(out var pEnum);
        return new(result, (nint)pEnum);
    }

    public HResult RequestProfilerDetach(int expectedCompletionMilliseconds)
    {
        return _impl.RequestProfilerDetach(expectedCompletionMilliseconds);
    }

    public unsafe HResult SetFunctionIDMapper2(delegate* unmanaged[Stdcall]<FunctionId, void*, int*, nint> pFunc, void* clientData)
    {
        return _impl.SetFunctionIDMapper2(pFunc, clientData);
    }

    public HResult<StringLayout2> GetStringLayout2()
    {
        var result = _impl.GetStringLayout2(out var stringLengthOffset, out var bufferOffset);
        return new(result, new(stringLengthOffset, bufferOffset));
    }

    public unsafe HResult SetEnterLeaveFunctionHooks3(void* pFuncEnter3, void* pFuncLeave3, void* pFuncTailcall3)
    {
        return _impl.SetEnterLeaveFunctionHooks3(pFuncEnter3, pFuncLeave3, pFuncTailcall3);
    }

    public unsafe HResult SetEnterLeaveFunctionHooks3WithInfo(void* pFuncEnter3WithInfo, void* pFuncLeave3WithInfo, void* pFuncTailcall3WithInfo)
    {
        return _impl.SetEnterLeaveFunctionHooks3WithInfo(pFuncEnter3WithInfo, pFuncLeave3WithInfo, pFuncTailcall3WithInfo);
    }

    public unsafe HResult GetFunctionEnter3Info(FunctionId functionId, COR_PRF_ELT_INFO eltInfo, out COR_PRF_FRAME_INFO pFrameInfo, int* pcbArgumentInfo, COR_PRF_FUNCTION_ARGUMENT_INFO* pArgumentInfo)
    {
        return _impl.GetFunctionEnter3Info(functionId, eltInfo, out pFrameInfo, pcbArgumentInfo, pArgumentInfo);
    }

    public HResult<FunctionLeave3Info> GetFunctionLeave3Info(FunctionId functionId, COR_PRF_ELT_INFO eltInfo)
    {
        var result = _impl.GetFunctionLeave3Info(functionId, eltInfo, out var frameInfo, out var retvalRange);
        return new(result, new(frameInfo, retvalRange));
    }

    public HResult<COR_PRF_FRAME_INFO> GetFunctionTailcall3Info(FunctionId functionId, COR_PRF_ELT_INFO eltInfo)
    {
        var result = _impl.GetFunctionTailcall3Info(functionId, eltInfo, out var pFrameInfo);
        return new(result, pFrameInfo);
    }

    public unsafe HResult<INativeEnumerator<ModuleId>> EnumModules()
    {
        var result = _impl.EnumModules(out var pEnum);
        return new(result, new(pEnum));
    }

    public unsafe HResult<RuntimeInformation> GetRuntimeInformation(Span<char> versionString, out uint versionStringLength)
    {
        fixed (char* szVersionString = versionString)
        {
            var result = _impl.GetRuntimeInformation(out var clrInstanceId, out var runtimeType, out var majorVersion, out var minorVersion, out var buildNumber, out var qfeVersion, (uint)versionString.Length, out versionStringLength, szVersionString);
            return new(result, new(clrInstanceId, runtimeType, majorVersion, minorVersion, buildNumber, qfeVersion));
        }
    }

    public unsafe HResult<nint> GetThreadStaticAddress2(ClassId classId, MdFieldDef fieldToken, AppDomainId appDomainId, ThreadId threadId)
    {
        var result = _impl.GetThreadStaticAddress2(classId, fieldToken, appDomainId, threadId, out var address);
        return new(result, (nint)address);
    }

    public unsafe HResult GetAppDomainsContainingModule(ModuleId moduleId, Span<AppDomainId> appDomainIds, out uint nbAppDomainIds)
    {
        fixed (AppDomainId* cAppDomainIds = appDomainIds)
        {
            return _impl.GetAppDomainsContainingModule(moduleId, (uint)appDomainIds.Length, out nbAppDomainIds, cAppDomainIds);
        }
    }

    public unsafe HResult<ModuleInfo2> GetModuleInfo2(ModuleId moduleId, Span<char> moduleName, out uint moduleNameLength)
    {
        fixed (char* szName = moduleName)
        {
            var result = _impl.GetModuleInfo2(moduleId, out var baseLoadAddress, (uint)moduleName.Length, out moduleNameLength, szName, out var assemblyId, out var moduleFlags);
            return new(result, new((nint)baseLoadAddress, assemblyId, moduleFlags));
        }
    }

    public unsafe HResult<ModuleInfoWithName2> GetModuleInfo2(ModuleId moduleId)
    {
        var (result, _) = GetModuleInfo2(moduleId, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        (result, var moduleInfo) = GetModuleInfo2(moduleId, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(buffer.WithoutNullTerminator(), moduleInfo.BaseLoadAddress, moduleInfo.AssemblyId, moduleInfo.ModuleFlags));
    }
}