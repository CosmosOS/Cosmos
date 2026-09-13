// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;
using Internal.Runtime;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

#pragma warning disable CS8500

/// <summary>
/// Mark-and-sweep garbage collector with free list allocation.
/// Manages GC heap segments, pinned heap, frozen segments, and GC handles.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    // --- Nested types ---

    /// <summary>
    /// Represents a free block in the GC heap, linked into size-class free lists.
    /// Laid out to be walkable like a <see cref="GCObject"/> (MethodTable at offset 0, Size at offset 8).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct FreeBlock
    {
        /// <summary>
        /// Points to <see cref="s_freeMethodTable"/> to identify this block as free during heap walks.
        /// </summary>
        public MethodTable* MethodTable;

        /// <summary>
        /// Total size of this free block in bytes (occupies the same position as <see cref="GCObject.Length"/>).
        /// </summary>
        public int Size;

        /// <summary>
        /// Next free block in this size class bucket.
        /// </summary>
        public FreeBlock* Next;
    }

    /// <summary>
    /// Marker type whose MethodTable is used to tag free blocks in the heap.
    /// </summary>
    internal struct FreeMarker { }

    /// <summary>
    /// Describes a value-type series within a GCDesc for arrays of structs containing references.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ValSerieItem
    {
        /// <summary>
        /// Number of pointer-sized reference fields in this series.
        /// </summary>
        public uint Nptrs;

        /// <summary>
        /// Number of bytes to skip after the reference fields.
        /// </summary>
        public uint Skip;
    }

    /// <summary>
    /// Describes a reference series within a GCDesc stored before the MethodTable.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct GCDescSeries
    {
        /// <summary>
        /// Size of the series relative to the object size. Added to object size to get byte count.
        /// </summary>
        public nint SeriesSize;

        /// <summary>
        /// Byte offset from the object base where this series begins.
        /// </summary>
        public nint StartOffset;
    }

    // --- Constants ---

    /// <summary>
    /// Number of free list size classes (powers of two: 16, 32, 64, ... 32768).
    /// </summary>
    private const int NumSizeClasses = 12;

    /// <summary>
    /// Smallest free list size class in bytes.
    /// </summary>
    private const uint MinSizeClass = 16;

    /// <summary>
    /// Minimum block size in bytes. Must be large enough to hold a <see cref="FreeBlock"/> header (24 bytes on x64).
    /// </summary>
    private const uint MinBlockSize = 24;

    /// <summary>
    /// Bytes excluded from the tail of every <see cref="FreeBlock"/> so the runtime object
    /// header slot (objRef-4) of whatever object follows the block can never be handed to
    /// another allocation (which would zero or overwrite a stored hash / lock word).
    /// </summary>
    private const uint ReservedHeaderSlotSize = 8;

    // --- Static fields ---

    /// <summary>
    /// Array of free list heads, indexed by size class.
    /// </summary>
    private static FreeBlock** s_freeLists;

    /// <summary>
    /// Whether the free list array has been allocated and initialized.
    /// </summary>
    private static bool s_freeListsInitialized;

    /// <summary>
    /// MethodTable pointer used to tag <see cref="FreeBlock"/> entries in the heap.
    /// </summary>
    private static MethodTable* s_freeMethodTable;

    /// <summary>
    /// Segment currently being used for bump allocation.
    /// </summary>
    private static GCSegment* s_currentSegment;

    /// <summary>
    /// Last segment where allocation succeeded (used as a fast-path hint).
    /// </summary>
    private static GCSegment* s_lastSegment;

    /// <summary>
    /// Segment Manager of the GC Heap
    /// </summary>
    private static GCSegmentManager s_segmentManager = new();
    /// <summary>
    /// Segment Manager for the pinned segments.
    /// </summary>
    private static GCSegmentManager s_pinnedSegmentManager = new();
    /// <summary>
    /// GCHandle Manager.
    /// </summary>
    internal static GCHandleManager s_gCHandleManager = new();

    /// <summary>
    /// Default segment size. Grows as needed.
    /// </summary>
    private static uint s_maxSegmentSize = (uint)PageAllocator.PageSize;

    /// <summary>
    /// Lowest address across all GC segments (for fast heap range pre-check).
    /// </summary>
    private static byte* s_gcHeapMin;

    /// <summary>
    /// Highest address across all GC segments (for fast heap range pre-check).
    /// </summary>
    private static byte* s_gcHeapMax;

    /// <summary>
    /// Set to <c>true</c> when segments are added or removed, triggering a range recomputation.
    /// </summary>
    private static bool s_heapRangeDirty;

    /// <summary>
    /// Stack used during the mark phase for iterative object traversal.
    /// </summary>
    private static nint* s_markStack;

    /// <summary>
    /// Maximum number of entries the mark stack can hold.
    /// </summary>
    private static int s_markStackCapacity;

    /// <summary>
    /// Current number of entries in the mark stack.
    /// </summary>
    private static int s_markStackCount;

    /// <summary>
    /// Number of pages currently backing the mark stack.
    /// </summary>
    private static ulong s_markStackPageCount = 1;

    /// <summary>
    /// Whether the GC has been initialized.
    /// </summary>
    private static bool s_initialized = false;

    /// <summary>
    /// Total number of collections performed since initialization.
    /// </summary>
    private static int s_totalCollections;

    /// <summary>
    /// Cumulative number of objects freed across all collections.
    /// </summary>
    private static int s_totalObjectsFreed;

    /// <summary>
    /// Last GC duration in timestamp ticks (Stopwatch ticks).
    /// </summary>
    private static long s_lastGCDurationTicks;

    /// <summary>
    /// Interval between last GC end and previous GC end in ticks.
    /// </summary>
    private static long s_lastGCIntervalTicks;

    /// <summary>
    /// Timestamp of last GC end (Stopwatch ticks).
    /// </summary>
    private static long s_lastGCEndTick;

    // Last recorded generation 0 metrics (before/after last collection)
    private static ulong s_lastGen0SizeBefore;
    private static ulong s_lastGen0FragmentationBefore;
    private static ulong s_lastGen0SizeAfter;
    private static ulong s_lastGen0FragmentationAfter;

    /// <summary>
    /// Cumulative total of all bytes ever allocated through the GC.
    /// Increments on every allocation, never decrements.
    /// Used by <c>RhGetTotalAllocatedBytes</c> / <c>GC.GetTotalAllocatedBytes()</c>.
    /// </summary>
    private static ulong s_totalAllocatedBytes;

    /// <summary>
    /// Number of live objects currently on the pinned object heap.
    /// Incremented on pinned allocation, decremented on pinned sweep free.
    /// </summary>
    private static ulong s_pinnedHeapObjectCount;

    /// <summary>
    /// Fallback allocation context used when the scheduler is not enabled (single-thread mode).
    /// </summary>
    private static AllocContext s_fallbackAllocContext;

    /// <summary>
    /// Cumulative unused TLAB bytes from dead threads. Subtracted from total allocation stats.
    /// </summary>
    private static ulong s_deadThreadsNonAllocBytes;

    // --- Properties ---

    /// <summary>
    /// Gets a value indicating whether the garbage collector is enabled.
    /// </summary>
    public static bool IsEnabled
    {
        get
        {
            return s_initialized;
        }
    }

    // --- Public methods ---

    /// <summary>
    /// Initializes the garbage collector, allocating the free list array, initial segment, and mark stack.
    /// </summary>
    public static void Initialize()
    {
        if (s_initialized)
        {
            return;
        }

        Serial.WriteString("[GC] Initializing with free list allocator\n");

        // Allocate free list array using page allocator (not GC heap)
        s_freeLists = (FreeBlock**)PageAllocator.AllocPages(PageType.Unmanaged, 1, true);
        if (s_freeLists == null)
        {
            Serial.WriteString("[GC] ERROR: Failed to allocate free lists\n");
            return;
        }

        for (int i = 0; i < NumSizeClasses; i++)
        {
            s_freeLists[i] = null;
        }

        s_freeListsInitialized = true;

        // Get the free marker MethodTable
        s_freeMethodTable = MethodTable.Of<FreeMarker>();

        // Allocate initial segment
        s_currentSegment = s_segmentManager.AllocateSegment(s_maxSegmentSize);
        s_lastSegment = s_currentSegment;
        s_heapRangeDirty = true;
        RecomputeHeapRange();
        if (s_segmentManager.Segments == null)
        {
            Serial.WriteString("[GC] ERROR: Failed to allocate initial segment\n");
            return;
        }

        // Allocate mark stack. Capacity MUST match the backing allocation:
        // one 4K page holds 512 nint entries, and PushMarkStack bounds-checks
        // against s_markStackCapacity before growing — a larger capacity here
        // means the mark phase writes past the page and corrupts adjacent
        // allocations as soon as more than 512 objects are pending.
        s_markStack = (nint*)PageAllocator.AllocPages(PageType.Unmanaged, s_markStackPageCount, true);
        s_markStackCapacity = (int)(s_markStackPageCount * PageAllocator.PageSize / (ulong)sizeof(nint));
        if (s_markStack == null)
        {
            Serial.WriteString("[GC] ERROR: Failed to allocate mark stack\n");
            return;
        }

        s_markStackCount = 0;
        s_gCHandleManager.InitializeGCHandleStore();

        s_initialized = true;
        Serial.WriteString("[GC] Initialization complete\n");
    }

    /// <summary>
    /// Performs a full garbage collection: mark, sweep, and segment reordering.
    /// </summary>
    /// <returns>The number of objects freed during this collection.</returns>
    public static int Collect()
    {
        if (!s_initialized)
        {
            return 0;
        }

        // TODO: Add compilation option to disable metric collection
        int freedCount;
        using (InternalCpu.DisableInterruptsScope())
        {
            // Record GC start timestamp
            long gcStart = Stopwatch.GetTimestamp();

            Serial.WriteString("[GC] Collection #");
            Serial.WriteNumber((uint)s_totalCollections + 1);
            Serial.WriteString("\n");

            // Return all TLABs before collection — stamps unused gaps as FreeBlocks
            ReturnAllAllocContexts();

            // Record pre-GC metrics
            s_lastGen0SizeBefore = GetGenerationSize(0);
            s_lastGen0FragmentationBefore = GetCurrentFragmentation(0);

            // Clear free lists - will be rebuilt during sweep
            for (int i = 0; i < NumSizeClasses; i++)
            {
                s_freeLists[i] = null;
            }

            // Mark reachable objects
            MarkPhase();

            // Free Weak GC Handles
            s_gCHandleManager.FreeWeakHandles();

            // Sweep and rebuild free lists
            freedCount = SweepPhase();

            // Reorder segments and free empty ones
            ReorderSegmentsAndFreeEmpty();
            ReorderPinnedSegmentsAndFreeEmpty();
            RecomputeHeapRange();

            // Record post-GC metrics
            s_lastGen0SizeAfter = GetGenerationSize(0);
            s_lastGen0FragmentationAfter = GetCurrentFragmentation(0);

            s_totalCollections++;
            s_totalObjectsFreed += freedCount;

            Serial.WriteString("[GC] Freed ");
            Serial.WriteNumber((uint)freedCount);
            Serial.WriteString(" objects\n");

            // Record GC end timestamp and compute duration/interval
            long gcEnd = Stopwatch.GetTimestamp();
            long duration = gcEnd - gcStart;
            long interval = s_lastGCEndTick == 0 ? duration : gcEnd - s_lastGCEndTick;
            s_lastGCDurationTicks = duration;
            s_lastGCIntervalTicks = interval;
            s_lastGCEndTick = gcEnd;
        }

        return freedCount;
    }
    // --- Internal methods ---

    /// <summary>
    /// Allocates memory for a managed object. Called by the runtime allocation helpers.
    /// Uses per-thread TLAB fast path for non-pinned allocations.
    /// </summary>
    /// <param name="size">Requested object size in bytes.</param>
    /// <param name="flags">Runtime allocation flags (e.g., pinned object heap).</param>
    /// <returns>Pointer to the allocated object, or <c>null</c> if allocation fails.</returns>
    internal static GCObject* AllocObject(nint size, GC_ALLOC_FLAGS flags)
    {
        if (!s_initialized)
        {
            Initialize();
        }

        // The whole allocation must be atomic with respect to interrupts: IRQ
        // handlers (scheduler tick, input) allocate too, and an interleaved
        // refill or bump on the same AllocContext leaves AllocPtr/AllocLimit
        // from different TLABs — overlapping objects and AllocPtr past
        // AllocLimit (seen as delta=104 in #382 debugging). RefillAllocContext
        // and Collect already disable interrupts internally; the scope nests
        // via saved flags.
        using (InternalCpu.DisableInterruptsScope())
        {
            return AllocObjectCore(size, flags);
        }
    }

    private static GCObject* AllocObjectCore(nint size, GC_ALLOC_FLAGS flags)
    {
        // Pinned objects bypass TLABs
        if ((flags & GC_ALLOC_FLAGS.GC_ALLOC_PINNED_OBJECT_HEAP) != 0)
        {
            GCObject* pinned = AllocPinnedObject(size, flags);
            if (pinned != null)
            {
                ref AllocContext pac = ref GetCurrentAllocContext();
                pac.AllocBytesUoh += Align((uint)size);
            }

            return pinned;
        }

        uint allocSize = Align((uint)size);
        if (allocSize < MinBlockSize)
        {
            allocSize = MinBlockSize;
        }

        // TLAB fast path
        ref AllocContext ac = ref GetCurrentAllocContext();
        byte* newPtr = ac.AllocPtr + allocSize;
        if (newPtr <= ac.AllocLimit)
        {
            byte* result = ac.AllocPtr;
            ac.AllocPtr = newPtr;
            ac.AllocBytes += allocSize;
            s_totalAllocatedBytes += allocSize;
            return (GCObject*)result;
        }

        // Slow path: refill TLAB and retry
        return AllocObjectSlow(ref ac, allocSize);
    }

    /// <summary>
    /// Slow path for object allocation: refills the TLAB, then retries. If all else fails, triggers GC.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static GCObject* AllocObjectSlow(ref AllocContext ac, uint allocSize)
    {
        if (RefillAllocContext(ref ac, allocSize))
        {
            byte* result = ac.AllocPtr;
            ac.AllocPtr += allocSize;
            ac.AllocBytes += allocSize;
            s_totalAllocatedBytes += allocSize;
            return (GCObject*)result;
        }

        // Last resort: collect and retry
        Collect();

        // Re-acquire ref after Collect (ReturnAllAllocContexts resets them)
        ac = ref GetCurrentAllocContext();

        if (RefillAllocContext(ref ac, allocSize))
        {
            byte* result = ac.AllocPtr;
            ac.AllocPtr += allocSize;
            ac.AllocBytes += allocSize;
            s_totalAllocatedBytes += allocSize;
            return (GCObject*)result;
        }

        return null;
    }

    /// <summary>
    /// Adds unused TLAB bytes from a dead thread to the cumulative counter.
    /// Called from <see cref="SchedulerManager.ExitThread"/> before the thread is unregistered.
    /// </summary>
    public static void AddDeadThreadNonAllocBytes(ulong unused)
    {
        s_deadThreadsNonAllocBytes += unused;
    }
}
