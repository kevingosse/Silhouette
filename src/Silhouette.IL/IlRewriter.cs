using dnlib.DotNet.Emit;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;

public sealed class IlRewriter : IDisposable
{
    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private readonly ICorProfilerFunctionControl _functionControl;
    private ModuleId _moduleId;
    private MdMethodDef _methodDefToken;
    private InstructionOperandResolver _instructionOperandResolver;

    private IlRewriter(ICorProfilerInfo3 corProfilerInfo, ICorProfilerFunctionControl functionControl = null)
    {
        _corProfilerInfo = corProfilerInfo;
        _functionControl = functionControl;
    }

    public CilBody Body { get; private set; }

    public static IlRewriter Create(ICorProfilerInfo3 corProfilerInfo)
    {
        return new IlRewriter(corProfilerInfo);
    }

    public static IlRewriter CreateForReJit(ICorProfilerInfo3 corProfilerInfo3, ICorProfilerFunctionControl functionControl)
    {
        return new IlRewriter(corProfilerInfo3, functionControl);
    }

    public void Import(FunctionId functionId)
    {
        var functionInfo = _corProfilerInfo.GetFunctionInfo(functionId).ThrowIfFailed();
        Import(functionInfo.ModuleId, new(functionInfo.Token));
    }

    public unsafe void Import(ModuleId moduleId, MdMethodDef methodDef)
    {
        var functionBody = _corProfilerInfo.GetILFunctionBody(moduleId, methodDef).ThrowIfFailed();

        _moduleId = moduleId;
        _methodDefToken = methodDef;

        var dataStream = DataStreamFactory.Create((byte*)functionBody.MethodHeader);
        var dataReader = new DataReader(dataStream, 0, uint.MaxValue);

        _instructionOperandResolver = new InstructionOperandResolver(moduleId, _corProfilerInfo);

        var parameters = new List<Parameter>();

        var bodyReader = new MethodBodyReader(_instructionOperandResolver, dataReader, parameters);

        if (!bodyReader.Read())
        {
            throw new InvalidOperationException("Failed to read method body.");
        }

        Body = bodyReader.CreateCilBody();
    }

    public unsafe void Export()
    {
        var writer = new MethodBodyWriter(_instructionOperandResolver, Body);
        writer.Write();

        var bodyBytes = writer.Code;

        if (_functionControl != null)
        {
            _functionControl.SetILFunctionBody(bodyBytes).ThrowIfFailed();
            return;
        }

        var malloc = _corProfilerInfo.GetILFunctionBodyAllocator(_moduleId).ThrowIfFailed();

        var bodyPtr = malloc.Alloc((uint)bodyBytes.Length);
        bodyBytes.AsSpan().CopyTo(new Span<byte>((void*)bodyPtr, bodyBytes.Length));

        _corProfilerInfo.SetILFunctionBody(_moduleId, _methodDefToken, bodyPtr).ThrowIfFailed();
    }

    public void Dispose()
    {
        _instructionOperandResolver?.Dispose();
    }
}
