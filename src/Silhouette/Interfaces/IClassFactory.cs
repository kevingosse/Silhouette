namespace Silhouette.Interfaces;

[NativeObject]
public interface IClassFactory : IUnknown
{
    public static readonly Guid Guid = new("00000001-0000-0000-C000-000000000046");

    HResult CreateInstance(nint outer, in Guid guid, out nint instance);

    HResult LockServer(bool @lock);

}