using System.Runtime.InteropServices;

namespace TestApp;

internal class PInvokes
{
#if WINDOWS
    private const string DLL_NAME = "ManagedDotnetProfiler.dll";
#else
    private const string DLL_NAME = "ManagedDotnetProfiler";
#endif

    public class CurrentOS
    {
#if WINDOWS
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        public static string GetNativeName() => nameof(GetCurrentThreadId);
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

    [DllImport(DLL_NAME)]
    public static extern unsafe int FetchLastLog(char* buffer, int bufferSize);

    [DllImport(DLL_NAME)]
    public static extern bool GetCurrentThreadInfo(out ulong threadId, out uint osId);

    [DllImport(DLL_NAME)]
    public static extern unsafe bool GetThreads(uint* array, int length, int* actualLength);

    [DllImport(DLL_NAME)]
    public static extern unsafe int GetModuleNames(char* buffer, int length);

    [DllImport(DLL_NAME)]
    public static extern unsafe int CountFrozenObjects();

    [DllImport(DLL_NAME)]
    public static extern unsafe bool EnumJittedFunctions(int version);
}