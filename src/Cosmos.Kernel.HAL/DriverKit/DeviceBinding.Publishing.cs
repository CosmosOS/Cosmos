// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.DriverKit;

/// <summary>
/// The publishing half of the binding: devices the driver makes available to
/// the ring, and child nodes a bus driver puts in the tree. Both are
/// withdrawn by teardown ahead of everything else the driver holds.
/// </summary>
public sealed partial class DeviceBinding
{
    /// <summary>Publishes a keyboard. The ring's keyboard manager, when present, receives it at once.</summary>
    /// <param name="keyboard">The driver's keyboard contract.</param>
    /// <returns>The sink the driver reports keys to.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public KeyboardSink PublishKeyboard(IKeyboard keyboard) =>
        new(Publish(DeviceKind.Keyboard, keyboard.Name, keyboard, nameof(PublishKeyboard)));

    /// <summary>Publishes a pointer.</summary>
    /// <param name="pointer">The driver's pointer contract.</param>
    /// <returns>The sink the driver reports movement to.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public PointerSink PublishPointer(IPointer pointer) =>
        new(Publish(DeviceKind.Pointer, pointer.Name, pointer, nameof(PublishPointer)));

    /// <summary>Publishes a network interface.</summary>
    /// <param name="network">The driver's interface contract.</param>
    /// <returns>The sink the driver reports frames and link changes to.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public NetworkSink PublishNetwork(INetworkInterface network) =>
        new(Publish(DeviceKind.Network, network.Name, network, nameof(PublishNetwork)));

    /// <summary>Publishes a block device. Block devices report nothing, so there is no sink.</summary>
    /// <param name="device">The driver's block device.</param>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public void PublishBlockDevice(IBlockDevice device) =>
        Publish(DeviceKind.Block, device.Name, device, nameof(PublishBlockDevice));

    /// <summary>Publishes a display under <see cref="IDisplay.Name"/>. The ring's display manager, when present, receives it at once.</summary>
    /// <param name="display">The driver's display contract.</param>
    /// <returns>The sink the driver reports mode changes to.</returns>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public DisplaySink PublishDisplay(IDisplay display) =>
        new(Publish(DeviceKind.Display, display.Name, display, nameof(PublishDisplay)));

    /// <summary>
    /// Puts a device the driver found on its bus into the tree beneath this
    /// node, to be offered to drivers. From the worker (a probe, a work item)
    /// the offer is queued behind the current job; from a driver thread it
    /// completes before this returns. Retracted with this node.
    /// </summary>
    /// <param name="identity">What the bus knows about the device.</param>
    /// <param name="resources">Its windows and port ranges.</param>
    /// <param name="interrupts">Its interrupt sources.</param>
    /// <param name="access">The bus's access object for it, or null.</param>
    /// <exception cref="InvalidOperationException">The binding is being torn down, or the caller is an interrupt handler.</exception>
    public DeviceNode PublishChild(DeviceIdentity identity, DeviceResource[] resources, InterruptSource[] interrupts, object? access)
    {
        ThrowIfNotThreadContext(nameof(PublishChild));
        DeviceNode child = new(identity, resources, interrupts, access, Node);
        using (_lock.AcquireIrqSafe())
        {
            ThrowIfDetachingLocked(nameof(PublishChild));
            _children.Add(child);
            Node.AddChild(child);
        }

        DriverEngine.PublishNode(child, this);
        return child;
    }

    /// <summary>
    /// Takes a child this node published out of the tree, tearing down its
    /// binding. Same context rule as <see cref="PublishChild"/>.
    /// </summary>
    /// <param name="child">A node from <see cref="PublishChild"/>.</param>
    /// <param name="hardwarePresent">Whether the child's hardware is still there to be quiesced.</param>
    /// <exception cref="ArgumentException"><paramref name="child"/> is not a child of this node.</exception>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public void RetractChild(DeviceNode child, bool hardwarePresent)
    {
        ThrowIfNotThreadContext(nameof(RetractChild));
        if (!ReferenceEquals(child.Parent, Node))
        {
            throw new ArgumentException("The node is not a child of this binding's node.", nameof(child));
        }

        DriverEngine.RetractNode(child, hardwarePresent, this);
    }

    private PublishedDevice Publish(DeviceKind kind, string name, object device, string member)
    {
        ThrowIfNotThreadContext(member);

        // Listed on the binding before any consumer hears of it, so a probe
        // that fails afterwards, or a consumer that throws, leaves nothing
        // published that the unwind does not withdraw.
        PublishedDevice published = DeviceRegistry.Add(kind, name, device, this, DeviceProvenance.Driver);
        bool late;
        using (_lock.AcquireIrqSafe())
        {
            late = _detaching;
            if (!late)
            {
                _devices.Add(published);
            }
        }

        if (late)
        {
            DeviceRegistry.Withdraw(published);
            ThrowDetaching(member);
        }

        try
        {
            DeviceRegistry.Notify(published);
        }
        catch
        {
            DeviceRegistry.Withdraw(published);
            throw;
        }

        DriverLog.Published(Node, Driver, published);
        return published;
    }
}
