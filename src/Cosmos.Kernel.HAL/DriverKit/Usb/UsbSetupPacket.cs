// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Usb;

/// <summary>
/// The 8-byte SETUP packet that opens every control transfer (USB 2.0
/// section 9.3). Any context, immutable.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public readonly struct UsbSetupPacket
{
    /// <summary>Builds a packet from its five fields.</summary>
    /// <param name="requestType">bmRequestType: direction, type and recipient.</param>
    /// <param name="request">bRequest.</param>
    /// <param name="value">wValue.</param>
    /// <param name="index">wIndex.</param>
    /// <param name="length">wLength: bytes in the data stage, 0 when there is none.</param>
    public UsbSetupPacket(UsbRequestType requestType, byte request, ushort value, ushort index, ushort length)
    {
        RequestType = requestType;
        Request = request;
        Value = value;
        Index = index;
        Length = length;
    }

    /// <summary>bmRequestType: direction, type and recipient.</summary>
    public UsbRequestType RequestType { get; }

    /// <summary>bRequest.</summary>
    public byte Request { get; }

    /// <summary>wValue.</summary>
    public ushort Value { get; }

    /// <summary>wIndex.</summary>
    public ushort Index { get; }

    /// <summary>wLength: bytes in the data stage, 0 when there is none.</summary>
    public ushort Length { get; }

    /// <summary>True when the data stage comes from the device.</summary>
    public bool IsDeviceToHost => (RequestType & UsbRequestType.DeviceToHost) != 0;

    /// <summary>
    /// The packet in its little-endian wire layout, as one 64-bit value: the
    /// form an xHCI Setup Stage TRB carries as immediate data.
    /// </summary>
    public ulong Pack() =>
        (byte)RequestType
        | ((ulong)Request << 8)
        | ((ulong)Value << 16)
        | ((ulong)Index << 32)
        | ((ulong)Length << 48);
}
