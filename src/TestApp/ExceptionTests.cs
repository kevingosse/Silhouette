using System.Reflection;

namespace TestApp;

internal class ExceptionTests : ITest
{
    public void Run()
    {
        var threadId = (IntPtr)typeof(Thread).GetField("_DONT_USE_InternalThread", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(Thread.CurrentThread)!;

        try
        {
            throw new InvalidOperationException("Expected");
        }
        catch (Exception)
        {
            try
            {
                throw new TaskCanceledException("Expected");
            }
            catch (OperationCanceledException) when (ExceptionFilter1())
            {
            }
        }

        try
        {
            Finally1();
        }
        catch
        {
            // Expected
        }

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, "ExceptionCatcherEnter - catch System.InvalidOperationException in TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, "ExceptionCatcherEnter - catch System.Threading.Tasks.TaskCanceledException in TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, $"ExceptionCatcherLeave - Thread {threadId:x2} - Nested level 1");
        Logs.AssertContains(logs, $"ExceptionCatcherLeave - Thread {threadId:x2} - Nested level 0");
        Logs.AssertContains(logs, "ExceptionSearchFilterEnter - TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, "ExceptionSearchFilterEnter - TestApp.ExceptionTests.ExceptionFilter1");
        Logs.AssertContains(logs, $"ExceptionSearchFilterLeave - Thread {threadId:x2} - Nested level 1");
        Logs.AssertContains(logs, $"ExceptionSearchFilterLeave - Thread {threadId:x2} - Nested level 0");
        Logs.AssertContains(logs, "ExceptionSearchFunctionEnter - TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, "ExceptionSearchFunctionEnter - TestApp.ExceptionTests.ExceptionFilter1");
        Logs.AssertContains(logs, $"ExceptionSearchFunctionLeave - Thread {threadId:x2} - Nested level 1");
        Logs.AssertContains(logs, $"ExceptionSearchFunctionLeave - Thread {threadId:x2} - Nested level 0");
        Logs.AssertContains(logs, "ExceptionUnwindFinallyEnter - TestApp.ExceptionTests.Finally1");
        Logs.AssertContains(logs, "ExceptionUnwindFinallyEnter - TestApp.ExceptionTests.Finally2");
        Logs.AssertContains(logs, $"ExceptionUnwindFinallyLeave - Thread {threadId:x2} - Nested level 1");
        Logs.AssertContains(logs, $"ExceptionUnwindFinallyLeave - Thread {threadId:x2} - Nested level 0");
        Logs.AssertContains(logs, "ExceptionUnwindFunctionEnter - TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, "ExceptionUnwindFunctionEnter - TestApp.ExceptionTests.ExceptionFilter1");
        Logs.AssertContains(logs, "ExceptionUnwindFunctionEnter - TestApp.ExceptionTests.Finally1");
        Logs.AssertContains(logs, "ExceptionUnwindFunctionEnter - TestApp.ExceptionTests.Finally2");
        Logs.AssertContains(logs, $"ExceptionUnwindFunctionLeave - Thread {threadId:x2} - Nested level 1");
        Logs.AssertContains(logs, $"ExceptionUnwindFunctionLeave - Thread {threadId:x2} - Nested level 0");
        Logs.AssertContains(logs, "ExceptionSearchCatcherFound - TestApp.ExceptionTests.Run");
        Logs.AssertContains(logs, "ExceptionSearchCatcherFound - TestApp.ExceptionTests.ExceptionFilter1");
        Logs.AssertContains(logs, "ExceptionSearchCatcherFound - TestApp.ExceptionTests.Finally1");
        Logs.AssertContains(logs, "ExceptionThrown - System.Exception");
        Logs.AssertContains(logs, "ExceptionThrown - System.NotSupportedException");
        Logs.AssertContains(logs, "ExceptionThrown - System.InvalidCastException");
        Logs.AssertContains(logs, "ExceptionThrown - System.InvalidOperationException");
        Logs.AssertContains(logs, "ExceptionThrown - System.Threading.Tasks.TaskCanceledException");
    }

    private static bool ExceptionFilter1()
    {
        try
        {
            throw new Exception("Expected");
        }
        catch (Exception) when (ExceptionFilter2())
        {
        }

        return true;
    }

    private static bool ExceptionFilter2()
    {
        return true;
    }

    private static void Finally1()
    {
        try
        {
            throw new NotSupportedException("Finally");
        }
        finally
        {
            try
            {
                Finally2();
            }
            catch
            {
                // Expected
            }
        }
    }

    private static void Finally2()
    {
        try
        {
            throw new InvalidCastException("Finally");
        }
        finally
        {
            GC.KeepAlive(null);
        }
    }
}