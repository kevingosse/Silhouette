using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ManagedDotnetProfiler;

internal unsafe class PInvoke
{
    [UnmanagedCallersOnly(EntryPoint = "GetCurrentThreadInfo")]
    public static bool GetCurrentThreadInfo(ulong* pThreadId, uint* pOsId)
    {
        var info = CorProfiler.Instance.GetCurrentThreadInfo();
        *pThreadId = info.threadId;
        *pOsId = info.osId;
        return info.result;
    }

    [UnmanagedCallersOnly(EntryPoint = "GetThreads")]
    public static bool GetThreads(uint* array, int length, int* actualLength)
    {
        return Task.Run(() => CorProfiler.Instance.GetThreads(array, length, actualLength)).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = "GetModuleNames")]
    public static int GetModuleNames(char* buffer, int length)
    {
        return Task.Run(() => CorProfiler.Instance.GetModuleNames(buffer, length)).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = "FetchLastLog")]
    public static int FetchLastLog(char* buffer, int size)
    {
        if (!CorProfiler.Logs.TryDequeue(out var log))
        {
            return -1;
        }

        if (size >= log.Length)
        {
            log.CopyTo(new Span<char>(buffer, size));
        }
        else
        {
            log.AsSpan(0, size).CopyTo(new Span<char>(buffer, size));
        }

        return log.Length;
    }
}