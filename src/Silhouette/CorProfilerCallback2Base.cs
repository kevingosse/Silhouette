using Silhouette.Interfaces;

namespace Silhouette
{
    public abstract class CorProfilerCallback2Base : CorProfilerCallbackBase, ICorProfilerCallback2
    {
        private readonly NativeObjects.ICorProfilerCallback2 _corProfilerCallback2;

        protected CorProfilerCallback2Base()
        {
            _corProfilerCallback2 = NativeObjects.ICorProfilerCallback2.Wrap(this);
        }

        protected override HResult QueryInterface(in Guid guid, out nint ptr)
        {
            if (guid == ICorProfilerCallback2.Guid)
            {
                ptr = _corProfilerCallback2;
                return HResult.S_OK;
            }

            return base.QueryInterface(guid, out ptr);
        }

        #region ICorProfilerCallback2

        unsafe HResult ICorProfilerCallback2.GarbageCollectionStarted(int cGenerations, int* generationCollected, COR_PRF_GC_REASON reason)
        {
            Span<bool> generationCollectedAsBool = stackalloc bool[cGenerations];

            for (int i = 0; i < cGenerations; i++)
            {
                generationCollectedAsBool[i] = generationCollected[i] != 0;
            }

            return GarbageCollectionStarted(generationCollectedAsBool, reason);
        }

        unsafe HResult ICorProfilerCallback2.SurvivingReferences(uint cSurvivingObjectIDRanges, ObjectId* objectIDRangeStart, uint* cObjectIDRangeLength)
        {
            return SurvivingReferences(cSurvivingObjectIDRanges, objectIDRangeStart, cObjectIDRangeLength);
        }

        HResult ICorProfilerCallback2.GarbageCollectionFinished()
        {
            return GarbageCollectionFinished();
        }

        HResult ICorProfilerCallback2.FinalizeableObjectQueued(COR_PRF_FINALIZER_FLAGS finalizerFlags, ObjectId objectID)
        {
            return FinalizeableObjectQueued(finalizerFlags, objectID);
        }

        unsafe HResult ICorProfilerCallback2.RootReferences2(uint cRootRefs, ObjectId* rootRefIds, COR_PRF_GC_ROOT_KIND* rootKinds, COR_PRF_GC_ROOT_FLAGS* rootFlags, uint* rootIds)
        {
            return RootReferences2(cRootRefs, rootRefIds, rootKinds, rootFlags, rootIds);
        }

        HResult ICorProfilerCallback2.HandleCreated(GCHandleId handleId, ObjectId initialObjectId)
        {
            return HandleCreated(handleId, initialObjectId);
        }

        HResult ICorProfilerCallback2.HandleDestroyed(GCHandleId handleId)
        {
            return HandleDestroyed(handleId);
        }

        unsafe HResult ICorProfilerCallback2.ThreadNameChanged(ThreadId threadId, uint cchName, char* name)
        {
            return ThreadNameChanged(threadId, cchName, name);
        }

        #endregion

        protected virtual unsafe HResult ThreadNameChanged(ThreadId threadId, uint cchName, char* name)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual HResult GarbageCollectionStarted(Span<bool> generationCollected, COR_PRF_GC_REASON reason)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual unsafe HResult SurvivingReferences(uint cSurvivingObjectIDRanges, ObjectId* objectIDRangeStart, uint* cObjectIDRangeLength)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual HResult GarbageCollectionFinished()
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual HResult FinalizeableObjectQueued(COR_PRF_FINALIZER_FLAGS finalizerFlags, ObjectId objectID)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual unsafe HResult RootReferences2(uint cRootRefs, ObjectId* rootRefIds, COR_PRF_GC_ROOT_KIND* rootKinds, COR_PRF_GC_ROOT_FLAGS* rootFlags, uint* rootIds)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual HResult HandleCreated(GCHandleId handleId, ObjectId initialObjectId)
        {
            return HResult.E_NOTIMPL;
        }

        protected virtual HResult HandleDestroyed(GCHandleId handleId)
        {
            return HResult.E_NOTIMPL;
        }
    }
}
