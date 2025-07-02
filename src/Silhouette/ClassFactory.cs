using Silhouette.Interfaces;

namespace Silhouette;

public class ClassFactory : IClassFactory
{
    private readonly NativeObjects.IClassFactory _classFactory;

    private readonly CorProfilerCallbackBase _corProfilerCallback;

    public ClassFactory(CorProfilerCallbackBase corProfilerCallback)
    {
        _classFactory = NativeObjects.IClassFactory.Wrap(this);
        _corProfilerCallback = corProfilerCallback;
    }

    public nint IClassFactory => _classFactory;

    public HResult CreateInstance(nint outer, in Guid guid, out nint instance)
    {
        instance = _corProfilerCallback.ICorProfilerCallback;
        return HResult.S_OK;
    }

    public HResult LockServer(bool @lock)
    {
        return default;
    }

    public HResult QueryInterface(in Guid guid, out nint ptr)
    {
        if (guid == Silhouette.Interfaces.IClassFactory.Guid)
        {
            ptr = IClassFactory;
            return HResult.S_OK;
        }

        ptr = nint.Zero;
        return HResult.E_NOINTERFACE;
    }

    public int AddRef()
    {
        return 1;
    }

    public int Release()
    {
        return 0;
    }
}
