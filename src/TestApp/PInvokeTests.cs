namespace TestApp;

internal class PInvokeTests : ITest
{
    public void Run()
    {
        _ = ProfilerPInvokes.CountFrozenObjects();

        var logs = Logs.Fetch().ToList();
        Logs.AssertContains(logs, "ManagedToUnmanagedTransition - TestApp.ProfilerPInvokes.CountFrozenObjects - COR_PRF_TRANSITION_CALL");
        Logs.AssertContains(logs, "UnmanagedToManagedTransition - TestApp.ProfilerPInvokes.CountFrozenObjects - COR_PRF_TRANSITION_RETURN");
    }
}