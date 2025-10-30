using System;
using System.Runtime.InteropServices;
using Silhouette;

namespace ManagedDotnetProfiler;

public static class DllMain
{
    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    public static unsafe HResult DllGetClassObject(Guid* rclsid, Guid* riid, nint* ppv)
    {
        if (*rclsid != new Guid("0A96F866-D763-4099-8E4E-ED1801BE9FBC"))
        {
            return HResult.E_NOINTERFACE;
        }

        *ppv = ClassFactory.For(new CorProfiler());

        return 0;
    }
}