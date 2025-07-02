using System.Runtime.InteropServices;

namespace Silhouette;

public readonly struct INativeEnumerator<T> : IDisposable
    where T : unmanaged
{
    private readonly NativeObjects.INativeEnumeratorInvoker _impl;

    public INativeEnumerator(nint ptr)
    {
        _impl = new(ptr);
    }

    public void Dispose()
    {
        _impl.Release();
    }

    public HResult Skip(uint count) => _impl.Skip(count);

    public HResult Reset() => _impl.Reset();

    public HResult<INativeEnumerator<T>> Clone()
    {
        var result = _impl.Clone(out var clone);
        return new(result, new(clone));
    }

    public HResult<uint> GetCount()
    {
        var result = _impl.GetCount(out var count);
        return new(result, count);
    }

    public unsafe HResult<uint> Next(Span<T> items)
    {
        fixed (void* pItems = items)
        {
            var result = _impl.Next((uint)items.Length, pItems, out var fetched);
            return new(result, fetched);
        }
    }

    public IEnumerable<T> AsEnumerable()
    {
        T buffer = default;

        while (true)
        {
            var result = GetNextItem(ref buffer);

            if (result == HResult.S_OK)
            {
                yield return buffer;
            }
            else
            {
                break;
            }
        }
    }

    public BufferedNativeEnumerable<T> AsEnumerable(Span<T> buffer)
    {
        return new BufferedNativeEnumerable<T>(this, buffer);
    }

    private HResult GetNextItem(ref T buffer)
    {
        return Next(MemoryMarshal.CreateSpan(ref buffer, 1)).Error;
    }
}
