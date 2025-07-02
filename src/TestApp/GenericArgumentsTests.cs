using System.Collections;

namespace TestApp;
internal unsafe class GenericArgumentsTests : ITest
{
    public void Run()
    {
        var method = typeof(Test).GetMethod(nameof(Test.Function))!;

        Span<char> buffer = stackalloc char[1024];
        int length;

        fixed (char* pBuffer = buffer)
        {
            length = PInvokes.GetGenericArguments(typeof(Test).TypeHandle.Value, method.MetadataToken, pBuffer, 1024);
        }

        if (length < 0)
        {
            foreach (var log in Logs.Fetch())
            {
                Console.WriteLine(log);
            }

            throw new InvalidOperationException("Failed to get module names");
        }

        var genericArguments = new string(buffer[..length]);
        Logs.Assert(genericArguments == "T1(Test, System.Collections.IEnumerable, System.Collections.IEqualityComparer), T2(System.IComparable, System.ValueType)");
    }

    private class Test
    {
#pragma warning disable CA1822
        // ReSharper disable twice UnusedTypeParameter
        public void Function<T1, T2>()
#pragma warning restore CA1822
            where T1: Test, IEnumerable, IEqualityComparer
            where T2: struct, IComparable
        {
        }
    }
}
