// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Synthetic;

/// <summary>
/// A bus with no hardware behind it, for tests: publishes nodes on demand,
/// retracts them, and raises their interrupts. A node may carry a register
/// window backed by one RAM page, so <see cref="DeviceBinding.MapRegisters"/>,
/// <see cref="DeviceBinding.MapRegion"/> and the test can see each other's
/// writes without remapping normal memory as device memory.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public static class SyntheticBus
{
    /// <summary>
    /// Publishes a device. After <see cref="DriverEngine.Start"/>, returns once
    /// the node was offered to the drivers; before it, returns at once and
    /// the start offers the node. Thread context.
    /// </summary>
    /// <param name="key">The device's key; its path is <c>synthetic:key</c>.</param>
    /// <param name="data">Bytes the driver can read through <see cref="SyntheticAccess.Data"/>.</param>
    /// <param name="interruptCount">How many interrupt sources the node has.</param>
    /// <param name="windowBytes">Size of the RAM-backed register window (resource 0), up to one page; zero for none.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="interruptCount"/> is negative or <paramref name="windowBytes"/> is out of range.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public static DeviceNode Publish(string key, byte[] data, int interruptCount = 0, int windowBytes = 0)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(Publish));
        ArgumentOutOfRangeException.ThrowIfNegative(interruptCount);
        ArgumentOutOfRangeException.ThrowIfNegative(windowBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan((ulong)windowBytes, PageAllocator.PageSize, nameof(windowBytes));

        SyntheticPage? page = null;
        DeviceResource[] resources = [];
        if (windowBytes > 0)
        {
            page = SyntheticPage.Allocate();
            resources = [DeviceResource.RamWindow(page.Address, page.PhysicalAddress, (ulong)windowBytes)];
        }

        SyntheticInterruptSource[] interrupts = new SyntheticInterruptSource[interruptCount];
        for (int i = 0; i < interrupts.Length; i++)
        {
            interrupts[i] = new SyntheticInterruptSource(i);
        }

        DeviceNode node = new(new SyntheticIdentity(key), resources, interrupts, new SyntheticAccess(data, page, windowBytes), null)
        {
            BusResource = page,
        };
        DriverEngine.PublishNode(node);
        return node;
    }

    /// <summary>
    /// Takes a device away. Returns once its binding was torn down, under
    /// the same rule as <see cref="Publish"/>. Thread context.
    /// </summary>
    /// <param name="node">A node from <see cref="Publish"/>.</param>
    /// <param name="hardwarePresent">What the driver's <see cref="DetachReason.HardwarePresent"/> says; false, as for a hot-unplug, by default.</param>
    /// <exception cref="ArgumentException"><paramref name="node"/> is not a synthetic device.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler or one of the node's own driver threads.</exception>
    public static void Retract(DeviceNode node, bool hardwarePresent = false)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(Retract));
        if (node.Identity is not SyntheticIdentity)
        {
            throw new ArgumentException("The node is not a synthetic device.", nameof(node));
        }

        DriverEngine.RetractNode(node, hardwarePresent);
    }

    /// <summary>
    /// Returns once every kit job queued before the call has run, and every
    /// node job those queued in turn: an offer a publish caused, the
    /// children a bus driver's probe published, a teardown a retract queued
    /// from a driver, a work item a handler scheduled. The test hook for
    /// asserting after <see cref="RaiseInterrupt"/>; work queued afterwards
    /// that is not a node job, such as periodic items, does not hold it up.
    /// Thread context, not from a driver thread of a binding that is being
    /// torn down.
    /// </summary>
    public static void WaitForQueuedJobs()
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(WaitForQueuedJobs));
        DriverEngine.WaitForQueuedJobs();
    }

    /// <summary>
    /// Raises one of a node's interrupts: runs the connected handler in a
    /// synthetic dispatch. Thread context.
    /// </summary>
    /// <param name="node">A node from <see cref="Publish"/>.</param>
    /// <param name="index">Index into <see cref="DeviceNode.Interrupts"/>.</param>
    /// <returns>False before <see cref="DriverEngine.Start"/>, when no handler is connected, or when the source is masked.</returns>
    /// <exception cref="ArgumentOutOfRangeException">No such interrupt.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public static bool RaiseInterrupt(DeviceNode node, int index)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(RaiseInterrupt));
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, node.Interrupts.Count);

        if (!DriverEngine.IsStarted)
        {
            return false;
        }

        return node.Interrupts[index] is SyntheticInterruptSource source && source.Raise();
    }
}
