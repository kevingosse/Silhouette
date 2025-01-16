using System.Runtime.ConstrainedExecution;

namespace TestApp;

internal class FinalizationTests : ITest
{
    public void Run()
    {
        InnerScope();

        GC.Collect(2, GCCollectionMode.Forced, true);

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, "FinalizeableObjectQueued - None - FinalizableType");
        Logs.AssertContains(logs, "FinalizeableObjectQueued - COR_PRF_FINALIZER_CRITICAL - CriticalFinalizableType");
    }

    private static void InnerScope()
    {
        _ = new FinalizableType();
        _ = new CriticalFinalizableType();
    }

    private class FinalizableType
    {
        ~FinalizableType()
        {
            GC.KeepAlive(null);
        }
    }

    private class CriticalFinalizableType : CriticalFinalizerObject
    {
    }
}