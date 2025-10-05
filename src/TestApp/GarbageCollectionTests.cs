namespace TestApp;

internal class GarbageCollectionTests : ITest
{
    public void Run()
    {
        var threadId = NativeMethods.GetCurrentThreadId();

        GC.Collect(2, GCCollectionMode.Default, blocking: true); // reason == COR_PRF_GC_INDUCED

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, "GarbageCollectionStarted - 0, 1, 2, 3, 4 - COR_PRF_GC_INDUCED - 1");
        Logs.AssertContains(logs, "GarbageCollectionFinished - 0");

        Logs.AssertContains(logs, "RuntimeSuspendStarted - COR_PRF_SUSPEND_FOR_GC");
        Logs.AssertContains(logs, "RuntimeSuspendFinished");
        Logs.AssertContains(logs, "RuntimeResumeStarted");
        Logs.AssertContains(logs, "RuntimeResumeFinished");

        Logs.AssertContains(logs, $"RuntimeThreadSuspended - {threadId}");
        Logs.AssertContains(logs, $"RuntimeThreadResumed - {threadId}");
    }
}