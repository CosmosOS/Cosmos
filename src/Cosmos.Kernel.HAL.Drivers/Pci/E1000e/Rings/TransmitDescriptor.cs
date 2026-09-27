// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Rings;

/// <summary>
/// One entry of the transmit ring in the legacy descriptor format (82574
/// datasheet §7.2.3): the frame the device is to send and how long it is.
/// Written in place in the ring's DMA memory, little-endian, and never read
/// back: the driver takes descriptors back by the head register rather than
/// by the status byte the device writes here.
/// </summary>
internal static class TransmitDescriptor
{
    /// <summary>Bytes one descriptor takes in the ring.</summary>
    internal const int Size = 16;

    /// <summary>
    /// EOP, IFCS and RS: the frame ends in this descriptor, the device adds
    /// the Ethernet CRC, and it reports the descriptor done when it has.
    /// </summary>
    private const byte Command = (1 << 0) | (1 << 1) | (1 << 3);

    private const int LengthOffset = 8;
    private const int ChecksumOffsetOffset = 10;
    private const int CommandOffset = 11;

    /// <summary>Where the device's write-back starts: the status, then the checksum start and the VLAN tag.</summary>
    private const int WriteBackOffset = 12;

    /// <summary>
    /// Describes one frame to send and clears the write-back behind it. The
    /// tail write that follows is what hands the descriptor over, and what
    /// orders these stores.
    /// </summary>
    /// <param name="descriptor">The entry's <see cref="Size"/> bytes in the ring.</param>
    /// <param name="address">The frame buffer, as the device addresses it.</param>
    /// <param name="length">Bytes of the frame, without the CRC the device adds.</param>
    internal static void Send(Span<byte> descriptor, ulong address, int length)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(descriptor, address);
        BinaryPrimitives.WriteUInt16LittleEndian(descriptor[LengthOffset..], (ushort)length);
        descriptor[ChecksumOffsetOffset] = 0;
        descriptor[CommandOffset] = Command;
        descriptor[WriteBackOffset..].Clear();
    }
}
