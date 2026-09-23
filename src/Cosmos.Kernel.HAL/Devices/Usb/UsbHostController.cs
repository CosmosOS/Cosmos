// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// A USB host controller. <see cref="UsbManager"/> drives every controller
/// through this type only: bring the controller up, reset and address each
/// device found on its root ports, and hand the addressed
/// <see cref="UsbDevice"/> to the shared enumeration and class drivers. A
/// new controller type (EHCI, a DWC2 on ARM boards, ...) plugs in by
/// deriving from this class.
/// </summary>
internal abstract class UsbHostController
{
    /// <summary>Short name used as the log prefix, e.g. "xHCI".</summary>
    public abstract string Name { get; }

    /// <summary>
    /// Takes the controller from firmware, resets it and starts it. Throws
    /// <see cref="InvalidOperationException"/> when the controller cannot be
    /// brought up.
    /// </summary>
    public abstract void Initialize();

    /// <summary>
    /// Resets every connected root port and enumerates the device behind it
    /// through <see cref="UsbManager.EnumerateDevice"/>.
    /// </summary>
    public abstract void ProbeRootPorts();

    /// <summary>
    /// Addresses a device that was just reset on <paramref name="port"/> of
    /// <paramref name="parentHub"/> (a root port when null), ready for
    /// control transfers on its default pipe.
    /// </summary>
    /// <returns>The device, or null when addressing failed.</returns>
    public abstract UsbDevice? AddressDevice(UsbDevice? parentHub, byte port, UsbSpeed speed);

    /// <summary>Releases the controller state of a device that will not be used.</summary>
    public abstract void ReleaseDevice(UsbDevice device);

    /// <summary>
    /// Processes pending completions. Only needed where interrupts are not
    /// delivered; safe to call from thread context at any time.
    /// </summary>
    public abstract void Poll();
}
