// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.System.Diagnostics;

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// The kernel state the interrupt cells compare before and after a binding
/// attempt: which thread runs, how many dynamic interrupt vectors are
/// bound, and how many pages are free. Read by the test drivers in their
/// Probe and handlers, and by the cells.
/// </summary>
internal static class KernelState
{
    // InterruptManager's dynamic vector window, which x64 MSI-X vectors
    // come from: 0x40 to 0xEE.
    private const int FirstDynamicVector = 0x40;
    private const int LastDynamicVector = 0xEE;

    /// <summary>The CPU every thread runs on: the kernel brings up no other.</summary>
    private const uint BootCpu = 0;

    /// <summary>Pages the page allocator has free now.</summary>
    public static ulong FreePages => MemoryInfo.FreePages;

    /// <summary>
    /// Dynamic interrupt vectors with a handler now. On x64 each MSI-X entry
    /// a driver binds takes one; ARM64 binds LPIs instead, which leave this
    /// count alone.
    /// </summary>
    public static int BoundVectors
    {
        get
        {
            InterruptManager.IrqDelegate?[]? handlers = InterruptManager.s_irqHandlers;
            if (handlers is null)
            {
                return 0;
            }

            int bound = 0;
            for (int vector = FirstDynamicVector; vector <= LastDynamicVector; vector++)
            {
                if (handlers[vector] is not null)
                {
                    bound++;
                }
            }

            return bound;
        }
    }

    /// <summary>
    /// The scheduler's ID for the thread running this code, and whether it is
    /// its CPU's idle thread, the thread that boots and runs the kernel.
    /// </summary>
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
