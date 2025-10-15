using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace ManagedDotnetProfiler;

[SuppressMessage("ReSharper", "InconsistentNaming")]
internal static unsafe partial class NativeMethods
{
    private static string GetModulePathLinux(nint address)
    {
        var export = NativeLibrary.GetExport(NativeLibrary.GetMainProgramHandle(), "dladdr");
        var dladdr = (delegate* unmanaged[Cdecl]<IntPtr, out DlInfo, int>)export;

        if (dladdr(address, out var info) == 0 || info.dli_fname == null)
        {
            return null;
        }

        return Marshal.PtrToStringUTF8((nint)info.dli_fname);
    }

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private readonly struct DlInfo
    {
        public readonly byte* dli_fname;
        public readonly IntPtr dli_fbase;
        public readonly byte* dli_sname;
        public readonly IntPtr dli_saddr;
    }
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}
