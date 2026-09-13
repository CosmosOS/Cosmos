// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Frozen segment registration: pre-initialized read-only objects that are never collected.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    // --- Nested types ---

    /// <summary>
    /// Tracks a frozen segment registered by the runtime.
    /// Frozen segments contain pre-initialized objects that are never collected.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct FrozenSegmentInfo
    {
        /// <summary>
        /// Start address of the frozen segment.
        /// </summary>
        public nint Start;

        /// <summary>
        /// Number of bytes allocated within this segment.
        /// </summary>
        public nuint AllocSize;

        /// <summary>
        /// Number of bytes committed (backed by physical memory).
        /// </summary>
        public nuint CommitSize;

        /// <summary>
        /// Total reserved address space for this segment.
        /// </summary>
        public nuint ReservedSize;

        /// <summary>
        /// Pointer to the next frozen segment in the linked list.
        /// </summary>
        public FrozenSegmentInfo* Next;
    }

    // --- Static fields ---

    /// <summary>
    /// Head of the linked list of registered frozen segments.
    /// </summary>
    private static FrozenSegmentInfo* s_frozenSegments;

    /// <summary>
    /// Current metadata page used to bump-allocate <see cref="FrozenSegmentInfo"/> structs.
    /// </summary>
    private static byte* s_frozenSegmentMetadataPage;

    /// <summary>
    /// Current offset into <see cref="s_frozenSegmentMetadataPage"/>.
    /// </summary>
    private static int s_frozenSegmentMetadataOffset;

    // --- Methods ---

    /// <summary>
    /// Checks if a pointer falls within any registered frozen segment.
    /// </summary>
    /// <param name="ptr">The pointer to test.</param>
    /// <returns><c>true</c> if <paramref name="ptr"/> is inside a frozen segment; otherwise, <c>false</c>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsInFrozenSegment(nint ptr)
    {
        FrozenSegmentInfo* segment = s_frozenSegments;
        while (segment != null)
        {
            if (ptr >= segment->Start && ptr < segment->Start + (nint)segment->AllocSize)
            {
                return true;
            }

            segment = segment->Next;
        }

        return false;
    }

    /// <summary>
    /// Registers a frozen segment with the GC. Called by the runtime during startup
    /// for pre-initialized data segments that should never be collected.
    /// </summary>
    /// <param name="pSegmentStart">Start address of the frozen segment.</param>
    /// <param name="allocSize">Allocated size in bytes.</param>
    /// <param name="commitSize">Committed size in bytes.</param>
    /// <param name="reservedSize">Reserved size in bytes.</param>
    /// <returns>The segment start address (used as a segment handle by the runtime).</returns>
    internal static nint RegisterFrozenSegment(nint pSegmentStart, nuint allocSize, nuint commitSize, nuint reservedSize)
    {
        // Allocate info structure from unmanaged memory (frozen segments are pre-allocated)
        // Use a simple bump allocator for metadata
        int infoSize = sizeof(FrozenSegmentInfo);
        const int pageSize = (int)PageAllocator.PageSize;

        // Allocate a page if we don't have one, or use existing
        if (s_frozenSegmentMetadataPage == null || s_frozenSegmentMetadataOffset + infoSize > pageSize)
        {
            s_frozenSegmentMetadataPage = (byte*)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
            if (s_frozenSegmentMetadataPage == null)
            {
                Serial.WriteString("[GC] ERROR: Failed to allocate frozen segment metadata page\n");
                return pSegmentStart;
            }

            s_frozenSegmentMetadataOffset = 0;
        }

        var info = (FrozenSegmentInfo*)(s_frozenSegmentMetadataPage + s_frozenSegmentMetadataOffset);
        s_frozenSegmentMetadataOffset += infoSize;

        info->Start = pSegmentStart;
        info->AllocSize = allocSize;
        info->CommitSize = commitSize;
        info->ReservedSize = reservedSize;
        info->Next = s_frozenSegments;

        s_frozenSegments = info;

        Serial.WriteString("[GC] Registered frozen segment at 0x");
        Serial.WriteHex((ulong)pSegmentStart);
        Serial.WriteString(", size: ");
        Serial.WriteNumber((uint)allocSize);
        Serial.WriteString("\n");

        return pSegmentStart;
    }

    /// <summary>
    /// Updates a previously registered frozen segment with new allocation and commit sizes.
    /// Called by the runtime when additional objects are placed into the segment.
    /// </summary>
    /// <param name="seg">The segment start address (handle returned by <see cref="RegisterFrozenSegment"/>).</param>
    /// <param name="allocated">New allocated size.</param>
    /// <param name="committed">New committed size.</param>
    internal static void UpdateFrozenSegment(nint seg, nint allocated, nint committed)
    {
        // Find the segment and update its committed size
        FrozenSegmentInfo* segment = s_frozenSegments;
        while (segment != null)
        {
            if (segment->Start == seg)
            {
                segment->CommitSize = (nuint)committed;
                segment->AllocSize = (nuint)allocated;
                break;
            }

            segment = segment->Next;
        }
    }
}
