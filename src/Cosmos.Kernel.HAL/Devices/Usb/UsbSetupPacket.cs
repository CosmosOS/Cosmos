// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// The 8-byte SETUP packet that opens every control transfer (USB 2.0 §9.3).
/// </summary>
internal readonly struct UsbSetupPacket
{
    public UsbSetupPacket(UsbRequestType requestType, byte request, ushort value, ushort index, ushort length)
    {
        RequestType = requestType;
        Request = request;
        Value = value;
        Index = index;
        Length = length;
    }

    public UsbRequestType RequestType { get; }
    public byte Request { get; }
    public ushort Value { get; }
    public ushort Index { get; }

    /// <summary>wLength: bytes in the data stage, 0 when there is none.</summary>
    public ushort Length { get; }

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
