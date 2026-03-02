using dnlib.DotNet.Emit;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;

public sealed class IlRewriter
{
    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private readonly ICorProfilerFunctionControl _functionControl;

    private IlRewriter(ICorProfilerInfo3 corProfilerInfo, ICorProfilerFunctionControl functionControl = null)
    {
        _corProfilerInfo = corProfilerInfo;
        _functionControl = functionControl;
    }

    public static IlRewriter Create(ICorProfilerInfo3 corProfilerInfo)
    {
        return new IlRewriter(corProfilerInfo);
    }

    public static IlRewriter CreateForReJit(ICorProfilerInfo3 corProfilerInfo3, ICorProfilerFunctionControl functionControl)
    {
        return new IlRewriter(corProfilerInfo3, functionControl);
    }

    public Method Import(FunctionId functionId)
    {
        var functionInfo = _corProfilerInfo.GetFunctionInfo(functionId).ThrowIfFailed();
        return Import(functionInfo.ModuleId, new(functionInfo.Token));
    }

    public unsafe Method Import(ModuleId moduleId, MdMethodDef methodDef)
    {
        var functionBody = _corProfilerInfo.GetILFunctionBody(moduleId, methodDef).ThrowIfFailed();

        var dataStream = DataStreamFactory.Create((byte*)functionBody.MethodHeader);
        var dataReader = new DataReader(dataStream, 0, uint.MaxValue);

        var metadata = new InstructionOperandResolver(moduleId, _corProfilerInfo);
        var parameters = metadata.ReadParameters(methodDef);
        var bodyReader = new MethodBodyReader(metadata, dataReader, parameters);

        if (!bodyReader.Read())
        {
            throw new InvalidOperationException("Failed to read method body.");
        }

        var body = bodyReader.CreateCilBody();
        return new Method(moduleId, methodDef, metadata, body);
    }

    public unsafe void Export(Method method)
    {
        var writer = new MethodBodyWriter(method.Metadata, method.Body);
        writer.Write();

        var bodyBytes = writer.Code;

        if (_functionControl != null)
        {
            _functionControl.SetILFunctionBody(bodyBytes).ThrowIfFailed();
            return;
        }

        var malloc = _corProfilerInfo.GetILFunctionBodyAllocator(method.ModuleId).ThrowIfFailed();

        var bodyPtr = malloc.Alloc((uint)bodyBytes.Length);
        bodyBytes.AsSpan().CopyTo(new Span<byte>((void*)bodyPtr, bodyBytes.Length));

        _corProfilerInfo.SetILFunctionBody(method.ModuleId, method.MethodDefToken, bodyPtr).ThrowIfFailed();
    }
}
