using System.Runtime.CompilerServices;

namespace TestApp;
internal class RejitTest
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Test()
    {
        Console.WriteLine("RejitTest");
        JustAnotherMethod();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static RejitTest JustAnotherMethod()
    {
        return null;
    }
}
