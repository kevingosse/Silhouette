namespace Silhouette.Interfaces;

[NativeObject]
public interface IMethodMalloc : IUnknown
{
    IntPtr Alloc(uint size);
}
