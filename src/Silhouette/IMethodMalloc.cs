using NativeObjects;

namespace Silhouette;
public class IMethodMalloc : Interfaces.IUnknown
{
    private readonly IMethodMallocInvoker _impl;

    public IMethodMalloc(nint ptr)
    {
        _impl = new(ptr);
    }

    public HResult QueryInterface(in Guid guid, out nint ptr)
    {
        return _impl.QueryInterface(in guid, out ptr);
    }

    public int AddRef()
    {
        return _impl.AddRef();
    }

    public int Release()
    {
        return _impl.Release();
    }

    public IntPtr Alloc(uint size)
    {
        return _impl.Alloc(size);        
    }
}
