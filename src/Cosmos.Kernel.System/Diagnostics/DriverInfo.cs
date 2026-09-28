// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Diagnostic view of the driver kit: whether the engine has started and how
/// it runs, the driver manifest, the device tree with every offer made for
/// each node, and the devices drivers have published. All reads are
/// allocation-free and safe to poll from a monitor loop; the snapshot structs
/// copy references out. Snapshots are taken without locking the kit, so a
/// node or device whose offer, teardown or withdrawal is running on the kit
/// worker may read one job stale.
/// <para>
/// Three shapes report "nothing there", and which one a member uses follows
/// from what it is. A plain read answers with its own empty value, so
/// <see cref="IsStarted"/> and <see cref="HasWorker"/> are false and
/// <see cref="DeviceCount"/> and <see cref="GetTotalHeldResourceCount"/> are 0
/// before the engine starts, with no separate error channel.
/// <see cref="DriverCount"/> and <see cref="NodeCount"/> can be non-zero
/// before then: the manifest registers its drivers ahead of the start, and a
/// bus may publish nodes ahead of it, which sit pending until the start
/// offers them. A read that must hand back a whole snapshot
/// cannot express absence in the snapshot itself, so
/// <see cref="TryGetDriver"/>, <see cref="TryGetNode"/>,
/// <see cref="TryGetOffer"/> and <see cref="TryGetDevice"/> are
/// <c>Try</c> members and the bool carries that answer: false for an index
/// out of range, which before the start is every device and offer index,
/// while a driver or node index can already be valid then, as said above.
/// <see cref="GetTotalHeldResourceCount"/> is a method because it adds the
/// counts up on every call rather than handing back a value the kit holds.
/// Nothing here acts on the kit, so nothing throws.
/// </para>
/// </summary>
public static class DriverInfo
{
    /// <summary>
    /// Whether the driver engine has started: the manifest was logged, the
    /// worker started or inline mode was settled for, and every node
    /// published before then was offered.
    /// </summary>
    public static bool IsStarted => DriverEngine.IsStarted;

    /// <summary>
    /// Whether a kit worker thread runs probes, teardowns and work items.
    /// False before the engine starts and in inline mode, where the thread
    /// that publishes or retracts a node drains the queue itself, work items
    /// cannot be scheduled, and no periodic work runs.
    /// </summary>
    public static bool HasWorker => DriverEngine.HasWorker;

    /// <summary>
    /// Number of drivers in the manifest, and the bound for
    /// <see cref="TryGetDriver"/>. Filled by the generated manifest before
    /// the kernel starts, so it may be non-zero before <see cref="IsStarted"/>.
    /// </summary>
    public static int DriverCount => DriverRegistry.Drivers.Count;

    /// <summary>
    /// Number of nodes ever published to the tree, retracted ones included,
    /// and the bound for the node index of <see cref="TryGetNode"/> and
    /// <see cref="TryGetOffer"/>. Nodes are never removed, so an index stays
    /// valid once it is.
    /// </summary>
    public static int NodeCount => DriverEngine.Nodes.Count;

    /// <summary>
    /// Number of devices currently published by drivers, and the bound for
    /// <see cref="TryGetDevice"/>. A withdrawn device leaves the list, so the
    /// positions after it shift down by one.
    /// </summary>
    public static int DeviceCount => DeviceRegistry.Devices.Count;

    /// <summary>
    /// Kit resources held by every bound node's binding, summed: register
    /// windows, regions, DMA buffers, interrupt handles, work items, periodic
    /// work, events and threads. Nodes in any other state contribute nothing;
    /// what a retraction could not take back is per node, in
    /// <see cref="DeviceNodeInfo.LeakedResourceCount"/>.
    /// </summary>
    public static int GetTotalHeldResourceCount()
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        int total = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            DeviceNode node = nodes[i];
            if (node.State == NodeState.Bound && node.Binding is DeviceBinding binding)
            {
                total += binding.HeldResourceCount;
            }
        }

        return total;
    }

    /// <summary>
    /// Snapshots one entry of the driver manifest.
    /// </summary>
    /// <param name="index">Manifest position, from 0 to <see cref="DriverCount"/> exclusive.</param>
    /// <param name="info">Snapshot of the driver at that position.</param>
    /// <returns><see langword="false"/> when the index is out of range.</returns>
    public static bool TryGetDriver(int index, out DriverEntryInfo info)
    {
        IReadOnlyList<Driver> drivers = DriverRegistry.Drivers;
        if (index < 0 || index >= drivers.Count)
        {
            info = default;
            return false;
        }

        Driver driver = drivers[index];
        info = new DriverEntryInfo(driver.Name, driver.Priority);
        return true;
    }

    /// <summary>
    /// Snapshots one node of the device tree.
    /// </summary>
    /// <param name="index">Publication position, from 0 to <see cref="NodeCount"/> exclusive.</param>
    /// <param name="info">Snapshot of the node at that position.</param>
    /// <returns><see langword="false"/> when the index is out of range.</returns>
    public static bool TryGetNode(int index, out DeviceNodeInfo info)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        if (index < 0 || index >= nodes.Count)
        {
            info = default;
            return false;
        }

        info = Snapshot(nodes[index]);
        return true;
    }

    /// <summary>
    /// Snapshots one offer made for a node. Offers are recorded in the order
    /// they were made and never change afterwards, so a walk from 0 to
    /// <see cref="DeviceNodeInfo.OfferCount"/> replays the node's arbitration.
    /// </summary>
    /// <param name="nodeIndex">Publication position of the node, from 0 to <see cref="NodeCount"/> exclusive.</param>
    /// <param name="offerIndex">Offer position, from 0 to the node's <see cref="DeviceNodeInfo.OfferCount"/> exclusive.</param>
    /// <param name="info">Snapshot of the offer.</param>
    /// <returns><see langword="false"/> when either index is out of range.</returns>
    public static bool TryGetOffer(int nodeIndex, int offerIndex, out DeviceOfferInfo info)
    {
        IReadOnlyList<DeviceNode> nodes = DriverEngine.Nodes;
        if (nodeIndex < 0 || nodeIndex >= nodes.Count)
        {
            info = default;
            return false;
        }

        IReadOnlyList<DeviceOffer> offers = nodes[nodeIndex].Offers;
        if (offerIndex < 0 || offerIndex >= offers.Count)
        {
            info = default;
            return false;
        }

        DeviceOffer offer = offers[offerIndex];
        info = new DeviceOfferInfo(
            offer.DriverName,
            offer.Priority,
            offer.Specificity,
            MapOutcome(offer.Outcome),
            offer.Reason,
            offer.ReleasedResourceCount);
        return true;
    }

    /// <summary>
    /// Snapshots one published device.
    /// </summary>
    /// <param name="index">
    /// Position in the published list, from 0 to <see cref="DeviceCount"/>
    /// exclusive. A withdrawal between the count and this read, or between two
    /// reads, shifts the positions after it, so a walk can miss a device; the
    /// list is for display, not for deciding anything.
    /// </param>
    /// <param name="info">Snapshot of the device at that position.</param>
    /// <returns><see langword="false"/> when the index is out of range.</returns>
    public static bool TryGetDevice(int index, out PublishedDeviceInfo info)
    {
        IReadOnlyList<PublishedDevice> devices = DeviceRegistry.Devices;
        if (index < 0 || index >= devices.Count)
        {
            info = default;
            return false;
        }

        PublishedDevice device = devices[index];
        DeviceBinding? binding = device.Binding;
        info = new PublishedDeviceInfo(
            MapKind(device.Kind),
            device.Name,
            binding?.Node.Path,
            binding?.Driver.Name,
            device.IsConsumed,
            device.IsWithdrawn);
        return true;
    }

    private static DeviceNodeInfo Snapshot(DeviceNode node)
    {
        NodeState state = node.State;
        DeviceBinding? binding = node.Binding;
        int heldResourceCount = 0;
        int publishedDeviceCount = 0;
        if (state == NodeState.Bound && binding is not null)
        {
            heldResourceCount = binding.HeldResourceCount;
            publishedDeviceCount = binding.PublishedDeviceCount;
        }

        return new DeviceNodeInfo(
            node.Path,
            node.Identity.BusName,
            node.Description,
            binding?.Driver.Name,
            MapState(state),
            node.Parent?.Path,
            node.Resources.Count,
            node.Interrupts.Count,
            node.Offers.Count,
            node.Children.Count,
            heldResourceCount,
            publishedDeviceCount,
            node.LeakedResourceCount,
            node.FaultCount,
            node.LastFault);
    }

    private static DeviceNodeState MapState(NodeState state) => state switch
    {
        NodeState.Pending => DeviceNodeState.Pending,
        NodeState.Bound => DeviceNodeState.Bound,
        NodeState.Unbound => DeviceNodeState.Unbound,
        _ => DeviceNodeState.Retracted,
    };

    private static DeviceOfferOutcome MapOutcome(ProbeOutcome outcome) => outcome switch
    {
        ProbeOutcome.Bound => DeviceOfferOutcome.Bound,
        ProbeOutcome.Declined => DeviceOfferOutcome.Declined,
        _ => DeviceOfferOutcome.Failed,
    };

    private static PublishedDeviceKind MapKind(DeviceKind kind) => kind switch
    {
        DeviceKind.Keyboard => PublishedDeviceKind.Keyboard,
        DeviceKind.Pointer => PublishedDeviceKind.Pointer,
        DeviceKind.Network => PublishedDeviceKind.Network,
        DeviceKind.Block => PublishedDeviceKind.Block,
        _ => PublishedDeviceKind.Display,
    };
}
