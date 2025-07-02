namespace Silhouette;

public class ICorProfilerInfo7 : ICorProfilerInfo6, ICorProfilerInfoFactory<ICorProfilerInfo7>
{
    private readonly NativeObjects.ICorProfilerInfo7Invoker _impl;

    public ICorProfilerInfo7(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo7 ICorProfilerInfoFactory<ICorProfilerInfo7>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo7>.Guid => Interfaces.ICorProfilerInfo7.Guid;

    public HResult ApplyMetaData(ModuleId moduleId)
    {
        return _impl.ApplyMetaData(moduleId);
    }

    public HResult<uint> GetInMemorySymbolsLength(ModuleId moduleId)
    {
        var result = _impl.GetInMemorySymbolsLength(moduleId, out var countSymbolBytes);
        return new(result, countSymbolBytes);
    }

    public unsafe HResult<uint> ReadInMemorySymbols(ModuleId moduleId, int symbolsReadOffset, Span<byte> symbolBytes)
    {
        fixed (byte* pSymbolBytes = symbolBytes)
        {
            var result = _impl.ReadInMemorySymbols(moduleId, symbolsReadOffset, pSymbolBytes, (uint)symbolBytes.Length, out var countSymbolBytesRead);
            return new(result, countSymbolBytesRead);
        }
    }
}