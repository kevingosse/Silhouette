namespace Silhouette;

public class ICorProfilerInfo9 : ICorProfilerInfo8, ICorProfilerInfoFactory<ICorProfilerInfo9>
{
    private readonly NativeObjects.ICorProfilerInfo9Invoker _impl;

    public ICorProfilerInfo9(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo9 ICorProfilerInfoFactory<ICorProfilerInfo9>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo9>.Guid => Interfaces.ICorProfilerInfo9.Guid;

    public unsafe HResult GetNativeCodeStartAddresses(FunctionId functionID, ReJITId reJitId, Span<uint> codeStartAddresses, out uint nbCodeStartAddresses)
    {
        fixed (uint* pCodeStartAddresses = codeStartAddresses)
        {
            return _impl.GetNativeCodeStartAddresses(functionID, reJitId, (uint)codeStartAddresses.Length, out nbCodeStartAddresses, pCodeStartAddresses);
        }
    }

    public unsafe HResult GetILToNativeMapping3(nint pNativeCodeStartAddress, Span<COR_DEBUG_IL_TO_NATIVE_MAP> map, out uint mapLength)
    {
        fixed (COR_DEBUG_IL_TO_NATIVE_MAP* pMap = map)
        {
            return _impl.GetILToNativeMapping3(pNativeCodeStartAddress, (uint)map.Length, out mapLength, pMap);
        }
    }

    public unsafe HResult GetCodeInfo4(nint nativeCodeStartAddress, Span<COR_PRF_CODE_INFO> codeInfos, out uint nbCodeInfos)
    {
        fixed (COR_PRF_CODE_INFO* pCodeInfos = codeInfos)
        {
            return _impl.GetCodeInfo4(nativeCodeStartAddress, (uint)codeInfos.Length, out nbCodeInfos, pCodeInfos);
        }
    }
}