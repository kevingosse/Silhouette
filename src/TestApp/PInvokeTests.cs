namespace TestApp;

internal class PInvokeTests : ITest
{
    public void Run()
    {
        _ = PInvokes.CurrentOS.GetCurrentThreadId();

        var logs = Logs.Fetch().ToList();
        Logs.AssertContains(logs, $"ManagedToUnmanagedTransition - CurrentOS.{PInvokes.CurrentOS.GetNativeMethodName()} - COR_PRF_TRANSITION_CALL");
        Logs.AssertContains(logs, $"UnmanagedToManagedTransition - CurrentOS.{PInvokes.CurrentOS.GetNativeMethodName()} - COR_PRF_TRANSITION_RETURN");
    }
}