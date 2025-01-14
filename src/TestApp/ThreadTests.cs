namespace TestApp;

internal class ThreadTests
{
    public static void Run()
    {
        var threadId = CreateAndDestroyThread();

        // Make sure the thread is finalized
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
        GC.WaitForPendingFinalizers();

        var logs = Logs.Fetch().ToList();

        Logs.AssertContains(logs, $"ThreadCreated - {threadId}");
        Logs.AssertContains(logs, $"ThreadDestroyed - {threadId}");
        Logs.AssertContains(logs, $"ThreadAssignedToOSThread - {threadId}");
        Logs.AssertContains(logs, "ThreadNameChanged - Test");

        ChildThreadsTest();
    }

    private static unsafe void ChildThreadsTest()
    {
        const int nbThreads = 8;

        var threads = new Thread[nbThreads];
        var expectedOsIds = new uint[nbThreads];
        using var barrier = new Barrier(nbThreads + 1);

        for (int i = 0; i < nbThreads; i++)
        {
            int index = i;
            threads[index] = new Thread(() =>
            {
                expectedOsIds[index] = PInvokes.Win32.GetCurrentThreadId();
                barrier.SignalAndWait();
                barrier.SignalAndWait();
            });

            threads[index].Start();
        }

        barrier.SignalAndWait();

        Span<uint> actualOsIds = stackalloc uint[50];
        int actualLength = 0;

        fixed (uint* pActualOsIds = actualOsIds)
        {
            PInvokes.GetThreads(pActualOsIds, actualOsIds.Length, &actualLength);            
        }

        if (actualLength >= actualOsIds.Length)
        {
            throw new InvalidOperationException("The buffer was too small");
        }

        Console.WriteLine($"Found {actualLength} threads");

        for (int i = 0; i < actualLength; i++)
        {
            Console.WriteLine($"Found thread {actualOsIds[i]}");
        }

        for (int i = 0; i < nbThreads; i++)
        {
            if (!actualOsIds.Contains(expectedOsIds[i]))
            {
                throw new InvalidOperationException($"Thread {expectedOsIds[i]} was not found");
            }

            Console.WriteLine($"Thread {expectedOsIds[i]} was found");
        }

        Console.WriteLine("All threads were found");

        barrier.SignalAndWait();

        Logs.Clear();
    }

    private static uint CreateAndDestroyThread()
    {
        uint id = 0;

        var thread = new Thread(() => 
        { 
            id = PInvokes.Win32.GetCurrentThreadId();
            Thread.CurrentThread.Name = "Test";
        });

        thread.Start();
        thread.Join();

        return id;
    }
}