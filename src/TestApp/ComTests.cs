using System.Runtime.InteropServices;

namespace TestApp;

internal class ComTests : ITest
{
    public void Run()
    {
        _ = Marshal.GetIUnknownForObject(new object());

        var logs = Logs.Fetch().ToList();
        Logs.AssertContains(logs, "COMClassicVTableCreated - System.Object - fbec27f0-fc44-396c-8a8c-4c1993516f2d - 11");
    }
}