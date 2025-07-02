namespace Silhouette;

public class ICorProfilerInfo6 : ICorProfilerInfo5, ICorProfilerInfoFactory<ICorProfilerInfo6>
{
    private readonly NativeObjects.ICorProfilerInfo6Invoker _impl;

    public ICorProfilerInfo6(nint ptr) : base(ptr)
    {
        _impl = new(ptr);
    }

    static ICorProfilerInfo6 ICorProfilerInfoFactory<ICorProfilerInfo6>.Create(nint ptr) => new(ptr);
    static Guid ICorProfilerInfoFactory<ICorProfilerInfo6>.Guid => Interfaces.ICorProfilerInfo6.Guid;

    public HResult<NgenModuleMethodsInliningThisMethod> EnumNgenModuleMethodsInliningThisMethod(ModuleId inlinersModuleId, ModuleId inlineeModuleId, MdMethodDef inlineeMethodId)
    {
        var result = _impl.EnumNgenModuleMethodsInliningThisMethod(inlinersModuleId, inlineeModuleId, inlineeMethodId, out var incompleteData, out var pEnum);
        return new(result, new(new(pEnum), incompleteData != 0));
    }
}