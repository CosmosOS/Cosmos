using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.Core.Scheduler;

/// <summary>
/// Thread execution state.
/// </summary>
[Experimental(Experimentals.SchedulerSeamDiagId)]
public enum SchedulerThreadState : byte
{
    /// <summary>Just created, not yet scheduled.</summary>
    Created,

    /// <summary>Can be scheduled.</summary>
    Ready,

    /// <summary>Currently executing on a CPU.</summary>
    Running,

    /// <summary>Waiting for I/O, a lock, or another wake signal.</summary>
    Blocked,

    /// <summary>In a timed wait.</summary>
    Sleeping,

    /// <summary>Terminated, awaiting cleanup.</summary>
    Dead
}

/// <summary>
/// Thread flags.
/// </summary>
[Flags]
[Experimental(Experimentals.SchedulerSeamDiagId)]
public enum SchedulerThreadFlags : ushort
{
    /// <summary>
    /// No flags set
    /// </summary>
    None = 0,
    /// <summary>
    /// Per-CPU idle thread
    /// </summary>
    IdleThread = 1 << 1,
    /// <summary>
    /// Cannot migrate to other CPUs
    /// </summary>
    Pinned = 1 << 2,
    /// <summary>
    /// Entrypoint parameter is a <see cref="System.Runtime.InteropServices.GCHandle"/>
    /// of <see cref="System.Threading.Thread"/>. When set, the thread entry
    /// trampoline calls the managed thread start instead of decoding the
    /// parameter as a free delegate. Set by the mechanism; a policy reads it
    /// but never assigns it.
    /// </summary>
    Managed = 1 << 3,
    // Bits 8-15 reserved for scheduler-specific flags
}
