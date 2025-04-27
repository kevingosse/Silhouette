using System.Reflection;
using System.Runtime.InteropServices;
using TestApp;

#if(!WINDOWS && !DEBUG)
// on linux runtime does not check that ManagedDotnetProfiler is already loaded
// could not find better option to get handle returned from dlopen so using this trick

// runtime does not load the same library twice so it returns handle of the loaded library:
var handle = NativeLibrary.Load("ManagedDotnetProfiler.so");

NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), (string libraryName, Assembly assembly, DllImportSearchPath? searchPath) =>
{
    return handle;
});
#endif


bool ngenEnabled = Environment.GetEnvironmentVariable("MONITOR_NGEN") == "1";

Console.WriteLine($"PID: {Environment.ProcessId}");
Console.WriteLine($"NGEN: {ngenEnabled}");

var logs = Logs.Fetch().ToList();

// foreach (var log in logs)
// {
//     Console.WriteLine(log);
// }

Logs.AssertContains(logs, $"AssemblyLoadFinished - TestApp - AppDomain clrhost - Module {typeof(Program).Assembly.Location}");
Logs.AssertContains(logs, $"AppDomainCreationStarted - System.Private.CoreLib.dll - Process Id {Environment.ProcessId}");
Logs.AssertContains(logs, $"AppDomainCreationStarted - DefaultDomain - Process Id {Environment.ProcessId}");
Logs.AssertContains(logs, "AppDomainCreationFinished - System.Private.CoreLib.dll - HResult S_OK");
Logs.AssertContains(logs, "AppDomainCreationFinished - DefaultDomain - HResult S_OK");

// Clear the logs before the next tests
Logs.Clear();

var tests = new List<ITest>
{
    new AssemblyLoadContextTests(),
    new ClassLoadTests(),
#if(WINDOWS)
    // not supported on linux
    new ComTests(),
#endif
    new ConditionalWeakTableTests(),
    new DynamicMethodTests(),
    new ExceptionTests(),
    new FinalizationTests(),
    new HandleTests(),
    new GarbageCollectionTests(),
    new JitCompilationTests(),
    new ThreadTests(),
    new ModuleTests()
};

if (!ngenEnabled)
{
    tests.Add(new PInvokeTests());
}
else
{
    tests.Add(new NgenTests());
}

foreach (var test in tests)
{
    try
    {
        test.Run();
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("[FAILURE] ");
        Console.ResetColor();

        Console.WriteLine($"{test.GetType().Name} failed");
        Console.WriteLine(e);
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("[SUCCESS] ");
    Console.ResetColor();
    Console.WriteLine($"{test.GetType().Name}");
}

// Dump last logs before exiting
//foreach (var log in Logs.Fetch())
//{
//    Console.WriteLine(log);
//}
