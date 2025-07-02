using System;
using System.Runtime.InteropServices;
using Silhouette;

namespace ManagedDotnetProfiler;

public static class DllMain
{
    private static ClassFactory Instance;

    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    public static unsafe HResult DllGetClassObject(Guid* rclsid, Guid* riid, nint* ppv)
    {
        if (*rclsid != new Guid("0A96F866-D763-4099-8E4E-ED1801BE9FBC"))
        {
            return HResult.E_NOINTERFACE;
        }

        Instance = new ClassFactory(new CorProfiler());
        *ppv = Instance.IClassFactory;

        return 0;
    }
}