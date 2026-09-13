namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Lifecycle state of a kernel thread as reported by
/// <see cref="SchedulerInfo"/>.
/// </summary>
public enum KernelThreadState : byte
{
    /// <summary>Created but never scheduled.</summary>
    Created,

    /// <summary>In a run queue, waiting for CPU time.</summary>
    Ready,

    /// <summary>Currently executing on a CPU.</summary>
    Running,

    /// <summary>Waiting on I/O or a synchronization primitive.</summary>
    Blocked,

    /// <summary>In a timed wait.</summary>
    Sleeping,

    /// <summary>Terminated, awaiting cleanup.</summary>
    Dead,
}
