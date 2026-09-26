// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Drivers.Engine;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Drivers;

/// <summary>
/// A network interface a driver published through
/// <see cref="DeviceContext.PublishNetworkLink"/>: the kernel's network stack
/// sends through the driver's <see cref="NetworkTransmitHandler"/>, and the
/// driver hands it every frame it receives through <see cref="Deliver"/>.
/// The link joins the network manager once the driver's Probe returned
/// Bound, after the devices the built-in drivers registered, so a built-in
/// device that registered stays the primary one.
/// </summary>
internal sealed class NetworkLink
{
    private readonly PublishedNetworkDevice _device;

    /// <summary>The MAC address the link was published with, which the stack sends from.</summary>
    public MACAddress Address => _device.MacAddress;

    /// <summary>The adapter the kit delivers to the network manager, and withdraws from it when a USB device leaves.</summary>
    internal PublishedNetworkDevice Device => _device;

    /// <summary>Puts a link in front of <paramref name="device"/>, the adapter the kit delivers to the network manager.</summary>
    internal NetworkLink(PublishedNetworkDevice device)
    {
        _device = device;
    }

    /// <summary>
    /// Hands one received Ethernet frame to the network stack. Thread
    /// context only, typically a <see cref="DeviceWorkItem"/> the interrupt
    /// handler scheduled: it copies the frame into a new array, which the
    /// stack keeps, and runs the stack's receive path with interrupts
    /// masked, as the built-in drivers' interrupt handlers do. A frame that
    /// arrives while the stack has not configured the link, before the
    /// driver's Probe returned Bound, on the link of an attempt that was
    /// declined or failed, or once the kit withdrew the link because its USB
    /// device left the bus, is dropped.
    /// </summary>
    /// <param name="frame">The frame, from the destination MAC address to the end of the payload, without the CRC.</param>
    public void Deliver(ReadOnlySpan<byte> frame) => _device.Deliver(frame);

    /// <summary>
    /// Records whether the link to the network is up, which the network
    /// manager reports. IRQ-safe: the driver's interrupt handler may call it
    /// when the device signals a link change.
    /// </summary>
    /// <param name="isUp">True when the device reports a link.</param>
    public void SetLinkState(bool isUp) => _device.SetLinkState(isUp);
}
