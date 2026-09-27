// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// A host controller's handle on the kit's USB core, one per published
/// controller, which the core hands to its root port callbacks: what the
/// controller reports about its root ports goes through here. The devices
/// behind hubs are the core's own business.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public sealed class UsbBus
{
    /// <summary>
    /// Set while the core runs one of the controller's root port callbacks,
    /// on the one thread that runs them, and read on that thread only.
    /// </summary>
    private bool _inPortCallback;

    /// <summary>The controller this bus is.</summary>
    internal UsbHostController Controller { get; }

    /// <summary>The bus number the device paths carry: the controller's position in delivery order, from 1.</summary>
    internal int Number { get; }

    internal UsbBus(UsbHostController controller, int number)
    {
        Controller = controller;
        Number = number;
    }

    /// <summary>
    /// Enumerates the device just reset on <paramref name="rootPort"/>:
    /// addresses it through the controller's
    /// <see cref="UsbHostController.AddressDevice"/>, reads its descriptors,
    /// selects its first configuration and offers its interfaces to the
    /// class drivers, the hub driver included, which enumerates what hangs
    /// off a hub before this returns. From the controller's
    /// <see cref="UsbHostController.ProbeRootPorts"/> or
    /// <see cref="UsbHostController.HandlePortChanges"/> only.
    /// </summary>
    /// <param name="rootPort">The root port, from 1.</param>
    /// <param name="speed">The speed the port reported.</param>
    /// <returns>True when the device was configured; false when enumeration failed, which the core logs.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rootPort"/> is 0.</exception>
    /// <exception cref="InvalidOperationException">Called outside the controller's root port callbacks.</exception>
    public bool EnumerateDevice(byte rootPort, UsbSpeed speed)
    {
        ThrowIfNotInPortCallback(nameof(EnumerateDevice));
        ArgumentOutOfRangeException.ThrowIfZero(rootPort);
        return UsbManager.EnumerateDevice(this, null, rootPort, speed) is not null;
    }

    /// <summary>
    /// Takes the device enumerated on <paramref name="rootPort"/> off the
    /// bus, and when it is a hub, every device behind it: their transfers
    /// fail from then on, their class drivers let go of them, then the
    /// controller's <see cref="UsbHostController.ReleaseDevice"/> frees each.
    /// Nothing happens when no device was enumerated there. From the
    /// controller's <see cref="UsbHostController.HandlePortChanges"/> only.
    /// </summary>
    /// <param name="rootPort">The root port, from 1.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="rootPort"/> is 0.</exception>
    /// <exception cref="InvalidOperationException">Called outside the controller's root port callbacks.</exception>
    public void DisconnectPort(byte rootPort)
    {
        ThrowIfNotInPortCallback(nameof(DisconnectPort));
        ArgumentOutOfRangeException.ThrowIfZero(rootPort);
        UsbManager.DisconnectPort(this, null, rootPort);
    }

    /// <summary>
    /// Wakes the hot-plug thread, which then calls the controller's
    /// <see cref="UsbHostController.HandlePortChanges"/>: what a controller
    /// calls when one of its root ports changes. Any context, interrupt
    /// handlers included: it allocates nothing and takes an IRQ-safe lock
    /// only. Before the hot-plug thread runs, the change waits for its first
    /// pass.
    /// </summary>
    public void NotifyPortChange() => UsbManager.NotifyPortChange();

    /// <summary>Runs the controller's <see cref="UsbHostController.ProbeRootPorts"/>, inside the window its bus calls are allowed in.</summary>
    internal void ProbeRootPorts()
    {
        _inPortCallback = true;
        try
        {
            Controller.ProbeRootPorts(this);
        }
        finally
        {
            _inPortCallback = false;
        }
    }

    /// <summary>Runs the controller's <see cref="UsbHostController.HandlePortChanges"/>, inside the window its bus calls are allowed in.</summary>
    internal void HandlePortChanges()
    {
        _inPortCallback = true;
        try
        {
            Controller.HandlePortChanges(this);
        }
        finally
        {
            _inPortCallback = false;
        }
    }

    private void ThrowIfNotInPortCallback(string member)
    {
        if (!_inPortCallback)
        {
            throw new InvalidOperationException($"{member} can only be called from the controller's ProbeRootPorts or HandlePortChanges.");
        }
    }
}
