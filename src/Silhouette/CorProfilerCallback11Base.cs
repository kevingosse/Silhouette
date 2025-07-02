using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallback11Base : CorProfilerCallback10Base, ICorProfilerCallback11
{
    private readonly NativeObjects.ICorProfilerCallback11 _corProfilerCallback11;

    protected CorProfilerCallback11Base()
    {
        _corProfilerCallback11 = NativeObjects.ICorProfilerCallback11.Wrap(this);
    }

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == ICorProfilerCallback11.Guid)
        {
            ptr = _corProfilerCallback11;
            return HResult.S_OK;
        }

        return base.QueryInterface(guid, out ptr);
    }

    #region ICorProfilerCallback11

    HResult ICorProfilerCallback11.LoadAsNotificationOnly(out int pbNotificationOnly)
    {
        var result = LoadAsNotificationOnly(out var notificationOnly);

        pbNotificationOnly = notificationOnly ? 1 : 0;

        return result;
    }

    #endregion

    protected virtual HResult LoadAsNotificationOnly(out bool pbNotificationOnly)
    {
        pbNotificationOnly = false;
        return HResult.E_NOTIMPL;
    }
}