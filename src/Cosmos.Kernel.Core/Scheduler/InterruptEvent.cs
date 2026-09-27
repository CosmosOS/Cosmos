// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
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

    /// <summary>The deadline of an untimed wait: a Stopwatch timestamp no clock reaches.</summary>
    private const long NoDeadline = long.MaxValue;

    /// <summary>Longest timed wait, in milliseconds: the most a sleeping thread's wake deadline takes.</summary>
    private const ulong MaxTimeoutMilliseconds = uint.MaxValue;

    private const long MillisecondsPerSecond = 1000;

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
    public void Wait() => WaitCore(NoDeadline);

    /// <summary>
    /// Timed variant of <see cref="Wait()"/>: returns false when
    /// <paramref name="timeout"/> passes without a signal to consume, a
    /// hang-breaker for lost device interrupts. A blocked caller sleeps with
    /// a wake deadline instead of blocking, the way
    /// <see cref="ConditionVariable.WaitTimeout"/> does, so it still gives
    /// the CPU up and the scheduler wakes it on the first tick past the
    /// deadline; the polling caller reads the Stopwatch between polls.
    /// </summary>
    /// <param name="timeout">How long to wait at most: zero looks once, and anything past <see cref="uint.MaxValue"/> milliseconds waits that long.</param>
    /// <returns>True when a signal was consumed; false when the timeout passed first.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is negative.</exception>
    public bool Wait(TimeSpan timeout)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeout, TimeSpan.Zero);
        return WaitCore(DeadlineAfter(timeout));
    }

    private bool WaitCore(long deadline)
    {
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
        if (currentThread is null)
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

                if (Stopwatch.GetTimestamp() >= deadline)
                {
                    return false;
                }
            }
        }

        try
        {
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

                        // A Signal that woke this thread took it off the
                        // list; a deadline, or a wake for another reason,
                        // did not, and a later Signal must not find it there.
                        RemoveWaiterLocked(currentThread);
                        return true;
                    }

                    long now = Stopwatch.GetTimestamp();
                    if (now >= deadline)
                    {
                        RemoveWaiterLocked(currentThread);
                        return false;
                    }

                    // ReferenceEquals scan (not List.Contains) to match
                    // RemoveWaiterLocked and the scheduler's own convention of
                    // avoiding EqualityComparer<T>.Default in kernel paths.
                    if (!ContainsWaiterLocked(currentThread))
                    {
                        _waiters.Add(currentThread);
                    }

                    // Park while interrupts are still masked by the scope:
                    // if the ISR-side Signal fired between the waiter-list
                    // insertion and the park, ReadyThread would hit a
                    // still-Running thread and the park that followed would
                    // bury the wakeup forever (lost-wakeup race). With the
                    // transition done under the scope, Signal can only
                    // observe a genuinely parked thread. A timed wait sleeps
                    // with a wake deadline instead, so the scheduler's tick
                    // wakes it once the deadline passes.
                    if (deadline == NoDeadline)
                    {
                        SchedulerManager.BlockThread(currentThread.CpuId, currentThread);
                    }
                    else
                    {
                        SchedulerManager.MarkSleeping(currentThread.CpuId, currentThread, MillisecondsUntil(now, deadline));
                    }
                }

                // Only park the CPU while still parked: if a Signal (or an
                // unrelated ReadyThread) raced in between the scope-dispose
                // and this point, the thread is already Ready/Running and
                // halting would sleep it until the next unrelated interrupt
                // instead of retrying the latch immediately. A wake racing
                // in after this check costs at most one timer tick — no
                // worse than the unconditional halt it replaces. Halted
                // again for as long as it stays parked: an interrupt that
                // woke the CPU before the scheduler switched this thread out
                // must not send it round the loop, which would park it a
                // second time (the scheduler taking its tickets twice) or
                // return with it still marked parked.
                while (currentThread.State is SchedulerThreadState.Blocked or SchedulerThreadState.Sleeping)
                {
                    InternalCpu.Halt();
                }

                // On wake, retry: a Signal targeted us, the deadline passed,
                // or we got readied for another reason; in every case
                // re-check state under the lock.
            }
        }
        finally
        {
            // Left set by a Signal that woke a timed sleep before its
            // deadline, as ConditionVariable.WaitTimeout clears it.
            currentThread.WakeupTime = 0;
        }
    }

    /// <summary>The Stopwatch timestamp <paramref name="timeout"/> from now, rounded up to the millisecond.</summary>
    private static long DeadlineAfter(TimeSpan timeout)
    {
        ulong milliseconds = (ulong)(timeout.Ticks / TimeSpan.TicksPerMillisecond);
        if (timeout.Ticks % TimeSpan.TicksPerMillisecond != 0)
        {
            milliseconds++;
        }

        milliseconds = Math.Min(milliseconds, MaxTimeoutMilliseconds);
        return Stopwatch.GetTimestamp() + (long)milliseconds * (Stopwatch.Frequency / MillisecondsPerSecond);
    }

    /// <summary>The milliseconds from <paramref name="now"/> to <paramref name="deadline"/>, rounded up, as a sleep takes them.</summary>
    private static uint MillisecondsUntil(long now, long deadline)
    {
        long ticksPerMillisecond = Stopwatch.Frequency / MillisecondsPerSecond;
        ulong milliseconds = (ulong)((deadline - now + ticksPerMillisecond - 1) / ticksPerMillisecond);
        return (uint)Math.Min(milliseconds, MaxTimeoutMilliseconds);
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
        using (_lockGuard.AcquireIrqSafe())
        {
            _pendingSignals++;

            // Readied under the scope, with interrupts masked: released
            // first, a timer tick could wake a timed waiter by its deadline
            // in between, and readying it a second time would queue it twice.
            if (TakeParkedWaiterLocked() is { } toReady)
            {
                SchedulerManager.ReadyThread(toReady.CpuId, toReady);
            }
        }
    }

    /// <summary>
    /// Takes the first waiter off the list that is still parked, blocked or
    /// in a timed sleep. One in any other state was already woken, by its
    /// deadline or for another reason, and readying it again would queue it
    /// twice: it takes itself off the list when it runs. Caller holds
    /// <see cref="_lockGuard"/>.
    /// </summary>
    private SchedulerThread? TakeParkedWaiterLocked()
    {
        for (int i = 0; i < _waiters.Count; i++)
        {
            SchedulerThread waiter = _waiters[i];
            if (waiter.State is SchedulerThreadState.Blocked or SchedulerThreadState.Sleeping)
            {
                _waiters.RemoveAt(i);
                return waiter;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether the event currently has pending signals no waiter has
    /// consumed yet. Read without locking; for diagnostics only.
    /// </summary>
    public bool IsSignaled => _pendingSignals > 0;
}
