using Silhouette.Interfaces;

namespace Silhouette;

public abstract class CorProfilerCallback5Base : CorProfilerCallback4Base, ICorProfilerCallback5
{
    private readonly NativeObjects.ICorProfilerCallback5 _corProfilerCallback5;

    protected CorProfilerCallback5Base()
    {
        _corProfilerCallback5 = NativeObjects.ICorProfilerCallback5.Wrap(this);
    }

    protected override HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == ICorProfilerCallback5.Guid)
        {
            ptr = _corProfilerCallback5;
            return HResult.S_OK;
        }

        return base.QueryInterface(guid, out ptr);
    }

    #region ICorProfilerCallback5
    unsafe HResult ICorProfilerCallback5.ConditionalWeakTableElementReferences(uint cRootRefs, ObjectId* keyRefIds, ObjectId* valueRefIds, GCHandleId* rootIds)
    {
        return ConditionalWeakTableElementReferences(cRootRefs, keyRefIds, valueRefIds, rootIds);
    }

    #endregion

    protected virtual unsafe HResult ConditionalWeakTableElementReferences(uint cRootRefs, ObjectId* keyRefIds, ObjectId* valueRefIds, GCHandleId* rootIds)
    {
        return HResult.E_NOTIMPL;
    }
}