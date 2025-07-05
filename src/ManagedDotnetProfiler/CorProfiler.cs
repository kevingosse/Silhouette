using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Silhouette;
using System.Linq;
using dnlib.DotNet.Emit;
using Silhouette.IL;

namespace ManagedDotnetProfiler;

internal unsafe class CorProfiler : CorProfilerCallback10Base
{
    private readonly ConcurrentDictionary<AssemblyId, bool> _assemblyLoads = new();
    private readonly ConcurrentDictionary<ClassId, bool> _classLoads = new();
    private readonly ConcurrentDictionary<int, int> _nestedCatchBlocks = new();
    private readonly ConcurrentDictionary<int, int> _nestedExceptionSearchFilter = new();
    private readonly ConcurrentDictionary<int, int> _nestedExceptionSearchFunction = new();
    private readonly ConcurrentDictionary<int, int> _nestedExceptionUnwindFinally = new();
    private readonly ConcurrentDictionary<int, int> _nestedExceptionUnwindFunction = new();
    private int _garbageCollectionsInProgress;

    public static CorProfiler Instance { get; private set; }

    public static ConcurrentQueue<string> Logs { get; } = new();

    public (bool result, ulong threadId, uint osId) GetCurrentThreadInfo()
    {
        var (result, threadId) = ICorProfilerInfo.GetCurrentThreadId();

        if (!result.IsOK)
        {
            Error(result, nameof(ICorProfilerInfo.GetCurrentThreadId));
            return default;
        }

        // Can't call GetThreadInfo in the CLR thread
        uint osId = 0;

        Task.Run(() =>
        {
            (result, osId) = ICorProfilerInfo.GetThreadInfo(threadId);
        }).Wait();

        if (!result.IsOK)
        {
            Error(result, nameof(ICorProfilerInfo.GetThreadInfo));
            return default;
        }

        return (true, threadId.Value, osId);
    }

    protected override HResult Initialize(int iCorProfilerInfoVersion)
    {
        if (iCorProfilerInfoVersion < 13)
        {
            Console.WriteLine($"This profiler requires ICorProfilerInfo13 ({iCorProfilerInfoVersion})");
            return HResult.E_FAIL;
        }

        Console.WriteLine("[Profiler] *** Profiler initialized ***");

        Instance = this;

        var eventMask = COR_PRF_MONITOR.COR_PRF_MONITOR_ALL;

        if (Environment.GetEnvironmentVariable("MONITOR_NGEN") == "1")
        {
            // JITCachedFunctionSearch events are not raised when those are enabled
            eventMask = eventMask
                & ~COR_PRF_MONITOR.COR_PRF_MONITOR_CODE_TRANSITIONS
                & ~COR_PRF_MONITOR.COR_PRF_MONITOR_ENTERLEAVE;
        }

        const COR_PRF_HIGH_MONITOR highEventMask = COR_PRF_HIGH_MONITOR.COR_PRF_HIGH_MONITOR_DYNAMIC_FUNCTION_UNLOADS;

        Log($"Setting event mask to {eventMask}");
        Log($"Setting high event mask to {highEventMask}");

        return ICorProfilerInfo5.SetEventMask2(eventMask, COR_PRF_HIGH_MONITOR.COR_PRF_HIGH_MONITOR_DYNAMIC_FUNCTION_UNLOADS);
    }

    protected override HResult JITCompilationStarted(FunctionId functionId, bool fIsSafeToBlock)
    {
        Log($"JITCompilationStarted - {GetFunctionFullName(functionId)}");

        var functionName = GetFunctionFullName(functionId);

        if (functionName.Contains("RejitTest.Test"))
        {
            using var rewriter = new IlRewriter(ICorProfilerInfo3);
            rewriter.Import(functionId);

            Console.WriteLine("Original method body:");

            foreach (var instruction in rewriter.Body.Instructions)
            {
                Console.WriteLine(instruction);

                if (instruction.OpCode == OpCodes.Ldstr && instruction.Operand is "Failure")
                {
                    instruction.Operand = "Success!";
                }
            }

            rewriter.Export();
        }

        return HResult.S_OK;
    }

    protected override HResult JITCompilationFinished(FunctionId functionId, HResult hrStatus, bool fIsSafeToBlock)
    {
        Log($"JITCompilationFinished - {GetFunctionFullName(functionId)}");
        return HResult.S_OK;
    }

    protected override HResult JITFunctionPitched(FunctionId functionId)
    {
        Environment.FailFast("Never called by the CLR");
        return HResult.E_NOTIMPL;
    }

    protected override HResult JITCachedFunctionSearchStarted(FunctionId functionId, out bool pbUseCachedFunction)
    {
        Log($"JITCachedFunctionSearchStarted - {GetFunctionFullName(functionId)}");
        pbUseCachedFunction = true;
        return HResult.S_OK;
    }

    protected override HResult JITCachedFunctionSearchFinished(FunctionId functionId, COR_PRF_JIT_CACHE searchResult)
    {
        if (searchResult == COR_PRF_JIT_CACHE.COR_PRF_CACHED_FUNCTION_FOUND)
        {
            Log($"JITCachedFunctionSearchFinished - {GetFunctionFullName(functionId)} - Found");

            var (result, functionInfo) = ICorProfilerInfo2.GetFunctionInfo(functionId);

            if (!result)
            {
                Error(result, nameof(ICorProfilerInfo2.GetFunctionInfo));
                return HResult.E_FAIL;
            }

            (result, var (methods, _)) = ICorProfilerInfo6.EnumNgenModuleMethodsInliningThisMethod(
                functionInfo.ModuleId, functionInfo.ModuleId, new(functionInfo.Token));

            if (!result)
            {
                Error(result, nameof(ICorProfilerInfo6.EnumNgenModuleMethodsInliningThisMethod));
                return HResult.E_FAIL;
            }

            using var _ = methods;

            foreach (var method in methods.AsEnumerable())
            {
                (result, var inliningFunctionId) = ICorProfilerInfo.GetFunctionFromToken(method.ModuleId, method.MethodId.Value);

                if (!result)
                {
                    return HResult.E_FAIL;
                }

                Log($"JITCachedFunctionSearchFinished - {GetFunctionFullName(inliningFunctionId)} inlined {GetFunctionFullName(functionId)}");
            }
        }
        else
        {
            Log($"JITCachedFunctionSearchFinished - {GetFunctionFullName(functionId)} - Not found");
        }

        return HResult.S_OK;
    }

    protected override HResult ManagedToUnmanagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        var functionName = GetFunctionFullName(functionId);

        // Don't log FetchLastLog to avoid producing new logs while fetching logs
        if (!functionName.Contains("FetchLastLog"))
        {
            Log($"ManagedToUnmanagedTransition - {functionName} - {reason}");
        }

        return HResult.S_OK;
    }

    protected override HResult UnmanagedToManagedTransition(FunctionId functionId, COR_PRF_TRANSITION_REASON reason)
    {
        var functionName = GetFunctionFullName(functionId);

        // Don't log FetchLastLog to avoid producing new logs while fetching logs
        if (!functionName.Contains("FetchLastLog"))
        {
            Log($"UnmanagedToManagedTransition - {functionName} - {reason}");
        }

        return HResult.S_OK;
    }

    protected override HResult JITInlining(FunctionId callerId, FunctionId calleeId, out bool pfShouldInline)
    {
        Log($"JITInlining - {GetFunctionFullName(calleeId)} -> {GetFunctionFullName(callerId)}");
        pfShouldInline = true;
        return HResult.S_OK;
    }

    protected override HResult RuntimeSuspendStarted(COR_PRF_SUSPEND_REASON suspendReason)
    {
        Log($"RuntimeSuspendStarted - {suspendReason}");
        return HResult.S_OK;
    }

    protected override HResult RuntimeSuspendFinished()
    {
        Log("RuntimeSuspendFinished");
        return HResult.S_OK;
    }

    protected override HResult RuntimeResumeStarted()
    {
        Log("RuntimeResumeStarted");
        return HResult.S_OK;
    }

    protected override HResult RuntimeResumeFinished()
    {
        Log("RuntimeResumeFinished");
        return HResult.S_OK;
    }

    protected override HResult RuntimeThreadSuspended(ThreadId threadId)
    {
        var osId = ICorProfilerInfo.GetThreadInfo(threadId).ThrowIfFailed();
        Logs.Enqueue($"RuntimeThreadSuspended - {osId}");
        return HResult.S_OK;
    }

    protected override HResult RuntimeThreadResumed(ThreadId threadId)
    {
        var osId = ICorProfilerInfo.GetThreadInfo(threadId).ThrowIfFailed();
        Logs.Enqueue($"RuntimeThreadResumed - {osId}");
        return HResult.S_OK;
    }

    protected override HResult ThreadCreated(ThreadId threadId)
    {
        var osId = ICorProfilerInfo.GetThreadInfo(threadId).ThrowIfFailed();
        Logs.Enqueue($"ThreadCreated - {osId}");
        return HResult.S_OK;
    }

    protected override HResult ThreadDestroyed(ThreadId threadId)
    {
        var osId = ICorProfilerInfo.GetThreadInfo(threadId).ThrowIfFailed();
        Logs.Enqueue($"ThreadDestroyed - {osId}");
        return HResult.S_OK;
    }

    protected override HResult ThreadAssignedToOSThread(ThreadId managedThreadId, int osThreadId)
    {
        Logs.Enqueue($"ThreadAssignedToOSThread - {osThreadId}");
        return HResult.S_OK;
    }

    protected override HResult ThreadNameChanged(ThreadId threadId, uint cchName, char* name)
    {
        var threadName = new Span<char>(name, (int)cchName);
        Logs.Enqueue($"ThreadNameChanged - {threadName}");
        return HResult.S_OK;
    }

    protected override HResult ExceptionSearchCatcherFound(FunctionId functionId)
    {
        var functionInfo = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(functionInfo.ModuleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(functionInfo.Token)).ThrowIfFailed();
        var typeDefProps = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionSearchCatcherFound - {typeDefProps.TypeName}.{methodProperties.Name}");
        return HResult.S_OK;
    }

    protected override HResult AppDomainCreationStarted(AppDomainId appDomainId)
    {
        var (appDomainName, processId) = ICorProfilerInfo.GetAppDomainInfo(appDomainId).ThrowIfFailed();

        Log($"AppDomainCreationStarted - {appDomainName} - Process Id {processId.Value}");

        return HResult.S_OK;
    }

    protected override HResult AppDomainCreationFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        var (appDomainName, _) = ICorProfilerInfo.GetAppDomainInfo(appDomainId).ThrowIfFailed();

        Log($"AppDomainCreationFinished - {appDomainName} - HResult {hrStatus}");

        return HResult.S_OK;
    }

    protected override HResult AppDomainShutdownStarted(AppDomainId appDomainId)
    {
        // TODO: Test on .NET Framework
        var (appDomainName, _) = ICorProfilerInfo.GetAppDomainInfo(appDomainId).ThrowIfFailed();

        Log($"AppDomainShutdownStarted - {appDomainName}");

        return HResult.S_OK;
    }

    protected override HResult AppDomainShutdownFinished(AppDomainId appDomainId, HResult hrStatus)
    {
        // TODO: Test on .NET Framework
        var (appDomainName, _) = ICorProfilerInfo.GetAppDomainInfo(appDomainId).ThrowIfFailed();

        Log($"AppDomainShutdownFinished - {appDomainName} - HResult {hrStatus}");

        return HResult.S_OK;
    }

    protected override HResult AssemblyLoadStarted(AssemblyId assemblyId)
    {
        if (!_assemblyLoads.TryAdd(assemblyId, true))
        {
            Error($"Assembly {assemblyId} already loading");
        }

        return HResult.S_OK;
    }

    protected override HResult AssemblyLoadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        var assemblyInfo = ICorProfilerInfo.GetAssemblyInfo(assemblyId).ThrowIfFailed();
        var (appDomainName, _) = ICorProfilerInfo.GetAppDomainInfo(assemblyInfo.AppDomainId).ThrowIfFailed();
        var moduleInfo = ICorProfilerInfo.GetModuleInfo(assemblyInfo.ModuleId).ThrowIfFailed();

        Log($"AssemblyLoadFinished - {assemblyInfo.AssemblyName} - AppDomain {appDomainName} - Module {moduleInfo.ModuleName}");

        if (!_assemblyLoads.TryRemove(assemblyId, out _))
        {
            Error($"Saw no AssemblyLoadStarted event for {assemblyId.Value}");
        }

        return HResult.S_OK;
    }

    protected override HResult AssemblyUnloadStarted(AssemblyId assemblyId)
    {
        // TODO: Test on .NET Framework or after the ALC bug is fixed
        Log("AssemblyUnloadStarted");

        return HResult.S_OK;
    }

    protected override HResult AssemblyUnloadFinished(AssemblyId assemblyId, HResult hrStatus)
    {
        var assemblyInfo = ICorProfilerInfo.GetAssemblyInfo(assemblyId).ThrowIfFailed();
        var (appDomainName, _) = ICorProfilerInfo.GetAppDomainInfo(assemblyInfo.AppDomainId).ThrowIfFailed();
        var (moduleName, _, _) = ICorProfilerInfo.GetModuleInfo(assemblyInfo.ModuleId).ThrowIfFailed();

        Log($"AssemblyUnloadFinished - {assemblyInfo.AssemblyName} - AppDomain {appDomainName} - Module {moduleName}");

        return HResult.S_OK;
    }

    protected override HResult ClassLoadStarted(ClassId classId)
    {
        if (!_classLoads.TryAdd(classId, true))
        {
            Error($"Class {classId.Value} already loading");
        }

        return HResult.S_OK;
    }

    protected override HResult ClassLoadFinished(ClassId classId, HResult hrStatus)
    {
        Log($"ClassLoadFinished - {GetTypeNameFromClassId(classId)}");

        if (!_classLoads.TryRemove(classId, out _))
        {
            Error($"Saw no ClassLoadStarted event for {classId.Value}");
        }

        return HResult.S_OK;
    }

    protected override HResult ClassUnloadStarted(ClassId classId)
    {
        Log($"ClassUnloadStarted - {GetTypeNameFromClassId(classId)}");
        return HResult.S_OK;
    }

    protected override HResult ClassUnloadFinished(ClassId classId, HResult hrStatus)
    {
        Log($"ClassUnloadFinished - {GetTypeNameFromClassId(classId)}");
        return HResult.S_OK;
    }

    protected override HResult COMClassicVTableCreated(ClassId wrappedClassId, in Guid implementedIID, void* pVTable, uint cSlots)
    {
        Log($"COMClassicVTableCreated - {GetTypeNameFromClassId(wrappedClassId)} - {implementedIID} - {cSlots}");
        return HResult.S_OK;
    }

    protected override HResult COMClassicVTableDestroyed(ClassId wrappedClassId, in Guid implementedIID, void* pVTable)
    {
        Error("The profiling API never raises the event COMClassicVTableDestroyed");
        return HResult.S_OK;
    }

    protected override HResult ConditionalWeakTableElementReferences(uint cRootRefs, ObjectId* keyRefIds, ObjectId* valueRefIds, GCHandleId* rootIds)
    {
        // Extract pairs of strings from the ConditionalWeakTable elements.

        var (stringLengthOffset, bufferOffset) = ICorProfilerInfo5.GetStringLayout2().ThrowIfFailed();

        for (int i = 0; i < cRootRefs; i++)
        {
            // Validate that they're strings
            var keyClassId = ICorProfilerInfo4.GetClassFromObject(keyRefIds[0]).ThrowIfFailed();
            var keyType = GetTypeNameFromClassId(keyClassId);

            if (keyType != "System.String")
            {
                continue;
            }

            var valueClassId = ICorProfilerInfo4.GetClassFromObject(valueRefIds[i]).ThrowIfFailed();
            var valueType = GetTypeNameFromClassId(valueClassId);

            if (valueType != "System.String")
            {
                continue;
            }

            var stringPtr1 = (byte*)keyRefIds[i].Value;
            var length1 = Unsafe.Read<int>(stringPtr1 + stringLengthOffset);
            var str1 = new ReadOnlySpan<char>(stringPtr1 + bufferOffset, length1);

            var stringPtr2 = (byte*)valueRefIds[i].Value;
            var length2 = Unsafe.Read<int>(stringPtr2 + stringLengthOffset);
            var str2 = new ReadOnlySpan<char>(stringPtr2 + bufferOffset, length2);

            Log($"ConditionalWeakTableElementReferences - {str1} -> {str2}");
        }

        return HResult.S_OK;
    }

    protected override HResult DynamicMethodJITCompilationStarted(FunctionId functionId, bool fIsSafeToBlock, byte* pILHeader, uint cbILHeader)
    {
        Log($"DynamicMethodJITCompilationStarted - {functionId.Value:x2}");
        return HResult.S_OK;
    }

    protected override HResult DynamicMethodJITCompilationFinished(FunctionId functionId, HResult hrStatus, bool fIsSafeToBlock)
    {
        Log($"DynamicMethodJITCompilationFinished - {functionId.Value:x2}");
        return HResult.S_OK;
    }

    protected override HResult DynamicMethodUnloaded(FunctionId functionId)
    {
        Log($"DynamicMethodUnloaded - {functionId.Value:x2}");
        return HResult.S_OK;
    }

    protected override HResult ExceptionCatcherEnter(FunctionId functionId, ObjectId objectId)
    {
        var typeName = GetTypeNameFromObjectId(objectId);

        var (_, moduleId, mdToken) = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(mdToken)).ThrowIfFailed();
        var (functionTypeName, _, _) = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionCatcherEnter - catch {typeName} in {functionTypeName}.{methodProperties.Name}");

        _nestedCatchBlocks.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (_, old) => old + 1);

        // It's weird but ExceptionUnwindFunctionLeave is not called when ExceptionCatcherEnter is called:
        // https://github.com/dotnet/runtime/issues/10871
        if (!_nestedExceptionUnwindFunction.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionCatcherEnter called without a matching ExceptionUnwindFunctionEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedExceptionUnwindFunction[Environment.CurrentManagedThreadId] = count;

        return HResult.S_OK;
    }

    protected override HResult ExceptionCatcherLeave()
    {
        if (!_nestedCatchBlocks.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionCatcherLeave called without a matching ExceptionCatcherEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedCatchBlocks[Environment.CurrentManagedThreadId] = count;

        var threadId = ICorProfilerInfo.GetCurrentThreadId().ThrowIfFailed();

        Log($"ExceptionCatcherLeave - Thread {threadId} - Nested level {count}");

        return HResult.S_OK;
    }

    protected override HResult ExceptionCLRCatcherExecute()
    {
        Error("The profiling API never raises the event ExceptionCLRCatcherExecute");
        return HResult.S_OK;
    }

    protected override HResult ExceptionCLRCatcherFound()
    {
        Error("The profiling API never raises the event ExceptionCLRCatcherFound");
        return HResult.S_OK;
    }

    protected override HResult ExceptionOSHandlerEnter(nint* _)
    {
        Error("The profiling API never raises the event ExceptionOSHandlerEnter");
        return HResult.S_OK;
    }

    protected override HResult ExceptionOSHandlerLeave(nint* _)
    {
        Error("The profiling API never raises the event ExceptionOSHandlerLeave");
        return HResult.S_OK;
    }

    protected override HResult ExceptionSearchFilterEnter(FunctionId functionId)
    {
        var (_, moduleId, mdToken) = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(mdToken)).ThrowIfFailed();
        var (functionTypeName, _, _) = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionSearchFilterEnter - {functionTypeName}.{methodProperties.Name}");

        _nestedExceptionSearchFilter.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (_, old) => old + 1);

        return HResult.S_OK;
    }

    protected override HResult ExceptionSearchFilterLeave()
    {
        if (!_nestedExceptionSearchFilter.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionSearchFilterLeave called without a matching ExceptionSearchFilterEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedExceptionSearchFilter[Environment.CurrentManagedThreadId] = count;

        var threadId = ICorProfilerInfo.GetCurrentThreadId().ThrowIfFailed();

        Log($"ExceptionSearchFilterLeave - Thread {threadId} - Nested level {count}");

        return HResult.S_OK;
    }

    protected override HResult ExceptionSearchFunctionEnter(FunctionId functionId)
    {
        var (_, moduleId, mdToken) = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(mdToken)).ThrowIfFailed();
        var (functionTypeName, _, _) = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionSearchFunctionEnter - {functionTypeName}.{methodProperties.Name}");

        _nestedExceptionSearchFunction.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (_, old) => old + 1);

        return HResult.S_OK;
    }

    protected override HResult ExceptionSearchFunctionLeave()
    {
        if (!_nestedExceptionSearchFunction.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionSearchFunctionLeave called without a matching ExceptionSearchFunctionEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedExceptionSearchFunction[Environment.CurrentManagedThreadId] = count;

        var threadId = ICorProfilerInfo.GetCurrentThreadId().ThrowIfFailed();

        Log($"ExceptionSearchFunctionLeave - Thread {threadId} - Nested level {count}");

        return HResult.S_OK;
    }
    protected override HResult ExceptionUnwindFinallyEnter(FunctionId functionId)
    {
        var (_, moduleId, mdToken) = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(mdToken)).ThrowIfFailed();
        var (functionTypeName, _, _) = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionUnwindFinallyEnter - {functionTypeName}.{methodProperties.Name}");

        _nestedExceptionUnwindFinally.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (_, old) => old + 1);

        return HResult.S_OK;
    }

    protected override HResult ExceptionUnwindFinallyLeave()
    {
        if (!_nestedExceptionUnwindFinally.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionUnwindFinallyLeave called without a matching ExceptionUnwindFinallyEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedExceptionUnwindFinally[Environment.CurrentManagedThreadId] = count;

        var threadId = ICorProfilerInfo.GetCurrentThreadId().ThrowIfFailed();

        Log($"ExceptionUnwindFinallyLeave - Thread {threadId} - Nested level {count}");

        return HResult.S_OK;
    }

    protected override HResult ExceptionUnwindFunctionEnter(FunctionId functionId)
    {
        var (_, moduleId, mdToken) = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
        var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(mdToken)).ThrowIfFailed();
        var (functionTypeName, _, _) = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

        Log($"ExceptionUnwindFunctionEnter - {functionTypeName}.{methodProperties.Name}");

        _nestedExceptionUnwindFunction.AddOrUpdate(Environment.CurrentManagedThreadId, 1, (_, old) => old + 1);

        return HResult.S_OK;
    }

    protected override HResult ExceptionUnwindFunctionLeave()
    {
        if (!_nestedExceptionUnwindFunction.TryGetValue(Environment.CurrentManagedThreadId, out var count) || count <= 0)
        {
            Error("ExceptionUnwindFunctionLeave called without a matching ExceptionUnwindFunctionEnter");
            return HResult.E_FAIL;
        }

        count -= 1;
        _nestedExceptionUnwindFunction[Environment.CurrentManagedThreadId] = count;

        var threadId = ICorProfilerInfo.GetCurrentThreadId().ThrowIfFailed();

        Log($"ExceptionUnwindFunctionLeave - Thread {threadId} - Nested level {count}");

        return HResult.S_OK;
    }

    protected override HResult ExceptionThrown(ObjectId thrownObjectId)
    {
        Log($"ExceptionThrown - {GetTypeNameFromObjectId(thrownObjectId)}");
        return HResult.S_OK;
    }

    protected override HResult FinalizeableObjectQueued(COR_PRF_FINALIZER_FLAGS finalizerFlags, ObjectId objectId)
    {
        Log($"FinalizeableObjectQueued - {finalizerFlags} - {GetTypeNameFromObjectId(objectId)}");
        return HResult.S_OK;
    }
    protected override HResult HandleCreated(GCHandleId handleId, ObjectId initialObjectId)
    {
        string name;

        try
        {
            name = GetTypeNameFromObjectId(initialObjectId);
        }
        catch (Win32Exception)
        {
            return HResult.S_OK;
        }

        Log($"HandleCreated - {handleId} - {name}");

        return HResult.S_OK;
    }

    protected override HResult HandleDestroyed(GCHandleId handleId)
    {
        Log($"HandleDestroyed - {handleId}");
        return HResult.S_OK;
    }

    protected override HResult GarbageCollectionStarted(Span<bool> generationCollected, COR_PRF_GC_REASON reason)
    {
        var generations = new List<int>();

        for (int i = 0; i < generationCollected.Length; i++)
        {
            if (generationCollected[i])
            {
                generations.Add(i);
            }
        }

        var count = Interlocked.Increment(ref _garbageCollectionsInProgress);

        Log($"GarbageCollectionStarted - {string.Join(", ", generations)} - {reason} - {count}");

        return HResult.S_OK;
    }

    protected override HResult GarbageCollectionFinished()
    {
        var count = Interlocked.Decrement(ref _garbageCollectionsInProgress);

        if (count < 0)
        {
            Error("GarbageCollectionFinished called without a matching GarbageCollectionStarted");
        }

        Log($"GarbageCollectionFinished - {count}");

        return HResult.S_OK;
    }

    protected override HResult Shutdown()
    {
        if (Environment.GetEnvironmentVariable("SHUTDOWN_LOGS") == "1")
        {
            Console.WriteLine("[Profiler] *** Shutting down, dumping remaining logs ***");

            while (Logs.TryDequeue(out var log))
            {
                Console.WriteLine($"[Profiler] {log}");
            }
        }
        else
        {
            Console.WriteLine("[Profiler] *** Shutting down ***");
        }

        return HResult.S_OK;
    }

    private static void Error(HResult hresult, string function)
    {
        Error($"Call to {function} failed with code {hresult}");
    }

    private static void Error(string explanation)
    {
        Log($"Error: {explanation}");
    }

    private static void Log(string line)
    {
        Logs.Enqueue(line);
        // Console.WriteLine($"[Profiler] {line}");
    }

    private string GetTypeNameFromObjectId(ObjectId objectId)
    {
        var classId = ICorProfilerInfo.GetClassFromObject(objectId).ThrowIfFailed();
        return GetTypeNameFromClassId(classId);
    }

    private string GetTypeNameFromClassId(ClassId classId)
    {
        var (moduleId, typeDef) = ICorProfilerInfo.GetClassIdInfo(classId).ThrowIfFailed();
        var moduleMetadata = ICorProfilerInfo.GetModuleMetaDataImport(moduleId, CorOpenFlags.ofRead).ThrowIfFailed();
        var typeDefProps = moduleMetadata.GetTypeDefProps(typeDef).ThrowIfFailed();

        return typeDefProps.TypeName;
    }

    private string GetFunctionFullName(FunctionId functionId)
    {
        try
        {
            var functionInfo = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
            var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(functionInfo.ModuleId, CorOpenFlags.ofRead).ThrowIfFailed();
            var methodProperties = metaDataImport.GetMethodProps(new MdMethodDef(functionInfo.Token)).ThrowIfFailed();
            var typeDefProps = metaDataImport.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

            return $"{typeDefProps.TypeName}.{methodProperties.Name}";
        }
        catch (Win32Exception ex)
        {
            return $"Failed ({ex.NativeErrorCode})";
        }
    }

    internal bool GetThreads(uint* array, int length, int* actualLength)
    {
        var (result, threads) = ICorProfilerInfo4.EnumThreads();

        if (!result)
        {
            Error($"Failed to enumerate threads: {result}");
            return false;
        }

        using var t = threads;

        Span<ThreadId> buffer = stackalloc ThreadId[5];
        int count = 0;

        foreach (var thread in threads.AsEnumerable(buffer))
        {
            if (count >= length)
            {
                break;
            }

            var (_, osId) = ICorProfilerInfo.GetThreadInfo(thread);

            array[count] = osId;
            count++;
        }

        *actualLength = count;

        return true;
    }

    internal int GetGenericArguments(nint typeHandle, int methodToken, char* buffer, int length)
    {
        var resolvedGenericParams = new List<string>();

        var classIdInfo = ICorProfilerInfo.GetClassIdInfo(new(typeHandle)).ThrowIfFailed();
        using var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport2(classIdInfo.ModuleId, CorOpenFlags.ofRead).ThrowIfFailed().Wrap();

        Span<MdGenericParam> genericParams = stackalloc MdGenericParam[10];
        Span<MdGenericParamConstraint> constraints = stackalloc MdGenericParamConstraint[10];
        HCORENUM genericParamsEnum = default;

        while (metaDataImport.Value.EnumGenericParams(ref genericParamsEnum, new(methodToken), genericParams, out var nbGenericParams)
            && nbGenericParams > 0)
        {
            foreach (var genericParam in genericParams[..(int)nbGenericParams])
            {
                var genericParamProps = metaDataImport.Value.GetGenericParamProps(genericParam).ThrowIfFailed();
                var resolvedGenericParam = genericParamProps.Name;

                var resolvedConstraints = new List<string>();
                HCORENUM constraintsEnum = default;

                while (metaDataImport.Value.EnumGenericParamConstraints(ref constraintsEnum, genericParam, constraints, out var nbConstraints)
                    && nbConstraints > 0)
                {
                    foreach (var constraint in constraints[..(int)nbConstraints])
                    {
                        var constraintProps = metaDataImport.Value.GetGenericParamConstraintProps(constraint).ThrowIfFailed();

                        if (constraintProps.ConstraintType.IsTypeRef())
                        {
                            var constraintTypeDef = metaDataImport.Value.GetTypeRefProps(new(constraintProps.ConstraintType)).ThrowIfFailed();
                            resolvedConstraints.Add(constraintTypeDef.TypeName);
                        }
                        else if (constraintProps.ConstraintType.IsTypeDef())
                        {
                            var constraintTypeDef = metaDataImport.Value.GetTypeDefProps(new(constraintProps.ConstraintType)).ThrowIfFailed();
                            resolvedConstraints.Add(constraintTypeDef.TypeName);
                        }
                    }
                }

                metaDataImport.Value.CloseEnum(constraintsEnum);

                if (resolvedConstraints.Count > 0)
                {
                    resolvedGenericParam += $"({string.Join(", ", resolvedConstraints)})";
                }

                resolvedGenericParams.Add(resolvedGenericParam);
            }
        }

        metaDataImport.Value.CloseEnum(genericParamsEnum);

        var result = string.Join(", ", resolvedGenericParams);

        if (result.Length > length)
        {
            Error("The buffer was too small");
            return -1;
        }

        result.AsSpan().CopyTo(new Span<char>(buffer, length));
        return result.Length;
    }

    internal int GetModuleNames(char* buffer, int length)
    {
        var (result, modules) = ICorProfilerInfo3.EnumModules();

        if (!result)
        {
            Error($"Failed to enumerate modules: {result}");
            return 0;
        }

        using var _ = modules;

        int size = 0;

        foreach (var module in modules.AsEnumerable())
        {
            var moduleInfo = ICorProfilerInfo.GetModuleInfo(module).ThrowIfFailed();
            var moduleName = moduleInfo.ModuleName;

            if (size + moduleName.Length > length)
            {
                Error("The buffer was too small");
                return -1;
            }

            fixed (char* moduleNamePtr = moduleName)
            {
                // Add 1 to the length to include the null terminator
                new ReadOnlySpan<char>(moduleNamePtr, moduleName.Length + 1)
                    .CopyTo(new Span<char>(buffer + size, moduleName.Length + 1));
            }

            size += moduleName.Length + 1;
        }

        return size;
    }

    internal bool EnumJittedFunctions(int apiVersion)
    {
        Func<HResult<INativeEnumerator<COR_PRF_FUNCTION>>> enumJittedFunctions = apiVersion switch
        {
            1 => ICorProfilerInfo3.EnumJITedFunctions,
            2 => ICorProfilerInfo4.EnumJITedFunctions2,
            _ => throw new InvalidOperationException($"Unknown API version {apiVersion}")
        };

        var (result, jittedFunctions) = enumJittedFunctions();

        if (!result)
        {
            Error(result, nameof(ICorProfilerInfo3.EnumJITedFunctions));
            return false;
        }

        using var _ = jittedFunctions;

        foreach (var functionInfo in jittedFunctions.AsEnumerable())
        {
            var name = GetFunctionFullName(functionInfo.FunctionId);
            Log($"Jitted function: {name}");
        }

        return true;
    }

    internal int CountFrozenObjects()
    {
        int count = 0;

        var (result, modules) = ICorProfilerInfo3.EnumModules();

        if (!result)
        {
            Error(result, nameof(ICorProfilerInfo3.EnumModules));
            return -1;
        }

        using var _ = modules;

        foreach (var module in modules.AsEnumerable())
        {
            (result, var frozenObjects) = ICorProfilerInfo2.EnumModuleFrozenObjects(module);

            if (!result)
            {
                // EnumModuleFrozenObjects should never fail
                Error(result, nameof(ICorProfilerInfo3.EnumModuleFrozenObjects));
                return -1;
            }

            using var __ = frozenObjects;

            count += frozenObjects.AsEnumerable().Count();
        }

        return count;
    }
}