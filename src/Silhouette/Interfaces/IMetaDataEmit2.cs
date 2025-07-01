namespace Silhouette.Interfaces;

[NativeObject]
public unsafe interface IMetaDataEmit2 : IMetaDataEmit
{
    public new static readonly Guid Guid = new("F5DD9950-F693-42e6-830E-7B833E8146A9");

    HResult DefineMethodSpec(
        MdToken tkParent,               // [IN] MethodDef or MemberRef
        IntPtr pvSigBlob,          // [IN] point to a blob value of signature
        uint cbSigBlob,              // [IN] count of bytes in the signature blob
        out MdMethodSpec pmi);            // [OUT] method instantiation token

    HResult GetDeltaSaveSize(            // S_OK or error.
        CorSaveSize fSave,                  // [IN] cssAccurate or cssQuick.
        out uint pdwSaveSize);     // [OUT] Put the size here.

    HResult SaveDelta(                   // S_OK or error.
        char* szFile,                 // [IN] The filename to save to.
        uint dwSaveFlags);      // [IN] Flags for the save.

    HResult SaveDeltaToStream(           // S_OK or error.
        IntPtr pIStream,              // [IN] A writable stream to save to.
        uint dwSaveFlags);      // [IN] Flags for the save.

    HResult SaveDeltaToMemory(           // S_OK or error.
        IntPtr pbData,                // [OUT] Location to write data.
        uint cbData);           // [IN] Max size of data buffer.

    HResult DefineGenericParam(          // S_OK or error.
        MdToken tk,                    // [IN] TypeDef or MethodDef
        uint ulParamSeq,            // [IN] Index of the type parameter
        uint dwParamFlags,          // [IN] Flags, for future use (e.g. variance)
        char* szname,                // [IN] Name
        uint reserved,              // [IN] For future use (e.g. non-type parameters)
        MdToken* rtkConstraints,      // [IN] Array of type constraints (TypeDef,TypeRef,TypeSpec)
        out MdGenericParam pgp);          // [OUT] Put GenericParam token here

    HResult SetGenericParamProps(        // S_OK or error.
        MdGenericParam gp,                  // [IN] GenericParam
        uint dwParamFlags,          // [IN] Flags, for future use (e.g. variance)
        char* szName,                // [IN] Optional name
        uint reserved,              // [IN] For future use (e.g. non-type parameters)
        MdToken* rtkConstraints);// [IN] Array of type constraints (TypeDef,TypeRef,TypeSpec)

    HResult ResetENCLog();          // S_OK or error.
}