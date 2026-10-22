using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A condition variable primitive for coordinating scheduler threads around state changes.
/// </summary>
internal class ConditionVariable : IDisposable
{
    private const long MillisecondsPerSecond = 1000;

    /// <summary>
    /// Protects access to the condition variable internal state.
    /// </summary>
    private SpinLock _lockGuard;

    /// <summary>
    /// Threads currently waiting for the condition.
    /// </summary>
    private readonly List<SchedulerThread> _waitingThreads;

    /// <summary>
    /// Gets the number of waiting threads.
    /// </summary>
    public int WaitingThreadCount => _waitingThreads.Count;

    /// <summary>
    /// Creates a new condition variable instance.
    /// </summary>
    public ConditionVariable()
    {
        _waitingThreads = [];
    }

    /// <summary>
    /// Waits for a condition to be signaled. The associated mutex must be held by the calling thread.
    /// </summary>
    /// <param name="mutex">The mutex protecting the condition.</param>
    /// <remarks>
    /// This method releases the mutex while the thread is blocked and re-acquires it before returning.
    /// Waiter-list insertion, the mutex release, and the park form one IRQ-off section so a Signal
    /// can only ever observe a fully parked thread. The mutex must be released before BlockThread
    /// (a Blocked thread still holding it could never be signaled), but inside the scope: releasing
    /// with interrupts enabled re-opens the lost-wakeup window (#357) — the release hands the mutex
    /// to a waiter and requests a reschedule, so the very next IRQ exit can run the new owner, whose
    /// Signal readies this still-Running thread; the subsequent BlockThread then buries the wakeup
    /// forever. Same shape as Mutex.Acquire / InterruptEvent.WaitCore (eab7e338).
    /// </remarks>
    public void Wait(Mutex mutex)
    {
        SchedulerThread? currentThread;

        do
        {
            currentThread = SchedulerManager.CurrentCpuState?.CurrentThread;
        }
        while (currentThread is null);

        if (currentThread.IsIdle)
        {
            WaitPolling(mutex, currentThread, deadline: 0);
            return;
        }

        using (_lockGuard.AcquireIrqSafe())
        {
            if (!ContainsWaiterLocked(currentThread))
            {
                _waitingThreads.Add(currentThread);
            }

            mutex.Release();
            SchedulerManager.BlockThread(currentThread.CpuId, currentThread);
        }

        // Give the CPU to the next thread until a Signal readies this one (same as Mutex.Acquire);
        // returns at once when a Signal already landed between scope-dispose and this point.
        SchedulerManager.Park(currentThread);

        // Reacquire the mutex before returning
        mutex.Acquire();
    }

    /// <summary>
    /// Waits for a condition with a timeout.
    /// </summary>
    /// <param name="mutex">The mutex protecting the condition.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>true if signaled, false if timeout occurred.</returns>
    public bool WaitTimeout(Mutex mutex, uint timeoutMs)
    {
        SchedulerThread? currentThread = SchedulerManager.CurrentCpuState?.CurrentThread;
        if (currentThread is null)
        {
            return false;
        }

        if (currentThread.IsIdle)
        {
            // At least one Stopwatch tick, so a 0 ms wait still reads as timed.
            long deadline = Stopwatch.GetTimestamp() + Math.Max(1, Stopwatch.Frequency / MillisecondsPerSecond * timeoutMs);
            return WaitPolling(mutex, currentThread, deadline);
        }

        // Same atomic insert+release+park section as Wait — the old order (release, then
        // insert) additionally lost any Signal landing between the two: it found no waiters.
        using (_lockGuard.AcquireIrqSafe())
        {
            if (!ContainsWaiterLocked(currentThread))
            {
                _waitingThreads.Add(currentThread);
            }

            mutex.Release();
            SchedulerManager.MarkSleeping(currentThread.CpuId, currentThread, timeoutMs);
        }

        SchedulerManager.Park(currentThread);

        // Signaled iff Signal/SignalAll removed this thread from the wait list before the
        // timeout fired. On timeout the stale entry must be removed here, or a later Signal
        // would consume it instead of waking a real waiter.
        bool signaled;
        using (_lockGuard.AcquireIrqSafe())
        {
            signaled = !ContainsWaiterLocked(currentThread);
            if (!signaled)
            {
                RemoveWaiterLocked(currentThread);
            }
        }

        currentThread.WakeupTime = 0;

        // Reacquire the mutex before returning
        mutex.Acquire();

        return signaled;
    }

    /// <summary>
    /// The wait of an idle-thread caller: it stays runnable and polls the waiter list with
    /// interrupts enabled between checks, the rule Mutex and InterruptEvent follow. The idle
    /// thread is the scheduler's fallback (PickNext ?? IdleThread), so it must never be Blocked
    /// or Sleeping: when the run queue empties the fallback puts it back on the CPU as Running
    /// without a wake, so the ReadyThread that would return its tickets never comes (a later
    /// Signal finds it Running and does nothing). Each such wait then drops the idle thread's
    /// tickets from the stride total for good; once the total reaches zero GlobalPass stops,
    /// the idle thread's pass runs ahead of it while it runs alone, and every thread created
    /// after that starts at GlobalPass and keeps the idle thread off the CPU until its pass
    /// catches up. Signal still finds this thread in the waiter list, and its ReadyThread does
    /// nothing on a thread that is already Running or Ready.
    /// </summary>
    /// <param name="mutex">The mutex protecting the condition, held by the caller.</param>
    /// <param name="currentThread">The calling idle thread.</param>
    /// <param name="deadline">Stopwatch timestamp to give up at, or 0 to wait until signaled.</param>
    /// <returns>true if signaled, false if the deadline passed first.</returns>
    private bool WaitPolling(Mutex mutex, SchedulerThread currentThread, long deadline)
    {
        // Insert and release in one section, as in Wait: a Signal after the release
        // must find this thread in the list.
        using (_lockGuard.AcquireIrqSafe())
        {
            if (!ContainsWaiterLocked(currentThread))
            {
                _waitingThreads.Add(currentThread);
            }

            mutex.Release();
        }

        bool signaled;
        while (true)
        {
            using (_lockGuard.AcquireIrqSafe())
            {
                if (!ContainsWaiterLocked(currentThread))
                {
                    signaled = true;
                    break;
                }

                if (deadline != 0 && Stopwatch.GetTimestamp() >= deadline)
                {
                    RemoveWaiterLocked(currentThread);
                    signaled = false;
                    break;
                }
            }
        }

        mutex.Acquire();

        return signaled;
    }

    /// <summary>
    /// ReferenceEquals scan (not List.Contains) to match the scheduler's convention of
    /// avoiding EqualityComparer&lt;T&gt;.Default in kernel paths (see Mutex, InterruptEvent).
    /// Caller must hold <see cref="_lockGuard"/>.
    /// </summary>
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

    /// <summary>Removes a thread from the wait list. Caller must hold <see cref="_lockGuard"/>.</summary>
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
    /// Signals one waiting thread, if there isn't any waiting thread then do nothing.
    /// </summary>
    public void Signal()
    {
        // IRQ-safe like every other _lockGuard use: Wait holds this lock with interrupts
        // masked, so a plain-acquire holder preempted mid-section would deadlock it.
        using (_lockGuard.AcquireIrqSafe())
        {
            if (_waitingThreads.Count > 0)
            {
                SchedulerThread waitingThread = _waitingThreads[0];
                _waitingThreads.RemoveAt(0);

                // Wake the thread by marking it ready
                SchedulerManager.ReadyThread(waitingThread.CpuId, waitingThread);
            }
        }
    }

    /// <summary>
    /// Signals all waiting threads, if there isn't any waiting thread then do nothing.
    /// </summary>
    public void SignalAll()
    {
        using (_lockGuard.AcquireIrqSafe())
        {
            foreach (SchedulerThread thread in _waitingThreads)
            {
                // Wake each thread by marking it ready
                SchedulerManager.ReadyThread(thread.CpuId, thread);
            }

            _waitingThreads.Clear();
        }
    }

    public void Dispose()
    {
        SignalAll();
        GC.SuppressFinalize(this);
    }
}
