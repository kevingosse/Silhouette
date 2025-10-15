using System.Runtime.InteropServices;

namespace TestApp;

internal static class NativeMethods
{
    public static uint GetCurrentThreadId()
    {
        if (OperatingSystem.IsWindows())
        {
            return GetCurrentThreadId_Windows();
        }

        if (OperatingSystem.IsLinux())
        {
            return GetCurrentThreadId_Linux();
        }

        throw new PlatformNotSupportedException("Unsupported OS");
    }

    [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId")]
    private static extern uint GetCurrentThreadId_Windows();

    [DllImport("libc", EntryPoint = "gettid")]
    private static extern uint GetCurrentThreadId_Linux();
}
