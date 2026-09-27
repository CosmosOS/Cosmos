// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Usb;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// One endpoint of an interface, as its endpoint descriptor declares it
/// (USB 2.0 §9.6.6). A driver hands it back to
/// <see cref="UsbDeviceContext.OpenInterruptIn"/> or
/// <see cref="UsbDeviceContext.TryOpenBulk"/>, which find the endpoint by
/// its <see cref="Address"/> in the context's interface, and the kit hands
/// it to the <see cref="UsbHostDevice"/> that opens it. A plain value:
/// every member can be read in any context. A <c>default</c> value names
/// endpoint 0, which is no endpoint of an interface, so the context refuses
/// to open it.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public readonly struct UsbEndpointInfo
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

    /// <summary>
    /// Transactions a high-speed interrupt or isochronous endpoint moves per
    /// microframe past the first: wMaxPacketSize bits 12:11. 0 for any other.
    /// </summary>
    public byte AdditionalTransactions { get; }

    /// <summary>
    /// Packets a SuperSpeed endpoint moves in one burst past the first:
    /// bMaxBurst of its SuperSpeed Endpoint Companion descriptor (USB 3.2
    /// §9.6.7). 0 below SuperSpeed.
    /// </summary>
    public byte MaxBurst { get; }

    /// <summary>Describes <paramref name="endpoint"/>.</summary>
    internal UsbEndpointInfo(UsbEndpoint endpoint)
    {
        Address = endpoint.Address;
        Type = endpoint.Type;
        Direction = endpoint.IsIn ? UsbDirection.In : UsbDirection.Out;
        MaxPacketSize = endpoint.MaxPacketSize;
        Interval = endpoint.Interval;
        AdditionalTransactions = endpoint.AdditionalTransactions;
        MaxBurst = endpoint.MaxBurst;
    }
}
