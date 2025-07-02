using System.Reflection;
using System.Reflection.Emit;

namespace TestApp;

internal class DynamicMethodTests : ITest
{
    public void Run()
    {
        var handle = InnerScope();

        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, $"DynamicMethodJITCompilationStarted - {handle:x2}");
        Logs.AssertContains(logs, $"DynamicMethodJITCompilationFinished - {handle:x2}");
        Logs.AssertContains(logs, $"DynamicMethodUnloaded - {handle:x2}");
    }

    private static nint InnerScope()
    {
        var dynamicMethod = new DynamicMethod("test", null, null);
        var ilGenerator = dynamicMethod.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ret);

        var handle = (RuntimeMethodHandle)typeof(DynamicMethod)
            .GetMethod("GetMethodDescriptor", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(dynamicMethod, null)!;

        dynamicMethod.CreateDelegate<Action>().Invoke();

        return handle.Value;
    }
}