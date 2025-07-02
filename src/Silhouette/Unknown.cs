using Silhouette.Interfaces;

namespace Silhouette;

public abstract class Unknown : IUnknown
{
    private int _referenceCount;

    protected abstract HResult QueryInterface(in Guid guid, out nint ptr);

    HResult IUnknown.QueryInterface(in Guid guid, out nint ptr) => QueryInterface(guid, out ptr);

    int IUnknown.AddRef()
    {
        return Interlocked.Increment(ref _referenceCount);
    }

    int IUnknown.Release()
    {
        var value = Interlocked.Decrement(ref _referenceCount);

        if (value == 0)
        {
            Dispose();
        }

        return value;
    }

    public virtual void Dispose()
    {
    }
}