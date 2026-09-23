// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Devices.Usb;

/// <summary>
/// One endpoint descriptor of an interface (USB 2.0 §9.6.6).
/// </summary>
internal sealed class UsbEndpoint
{
    /// <summary>bEndpointAddress bit 7: set for an IN (device-to-host) endpoint.</summary>
    private const byte DirectionInBit = 0x80;
    private const byte NumberMask = 0x0F;
    private const byte TransferTypeMask = 0x03;

    /// <summary>wMaxPacketSize bits 10:0 hold the packet size.</summary>
    private const ushort MaxPacketSizeMask = 0x07FF;

    /// <summary>wMaxPacketSize bits 12:11 hold the additional transactions per microframe (high-speed periodic endpoints).</summary>
    private const int AdditionalTransactionsShift = 11;
    private const ushort AdditionalTransactionsMask = 0x3;

    public UsbEndpoint(byte address, byte attributes, ushort maxPacketSize, byte interval)
    {
        Address = address;
        Type = (UsbEndpointType)(attributes & TransferTypeMask);
        MaxPacketSize = (ushort)(maxPacketSize & MaxPacketSizeMask);
        AdditionalTransactions = (byte)((maxPacketSize >> AdditionalTransactionsShift) & AdditionalTransactionsMask);
        Interval = interval;
    }

    /// <summary>bEndpointAddress: the endpoint number with the direction in bit 7.</summary>
    public byte Address { get; }

    public byte Number => (byte)(Address & NumberMask);
    public bool IsIn => (Address & DirectionInBit) != 0;
    public UsbEndpointType Type { get; }
    public ushort MaxPacketSize { get; }
    public byte AdditionalTransactions { get; }

    /// <summary>bInterval, in the unit the device speed gives it (USB 2.0 §9.6.6).</summary>
    public byte Interval { get; }
}

/// <summary>Transfer type of an endpoint, from bmAttributes bits 1:0.</summary>
internal enum UsbEndpointType : byte
{
    Control = 0,
    Isochronous = 1,
    Bulk = 2,
    Interrupt = 3
}
