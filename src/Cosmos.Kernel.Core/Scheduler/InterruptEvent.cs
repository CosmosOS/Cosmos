// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.CPU;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// One-shot binary completion event with auto-reset semantics, designed
/// to be signaled from an ISR and waited on from a normal thread.
///
/// <para><see cref="Wait()"/> blocks the caller via <see cref="SchedulerManager.BlockThread"/>
/// until <see cref="Signal"/> is called. <see cref="Signal"/> only walks
/// internal state with the spinlock held and calls
/// <see cref="SchedulerManager.ReadyThread"/> — no allocation, no
/// interface dispatch — so it is safe from interrupt context.</para>
///
/// <para>Modeled after <see cref="Mutex"/> but reduced to the surface
/// drivers actually need (a producer ISR and a consumer thread). Multiple
/// consumers can wait; <see cref="Signal"/> wakes one. Signals are
/// counted, not latched as a single bit: two <see cref="Signal"/>s with
/// two parked waiters wake both, and signals arriving while no consumer
/// waits are consumed one per subsequent <see cref="Wait()"/>.</para>
/// </summary>
internal class InterruptEvent
{
    /// <summary>Initial waiter-list capacity: pre-sized so Wait's first Add doesn't heap-allocate under the IRQ-off spinlock; driver flows park at most one or two waiters.</summary>
    private const int InitialWaiterCapacity = 4;

    private SpinLock _lockGuard;
    private uint _pendingSignals;
    private readonly List<SchedulerThread> _waiters;

    public InterruptEvent()
    {
        _pendingSignals = 0;
        // Pre-sized so Wait's first waiter-list Add doesn't heap-allocate
        // while holding the IRQ-off spinlock the ISR-side Signal spins on
        // (an allocation there can trigger GC, making the IRQ-off window
        // unbounded). More than 4 simultaneous waiters would still grow
        // the list under the lock; driver flows park at most one or two.
        _waiters = new List<SchedulerThread>(InitialWaiterCapacity);
    }

    /// <summary>
    /// Blocks the calling thread until <see cref="Signal"/> is called.
    /// Consumes the latched signal on return (auto-reset).
    /// Without a scheduler thread context (scheduler feature off, or
    /// pre-scheduler boot code) this degrades to halt-and-poll on the
    /// latch instead of blocking, so single-context kernels still get
    /// correct completion semantics.
    /// </summary>
    public void Wait() => WaitCore(0);

    /// <summary>
    /// Bounded variant of <see cref="Wait()"/>: returns false when the wait
    /// loop exhausts <paramref name="maxIterations"/> without consuming a
    /// signal. Iterations are loop passes — IF-enabled latch polls on the
    /// no-context/idle path, interrupt wake-ups on the blocked path — so
    /// this is a hang-breaker for lost device interrupts, not a clock.
    /// </summary>
    public bool Wait(ulong maxIterations) => WaitCore(maxIterations);

    private bool WaitCore(ulong maxIterations)
    {
        ulong iterations = 0;
        SchedulerThread? currentThread = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;
        // The idle thread is the scheduler's fallback (PickNext ?? IdleThread):
        // blocking it only gets it resurrected on the next tick, which re-runs
        // the retry loop and drifts the stride accounting (OnThreadBlocked
        // subtracts tickets that OnThreadReady never added for it). Treat an
        // idle-thread caller — the main kernel thread — like the no-context
        // case and poll the latch instead.
        if (currentThread is not null && (currentThread.Flags & SchedulerThreadFlags.IdleThread) != 0)
        {
            currentThread = null;
        }
        if (currentThread == null)
        {
            // Single execution context: nothing to block, so spin on the
            // latch with interrupts enabled between checks. Deliberately no
            // Halt here — if the signaling ISR fires between the check and
            // a hypothetical Halt, no further interrupt may ever arrive
            // (e.g. a build without a timer) and the CPU would sleep past
            // a latched signal forever.
            while (true)
            {
                using (_lockGuard.AcquireIrqSafe())
                {
                    if (_pendingSignals > 0)
                    {
                        _pendingSignals--;
                        return true;
                    }
                }

                if (maxIterations != 0 && ++iterations >= maxIterations)
                {
                    return false;
                }
            }
        }

        while (true)
        {
            // IRQ-safe: holding the plain spinlock with IF=1 deadlocks
            // when the matching ISR-side Signal fires on the same CPU
            // (single-CPU spinlock against itself).
            using (_lockGuard.AcquireIrqSafe())
            {
                if (_pendingSignals > 0)
                {
                    _pendingSignals--;
                    return true;
                }

                // ReferenceEquals scan (not List.Contains) to match
                // RemoveWaiterLocked and the scheduler's own convention of
                // avoiding EqualityComparer<T>.Default in kernel paths.
                if (!ContainsWaiterLocked(currentThread))
                {
                    _waiters.Add(currentThread);
                }

                // Block while interrupts are still masked by the scope:
                // if the ISR-side Signal fired between the waiter-list
                // insertion and BlockThread, ReadyThread would hit a
                // still-Running thread and the subsequent BlockThread
                // would bury the wakeup forever (lost-wakeup race). With
                // the transition done under the scope, Signal can only
                // observe a genuinely Blocked thread.
                SchedulerManager.BlockThread(currentThread.CpuId, currentThread);
            }

            // Only park the CPU while still Blocked: if a Signal (or an
            // unrelated ReadyThread) raced in between the scope-dispose and
            // this point, the thread is already Ready/Running and halting
            // would sleep it until the next unrelated interrupt instead of
            // retrying the latch immediately. A wake racing in after this
            // check costs at most one timer tick — no worse than the
            // unconditional halt it replaces.
            if (currentThread.State == SchedulerThreadState.Blocked)
            {
                InternalCpu.Halt();
            }
            // On wake, retry: either a Signal targeted us (we were removed
            // from _waiters) or we got readied for another reason; in
            // either case re-check state under the lock.

            if (maxIterations != 0 && ++iterations >= maxIterations)
            {
                using (_lockGuard.AcquireIrqSafe())
                {
                    // Consume a signal that raced in just before giving up,
                    // and otherwise leave the waiter list clean so a later
                    // Signal can't dequeue a thread that is no longer waiting.
                    if (_pendingSignals > 0)
                    {
                        _pendingSignals--;
                        return true;
                    }

                    RemoveWaiterLocked(currentThread);
                }

                return false;
            }
        }
    }

    private bool ContainsWaiterLocked(SchedulerThread thread)
    {
        for (int i = 0; i < _waiters.Count; i++)
        {
            if (ReferenceEquals(_waiters[i], thread))
            {
                return true;
            }
        }

        return false;
    }

    // ReferenceEquals scan on purpose: List<T>.Remove routes through
    // EqualityComparer<T>.Default, which this runtime's scheduler avoids
    // (see StrideScheduler.RemoveThreadFromQueue). Caller holds the lock.
    private void RemoveWaiterLocked(SchedulerThread thread)
    {
        for (int i = 0; i < _waiters.Count; i++)
        {
            if (ReferenceEquals(_waiters[i], thread))
            {
                _waiters.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Signals the event. Latches the signal so the next <see cref="Wait()"/>
    /// consumes it immediately, and wakes one parked waiter (if any).
    /// Latching is required because the woken thread re-enters
    /// <see cref="Wait()"/>'s loop and must see something durable rather
    /// than just "I'm not in the waiters list anymore". Safe to call
    /// from interrupt context.
    /// </summary>
    public void Signal()
    {
        // IRQ-safe acquire — Signal runs in ISR context; using the plain
        // Acquire would deadlock against a same-CPU mainline Wait that is
        // already holding the lock when the ISR fires.
        SchedulerThread? toReady = null;
        using (_lockGuard.AcquireIrqSafe())
        {
            _pendingSignals++;

            if (_waiters.Count > 0)
            {
                toReady = _waiters[0];
                _waiters.RemoveAt(0);
            }
        }

        if (toReady != null)
        {
            SchedulerManager.ReadyThread(toReady.CpuId, toReady);
        }
    }

    /// <summary>
    /// Whether the event currently has pending signals no waiter has
    /// consumed yet. Read without locking; for diagnostics only.
    /// </summary>
    public bool IsSignaled => _pendingSignals > 0;
}
