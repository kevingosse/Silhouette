using System.Runtime.InteropServices;
using TestApp;

// Attach mode: TestApp self-attaches the profiler at runtime
if (args.Length >= 2 && args[0] == "--attach")
{
    Console.WriteLine($"PID: {Environment.ProcessId}");
    Console.WriteLine("Mode: attach");

    var profilerPath = args[1];
    var attachTest = new AttachTest(profilerPath);

    // Run the attach test first to get the profiler loaded
    var attachTests = new List<ITest> { attachTest };

    // After attach, run compatible tests that only need events available after attach.
    // Incompatible: ComTests (needs COR_PRF_MONITOR_CCW),
    //               PInvokeTests (needs COR_PRF_MONITOR_CODE_TRANSITIONS),
    //               NgenTests (needs COR_PRF_MONITOR_CACHE_SEARCHES)
    attachTests.Add(new AssemblyLoadContextTests());
    attachTests.Add(new ClassLoadTests());
    attachTests.Add(new ConditionalWeakTableTests());
    attachTests.Add(new DynamicMethodTests());
    attachTests.Add(new ExceptionTests());
    attachTests.Add(new FinalizationTests());
    attachTests.Add(new HandleTests());
    attachTests.Add(new GarbageCollectionTests());
    attachTests.Add(new JitCompilationTests());
    attachTests.Add(new ThreadTests());
    attachTests.Add(new ModuleTests());
    attachTests.Add(new GenericArgumentsTests());
    attachTests.Add(new IlRewriteTest());
    attachTests.Add(new FunctionInfoTests());

    return RunTests(attachTests);
}

// Startup mode: profiler was attached via environment variables
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
    new IlRewriteTest(),
    new FunctionInfoTests()
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

return RunTests(tests);

static int RunTests(List<ITest> tests)
{
    int failures = 0;

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
            failures++;
            continue;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("[SUCCESS] ");
        Console.ResetColor();
        Console.WriteLine($"{test.GetType().Name}");
    }

    return failures > 0 ? 1 : 0;
}
