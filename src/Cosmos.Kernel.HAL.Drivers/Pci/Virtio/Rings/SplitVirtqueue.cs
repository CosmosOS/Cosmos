// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Buffers.Binary;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Rings;

/// <summary>
/// One virtqueue in the split ring layout (virtio 1.2 §2.7): the descriptor
/// table, the available ring the driver offers buffers through, and the used
/// ring the device returns them in, laid out back to back in one DMA buffer
/// so the three addresses the device is programmed with come from one
/// allocation.
/// </summary>
/// <remarks>
/// <para>
/// A queue is not thread-safe, and it takes no lock: its owning driver
/// reaches each of its queues from one place only. In virtio-net the receive
/// queue belongs to the work item the interrupt handler schedules, and the
/// transmit queue to the transmit handler and the interrupt handler, which
/// the kit already keeps apart by masking interrupts around the former.
/// </para>
/// <para>
/// A driver's own indices are shadows, never read back out of the rings:
/// the device writes the used ring and reads the available one, so reading
/// an index the driver itself published would only trust the device with a
/// number it already knows.
/// </para>
/// </remarks>
internal sealed class SplitVirtqueue
{
    /// <summary>Bytes in front of either ring's entries: its flags, then its index.</summary>
    private const int RingHeaderLength = 4;

    /// <summary>Bytes one used ring entry takes: the descriptor's index, then how much the device wrote.</summary>
    private const int UsedEntryLength = 8;

    /// <summary>Bytes of an available ring entry: one descriptor index.</summary>
    private const int AvailableEntryLength = 2;

    /// <summary>
    /// The event suffix both rings carry, <c>used_event</c> and
    /// <c>avail_event</c>: unused here, since the driver negotiates no
    /// VIRTIO_F_EVENT_IDX, but part of either structure's size.
    /// </summary>
    private const int EventSuffixLength = 2;

    /// <summary>
    /// What the rings are laid out on. 16 bytes covers every split ring
    /// alignment rule at once: 16 for the descriptor table, 2 for the
    /// available ring and 4 for the used ring.
    /// </summary>
    private const int RingAlignment = 16;

    /// <summary>End of the free list, and what an exhausted queue hands back.</summary>
    private const int NoDescriptor = -1;

    private readonly DmaBuffer _memory;

    /// <summary>Byte offsets of the two rings inside <see cref="_memory"/>.</summary>
    private readonly int _availableOffset;
    private readonly int _usedOffset;

    /// <summary>The free list: for each descriptor, the next free one, or <see cref="NoDescriptor"/>.</summary>
    private readonly int[] _freeNext;

    private int _freeHead;

    /// <summary>What the driver has published in the available ring's index, which only it writes.</summary>
    private ushort _availableIndex;

    /// <summary>How far the driver has read the used ring, which only the device writes.</summary>
    private ushort _lastUsedIndex;

    /// <summary>Descriptors in the queue, which the device chose when it was activated.</summary>
    internal int Count { get; }

    /// <summary>The descriptor table, as the device addresses it.</summary>
    internal ulong DescriptorTableAddress => _memory.DeviceAddress;

    /// <summary>The available ring, as the device addresses it.</summary>
    internal ulong AvailableRingAddress => _memory.DeviceAddress + (ulong)_availableOffset;

    /// <summary>The used ring, as the device addresses it.</summary>
    internal ulong UsedRingAddress => _memory.DeviceAddress + (ulong)_usedOffset;

    /// <summary>
    /// Bytes a queue of <paramref name="count"/> descriptors needs, which the
    /// driver allocates before it builds one.
    /// </summary>
    /// <param name="count">Descriptors in the queue, a power of two.</param>
    /// <returns>The length to allocate.</returns>
    internal static int MemoryLength(int count) => UsedRingOffset(count) + RingHeaderLength
        + (count * UsedEntryLength) + EventSuffixLength;

    /// <summary>
    /// Lays a queue of <paramref name="count"/> descriptors out over
    /// <paramref name="memory"/> and puts every descriptor on the free list.
    /// The buffer comes zeroed, which is the empty state of both rings.
    /// </summary>
    /// <param name="memory">At least <see cref="MemoryLength"/> bytes, page aligned as the kit allocates it.</param>
    /// <param name="count">Descriptors in the queue, a power of two.</param>
    internal SplitVirtqueue(DmaBuffer memory, int count)
    {
        _memory = memory;
        Count = count;
        _availableOffset = count * VringDescriptor.Size;
        _usedOffset = UsedRingOffset(count);

        _freeNext = new int[count];
        for (int i = 0; i < count - 1; i++)
        {
            _freeNext[i] = i + 1;
        }

        _freeNext[count - 1] = NoDescriptor;
        _freeHead = 0;
    }

    /// <summary>
    /// Takes a descriptor off the free list. Callers keep to the one context
    /// the queue is reached from; see the remarks on the class.
    /// </summary>
    /// <param name="index">The descriptor, whose slot in the buffer pool it also names.</param>
    /// <returns>False when every descriptor is with the device.</returns>
    internal bool TryAllocateDescriptor(out int index)
    {
        index = _freeHead;
        if (index == NoDescriptor)
        {
            return false;
        }

        _freeHead = _freeNext[index];
        return true;
    }

    /// <summary>Puts a descriptor the device has returned back on the free list.</summary>
    /// <param name="index">A descriptor this queue handed out.</param>
    internal void FreeDescriptor(int index)
    {
        _freeNext[index] = _freeHead;
        _freeHead = index;
    }

    /// <summary>Points descriptor <paramref name="index"/> at one buffer, before it is offered.</summary>
    /// <param name="index">The descriptor to write.</param>
    /// <param name="address">The buffer, as the device addresses it.</param>
    /// <param name="length">Bytes of it the device may use.</param>
    /// <param name="flags">
    /// <see cref="VringDescriptor.DeviceWritable"/> to receive into the
    /// buffer, <see cref="VringDescriptor.DeviceReadable"/> to send it.
    /// </param>
    internal void Describe(int index, ulong address, int length, ushort flags) =>
        VringDescriptor.Write(DescriptorAt(index), address, (uint)length, flags);

    /// <summary>
    /// Hands descriptor <paramref name="index"/> to the device by adding it
    /// to the available ring. The device only looks once the caller rings its
    /// doorbell, which the transport does.
    /// </summary>
    /// <param name="index">A descriptor <see cref="Describe"/> has just written.</param>
    internal void Offer(int index)
    {
        int slot = _availableIndex % Count;
        BinaryPrimitives.WriteUInt16LittleEndian(
            Window(_availableOffset + RingHeaderLength + (slot * AvailableEntryLength), AvailableEntryLength),
            (ushort)index);

        // The index is what hands the entry over, so the entry, and the
        // descriptor it points at, have to be visible before it lands.
        DmaBuffer.WriteBarrier();
        _availableIndex++;
        BinaryPrimitives.WriteUInt16LittleEndian(Window(_availableOffset + 2, sizeof(ushort)), _availableIndex);
    }

    /// <summary>
    /// Takes the next descriptor the device has finished with off the used
    /// ring.
    /// </summary>
    /// <param name="index">The descriptor the device returned.</param>
    /// <param name="length">Bytes it wrote into the buffer, zero for one it only read.</param>
    /// <returns>False once the ring holds nothing new.</returns>
    /// <remarks>
    /// An entry naming a descriptor this queue never handed out is dropped:
    /// a device that writes one is broken, and following it would return a
    /// descriptor twice or index past the buffer pool.
    /// </remarks>
    internal bool TryTakeUsed(out int index, out int length)
    {
        while (_lastUsedIndex != BinaryPrimitives.ReadUInt16LittleEndian(Window(_usedOffset + 2, sizeof(ushort))))
        {
            // The device wrote the entry before it published the index; keep
            // the entry's loads from being hoisted above the index's, which
            // ARM64 would otherwise allow.
            DmaBuffer.ReadBarrier();

            int slot = _lastUsedIndex % Count;
            ReadOnlySpan<byte> entry = Window(_usedOffset + RingHeaderLength + (slot * UsedEntryLength), UsedEntryLength);
            uint id = BinaryPrimitives.ReadUInt32LittleEndian(entry);
            uint written = BinaryPrimitives.ReadUInt32LittleEndian(entry[sizeof(uint)..]);
            _lastUsedIndex++;

            if (id < (uint)Count)
            {
                index = (int)id;
                length = (int)written;
                return true;
            }
        }

        index = NoDescriptor;
        length = 0;
        return false;
    }

    /// <summary>Offset of the used ring, which follows the descriptor table and the available ring.</summary>
    private static int UsedRingOffset(int count)
    {
        int untilUsed = (count * VringDescriptor.Size) + RingHeaderLength
            + (count * AvailableEntryLength) + EventSuffixLength;
        return (untilUsed + RingAlignment - 1) & ~(RingAlignment - 1);
    }

    private Span<byte> DescriptorAt(int index) => Window(index * VringDescriptor.Size, VringDescriptor.Size);

    private Span<byte> Window(int offset, int length) => _memory.Span.Slice(offset, length);
}
