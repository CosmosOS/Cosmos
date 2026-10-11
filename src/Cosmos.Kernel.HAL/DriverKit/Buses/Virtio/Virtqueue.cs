// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Cosmos.Kernel.HAL.DriverKit.Resources;
using Cosmos.Kernel.HAL.DriverKit.Threading;

namespace Cosmos.Kernel.HAL.DriverKit.Buses.Virtio;

/// <summary>
/// A split virtqueue (virtio specification section 2.6) over DMA memory the
/// leaf's binding owns, created by <see cref="VirtioAccess.TryCreateQueue"/>.
/// Not thread-safe: the driver serializes its use, with a
/// <see cref="DeviceLock"/> between its transmit path and its drain. Every
/// member is allocation-free, so a handler may pop used buffers; the
/// members that touch the rings order them with the DMA barriers. Dead
/// once the binding that created it was torn down: every ring member
/// throws <see cref="InvalidOperationException"/> from then on, in every
/// build, as <see cref="DmaBuffer.Span"/> does, so a consumer that kept a
/// reference across a withdrawal gets the exception and never a write into
/// freed pages. The kit never rounds a size: queue sizes are powers of two
/// by the specification, and QEMU offers 256 or 1024.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class Virtqueue
{
    /// <summary>Bytes of one descriptor: address, length, flags, next.</summary>
    internal const int DescriptorBytes = 16;

    /// <summary>Bytes of the available ring before its entries: flags and idx.</summary>
    internal const int AvailableRingHeaderBytes = 4;

    /// <summary>Bytes of one available ring entry: a descriptor index.</summary>
    internal const int AvailableRingEntryBytes = 2;

    /// <summary>Bytes of the used_event field after the available ring's entries.</summary>
    internal const int AvailableRingTrailerBytes = 2;

    /// <summary>Bytes of the used ring before its elements: flags and idx.</summary>
    internal const int UsedRingHeaderBytes = 4;

    /// <summary>Bytes of one used ring element: a 32-bit id and a 32-bit length.</summary>
    internal const int UsedRingElementBytes = 8;

    /// <summary>Bytes of the avail_event field after the used ring's elements.</summary>
    internal const int UsedRingTrailerBytes = 2;

    /// <summary>The free list's end marker.</summary>
    private const ushort EndOfFreeList = 0xFFFF;

    private readonly VirtioTransport _transport;
    private readonly DmaBuffer _memory;
    private readonly int _availableOffset;
    private readonly int _usedOffset;
    private readonly ushort[] _freeList;
    private ushort _freeHead;
    private ushort _lastUsed;

    /// <summary>The queue's index on its device.</summary>
    public ushort Index { get; }

    /// <summary>How many descriptors the queue has; also the length of each ring.</summary>
    public ushort Size { get; }

    /// <summary>How many descriptors are not allocated. Any context.</summary>
    public int FreeDescriptorCount { get; private set; }

    /// <summary>Used elements whose id was at or above <see cref="Size"/>, dropped by <see cref="TryTakeUsed"/> rather than indexed. Any context.</summary>
    public int DroppedUsedElements { get; private set; }

    /// <summary>
    /// True when the device has returned a buffer the driver has not taken.
    /// No DMA read barrier: for a work item loop that returns, not a spin; a leaf
    /// that must wait waits on its interrupt or spins with
    /// <see cref="DmaBuffer.ReadBarrier"/>. Interrupt context; allocation-free.
    /// </summary>
    /// <exception cref="InvalidOperationException">The binding that created the queue was torn down.</exception>
    public bool HasUsed
    {
        get
        {
            ThrowIfReleased();
            return _lastUsed != Volatile.Read(ref Used.Index);
        }
    }

    /// <summary>The descriptor table, at the start of the block. Throws once the block was freed.</summary>
    private Span<Descriptor> Descriptors => MemoryMarshal.Cast<byte, Descriptor>(_memory.Span.Slice(0, Size * DescriptorBytes));

    /// <summary>The available ring's flags and index. Throws once the block was freed.</summary>
    private ref RingHeader Available => ref MemoryMarshal.AsRef<RingHeader>(_memory.Span.Slice(_availableOffset, AvailableRingHeaderBytes));

    /// <summary>The available ring's entries, after its header. Throws once the block was freed.</summary>
    private Span<ushort> AvailableRing => MemoryMarshal.Cast<byte, ushort>(_memory.Span.Slice(_availableOffset + AvailableRingHeaderBytes, Size * AvailableRingEntryBytes));

    /// <summary>The used ring's flags and index, which the device advances. Throws once the block was freed.</summary>
    private ref RingHeader Used => ref MemoryMarshal.AsRef<RingHeader>(_memory.Span.Slice(_usedOffset, UsedRingHeaderBytes));

    /// <summary>The used ring's elements, after its header. Throws once the block was freed.</summary>
    private Span<UsedElement> UsedRing => MemoryMarshal.Cast<byte, UsedElement>(_memory.Span.Slice(_usedOffset + UsedRingHeaderBytes, Size * UsedRingElementBytes));

    /// <summary>
    /// Records where the rings sit in <paramref name="memory"/> and builds
    /// the free list. Thread context: the free list is a managed array.
    /// </summary>
    /// <param name="transport">The transport, for the doorbell.</param>
    /// <param name="index">The queue index.</param>
    /// <param name="size">The queue size.</param>
    /// <param name="memory">The zeroed, page-aligned block holding the rings, on the leaf's ledger.</param>
    /// <param name="availableOffset">Offset of the available ring within the block.</param>
    /// <param name="usedOffset">Offset of the used ring within the block.</param>
    internal Virtqueue(VirtioTransport transport, ushort index, ushort size, DmaBuffer memory, ulong availableOffset, ulong usedOffset)
    {
        _transport = transport;
        _memory = memory;
        Index = index;
        Size = size;

        _availableOffset = (int)availableOffset;
        _usedOffset = (int)usedOffset;

        _freeList = new ushort[size];
        for (int i = 0; i < size - 1; i++)
        {
            _freeList[i] = (ushort)(i + 1);
        }

        _freeList[size - 1] = EndOfFreeList;
        _freeHead = 0;
        FreeDescriptorCount = size;
    }

    /// <summary>Takes a descriptor off the free list. Managed state only; any context; allocation-free.</summary>
    /// <param name="index">The descriptor, when one was free.</param>
    /// <returns>False when every descriptor is allocated.</returns>
    public bool TryAllocateDescriptor(out ushort index)
    {
        if (FreeDescriptorCount == 0)
        {
            index = 0;
            return false;
        }

        index = _freeHead;
        _freeHead = _freeList[_freeHead];
        FreeDescriptorCount--;
        return true;
    }

    /// <summary>Puts a descriptor back on the free list. Managed state only; any context; allocation-free.</summary>
    /// <param name="index">A descriptor <see cref="TryAllocateDescriptor"/> handed out.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is at or above <see cref="Size"/>.</exception>
    public void FreeDescriptor(ushort index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Size);
        _freeList[index] = _freeHead;
        _freeHead = index;
        FreeDescriptorCount++;
    }

    /// <summary>Fills one descriptor. Interrupt context; allocation-free.</summary>
    /// <param name="index">The descriptor.</param>
    /// <param name="physicalAddress">The buffer's physical address.</param>
    /// <param name="length">The buffer's length in bytes.</param>
    /// <param name="flags">Whether the device writes the buffer, and whether the chain continues.</param>
    /// <param name="next">The next descriptor of the chain, read only with <see cref="VirtqueueDescriptorFlags.Next"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is at or above <see cref="Size"/>.</exception>
    /// <exception cref="InvalidOperationException">The binding that created the queue was torn down.</exception>
    public void SetDescriptor(ushort index, ulong physicalAddress, uint length, VirtqueueDescriptorFlags flags, ushort next = 0)
    {
        ThrowIfReleased();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Size);
        ref Descriptor descriptor = ref Descriptors[index];
        descriptor.Address = physicalAddress;
        descriptor.Length = length;
        descriptor.Flags = (ushort)flags;
        descriptor.Next = next;
    }

    /// <summary>
    /// Offers the chain starting at <paramref name="head"/> to the device:
    /// the head goes into the available ring, a write barrier orders it
    /// after the descriptors, then the ring's index advances. The device
    /// is not told; a leaf calls <see cref="Notify"/> for that, and only
    /// after DRIVER_OK. Interrupt context; allocation-free.
    /// </summary>
    /// <param name="head">The first descriptor of the chain.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="head"/> is at or above <see cref="Size"/>.</exception>
    /// <exception cref="InvalidOperationException">The binding that created the queue was torn down.</exception>
    public void Submit(ushort head)
    {
        ThrowIfReleased();
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(head, Size);
        ref RingHeader available = ref Available;
        ushort index = available.Index;
        AvailableRing[index % Size] = head;
        DmaBuffer.WriteBarrier();
        available.Index = (ushort)(index + 1);
    }

    /// <summary>
    /// Takes the next buffer the device returned. An element whose id is at
    /// or above <see cref="Size"/> is out of contract: it is skipped, counted
    /// in <see cref="DroppedUsedElements"/>, and the next one is looked at,
    /// with no log and no exception, since the kit cancels a work item that
    /// throws and the drain would never run again. Interrupt context;
    /// allocation-free.
    /// </summary>
    /// <param name="id">The head descriptor of the chain the device finished.</param>
    /// <param name="length">How many bytes the device wrote into the chain.</param>
    /// <returns>False when no valid used element remains.</returns>
    /// <exception cref="InvalidOperationException">The binding that created the queue was torn down.</exception>
    public bool TryTakeUsed(out ushort id, out uint length)
    {
        ThrowIfReleased();
        while (_lastUsed != Volatile.Read(ref Used.Index))
        {
            DmaBuffer.ReadBarrier();
            ref UsedElement element = ref UsedRing[_lastUsed % Size];
            uint elementId = element.Id;
            uint elementLength = element.Length;
            _lastUsed++;
            if (elementId >= Size)
            {
                DroppedUsedElements++;
                continue;
            }

            id = (ushort)elementId;
            length = elementLength;
            return true;
        }

        id = 0;
        length = 0;
        return false;
    }

    /// <summary>
    /// Rings the doorbell: a write barrier, then the transport's notify.
    /// Always notifies (no VIRTQ_USED_F_NO_NOTIFY check). A leaf notifies
    /// only after <see cref="VirtioAccess.SetDriverOk"/>; submitting before
    /// it is fine. Interrupt context; allocation-free.
    /// </summary>
    /// <exception cref="InvalidOperationException">The binding that created the queue was torn down.</exception>
    public void Notify()
    {
        ThrowIfReleased();
        DmaBuffer.WriteBarrier();
        _transport.NotifyQueue(Index);
    }

    /// <summary>Refuses a ring access once the binding's DMA memory went back to the allocator. Allocation-free.</summary>
    private void ThrowIfReleased()
    {
        if (_memory.IsReleased)
        {
            throw new InvalidOperationException(DmaBuffer.ReleasedMessage);
        }
    }

    /// <summary>One descriptor of the table (struct virtq_desc).</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct Descriptor
    {
        /// <summary>The buffer's physical address.</summary>
        public ulong Address;

        /// <summary>The buffer's length in bytes.</summary>
        public uint Length;

        /// <summary>The VIRTQ_DESC_F_* bits.</summary>
        public ushort Flags;

        /// <summary>The next descriptor of the chain.</summary>
        public ushort Next;
    }

    /// <summary>The head of the available and the used ring: flags, then the ring index; the entries follow.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RingHeader
    {
        /// <summary>The ring's flags.</summary>
        public ushort Flags;

        /// <summary>How many entries have been put in the ring, free-running.</summary>
        public ushort Index;
    }

    /// <summary>One element of the used ring (struct virtq_used_elem).</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct UsedElement
    {
        /// <summary>The head descriptor of the chain, 32 bits wide in the ring.</summary>
        public uint Id;

        /// <summary>How many bytes the device wrote into the chain.</summary>
        public uint Length;
    }
}
