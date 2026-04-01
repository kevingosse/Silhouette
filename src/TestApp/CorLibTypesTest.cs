using System.Runtime.CompilerServices;

namespace TestApp;

internal class CorLibTypesTest : ITest
{
    public void Run()
    {
        Logs.Clear();

        // This method is rewritten by the profiler to call Object.ToString() on a string,
        // using a MemberRef built from corLib.Object.TypeDefOrRef.MDToken as parent.
        // If CorLibTypes.AssemblyRef/ResolveTypeSig are broken, the TypeDefOrRef is a
        // TypeDef(0) instead of a proper TypeRef, producing a MemberRef for "<Module>.ToString()"
        // which throws MissingMethodException at runtime.
        var result = CorLibTypesProbe();
        Logs.Assert(result == 4, $"CorLibTypesProbe returned {result} instead of 4");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int CorLibTypesProbe()
    {
        // Will be rewritten by the profiler to:
        //   return "test".ToString().Length;  (== 4)
        // using MemberRefs derived from CorLibTypes
        return 0;
    }
}
