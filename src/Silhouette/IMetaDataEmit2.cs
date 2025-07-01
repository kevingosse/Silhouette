using NativeObjects;

namespace Silhouette;

public unsafe class IMetaDataEmit2 : IMetaDataEmit
{
    private IMetaDataEmit2Invoker _impl;

    public IMetaDataEmit2(nint ptr)
        : base(ptr)
    {
        _impl = new(ptr);
    }

    public HResult<MdMethodSpec> DefineMethodSpec(MdToken tkParent, ReadOnlySpan<byte> sigBlob)
    {
        fixed (byte* pvSigBlob = sigBlob)
        {
            var result = _impl.DefineMethodSpec(tkParent, (IntPtr)pvSigBlob, (uint)sigBlob.Length, out var pmi);
            return new(result, pmi);
        }
    }

    public HResult<uint> GetDeltaSaveSize(CorSaveSize save)
    {
        var result = _impl.GetDeltaSaveSize(save, out var saveSize);
        return new(result, saveSize);
    }

    public HResult SaveDelta(string file, uint saveFlags)
    {
        fixed (char* szFile = file)
        {
            return _impl.SaveDelta(szFile, saveFlags);
        }
    }

    public HResult SaveDeltaToStream(IntPtr pIStream, uint saveFlags)
    {
        return _impl.SaveDeltaToStream(pIStream, saveFlags);
    }

    public HResult SaveDeltaToMemory(Span<byte> data)
    {
        fixed (byte* pbData = data)
        {
            return _impl.SaveDeltaToMemory((IntPtr)pbData, (uint)data.Length);
        }
    }

    public HResult<MdGenericParam> DefineGenericParam(MdToken tk, uint paramSeq, uint paramFlags, string name, uint reserved, ReadOnlySpan<MdToken> constraints)
    {
        fixed (char* szName = name)
        fixed (MdToken* rtkConstraints = constraints)
        {
            var result = _impl.DefineGenericParam(tk, paramSeq, paramFlags, szName, reserved, rtkConstraints, out var pgp);
            return new(result, pgp);
        }
    }

    public HResult SetGenericParamProps(MdGenericParam gp, uint paramFlags, string name, uint reserved, ReadOnlySpan<MdToken> constraints)
    {
        fixed (char* szName = name)
        fixed (MdToken* rtkConstraints = constraints)
        {
            return _impl.SetGenericParamProps(gp, paramFlags, szName, reserved, rtkConstraints);
        }
    }

    public HResult ResetENCLog()
    {
        return _impl.ResetENCLog();
    }
}
