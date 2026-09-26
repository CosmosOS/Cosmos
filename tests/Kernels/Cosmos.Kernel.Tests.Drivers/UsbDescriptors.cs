// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Tests.Drivers;

/// <summary>
/// What the USB cells read from a device's descriptors, and the IDs QEMU's
/// HID devices present: its usb-mouse and usb-tablet share one vendor and
/// product ID and tell each other apart by their interface only.
/// </summary>
internal static class UsbDescriptors
{
    /// <summary>idVendor of QEMU's HID devices.</summary>
    public const ushort QemuHidVendorId = 0x0627;

    /// <summary>idProduct of QEMU's HID devices.</summary>
    public const ushort QemuHidProductId = 0x0001;

    /// <summary>GET_DESCRIPTOR (USB 2.0 §9.4.3).</summary>
    public const byte GetDescriptorRequest = 0x06;

    /// <summary>GET_DESCRIPTOR's value for the device descriptor: type 1 in the high byte, index 0.</summary>
    public const ushort DeviceDescriptorValue = 0x0100;

    /// <summary>bLength of a device descriptor (USB 2.0 §9.6.1).</summary>
    public const int DeviceDescriptorLength = 18;

    /// <summary>bDescriptorType of a device descriptor.</summary>
    public const byte DeviceDescriptorType = 0x01;

    /// <summary>Offset of bDescriptorType in a device descriptor (USB 2.0 §9.6.1).</summary>
    public const int DescriptorTypeOffset = 1;

    /// <summary>Offset of idVendor, little-endian, in a device descriptor.</summary>
    public const int VendorIdOffset = 8;

    /// <summary>Offset of idProduct, little-endian, in a device descriptor.</summary>
    public const int ProductIdOffset = 10;
}
