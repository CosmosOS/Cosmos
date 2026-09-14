using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Per-CPU scheduling state.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public sealed class PerCpuState : SchedulerExtensible
{
    /// <summary>
    /// Per-CPU state is created by <see cref="SchedulerManager"/> during CPU
    /// bring-up. A scheduler receives one through its <see cref="IScheduler"/>
    /// hooks and attaches its own state to
    /// <see cref="SchedulerExtensible.SchedulerData"/>.
    /// </summary>
    internal PerCpuState()
    {
    }

    // ===== Identity =====

    /// <summary>
    /// ID of the CPU this state belongs to.
    /// </summary>
    public uint CpuId { get; internal set; }

    // ===== Current Execution =====

    /// <summary>
    /// Thread currently executing on this CPU, or <see langword="null"/>
    /// before the scheduler has run.
    /// </summary>
    public SchedulerThread? CurrentThread { get; internal set; }

    /// <summary>
    /// This CPU's idle thread, scheduled when no other thread is runnable, or
    /// <see langword="null"/> before the scheduler has set one up. Once set it
    /// stays set: the idle thread is never unregistered.
    /// </summary>
    public SchedulerThread? IdleThread { get; internal set; }

    // Set by ReadyThread when it wakes a thread (typically an ISR-side
    // InterruptEvent.Signal); consumed by ReschedulePendingFromIrq on
    // hardware-IRQ exit so the woken thread runs immediately instead of
    // sitting in the run queue until the next timer tick.
    internal bool _needReschedule;

    // ===== Synchronization =====
    internal SpinLock _lock;
}
