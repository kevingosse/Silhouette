using System.Runtime.InteropServices;

namespace TestApp;

internal static unsafe class PInvokes
{
#if WINDOWS
    private const string DllName = "ManagedDotnetProfiler.dll";
#else
    private const string DllName = "ManagedDotnetProfiler";
#endif

    public static class CurrentOs
    {
#if WINDOWS
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        public static string GetNativeMethodName() => nameof(GetCurrentThreadId);
#else
        [DllImport("libc")]
        public static extern uint gettid();

        public static uint GetCurrentThreadId()
        {
            return gettid();
        }

        public static string GetNativeMethodName() => nameof(gettid);
#endif
    }

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