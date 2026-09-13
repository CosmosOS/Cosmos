// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Bridge;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Low-level context switch operations.
/// Native imports live in Cosmos.Kernel.Core/Bridge/Import/ContextSwitchNative.cs.
/// </summary>
internal static class ContextSwitch
{
    /// <summary>
    /// Requests a context switch to the specified thread.
    /// Called from the timer interrupt handler when preemption is needed.
    /// </summary>
    /// <param name="currentRsp">Current stack pointer (from IRQ context).</param>
    /// <param name="current">Currently running thread (may be null for idle).</param>
    /// <param name="next">Next thread to run.</param>
    public static void Switch(nuint currentRsp, SchedulerThread? current, SchedulerThread next)
    {
        if (current != null)
        {
            current.StackPointer = currentRsp;
            current.State = SchedulerThreadState.Ready;
        }

        bool isNewThread = next.State == SchedulerThreadState.Created;

        next.State = SchedulerThreadState.Running;
        ContextSwitchNative.SetContextSwitchNewThread(isNewThread ? 1 : 0);
        ContextSwitchNative.SetContextSwitchSp(next.StackPointer);
    }
}
