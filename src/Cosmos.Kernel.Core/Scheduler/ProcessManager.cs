using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory.VAS;
using Cosmos.Kernel.Core.Scheduler.Atomics;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Lightweight registry for processes. This class intentionally does not
/// implement a loader; callers build processes manually with AddressSpace,
/// Thread, and SchedulerManager APIs.
/// </summary>
internal static class ProcessManager
{
    private static AtomicIdULong s_nextProcessId = new AtomicIdULong();
    private static readonly List<Process> s_processes = new List<Process>();

    /// <summary>
    /// Registers a process in the global registry.
    /// </summary>
    public static void RegisterProcess(Process process)
    {
        process.Id = s_nextProcessId.Next();
        s_processes.Add(process);
    }

    /// <summary>
    /// Marks a process as dead and releases its address-space reference.
    /// Threads are moved to the Dead state by the caller / scheduler.
    /// </summary>
    public static void TerminateProcess(Process process, int exitCode)
    {
        if (process.State == ProcessState.Dead)
        {
            return;
        }

        process.Kill(exitCode);


    }

    /// <summary>
    /// Finds the process that owns the given address space, if any.
    /// </summary>
    public static Process? FindProcessByAddressSpace(AddressSpace addressSpace)
    {
        for (int i = 0; i < s_processes.Count; i++)
        {
            Process process = s_processes[i];
            if (process.AddressSpace == addressSpace)
            {
                return process;
            }
        }

        return null;
    }

    /// <summary>
    /// Terminates the process that owns <paramref name="addressSpace"/>, if found.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void TerminateByAddressSpace(AddressSpace addressSpace, int exitCode)
    {
        Process? process = FindProcessByAddressSpace(addressSpace);
        if (process != null)
        {
            TerminateProcess(process, exitCode);
        }
    }
}
