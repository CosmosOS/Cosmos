// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Usb;

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// The 8-byte SETUP packet that opens every control transfer (USB 2.0
/// §9.3), as the kit hands it to a <see cref="UsbHostDevice"/>. A plain
/// value: every member can be read in any context, and none allocates.
/// </summary>
[Experimental(Experimentals.DriverKitDiagId)]
public readonly struct UsbSetupPacket
{
    /// <summary>bmRequestType bit 7: set for a device-to-host request.</summary>
    private const byte DeviceToHostBit = 0x80;

    /// <summary>bmRequestType: the direction, who defines the request, and what it is addressed to.</summary>
    public byte RequestType { get; }

    /// <summary>bRequest.</summary>
    public byte Request { get; }

    /// <summary>wValue.</summary>
    public ushort Value { get; }

    /// <summary>wIndex: for an interface or endpoint request, its number or address.</summary>
    public ushort Index { get; }

    /// <summary>wLength: bytes in the data stage, 0 when there is none.</summary>
    public ushort Length { get; }

    /// <summary>Which way the data stage moves, from bit 7 of <see cref="RequestType"/>.</summary>
    public UsbDirection Direction => (RequestType & DeviceToHostBit) != 0 ? UsbDirection.In : UsbDirection.Out;

    /// <summary>Builds a request from its fields.</summary>
    /// <param name="direction">Which way the data stage moves; <see cref="UsbDirection.Out"/> for a request without one.</param>
    /// <param name="kind">Who defines the request.</param>
    /// <param name="recipient">What the request is addressed to.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex.</param>
    /// <param name="length">wLength.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="direction"/>, <paramref name="kind"/> or
    /// <paramref name="recipient"/> is not a defined value.
    /// </exception>
    public UsbSetupPacket(UsbDirection direction, UsbRequestKind kind, UsbRecipient recipient, byte request, ushort value,
        ushort index, ushort length)
    {
        UsbRequestType requestType = direction switch
        {
            UsbDirection.Out => UsbRequestType.HostToDevice,
            UsbDirection.In => UsbRequestType.DeviceToHost,
            _ => throw new ArgumentOutOfRangeException(nameof(direction))
        };

        RequestType = (byte)(requestType | RequestTypeOf(kind, recipient));
        Request = request;
        Value = value;
        Index = index;
        Length = length;
    }

    /// <summary>Builds a request whose bmRequestType the USB core already put together.</summary>
    internal UsbSetupPacket(UsbRequestType requestType, byte request, ushort value, ushort index, ushort length)
    {
        RequestType = (byte)requestType;
        Request = request;
        Value = value;
        Index = index;
        Length = length;
    }

    /// <summary>
    /// The packet in its little-endian wire layout, as one 64-bit value: the
    /// form an xHCI Setup Stage TRB carries as immediate data.
    /// </summary>
    /// <returns>bmRequestType in bits 7:0, then bRequest, wValue, wIndex and wLength.</returns>
    public ulong ToUInt64() =>
        RequestType
        | ((ulong)Request << 8)
        | ((ulong)Value << 16)
        | ((ulong)Index << 32)
        | ((ulong)Length << 48);

    /// <summary>bmRequestType's type and recipient fields for a request of <paramref name="kind"/> to <paramref name="recipient"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="kind"/> or <paramref name="recipient"/> is not a defined value.</exception>
    internal static UsbRequestType RequestTypeOf(UsbRequestKind kind, UsbRecipient recipient)
    {
        UsbRequestType type = kind switch
        {
            UsbRequestKind.Standard => UsbRequestType.Standard,
            UsbRequestKind.Class => UsbRequestType.Class,
            UsbRequestKind.Vendor => UsbRequestType.Vendor,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };

        UsbRequestType target = recipient switch
        {
            UsbRecipient.Device => UsbRequestType.Device,
            UsbRecipient.Interface => UsbRequestType.Interface,
            UsbRecipient.Endpoint => UsbRequestType.Endpoint,
            UsbRecipient.Other => UsbRequestType.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(recipient))
        };

        return type | target;
    }
}
