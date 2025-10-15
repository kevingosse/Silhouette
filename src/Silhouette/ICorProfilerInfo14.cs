namespace Silhouette;

public unsafe class ICorProfilerInfo14 : ICorProfilerInfo13, ICorProfilerInfoFactory<ICorProfilerInfo14>
{
    private readonly NativeObjects.ICorProfilerInfo14Invoker _impl;

    public ICorProfilerInfo14(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo14 ICorProfilerInfoFactory<ICorProfilerInfo14>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo14>.Guid => Interfaces.ICorProfilerInfo14.Guid;

    public HResult<INativeEnumerator<ObjectId>> EnumerateNonGCObjects()
    {
        var result = _impl.EnumerateNonGCObjects(out var pEnum);
        return new(result, new(pEnum));
    }

    public HResult GetNonGCHeapBounds(Span<COR_PRF_NONGC_HEAP_RANGE> ranges, out uint nbObjectRanges)
    {
        fixed (COR_PRF_NONGC_HEAP_RANGE* pRanges = ranges)
        {
            return _impl.GetNonGCHeapBounds((uint)ranges.Length, out nbObjectRanges, pRanges);
        }
    }

    /// <summary>
    /// EventPipeCreateProvider2 allows you to pass in a callback which will be called whenever a
    /// session enables your provider. The behavior of the callback matches the ETW behavior which
    /// can be counter intuitive. You will get a callback any time a session changes with the updated
    /// global keywords enabled for your session. The is_enabled parameter will be true if any
    /// session has your provider enabled. The source_id parameter will be a valid id if the callback
    /// was triggered due to a session enabling and it will be NULL if it was triggered due to a session
    /// disabling.
    ///
    /// Example:
    ///      Session A enables your provider: callback with is_enabled == true, session_id == A,  and keywords == Session A
    ///      Session B enables your provider: callback with is_enabled == true, session_id == B, and keywords == Session A | Session B
    ///      Session B disables your provider: callback with is_enabled == true, session_id == NULL, and keywords == Session A
    ///      Session A disables your provider: callback with is_enabled == false, session_id == NULL, and keywords == 0    /// </summary>
    public HResult<EVENTPIPE_PROVIDER> EventPipeCreateProvider2(string providerName, IntPtr pCallback)
    {
        fixed (char* pProviderName = providerName)
        {
            var result = _impl.EventPipeCreateProvider2(pProviderName, pCallback, out var provider);
            return new(result, provider);
        }
    }
}