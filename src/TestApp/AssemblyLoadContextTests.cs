using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace TestApp;

internal class AssemblyLoadContextTests : ITest
{
    public void Run()
    {
        CreateAndUnloadAlc();

        GC.Collect(2, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();
        GC.Collect(2, GCCollectionMode.Forced, true);

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, $"AssemblyUnloadFinished - TestApp - AppDomain clrhost - Module {typeof(Program).Assembly.Location}");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static void CreateAndUnloadAlc()
    {
        var alc = new TestAssemblyLoadContext();
        _ = alc.LoadFromAssemblyPath(typeof(Program).Assembly.Location);
        alc.Unload();
    }

    private class TestAssemblyLoadContext : AssemblyLoadContext
    {
        public TestAssemblyLoadContext()
            : base(true)
        {
        }
        protected override Assembly? Load(AssemblyName name)
        {
            return null;
        }
    }
}