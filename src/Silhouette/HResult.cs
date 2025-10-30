using System.ComponentModel;

namespace Silhouette;

// ReSharper disable InconsistentNaming
public readonly struct HResult : IEquatable<HResult>
{
    public const int S_OK = 0;
    public const int S_FALSE = 1;
    public const int E_ABORT = unchecked((int)0x80004004);
    public const int E_FAIL = unchecked((int)0x80004005);
    public const int E_INVALIDARG = unchecked((int)0x80070057);
    public const int E_NOTIMPL = unchecked((int)0x80004001);
    public const int E_NOINTERFACE = unchecked((int)0x80004002);
    public const int CORPROF_E_UNSUPPORTED_CALL_SEQUENCE = unchecked((int)0x80131363);
    public const int CORPROF_E_PROFILER_CANCEL_ACTIVATION = unchecked((int)0x80131375);

    private static readonly Dictionary<int, (string name, string description)> KnownErrorCodes = new()
    {
        { S_OK, ("S_OK", null) },
        { S_FALSE, ("S_FALSE", null) },
        { E_ABORT, ("E_ABORT", null) },
        { E_FAIL, ("E_FAIL", null) },
        { E_INVALIDARG, ("E_INVALIDARG", null) },
        { E_NOTIMPL, ("E_NOTIMPL", null) },
        { E_NOINTERFACE, ("E_NOINTERFACE", null) },
        
        { unchecked((int)0x80131351), ("CORPROF_E_DATAINCOMPLETE", "The ID is not fully loaded/defined yet.") },
        { unchecked((int)0x80131354), ("CORPROF_E_FUNCTION_NOT_IL", "The Method has no associated IL.") },
        { unchecked((int)0x80131355), ("CORPROF_E_NOT_MANAGED_THREAD", "The thread has never run managed code before.") },
        { unchecked((int)0x80131356), ("CORPROF_E_CALL_ONLY_FROM_INIT", "The function may only be called during profiler initialization.") },
        { unchecked((int)0x8013135b), ("CORPROF_E_NOT_YET_AVAILABLE", "Requested information is not yet available.") },
        { unchecked((int)0x8013135c), ("CORPROF_E_TYPE_IS_PARAMETERIZED", "The given type is a generic and cannot be used with this method.") },
        { unchecked((int)0x8013135d), ("CORPROF_E_FUNCTION_IS_PARAMETERIZED", "The given function is a generic and cannot be used with this method.") },
        { unchecked((int)0x8013135e), ("CORPROF_E_STACKSNAPSHOT_INVALID_TGT_THREAD", "A profiler tried to walk the stack of an invalid thread") },
        { unchecked((int)0x8013135f), ("CORPROF_E_STACKSNAPSHOT_UNMANAGED_CTX", "A profiler can not walk a thread that is currently executing unmanaged code") },
        { unchecked((int)0x80131360), ("CORPROF_E_STACKSNAPSHOT_UNSAFE", "A stackwalk at this point may cause dead locks or data corruption") },
        { unchecked((int)0x80131361), ("CORPROF_E_STACKSNAPSHOT_ABORTED", "Stackwalking callback requested the walk to abort") },
        { unchecked((int)0x80131362), ("CORPROF_E_LITERALS_HAVE_NO_ADDRESS", "Returned when asked for the address of a static that is a literal.") },
        { CORPROF_E_UNSUPPORTED_CALL_SEQUENCE, ("CORPROF_E_UNSUPPORTED_CALL_SEQUENCE", "A call was made at an unsupported time.  Examples include illegally calling a profiling API method asynchronously, calling a method that might trigger a GC at an unsafe time, and calling a method at a time that could cause locks to be taken out of order.") },
        { unchecked((int)0x80131364), ("CORPROF_E_ASYNCHRONOUS_UNSAFE", "A legal asynchronous call was made at an unsafe time (e.g., CLR locks are held)") },
        { unchecked((int)0x80131365), ("CORPROF_E_CLASSID_IS_ARRAY", "The specified ClassID cannot be inspected by this function because it is an array") },
        { unchecked((int)0x80131366), ("CORPROF_E_CLASSID_IS_COMPOSITE", "The specified ClassID is a non-array composite type (e.g., ref) and cannot be inspected") },
        { unchecked((int)0x80131367), ("CORPROF_E_PROFILER_DETACHING", "The profiler's call into the CLR is disallowed because the profiler is attempting to detach.") },
        { unchecked((int)0x80131368), ("CORPROF_E_PROFILER_NOT_ATTACHABLE", "The profiler does not support attaching to a live process.") },
        { unchecked((int)0x80131369), ("CORPROF_E_UNRECOGNIZED_PIPE_MSG_FORMAT", "The message sent on the profiling API attach pipe is in an unrecognized format.") },
        { unchecked((int)0x8013136a), ("CORPROF_E_PROFILER_ALREADY_ACTIVE", "The request to attach a profiler was denied because a profiler is already loaded.") },
        { unchecked((int)0x8013136b), ("CORPROF_E_PROFILEE_INCOMPATIBLE_WITH_TRIGGER", "Unable to request a profiler attach because the target profilee's runtime is of a version incompatible with the current process calling AttachProfiler().") },
        { unchecked((int)0x8013136c), ("CORPROF_E_IPC_FAILED", "AttachProfiler() encountered an error while communicating on the pipe to the target profilee.  This is often caused by a target profilee that is shutting down or killed while AttachProfiler() is reading or writing the pipe.") },
        { unchecked((int)0x8013136d), ("CORPROF_E_PROFILEE_PROCESS_NOT_FOUND", "AttachProfiler() was unable to find a profilee with the specified process ID.") },
        { unchecked((int)0x8013136e), ("CORPROF_E_CALLBACK3_REQUIRED", "Profiler must implement ICorProfilerCallback3 interface for this call to be supported.") },
        { unchecked((int)0x8013136f), ("CORPROF_E_UNSUPPORTED_FOR_ATTACHING_PROFILER", "This call was attempted by a profiler that attached to the process after startup, but this call is only supported by profilers that are loaded into the process on startup.") },
        { unchecked((int)0x80131370), ("CORPROF_E_IRREVERSIBLE_INSTRUMENTATION_PRESENT", "Detach is impossible because the profiler has either instrumented IL or inserted enter/leave hooks. Detach was not attempted; the profiler is still fully attached.") },
        { unchecked((int)0x80131371), ("CORPROF_E_RUNTIME_UNINITIALIZED", "The profiler called a function that cannot complete because the CLR is not yet fully initialized.  The profiler may try again once the CLR has fully started.") },
        { unchecked((int)0x80131372), ("CORPROF_E_IMMUTABLE_FLAGS_SET", "Detach is impossible because immutable flags were set by the profiler at startup. Detach was not attempted; the profiler is still fully attached.") },
        { unchecked((int)0x80131373), ("CORPROF_E_PROFILER_NOT_YET_INITIALIZED", "The profiler called a function that cannot complete because the profiler is not yet fully initialized.") },
        { unchecked((int)0x80131374), ("CORPROF_E_INCONSISTENT_WITH_FLAGS", "The profiler called a function that first requires additional flags to be set in the event mask.  This HRESULT may also indicate that the profiler called a function that first requires that some of the flags currently set in the event mask be reset.") },
        { CORPROF_E_PROFILER_CANCEL_ACTIVATION, ("CORPROF_E_PROFILER_CANCEL_ACTIVATION", "The profiler has requested that the CLR instance not load the profiler into this process.") },
        { unchecked((int)0x80131376), ("CORPROF_E_CONCURRENT_GC_NOT_PROFILABLE", "Concurrent GC mode is enabled, which prevents use of COR_PRF_MONITOR_GC") },
        { unchecked((int)0x80131378), ("CORPROF_E_DEBUGGING_DISABLED", "This functionality requires CoreCLR debugging to be enabled.") },
        { unchecked((int)0x80131379), ("CORPROF_E_TIMEOUT_WAITING_FOR_CONCURRENT_GC", "Timed out on waiting for concurrent GC to finish during attach.") },
        { unchecked((int)0x8013137a), ("CORPROF_E_MODULE_IS_DYNAMIC", "The specified module was dynamically generated (e.g., via Reflection.Emit API), and is thus not supported by this API method.") },
        { unchecked((int)0x8013137b), ("CORPROF_E_CALLBACK4_REQUIRED", "Profiler must implement ICorProfilerCallback4 interface for this call to be supported.") },
        { unchecked((int)0x8013137c), ("CORPROF_E_REJIT_NOT_ENABLED", "This call is not supported unless ReJIT is first enabled during initialization by setting COR_PRF_ENABLE_REJIT via SetEventMask.") },
        { unchecked((int)0x8013137e), ("CORPROF_E_FUNCTION_IS_COLLECTIBLE", "The specified function is instantiated into a collectible assembly, and is thus not supported by this API method.") },
        { unchecked((int)0x80131380), ("CORPROF_E_CALLBACK6_REQUIRED", "Profiler must implement ICorProfilerCallback6 interface for this call to be supported.") },
        { unchecked((int)0x80131382), ("CORPROF_E_CALLBACK7_REQUIRED", "Profiler must implement ICorProfilerCallback7 interface for this call to be supported.") },
        { unchecked((int)0x80131383), ("CORPROF_E_REJIT_INLINING_DISABLED", "The runtime's tracking of inlined methods for ReJIT is not enabled.") },
        { unchecked((int)0x80131388), ("CORPROF_E_SUSPENSION_IN_PROGRESS", "The runtime cannot be suspened since a suspension is already in progress.") },
        { unchecked((int)0x80131389), ("CORPROF_E_NOT_GC_OBJECT", "This object belongs to a non-gc heap.") },
        { unchecked((int)0x8013138a), ("CORPROF_E_MODULE_IS_ENC", "The module is EnC") }
    };

    public bool IsOK => Code == S_OK;

    public int Code { get; }

    public HResult(int hr) => Code = hr;

    public static implicit operator HResult(int hr) => new(hr);

    /// <summary>
    /// Helper to convert to int for comparisons.
    /// </summary>
    public static implicit operator int(HResult hr) => hr.Code;

    /// <summary>
    /// This makes "if (hr)" equivalent to SUCCEEDED(hr).
    /// </summary>
    public static implicit operator bool(HResult hr) => hr.Code >= 0;

    public static bool operator ==(HResult left, HResult right) => left.Equals(right);

    public static bool operator !=(HResult left, HResult right) => !left.Equals(right);

    public static string ToString(int code)
    {
        if (KnownErrorCodes.TryGetValue(code, out var errorCode))
        {
            return errorCode.description == null ? errorCode.name : $"{errorCode.name} ({errorCode.description})";
        }

        return code.ToString("x8");
    }

    public override string ToString() => ToString(Code);

    public void ThrowIfFailed()
    {
        if (Code < 0)
        {
            throw new Win32Exception(this);
        }
    }

    public override bool Equals(object obj)
    {
        return obj is HResult other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Code;
    }

    public bool Equals(HResult other)
    {
        return Code == other.Code;
    }
}

public readonly struct HResult<T> : IEquatable<HResult<T>>
{
    public HResult(HResult error, T result)
    {
        Error = error;
        Result = result;
    }

    public HResult Error { get; }

    public T Result { get; }

    public static implicit operator HResult<T>(T t) => new(HResult.S_OK, t);

    public static implicit operator HResult<T>(HResult error) => new(error, default);

    public static bool operator ==(HResult<T> left, HResult<T> right) => left.Equals(right);

    public static bool operator !=(HResult<T> left, HResult<T> right) => !left.Equals(right);

    public T ThrowIfFailed()
    {
        if (Error.Code < 0)
        {
            throw new Win32Exception(Error);
        }

        return Result;
    }

    public void Deconstruct(out T result)
    {
        result = Result;
    }

    public void Deconstruct(out HResult error, out T result)
    {
        error = Error;
        result = Result;
    }

    public bool Equals(HResult<T> other)
    {
        return Error.Equals(other.Error) && EqualityComparer<T>.Default.Equals(Result, other.Result);
    }

    public override bool Equals(object obj)
    {
        return obj is HResult<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Error, Result);
    }
}
