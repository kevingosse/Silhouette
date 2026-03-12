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

        var result = SignatureTest();
        Logs.Assert(result == 42, $"SignatureTest returned {result} instead of 42");

        result = ParameterRoundTripTest(1, 2, 3, 4, 5);
        Logs.Assert(result == 16, $"ParameterRoundTripTest returned {result} instead of 16");
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
        Logs.Assert(ProfilerPInvokes.RequestReJit(moduleHandle, methodDef));
        Logs.Assert(GetValue() == 12);
        Logs.Assert(ProfilerPInvokes.RequestRevert(moduleHandle, methodDef));
        Logs.Assert(GetValue() == 10);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int SignatureTest()
    {
        throw new Exception("Should never get there");
    }


    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int ParameterRoundTripTest(int a, int b, int c, int d, int e)
    {
        int result = 0;

        try
        {
            // 5 parameters forces the compiler to use ldarg.s for the 5th parameter (index 4).
            result = a + b + c + d + e;
            throw new Exception("test");
        }
        catch
        {
            // Make sure EH sections are not lost during rewriting
            result += 1;
        }        

        return result;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int GetValue() => 10;
}
