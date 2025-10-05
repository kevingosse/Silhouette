using System.Runtime.InteropServices;

namespace TestApp;

/// <summary>
/// The p/invokes in this class are automatically rewritten by the profiler.
/// </summary>
internal static unsafe class ProfilerPInvokes
{
    private const string DllName = "ManagedDotnetProfiler";

    [DllImport(DllName)]
    public static extern int FetchLastLog(char* buffer, int bufferSize);

    [DllImport(DllName)]
    public static extern bool GetCurrentThreadInfo(out ulong threadId, out uint osId);

    [DllImport(DllName)]
    public static extern bool GetThreads(uint* array, int length, int* actualLength);

    [DllImport(DllName)]
    public static extern int GetModuleNames(char* buffer, int length);

    [DllImport(DllName)]
    public static extern int CountFrozenObjects();

    [DllImport(DllName)]
    public static extern bool EnumJittedFunctions(int version);

    [DllImport(DllName)]
    public static extern int GetGenericArguments(nint typeHandle, int methodToken, char* buffer, int size);

    [DllImport(DllName)]
    public static extern bool RequestReJit(IntPtr module, int methodDef);

    [DllImport(DllName)]
    public static extern bool RequestRevert(IntPtr module, int methodDef);
}