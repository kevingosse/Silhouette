using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallbackBase : Unknown, ICorProfilerCallback
{
    private readonly NativeObjects.ICorProfilerCallback _corProfilerCallback;

    protected CorProfilerCallbackBase()
    {
        _corProfilerCallback = NativeObjects.ICorProfilerCallback.Wrap(this);
    }

    public nint ICorProfilerCallback => _corProfilerCallback;
    public ICorProfilerInfo ICorProfilerInfo;
    public ICorProfilerInfo2 ICorProfilerInfo2;
    public ICorProfilerInfo3 ICorProfilerInfo3;
    public ICorProfilerInfo4 ICorProfilerInfo4;
    public ICorProfilerInfo5 ICorProfilerInfo5;
    public ICorProfilerInfo6 ICorProfilerInfo6;
    public ICorProfilerInfo7 ICorProfilerInfo7;
    public ICorProfilerInfo8 ICorProfilerInfo8;
    public ICorProfilerInfo9 ICorProfilerInfo9;
    public ICorProfilerInfo10 ICorProfilerInfo10;
    public ICorProfilerInfo11 ICorProfilerInfo11;
    public ICorProfilerInfo12 ICorProfilerInfo12;
    public ICorProfilerInfo13 ICorProfilerInfo13;
    public ICorProfilerInfo14 ICorProfilerInfo14;
    public ICorProfilerInfo15 ICorProfilerInfo15;

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == Interfaces.ICorProfilerCallback.Guid)
        {
            ptr = _corProfilerCallback;
            return HResult.S_OK;
        }

        ptr = 0;
        return HResult.E_NOINTERFACE;
    }

    protected abstract HResult Initialize(int iCorProfilerInfoVersion);

    private static bool TryInitialize<T>(NativeObjects.IUnknownInvoker iUnknown, ref T destination, ref int counter)
        where T : ICorProfilerInfoFactory<T>
    {
        var result = iUnknown.QueryInterface(T.Guid, out var ptr);

        if (!result.IsOK)
        {
            return false;
        }

        destination = T.Create(ptr);
        counter++;
        return true;
    }

    // ReSharper disable once DuplicatedStatements
    private int GetICorProfilerInfo(nint pICorProfilerInfoUnk)
    {
        int supportedInterface = 0;
        var impl = new NativeObjects.IUnknownInvoker(pICorProfilerInfoUnk);

        if (!TryInitialize(impl, ref ICorProfilerInfo, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo2, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo3, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo4, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo5, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo6, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo7, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo8, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo9, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo10, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo11, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo12, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo13, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo14, ref supportedInterface))
        {
            return supportedInterface;
        }

        if (!TryInitialize(impl, ref ICorProfilerInfo15, ref supportedInterface))
        {
            return supportedInterface;
        }

        return supportedInterface;
    }

    #region ICorProfilerCallback

    HResult ICorProfilerCallback.Initialize(nint pICorProfilerInfoUnk)
    {
        int version = GetICorProfilerInfo(pICorProfilerInfoUnk);

        return Initialize(version);
    }

    HResult ICorProfilerCallback.Shutdown()
    {
        return Shutdown();
    }

    HResult ICorProfilerCallback.AppDomainCreationStarted(AppDomainId appDomainId)
    {
        return AppDomainCreationStarted(appDomainId);
    }

    HResult ICorProfilerCallback.AppDomainCreationFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        return AppDomainCreationFinished(appDomainId, hrStatus);
    }

    HResult ICorProfilerCallback.AppDomainShutdownStarted(AppDomainId appDomainId)
    {
        return AppDomainShutdownStarted(appDomainId);
    }

    HResult ICorProfilerCallback.AppDomainShutdownFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        return AppDomainShutdownFinished(appDomainId, hrStatus);
    }

    HResult ICorProfilerCallback.AssemblyLoadStarted(AssemblyId assemblyId)
    {
        return AssemblyLoadStarted(assemblyId);
    }

    HResult ICorProfilerCallback.AssemblyLoadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        return AssemblyLoadFinished(assemblyId, hrStatus);
    }

    HResult ICorProfilerCallback.AssemblyUnloadStarted(AssemblyId assemblyId)
    {
        return AssemblyUnloadStarted(assemblyId);
    }

    HResult ICorProfilerCallback.AssemblyUnloadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        return AssemblyUnloadFinished(assemblyId, hrStatus);
    }

    HResult ICorProfilerCallback.ModuleLoadStarted(ModuleId moduleId)
    {
        return ModuleLoadStarted(moduleId);
    }

    HResult ICorProfilerCallback.ModuleLoadFinished(ModuleId moduleId, HResult hrStatus)
    {
        return ModuleLoadFinished(moduleId, hrStatus);
    }

    HResult ICorProfilerCallback.ModuleUnloadStarted(ModuleId moduleId)
    {
        return ModuleUnloadStarted(moduleId);
    }

    HResult ICorProfilerCallback.ModuleUnloadFinished(ModuleId moduleId, HResult hrStatus)
    {
        return ModuleUnloadFinished(moduleId, hrStatus);
    }

    HResult ICorProfilerCallback.ModuleAttachedToAssembly(ModuleId moduleId, AssemblyId assemblyId)
    {
        return ModuleAttachedToAssembly(moduleId, assemblyId);
    }

    HResult ICorProfilerCallback.ClassLoadStarted(ClassId classId)
    {
        return ClassLoadStarted(classId);
    }

    HResult ICorProfilerCallback.ClassLoadFinished(ClassId classId, HResult hrStatus)
    {
        return ClassLoadFinished(classId, hrStatus);
    }

    HResult ICorProfilerCallback.ClassUnloadStarted(ClassId classId)
    {
        return ClassUnloadStarted(classId);
    }

    HResult ICorProfilerCallback.ClassUnloadFinished(ClassId classId, HResult hrStatus)
    {
        return ClassUnloadFinished(classId, hrStatus);
    }

    HResult ICorProfilerCallback.FunctionUnloadStarted(FunctionId functionId)
    {
        return FunctionUnloadStarted(functionId);
    }

    HResult ICorProfilerCallback.JITCompilationStarted(FunctionId functionId, int fIsSafeToBlock)
    {
        return JITCompilationStarted(functionId, fIsSafeToBlock != 0);
    }

    HResult ICorProfilerCallback.JITCompilationFinished(FunctionId functionId, HResult hrStatus, int fIsSafeToBlock)
    {
        return JITCompilationFinished(functionId, hrStatus, fIsSafeToBlock != 0);
    }

    HResult ICorProfilerCallback.JITCachedFunctionSearchStarted(FunctionId functionId, out int pbUseCachedFunction)
    {
        var result = JITCachedFunctionSearchStarted(functionId, out var useCachedFunction);

        pbUseCachedFunction = useCachedFunction ? 1 : 0;

        return result;
    }

    HResult ICorProfilerCallback.JITCachedFunctionSearchFinished(FunctionId functionId, COR_PRF_JIT_CACHE result)
    {
        return JITCachedFunctionSearchFinished(functionId, result);
    }

    HResult ICorProfilerCallback.JITFunctionPitched(FunctionId functionId)
    {
        return JITFunctionPitched(functionId);
    }

    HResult ICorProfilerCallback.JITInlining(FunctionId callerId, FunctionId calleeId, out int pfShouldInline)
    {
        var result = JITInlining(callerId, calleeId, out var shouldInline);

        pfShouldInline = shouldInline ? 1 : 0;

        return result;
    }

    HResult ICorProfilerCallback.ThreadCreated(ThreadId threadId)
    {
        return ThreadCreated(threadId);
    }

    HResult ICorProfilerCallback.ThreadDestroyed(ThreadId threadId)
    {
        return ThreadDestroyed(threadId);
    }

    HResult ICorProfilerCallback.ThreadAssignedToOSThread(ThreadId managedThreadId, int osThreadId)
    {
        return ThreadAssignedToOSThread(managedThreadId, osThreadId);
    }

    HResult ICorProfilerCallback.RemotingClientInvocationStarted()
    {
        return RemotingClientInvocationStarted();
    }

    HResult ICorProfilerCallback.RemotingClientSendingMessage(in Guid pCookie, int fIsAsync)
    {
        return RemotingClientSendingMessage(in pCookie, fIsAsync != 0);
    }

    HResult ICorProfilerCallback.RemotingClientReceivingReply(in Guid pCookie, int fIsAsync)
    {
        return RemotingClientReceivingReply(in pCookie, fIsAsync != 0);
    }

    HResult ICorProfilerCallback.RemotingClientInvocationFinished()
    {
        return RemotingClientInvocationFinished();
    }

    HResult ICorProfilerCallback.RemotingServerReceivingMessage(in Guid pCookie, int fIsAsync)
    {
        return RemotingServerReceivingMessage(in pCookie, fIsAsync != 0);
    }

    HResult ICorProfilerCallback.RemotingServerInvocationStarted()
    {
        return RemotingServerInvocationStarted();
    }

    HResult ICorProfilerCallback.RemotingServerInvocationReturned()
    {
        return RemotingServerInvocationReturned();
    }

    HResult ICorProfilerCallback.RemotingServerSendingReply(in Guid pCookie, int fIsAsync)
    {
        return RemotingServerSendingReply(in pCookie, fIsAsync != 0);
    }

    HResult ICorProfilerCallback.UnmanagedToManagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        return UnmanagedToManagedTransition(functionId, reason);
    }

    HResult ICorProfilerCallback.ManagedToUnmanagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        return ManagedToUnmanagedTransition(functionId, reason);
    }

    HResult ICorProfilerCallback.RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
    {
        return RuntimeSuspendStarted(suspendReason);
    }

    HResult ICorProfilerCallback.RuntimeSuspendFinished()
    {
        return RuntimeSuspendFinished();
    }

    HResult ICorProfilerCallback.RuntimeSuspendAborted()
    {
        return RuntimeSuspendAborted();
    }

    HResult ICorProfilerCallback.RuntimeResumeStarted()
    {
        return RuntimeResumeStarted();
    }

    HResult ICorProfilerCallback.RuntimeResumeFinished()
    {
        return RuntimeResumeFinished();
    }

    HResult ICorProfilerCallback.RuntimeThreadSuspended(ThreadId threadId)
    {
        return RuntimeThreadSuspended(threadId);
    }

    HResult ICorProfilerCallback.RuntimeThreadResumed(ThreadId threadId)
    {
        return RuntimeThreadResumed(threadId);
    }

    unsafe HResult ICorProfilerCallback.MovedReferences(uint cMovedObjectIDRanges, ObjectId* oldObjectIDRangeStart, ObjectId* newObjectIDRangeStart, uint* cObjectIDRangeLength)
    {
        return MovedReferences(cMovedObjectIDRanges, oldObjectIDRangeStart, newObjectIDRangeStart, cObjectIDRangeLength);
    }

    HResult ICorProfilerCallback.ObjectAllocated(ObjectId objectId, ClassId classId)
    {
        return ObjectAllocated(objectId, classId);
    }

    unsafe HResult ICorProfilerCallback.ObjectsAllocatedByClass(uint cClassCount, ClassId* classIds, uint* cObjects)
    {
        return ObjectsAllocatedByClass(cClassCount, classIds, cObjects);
    }

    unsafe HResult ICorProfilerCallback.ObjectReferences(ObjectId objectId, ClassId classId, uint cObjectRefs, ObjectId* objectRefIds)
    {
        return ObjectReferences(objectId, classId, cObjectRefs, objectRefIds);
    }

    unsafe HResult ICorProfilerCallback.RootReferences(uint cRootRefs, ObjectId* rootRefIds)
    {
        return RootReferences(cRootRefs, rootRefIds);
    }

    HResult ICorProfilerCallback.ExceptionThrown(ObjectId thrownObjectId)
    {
        return ExceptionThrown(thrownObjectId);
    }

    HResult ICorProfilerCallback.ExceptionSearchFunctionEnter(FunctionId functionId)
    {
        return ExceptionSearchFunctionEnter(functionId);
    }

    HResult ICorProfilerCallback.ExceptionSearchFunctionLeave()
    {
        return ExceptionSearchFunctionLeave();
    }

    HResult ICorProfilerCallback.ExceptionSearchFilterEnter(FunctionId functionId)
    {
        return ExceptionSearchFilterEnter(functionId);
    }

    HResult ICorProfilerCallback.ExceptionSearchFilterLeave()
    {
        return ExceptionSearchFilterLeave();
    }

    HResult ICorProfilerCallback.ExceptionSearchCatcherFound(FunctionId functionId)
    {
        return ExceptionSearchCatcherFound(functionId);
    }

    unsafe HResult ICorProfilerCallback.ExceptionOSHandlerEnter(nint* __unused)
    {
        return ExceptionOSHandlerEnter(__unused);
    }

    unsafe HResult ICorProfilerCallback.ExceptionOSHandlerLeave(nint* __unused)
    {
        return ExceptionOSHandlerLeave(__unused);
    }

    HResult ICorProfilerCallback.ExceptionUnwindFunctionEnter(FunctionId functionId)
    {
        return ExceptionUnwindFunctionEnter(functionId);
    }

    HResult ICorProfilerCallback.ExceptionUnwindFunctionLeave()
    {
        return ExceptionUnwindFunctionLeave();
    }

    HResult ICorProfilerCallback.ExceptionUnwindFinallyEnter(FunctionId functionId)
    {
        return ExceptionUnwindFinallyEnter(functionId);
    }

    HResult ICorProfilerCallback.ExceptionUnwindFinallyLeave()
    {
        return ExceptionUnwindFinallyLeave();
    }

    HResult ICorProfilerCallback.ExceptionCatcherEnter(FunctionId functionId, ObjectId objectId)
    {
        return ExceptionCatcherEnter(functionId, objectId);
    }

    HResult ICorProfilerCallback.ExceptionCatcherLeave()
    {
        return ExceptionCatcherLeave();
    }

    unsafe HResult ICorProfilerCallback.COMClassicVTableCreated(ClassId wrappedClassId, in Guid implementedIID, void* pVTable, uint cSlots)
    {
        return COMClassicVTableCreated(wrappedClassId, in implementedIID, pVTable, cSlots);
    }

    unsafe HResult ICorProfilerCallback.COMClassicVTableDestroyed(ClassId wrappedClassId, in Guid implementedIID, void* pVTable)
    {
        return COMClassicVTableDestroyed(wrappedClassId, in implementedIID, pVTable);
    }

    HResult ICorProfilerCallback.ExceptionCLRCatcherFound()
    {
        return ExceptionCLRCatcherFound();
    }

    HResult ICorProfilerCallback.ExceptionCLRCatcherExecute()
    {
        return ExceptionCLRCatcherExecute();
    }

    #endregion

    protected virtual HResult Shutdown()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AppDomainCreationStarted(AppDomainId appDomainId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AppDomainCreationFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AppDomainShutdownStarted(AppDomainId appDomainId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AppDomainShutdownFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AssemblyLoadStarted(AssemblyId assemblyId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AssemblyLoadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AssemblyUnloadStarted(AssemblyId assemblyId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult AssemblyUnloadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ModuleLoadStarted(ModuleId moduleId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ModuleLoadFinished(ModuleId moduleId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ModuleUnloadStarted(ModuleId moduleId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ModuleUnloadFinished(ModuleId moduleId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ModuleAttachedToAssembly(ModuleId moduleId, AssemblyId assemblyId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ClassLoadStarted(ClassId classId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ClassLoadFinished(ClassId classId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ClassUnloadStarted(ClassId classId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ClassUnloadFinished(ClassId classId, HResult hrStatus)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult FunctionUnloadStarted(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITCompilationStarted(FunctionId functionId, bool fIsSafeToBlock)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITCompilationFinished(FunctionId functionId, HResult hrStatus, bool fIsSafeToBlock)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITCachedFunctionSearchStarted(FunctionId functionId, out bool pbUseCachedFunction)
    {
        pbUseCachedFunction = false;

        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITCachedFunctionSearchFinished(FunctionId functionId, COR_PRF_JIT_CACHE result)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITFunctionPitched(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult JITInlining(FunctionId callerId, FunctionId calleeId, out bool pfShouldInline)
    {
        pfShouldInline = false;

        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ThreadCreated(ThreadId threadId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ThreadDestroyed(ThreadId threadId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ThreadAssignedToOSThread(ThreadId managedThreadId, int osThreadId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingClientInvocationStarted()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingClientSendingMessage(in Guid pCookie, bool fIsAsync)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingClientReceivingReply(in Guid pCookie, bool fIsAsync)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingClientInvocationFinished()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingServerReceivingMessage(in Guid pCookie, bool fIsAsync)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingServerInvocationStarted()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingServerInvocationReturned()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RemotingServerSendingReply(in Guid pCookie, bool fIsAsync)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult UnmanagedToManagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ManagedToUnmanagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeSuspendFinished()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeSuspendAborted()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeResumeStarted()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeResumeFinished()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeThreadSuspended(ThreadId threadId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult RuntimeThreadResumed(ThreadId threadId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult MovedReferences(uint cMovedObjectIDRanges, ObjectId* oldObjectIDRangeStart, ObjectId* newObjectIDRangeStart, uint* cObjectIDRangeLength)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ObjectAllocated(ObjectId objectId, ClassId classId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult ObjectsAllocatedByClass(uint cClassCount, ClassId* classIds, uint* cObjects)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult ObjectReferences(ObjectId objectId, ClassId classId, uint cObjectRefs, ObjectId* objectRefIds)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult RootReferences(uint cRootRefs, ObjectId* rootRefIds)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionThrown(ObjectId thrownObjectId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionSearchFunctionEnter(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionSearchFunctionLeave()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionSearchFilterEnter(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionSearchFilterLeave()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionSearchCatcherFound(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult ExceptionOSHandlerEnter(nint* __unused)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult ExceptionOSHandlerLeave(nint* __unused)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionUnwindFunctionEnter(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionUnwindFunctionLeave()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionUnwindFinallyEnter(FunctionId functionId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionUnwindFinallyLeave()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionCatcherEnter(FunctionId functionId, ObjectId objectId)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionCatcherLeave()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult COMClassicVTableCreated(ClassId wrappedClassId, in Guid implementedIID, void* pVTable, uint cSlots)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual unsafe HResult COMClassicVTableDestroyed(ClassId wrappedClassId, in Guid implementedIID, void* pVTable)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionCLRCatcherFound()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ExceptionCLRCatcherExecute()
    {
        return HResult.E_NOTIMPL;
    }
}