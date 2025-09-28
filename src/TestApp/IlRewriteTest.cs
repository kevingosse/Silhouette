using System.Reflection;
using System.Runtime.CompilerServices;

namespace TestApp;

internal class IlRewriteTest : ITest
{
    public void Run()
    {
        Logs.Clear();

        StringSubstitutionTest();
        RequestReJitTest();
    }

    private static void StringSubstitutionTest()
    {
        Logs.Assert("success".Equals("failure")); // Failure will be rewritten to success by the profiler
    }

    private static void RequestReJitTest()
    {
        var method = typeof(IlRewriteTest).GetMethod(nameof(GetValue), BindingFlags.NonPublic | BindingFlags.Static)!;
        var methodDef = method.MetadataToken;

        var module = typeof(IlRewriteTest).Module;
        var moduleHandle = (IntPtr)module.GetType().GetMethod("GetUnderlyingNativeHandle", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(module, null)!;

        Logs.Assert(GetValue() == 10);
        Logs.Assert(PInvokes.RequestReJit(moduleHandle, methodDef));
        Logs.Assert(GetValue() == 12);
        Logs.Assert(PInvokes.RequestRevert(moduleHandle, methodDef));
        Logs.Assert(GetValue() == 10);
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetValue() => 10;
}
