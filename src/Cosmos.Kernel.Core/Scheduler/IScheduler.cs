using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A scheduling policy: everything that decides which thread runs when.
/// The mechanism around it (the thread registry, the lifecycle transitions,
/// the timer-tick entry, the register save and restore) is fixed.
/// Install an implementation with <see cref="SchedulerManager.SetScheduler"/>.
/// <para>
/// Every hook runs in the kernel's most sensitive window. Most are called
/// with interrupts already masked, and the two scheduling hooks run inside
/// the timer interrupt itself; each member below says which. No hook may
/// block, park, or wait, and none may allocate on the tick path. Four get no
/// mask from the manager (<see cref="SetPriority"/>,
/// <see cref="GetPriority"/> and the two run-queue diagnostics) and must
/// take <see cref="SchedulerManager.MaskInterrupts"/> themselves before
/// touching the run structure. The spinlock around <see cref="SetPriority"/>
/// is not a substitute: it excludes another caller, not the tick.
/// </para>
/// <para>
/// Several hooks have no mechanism-side caller yet, because the kernel runs
/// on one CPU and nothing in-tree sets priorities. Implement them, but do
/// not expect them to be exercised.
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
    /// ring as <c>SchedulerInfo.SchedulerName</c>.
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
    /// this call.
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
    /// <see cref="OnThreadReady"/> does that. Called with interrupts masked,
    /// once for a new thread and once for every live thread the policy
    /// inherits through <see cref="SchedulerManager.SetScheduler"/>. An
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
    /// expiry, so it arrives both from thread context with interrupts masked
    /// and from interrupt context, either the timer tick waking a sleeper or
    /// a device ISR signalling a waiter. It is the hook most likely to fire
    /// twice for one thread, so guard against inserting it twice.
    /// </summary>
    /// <param name="cpuState">CPU the thread is queued on.</param>
    /// <param name="thread">Thread becoming runnable.</param>
    void OnThreadReady(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Remove the thread from the run structure and save whatever must
    /// survive the park. Called for a block and for a timed sleep, with
    /// interrupts masked.
    /// </summary>
    /// <param name="cpuState">CPU the thread was queued on.</param>
    /// <param name="thread">Thread parking.</param>
    void OnThreadBlocked(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Remove the thread everywhere and drop its bookkeeping. Called with
    /// interrupts masked, before the mechanism unregisters the thread.
    /// </summary>
    /// <param name="cpuState">CPU the thread was managed on.</param>
    /// <param name="thread">Thread terminating.</param>
    void OnThreadExit(PerCpuState cpuState, SchedulerThread thread);

    /// <summary>
    /// Re-insert a thread that gave up the CPU while still runnable. Called
    /// from an explicit yield in thread context with interrupts masked, and
    /// from the preemption path in interrupt context, where it runs
    /// <em>after</em> <see cref="PickNext"/> has already chosen the
    /// replacement: the outgoing thread is not a candidate for the switch it
    /// is being preempted by.
    /// </summary>
    /// <param name="cpuState">CPU the thread is queued on.</param>
    /// <param name="thread">Thread giving up the CPU.</param>
    void OnThreadYield(PerCpuState cpuState, SchedulerThread thread);

    // ========== Scheduling Decisions ==========

    /// <summary>
    /// Pick the next thread to run. Called in interrupt context, on every
    /// reschedule. The outgoing thread is not in the run structure at this
    /// point; it goes back through <see cref="OnThreadYield"/> afterwards.
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
    /// the timer interrupt, on every tick, and it is the only hook that runs
    /// on a fixed schedule: keep it allocation-free.
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
    /// <see cref="SchedulerThreadFlags.Pinned"/>. Dormant until SMP lands: nothing
    /// calls it today, and this hook receives no <see cref="PerCpuState"/>.
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
    /// Dormant until SMP lands: nothing calls it today.
    /// </summary>
    /// <param name="cpuState">CPU asking for work, or offering it.</param>
    /// <param name="allCpuStates">Every CPU the scheduler manages.</param>
    void Balance(PerCpuState cpuState, PerCpuState[] allCpuStates);

    // ========== Dynamic Reconfiguration ==========

    /// <summary>
    /// Change a thread's priority. The meaning is policy-defined: Stride
    /// reads it as a ticket count, a real-time policy would read it as a
    /// priority level. Reached only through
    /// <see cref="SchedulerManager.SetPriority"/>, whose per-CPU spinlock
    /// excludes another caller but not the timer, so this hook must take
    /// <see cref="SchedulerManager.MaskInterrupts"/> itself before touching
    /// the run structure.
    /// </summary>
    /// <param name="cpuState">CPU whose state guards the update.</param>
    /// <param name="thread">Thread to reprioritize.</param>
    /// <param name="priority">New policy-defined priority.</param>
    void SetPriority(PerCpuState cpuState, SchedulerThread thread, long priority);

    /// <summary>
    /// Report a thread's current priority in the same policy-defined units
    /// <see cref="SetPriority"/> takes. Called from thread context with no
    /// guard at all, on every thread snapshot the <c>SchedulerInfo</c> facade
    /// takes, so it must not mutate anything and must mask itself if it reads
    /// something a tick can be rewriting.
    /// </summary>
    /// <param name="thread">Thread to query. This hook receives no <see cref="PerCpuState"/>.</param>
    /// <returns>The thread's priority, or 0 when the policy does not track one for it.</returns>
    long GetPriority(SchedulerThread thread);

    // ========== Diagnostics ==========

    /// <summary>
    /// Report how many threads are waiting in the run structure. Read-only
    /// introspection for the <c>SchedulerInfo</c> facade, called from thread
    /// context with no guard, so guard it yourself with
    /// <see cref="SchedulerManager.MaskInterrupts"/> if a concurrent tick
    /// could be mutating the structure underneath it. Masking inside the hook
    /// makes one call atomic and no more: a caller that counts and then reads
    /// each index needs its own mask around the whole walk, or the tick can
    /// rotate the queue between the calls.
    /// </summary>
    /// <param name="cpuState">CPU to inspect.</param>
    /// <returns>The number of queued threads.</returns>
    int GetRunQueueCount(PerCpuState cpuState);

    /// <summary>
    /// Report the queued thread at a position. Same calling context and same
    /// guarding duty as <see cref="GetRunQueueCount"/>.
    /// </summary>
    /// <param name="cpuState">CPU to inspect.</param>
    /// <param name="index">Queue position, from 0 to <see cref="GetRunQueueCount"/> exclusive.</param>
    /// <returns>The queued thread, or <see langword="null"/> when the index is out of range.</returns>
    SchedulerThread? GetRunQueueThread(PerCpuState cpuState, int index);
}
