// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Engine;

/// <summary>
/// One device as the driver pass left it: where it sits, which driver owns
/// it, and the IDs drivers match on. The engine's device list is made of
/// these, built-in drivers' devices included.
/// </summary>
internal readonly struct DeviceRecord
{
    /// <summary>Where the device sits, such as <c>pci/0000:00:04.0</c>.</summary>
    public string Path { get; }

    /// <summary>
    /// The name of the driver that owns the device, built-in or registered;
    /// <c>gop</c> for a display function only reserved as the boot display;
    /// null when nothing owns it.
    /// </summary>
    public string? DriverName { get; }

    /// <summary>The vendor ID.</summary>
    public ushort VendorId { get; }

    /// <summary>The device ID.</summary>
    public ushort DeviceId { get; }

    /// <summary>The base class code.</summary>
    public byte Class { get; }

    /// <summary>The subclass code.</summary>
    public byte Subclass { get; }

    /// <summary>The protocol: a PCI function's programming interface.</summary>
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
