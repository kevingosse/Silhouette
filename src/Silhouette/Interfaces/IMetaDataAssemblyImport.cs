namespace Silhouette.Interfaces;

[NativeObject]
public unsafe interface IMetaDataAssemblyImport : IUnknown
{
    public static readonly Guid Guid = new("EE62470B-E94B-424e-9B7C-2F00C9249F93");

    HResult GetAssemblyProps(
        MdAssembly mda,
        out nint ppbPublicKey,
        out uint pcbPublicKey,
        out uint pulHashAlgId,
        char* szName,
        uint cchName,
        out uint pchName,
        ASSEMBLYMETADATA* pMetaData,
        out CorAssemblyFlags pdwAssemblyFlags);

    HResult GetAssemblyRefProps(
        MdAssemblyRef mdar,
        out nint ppbPublicKeyOrToken,
        out uint pcbPublicKeyOrToken,
        char* szName,
        uint cchName,
        out uint pchName,
        ASSEMBLYMETADATA* pMetaData,
        out nint ppbHashValue,
        out uint pcbHashValue,
        out CorAssemblyFlags pdwAssemblyRefFlags);

    HResult GetFileProps(
        MdFile mdf,
        char* szName,
        uint cchName,
        out uint pchName,
        out nint ppbHashValue,
        out uint pcbHashValue,
        out CorFileFlags pdwFileFlags);

    HResult GetExportedTypeProps(
        MdExportedType mdct,
        char* szName,
        uint cchName,
        out uint pchName,
        out MdToken ptkImplementation,
        out MdTypeDef ptkTypeDef,
        out CorTypeAttr pdwExportedTypeFlags);

    HResult GetManifestResourceProps(
        MdManifestResource mdmr,
        char* szName,
        uint cchName,
        out uint pchName,
        out MdToken ptkImplementation,
        out uint pdwOffset,
        out CorManifestResourceFlags pdwResourceFlags);

    HResult EnumAssemblyRefs(
        HCORENUM* phEnum,
        MdAssemblyRef* rAssemblyRefs,
        uint cMax,
        out uint pcTokens);

    HResult EnumFiles(
        HCORENUM* phEnum,
        MdFile* rFiles,
        uint cMax,
        out uint pcTokens);

    HResult EnumExportedTypes(
        HCORENUM* phEnum,
        MdExportedType* rExportedTypes,
        uint cMax,
        out uint pcTokens);

    HResult EnumManifestResources(
        HCORENUM* phEnum,
        MdManifestResource* rManifestResources,
        uint cMax,
        out uint pcTokens);

    HResult GetAssemblyFromScope(
        out MdAssembly ptkAssembly);

    HResult FindExportedTypeByName(
        char* szName,
        MdToken mdtExportedType,
        out MdExportedType ptkExportedType);

    HResult FindManifestResourceByName(
        char* szName,
        out MdManifestResource ptkManifestResource);

    void CloseEnum(
        HCORENUM hEnum);

    HResult FindAssembliesByName(
        char* szAppBase,
        char* szPrivateBin,
        char* szAssemblyName,
        nint* ppIUnk,
        uint cMax,
        out uint pcAssemblies);
}
