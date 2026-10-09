// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Per-thread allocation context (Thread-Local Allocation Buffer).
/// Stored inline on each <see cref="Scheduler.SchedulerThread"/> to provide contention-free allocation.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal unsafe struct AllocContext
{
    /// <summary>
    /// Current allocation pointer within the TLAB. Advances toward <see cref="AllocLimit"/>.
    /// </summary>
    public byte* AllocPtr;

    /// <summary>
    /// End of the TLAB buffer. When <see cref="AllocPtr"/> reaches this, a refill is needed.
    /// </summary>
    public byte* AllocLimit;

    /// <summary>
    /// Cumulative bytes allocated by this thread (SOH objects).
    /// </summary>
    public ulong AllocBytes;

    /// <summary>
    /// Cumulative bytes allocated by this thread (pinned/LOH objects).
    /// </summary>
    public ulong AllocBytesUoh;
}
