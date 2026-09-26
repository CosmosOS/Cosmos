// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.System.Diagnostics;

namespace SampleDrivers;

/// <summary>
/// The kernel state the interrupt cells compare before and after a binding
/// attempt: which thread runs, how many dynamic interrupt vectors are
/// bound, and how many pages are free. Read by the drivers in their Probe
/// and handlers, and by the cells.
/// </summary>
public static class KernelState
{
    /// <summary>The CPU every thread runs on: the kernel brings up no other.</summary>
    private const uint BootCpu = 0;

    /// <summary>Pages the page allocator has free now. Any context.</summary>
    public static ulong FreePages => MemoryInfo.FreePages;

    /// <summary>
    /// Dynamic interrupt vectors with a handler now. On x64 each MSI-X entry
    /// a driver binds takes one; ARM64 binds LPIs instead, which leave this
    /// count alone. Thread context, such as Probe; see
    /// <see cref="InterruptVectors.CountBound"/> for how it is read.
    /// </summary>
    /// <exception cref="MissingFieldException">Core's vector allocator moved.</exception>
    /// <exception cref="MissingMethodException">Core's vector allocator moved.</exception>
    public static int BoundVectors => InterruptVectors.CountBound();

    /// <summary>
    /// The scheduler's ID for the thread running this code, and whether it is
    /// its CPU's idle thread, the thread that boots and runs the kernel.
    /// Thread context.
    /// </summary>
    /// <param name="id">The thread's ID, or 0 when there is no current thread.</param>
    /// <param name="isIdle">Whether it is the idle thread, or false when there is no current thread.</param>
    /// <returns>False when the scheduler has no current thread.</returns>
    public static bool TryGetCurrentThread(out uint id, out bool isIdle)
    {
        if (!SchedulerInfo.TryGetCurrentThread(BootCpu, out KernelThreadInfo info))
        {
            id = 0;
            isIdle = false;
            return false;
        }

        id = info.Id;
        isIdle = info.IsIdle;
        return true;
    }
}
