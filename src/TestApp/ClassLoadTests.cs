using System.Reflection.Emit;

namespace TestApp;

internal class ClassLoadTests : ITest
{
    public void Run()
    {
        CreateAndUnloadType();

        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, "ClassLoadFinished - DynamicType");
        Logs.AssertContains(logs, "ClassUnloadStarted - DynamicType");
        Logs.AssertContains(logs, "ClassUnloadFinished - DynamicType");
    }

    private static void CreateAndUnloadType()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(new("DynamicAssembly"), AssemblyBuilderAccess.RunAndCollect);
        var module = assembly.DefineDynamicModule("DynamicModule");
        var type = module.DefineType("DynamicType");
        type.CreateType();
    }
}