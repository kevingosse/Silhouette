using System.Runtime.InteropServices;

namespace TestApp;

internal class HandleTests : ITest
{
    public void Run()
    {
        var obj = new object();

        var handle = GCHandle.Alloc(obj, GCHandleType.Normal);

        var handleAddress = GCHandle.ToIntPtr(handle);

        handle.Free();

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, $"HandleCreated - {handleAddress:x2} - System.Object");
        Logs.AssertContains(logs, $"HandleDestroyed - {handleAddress:x2}");
    }
}