using System.Reflection;

namespace TestApp;

internal class AssemblyImportTests : ITest
{
    public unsafe void Run()
    {
        Logs.Clear();

        var buffer = new char[1024 * 10];
        int length;

        fixed (char* p = buffer)
        {
            length = ProfilerPInvokes.GetAssemblyImportData(p, buffer.Length);
        }

        Logs.Assert(length >= 0, $"GetAssemblyImportData failed with length={length}");

        var logs = Logs.Fetch().ToList();

        // Verify GetAssemblyFromScope returned a valid token (non-zero)
        Logs.Assert(logs.Any(l => l.StartsWith("AssemblyImport - GetAssemblyFromScope token=0x") && !l.EndsWith("0x00000000")));

        // Verify GetAssemblyProps returned the correct assembly name
        Logs.Assert(logs.Any(l => l.StartsWith("AssemblyImport - GetAssemblyProps name=TestApp")));

        // Verify GetAssemblyProps returned version info (version=M.m.B.R pattern)
        var propsLog = logs.First(l => l.StartsWith("AssemblyImport - GetAssemblyProps"));
        Logs.Assert(propsLog.Contains("version="), $"GetAssemblyProps missing version: {propsLog}");

        // Verify success
        Logs.AssertContains(logs, "AssemblyImport - Success");

        // Cross-check assembly refs: names AND versions from profiler vs managed reflection
        var profilerEntries = ParseNullTerminatedStrings(buffer, length);

        var managedEntries = typeof(AssemblyImportTests).Assembly
            .GetReferencedAssemblies()
            .Select(a =>
            {
                var v = a.Version ?? new Version(0, 0, 0, 0);
                return $"{a.Name}|{v.Major}.{v.Minor}.{v.Build}.{v.Revision}";
            })
            .Order(StringComparer.Ordinal)
            .ToList();

        Logs.Assert(
            profilerEntries.SequenceEqual(managedEntries),
            $"Assembly ref mismatch.\nProfiler: [{string.Join(", ", profilerEntries)}]\nManaged:  [{string.Join(", ", managedEntries)}]");
    }

    private static List<string> ParseNullTerminatedStrings(char[] buffer, int length)
    {
        var result = new List<string>();
        var span = new Span<char>(buffer, 0, length);

        while (span.Length > 0)
        {
            var nullIndex = span.IndexOf('\0');

            if (nullIndex < 0)
            {
                break;
            }

            result.Add(span[..nullIndex].ToString());
            span = span[(nullIndex + 1)..];
        }

        return result;
    }
}
