// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Rings;

/// <summary>
/// One entry of a split ring's descriptor table (virtio 1.2 §2.7.5): where
/// one buffer is, how long it is, and whether the device writes it. Written
/// in place in the queue's DMA memory, so no descriptor is ever copied, and
/// little-endian, as every virtio structure is on both architectures.
/// </summary>
/// <remarks>
/// A driver here describes each request with a single descriptor, never a
/// chain: what it sends is copied behind its device-type header into one
/// slot, and what it receives is written into one slot whole. So the
/// <c>next</c> field is always zero and VRING_DESC_F_NEXT is never set,
/// which is why <see cref="Write"/> takes no successor.
/// </remarks>
internal static class VringDescriptor
{
    /// <summary>Bytes one descriptor takes in the table.</summary>
    internal const int Size = 16;

    /// <summary>No flags: a buffer the device reads, such as a frame to transmit.</summary>
    internal const ushort DeviceReadable = 0;

    /// <summary>VRING_DESC_F_WRITE: a buffer the device writes, such as one posted to receive into.</summary>
    internal const ushort DeviceWritable = 2;

    private const int LengthOffset = 8;
    private const int FlagsOffset = 12;
    private const int NextOffset = 14;

    /// <summary>
    /// Writes one descriptor over the <see cref="Size"/> bytes at
    /// <paramref name="descriptor"/>. The caller hands the buffer to the
    /// device afterwards, which is what orders these stores.
    /// </summary>
    /// <param name="descriptor">The entry's bytes in the descriptor table.</param>
    /// <param name="address">The buffer, as the device addresses it.</param>
    /// <param name="length">Bytes of the buffer the device may use.</param>
    /// <param name="flags">
    /// <see cref="DeviceWritable"/> for a buffer the device writes,
    /// <see cref="DeviceReadable"/> for one it reads.
    /// </param>
    internal static void Write(Span<byte> descriptor, ulong address, uint length, ushort flags)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(descriptor, address);
        BinaryPrimitives.WriteUInt32LittleEndian(descriptor[LengthOffset..], length);
        BinaryPrimitives.WriteUInt16LittleEndian(descriptor[FlagsOffset..], flags);
        BinaryPrimitives.WriteUInt16LittleEndian(descriptor[NextOffset..], 0);
    }
}
