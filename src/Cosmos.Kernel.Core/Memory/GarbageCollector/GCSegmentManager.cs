// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

internal unsafe struct GCSegmentManager
{
    /// <summary>
    /// Default segment size. Grows as needed.
    /// </summary>
    private static readonly uint s_minSegmentSize = (uint)PageAllocator.PageSize;

    /// <summary>
    /// Bytes excluded from the tail of every <see cref="GarbageCollector.FreeBlock"/> so the runtime object
    /// header slot (objRef-4) of whatever object follows the block can never be handed to
    /// another allocation (which would zero or overwrite a stored hash / lock word).
    /// </summary>
    private const uint ReservedHeaderSlotSize = 8;

    public GCSegment* Segments { get; internal set; }
    public GCSegment* TailSegment { get; internal set; }

    /// <summary>
    /// Allocates a new GC segment backed by page-allocated memory.
    /// </summary>
    /// <param name="requestedSize">Minimum usable size in bytes.</param>
    /// <returns>Pointer to the initialized segment, or <c>null</c> if page allocation fails.</returns>
    public GCSegment* AllocateSegment(uint requestedSize)
    {
        uint size = requestedSize < s_minSegmentSize ? s_minSegmentSize : requestedSize;
        long totalSlots = size / IntPtr.Size;
        uint brickTableLength = Align((uint)((totalSlots + GCSegment.SlotsPerChunk - 1) / GCSegment.SlotsPerChunk));
        uint totalSize = size + (uint)sizeof(GCSegment) + ReservedHeaderSlotSize + brickTableLength;
        ulong pageCount = (totalSize + PageAllocator.PageSize - 1) / PageAllocator.PageSize;

        var memory = (byte*)PageAllocator.AllocPages(PageType.GCHeap, pageCount, true);
        if (memory == null)
        {
            return null;
        }

        var segment = (GCSegment*)memory;
        segment->Next = null;

        AppendSegment(segment);

        // Pad Start so the first object's runtime header write (objRef-4) lands in
        // zeroed filler instead of the segment struct's last field (UsedSize).
        //segment->BrickTable = memory + Align((uint)sizeof(GCSegment));
        segment->Start = memory + Align((uint)sizeof(GCSegment)) + ReservedHeaderSlotSize + brickTableLength;
        segment->End = memory + (pageCount * PageAllocator.PageSize);
        segment->Bump = segment->Start;
        segment->TotalSize = (uint)(segment->End - segment->Start);
        segment->UsedSize = 0;

        return segment;
    }

    /// <summary>
    /// Adds a new segment to the end of the linked list of segments managed by this instance.
    /// </summary>
    /// <param name="segment">The segment to add.</param>
    public void AppendSegment(GCSegment* segment)
    {
        if (Segments == null)
        {
            Segments = segment;
            TailSegment = segment;
        }
        else
        {
            TailSegment->Next = segment;
            TailSegment = segment;
        }
    }

    /// <summary>
    /// Frees a GC segment.
    /// </summary>
    /// <param name="segment">The segment to free.</param>
    public void FreeSegment(GCSegment* segment)
    {
        // Segment pointer is the same as the page pointer.
        PageAllocator.Free(segment);
    }

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
}
