// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.IO;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Sweep phase: segment sweeping and heap range helpers.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    /// <summary>
    /// Executes the sweep phase across the GC-managed heaps: GC segments and the
    /// pinned heap. The unmanaged malloc heaps (Small/Medium/Large) must never be
    /// swept: managed allocations cannot live there, and a live block whose first
    /// word coincidentally holds a GC-heap pointer is indistinguishable from an
    /// unmarked object header (issue #386).
    /// </summary>
    /// <returns>Total number of objects freed.</returns>
    private static int SweepPhase()
    {
        int totalFreed = 0;

        GCSegment* segment = s_segmentManager.Segments;
        while (segment != null)
        {
            totalFreed += SweepSegment(segment);
            segment = segment->Next;
        }

        totalFreed += SweepPinnedHeap();

        return totalFreed;
    }

    /// <summary>
    /// Sweeps a single GC segment, freeing unmarked objects and coalescing adjacent dead objects
    /// into free blocks. Trailing dead objects reclaim bump pointer space.
    /// </summary>
    /// <param name="segment">The segment to sweep.</param>
    /// <returns>The number of objects freed in this segment.</returns>
    private static int SweepSegment(GCSegment* segment)
    {
        int freed = 0;
        byte* ptr = segment->Start;
        byte* freeRunStart = null;
        uint freeRunSize = 0;

        while (ptr < segment->Bump)
        {
            var obj = (GCObject*)ptr;

            // Get MethodTable (mask off mark bit)
            MethodTable* mt = obj->GetMethodTable();

            // Check if this is a free block from previous GC
            if (mt == s_freeMethodTable)
            {
                var freeBlock = (FreeBlock*)ptr;
                uint blockSize = (uint)freeBlock->Size;
                if (blockSize == 0 || blockSize > (uint)(segment->End - ptr))
                {
                    break;
                }

                // Accumulate into free run
                if (freeRunStart == null)
                {
                    freeRunStart = ptr;
                }

                freeRunSize += blockSize;

                ptr += blockSize;
                continue;
            }

            // Anything that is not a plausible MethodTable is dead filler:
            //  - null: zeroed TLAB gap (< MinBlockSize, zeroed by StampUnusedTlab);
            //  - below AddressSpace.KernelSpaceStart: data, most commonly the runtime object header
            //    (identity hash / thin lock) written at objRef-4 of an object that
            //    directly follows a gap — the word then reads as header << 32;
            //  - inside the GC heap: a stale interior pointer.
            // Fold it into the free run and keep walking: breaking here would strand
            // everything up to Bump, so the trailing reset never fires and the segment
            // can never be returned to the page allocator. Dereferencing it (the old
            // behavior for non-null values) faulted on non-canonical addresses (#382 GP).
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
                // Live object - unmark it
                obj->Unmark();

                // Flush accumulated free run as a FreeBlock
                FlushFreeRun(freeRunStart, freeRunSize);
                freeRunStart = null;
                freeRunSize = 0;
            }
            else
            {
                // Dead object - add to free run
                freed++;

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
                FlushFreeRun(freeRunStart, freeRunSize);
            }
        }

        return freed;
    }

    /// <summary>
    /// Converts a contiguous free run into a <see cref="FreeBlock"/> and adds it to the free list.
    /// The last <see cref="ReservedHeaderSlotSize"/> bytes of the run are excluded from the block:
    /// they may hold the runtime object header (objRef-4) of the object that follows the run,
    /// which must survive block recycling (<see cref="AllocFromFreeList"/> zeroes what it hands out).
    /// </summary>
    /// <param name="start">Start of the free run.</param>
    /// <param name="size">Size of the free run in bytes.</param>
    private static void FlushFreeRun(byte* start, uint size)
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
        AddToFreeList(freeBlock, 's');
    }

    /// <summary>
    /// Prepares the 8-byte tail slot excluded from a free block. The high 4 bytes are the
    /// following object's runtime header (identity hash / thin lock) and must survive; the
    /// low 4 bytes are dead. Clears any leftover value that could still read as a kernel
    /// pointer (a stale reference in a dead object's tail) so the sweep walk can never
    /// misparse the slot as an object — real header words always stay below
    /// <see cref="AddressSpace.KernelSpaceStart"/>.
    /// </summary>
    /// <param name="slot">Address of the 8-byte reserved slot.</param>
    private static void SanitizeReservedHeaderSlot(byte* slot)
    {
        *(uint*)slot = 0;
        if (*(ulong*)slot >= AddressSpace.KernelSpaceStart)
        {
            *(ulong*)slot = 0;
        }
    }

    /// <summary>
    /// Reorders GC segments (FULL, then SEMI-FULL, then FREE) and releases
    /// fully empty multi-page segments back to the page allocator.
    /// </summary>
    private static void ReorderSegmentsAndFreeEmpty()
    {
        GCSegment* fullHead = null;
        GCSegment* fullTail = null;
        GCSegment* semiHead = null;
        GCSegment* semiTail = null;
        GCSegment* freeHead = null;
        GCSegment* freeTail = null;
        GCSegment* seg = s_segmentManager.Segments;

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

        s_segmentManager.Segments = newHead;
        s_segmentManager.TailSegment = tail;
        s_lastSegment = semiHead != null ? semiHead : freeHead;
        s_currentSegment = s_lastSegment;
    }

    // --- Helpers ---

    /// <summary>
    /// Checks if a pointer falls within any GC heap segment (including pinned segments).
    /// </summary>
    /// <param name="ptr">The pointer to test.</param>
    /// <returns><c>true</c> if <paramref name="ptr"/> is inside a GC or pinned heap segment; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInGCHeap(nint ptr)
    {
        return GetSegmentContaining((byte*)ptr) != null;
    }

    /// <summary>
    /// Whether an object survives the collection under way: marked, or outside the GC heap (a
    /// frozen object, never collected and never marked).
    /// </summary>
    /// <param name="obj">The object, not null.</param>
    /// <returns><c>true</c> if <paramref name="obj"/> is marked or not in a GC or pinned heap segment.</returns>
    internal static bool IsLive(GCObject* obj)
    {
        return obj->IsMarked || !IsInGCHeap((nint)obj);
    }

    /// <summary>
    /// Finds the segment, regular or pinned, whose object area contains a pointer.
    /// </summary>
    /// <remarks>
    /// Both segment managers take their segments from the page allocator as
    /// <see cref="PageType.GCHeap"/> runs, with the <see cref="GCSegment"/> at the start of the run,
    /// so the page allocator's table finds the segment without walking the segment lists. Segments
    /// are sized for one TLAB refill or one large allocation, so a full heap has tens of thousands of
    /// them, and the mark phase looks up every reference it follows: a walk per lookup made a
    /// collection take time in the square of the heap size.
    /// </remarks>
    /// <param name="ptr">The pointer to look up.</param>
    /// <returns>The segment, or <c>null</c> when the pointer is in no segment's object area.</returns>
    private static GCSegment* GetSegmentContaining(byte* ptr)
    {
        if (PageAllocator.GetAllocation(ptr, out byte* start) != PageType.GCHeap)
        {
            return null;
        }

        var segment = (GCSegment*)start;
        return ptr >= segment->Start && ptr < segment->End ? segment : null;
    }

    /// <summary>
    /// One step of a walk over a segment's objects: the size of the entry at
    /// <paramref name="ptr"/>.
    /// </summary>
    /// <remarks>
    /// The walk steps with the rules of <see cref="SweepSegment"/>, so the objects it finds are the
    /// ones the sweep also visits as object starts and unmarks: a <see cref="FreeBlock"/> advances by
    /// its <see cref="FreeBlock.Size"/>, a header word that cannot be a MethodTable (zeroed TLAB gap,
    /// reserved header slot, stale heap pointer) by one pointer, and an object by its size rounded up
    /// the way the allocator rounds it. <see cref="GCObject.ComputeSize"/> alone is not a stride:
    /// strings and byte, char and short arrays have sizes that are not pointer multiples.
    /// </remarks>
    /// <param name="segment">The segment being walked.</param>
    /// <param name="ptr">The entry, between the segment's start and its bump pointer.</param>
    /// <param name="isObject">Whether the entry is an object.</param>
    /// <returns>
    /// The size of the entry, or 0 where the sweep stops: nothing past it is ever an object start.
    /// </returns>
    private static uint GetHeapEntrySize(GCSegment* segment, byte* ptr, out bool isObject)
    {
        var obj = (GCObject*)ptr;

        // Masked: walks also run mid-mark, when objects already reached carry the mark bit.
        MethodTable* mt = obj->GetMethodTable();
        isObject = false;
        uint size;

        if (mt == s_freeMethodTable)
        {
            size = (uint)((FreeBlock*)ptr)->Size;
        }
        else if (mt == null || (ulong)mt < AddressSpace.KernelSpaceStart || IsInGCHeap((nint)mt))
        {
            size = (uint)sizeof(nint);
        }
        else
        {
            size = Align(obj->ComputeSize());
            isObject = true;
        }

        return size == 0 || size > (uint)(segment->End - ptr) ? 0 : size;
    }
}
