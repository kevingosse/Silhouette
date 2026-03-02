using dnlib.DotNet.Emit;

namespace Silhouette.IL;

public sealed class Method : IDisposable
{
    internal Method(ModuleId moduleId, MdMethodDef methodDefToken, InstructionOperandResolver metadata, CilBody body)
    {
        ModuleId = moduleId;
        MethodDefToken = methodDefToken;
        Metadata = metadata;
        Body = body;
    }

    public InstructionOperandResolver Metadata { get; }

    public CilBody Body { get; }

    internal ModuleId ModuleId { get; }

    internal MdMethodDef MethodDefToken { get; }

    public void Dispose()
    {
        Metadata?.Dispose();
    }
}
