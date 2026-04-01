using NativeObjects;
using System.Runtime.CompilerServices;

namespace Silhouette;

public unsafe class IMetaDataAssemblyImport : Interfaces.IUnknown
{
    private readonly IMetaDataAssemblyImportInvoker _impl;

    public IMetaDataAssemblyImport(nint ptr)
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

    public HResult<AssemblyPropsWithName> GetAssemblyProps(MdAssembly assembly)
    {
        ASSEMBLYMETADATA metadata = default;
        var (result, _) = GetAssemblyProps(assembly, [], out var length, &metadata);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        Span<char> localeBuffer = stackalloc char[(int)metadata.cbLocale];
        metadata.szLocale = (char*)Unsafe.AsPointer(ref localeBuffer.GetPinnableReference());

        (result, var props) = GetAssemblyProps(assembly, buffer, out _, &metadata);

        if (!result)
        {
            return result;
        }

        var locale = metadata.cbLocale > 1 ? new string(localeBuffer[..(int)(metadata.cbLocale - 1)]) : null;

        return new(result, new(
            buffer.WithoutNullTerminator(),
            new(metadata.usMajorVersion, metadata.usMinorVersion, metadata.usBuildNumber, metadata.usRevisionNumber),
            locale,
            props.PublicKey,
            props.HashAlgId,
            props.AssemblyFlags));
    }

    public HResult<AssemblyProps> GetAssemblyProps(MdAssembly assembly, Span<char> name, out uint nameLength, ASSEMBLYMETADATA* pMetaData)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetAssemblyProps(assembly, out var publicKey, out var publicKeyLength, out var hashAlgId, szName, (uint)name.Length, out nameLength, pMetaData, out var assemblyFlags);
            return new(result, new(new(publicKey, (int)publicKeyLength), hashAlgId, assemblyFlags));
        }
    }

    public HResult<AssemblyRefPropsWithName> GetAssemblyRefProps(MdAssemblyRef assemblyRef)
    {
        ASSEMBLYMETADATA metadata = default;
        var (result, _) = GetAssemblyRefProps(assemblyRef, [], out var length, &metadata);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];

        Span<char> localeBuffer = stackalloc char[(int)metadata.cbLocale];
        metadata.szLocale = (char*)Unsafe.AsPointer(ref localeBuffer.GetPinnableReference());

        (result, var props) = GetAssemblyRefProps(assemblyRef, buffer, out _, &metadata);

        if (!result)
        {
            return result;
        }

        var locale = metadata.cbLocale > 1 ? new string(localeBuffer[..(int)(metadata.cbLocale - 1)]) : null;

        return new(result, new(
            buffer.WithoutNullTerminator(),
            new(metadata.usMajorVersion, metadata.usMinorVersion, metadata.usBuildNumber, metadata.usRevisionNumber),
            locale,
            props.PublicKeyOrToken,
            props.HashValue,
            props.AssemblyRefFlags));
    }

    public HResult<AssemblyRefProps> GetAssemblyRefProps(MdAssemblyRef assemblyRef, Span<char> name, out uint nameLength, ASSEMBLYMETADATA* pMetaData)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetAssemblyRefProps(assemblyRef, out var publicKeyOrToken, out var publicKeyOrTokenLength, szName, (uint)name.Length, out nameLength, pMetaData, out var hashValue, out var hashValueLength, out var assemblyRefFlags);
            return new(result, new(new(publicKeyOrToken, (int)publicKeyOrTokenLength), new(hashValue, (int)hashValueLength), assemblyRefFlags));
        }
    }

    public HResult<FilePropsWithName> GetFileProps(MdFile file)
    {
        var (result, _) = GetFileProps(file, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var props) = GetFileProps(file, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(buffer.WithoutNullTerminator(), props.HashValue, props.FileFlags));
    }

    public HResult<FileProps> GetFileProps(MdFile file, Span<char> name, out uint nameLength)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetFileProps(file, szName, (uint)name.Length, out nameLength, out var hashValue, out var hashValueLength, out var fileFlags);
            return new(result, new(new(hashValue, (int)hashValueLength), fileFlags));
        }
    }

    public HResult<ExportedTypePropsWithName> GetExportedTypeProps(MdExportedType exportedType)
    {
        var (result, _) = GetExportedTypeProps(exportedType, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var props) = GetExportedTypeProps(exportedType, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(buffer.WithoutNullTerminator(), props.Implementation, props.TypeDef, props.ExportedTypeFlags));
    }

    public HResult<ExportedTypeProps> GetExportedTypeProps(MdExportedType exportedType, Span<char> name, out uint nameLength)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetExportedTypeProps(exportedType, szName, (uint)name.Length, out nameLength, out var implementation, out var typeDef, out var exportedTypeFlags);
            return new(result, new(implementation, typeDef, exportedTypeFlags));
        }
    }

    public HResult<ManifestResourcePropsWithName> GetManifestResourceProps(MdManifestResource manifestResource)
    {
        var (result, _) = GetManifestResourceProps(manifestResource, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var props) = GetManifestResourceProps(manifestResource, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(buffer.WithoutNullTerminator(), props.Implementation, props.Offset, props.ResourceFlags));
    }

    public HResult<ManifestResourceProps> GetManifestResourceProps(MdManifestResource manifestResource, Span<char> name, out uint nameLength)
    {
        fixed (char* szName = name)
        {
            var result = _impl.GetManifestResourceProps(manifestResource, szName, (uint)name.Length, out nameLength, out var implementation, out var offset, out var resourceFlags);
            return new(result, new(implementation, offset, resourceFlags));
        }
    }

    public HResult EnumAssemblyRefs(ref HCORENUM hEnum, Span<MdAssemblyRef> assemblyRefs, out uint nbAssemblyRefs)
    {
        fixed (MdAssemblyRef* rAssemblyRefs = assemblyRefs)
        {
            return _impl.EnumAssemblyRefs((HCORENUM*)Unsafe.AsPointer(ref hEnum), rAssemblyRefs, (uint)assemblyRefs.Length, out nbAssemblyRefs);
        }
    }

    public HResult EnumFiles(ref HCORENUM hEnum, Span<MdFile> files, out uint nbFiles)
    {
        fixed (MdFile* rFiles = files)
        {
            return _impl.EnumFiles((HCORENUM*)Unsafe.AsPointer(ref hEnum), rFiles, (uint)files.Length, out nbFiles);
        }
    }

    public HResult EnumExportedTypes(ref HCORENUM hEnum, Span<MdExportedType> exportedTypes, out uint nbExportedTypes)
    {
        fixed (MdExportedType* rExportedTypes = exportedTypes)
        {
            return _impl.EnumExportedTypes((HCORENUM*)Unsafe.AsPointer(ref hEnum), rExportedTypes, (uint)exportedTypes.Length, out nbExportedTypes);
        }
    }

    public HResult EnumManifestResources(ref HCORENUM hEnum, Span<MdManifestResource> manifestResources, out uint nbManifestResources)
    {
        fixed (MdManifestResource* rManifestResources = manifestResources)
        {
            return _impl.EnumManifestResources((HCORENUM*)Unsafe.AsPointer(ref hEnum), rManifestResources, (uint)manifestResources.Length, out nbManifestResources);
        }
    }

    public HResult<MdAssembly> GetAssemblyFromScope()
    {
        var result = _impl.GetAssemblyFromScope(out var assembly);
        return new(result, assembly);
    }

    public HResult<MdExportedType> FindExportedTypeByName(string name, MdToken enclosingType)
    {
        fixed (char* szName = name)
        {
            var result = _impl.FindExportedTypeByName(szName, enclosingType, out var token);
            return new(result, token);
        }
    }

    public HResult<MdManifestResource> FindManifestResourceByName(string name)
    {
        fixed (char* szName = name)
        {
            var result = _impl.FindManifestResourceByName(szName, out var token);
            return new(result, token);
        }
    }

    public void CloseEnum(HCORENUM hEnum)
    {
        _impl.CloseEnum(hEnum);
    }
}
