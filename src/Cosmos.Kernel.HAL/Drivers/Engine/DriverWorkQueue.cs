// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using SchedSpinLock = Cosmos.Kernel.Core.Scheduler.SpinLock;

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// The <c>driver-work</c> thread and the queue of <see cref="DeviceWorkItem"/>s
/// it runs, one at a time, in the order they were scheduled. The thread is
/// started by the first <see cref="DeviceContext.TryCreateWorkItem"/>, so a
/// kernel whose drivers never ask for one has no extra thread.
/// </summary>
/// <remarks>
/// The queue is a list threaded through the items themselves, so that
/// scheduling from an interrupt handler allocates nothing. Its lock is taken
/// IRQ-safe: an interrupt handler schedules on the same CPU that may be
/// holding it.
/// </remarks>
internal static class DriverWorkQueue
{
    // Not readonly: SpinLock is a mutable struct.
    private static SchedSpinLock s_lock;

    // The queue, guarded by s_lock.
    private static DeviceWorkItem? s_head;
    private static DeviceWorkItem? s_tail;

    /// <summary>Signalled once per queued item; the thread waits on it. Created before the thread starts.</summary>
    private static InterruptEvent? s_wake;

    /// <summary>Whether the thread started: null until the first attempt, which is the only one.</summary>
    private static bool? s_started;

    /// <summary>
    /// Starts the driver-work thread unless it is already running. Called
    /// from a driver's Probe only; probes run one at a time, so no second
    /// caller can race the first.
    /// </summary>
    /// <returns>
    /// True when the thread runs. False when the scheduler is compiled out,
    /// or when it did not switch to the new thread in time: then no work item
    /// can ever run, and every later call answers false at once instead of
    /// waiting again.
    /// </returns>
    internal static bool TryEnsureStarted()
    {
        if (!CosmosFeatures.SchedulerEnabled)
        {
            return false;
        }

        if (s_started is { } started)
        {
            return started;
        }

        s_wake = new InterruptEvent();
        bool began = KernelThread.TryStart(RunWorkThread);
        s_started = began;
        Serial.WriteString(began
            ? "[Drivers] driver-work thread started\n"
            : "[Drivers] driver-work thread did not start: the scheduler is not switching threads\n");
        return began;
    }

    /// <summary>Masks interrupts and takes the queue's lock, which also guards every item's state.</summary>
    internal static IrqLockScope AcquireLock() => s_lock.AcquireIrqSafe();

    /// <summary>Appends <paramref name="item"/> to the queue. The caller holds the lock and has checked the item is not queued.</summary>
    internal static void EnqueueLocked(DeviceWorkItem item)
    {
        item.NextQueued = null;
        if (s_tail is { } tail)
        {
            tail.NextQueued = item;
        }
        else
        {
            s_head = item;
        }

        s_tail = item;
    }

    /// <summary>Wakes the driver-work thread for one more queued item. IRQ-safe.</summary>
    internal static void Wake() => s_wake?.Signal();

    /// <summary>
    /// The driver-work thread: waits to be woken, then runs every item
    /// queued, and waits again. The wake-ups are counted, so one that comes
    /// while items run is not lost; at worst it finds the queue empty.
    /// </summary>
    private static void RunWorkThread()
    {
        if (s_wake is not { } wake)
        {
            return;
        }

        while (true)
        {
            wake.Wait();
            while (TryDequeue() is { } item)
            {
                item.Run();
            }
        }
    }

    /// <summary>
    /// Takes the first item off the queue whose callback should run,
    /// skipping any item disarmed while it waited. Null when none is left.
    /// </summary>
    private static DeviceWorkItem? TryDequeue()
    {
        using (s_lock.AcquireIrqSafe())
        {
            while (s_head is { } item)
            {
                s_head = item.NextQueued;
                if (s_head is null)
                {
                    s_tail = null;
                }

                item.NextQueued = null;
                if (item.TakeForRunLocked())
                {
                    return item;
                }
            }

            return null;
        }
    }
}
