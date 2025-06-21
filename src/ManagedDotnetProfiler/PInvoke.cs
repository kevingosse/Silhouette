using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ManagedDotnetProfiler;

internal unsafe class PInvoke
{
    [UnmanagedCallersOnly(EntryPoint = nameof(GetCurrentThreadInfo))]
    public static bool GetCurrentThreadInfo(ulong* pThreadId, uint* pOsId)
    {
        var info = CorProfiler.Instance.GetCurrentThreadInfo();
        *pThreadId = info.threadId;
        *pOsId = info.osId;
        return info.result;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(GetThreads))]
    public static bool GetThreads(uint* array, int length, int* actualLength)
    {
        return Task.Run(() => CorProfiler.Instance.GetThreads(array, length, actualLength)).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(GetModuleNames))]
    public static int GetModuleNames(char* buffer, int length)
    {
        return Task.Run(() => CorProfiler.Instance.GetModuleNames(buffer, length)).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(FetchLastLog))]
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

    [UnmanagedCallersOnly(EntryPoint = nameof(EnumJittedFunctions))]
    public static bool EnumJittedFunctions(int version)
    {
        return Task.Run(() => CorProfiler.Instance.EnumJittedFunctions(version)).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(CountFrozenObjects))]
    public static int CountFrozenObjects()
    {
        return Task.Run(CorProfiler.Instance.CountFrozenObjects).Result;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(GetGenericArguments))]
    public static int GetGenericArguments(nint typeHandle, int methodToken, char* buffer, int size)
    {
        return Task.Run(() => CorProfiler.Instance.GetGenericArguments(typeHandle, methodToken, buffer, size)).Result;
    }
}