namespace Silhouette.Interfaces;

[NativeObject]
internal unsafe interface ICorProfilerFunctionControl : IUnknown
{
    /*
     * Set one or more flags from COR_PRF_CODEGEN_FLAGS to control code
     * generation just for this method.
     */
    HResult SetCodegenFlags(COR_PRF_CODEGEN_FLAGS flags);

    /*
     * Override the method body.
     */
    HResult SetILFunctionBody(uint cbNewILMethodHeader, IntPtr newILMethodHeader);

    /*
     * This is not currently implemented, and will return E_NOTIMPL
     */
    HResult SetILInstrumentedCodeMap(uint ilMapEntriesCount, COR_IL_MAP* ilMapEntries);
}
