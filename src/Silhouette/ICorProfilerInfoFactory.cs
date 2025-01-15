namespace Silhouette;

internal interface ICorProfilerInfoFactory<T>
{
    static abstract T Create(nint ptr);
    static abstract Guid Guid { get; }
}
