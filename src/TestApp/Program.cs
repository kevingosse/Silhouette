using System.Runtime.InteropServices;
using TestApp;

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
    new ConditionalWeakTableTests(),
    new DynamicMethodTests(),
    new ExceptionTests(),
    new FinalizationTests(),
    new HandleTests(),
    new GarbageCollectionTests(),
    new JitCompilationTests(),
    new ThreadTests(),
    new ModuleTests(),
    new GenericArgumentsTests(),
    new IlRewriteTest()
};

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    tests.Add(new ComTests());
}

if (!ngenEnabled)
{
    // p/invoke callbacks are not raised when NGEN callbacks are enabled
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
