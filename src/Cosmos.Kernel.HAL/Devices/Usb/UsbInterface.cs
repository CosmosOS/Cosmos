// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// One interface of the active configuration (alternate setting 0) and its
/// endpoints (USB 2.0 §9.6.5). This is the unit a <see cref="UsbDriver"/>
/// binds to, so a composite device can be driven by several drivers.
/// </summary>
internal sealed class UsbInterface
{
    public byte Number { get; }
    public byte Class { get; }
    public byte Subclass { get; }
    public byte Protocol { get; }
    public List<UsbEndpoint> Endpoints { get; } = [];

    /// <summary>The driver that bound this interface, or null while none has.</summary>
    public UsbDriver? Driver { get; set; }

    public UsbInterface(byte number, byte interfaceClass, byte subclass, byte protocol)
    {
        Number = number;
        Class = interfaceClass;
        Subclass = subclass;
        Protocol = protocol;
    }

    /// <summary>
    /// First endpoint of the given transfer type and direction, or null when
    /// the interface has none.
    /// </summary>
    public UsbEndpoint? FindEndpoint(UsbEndpointType type, bool isIn)
    {
        foreach (UsbEndpoint endpoint in Endpoints)
        {
            if (endpoint.Type == type && endpoint.IsIn == isIn)
            {
                return endpoint;
            }
        }

        return null;
    }
}
