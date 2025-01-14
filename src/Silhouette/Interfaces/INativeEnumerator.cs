namespace Silhouette.Interfaces;

[NativeObject]
internal unsafe interface INativeEnumerator : IUnknown
{
    HResult Skip(uint count);
    HResult Reset();
    HResult Clone(out IntPtr clone);
    HResult GetCount(out uint count);
    HResult Next(uint count, void* items, out uint fetched);
}
