// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// One device as the driver pass left it, or as the USB stack last changed
/// it: where it sits, which driver owns it, and the IDs drivers match on.
/// A PCI function is one device, and so is each interface of a USB device.
/// The engine's device list is made of these, built-in drivers' devices
/// included.
/// </summary>
internal readonly struct DeviceRecord
{
    /// <summary>Where the device sits, such as <c>pci/0000:00:04.0</c> or <c>usb/1-2.1:1.0</c>.</summary>
    public string Path { get; }

    /// <summary>
    /// The name of the driver that owns the device, built-in or registered;
    /// <c>gop</c> for a display function only reserved as the boot display;
    /// null when nothing owns it.
    /// </summary>
    public string? DriverName { get; }

    /// <summary>The vendor ID.</summary>
    public ushort VendorId { get; }

    /// <summary>The device ID; for a USB interface, its device's product ID.</summary>
    public ushort DeviceId { get; }

    /// <summary>The base class code; for a USB interface, the interface's class.</summary>
    public byte Class { get; }

    /// <summary>The subclass code; for a USB interface, the interface's subclass.</summary>
    public byte Subclass { get; }

    /// <summary>The protocol: a PCI function's programming interface, or a USB interface's protocol.</summary>
    public byte Protocol { get; }

    internal DeviceRecord(string path, string? driverName, ushort vendorId, ushort deviceId, byte baseClass, byte subclass, byte protocol)
    {
        Path = path;
        DriverName = driverName;
        VendorId = vendorId;
        DeviceId = deviceId;
        Class = baseClass;
        Subclass = subclass;
        Protocol = protocol;
    }
}
