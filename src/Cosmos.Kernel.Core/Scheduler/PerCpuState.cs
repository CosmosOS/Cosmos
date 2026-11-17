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
    /// before the scheduler has run. Written by the switch on this CPU under
    /// its queue lock; a read from another CPU is a snapshot.
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
    // YieldThread set it to give up the CPU on that same exit. Written on
    // another CPU by a wake that queued a thread here (SMP phase 3a), read on
    // this one by its interrupt exit: always through Volatile. A remote
    // writer sets it under this CPU's queue lock and sends the reschedule
    // interrupt after; this CPU's own request (YieldThread, Park, the swap
    // with the world stopped) is a bare volatile store, since the exit that
    // consumes it runs here, under the lock, and a store that lands after
    // its read is served by the next exit.
    internal bool _needReschedule;

    // The thread the last switch-out on this CPU evicted because its set
    // excludes this CPU (SMP phase 3b): left out of the run structure, still
    // Running when it was, and moved by SchedulerManager.MigratePending on
    // the same interrupt exit once this CPU's queue lock is released, under
    // both CPUs' locks in order. Written and read on this CPU alone, in
    // interrupt context.
    internal SchedulerThread? _pendingMigration;

    // ===== Synchronization =====

    // The queue lock: held around every policy hook that touches this CPU's
    // run structure, around CurrentThread and around _needReschedule. The
    // owning CPU takes it from its tick and its interrupt exits (interrupts
    // already masked there, so the plain acquire); a remote CPU takes it, with
    // its own interrupts masked, to wake a thread onto this CPU or to pull one
    // off it (SchedulerManager.LockCpu). Two queue locks are held at once only
    // by a migration, in ascending CPU index; the registry lock comes after
    // either, the heap lock after that. Nothing allocates while it is held.
    internal SpinLock _lock;
}
