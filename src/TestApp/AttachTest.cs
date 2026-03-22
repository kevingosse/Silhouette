using Microsoft.Diagnostics.NETCore.Client;

namespace TestApp;

internal class AttachTest : ITest
{
    private readonly string _profilerPath;

    public AttachTest(string profilerPath)
    {
        _profilerPath = profilerPath;
    }

    public void Run()
    {
        var profilerGuid = new Guid("0A96F866-D763-4099-8E4E-ED1801BE9FBC");
        var clientData = "Hello from TestApp"u8.ToArray();

        Console.WriteLine($"Attaching profiler to self (PID {Environment.ProcessId})...");

        var client = new DiagnosticsClient(Environment.ProcessId);

        client.AttachProfiler(
            attachTimeout: TimeSpan.FromMinutes(1),
            profilerGuid: profilerGuid,
            profilerPath: _profilerPath,
            additionalData: clientData);

        Console.WriteLine("Profiler attached successfully.");

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, "InitializeForAttach - ClientData: Hello from TestApp");
        Logs.AssertContains(logs, "ProfilerAttachComplete");
        Logs.AssertContains(logs, $"ProfilerAttachComplete - Rewriting PInvoke maps for module {typeof(Program).Assembly.Location}");
    }
}
