// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Allocation methods: segment management, bump allocation, and free list operations.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    /// <summary>
    /// Aligns a size up to the nearest pointer-sized boundary.
    /// </summary>
    /// <param name="size">The size to align.</param>
    /// <returns>The aligned size.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint Align(uint size)
    {
        return (size + ((uint)sizeof(nint) - 1)) & ~((uint)sizeof(nint) - 1);
    }

    /// <summary>
    /// Searches the free lists for a block of the requested size.
    /// Splits oversized blocks and returns the remainder to the free list.
    /// </summary>
    /// <param name="size">Aligned allocation size in bytes.</param>
    /// <returns>Pointer to zeroed memory, or <c>null</c> if no suitable block is found.</returns>
    private static void* AllocFromFreeList(uint size)
    {
        if (!s_freeListsInitialized)
        {
            return null;
        }

        int sizeClass = -1;
        uint classSize = MinSizeClass;
        for (int i = 0; i < NumSizeClasses; i++, classSize <<= 1)
        {
            if (size <= classSize)
            {
                sizeClass = i;
                break;
            }
        }

        if (sizeClass < 0)
        {
            return null; // Too large
        }

        // Try this size class and larger
        for (int i = sizeClass; i < NumSizeClasses; i++)
        {
            FreeBlock* block = s_freeLists[i];
            if (block == null)
            {
                continue;
            }

            // Check each block in this class
            FreeBlock* prev = null;
            while (block != null)
            {
                if (block->Size >= size)
                {
                    uint remainder = (uint)(block->Size - size);

                    // Avoid unsplittable tail: skip this block if it would leave a tiny remainder
                    if (remainder != 0 && remainder < MinBlockSize)
                    {
                        prev = block;
                        block = block->Next;
                        continue;
                    }

                    // Remove from free list
                    if (prev != null)
                    {
                        prev->Next = block->Next;
                    }
                    else
                    {
                        s_freeLists[i] = block->Next;
                    }

                    s_freeListBytes -= (uint)block->Size;

                    // Split if remainder is usable
                    if (remainder >= MinBlockSize)
                    {
                        var split = (FreeBlock*)((byte*)block + size);
                        split->MethodTable = s_freeMethodTable;
                        split->Size = (int)remainder;
                        split->Next = null;
                        AddToFreeList(split, 'a');
                    }

                    // Clear and return
                    MemoryOp.MemSet((byte*)block, 0, (int)size);
                    s_totalAllocatedBytes += size;
                    return block;
                }

                prev = block;
                block = block->Next;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts bump allocation within a specific segment.
    /// </summary>
    /// <param name="segment">The segment to allocate from.</param>
    /// <param name="size">Number of bytes to allocate.</param>
    /// <returns>Pointer to the allocated memory, or <c>null</c> if the segment has insufficient space.</returns>
    private static void* BumpAllocInSegment(GCSegment* segment, uint size)
    {
        if (segment == null)
        {
            return null;
        }

        byte* newBump = segment->Bump + size;
        if (newBump <= segment->End)
        {
            void* result = segment->Bump;
            segment->Bump = newBump;
            segment->UsedSize += size;
            segment->MarkObject((nint)result);
            s_totalAllocatedBytes += size;
            s_currentSegment = segment;
            s_lastSegment = segment;
            return result;
        }

        return null;
    }

    /// <summary>
    /// Slow allocation path: walks the segments from <see cref="s_lastSegment"/> looking for space,
    /// then allocates a new segment if needed.
    /// </summary>
    /// <remarks>
    /// The segments before <see cref="s_lastSegment"/> are not walked again: the sweep orders the
    /// full ones first and <see cref="s_lastSegment"/> after them, and between collections it only
    /// moves forward, past segments that had no room for a request. Walking them from the head for
    /// every request made each one cost the whole heap, so that filling the heap took time in the
    /// square of its size; the room left at their end comes back with the next sweep.
    /// </remarks>
    /// <param name="size">Number of bytes to allocate.</param>
    /// <returns>Pointer to the allocated memory, or <c>null</c> if allocation fails.</returns>
    private static void* AllocateObjectSlow(uint size)
    {
        if (s_segmentManager.Segments == null)
        {
            return null;
        }

        if (s_lastSegment == null)
        {
            s_lastSegment = s_segmentManager.Segments;
        }

        for (GCSegment* seg = s_lastSegment; seg != null; seg = seg->Next)
        {
            void* result = BumpAllocInSegment(seg, size);
            if (result != null)
            {
                return result;
            }
        }

        // No segment fits: allocate and append a new segment at tail
        GCSegment* newSegment = s_segmentManager.AllocateSegment(size);

        if (newSegment == null)
        {
            return null;
        }
        s_lastSegment = newSegment;
        s_currentSegment = newSegment;

        return BumpAllocInSegment(newSegment, size);
    }
    // --- Raw variants (no s_totalAllocatedBytes increment) for TLAB refill ---

    /// <summary>
    /// Allocates from the free list without incrementing <see cref="s_totalAllocatedBytes"/>.
    /// Used by TLAB refill to avoid double-counting (individual objects are counted at TLAB alloc time).
    /// </summary>
    private static void* AllocFromFreeListRaw(uint size)
    {
        if (!s_freeListsInitialized)
        {
            return null;
        }

        int sizeClass = -1;
        uint classSize = MinSizeClass;
        for (int i = 0; i < NumSizeClasses; i++, classSize <<= 1)
        {
            if (size <= classSize)
            {
                sizeClass = i;
                break;
            }
        }

        if (sizeClass < 0)
        {
            return null;
        }

        for (int i = sizeClass; i < NumSizeClasses; i++)
        {
            FreeBlock* block = s_freeLists[i];
            if (block == null)
            {
                continue;
            }

            FreeBlock* prev = null;
            while (block != null)
            {
                if (block->Size >= size)
                {
                    uint remainder = (uint)(block->Size - size);

                    if (remainder != 0 && remainder < MinBlockSize)
                    {
                        prev = block;
                        block = block->Next;
                        continue;
                    }

                    if (prev != null)
                    {
                        prev->Next = block->Next;
                    }
                    else
                    {
                        s_freeLists[i] = block->Next;
                    }

                    s_freeListBytes -= (uint)block->Size;

                    if (remainder >= MinBlockSize)
                    {
                        FreeBlock* split = (FreeBlock*)((byte*)block + size);
                        split->MethodTable = s_freeMethodTable;
                        split->Size = (int)remainder;
                        split->Next = null;
                        AddToFreeList(split, 'r');
                    }

                    MemoryOp.MemSet((byte*)block, 0, (int)size);
                    return block;
                }

                prev = block;
                block = block->Next;
            }
        }

        return null;
    }

    /// <summary>
    /// Removes and returns a free-list block of at least <paramref name="minSize"/>
    /// bytes from the highest size class that has one, taken whole (no split) and
    /// zeroed: one of the largest blocks, at least half the size of the largest.
    /// TLAB-refill fallback: lets refills reuse the sub-TlabSize blocks that
    /// sweep leaves in partially live segments instead of growing the heap
    /// with a new segment. The unused tail is stamped back to the free list
    /// by the next <see cref="StampUnusedTlab"/> like any other TLAB gap.
    /// </summary>
    /// <remarks>
    /// It takes the first block of the class that fits rather than the largest: a
    /// class can hold every gap a sweep left, and finding the largest walked it
    /// whole for every refill.
    /// </remarks>
    private static void* AllocLargestFromFreeListRaw(uint minSize, out uint blockSize)
    {
        blockSize = 0;
        if (!s_freeListsInitialized)
        {
            return null;
        }

        for (int i = NumSizeClasses - 1; i >= 0; i--)
        {
            // Below the top class (which also takes larger blocks) a class holds no
            // block larger than its size: neither it nor any class under it can fit.
            if (i < NumSizeClasses - 1 && (MinSizeClass << i) < minSize)
            {
                break;
            }

            FreeBlock* prev = null;
            for (FreeBlock* block = s_freeLists[i]; block != null; block = block->Next)
            {
                if ((uint)block->Size >= minSize)
                {
                    if (prev != null)
                    {
                        prev->Next = block->Next;
                    }
                    else
                    {
                        s_freeLists[i] = block->Next;
                    }

                    s_freeListBytes -= (uint)block->Size;
                    blockSize = (uint)block->Size;
                    MemoryOp.MemSet((byte*)block, 0, (int)blockSize);
                    return block;
                }

                prev = block;
            }
        }

        return null;
    }

    /// <summary>
    /// Bump allocation in a segment without incrementing <see cref="s_totalAllocatedBytes"/>.
    /// Used by TLAB refill.
    /// </summary>
    private static void* BumpAllocInSegmentRaw(GCSegment* segment, uint size)
    {
        if (segment == null)
        {
            return null;
        }

        byte* newBump = segment->Bump + size;
        if (newBump <= segment->End)
        {
            void* result = segment->Bump;
            segment->Bump = newBump;
            segment->UsedSize += size;
            segment->MarkObject((nint)result);
            s_currentSegment = segment;
            s_lastSegment = segment;
            return result;
        }

        return null;
    }

    /// <summary>
    /// Slow allocation path without incrementing <see cref="s_totalAllocatedBytes"/>.
    /// Walks the segments from <see cref="s_lastSegment"/>, as <see cref="AllocateObjectSlow"/>
    /// does, and allocates a new one if needed. Used by TLAB refill.
    /// </summary>
    private static void* AllocateObjectSlowRaw(uint size)
    {
        if (s_segmentManager.Segments == null)
        {
            return null;
        }

        if (s_lastSegment == null)
        {
            s_lastSegment = s_segmentManager.Segments;
        }

        for (GCSegment* seg = s_lastSegment; seg != null; seg = seg->Next)
        {
            void* result = BumpAllocInSegmentRaw(seg, size);
            if (result != null)
            {
                return result;
            }
        }

        GCSegment* newSegment = s_segmentManager.AllocateSegment(size);
        if (newSegment == null)
        {
            return null;
        }

        s_lastSegment = newSegment;
        s_currentSegment = newSegment;

        return BumpAllocInSegmentRaw(newSegment, size);
    }

    /// <summary>
    /// Inserts a free block into the appropriate size-class free list.
    /// </summary>
    /// <param name="block">The free block to add.</param>
    /// <param name="source">Call-site tag for diagnostics: 'a'=AllocFromFreeList split,
    /// 'r'=AllocFromFreeListRaw split, 't'=TLAB gap stamp, 's'=sweep, 'p'=pinned heap.</param>
    private static void AddToFreeList(FreeBlock* block, char source)
    {
        if (!s_freeListsInitialized || block == null || block->Size < MinBlockSize)
        {
            return;
        }

        block->MethodTable = s_freeMethodTable;

        int sizeClass = -1;
        uint classSize = MinSizeClass;
        uint size = (uint)block->Size;
        for (int i = 0; i < NumSizeClasses; i++, classSize <<= 1)
        {
            if (size <= classSize)
            {
                sizeClass = i;
                break;
            }
        }

        if (sizeClass < 0)
        {
            sizeClass = NumSizeClasses - 1;
        }

        // Cheap last-line guard against re-inserting the current head: a second
        // head-insert of the same block would set block->Next = block, and the
        // next free-list walk would spin forever inside DisableInterrupts (this
        // is how the double TLAB-stamp bug hung Collect(); see StampUnusedTlab).
        // Deeper duplicates/overlaps need the O(list) debug tripwire this check
        // replaced — reintroduce it locally when hunting free-list corruption.
        if (s_freeLists[sizeClass] == block)
        {
            Serial.WriteString("[GC] BUG: duplicate free-list head insert src=");
            Serial.WriteString(source == 'a' ? "a" : source == 'r' ? "r" : source == 't' ? "t" : source == 's' ? "s" : "p");
            Serial.WriteString(" blk=0x");
            Serial.WriteHex((ulong)block);
            Serial.WriteString("\n");
            return;
        }

        block->Next = s_freeLists[sizeClass];
        s_freeLists[sizeClass] = block;
        s_freeListBytes += size;
    }
}
