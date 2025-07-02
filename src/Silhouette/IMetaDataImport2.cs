using NativeObjects;
using System.Runtime.CompilerServices;

namespace Silhouette;

public unsafe class IMetaDataImport2 : IMetaDataImport
{
    private readonly IMetaDataImport2Invoker _impl;

    public IMetaDataImport2(nint ptr)
        : base(ptr)
    {
        _impl = new(ptr);
    }

    public HResult EnumGenericParams(
        ref HCORENUM phEnum,
        MdToken tk,
        Span<MdGenericParam> genericParams,
        out uint nbGenericParams)
    {
        fixed (MdGenericParam* pGenericParams = genericParams)
        {
            return _impl.EnumGenericParams((HCORENUM*)Unsafe.AsPointer(ref phEnum), tk, pGenericParams, (uint)genericParams.Length, out nbGenericParams);
        }
    }

    public HResult<GenericParamProps> GetGenericParamProps(
        MdGenericParam gp,
        Span<char> name,
        out uint nameLength)
    {
        fixed (char* pName = name)
        {
            var result = _impl.GetGenericParamProps(gp, out var paramSeq, out var paramFlags, out var owner, out var reserved, pName, (uint)name.Length, out nameLength);
            return new(result, new(paramSeq, (CorGenericParamAttr)paramFlags, owner, reserved));
        }
    }

    public HResult<GenericParamPropsWithName> GetGenericParamProps(MdGenericParam gp)
    {
        var (result, _) = GetGenericParamProps(gp, [], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        (result, var props) = GetGenericParamProps(gp, buffer, out _);

        if (!result)
        {
            return result;
        }

        return new(result, new(props.ParamSeq, props.ParamFlags, props.Owner, props.Reserved, buffer.WithoutNullTerminator()));
    }

    public HResult<MethodSpecProps> GetMethodSpecProps(MdMethodSpec mi)
    {
        var result = _impl.GetMethodSpecProps(mi, out var parent, out var sigBlob, out var sigBlobSize);            
        return new(result, new(parent, new(sigBlob, (int)sigBlobSize)));
    }

    public HResult EnumGenericParamConstraints(
        ref HCORENUM hEnum,
        MdGenericParam tk,
        Span<MdGenericParamConstraint> genericParamConstraints,
        out uint nbGenericParamConstraints)
    {
        fixed (MdGenericParamConstraint* pGenericParamConstraints = genericParamConstraints)
        {
            return _impl.EnumGenericParamConstraints((HCORENUM*)Unsafe.AsPointer(ref hEnum), tk, pGenericParamConstraints, (uint)genericParamConstraints.Length, out nbGenericParamConstraints);
        }
    }

    public HResult<GenericParamConstraintProps> GetGenericParamConstraintProps(MdGenericParamConstraint gpc)
    {
        var result = _impl.GetGenericParamConstraintProps(gpc, out var genericParam, out var constraintType);
        return new(result, new(genericParam, constraintType));
    }

    public HResult<PEKind> GetPEKind()
    {
        var result = _impl.GetPEKind(out var peKind, out var machine);
        return new(result, new((CorPEKind)peKind, machine));
    }

    public HResult GetVersionString(
        Span<char> buffer,
        out uint length)
    {
        fixed (char* pBuffer = buffer)
        {
            var result = _impl.GetVersionString(pBuffer, (uint)buffer.Length, out length);
            return new(result);
        }
    }

    public HResult<string> GetVersionString()
    {
        var result = GetVersionString([], out var length);

        if (!result)
        {
            return result;
        }

        Span<char> buffer = stackalloc char[(int)length];
        result = GetVersionString(buffer, out _);
            
        if (!result)
        {
            return result;
        }
            
        return new(result, buffer.WithoutNullTerminator());
    }

    public HResult EnumMethodSpecs(
        ref HCORENUM phEnum,
        MdToken tk,
        Span<MdMethodSpec> methodSpecs,
        out uint nbMethodSpecs)
    {
        fixed (MdMethodSpec* pMethodSpecs = methodSpecs)
        {
            return _impl.EnumMethodSpecs((HCORENUM*)Unsafe.AsPointer(ref phEnum), tk, pMethodSpecs, (uint)methodSpecs.Length, out nbMethodSpecs);
        }
    }
}