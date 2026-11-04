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

    // ===== Tick =====

    internal uint _tickCount;

    /// <summary>
    /// Timer interrupts this CPU handled since it started its tick. Each CPU's timer fires on
    /// its own, so the per-CPU count is the one that says a CPU is alive;
    /// <see cref="SchedulerManager.TickCount"/> is CPU 0's and keeps the pace of the thread
    /// clock (the timed waits, the suites' tick budgets) whatever the CPU count. Written by the
    /// tick on this CPU alone, read from any thread. Wraps: compare differences.
    /// </summary>
    public uint TickCount => Volatile.Read(ref _tickCount);

    // Set by ReadyThread when it wakes a thread (typically an ISR-side
    // InterruptEvent.Signal); consumed by ReschedulePendingFromIrq on
    // hardware-IRQ exit so the woken thread runs immediately instead of
    // sitting in the run queue until the next timer tick. BlockThread and
    // YieldThread set it to give up the CPU on that same exit.
    internal bool _needReschedule;

    // ===== Synchronization =====
    internal SpinLock _lock;
}
