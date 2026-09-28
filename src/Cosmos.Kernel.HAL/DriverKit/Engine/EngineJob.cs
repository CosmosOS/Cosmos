// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.HAL.DriverKit.Engine;

/// <summary>
/// One entry of the kit worker's queue. Node jobs are allocated in thread
/// context by whoever publishes or retracts, and carry a completion their
/// caller may wait on. A work item owns one job for its whole life, so
/// scheduling it from interrupt context links a preallocated object and
/// allocates nothing. The queue link and the queued flag belong to the
/// engine and change only under its lock.
/// </summary>
internal sealed class EngineJob
{
    internal EngineJob(EngineJobKind kind)
    {
        Kind = kind;
    }

    /// <summary>What to do.</summary>
    public EngineJobKind Kind { get; }

    /// <summary>The node, for node jobs.</summary>
    public DeviceNode? Node { get; init; }

    /// <summary>For a retraction: whether the hardware is still present.</summary>
    public bool HardwarePresent { get; init; }

    /// <summary>The item, for work item jobs.</summary>
    public WorkItem? Item { get; init; }

    /// <summary>Signaled by the worker once the job ran; null when nobody waits.</summary>
    public InterruptEvent? Completion { get; init; }

    /// <summary>Next job in the queue. Engine lock.</summary>
    internal EngineJob? Next { get; set; }

    /// <summary>True while linked in the queue. Engine lock.</summary>
    internal bool IsQueued { get; set; }
}
