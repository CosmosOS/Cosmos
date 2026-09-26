// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// One endpoint of an interface, as its endpoint descriptor declares it
/// (USB 2.0 §9.6.6). A driver hands it back to
/// <see cref="UsbDeviceContext.OpenInterruptIn"/> or
/// <see cref="UsbDeviceContext.TryOpenBulk"/>, which find the endpoint by
/// its <see cref="Address"/> in the context's interface.
/// </summary>
internal readonly struct UsbEndpointInfo
{
    /// <summary>bEndpointAddress: the endpoint number in bits 3:0, the direction in bit 7.</summary>
    public byte Address { get; }

    /// <summary>The transfer type, from bmAttributes bits 1:0.</summary>
    public UsbEndpointType Type { get; }

    /// <summary>Which way the endpoint moves data, from bit 7 of <see cref="Address"/>.</summary>
    public UsbDirection Direction { get; }

    /// <summary>Largest packet the endpoint sends or receives, in bytes: wMaxPacketSize bits 10:0.</summary>
    public ushort MaxPacketSize { get; }

    /// <summary>bInterval: how often an interrupt endpoint is polled, in the unit the device's speed gives it.</summary>
    public byte Interval { get; }

    /// <summary>Describes <paramref name="endpoint"/>.</summary>
    internal UsbEndpointInfo(UsbEndpoint endpoint)
    {
        Address = endpoint.Address;
        Type = endpoint.Type;
        Direction = endpoint.IsIn ? UsbDirection.In : UsbDirection.Out;
        MaxPacketSize = endpoint.MaxPacketSize;
        Interval = endpoint.Interval;
    }
}
