// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// The device an offered interface belongs to, as its device descriptor
/// declares it (USB 2.0 §9.6.1), with every interface of its active
/// configuration. A driver bound to one interface can read its siblings
/// here, but it drives only its own.
/// </summary>
internal sealed class UsbDeviceInfo
{
    private readonly UsbInterfaceInfo[] _interfaces;

    /// <summary>idVendor.</summary>
    public ushort VendorId { get; }

    /// <summary>idProduct.</summary>
    public ushort ProductId { get; }

    /// <summary>bDeviceClass: 0 when each interface names its own class, as most devices do.</summary>
    public byte Class { get; }

    /// <summary>bDeviceSubClass.</summary>
    public byte Subclass { get; }

    /// <summary>bDeviceProtocol.</summary>
    public byte Protocol { get; }

    /// <summary>The interfaces of the active configuration, alternate setting 0 of each, in descriptor order.</summary>
    public IReadOnlyList<UsbInterfaceInfo> Interfaces => _interfaces;

    /// <summary>Describes <paramref name="device"/> and every interface it enumerated with.</summary>
    internal UsbDeviceInfo(UsbDevice device)
    {
        VendorId = device.VendorId;
        ProductId = device.ProductId;
        Class = device.DeviceClass;
        Subclass = device.DeviceSubclass;
        Protocol = device.DeviceProtocol;

        List<UsbInterface> interfaces = device.Interfaces;
        _interfaces = new UsbInterfaceInfo[interfaces.Count];
        for (int i = 0; i < _interfaces.Length; i++)
        {
            _interfaces[i] = new UsbInterfaceInfo(interfaces[i]);
        }
    }

    /// <summary>The description of <paramref name="usbInterface"/>, which is one of the device's.</summary>
    internal UsbInterfaceInfo Describe(UsbInterface usbInterface)
    {
        for (int i = 0; i < _interfaces.Length; i++)
        {
            if (_interfaces[i].Number == usbInterface.Number)
            {
                return _interfaces[i];
            }
        }

        // The context is only ever built for an interface of the device it
        // describes, so this would be a kit bug, not a driver's.
        throw new InvalidOperationException($"Interface {usbInterface.Number} is not one of the device's.");
    }
}
