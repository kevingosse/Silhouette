using System.Runtime.CompilerServices;

namespace TestApp;
internal class RejitTest
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Test()
    {
        Console.WriteLine("************************");
        Console.WriteLine("String substitution test");
        Console.WriteLine("Failure");
    }
}
