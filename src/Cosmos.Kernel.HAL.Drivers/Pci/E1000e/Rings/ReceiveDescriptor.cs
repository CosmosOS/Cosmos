// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Rings;

/// <summary>
/// One entry of the receive ring in the legacy descriptor format (82574
/// datasheet §7.1.5): where the device writes the next frame it accepts, and
/// what it writes back about the one it wrote there. Read and written in
/// place in the ring's DMA memory, and little-endian, as every descriptor of
/// this family is.
/// </summary>
internal static class ReceiveDescriptor
{
    /// <summary>Bytes one descriptor takes in the ring.</summary>
    internal const int Size = 16;

    /// <summary>DD: the device has written this descriptor back, so the fields below it are its answer.</summary>
    internal const byte Done = 1 << 0;

    /// <summary>EOP: the frame ends in this descriptor, which is every frame a 2048-byte buffer holds whole.</summary>
    internal const byte EndOfPacket = 1 << 1;

    /// <summary>Where the device's write-back starts: the length, then the checksum, status, errors and VLAN tag.</summary>
    private const int WriteBackOffset = 8;

    private const int StatusOffset = 12;
    private const int ErrorsOffset = 13;

    /// <summary>
    /// Points the descriptor at its buffer and clears the write-back behind
    /// it, which clears <see cref="Done"/> and so hands the descriptor back to
    /// the device. The tail write that follows is what orders these stores.
    /// </summary>
    /// <param name="descriptor">The entry's <see cref="Size"/> bytes in the ring.</param>
    /// <param name="address">The frame buffer, as the device addresses it.</param>
    internal static void Post(Span<byte> descriptor, ulong address)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(descriptor, address);
        descriptor[WriteBackOffset..].Clear();
    }

    /// <summary>The status byte, which says whether the device wrote the descriptor back and where the frame ends.</summary>
    internal static byte ReadStatus(ReadOnlySpan<byte> descriptor) => descriptor[StatusOffset];

    /// <summary>The error byte; anything set means the frame is not one to hand to the stack.</summary>
    internal static byte ReadErrors(ReadOnlySpan<byte> descriptor) => descriptor[ErrorsOffset];

    /// <summary>Bytes the device wrote into the buffer, the frame without its CRC, which the receiver strips.</summary>
    internal static int ReadLength(ReadOnlySpan<byte> descriptor) =>
        BinaryPrimitives.ReadUInt16LittleEndian(descriptor[WriteBackOffset..]);
}
