

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// A simple monitor implementation for kernel threads.
/// </summary>
/// <remarks>
/// This monitor combines a <see cref="Mutex"/> and a <see cref="ConditionVariable"/> to provide
/// acquire/release semantics and wait/signal behavior for the scheduler.
/// </remarks>
internal class Monitor : IDisposable
{
    /// <summary>
    /// Mutex used to protect the monitor's critical section.
    /// </summary>
    private readonly Mutex _mutex;

    /// <summary>
    /// Condition variable used to suspend and wake waiting threads.
    /// </summary>
    private readonly ConditionVariable _condition;

    /// <summary>
    /// Initializes a new instance of the <see cref="Monitor"/> class.
    /// </summary>
    public Monitor()
    {
        _mutex = new Mutex();
        _condition = new ConditionVariable();
    }

    /// <summary>
    /// Finalizes the monitor and ensures unmanaged resources are released.
    /// </summary>
    ~Monitor()
    {
        Dispose();
    }

    /// <summary>
    /// Acquires the monitor lock.
    /// </summary>
    public void Acquire()
    {
        _mutex.Acquire();
    }

    /// <summary>
    /// Releases the monitor lock.
    /// </summary>
    public void Release()
    {
        _mutex.Release();
    }

    /// <summary>
    /// Waits on the condition variable until signaled.
    /// </summary>
    /// <remarks>
    /// The mutex is released while waiting and re-acquired before returning.
    /// </remarks>
    public void Wait()
    {
        _condition.Wait(_mutex);
    }

    /// <summary>
    /// Waits on the condition variable until signaled or the timeout expires.
    /// </summary>
    /// <param name="timeoutMilliseconds">The timeout duration in milliseconds.</param>
    public bool Wait(int timeoutMilliseconds)
    {
        return _condition.WaitTimeout(_mutex, (uint)timeoutMilliseconds);
    }

    /// <summary>
    /// Signals one waiting thread and releases the monitor lock.
    /// </summary>
    public void Signal()
    {
        _condition.Signal();
        _mutex.Release();
    }

    /// <summary>
    /// Signals all waiting threads and releases the monitor lock.
    /// </summary>
    public void SignalAll()
    {
        _condition.SignalAll();
        _mutex.Release();
    }

    /// <summary>
    /// Releases resources used by the monitor.
    /// </summary>
    public void Dispose()
    {
        _condition.Dispose();
        _mutex.Dispose();
        GC.SuppressFinalize(this);
    }
}
