// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// One entry of a USB driver's match table: the interfaces a registration
/// is offered. A driver binds one interface at a time, so a
/// <see cref="Device"/> entry matches each interface of that product in
/// turn. When several registrations match an interface, a
/// <see cref="Device"/> entry beats a three-part
/// <see cref="Interface(byte, byte, byte)"/> entry, which beats a two-part
/// <see cref="Interface(byte, byte)"/> entry, which beats a class-only
/// <see cref="Interface(byte)"/> entry, and the earlier registration wins a
/// tie. A <c>default(UsbMatch)</c> matches nothing, and a registration
/// refuses it.
/// </summary>
internal readonly struct UsbMatch
{
    private readonly ushort _vendorId;
    private readonly ushort _productId;
    private readonly byte _interfaceClass;
    private readonly byte _subclass;
    private readonly byte _protocol;

    /// <summary>What this entry compares, and so how it ranks; <see cref="UsbMatchKind.None"/> for a default value.</summary>
    internal UsbMatchKind Kind { get; }

    private UsbMatch(UsbMatchKind kind, ushort vendorId, ushort productId, byte interfaceClass, byte subclass, byte protocol)
    {
        Kind = kind;
        _vendorId = vendorId;
        _productId = productId;
        _interfaceClass = interfaceClass;
        _subclass = subclass;
        _protocol = protocol;
    }

    /// <summary>Matches every interface of the devices with this vendor and product ID.</summary>
    /// <param name="vendorId">idVendor from the device descriptor.</param>
    /// <param name="productId">idProduct from the device descriptor.</param>
    public static UsbMatch Device(ushort vendorId, ushort productId) =>
        new(UsbMatchKind.Device, vendorId, productId, 0, 0, 0);

    /// <summary>Matches the interfaces of this class, whatever their subclass and protocol.</summary>
    /// <param name="interfaceClass">bInterfaceClass, such as 0x03 for HID.</param>
    public static UsbMatch Interface(byte interfaceClass) =>
        new(UsbMatchKind.Class, 0, 0, interfaceClass, 0, 0);

    /// <summary>Matches the interfaces of this class and subclass, whatever their protocol.</summary>
    /// <param name="interfaceClass">bInterfaceClass, such as 0x03 for HID.</param>
    /// <param name="subclass">bInterfaceSubClass, such as 0x01 for a HID boot interface.</param>
    public static UsbMatch Interface(byte interfaceClass, byte subclass) =>
        new(UsbMatchKind.ClassWithSubclass, 0, 0, interfaceClass, subclass, 0);

    /// <summary>Matches the interfaces of this class, subclass and protocol.</summary>
    /// <param name="interfaceClass">bInterfaceClass, such as 0x03 for HID.</param>
    /// <param name="subclass">bInterfaceSubClass, such as 0x01 for a HID boot interface.</param>
    /// <param name="protocol">bInterfaceProtocol, such as 0x02 for a HID boot mouse.</param>
    public static UsbMatch Interface(byte interfaceClass, byte subclass, byte protocol) =>
        new(UsbMatchKind.ClassWithProtocol, 0, 0, interfaceClass, subclass, protocol);

    /// <summary>True when <paramref name="usbInterface"/> of <paramref name="device"/> is one this entry names.</summary>
    internal bool Matches(UsbDevice device, UsbInterface usbInterface) => Kind switch
    {
        UsbMatchKind.Device => device.VendorId == _vendorId && device.ProductId == _productId,
        UsbMatchKind.ClassWithProtocol => usbInterface.Class == _interfaceClass && usbInterface.Subclass == _subclass
            && usbInterface.Protocol == _protocol,
        UsbMatchKind.ClassWithSubclass => usbInterface.Class == _interfaceClass && usbInterface.Subclass == _subclass,
        UsbMatchKind.Class => usbInterface.Class == _interfaceClass,
        _ => false
    };
}
