// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The contract a USB host controller driver implements for the kit: how
/// a device on a port gets its address and how its state is freed again.
/// The host driver binds the controller's own node (a PCI function for
/// xHCI), creates a <see cref="UsbBus"/> over this object from its probe,
/// and owns every register, ring, context and DMA page; the kit never
/// publishes a node for the controller itself. A host implements the
/// protected members; the kit calls them through the internal ones, so a
/// class driver holding a reference can reach neither.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public abstract class UsbHostController
{
    /// <summary>Creates the contract half of a host controller.</summary>
    protected UsbHostController()
    {
    }

    /// <summary>A short name for logs, <c>"xHCI"</c>.</summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gives the device on <paramref name="port"/> of <paramref name="parentHub"/>
    /// (null: a root port) its address and default control pipe, reads the
    /// 8-byte head of its device descriptor and sets
    /// <see cref="UsbDevice.MaxPacketSize0"/>. Thread context (a probe, a
    /// hot-plug thread).
    /// </summary>
    /// <param name="parentHub">The hub the port belongs to, or null for a root port.</param>
    /// <param name="port">The 1-based port number on the hub or the controller.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>The addressed device; null when any step failed, the host having logged why and released what it allocated.</returns>
    protected abstract UsbDevice? AddressDeviceCore(UsbDevice? parentHub, byte port, UsbSpeed speed);

    /// <summary>
    /// Frees the host's state for a device the kit no longer tracks. Thread
    /// context. With <paramref name="hostPresent"/> the host first waits out
    /// any transfer still running on the device and tells the controller the
    /// slot is free; without it (the controller's own hardware is gone)
    /// nothing is written and the state is only dropped. Called after every
    /// interface node of the device was torn down: the kit never releases a
    /// device whose nodes still have a live binding (an attach over a stale
    /// state refuses it rather than detaching it from the worker).
    /// </summary>
    /// <param name="device">A device this host addressed.</param>
    /// <param name="hostPresent">Whether the controller is still there to be told.</param>
    protected abstract void ReleaseDeviceCore(UsbDevice device, bool hostPresent);

    /// <summary>Kit side of <see cref="AddressDeviceCore"/>; called by the enumeration core, never by a driver.</summary>
    internal UsbDevice? AddressDevice(UsbDevice? parentHub, byte port, UsbSpeed speed) => AddressDeviceCore(parentHub, port, speed);

    /// <summary>Kit side of <see cref="ReleaseDeviceCore"/>; called by the enumeration core and the bus, never by a driver.</summary>
    internal void ReleaseDevice(UsbDevice device, bool hostPresent) => ReleaseDeviceCore(device, hostPresent);
}
