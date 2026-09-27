// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// A USB host controller, as the kit's USB core drives it. A PCI driver
/// brings the controller up in its Probe and publishes it through
/// <see cref="Pci.PciDeviceContext.PublishUsbHostController"/>; the core
/// then gives it a <see cref="UsbBus"/>, and does the rest itself: it
/// enumerates the devices the controller reports on its root ports, reads
/// their descriptors, runs the hubs behind them and binds the class drivers
/// to their interfaces. The controller only addresses and releases
/// devices, looks at its root ports, and carries the transfers of the
/// <see cref="UsbHostDevice"/>s it created. Its root ports are looked at
/// by one thread at a time: the thread that ran the probe, then the USB
/// hot-plug thread. A new kind of controller (EHCI, a DWC2 on an ARM board)
/// plugs in by deriving from this class; it overrides the callbacks as
/// <c>protected override</c>.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public abstract class UsbHostController
{
    /// <summary>Short name the core prefixes its log lines with, such as <c>xHCI</c>. Any context.</summary>
    public abstract string Name { get; }

    /// <summary>
    /// Whether completions and port changes are only seen through
    /// <see cref="Poll"/>: nothing calls the controller's own handler for
    /// them, so the hot-plug thread polls it every 250 ms instead of waiting
    /// for <see cref="UsbBus.NotifyPortChange"/>. Any context.
    /// </summary>
    public abstract bool IsPolled { get; }

    /// <summary>
    /// Resets every connected root port and enumerates the device behind each
    /// through <see cref="UsbBus.EnumerateDevice"/>. Called once, right after
    /// the kit delivered the published controller, on the thread that ran
    /// the probe and before the driver's interrupts are armed: every transfer
    /// it waits on completes by polling the controller. The controller keeps
    /// <paramref name="bus"/> to report later port changes through
    /// <see cref="UsbBus.NotifyPortChange"/>.
    /// </summary>
    /// <param name="bus">The controller's bus, the same one on every call.</param>
    protected internal abstract void ProbeRootPorts(UsbBus bus);

    /// <summary>
    /// Handles every root port whose connection changed since the last call:
    /// the device that was on it leaves through
    /// <see cref="UsbBus.DisconnectPort"/>, and a device now on it is reset
    /// and enumerated through <see cref="UsbBus.EnumerateDevice"/>. Called on
    /// the hot-plug thread, once it starts and after every
    /// <see cref="UsbBus.NotifyPortChange"/>.
    /// </summary>
    /// <param name="bus">The controller's bus.</param>
    protected internal abstract void HandlePortChanges(UsbBus bus);

    /// <summary>
    /// Processes the completions and port changes nothing reported, for a
    /// controller that <see cref="IsPolled"/>. Called on the hot-plug thread.
    /// </summary>
    protected internal abstract void Poll();

    /// <summary>
    /// Addresses a device that was just reset on <paramref name="port"/>,
    /// ready for control transfers on its default pipe, with its default
    /// pipe's packet size read from the device. Thread context.
    /// </summary>
    /// <param name="parentHub">The hub the device hangs off, a device this controller addressed; null for a root port.</param>
    /// <param name="port">The port on <paramref name="parentHub"/>, or the root port, from 1.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>The device, or null when addressing failed, which the controller logs.</returns>
    protected internal abstract UsbHostDevice? AddressDevice(UsbHostDevice? parentHub, byte port, UsbSpeed speed);

    /// <summary>
    /// Frees the controller state of a device that failed enumeration or
    /// left the bus, once no class driver uses it any more. Marks it
    /// disconnected first and waits out every transfer still running on it,
    /// so no thread is left using what is freed. A device of another
    /// controller is ignored. Thread context.
    /// </summary>
    /// <param name="device">A device <see cref="AddressDevice"/> returned.</param>
    protected internal abstract void ReleaseDevice(UsbHostDevice device);
}
