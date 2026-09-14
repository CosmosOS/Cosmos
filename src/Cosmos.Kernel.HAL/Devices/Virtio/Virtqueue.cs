// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;

namespace Cosmos.Kernel.HAL.Devices.Virtio;

/// <summary>
/// Virtqueue implementation for virtio devices (split ring layout).
/// A virtqueue consists of three parts:
/// - Descriptor table: array of buffer descriptors
/// - Available ring: buffers offered to device
/// - Used ring: buffers returned by device
/// The layout is transport-independent; the same rings work over MMIO and PCI.
/// </summary>
internal unsafe class Virtqueue
{
    // Descriptor flags
    public const ushort VRING_DESC_F_NEXT = 1;      // Buffer continues via next field
    public const ushort VRING_DESC_F_WRITE = 2;    // Buffer is write-only (for device)
    public const ushort VRING_DESC_F_INDIRECT = 4; // Buffer contains list of descriptors

    // Used ring flags
    public const ushort VRING_USED_F_NO_NOTIFY = 1;

    // Available ring flags
    public const ushort VRING_AVAIL_F_NO_INTERRUPT = 1;

    private readonly uint _queueSize;
    private readonly ulong _baseAddress;

    // Pointers to virtqueue structures
    private VringDesc* _descriptors;
    private VringAvail* _available;
    private VringUsed* _used;

    // Index tracking
    private ushort _freeHead;
    private ushort _lastUsedIdx;
    private ushort _numFree;

    // Free descriptor list
    private ushort* _freeList;

    /// <summary>
    /// Queue size (number of descriptors).
    /// </summary>
    public uint QueueSize => _queueSize;

    /// <summary>
    /// Physical address of descriptor table (for DMA).
    /// </summary>
    public ulong DescriptorTableAddr => VirtioDma.VirtToPhys((ulong)_descriptors);

    /// <summary>
    /// Physical address of available ring (for DMA).
    /// </summary>
    public ulong AvailableRingAddr => VirtioDma.VirtToPhys((ulong)_available);

    /// <summary>
    /// Physical address of used ring (for DMA).
    /// </summary>
    public ulong UsedRingAddr => VirtioDma.VirtToPhys((ulong)_used);

    /// <summary>
    /// Physical base address of the entire queue (for legacy PFN calculation).
    /// </summary>
    public ulong QueueBaseAddr => VirtioDma.VirtToPhys(_baseAddress);

    /// <summary>
    /// Creates a new virtqueue with the specified size.
    /// Uses page-aligned allocation for legacy virtio MMIO compatibility.
    /// </summary>
    public Virtqueue(uint queueSize)
    {
        _queueSize = queueSize;

        // Calculate sizes
        uint descTableSize = queueSize * (uint)sizeof(VringDesc);
        uint availRingSize = (uint)sizeof(VringAvail) + queueSize * sizeof(ushort);
        uint usedRingSize = (uint)sizeof(VringUsed) + queueSize * (uint)sizeof(VringUsedElem);

        // Align sizes
        descTableSize = Align(descTableSize, 16);
        availRingSize = Align(availRingSize, 2);
        usedRingSize = Align(usedRingSize, 4);

        // Total size with alignment for used ring (needs page alignment)
        uint totalSize = descTableSize + availRingSize;
        totalSize = Align(totalSize, (uint)PageAllocator.PageSize);  // Used ring needs page alignment
        totalSize += usedRingSize;

        // Calculate pages needed (round up)
        ulong pageCount = (totalSize + (uint)PageAllocator.PageSize - 1) / (uint)PageAllocator.PageSize;

        // Allocate page-aligned memory for legacy MMIO compatibility
        byte* mem = (byte*)PageAllocator.AllocPages(PageType.HeapLarge, pageCount, true);

        _baseAddress = (ulong)mem;

        Serial.Write("[Virtqueue] Allocated ");
        Serial.WriteNumber(pageCount);
        Serial.Write(" pages at 0x");
        Serial.WriteHex(_baseAddress);
        Serial.Write("\n");

        // Set up pointers
        _descriptors = (VringDesc*)mem;
        _available = (VringAvail*)(mem + descTableSize);
        _used = (VringUsed*)(mem + Align(descTableSize + availRingSize, (uint)PageAllocator.PageSize));

        // Initialize free list
        _freeList = (ushort*)MemoryOp.Alloc(queueSize * sizeof(ushort));
        for (ushort i = 0; i < queueSize; i++)
        {
            _freeList[i] = (ushort)(i + 1);
        }
        _freeList[queueSize - 1] = 0xFFFF;  // End of list

        _freeHead = 0;
        _numFree = (ushort)queueSize;
        _lastUsedIdx = 0;
    }

    /// <summary>
    /// Allocates a descriptor from the free list.
    /// </summary>
    public int AllocDescriptor()
    {
        if (_numFree == 0)
        {
            return -1;
        }

        int idx = _freeHead;
        _freeHead = _freeList[_freeHead];
        _numFree--;
        return idx;
    }

    /// <summary>
    /// Frees a descriptor back to the free list.
    /// </summary>
    public void FreeDescriptor(int idx)
    {
        _freeList[idx] = _freeHead;
        _freeHead = (ushort)idx;
        _numFree++;
    }

    /// <summary>
    /// Sets up a descriptor with the given buffer.
    /// </summary>
    public void SetupDescriptor(int idx, ulong addr, uint len, ushort flags, ushort next)
    {
        _descriptors[idx].Addr = addr;
        _descriptors[idx].Len = len;
        _descriptors[idx].Flags = flags;
        _descriptors[idx].Next = next;
    }

    /// <summary>
    /// Adds a buffer chain to the available ring.
    /// </summary>
    public void AddAvailable(ushort headIdx)
    {
        ushort availIdx = (ushort)(_available->Idx % _queueSize);
        _available->Ring[availIdx] = headIdx;

        // Memory barrier before updating idx
        System.Threading.Thread.MemoryBarrier();

        _available->Idx++;
    }

    /// <summary>
    /// Checks if there are used buffers to process.
    /// </summary>
    public bool HasUsedBuffers()
    {
        return _lastUsedIdx != _used->Idx;
    }

    /// <summary>
    /// Gets the next used buffer.
    /// </summary>
    public bool GetUsedBuffer(out uint id, out uint len)
    {
        if (_lastUsedIdx == _used->Idx)
        {
            id = 0;
            len = 0;
            return false;
        }

        // The device wrote the ring element before publishing Idx; keep the
        // element read from being speculated ahead of the Idx read (ARM
        // permits load-load reordering).
        System.Threading.Thread.MemoryBarrier();

        ushort usedIdx = (ushort)(_lastUsedIdx % _queueSize);

        // Access used ring element through pointer arithmetic
        // VringUsed layout: Flags (2) + Idx (2) + Ring[N] where each element is VringUsedElem (8 bytes)
        VringUsedElem* ring = (VringUsedElem*)((byte*)_used + 4);  // Skip flags and idx
        id = ring[usedIdx].Id;
        len = ring[usedIdx].Len;

        _lastUsedIdx++;
        return true;
    }

    private static uint Align(uint value, uint alignment)
    {
        return (value + alignment - 1) & ~(alignment - 1);
    }

    // Virtqueue descriptor
    [StructLayout(LayoutKind.Sequential)]
    public struct VringDesc
    {
        public ulong Addr;    // Buffer address
        public uint Len;      // Buffer length
        public ushort Flags;  // Descriptor flags
        public ushort Next;   // Next descriptor index
    }

    // Available ring header
    [StructLayout(LayoutKind.Sequential)]
    public struct VringAvail
    {
        public ushort Flags;
        public ushort Idx;
        public fixed ushort Ring[1];  // Variable length
    }

    // Used ring element
    [StructLayout(LayoutKind.Sequential)]
    public struct VringUsedElem
    {
        public uint Id;   // Descriptor chain head
        public uint Len;  // Total bytes written
    }

    // Used ring header
    [StructLayout(LayoutKind.Sequential)]
    public struct VringUsed
    {
        public ushort Flags;
        public ushort Idx;
        public fixed byte Ring[1];  // VringUsedElem array (can't use fixed with struct)
    }
}
