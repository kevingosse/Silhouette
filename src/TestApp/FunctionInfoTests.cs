namespace TestApp;

internal class FunctionInfoTests : ITest
{
    public void Run()
    {
        Logs.Clear();

        Logs.Assert(ProfilerPInvokes.TestGetTokenAndMetaDataFromFunction());

        var logs = Logs.Fetch().ToList();

        // Verify the profiler could use the IMetaDataImport to resolve a method name,
        // proving the returned interface pointer is valid (not garbage from wrong IID)
        Logs.Assert(logs.Any(l => l.StartsWith("GetTokenAndMetaDataFromFunction -") && l.Contains("0x")));
        Logs.AssertContains(logs, "GetTokenAndMetaDataFromFunction - Success");

        Logs.Clear();

        Logs.Assert(ProfilerPInvokes.TestGetNativeCodeStartAddresses());

        logs = Logs.Fetch().ToList();

        // Verify the profiler could use returned addresses with GetCodeInfo4,
        // proving the addresses were not truncated from 64-bit to 32-bit
        Logs.Assert(logs.Any(l => l.Contains("Address[0]: 0x") && l.Contains("code region(s)")));
        Logs.AssertContains(logs, "GetNativeCodeStartAddresses - Success");
    }
}
