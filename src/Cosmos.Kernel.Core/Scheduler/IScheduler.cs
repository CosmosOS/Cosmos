using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A scheduling policy: everything that decides which thread runs when.
/// The mechanism around it (the thread registry, the lifecycle transitions,
/// the timer-tick entry, the register save and restore) is fixed.
/// Install an implementation with <see cref="SchedulerManager.SetScheduler"/>.
/// <para>
/// Every hook runs in the kernel's most sensitive window. Every hook that
/// touches a CPU's run structure is called with that CPU's queue lock held
/// by the mechanism and interrupts masked on the calling CPU, the two
/// scheduling hooks inside the timer interrupt itself; each member below
/// says which. The lock is what keeps the tick of that CPU and a wake from
/// another CPU out of the structure while the hook runs, and it is not
/// re-entrant: a hook never takes it. No hook may block, park, or wait, and
/// none may allocate on the tick path, nor under the lock (SMP phase 3a:
/// a holder that started a collection would wait for CPUs that are waiting
/// for it). The two per-CPU lifecycle hooks and <see cref="OnThreadCreate"/>
/// run under no lock, with interrupts masked, and may allocate.
/// </para>
/// <para>
/// Several hooks have no mechanism-side caller yet, because every thread is
/// placed on CPU 0 until SMP phase 5 and nothing in-tree sets priorities.
/// Implement them, but do not expect them to be exercised.
/// </para>
/// <para>
/// Two rules hold for every hook. Each must tolerate being called more than
/// once for the same thread, and each must tolerate an empty
/// <see cref="SchedulerExtensible.SchedulerData"/> slot, because
/// <see cref="OnThreadExit"/> clears it and a thread can lose its record
/// between a tick and the hook that observes it; that is why a hook reads
/// the slot with <c>as</c> and never a cast. A record written by a different
/// policy is never handed to a hook: <see cref="SchedulerManager.SetScheduler"/>
/// re-homes every live thread through <see cref="OnThreadExit"/> on the
/// outgoing policy and <see cref="OnThreadCreate"/> on the incoming one.
/// </para>
/// <para>Inspired by Ekiben's EkibenScheduler trait.</para>
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public interface IScheduler
{
    // ========== Identity ==========

    /// <summary>
    /// Display name for this policy, used in boot logs and reported on the
    /// ring as <c>SchedulerDiagnostics.SchedulerName</c>.
    /// </summary>
    string Name { get; }

    // ========== Lifecycle ==========

    /// <summary>
    /// Allocate this policy's per-CPU bookkeeping into
    /// <see cref="SchedulerExtensible.SchedulerData"/> on
    /// <paramref name="cpuState"/>. Called once per CPU by
    /// <see cref="SchedulerManager.SetScheduler"/>, in thread context with
    /// interrupts masked across the whole swap, so no tick observes the
    /// window between the outgoing policy's <see cref="ShutdownCpu"/> and
    /// this call. No queue lock is held: allocate here.
    /// </summary>
    /// <param name="cpuState">CPU whose state to attach bookkeeping to.</param>
    void InitializeCpu(PerCpuState cpuState);

    /// <summary>
    /// Release the per-CPU bookkeeping, leaving a clean slot for the incoming
    /// policy. Called once per CPU by
    /// <see cref="SchedulerManager.SetScheduler"/> before the replacement is
    /// initialized, in thread context with interrupts masked, like
    /// <see cref="InitializeCpu"/>. Thread slots are not cleared here; see
    /// <see cref="SchedulerExtensible.SchedulerData"/>.
    /// </summary>
    /// <param name="cpuState">CPU whose state to release bookkeeping from.</param>
    void ShutdownCpu(PerCpuState cpuState);

    // ========== Thread Lifecycle ==========

    /// <summary>
    /// Allocate this policy's per-thread bookkeeping into
    /// <see cref="SchedulerExtensible.SchedulerData"/> on
    /// <paramref name="thread"/>. Do not queue the thread yet:
    /// <see cref="OnThreadReady"/> does that. Called with interrupts masked
    /// and no lock held (the thread is reachable from no run structure yet,
    /// so the record may be allocated here), once for a new thread and once
    /// for every live thread the policy inherits through
    /// <see cref="SchedulerManager.SetScheduler"/>. An
    /// inherited thread arrives in whatever state it is in:
    /// <see cref="SchedulerThreadState.Ready"/> ones are handed to
    /// <see cref="OnThreadReady"/> right after, blocked and sleeping ones
    /// come back through it when they wake, and a
    /// <see cref="SchedulerThreadState.Running"/> one keeps running and must
    /// be accounted as runnable without being queued. The boot thread
    /// arrives that way at startup as well.
    /// </summary>
    /// <param name="cpuState">CPU the thread is being created on.</param>
    /// <param name="thread">Thread entering this policy's management.</param>
    void OnThreadCreate(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Make the thread runnable: place it and insert it into the run
    /// structure. Called for a first start, for a wake, and for a sleep
    /// expiry, so it arrives both from thread context and from interrupt
    /// context, either the timer tick waking a sleeper or a device ISR
    /// signalling a waiter, and from whichever CPU woke the thread, under
    /// the queue lock of <paramref name="cpuState"/>, the CPU the thread is
    /// assigned to; at a policy swap, for each thread that was waiting in a
    /// run queue, with interrupts masked and no queue lock. It is the hook most
    /// likely to fire twice for one thread, so guard against inserting it
    /// twice. The manager calls it before it marks the thread Ready, so
    /// <see cref="SchedulerThread.State"/> still tells a wake (Blocked or
    /// Sleeping) from a first start (Created) and from the Ready threads a
    /// policy swap hands over.
    /// </summary>
    /// <param name="cpuState">CPU the thread is queued on.</param>
    /// <param name="thread">Thread becoming runnable.</param>
    void OnThreadReady(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Remove the thread from the run structure and save whatever must
    /// survive the park. Called for a block and for a timed sleep, under the
    /// CPU's queue lock.
    /// </summary>
    /// <param name="cpuState">CPU the thread was queued on.</param>
    /// <param name="thread">Thread parking.</param>
    void OnThreadBlocked(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Remove the thread everywhere and drop its bookkeeping. Called under
    /// the CPU's queue lock, before the mechanism unregisters the thread;
    /// with interrupts masked and no queue lock at a policy swap; with interrupts
    /// masked and no lock for a thread the registry refused right after
    /// <see cref="OnThreadCreate"/>, which no run structure has seen.
    /// </summary>
    /// <param name="cpuState">CPU the thread was managed on.</param>
    /// <param name="thread">Thread terminating.</param>
    void OnThreadExit(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Re-insert a thread that gave up the CPU while still runnable, whether
    /// preempted or yielding. Called in interrupt context, under the CPU's
    /// queue lock, by the switch that takes the thread off the CPU,
    /// <em>after</em> <see cref="PickNext"/>
    /// has already chosen the replacement: the outgoing thread is not a
    /// candidate for the switch it is leaving by.
    /// </summary>
    /// <param name="cpuState">CPU the thread is queued on.</param>
    /// <param name="thread">Thread giving up the CPU.</param>
    void OnThreadYield(PerCpuState cpuState, SchedulerThread thread);

    // ========== Scheduling Decisions ==========

    /// <summary>
    /// Pick the next thread to run. Called in interrupt context, under the
    /// CPU's queue lock, on every reschedule. The outgoing thread is not in
    /// the run structure at this point; it goes back through
    /// <see cref="OnThreadYield"/> afterwards.
    /// </summary>
    /// <param name="cpuState">CPU to schedule.</param>
    /// <returns>
    /// The thread to switch to, or <see langword="null"/> to fall back to the
    /// CPU's idle thread. Remove the thread you return from the run
    /// structure: the manager re-inserts the outgoing thread through
    /// <see cref="OnThreadYield"/> after this call, and does not remove the
    /// incoming one, so leaving it queued grants it a second turn. The
    /// manager does not check the thread's state either, so never return one
    /// that is dead or parked.
    /// </returns>
    SchedulerThread? PickNext(PerCpuState cpuState);

    /// <summary>
    /// Put back a thread the mechanism picked but could not switch to.
    /// Declared for completeness; nothing calls it today.
    /// </summary>
    /// <param name="cpuState">CPU the pick was made for.</param>
    /// <param name="thread">Thread that could not be switched to.</param>
    void OnPickFailed(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Account the elapsed time and decide whether to preempt. Called from
    /// the timer interrupt, on every tick, under the CPU's queue lock, and it
    /// is the only hook that runs on a fixed schedule: keep it allocation-free.
    /// </summary>
    /// <param name="cpuState">CPU being ticked.</param>
    /// <param name="current">Thread running on it.</param>
    /// <param name="elapsedNs">
    /// The timer's configured period in nanoseconds, not a measurement of
    /// how long the thread actually ran. Charging it to
    /// <c>Thread.TotalRuntime</c> is the policy's job, and the ring's
    /// busy-CPU-time reading depends on the policy doing it.
    /// </param>
    /// <returns>
    /// <see langword="true"/> to request a reschedule, which runs
    /// <see cref="PickNext"/> before this interrupt returns.
    /// </returns>
    bool OnTick(PerCpuState cpuState, SchedulerThread current, ulong elapsedNs);

    // ========== Load Balancing ==========

    /// <summary>
    /// Choose a starting CPU for a new or migrating thread, honouring
    /// <see cref="SchedulerThreadFlags.Pinned"/>. Dormant until placement
    /// lands (SMP phase 5): nothing calls it today, and this hook receives
    /// no <see cref="PerCpuState"/>.
    /// </summary>
    /// <param name="thread">Thread being placed.</param>
    /// <param name="currentCpu">CPU the thread is on now.</param>
    /// <param name="cpuCount">Number of CPUs the scheduler manages.</param>
    /// <returns>The CPU to place the thread on.</returns>
    uint SelectCpu(SchedulerThread thread, uint currentCpu, uint cpuCount);

    /// <summary>
    /// Move a thread's bookkeeping, and any virtual-time base, between CPUs.
    /// The mechanism never calls this; a policy calls it from its own
    /// <see cref="Balance"/>.
    /// </summary>
    /// <param name="thread">Thread being migrated.</param>
    /// <param name="fromState">CPU the thread is leaving.</param>
    /// <param name="toState">CPU the thread is joining.</param>
    void OnThreadMigrate(SchedulerThread thread, PerCpuState fromState, PerCpuState toState);

    /// <summary>
    /// Rebalance load across CPUs, honouring <see cref="SchedulerThreadFlags.Pinned"/>.
    /// Dormant until placement lands (SMP phase 5): nothing calls it today.
    /// </summary>
    /// <param name="cpuState">CPU asking for work, or offering it.</param>
    /// <param name="allCpuStates">Every CPU the scheduler manages.</param>
    void Balance(PerCpuState cpuState, PerCpuState[] allCpuStates);

    // ========== Dynamic Reconfiguration ==========

    /// <summary>
    /// Change a thread's priority. The meaning is policy-defined: Stride
    /// reads it as a ticket count, a real-time policy would read it as a
    /// priority level. Reached only through
    /// <see cref="SchedulerManager.SetPriority"/>, under the queue lock of
    /// the CPU the thread is assigned to, which is <paramref name="cpuState"/>.
    /// </summary>
    /// <param name="cpuState">CPU whose run structure the update rewrites.</param>
    /// <param name="thread">Thread to reprioritize.</param>
    /// <param name="priority">New policy-defined priority.</param>
    void SetPriority(PerCpuState cpuState, SchedulerThread thread, long priority);

    /// <summary>
    /// Report a thread's current priority in the same policy-defined units
    /// <see cref="SetPriority"/> takes. Called from thread context under the
    /// queue lock of the thread's CPU, on every thread snapshot the
    /// <c>SchedulerDiagnostics</c> facade takes, so it must not mutate anything.
    /// </summary>
    /// <param name="thread">Thread to query. This hook receives no <see cref="PerCpuState"/>.</param>
    /// <returns>The thread's priority, or 0 when the policy does not track one for it.</returns>
    long GetPriority(SchedulerThread thread);

    // ========== Diagnostics ==========

    /// <summary>
    /// Report how many threads are waiting in the run structure. Read-only
    /// introspection for the <c>SchedulerDiagnostics</c> facade, called from
    /// thread context under the CPU's queue lock, which the caller holds
    /// through <see cref="SchedulerManager.LockCpu"/> around the whole walk
    /// (this count, then each index): one hook call alone would be atomic,
    /// the walk would not, and the tick could rotate the queue between the
    /// calls.
    /// </summary>
    /// <param name="cpuState">CPU to inspect.</param>
    /// <returns>The number of queued threads.</returns>
    int GetRunQueueCount(PerCpuState cpuState);

    /// <summary>
    /// Report the queued thread at a position. Same calling context as
    /// <see cref="GetRunQueueCount"/>.
    /// </summary>
    /// <param name="cpuState">CPU to inspect.</param>
    /// <param name="index">Queue position, from 0 to <see cref="GetRunQueueCount"/> exclusive.</param>
    /// <returns>The queued thread, or <see langword="null"/> when the index is out of range.</returns>
    SchedulerThread? GetRunQueueThread(PerCpuState cpuState, int index);
}
