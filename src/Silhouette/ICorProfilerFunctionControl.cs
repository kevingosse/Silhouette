using NativeObjects;

namespace Silhouette;

public unsafe class ICorProfilerFunctionControl : Interfaces.IUnknown
{
    private readonly ICorProfilerFunctionControlInvoker _impl;

    public ICorProfilerFunctionControl(nint ptr)
    {
        _impl = new(ptr);
    }

    public HResult QueryInterface(in Guid guid, out nint ptr)
    {
        return _impl.QueryInterface(in guid, out ptr);
    }

    public int AddRef()
    {
        return _impl.AddRef();
    }

    public int Release()
    {
        return _impl.Release();
    }

    public HResult SetCodegenFlags(COR_PRF_CODEGEN_FLAGS flags)
    {
        return _impl.SetCodegenFlags(flags);
    }

    public HResult SetILFunctionBody(uint cbNewILMethodHeader, IntPtr newILMethodHeader)
    {
        return _impl.SetILFunctionBody(cbNewILMethodHeader, newILMethodHeader);
    }

    public HResult SetILFunctionBody(ReadOnlySpan<byte> newILMethodHeader)
    {
        fixed (byte* p = newILMethodHeader)
        {
            return _impl.SetILFunctionBody((uint)newILMethodHeader.Length, (IntPtr)p);
        }
    }

    public HResult SetILInstrumentedCodeMap(uint ilMapEntriesCount, COR_IL_MAP* ilMapEntries)
    {
        return _impl.SetILInstrumentedCodeMap(ilMapEntriesCount, ilMapEntries);
    }

    public HResult SetILInstrumentedCodeMap(ReadOnlySpan<COR_IL_MAP> ilMapEntries)
    {
        fixed (COR_IL_MAP* p = ilMapEntries)
        {
            return _impl.SetILInstrumentedCodeMap((uint)ilMapEntries.Length, p);
        }
    }
}
