namespace TestApp;

internal class ModuleTests
{
    public static unsafe void Run()
    {
        var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();

        var buffer = new char[1024 * 100];

        int length;

        fixed (char* p = buffer)
        {
            length = PInvokes.GetModuleNames(p, buffer.Length);
        }

        if (length < 0)
        {
            foreach (var log in Logs.Fetch())
            {
                Console.WriteLine(log);
            }

            throw new InvalidOperationException("Failed to get module names");
        }

        // buffer contains multiple null-terminated strings
        var moduleNames = new List<string>();

        var span = new Span<char>(buffer, 0, length);

        while (span.Length > 0)
        {
            var nullIndex = span.IndexOf('\0');

            if (nullIndex < 0)
            {
                break;
            }

            moduleNames.Add(span[..nullIndex].ToString());
            span = span[(nullIndex + 1)..];
        }

        var expectedModules = from assembly in currentAssemblies
                              from module in assembly.Modules
                              let name = assembly.IsDynamic ? module.ScopeName : module.FullyQualifiedName
                              orderby name ascending
                              select name;

        Logs.Assert(expectedModules.SequenceEqual(moduleNames.Order()));
        Logs.Clear();
    }
}
