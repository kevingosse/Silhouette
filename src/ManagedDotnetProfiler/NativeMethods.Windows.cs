using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ManagedDotnetProfiler;

internal static unsafe partial class NativeMethods
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string GetModulePathWindows(nint address)
    {
        const int flags = 0x4 /* GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS */ | 0x2 /* GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT */;
        if (GetModuleHandleExW(flags, address, out var hModule) == 0)
        {
            return null;
        }

        const int bufferSize = 1024;
        var buffer = stackalloc char[bufferSize];
        var length = GetModuleFileNameW(hModule, buffer, bufferSize);
        return length > 0 ? new string(buffer, 0, length) : null;
    }

    [LibraryImport("kernel32", SetLastError = true)]
    private static partial int GetModuleHandleExW(int dwFlags, IntPtr lpModuleName, out IntPtr phModule);

    [LibraryImport("kernel32", SetLastError = true)]
    private static partial int GetModuleFileNameW(IntPtr hModule, char* lpFilename, int nSize);
}
