Silhouette - A library to build .NET profilers in .NET
=======================

# Quick start

Create a new C# NativeAOT project. Reference the Silhouette nuget package and add a class inheriting from `Silhouette.CorProfilerCallback11Base` (you can use a different version of `CorProfilerCallbackBase` depending on the version of .NET you're targeting). Override the `Initialize` method. It will be called with the highest version number of `ICorProfilerInfo` supported by the target runtime.

```csharp

using Silhouette;

[Profiler("0A96F866-D763-4099-8E4E-ED1801BE9FBC")] // Use your own profiler GUID here
internal partial class CorProfilerCallback : CorProfilerCallback11Base
{
    protected override HResult Initialize(int iCorProfilerInfoVersion)
    {
        if (iCorProfilerInfoVersion < 11)
        {
            return HResult.E_FAIL;
        }

        var result = ICorProfilerInfo11.SetEventMask(COR_PRF_MONITOR.COR_PRF_ENABLE_STACK_SNAPSHOT | COR_PRF_MONITOR.COR_PRF_MONITOR_THREADS);

        return result;
    }
}
```

The `Profiler` attribute triggers a source-generator that emits the proper `DllGetClassObject` function and validates that the user is using the matching guid for the profiler. Alternatively, you can manually implement a `DllGetClassObject` method that will be called by the .NET runtime when initializing the profiler. Use the built-in `ClassFactory` implementation and give it an instance of your `CorProfiler` class.

```csharp
using Silhouette;
using System.Runtime.InteropServices;

internal class DllMain
{
     // This code is automatically generated when using the `[Profiler]` attribute on `CorProfilerCallback`
    [UnmanagedCallersOnly(EntryPoint = "DllGetClassObject")]
    public static unsafe HResult DllGetClassObject(Guid* rclsid, Guid* riid, nint* ppv)
    {
        // Use your own profiler GUID here
        if (*rclsid != new Guid("0A96F866-D763-4099-8E4E-ED1801BE9FBC"))
        {
            return HResult.CORPROF_E_PROFILER_CANCEL_ACTIVATION;
        }

        *ppv = ClassFactory.For(new CorProfilerBase());

        return HResult.S_OK;
    }
}
```

`CorProfilerXxBase` offers base virtual methods for all `ICorProfilerCallback` methods, so override the ones you're interested in:

```csharp
    protected override HResult ThreadCreated(ThreadId threadId)
    {
        Console.WriteLine($"Thread created: {threadId.Value}");
        return HResult.S_OK;
    }
```

Use the `ICorProfilerInfoXx` fields to access the `ICorProfilerInfo` APIs:

```csharp
    private unsafe string ResolveMethodName(nint ip)
    {
        try
        {
            var functionId = ICorProfilerInfo11.GetFunctionFromIP(ip).ThrowIfFailed();
            var functionInfo = ICorProfilerInfo2.GetFunctionInfo(functionId).ThrowIfFailed();
            using var metaDataImport = ICorProfilerInfo2.GetModuleMetaDataImport(functionInfo.ModuleId, CorOpenFlags.ofRead).ThrowIfFailed().Wrap();
            var methodProperties = metaDataImport.Value.GetMethodProps(new MdMethodDef(functionInfo.Token)).ThrowIfFailed();
            var typeDefProps = metaDataImport.Value.GetTypeDefProps(methodProperties.Class).ThrowIfFailed();

            return $"{typeDefProps.TypeName}.{methodProperties.Name}";
        }
        catch (Win32Exception)
        {
            return "<unknown>";
        }
    }
```

Most methods return an instance of `HResult<T>`. You can deconstruct it into a `(HResult error, T result)` and manually check the error code. You can also use the `ThrowIfFailed()` method that will return only the result and throw a `Win32Exception` if the error code is not `S_OK`.
