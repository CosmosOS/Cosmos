// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// A USB class driver. <see cref="UsbManager"/> offers every interface of a
/// newly configured device to each registered driver in turn; the first one
/// whose <see cref="TryBind"/> returns true owns the interface. Adding
/// support for a new kind of device (mouse, mass storage, ...) means
/// deriving from this class and registering it in
/// <see cref="UsbManager.Initialize"/>.
/// </summary>
internal abstract class UsbDriver
{
    /// <summary>Short name used in enumeration logs.</summary>
    public abstract string Name { get; }

    /// <summary>
    /// Claims <paramref name="usbInterface"/> of <paramref name="device"/>
    /// when this driver supports it. Runs in thread context during
    /// enumeration, after the device's configuration was selected.
    /// </summary>
    /// <returns><see langword="true"/> when the driver now owns the interface.</returns>
    public abstract bool TryBind(UsbDevice device, UsbInterface usbInterface);
}
