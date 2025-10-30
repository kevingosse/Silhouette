using Silhouette.Interfaces;
using System.Runtime.InteropServices;

namespace Silhouette;

public class ClassFactory : IClassFactory
{
    private readonly NativeObjects.IClassFactory _classFactory;
    private readonly CorProfilerCallbackBase _corProfilerCallback;

    private GCHandle _handle;
    private int _refCount;

    private ClassFactory(CorProfilerCallbackBase corProfilerCallback)
    {
        _classFactory = NativeObjects.IClassFactory.Wrap(this);
        _corProfilerCallback = corProfilerCallback;
        _handle = GCHandle.Alloc(this);
        _refCount = 1;
    }

    public nint IClassFactory => _classFactory;

    public static IntPtr For(CorProfilerCallbackBase corProfilerCallback)
    {
        return new ClassFactory(corProfilerCallback).IClassFactory;
    }

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
        return Interlocked.Increment(ref _refCount);
    }

    public int Release()
    {
        var newCount = Interlocked.Decrement(ref _refCount);

        if (newCount == 0 && _handle.IsAllocated)
        {
            _handle.Free();
        }

        return newCount;
    }
}
