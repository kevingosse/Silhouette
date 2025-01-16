namespace TestApp;

internal class NgenTests : ITest
{
    public void Run()
    {
        int count = 0;

        var allLogs = Logs.All.ToList();

        Logs.AssertContains(allLogs, "JITCachedFunctionSearchStarted - System.String.ToString");
        Logs.AssertContains(allLogs, "JITCachedFunctionSearchFinished - System.String.ToString - Found");
        Logs.AssertContains(allLogs, "JITCachedFunctionSearchFinished - System.Text.StringBuilder.AppendFormat inlined System.String.ToString");
    }
}