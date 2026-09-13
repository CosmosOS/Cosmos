using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A condition variable primitive for coordinating scheduler threads around state changes.
/// </summary>
internal class ConditionVariable : IDisposable
{
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
        while (currentThread == null);

        Serial.WriteString("[CV] Wait BEGIN thread=");
        Serial.WriteNumber(currentThread.Id);
        Serial.WriteString("\n");

        using (_lockGuard.AcquireIrqSafe())
        {
            if (!ContainsWaiterLocked(currentThread))
            {
                _waitingThreads.Add(currentThread);
            }

            mutex.Release();
            SchedulerManager.BlockThread(currentThread.CpuId, currentThread);
        }

        // Only park the CPU while still Blocked (same rationale as Mutex.Acquire): if a Signal
        // already readied this thread between scope-dispose and this point, halting would sleep
        // past the wake-up until an unrelated interrupt.
        if (currentThread.State == SchedulerThreadState.Blocked)
        {
            InternalCpu.Halt();
        }

        Serial.WriteString("[CV] Wait WOKE thread=");
        Serial.WriteNumber(currentThread.Id);
        Serial.WriteString("\n");

        // Reacquire the mutex before returning
        mutex.Acquire();

        Serial.WriteString("[CV] Wait END thread=");
        Serial.WriteNumber(currentThread.Id);
        Serial.WriteString("\n");
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
        if (currentThread == null)
        {
            return false;
        }

        Serial.WriteString("[CV] WaitTimeout BEGIN thread=");
        Serial.WriteNumber(currentThread.Id);
        Serial.WriteString(" timeoutMs=");
        Serial.WriteNumber(timeoutMs);
        Serial.WriteString("\n");

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

        if (currentThread.State == SchedulerThreadState.Sleeping)
        {
            InternalCpu.Halt();
        }

        Serial.WriteString("[CV] WaitTimeout WOKE thread=");
        Serial.WriteNumber(currentThread.Id);
        Serial.WriteString("\n");

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

                Serial.WriteString("[CV] Signal -> ReadyThread id=");
                Serial.WriteNumber(waitingThread.Id);
                Serial.WriteString("\n");

                // Wake the thread by marking it ready
                SchedulerManager.ReadyThread(waitingThread.CpuId, waitingThread);
            }
            else
            {
                Serial.WriteString("[CV] Signal (no waiters)\n");
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
