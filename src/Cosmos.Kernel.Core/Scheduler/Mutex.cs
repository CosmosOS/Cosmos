using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A simple mutex primitive for protecting critical sections in the scheduler.
/// </summary>
/// <remarks>
/// This mutex supports recursive locking by the same thread.
/// </remarks>
internal class Mutex : IDisposable
{
    /// <summary>Initial capacity of the wait queue, pre-sized so the first Add in Acquire allocates nothing under the IRQ-off state lock (mirrors InterruptEvent._waiters).</summary>
    private const int InitialWaitQueueCapacity = 4;

    /// <summary>
    /// Protects access to the mutex internal state.
    /// </summary>
    private SpinLock _lockGuard;

    /// <summary>
    /// The thread that currently owns the mutex, or <c>null</c> if unlocked.
    /// </summary>
    private SchedulerThread? _ownerThread;

    /// <summary>
    /// The recursion depth for the owning thread.
    /// </summary>
    private int _recursionDepth;

    /// <summary>
    /// Threads waiting to acquire the mutex.
    /// </summary>
    private readonly List<SchedulerThread> _waitingThreads;

    /// <summary>
    /// Creates a new mutex instance.
    /// </summary>
    public Mutex()
    {
        _ownerThread = null;
        _recursionDepth = 0;
        // Pre-sized for the same reason as InterruptEvent._waiters: the
        // first Add in Acquire happens under the IRQ-off state lock.
        _waitingThreads = new List<SchedulerThread>(InitialWaitQueueCapacity);
    }

    /// <summary>
    /// Acquires the mutex. Blocks if already held by another thread.
    /// Without a scheduler thread context there is a single execution
    /// context and nothing to contend with, so the call is a no-op.
    /// </summary>
    public void Acquire()
    {
        SchedulerThread? currentThread = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;

        if (currentThread == null)
        {
            return;
        }

        // The idle thread is the scheduler's fallback (PickNext ?? IdleThread):
        // blocking it only gets it resurrected on the next tick, which re-runs
        // this loop — every pass calls OnThreadBlocked again and drifts
        // TotalTickets (and Release's ReadyThread would insert the idle
        // thread into the run queue it is supposed to back-stop). Unlike
        // InterruptEvent the context can't be nulled — Release checks
        // ownership — so an idle-thread caller spin-acquires instead:
        // ownership stays real, it just never parks in _waitingThreads.
        // Interrupts stay enabled between attempts, so the timer keeps
        // preempting to the (runnable) holder until it releases.
        if ((currentThread.Flags & SchedulerThreadFlags.IdleThread) != 0)
        {
            while (true)
            {
                using (_lockGuard.AcquireIrqSafe())
                {
                    if (_ownerThread == null)
                    {
                        _ownerThread = currentThread;
                        _recursionDepth = 1;
                        return;
                    }

                    if (_ownerThread == currentThread)
                    {
                        _recursionDepth++;
                        return;
                    }
                }
            }
        }

        bool queued = false;
        while (true)
        {
            // IRQ-safe for two reasons: a holder preempted mid-section
            // would deadlock any other thread spinning on the state lock
            // with interrupts masked, and blocking must be atomic with
            // the wait-queue insertion — a Release() racing between the
            // insertion and BlockThread would ready a still-Running
            // thread whose subsequent BlockThread buries the wakeup
            // forever (lost-wakeup race, same shape as InterruptEvent).
            using (_lockGuard.AcquireIrqSafe())
            {
                if (_ownerThread == currentThread)
                {
                    if (queued)
                    {
                        // Release handed the mutex directly to this thread
                        // while it was parked (depth already set to 1): this
                        // re-entry is the wake-up, not a recursive acquire.
                        return;
                    }

                    _recursionDepth++;
                    return;
                }

                if (_ownerThread == null)
                {
                    if (queued)
                    {
                        // Spurious wake while still queued: claim the free
                        // mutex, but leave the queue clean so a later
                        // Release can't hand ownership to us a second time.
                        RemoveWaiterLocked(currentThread);
                    }

                    _ownerThread = currentThread;
                    _recursionDepth = 1;
                    return;
                }

                // ReferenceEquals scan (not List.Contains) to match the
                // scheduler's convention of avoiding EqualityComparer<T>
                // .Default in kernel paths (see InterruptEvent).
                if (!ContainsWaiterLocked(currentThread))
                {
                    _waitingThreads.Add(currentThread);
                }

                queued = true;
                SchedulerManager.BlockThread(currentThread.CpuId, currentThread);
            }

            // Only park the CPU while still Blocked (same rationale as
            // InterruptEvent.WaitCore): if the hand-off already readied us
            // between scope-dispose and this point, halting would sleep past
            // the wake-up until an unrelated interrupt.
            if (currentThread.State == SchedulerThreadState.Blocked)
            {
                InternalCpu.Halt();
            }
        }
    }

    private bool ContainsWaiterLocked(SchedulerThread thread)
    {
        for (int i = 0; i < _waitingThreads.Count; i++)
        {
            if (ReferenceEquals(_waitingThreads[i], thread))
            {
                return true;
            }
        }

        return false;
    }

    // ReferenceEquals scan for the same reason as ContainsWaiterLocked.
    // Caller holds the lock.
    private void RemoveWaiterLocked(SchedulerThread thread)
    {
        for (int i = 0; i < _waitingThreads.Count; i++)
        {
            if (ReferenceEquals(_waitingThreads[i], thread))
            {
                _waitingThreads.RemoveAt(i);
                return;
            }
        }
    }

    /// <summary>
    /// Tries to acquire the mutex without blocking.
    /// </summary>
    /// <returns>true if acquired, false if held by another thread.</returns>
    public bool TryAcquire()
    {
        SchedulerThread? currentThread = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;

        if (currentThread == null)
        {
            return true;
        }

        using (_lockGuard.AcquireIrqSafe())
        {
            if (_ownerThread == null)
            {
                _ownerThread = currentThread;
                _recursionDepth = 1;
                return true;
            }

            if (_ownerThread == currentThread)
            {
                _recursionDepth++;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Releases the mutex. Must be called by the thread that holds it.
    /// </summary>
    /// <remarks>
    /// Recursive locks must be released the same number of times they were acquired.
    /// </remarks>
    public void Release()
    {
        SchedulerThread? currentThread = SchedulerManager.IsReady
            ? SchedulerManager.CurrentCpuState?.CurrentThread
            : null;

        if (currentThread == null)
        {
            return;
        }

        SchedulerThread? toReady = null;
        using (_lockGuard.AcquireIrqSafe())
        {
            if (_ownerThread != currentThread)
            {
                return; // Error: not the owner
            }

            _recursionDepth--;

            if (_recursionDepth == 0)
            {
                if (_waitingThreads.Count > 0)
                {
                    // Direct hand-off: the dequeued waiter owns the mutex
                    // from this instant, so a running thread can't barge in
                    // during the wake-up latency and send the waiter to the
                    // back of the queue again (repeatable starvation).
                    // Acquire detects the hand-off via its `queued` flag.
                    toReady = _waitingThreads[0];
                    _waitingThreads.RemoveAt(0);
                    _ownerThread = toReady;
                    _recursionDepth = 1;
                }
                else
                {
                    _ownerThread = null;
                }
            }
        }

        if (toReady != null)
        {
            SchedulerManager.ReadyThread(toReady.CpuId, toReady);
        }
    }

    /// <summary>
    /// Gets whether this mutex is currently locked by any thread.
    /// </summary>
    public bool IsLocked
    {
        get
        {
            using (_lockGuard.AcquireIrqSafe())
            {
                return _ownerThread != null;
            }
        }
    }

    /// <summary>
    /// Gets the current owner thread.
    /// </summary>
    public SchedulerThread? OwnerThread
    {
        get
        {
            using (_lockGuard.AcquireIrqSafe())
            {
                return _ownerThread;
            }
        }
    }

    /// <summary>
    /// Gets the number of waiting threads.
    /// </summary>
    public int WaitingThreadCount
    {
        get
        {
            using (_lockGuard.AcquireIrqSafe())
            {
                return _waitingThreads.Count;
            }
        }
    }

    public void Dispose()
    {
        while (true)
        {
            SchedulerThread waitingThread;
            using (_lockGuard.AcquireIrqSafe())
            {
                if (_waitingThreads.Count == 0)
                {
                    break;
                }

                waitingThread = _waitingThreads[0];
                _waitingThreads.RemoveAt(0);
            }

            SchedulerManager.ReadyThread(waitingThread.CpuId, waitingThread);
        }

        _ownerThread = null;
        _recursionDepth = 0;
        GC.SuppressFinalize(this);
    }
}
