using System.Runtime;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Runtime;

/// <summary>
/// Primitive-typed introspection helpers callable from gdb during a debug
/// session (via the Cosmos VS Code extension's Kernel views). Returns
/// integers so the host can build its own view of the registry without
/// having to walk managed object layouts over the gdbstub wire.
/// </summary>
internal static class DebugIntrospection
{
    [RuntimeExport("CosmosDbg_Ping")]
    internal static uint Ping()
    {
        return 0x42424242u;
    }

    [RuntimeExport("CosmosDbg_GetThreadCount")]
    internal static int GetThreadCount()
    {
        return SchedulerManager.ThreadCount;
    }

    [RuntimeExport("CosmosDbg_GetThreadSlotCount")]
    internal static int GetThreadSlotCount()
    {
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        return threads?.Length ?? 0;
    }

    /// <summary>
    /// Returns the thread Id +1 (so 0 means "empty slot"). The host
    /// subtracts 1 to get the real Id. Needed because real thread Ids
    /// start at 0 — we can't use raw 0 as an "empty" sentinel.
    /// </summary>
    [RuntimeExport("CosmosDbg_GetThreadId")]
    internal static uint GetThreadId(int slot)
    {
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (threads is null || (uint)slot >= (uint)threads.Length)
        {
            return 0;
        }
        SchedulerThread? t = threads[slot];
        return t is null ? 0u : (t.Id + 1u);
    }

    [RuntimeExport("CosmosDbg_GetThreadState")]
    internal static int GetThreadState(int slot)
    {
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (threads is null || (uint)slot >= (uint)threads.Length)
        {
            return -1;
        }
        SchedulerThread? t = threads[slot];
        return t is null ? -1 : (int)t.State;
    }

    [RuntimeExport("CosmosDbg_GetThreadCpu")]
    internal static int GetThreadCpu(int slot)
    {
        SchedulerThread?[]? threads = SchedulerManager.Threads;
        if (threads is null || (uint)slot >= (uint)threads.Length)
        {
            return -1;
        }
        SchedulerThread? t = threads[slot];
        return t is null ? -1 : (int)t.CpuId;
    }
}
