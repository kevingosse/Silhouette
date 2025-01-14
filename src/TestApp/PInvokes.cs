using System.Runtime.InteropServices;

namespace TestApp;

internal class PInvokes
{
    public class Win32
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();
    }

    [DllImport("ManagedDotnetProfiler.dll")]
    public static extern unsafe int FetchLastLog(char* buffer, int bufferSize);

    [DllImport("ManagedDotnetProfiler.dll")]
    public static extern bool GetCurrentThreadInfo(out ulong threadId, out uint osId);

    [DllImport("ManagedDotnetProfiler.dll")]
    public static extern unsafe bool GetThreads(uint* array, int length, int* actualLength);

    [DllImport("ManagedDotnetProfiler.dll")]
    public static extern unsafe int GetModuleNames(char* buffer, int length);
}