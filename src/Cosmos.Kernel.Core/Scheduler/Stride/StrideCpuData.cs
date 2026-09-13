namespace Cosmos.Kernel.Core.Scheduler.Stride;

/// <summary>
/// Stride scheduler per-CPU extension data.
/// </summary>
internal class StrideCpuData
{
    /// <summary>
    /// Sum of tickets in run queue.
    /// </summary>
    public ulong TotalTickets { get; internal set; }

    /// <summary>
    /// Global virtual time.
    /// </summary>
    public ulong GlobalPass { get; internal set; }

    /// <summary>
    /// Timestamp of last global pass update.
    /// </summary>
    public ulong LastPassUpdate { get; internal set; }

    /// <summary>
    /// Run queue sorted by Pass value (ascending). Pre-sized to the thread
    /// registry limit: InsertByPass runs in interrupt context (the tick's
    /// OnThreadYield and the sleep-expiry OnThreadReady), and a List growth
    /// there is an allocation inside the tick, the case the plugging guide
    /// forbids. Mutex and InterruptEvent pre-size their wait lists for the
    /// same reason.
    /// </summary>
    public List<SchedulerThread> RunQueue { get; } = new(SchedulerThread.MaxThreadCount);
}
