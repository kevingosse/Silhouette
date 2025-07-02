using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallback3Base : CorProfilerCallback2Base, ICorProfilerCallback3
{
    private readonly NativeObjects.ICorProfilerCallback3 _corProfilerCallback3;

    protected CorProfilerCallback3Base()
    {
        _corProfilerCallback3 = NativeObjects.ICorProfilerCallback3.Wrap(this);
    }

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == ICorProfilerCallback3.Guid)
        {
            ptr = _corProfilerCallback3;
            return HResult.S_OK;
        }

        return base.QueryInterface(guid, out ptr);
    }

    #region ICorProfilerCallback3

    HResult ICorProfilerCallback3.ProfilerAttachComplete()
    {
        return ProfilerAttachComplete();
    }

    HResult ICorProfilerCallback3.ProfilerDetachSucceeded()
    {
        return ProfilerDetachSucceeded();
    }

    HResult ICorProfilerCallback3.InitializeForAttach(nint pCorProfilerInfoUnk, nint pvClientData, uint cbClientData)
    {
        return InitializeForAttach(pCorProfilerInfoUnk, pvClientData, cbClientData);
    }

    #endregion

    protected virtual HResult InitializeForAttach(nint pCorProfilerInfoUnk, nint pvClientData, uint cbClientData)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ProfilerAttachComplete()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ProfilerDetachSucceeded()
    {
        return HResult.E_NOTIMPL;
    }
}