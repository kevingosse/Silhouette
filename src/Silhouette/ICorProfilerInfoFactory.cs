namespace Silhouette;

internal interface ICorProfilerInfoFactory<out T>
{
    static abstract T Create(nint ptr);
    static abstract Guid Guid { get; }
}
