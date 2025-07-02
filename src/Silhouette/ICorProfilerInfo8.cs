namespace Silhouette;

public class ICorProfilerInfo8 : ICorProfilerInfo7, ICorProfilerInfoFactory<ICorProfilerInfo8>
{
    private readonly NativeObjects.ICorProfilerInfo8Invoker _impl;

    public ICorProfilerInfo8(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo8 ICorProfilerInfoFactory<ICorProfilerInfo8>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo8>.Guid => Interfaces.ICorProfilerInfo8.Guid;

    public HResult<bool> IsFunctionDynamic(FunctionId functionId)
    {
        var result = _impl.IsFunctionDynamic(functionId, out var isDynamic);
        return new(result, isDynamic != 0);
    }

    public HResult<FunctionFromIP> GetFunctionFromIP3(nint ip)
    {
        var result = _impl.GetFunctionFromIP3(ip, out var functionId, out var reJitId);
        return new(result, new(functionId, reJitId));
    }

    public unsafe HResult<DynamicFunctionInfo> GetDynamicFunctionInfo(FunctionId functionId, Span<char> name, out uint nameLength)
    {
        fixed (char* pName = name)
        {
            var result = _impl.GetDynamicFunctionInfo(functionId, out var moduleId, out var sig, out var pbSig, (uint)name.Length, out nameLength, pName);
            return new(result, new(moduleId, new(sig, (int)pbSig)));
        }
    }

    public unsafe HResult<DynamicFunctionInfoWithName> GetDynamicFunctionInfo(FunctionId functionId)
    {
        var (result, _) = GetDynamicFunctionInfo(functionId, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        (result, var functionInfo) = GetDynamicFunctionInfo(functionId, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(functionInfo.ModuleId, functionInfo.Signature, buffer.WithoutNullTerminator()));
    }
}