// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.Engine;

namespace Cosmos.Kernel.System.Drivers;

/// <summary>
/// One device as the driver kit last saw it: a PCI function, or one
/// interface of a configured USB device, with the driver that owns it and
/// the IDs drivers match on. <see cref="DriverManager.Devices"/> lists them,
/// the devices built-in drivers own included. A plain value: every member
/// can be read in any context. Only the kit creates one; a <c>default</c>
/// value has an empty <see cref="Path"/> and describes no device.
/// </summary>
[Experimental(Cosmos.Kernel.HAL.Drivers.Experimentals.DriverKitDiagId)]
public readonly struct DeviceInfo
{
    /// <summary>Null only in a <c>default</c> value, which <see cref="Path"/> reads as empty.</summary>
    private readonly string? _path;

    /// <summary>
    /// Where the device sits, as drivers see it in their context's Path:
    /// <c>pci/0000:00:04.0</c> for a PCI function (segment, bus, device and
    /// function, in hexadecimal) or <c>usb/1-2.1:1.0</c> for a USB interface
    /// (host controller, the root port and each hub port below it,
    /// configuration value and interface number, in decimal).
    /// </summary>
    public string Path => _path ?? string.Empty;

    /// <summary>
    /// The name of the driver that owns the device: a registered driver's
    /// registration name, or a built-in driver's name, such as <c>xhci</c>,
    /// <c>nvme</c> or <c>virtio-net</c> for a PCI function and <c>hub</c>,
    /// <c>HID boot keyboard</c> or <c>mass storage</c> for a USB interface.
    /// <c>gop</c> for the display function reserved as the boot display: no
    /// driver owns it yet, and no registered driver is ever offered it. Null
    /// when no driver owns the device.
    /// </summary>
    public string? DriverName { get; }

    /// <summary>The vendor ID: a PCI function's, or the USB device's idVendor.</summary>
    public ushort VendorId { get; }

    /// <summary>The device ID: a PCI function's, or, for a USB interface, its device's idProduct.</summary>
    public ushort DeviceId { get; }

    /// <summary>A PCI function's base class code, or a USB interface's bInterfaceClass.</summary>
    public byte Class { get; }

    /// <summary>A PCI function's subclass code, or a USB interface's bInterfaceSubClass.</summary>
    public byte Subclass { get; }

    /// <summary>A PCI function's programming interface, or a USB interface's bInterfaceProtocol.</summary>
    public byte Protocol { get; }

    /// <summary>Copies one record of the engine's device list.</summary>
    internal DeviceInfo(DeviceRecord record)
    {
        _path = record.Path;
        DriverName = record.DriverName;
        VendorId = record.VendorId;
        DeviceId = record.DeviceId;
        Class = record.Class;
        Subclass = record.Subclass;
        Protocol = record.Protocol;
    }
}
