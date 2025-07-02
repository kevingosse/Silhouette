using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallback10Base : CorProfilerCallback9Base, ICorProfilerCallback10
{
    private readonly NativeObjects.ICorProfilerCallback10 _corProfilerCallback10;

    protected CorProfilerCallback10Base()
    {
        _corProfilerCallback10 = NativeObjects.ICorProfilerCallback10.Wrap(this);
    }

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == ICorProfilerCallback10.Guid)
        {
            ptr = _corProfilerCallback10;
            return HResult.S_OK;
        }

        return base.QueryInterface(guid, out ptr);
    }

    #region ICorProfilerCallback10

    HResult ICorProfilerCallback10.EventPipeProviderCreated(nint provider)
    {
        return EventPipeProviderCreated(provider);
    }

    unsafe HResult ICorProfilerCallback10.EventPipeEventDelivered(nint provider, int eventId, int eventVersion, uint cbMetadataBlob, byte* metadataBlob, uint cbEventData, byte* eventData, in Guid pActivityId, in Guid pRelatedActivityId, ThreadId eventThread, uint numStackFrames, nint* stackFrames)
    {
        return EventPipeEventDelivered(provider, eventId, eventVersion, cbMetadataBlob, metadataBlob, cbEventData, eventData, in pActivityId, in pRelatedActivityId, eventThread, numStackFrames, stackFrames);
    }

    #endregion

    protected virtual unsafe HResult EventPipeEventDelivered(nint provider, int eventId, int eventVersion, uint cbMetadataBlob, byte* metadataBlob, uint cbEventData, byte* eventData, in Guid pActivityId, in Guid pRelatedActivityId, ThreadId eventThread, uint numStackFrames, nint* stackFrames)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult EventPipeProviderCreated(nint provider)
    {
        return HResult.E_NOTIMPL;
    }
}