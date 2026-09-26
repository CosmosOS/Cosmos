// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.Usb;

/// <summary>
/// How a USB transfer that moves data ended: its status, and how many bytes
/// it moved.
/// </summary>
internal readonly struct UsbTransferResult
{
    /// <summary>How the transfer ended.</summary>
    public UsbTransferStatus Status { get; }

    /// <summary>
    /// Bytes the transfer moved. On success it can be less than asked for:
    /// a device answers a read with a short packet when it has no more to
    /// send, and only that many bytes at the start of the buffer are its.
    /// After a failure, the bytes a bulk transfer moved before it failed; 0
    /// for a control transfer.
    /// </summary>
    public int Length { get; }

    internal UsbTransferResult(UsbTransferStatus status, int length)
    {
        Status = status;
        Length = length;
    }
}
