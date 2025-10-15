namespace Silhouette.Interfaces;

[NativeObject]
internal unsafe interface ICorProfilerInfo14 : ICorProfilerInfo13
{
    public new static readonly Guid Guid = new("F460E352-D76D-4FE9-835F-F6AF9D6E862D");

    HResult EnumerateNonGCObjects(out IntPtr pEnum);

    HResult GetNonGCHeapBounds(
        uint cObjectRanges,
        out uint pcObjectRanges,
        COR_PRF_NONGC_HEAP_RANGE* ranges);


    // EventPipeCreateProvider2 allows you to pass in a callback which will be called whenever a
    // session enables your provider. The behavior of the callback matches the ETW behavior which
    // can be counter intuitive. You will get a callback any time a session changes with the updated
    // global keywords enabled for your session. The is_enabled parameter will be true if any
    // session has your provider enabled. The source_id parameter will be a valid id if the callback
    // was triggered due to a session enabling and it will be NULL if it was triggered due to a session
    // disabling.
    //
    // Example:
    //      Session A enables your provider: callback with is_enabled == true, session_id == A,  and keywords == Session A
    //      Session B enables your provider: callback with is_enabled == true, session_id == B, and keywords == Session A | Session B
    //      Session B disables your provider: callback with is_enabled == true, session_id == NULL, and keywords == Session A
    //      Session A disables your provider: callback with is_enabled == false, session_id == NULL, and keywords == 0
    HResult EventPipeCreateProvider2(
        char* providerName,
        IntPtr pCallback,
        out EVENTPIPE_PROVIDER pProvider);
}