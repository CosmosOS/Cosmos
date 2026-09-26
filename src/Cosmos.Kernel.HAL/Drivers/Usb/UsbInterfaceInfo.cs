// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// One interface of a device's active configuration, alternate setting 0,
/// as its interface descriptor declares it (USB 2.0 §9.6.5), with its
/// endpoints. It describes; it opens nothing.
/// </summary>
internal sealed class UsbInterfaceInfo
{
    private readonly UsbEndpointInfo[] _endpoints;

    /// <summary>bInterfaceNumber, which a class request addressed to the interface carries in its index.</summary>
    public byte Number { get; }

    /// <summary>bInterfaceClass, such as 0x03 for HID.</summary>
    public byte Class { get; }

    /// <summary>bInterfaceSubClass.</summary>
    public byte Subclass { get; }

    /// <summary>bInterfaceProtocol.</summary>
    public byte Protocol { get; }

    /// <summary>The interface's endpoints, in descriptor order; the default control endpoint is not one of them.</summary>
    public IReadOnlyList<UsbEndpointInfo> Endpoints => _endpoints;

    /// <summary>Describes <paramref name="usbInterface"/> and its endpoints.</summary>
    internal UsbInterfaceInfo(UsbInterface usbInterface)
    {
        Number = usbInterface.Number;
        Class = usbInterface.Class;
        Subclass = usbInterface.Subclass;
        Protocol = usbInterface.Protocol;

        List<UsbEndpoint> endpoints = usbInterface.Endpoints;
        _endpoints = new UsbEndpointInfo[endpoints.Count];
        for (int i = 0; i < _endpoints.Length; i++)
        {
            _endpoints[i] = new UsbEndpointInfo(endpoints[i]);
        }
    }

    /// <summary>Finds the first endpoint of this interface with the given transfer type and direction.</summary>
    /// <param name="type">The transfer type, such as <see cref="UsbEndpointType.Interrupt"/>.</param>
    /// <param name="direction">The direction, such as <see cref="UsbDirection.In"/>.</param>
    /// <param name="endpoint">The endpoint when the call returns true.</param>
    /// <returns>False when the interface has no such endpoint.</returns>
    public bool TryFindEndpoint(UsbEndpointType type, UsbDirection direction, out UsbEndpointInfo endpoint)
    {
        for (int i = 0; i < _endpoints.Length; i++)
        {
            if (_endpoints[i].Type == type && _endpoints[i].Direction == direction)
            {
                endpoint = _endpoints[i];
                return true;
            }
        }

        endpoint = default;
        return false;
    }
}
