namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Point-in-time snapshot of one kernel thread, produced by
/// <see cref="SchedulerInfo"/>. The snapshot is taken without locking the
/// scheduler, so fields of a thread that is being rescheduled concurrently
/// may be one tick stale.
/// <para>
/// This is a projection of <see cref="global::Cosmos.Kernel.Core.Scheduler.SchedulerThread"/>,
/// the thread control block on the experimental scheduler seam, and it is
/// narrower on purpose. It carries what a monitor can render off an unlocked
/// read: identity, state, and two totals. It leaves behind the saved stack
/// pointer, the entry address, the stack base and the two scheduling
/// timestamps, because each of those is meaningful only in a state the
/// snapshot cannot guarantee it is observing, and a policy that wants them
/// already holds the control block. Two names differ from the seam's, both
/// to state a unit the seam leaves to its own docs:
/// <see cref="TotalRuntimeNs"/> and <see cref="StackSizeBytes"/>.
/// </para>
/// </summary>
public readonly struct KernelThreadInfo
{
    internal KernelThreadInfo(
        uint id,
        uint cpuId,
        KernelThreadState state,
        bool isIdle,
        bool isManaged,
        ulong totalRuntimeNs,
        ulong stackSizeBytes,
        long priority,
        bool hasPriority)
    {
        Id = id;
        CpuId = cpuId;
        State = state;
        IsIdle = isIdle;
        IsManaged = isManaged;
        TotalRuntimeNs = totalRuntimeNs;
        StackSizeBytes = stackSizeBytes;
        Priority = priority;
        HasPriority = hasPriority;
    }

    /// <summary>Unique thread identifier. The idle thread has ID 0.</summary>
    public uint Id { get; }

    /// <summary>CPU the thread is assigned to.</summary>
    public uint CpuId { get; }

    /// <summary>Lifecycle state at the time of the snapshot.</summary>
    public KernelThreadState State { get; }

    /// <summary>Whether this is a per-CPU idle thread.</summary>
    public bool IsIdle { get; }

    /// <summary>
    /// Whether the thread was started from a managed
    /// <see cref="global::System.Threading.Thread"/>.
    /// </summary>
    public bool IsManaged { get; }

    /// <summary>Accumulated CPU time in nanoseconds.</summary>
    public ulong TotalRuntimeNs { get; }

    /// <summary>Size of the thread's stack in bytes.</summary>
    public ulong StackSizeBytes { get; }

    /// <summary>
    /// Scheduling priority as reported by the active scheduler. Only
    /// meaningful when <see cref="HasPriority"/> is set; the
    /// interpretation is scheduler-specific (the default stride scheduler
    /// reports tickets, where higher means more CPU time).
    /// </summary>
    public long Priority { get; }

    /// <summary>
    /// Whether the active scheduler is tracking this thread and reported a
    /// priority for it.
    /// </summary>
    public bool HasPriority { get; }
}
