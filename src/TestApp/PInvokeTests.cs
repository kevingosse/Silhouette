namespace TestApp;

internal class PInvokeTests : ITest
{
    public void Run()
    {
        _ = PInvokes.CurrentOs.GetCurrentThreadId();

        var logs = Logs.Fetch().ToList();
        Logs.AssertContains(logs, $"ManagedToUnmanagedTransition - CurrentOs.{PInvokes.CurrentOs.GetNativeMethodName()} - COR_PRF_TRANSITION_CALL");
        Logs.AssertContains(logs, $"UnmanagedToManagedTransition - CurrentOs.{PInvokes.CurrentOs.GetNativeMethodName()} - COR_PRF_TRANSITION_RETURN");
    }
}