// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The identity of one interface of a USB device, as the enumeration core
/// read it from the descriptors: where it is (the port path from the host
/// controller down, the interface number), what the device is (vendor,
/// product, device class triplet, speed) and what the interface is (its
/// class triplet). Read once when the device was attached; never changes.
/// Path is <c>usb:&lt;port path&gt;:&lt;interface number&gt;</c>, so a
/// stick on root port 1 of the first controller is <c>usb:1-1:0</c>.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class UsbIdentity : DeviceIdentity
{
    internal UsbIdentity(string portPath, byte interfaceNumber, ushort vendorId, ushort productId, byte deviceClass, byte deviceSubclass,
        byte deviceProtocol, byte interfaceClass, byte interfaceSubclass, byte interfaceProtocol, UsbSpeed speed, byte configurationValue)
    {
        PortPath = portPath;
        InterfaceNumber = interfaceNumber;
        VendorId = vendorId;
        ProductId = productId;
        DeviceClass = deviceClass;
        DeviceSubclass = deviceSubclass;
        DeviceProtocol = deviceProtocol;
        InterfaceClass = interfaceClass;
        InterfaceSubclass = interfaceSubclass;
        InterfaceProtocol = interfaceProtocol;
        Speed = speed;
        ConfigurationValue = configurationValue;
        Address = $"{PortPath}:{InterfaceNumber}";
    }

    /// <summary>The dotted port chain from the host controller down to the device: <c>1-2</c> is root port 2 of controller 1, <c>1-2.1</c> port 1 of the hub on it.</summary>
    public string PortPath { get; }

    /// <summary>bInterfaceNumber of the interface this node is.</summary>
    public byte InterfaceNumber { get; }

    /// <summary>idVendor of the device descriptor.</summary>
    public ushort VendorId { get; }

    /// <summary>idProduct of the device descriptor.</summary>
    public ushort ProductId { get; }

    /// <summary>bDeviceClass of the device descriptor.</summary>
    public byte DeviceClass { get; }

    /// <summary>bDeviceSubClass of the device descriptor.</summary>
    public byte DeviceSubclass { get; }

    /// <summary>bDeviceProtocol of the device descriptor.</summary>
    public byte DeviceProtocol { get; }

    /// <summary>bInterfaceClass of the interface descriptor.</summary>
    public byte InterfaceClass { get; }

    /// <summary>bInterfaceSubClass of the interface descriptor.</summary>
    public byte InterfaceSubclass { get; }

    /// <summary>bInterfaceProtocol of the interface descriptor.</summary>
    public byte InterfaceProtocol { get; }

    /// <summary>The speed the device was attached at.</summary>
    public UsbSpeed Speed { get; }

    /// <summary>bConfigurationValue of the configuration the kit selected, the first one.</summary>
    public byte ConfigurationValue { get; }

    /// <inheritdoc/>
    public override string BusName => "usb";

    /// <inheritdoc/>
    public override string Address { get; }

    /// <inheritdoc/>
    public override string Describe() =>
        $"{VendorId:x4}:{ProductId:x4} class {DeviceClass:x2}.{DeviceSubclass:x2}.{DeviceProtocol:x2} interface {InterfaceNumber} class {InterfaceClass:x2}.{InterfaceSubclass:x2}.{InterfaceProtocol:x2}";
}
