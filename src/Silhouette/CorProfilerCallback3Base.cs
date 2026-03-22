using Silhouette.Interfaces;

namespace Silhouette;

public abstract unsafe class CorProfilerCallback3Base : CorProfilerCallback2Base, ICorProfilerCallback3
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

    protected virtual HResult InitializeForAttach(int iCorProfilerInfoVersion, ReadOnlySpan<byte> clientData)
    {
        return HResult.E_NOTIMPL;
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
        int version = GetICorProfilerInfo(pCorProfilerInfoUnk);
        ReadOnlySpan<byte> data = pvClientData == 0 ? [] : new((void*)pvClientData, (int)cbClientData);

        return InitializeForAttach(version, data);
    }

    #endregion

    protected virtual HResult ProfilerAttachComplete()
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult ProfilerDetachSucceeded()
    {
        return HResult.E_NOTIMPL;
    }
}