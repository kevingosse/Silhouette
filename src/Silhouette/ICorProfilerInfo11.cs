namespace Silhouette;

public class ICorProfilerInfo11 : ICorProfilerInfo10, ICorProfilerInfoFactory<ICorProfilerInfo11>
{
    private readonly NativeObjects.ICorProfilerInfo11Invoker _impl;

    public ICorProfilerInfo11(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo11 ICorProfilerInfoFactory<ICorProfilerInfo11>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo11>.Guid => Interfaces.ICorProfilerInfo11.Guid;

    public unsafe HResult GetEnvironmentVariable(string name, ReadOnlySpan<char> value, out uint valueLength)
    {
        fixed (char* pValue = value)
        fixed (char* pName = name)
        {
            return _impl.GetEnvironmentVariable(pName, (uint)value.Length, out valueLength, pValue);
        }
    }

    public unsafe HResult<string> GetEnvironmentVariable(string name)
    {
        var result = GetEnvironmentVariable(name, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        result = GetEnvironmentVariable(name, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, buffer.WithoutNullTerminator());
    }

    public unsafe HResult SetEnvironmentVariable(string name, string value)
    {
        fixed (char* pName = name)
        fixed (char* pValue = value)
        {
            return _impl.SetEnvironmentVariable(pName, pValue);
        }
    }
}