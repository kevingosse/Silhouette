namespace Silhouette;

public class ICorProfilerInfo5 : ICorProfilerInfo4, ICorProfilerInfoFactory<ICorProfilerInfo5>
{
    private readonly NativeObjects.ICorProfilerInfo5Invoker _impl;

    public ICorProfilerInfo5(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo5 ICorProfilerInfoFactory<ICorProfilerInfo5>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo5>.Guid => Interfaces.ICorProfilerInfo5.Guid;

    public HResult<EventMask2> GetEventMask2()
    {
        var result = _impl.GetEventMask2(out var eventsLow, out var eventsHigh);
        return new(result, new(eventsLow, eventsHigh));
    }

    public HResult SetEventMask2(COR_PRF_MONITOR dwEventsLow, COR_PRF_HIGH_MONITOR dwEventsHigh)
    {
        return _impl.SetEventMask2(dwEventsLow, dwEventsHigh);
    }
}