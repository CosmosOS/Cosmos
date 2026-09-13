// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Pinned object heap: allocation, sweeping, and segment management for pinned objects.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    // --- Constants ---

    /// <summary>
    /// Minimum size for a pinned heap segment (one page).
    /// </summary>
    private const uint PinnedHeapMinSize = (uint)PageAllocator.PageSize;

    /// <summary>
    /// Current pinned segment used for bump allocation.
    /// </summary>
    private static GCSegment* s_currentPinnedSegment;

    // --- Methods ---

    /// <summary>
    /// Allocates a pinned object on the pinned heap.
    /// Pinned objects are not subject to compaction but can still be collected when unreachable.
    /// </summary>
    /// <param name="size">Requested object size in bytes.</param>
    /// <param name="flags">Allocation flags from the runtime.</param>
    /// <returns>Pointer to the allocated object, or <c>null</c> if allocation fails.</returns>
    private static GCObject* AllocPinnedObject(nint size, GC_ALLOC_FLAGS flags)
    {
        uint allocSize = Align((uint)size);
        if (allocSize < MinBlockSize)
        {
            allocSize = MinBlockSize;
        }

        // Try bump allocation in current pinned segment
        void* result = BumpAllocInPinnedSegment(s_currentPinnedSegment, allocSize);
        if (result != null)
        {
            return (GCObject*)result;
        }

        // Need new segment
        GCSegment* newSegment = s_pinnedSegmentManager.AllocateSegment(allocSize);
        if (newSegment == null)
        {
            return null;
        }

        s_currentPinnedSegment = newSegment;

        // Try allocation again
        result = BumpAllocInPinnedSegment(s_currentPinnedSegment, allocSize);
        return (GCObject*)result;
    }

    /// <summary>
    /// Attempts bump allocation in a pinned segment.
    /// </summary>
    /// <param name="segment">The pinned segment to allocate from.</param>
    /// <param name="size">Number of bytes to allocate.</param>
    /// <returns>Pointer to the allocated memory, or <c>null</c> if the segment has insufficient space.</returns>
    private static void* BumpAllocInPinnedSegment(GCSegment* segment, uint size)
    {
        if (segment == null)
        {
            return null;
        }

        byte* newBump = segment->Bump + size;
        if (newBump <= segment->End)
        {
            void* result = segment->Bump;
            segment->MarkObject((nint)result);
            segment->Bump = newBump;
            segment->UsedSize += size;
            s_totalAllocatedBytes += size;
            s_pinnedHeapObjectCount++;
            return result;
        }

        return null;
    }

    /// <summary>
    /// Checks if a pointer falls within any pinned heap segment.
    /// </summary>
    /// <param name="ptr">The pointer to test.</param>
    /// <returns><c>true</c> if <paramref name="ptr"/> is inside a pinned segment; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInPinnedHeap(nint ptr)
    {
        byte* p = (byte*)ptr;
        GCSegment* segment = s_pinnedSegmentManager.Segments;
        while (segment != null)
        {
            if (p >= segment->Start && p < segment->End)
            {
                return true;
            }

            segment = segment->Next;
        }

        return false;
    }

    /// <summary>
    /// Sweeps all pinned heap segments, collecting unmarked objects.
    /// </summary>
    /// <returns>The number of objects freed across all pinned segments.</returns>
    private static int SweepPinnedHeap()
    {
        int freed = 0;
        GCSegment* segment = s_pinnedSegmentManager.Segments;
        while (segment != null)
        {
            freed += SweepPinnedSegment(segment);
            segment = segment->Next;
        }

        return freed;
    }

    /// <summary>
    /// Sweeps a single pinned segment, freeing unmarked objects and coalescing free runs.
    /// </summary>
    /// <param name="segment">The pinned segment to sweep.</param>
    /// <returns>The number of objects freed in this segment.</returns>
    private static int SweepPinnedSegment(GCSegment* segment)
    {
        int freed = 0;
        byte* ptr = segment->Start;
        byte* freeRunStart = null;
        uint freeRunSize = 0;

        while (ptr < segment->Bump)
        {
            var obj = (GCObject*)ptr;

            // Validate MethodTable. Anything that is not a plausible MethodTable —
            // null (zeroed gap), a value below kernel space (data, e.g. the runtime
            // object header written at objRef-4 of the following object), or a stale
            // pointer into the GC heap — is dead filler. Fold it into the free run so
            // the run stays contiguous and the trailing reset can reach Bump.
            MethodTable* mt = obj->GetMethodTable();
            if (mt == null || (ulong)mt < AddressSpace.KernelSpaceStart || IsInGCHeap((nint)mt))
            {
                if (freeRunStart == null)
                {
                    freeRunStart = ptr;
                }

                freeRunSize += (uint)sizeof(nint);
                ptr += sizeof(nint);
                continue;
            }

            uint objSize = Align(obj->ComputeSize());
            if (objSize == 0 || objSize > (uint)(segment->End - ptr))
            {
                break;
            }

            if (obj->IsMarked)
            {
                // Live pinned object - unmark it
                obj->Unmark();

                // Flush free run
                FlushPinnedFreeRun(freeRunStart, freeRunSize);
                freeRunStart = null;
                freeRunSize = 0;
            }
            else
            {
                // Dead pinned object - add to free run
                freed++;
                if (s_pinnedHeapObjectCount > 0)
                {
                    s_pinnedHeapObjectCount--;
                }

                if (freeRunStart == null)
                {
                    freeRunStart = ptr;
                }

                freeRunSize += objSize;
            }

            ptr += objSize;
        }

        // Handle trailing free space
        if (freeRunStart != null)
        {
            if (freeRunStart + freeRunSize >= segment->Bump)
            {
                segment->Bump = freeRunStart;
                segment->UsedSize = (uint)(freeRunStart - segment->Start);
            }
            else
            {
                FlushPinnedFreeRun(freeRunStart, freeRunSize);
            }
        }

        return freed;
    }

    /// <summary>
    /// Converts a contiguous free run in the pinned heap into a <see cref="FreeBlock"/>
    /// and adds it to the free list. Like <see cref="FlushFreeRun"/>, the last
    /// <see cref="ReservedHeaderSlotSize"/> bytes are excluded so the following object's
    /// runtime header slot (objRef-4) can never be recycled into another allocation.
    /// </summary>
    /// <param name="start">Start of the free run.</param>
    /// <param name="size">Size of the free run in bytes.</param>
    private static void FlushPinnedFreeRun(byte* start, uint size)
    {
        if (start == null || size < MinBlockSize + ReservedHeaderSlotSize)
        {
            return;
        }

        size -= ReservedHeaderSlotSize;
        SanitizeReservedHeaderSlot(start + size);

        var freeBlock = (FreeBlock*)start;
        freeBlock->MethodTable = s_freeMethodTable;
        freeBlock->Size = (int)size;
        freeBlock->Next = null;
        AddToFreeList(freeBlock, 'p');
    }

    /// <summary>
    /// Reorders pinned segments (FULL, then SEMI-FULL, then FREE) and releases
    /// fully empty multi-page segments back to the page allocator.
    /// </summary>
    private static void ReorderPinnedSegmentsAndFreeEmpty()
    {
        GCSegment* fullHead = null;
        GCSegment* fullTail = null;
        GCSegment* semiHead = null;
        GCSegment* semiTail = null;
        GCSegment* freeHead = null;
        GCSegment* freeTail = null;
        GCSegment* seg = s_pinnedSegmentManager.Segments;

        while (seg != null)
        {
            GCSegment* next = seg->Next;

            bool isFree = seg->UsedSize == 0 || seg->Bump == seg->Start;
            bool isFull = seg->Bump >= seg->End;

            if (isFree && seg->TotalSize > PageAllocator.PageSize)
            {
                PageAllocator.Free(seg);
            }
            else
            {
                seg->Next = null;

                if (isFull)
                {
                    if (fullHead == null) { fullHead = seg; }
                    else { fullTail->Next = seg; }
                    fullTail = seg;
                }
                else if (isFree)
                {
                    if (freeHead == null) { freeHead = seg; }
                    else { freeTail->Next = seg; }
                    freeTail = seg;
                }
                else
                {
                    if (semiHead == null) { semiHead = seg; }
                    else { semiTail->Next = seg; }
                    semiTail = seg;
                }
            }

            seg = next;
        }

        GCSegment* newHead = null;
        GCSegment* tail = null;

        if (fullHead != null)
        {
            newHead = fullHead;
            tail = fullTail;
        }

        if (semiHead != null)
        {
            if (newHead == null) { newHead = semiHead; }
            else { tail->Next = semiHead; }
            tail = semiTail;
        }

        if (freeHead != null)
        {
            if (newHead == null) { newHead = freeHead; }
            else { tail->Next = freeHead; }
            tail = freeTail;
        }

        s_pinnedSegmentManager.Segments = newHead;
        s_currentPinnedSegment = semiHead != null ? semiHead : freeHead;

        s_heapRangeDirty = true;
    }
}
