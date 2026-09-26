// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// Base class of a USB class driver a kernel registers. The kit creates one
/// instance per interface it offers the driver, through the registration's
/// factory, and calls <see cref="Probe"/> on it once. A bound instance
/// lives until its device leaves the bus, when the kit calls
/// <see cref="Remove"/>. The kit offers a driver only the interfaces no
/// built-in class driver (hub, keyboard, mass storage) took; neither the
/// host controller nor the built-ins know the driver exists.
/// </summary>
internal abstract class UsbDriver
{
    /// <summary>
    /// Decides whether this driver takes the interface behind
    /// <paramref name="context"/> and, if it does, brings it up: control
    /// requests, the interrupt and bulk endpoints it opens, and what it
    /// publishes. Runs in thread context with interrupts on, on a thread the
    /// driver must not depend on: the boot thread for a device present at
    /// boot, the USB hot-plug thread for one plugged in later. It may
    /// allocate, run synchronous transfers and busy-wait
    /// (<see cref="DeviceContext.Delay"/>), but must not sleep, nor register
    /// drivers.
    /// </summary>
    /// <param name="context">The interface on offer and everything the driver may open for it.</param>
    /// <returns>
    /// <see cref="ProbeResult.Bound"/> to keep the interface. Anything else,
    /// or an exception, makes the kit release what the probe acquired. The
    /// interface then goes to the next candidate, unless the probe opened an
    /// endpoint: the host controller cannot close one, so that interface is
    /// offered to no other driver and stays without one.
    /// </returns>
    protected internal abstract ProbeResult Probe(UsbDeviceContext context);

    /// <summary>
    /// Called once the device of an interface this driver bound has left the
    /// bus, on the USB hot-plug thread. By then the context reports
    /// <see cref="DeviceContext.IsPresent"/> false, the kit has stopped its
    /// report handlers and withdrawn what it published, its work items no
    /// longer run and its events are cancelled, a work item that was running
    /// when the device left has returned (or ran a second without
    /// returning, which the kit logs), and every transfer fails with
    /// <see cref="UsbTransferStatus.Disconnected"/>. A driver overrides it to
    /// let go of what the kit does not know about, such as a disk it
    /// registered with the storage manager; it must not acquire anything,
    /// nor register drivers. An exception is logged with the driver's name
    /// and the interface's path. Once it returns, the binding is over: the
    /// context keeps answering Disconnected.
    /// </summary>
    /// <param name="context">The binding's context, no longer present.</param>
    protected internal virtual void Remove(UsbDeviceContext context)
    {
        // Empty on purpose: a driver that published only through the
        // context has nothing left to undo, since the kit withdraws its
        // publications before calling this.
    }
}
