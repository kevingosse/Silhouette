using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallback8Base : CorProfilerCallback7Base, ICorProfilerCallback8
{
    private readonly NativeObjects.ICorProfilerCallback8 _corProfilerCallback8;

    protected CorProfilerCallback8Base()
    {
        _corProfilerCallback8 = NativeObjects.ICorProfilerCallback8.Wrap(this);
    }

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == ICorProfilerCallback8.Guid)
        {
            ptr = _corProfilerCallback8;
            return HResult.S_OK;
        }

        return base.QueryInterface(guid, out ptr);
    }

    #region ICorProfilerCallback8

    HResult ICorProfilerCallback8.DynamicMethodJITCompilationFinished(FunctionId functionId, HResult hrStatus, int fIsSafeToBlock)
    {
        return DynamicMethodJITCompilationFinished(functionId, hrStatus, fIsSafeToBlock != 0);
    }

    unsafe HResult ICorProfilerCallback8.DynamicMethodJITCompilationStarted(FunctionId functionId, int fIsSafeToBlock, byte* pILHeader, uint cbILHeader)
    {
        return DynamicMethodJITCompilationStarted(functionId, fIsSafeToBlock != 0, pILHeader, cbILHeader);
    }

    #endregion

    protected virtual unsafe HResult DynamicMethodJITCompilationStarted(FunctionId functionId, bool fIsSafeToBlock, byte* pILHeader, uint cbILHeader)
    {
        return HResult.E_NOTIMPL;
    }

    protected virtual HResult DynamicMethodJITCompilationFinished(FunctionId functionId, HResult hrStatus, bool fIsSafeToBlock)
    {
        return HResult.E_NOTIMPL;
    }
}