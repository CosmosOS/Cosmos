// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Point-in-time snapshot of one device node in the driver kit's tree,
/// produced by <see cref="DriverInfo.TryGetNode"/>. The snapshot is taken
/// without locking the kit, so a node whose offer or teardown is running on
/// the kit worker may read one job stale: its state and the counts that
/// depend on its binding can lag the log by one line.
/// <para>
/// It carries what a monitor can render off an unlocked read: identity,
/// state, the driver that holds it, and counts. The offers themselves are
/// reached by index through <see cref="DriverInfo.TryGetOffer"/>, bounded by
/// <see cref="OfferCount"/>; <see cref="ResourceCount"/> and
/// <see cref="InterruptCount"/> are counts only.
/// </para>
/// </summary>
public readonly struct DeviceNodeInfo
{
    internal DeviceNodeInfo(
        string path,
        string busName,
        string description,
        string? driverName,
        DeviceNodeState state,
        string? parentPath,
        int resourceCount,
        int interruptCount,
        int offerCount,
        int childCount,
        int heldResourceCount,
        int publishedDeviceCount,
        int leakedResourceCount,
        int faultCount,
        string? lastFault)
    {
        Path = path;
        BusName = busName;
        Description = description;
        DriverName = driverName;
        State = state;
        ParentPath = parentPath;
        ResourceCount = resourceCount;
        InterruptCount = interruptCount;
        OfferCount = offerCount;
        ChildCount = childCount;
        HeldResourceCount = heldResourceCount;
        PublishedDeviceCount = publishedDeviceCount;
        LeakedResourceCount = leakedResourceCount;
        FaultCount = faultCount;
        LastFault = lastFault;
    }

    /// <summary>The node's name in the <c>[Drivers]</c> log: bus name, colon, bus address.</summary>
    public string Path { get; }

    /// <summary>The bus the device sits on: <c>synthetic</c>, later <c>pci</c>, <c>usb</c> and the rest.</summary>
    public string BusName { get; }

    /// <summary>
    /// The device's identity in the bus's own words, as the bus describes it
    /// for the log.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Name of the driver bound to the node, or <see langword="null"/> while
    /// the node is pending or unbound. After a retraction it still names the
    /// driver that held the node last.
    /// </summary>
    public string? DriverName { get; }

    /// <summary>Where the node is in its life at the time of the snapshot.</summary>
    public DeviceNodeState State { get; }

    /// <summary>Path of the node this one hangs off, or <see langword="null"/> for a root of the tree.</summary>
    public string? ParentPath { get; }

    /// <summary>Number of memory windows, RAM windows and port ranges the node exposes.</summary>
    public int ResourceCount { get; }

    /// <summary>Number of interrupt sources the node exposes.</summary>
    public int InterruptCount { get; }

    /// <summary>
    /// Number of offers made for the node so far, and the bound for the offer
    /// index of <see cref="DriverInfo.TryGetOffer"/>. Zero while the node is
    /// pending, and zero for an unbound node no driver matched.
    /// </summary>
    public int OfferCount { get; }

    /// <summary>Number of nodes the node's driver published beneath it, retracted ones included.</summary>
    public int ChildCount { get; }

    /// <summary>
    /// Kit resources the node's binding holds right now: register windows,
    /// regions, DMA buffers, interrupt handles, work items, periodic work,
    /// events and threads. Zero unless <see cref="State"/> is
    /// <see cref="DeviceNodeState.Bound"/>: a node with no live binding holds
    /// nothing, and what a retraction could not take back is
    /// <see cref="LeakedResourceCount"/>.
    /// </summary>
    public int HeldResourceCount { get; }

    /// <summary>
    /// Devices the node's binding has published and not withdrawn. Zero unless
    /// <see cref="State"/> is <see cref="DeviceNodeState.Bound"/>.
    /// </summary>
    public int PublishedDeviceCount { get; }

    /// <summary>
    /// Resources a retraction left allocated because a driver thread did not
    /// stop in time: they stay rather than be handed to someone else while
    /// that thread may still touch them. Zero for a clean retraction and for a
    /// node never retracted.
    /// </summary>
    public int LeakedResourceCount { get; }

    /// <summary>How many times an interrupt handler of the node's driver has thrown.</summary>
    public int FaultCount { get; }

    /// <summary>The message of the most recent handler exception, or <see langword="null"/> when none has thrown.</summary>
    public string? LastFault { get; }
}
