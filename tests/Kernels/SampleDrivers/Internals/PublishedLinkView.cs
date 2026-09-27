// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace SampleDrivers;

/// <summary>
/// The network device the kit registers with the network manager for a
/// <see cref="NetworkLink"/>, an internal <c>PublishedNetworkDevice</c>, as
/// the withdrawal cells drive and observe it: its state, the stack's send
/// path into the driver's transmit handler, and whether the stack still
/// hands it frames. The race cells also create such devices directly, to
/// withdraw one while another thread uses it. Reached through the helpers
/// described on <see cref="KitInternals"/>, each of which throws should its
/// target move.
/// </summary>
public sealed class PublishedLinkView
{
    private const string PublishedNetworkDeviceType = "Cosmos.Kernel.HAL.DriverKit.Engine.PublishedNetworkDevice, Cosmos.Kernel.HAL";
    private const string NetworkDeviceType = "Cosmos.Kernel.HAL.Devices.Network.NetworkDevice, Cosmos.Kernel.HAL";
    private const string PacketReceivedHandlerType = "Cosmos.Kernel.HAL.Interfaces.Devices.PacketReceivedHandler, Cosmos.Kernel.HAL.Interfaces";

    /// <summary>
    /// The internal device itself, for a helper that hands it to a manager
    /// or stack member taking the device's interface, and to compare two
    /// views by.
    /// </summary>
    public object Device { get; }

    /// <summary>True once the kit withdrew the link: on unplug, before the manager lets go of it. Any context.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the property.</exception>
    public bool IsWithdrawn => GetIsWithdrawn(Device);

    /// <summary>What the device tells the stack about carrying traffic. Any context.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the property.</exception>
    public bool Ready => GetReady(Device);

    /// <summary>True while the stack has a receive handler set on the device. Any context.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the property.</exception>
    public bool HasReceiveHandler => GetOnPacketReceived(Device) is not null;

    /// <summary>Views the device behind <paramref name="link"/>.</summary>
    /// <param name="link">A network link a driver published.</param>
    /// <exception cref="MissingMethodException">The link no longer exposes its device.</exception>
    public PublishedLinkView(NetworkLink link)
    {
        Device = GetDevice(link);
    }

    private PublishedLinkView(object device)
    {
        Device = device;
    }

    /// <summary>
    /// Creates a device the way the kit does for
    /// <see cref="DeviceContext.PublishNetworkLink"/>, but registered with
    /// no manager: queued until <see cref="Initialize"/>. Thread context.
    /// </summary>
    /// <param name="context">The binding the device names itself after.</param>
    /// <param name="address">The device's MAC address.</param>
    /// <param name="transmit">What its sends reach, with interrupts masked.</param>
    /// <returns>A view of the new device.</returns>
    /// <exception cref="MissingMethodException">The device's constructor changed.</exception>
    public static PublishedLinkView Create(DeviceContext context, MACAddress address, NetworkTransmitHandler transmit) =>
        new(NewDevice(context, address, transmit));

    /// <summary>Makes a queued device live, as the kit does once the binding is Bound. Any context.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the method.</exception>
    public void Initialize() => InitializeDevice(Device);

    /// <summary>Withdraws a live device, the kit's first unplug step for it. Any context.</summary>
    /// <exception cref="MissingMethodException">The device no longer has the method.</exception>
    public void Withdraw() => WithdrawDevice(Device);

    /// <summary>
    /// Sends a frame the way the stack does, through the device and on to
    /// the driver's transmit handler while the device carries traffic.
    /// Thread context.
    /// </summary>
    /// <param name="data">The frame.</param>
    /// <param name="length">How many bytes of <paramref name="data"/> to send.</param>
    /// <returns>What the device answers the stack: false once it no longer carries traffic.</returns>
    /// <exception cref="MissingMethodException">The device no longer has the method.</exception>
    public bool Send(byte[] data, int length) => SendThroughDevice(Device, data, length);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Device")]
    [return: UnsafeAccessorType(PublishedNetworkDeviceType)]
    private static extern object GetDevice(NetworkLink link);

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(PublishedNetworkDeviceType)]
    private static extern object NewDevice(DeviceContext context, MACAddress address, NetworkTransmitHandler transmit);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_IsWithdrawn")]
    private static extern bool GetIsWithdrawn([UnsafeAccessorType(PublishedNetworkDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_Ready")]
    private static extern bool GetReady([UnsafeAccessorType(PublishedNetworkDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_OnPacketReceived")]
    [return: UnsafeAccessorType(PacketReceivedHandlerType)]
    private static extern object? GetOnPacketReceived([UnsafeAccessorType(NetworkDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Initialize")]
    private static extern void InitializeDevice([UnsafeAccessorType(PublishedNetworkDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Withdraw")]
    private static extern void WithdrawDevice([UnsafeAccessorType(PublishedNetworkDeviceType)] object device);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Send")]
    private static extern bool SendThroughDevice([UnsafeAccessorType(PublishedNetworkDeviceType)] object device, byte[] data, int length);
}
