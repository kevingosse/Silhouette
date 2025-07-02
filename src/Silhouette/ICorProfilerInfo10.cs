namespace Silhouette;

public class ICorProfilerInfo10 : ICorProfilerInfo9, ICorProfilerInfoFactory<ICorProfilerInfo10>
{
    private readonly NativeObjects.ICorProfilerInfo10Invoker _impl;

    public ICorProfilerInfo10(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo10 ICorProfilerInfoFactory<ICorProfilerInfo10>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo10>.Guid => Interfaces.ICorProfilerInfo10.Guid;

    public unsafe HResult EnumerateObjectReferences(ObjectId objectId, delegate* unmanaged<ObjectId, ObjectId*, void*, int> callback, void* clientData)
    {
        return _impl.EnumerateObjectReferences(objectId, callback, clientData);
    }

    public HResult<bool> IsFrozenObject(ObjectId objectId)
    {
        var result = _impl.IsFrozenObject(objectId, out var frozen);
        return new(result, frozen != 0);
    }

    public HResult<uint> GetLOHObjectSizeThreshold()
    {
        var result = _impl.GetLOHObjectSizeThreshold(out var threshold);
        return new(result, threshold);
    }

    public unsafe HResult RequestReJITWithInliners(COR_PRF_REJIT_FLAGS rejitFlags, ReadOnlySpan<ModuleId> moduleIds, ReadOnlySpan<MdMethodDef> methodIds)
    {
        if (moduleIds.Length != methodIds.Length)
        {
            throw new ArgumentException("moduleIds and methodIds must have the same length.");
        }

        fixed (ModuleId* pModuleIds = moduleIds)
        fixed (MdMethodDef* pMethodIds = methodIds)
        {
            return _impl.RequestReJITWithInliners((uint)rejitFlags, (uint)moduleIds.Length, pModuleIds, pMethodIds);
        }
    }

    public HResult SuspendRuntime()
    {
        return _impl.SuspendRuntime();
    }

    public HResult ResumeRuntime()
    {
        return _impl.ResumeRuntime();
    }
}