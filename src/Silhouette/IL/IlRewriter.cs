using dnlib.DotNet.Emit;
using dnlib.DotNet;
using dnlib.IO;
using dnlib.DotNet.Writer;

namespace Silhouette.IL;
public class IlRewriter : IDisposable
{
    private readonly ICorProfilerInfo3 _corProfilerInfo;
    private ModuleId _moduleId;
    private MdMethodDef _methodDefToken;
    private InstructionOperandResolver _instructionOperandResolver;

    public IlRewriter(ICorProfilerInfo3 corProfilerInfo)
    {
        _corProfilerInfo = corProfilerInfo;
    }

    public CilBody Body { get; private set; }

    public unsafe void Import(FunctionId functionId)
    {
        var functionInfo = _corProfilerInfo.GetFunctionInfo(functionId).ThrowIfFailed();
        var functionBody = _corProfilerInfo.GetILFunctionBody(functionInfo.ModuleId, new(functionInfo.Token)).ThrowIfFailed();

        _moduleId = functionInfo.ModuleId;
        _methodDefToken = new MdMethodDef(functionInfo.Token);

        var dataStream = DataStreamFactory.Create((byte*)functionBody.MethodHeader);
        var dataReader = new DataReader(dataStream, 0, uint.MaxValue);
        
        _instructionOperandResolver = new InstructionOperandResolver(functionInfo.ModuleId, _corProfilerInfo);

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
