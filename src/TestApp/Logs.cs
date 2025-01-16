using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace TestApp;

internal static class Logs
{
    private static ConcurrentQueue<string> _allLogs = new();

    public static IEnumerable<string> All => _allLogs;

    public static void Clear()
    {        
        foreach (var log in Fetch())
        {
            // Do nothing
        }
    }

    public static IEnumerable<string> Fetch()
    {
        while (true)
        {
            var log = FetchNext();

            if (log == null)
            {
                yield break;
            }

            _allLogs.Enqueue(log);

            if (log.StartsWith("Error:"))
            {
                throw new Exception($"Found error log: {log}");
            }

            yield return log;
        }
    }

    public static void AssertContains(List<string> logs, string expected)
    {
        if (!logs.Contains(expected))
        {
            Fail("Could not find log: '{expected}'", logs);
        }
    }

    public static void Assert(bool value, [CallerArgumentExpression(nameof(value))] string expression = null)
    {
        if (!value)
        {
            Fail(expression, null);
        }
    }

    private static void Fail(string message, IEnumerable<string>? logs)
    {
        Console.WriteLine("********* Assertion failed, dumping logs *********");

        logs ??= Fetch();

        foreach (var log in logs)
        {
            Console.WriteLine(log);
        }

        throw new Exception($"Assertion failed: {message}");
    }

    private static unsafe string? FetchNext()
    {
        const int bufferSize = 1024;
        Span<char> buffer = stackalloc char[bufferSize];

        fixed (char* c = buffer)
        {
            int length = PInvokes.FetchLastLog(c, buffer.Length);
            return length >= 0 ? new string(buffer[..length]) : null;
        }
    }
}