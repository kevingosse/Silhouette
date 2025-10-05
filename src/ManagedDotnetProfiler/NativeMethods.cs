using System;

namespace ManagedDotnetProfiler;

internal unsafe partial class NativeMethods
{
    internal static string GetCurrentModulePath()
    {
        var address = (IntPtr)(delegate*<string>)&GetCurrentModulePath;

        if (OperatingSystem.IsWindows())
        {
            return GetModulePathWindows(address);
        }

        if (OperatingSystem.IsLinux())
        {
            return GetModulePathLinux(address);
        }

        throw new PlatformNotSupportedException();
    }
}
