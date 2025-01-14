using System.Reflection;
using TestApp;

Console.WriteLine($"PID: {Environment.ProcessId}");

var logs = Logs.Fetch().ToList();

foreach (var log in logs)
{
    Console.WriteLine(log);
}

Logs.AssertContains(logs, $"AssemblyLoadFinished - TestApp - AppDomain clrhost - Module {typeof(Program).Assembly.Location}");
Logs.AssertContains(logs, $"AppDomainCreationStarted - System.Private.CoreLib.dll - Process Id {Environment.ProcessId}");
Logs.AssertContains(logs, $"AppDomainCreationStarted - DefaultDomain - Process Id {Environment.ProcessId}");
Logs.AssertContains(logs, "AppDomainCreationFinished - System.Private.CoreLib.dll - HResult S_OK");
Logs.AssertContains(logs, "AppDomainCreationFinished - DefaultDomain - HResult S_OK");

var threadId = (IntPtr)typeof(Thread).GetField("_DONT_USE_InternalThread", BindingFlags.Instance | BindingFlags.NonPublic)
    .GetValue(Thread.CurrentThread);
var osId = PInvokes.Win32.GetCurrentThreadId();

Logs.Assert(PInvokes.GetCurrentThreadInfo(out var actualThreadId, out var actualOsId));
Logs.Assert((ulong)threadId == actualThreadId);
Logs.Assert(osId == actualOsId);

// Clear the logs before the next tests
Logs.Clear();

AssemblyLoadContextTests.Run();
ClassLoadTests.Run();
ComTests.Run();
ConditionalWeakTableTests.Run();
DynamicMethodTests.Run();
ExceptionTests.Run(threadId);
FinalizationTests.Run();
HandleTests.Run();
GarbageCollectionTests.Run();
JitCompilationTests.Run();
PInvokeTests.Run();
ThreadTests.Run();
ModuleTests.Run();

// Dump last logs before exiting
foreach (var log in Logs.Fetch())
{
    Console.WriteLine(log);
}
