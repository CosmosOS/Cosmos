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
    /// Total size of the blocks on the free lists, kept as blocks go on and off them: the
    /// fragmentation metrics read it instead of walking the lists, which hold every gap a sweep
    /// leaves, and the scheduler tick reads them for the debug snapshot.
    /// </summary>
    private static ulong s_freeListBytes;

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
    /// Size requested from the segment manager for every new segment.
    /// </summary>
    private const uint MaxSegmentSize = (uint)PageAllocator.PageSize;

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
    /// Set when the mark stack could not grow during this collection. A collection runs when the
    /// heap is out of pages, so a failed growth would fail again: the rest of the mark phase works
    /// within the capacity it has.
    /// </summary>
    private static bool s_markStackFull;

    /// <summary>
    /// Lowest address of a marked object that a full mark stack left unscanned, or <c>null</c>
    /// when there is none. See <see cref="ProcessMarkOverflow"/>.
    /// </summary>
    private static byte* s_markOverflowMin;

    /// <summary>
    /// Highest address of a marked object that a full mark stack left unscanned.
    /// </summary>
    private static byte* s_markOverflowMax;

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
    /// Heap-wide metrics as they stood at the end of the last collection.
    /// This is what <see cref="GCMemoryInfo"/> reports.
    /// Stays zeroed until the first collection runs.
    /// Matches what the runtime reports before its first GC.
    /// </summary>
    private static SimpleMemoryInfo s_lastGCMemoryInfo;

    /// <summary>
    /// The heap lock: what serializes the allocation slow paths and the collection across CPUs.
    /// Around a TLAB refill, a pinned allocation, a TLAB's return, the finalizer registry and
    /// queue, and the phases of a collection, which <see cref="CpuStop.Enter"/> has already
    /// made the only code running. The fast path does not take it: a TLAB belongs to one
    /// thread, a thread runs on one CPU at a time, and an interrupt handler that allocates uses
    /// the interrupted thread's TLAB on the same CPU, which <see cref="AllocObject"/>'s
    /// interrupt mask keeps atomic. Taken after the stop ownership and before the handle
    /// store's lock and the page allocator's, through
    /// <see cref="Scheduler.SpinLock.AcquireCollectorLock"/>: a holder never acknowledges a
    /// stop and never allocates.
    /// </summary>
    private static Scheduler.SpinLock s_heapLock;

    /// <summary>
    /// TLAB refills since the collector started, counted under the heap lock. Every refill is
    /// a lock acquisition, so the rate says how the TLAB size holds up with several CPUs
    /// allocating; the collection log prints the refills since the previous collection.
    /// </summary>
    private static ulong s_refillCount;

    /// <summary>The value of <see cref="s_refillCount"/> when the last collection ran.</summary>
    private static ulong s_refillsAtLastCollection;

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
    /// What the threads that have exited had allocated, added by <see cref="RetireAllocContext"/>
    /// under the heap lock. The total allocated is summed over the live threads on read, and
    /// this is the dead ones' share of it.
    /// </summary>
    private static ulong s_deadThreadsAllocBytes;

    // --- Properties ---

    /// <summary>TLAB refills since the collector started, for the suites' measurement.</summary>
    internal static ulong RefillCount => Volatile.Read(ref s_refillCount);

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
        s_currentSegment = s_segmentManager.AllocateSegment(MaxSegmentSize);
        s_lastSegment = s_currentSegment;
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
    /// Performs a full garbage collection: mark, sweep, and segment reordering. Stops every
    /// other CPU first (<see cref="CpuStop.Enter"/>) and runs the phases under the heap lock.
    /// </summary>
    /// <returns>The number of objects freed during this collection.</returns>
    public static int Collect()
    {
        return CollectUnlessCollectedSince(-1);
    }

    /// <summary>
    /// <see cref="Collect"/>, unless another CPU has collected since the caller read
    /// <paramref name="collectionsSeen"/> from <see cref="s_totalCollections"/>, in which case
    /// nothing is collected and 0 is returned: the allocation slow path that lost the stop to
    /// another CPU's collection retries its refill on what that collection freed rather than
    /// repeat it. A negative value always collects.
    /// </summary>
    /// <param name="collectionsSeen">The collection count the caller saw before deciding to collect, or -1.</param>
    /// <returns>The number of objects freed during this collection.</returns>
    private static int CollectUnlessCollectedSince(int collectionsSeen)
    {
        if (!s_initialized)
        {
            return 0;
        }

        // TODO: Add compilation option to disable metric collection
        int freedCount = 0;
        using (InternalCpu.DisableInterruptsScope())
        {
            // The stop before the heap lock: a CPU waiting for the stop holds nothing another
            // CPU's collection needs, and the policy swap, which allocates under the stop it
            // owns, finds the stop its own on the nested entry.
            CpuStop.Enter(SchedulerManager.CpuCount);

            if (collectionsSeen >= 0 && s_totalCollections != collectionsSeen)
            {
                CpuStop.Exit();
                return 0;
            }

            using (s_heapLock.AcquireCollectorLock())
            {
                freedCount = CollectLocked();
            }

            CpuStop.Exit();
        }

        // Outside the masked section; left to the next allocation when the caller masked
        // interrupts itself (the allocator does)
        SignalPendingFinalizers();

        return freedCount;
    }

    /// <summary>The phases of a collection, the world stopped and the heap lock held.</summary>
    private static int CollectLocked()
    {
        int freedCount;

        // Record GC start timestamp
        long gcStart = Stopwatch.GetTimestamp();

        Serial.WriteString("[GC] Collection #");
        Serial.WriteNumber((uint)s_totalCollections + 1);
        Serial.WriteString(", refills since the last: ");
        Serial.WriteNumber(s_refillCount - s_refillsAtLastCollection);
        Serial.WriteString("\n");
        s_refillsAtLastCollection = s_refillCount;

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

        s_freeListBytes = 0;

        // Mark reachable objects
        MarkPhase();

        // Clear the short weak GC handles whose targets died: they do not follow an object
        // through its finalization
        s_gCHandleManager.ClearShortWeakHandles();

        // Queue the dead objects that have a finalizer and keep them alive until it has run,
        // with what the dependent handles hang on them
        MarkFinalizableObjects();
        ScanDependentHandles();

        // Clear the resurrection-tracking weak and the dependent GC handles whose targets died
        s_gCHandleManager.ClearLongWeakHandles();

        // Sweep and rebuild free lists
        freedCount = SweepPhase();

        // Reorder segments and free empty ones
        ReorderSegmentsAndFreeEmpty();
        ReorderPinnedSegmentsAndFreeEmpty();

        // Record post-GC metrics
        s_lastGen0SizeAfter = GetGenerationSize(0);
        s_lastGen0FragmentationAfter = GetCurrentFragmentation(0);

        s_totalCollections++;
        s_totalObjectsFreed += freedCount;

        // Freeze the heap-wide metrics reported by GCMemoryInfo. Recorded after the
        // counters above so the snapshot carries the index of this very collection.
        RecordLastGCMemoryInfo();

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

        return freedCount;
    }
    // --- Internal methods ---

    /// <summary>
    /// Allocates a managed object and stores its header. Called by the runtime allocation helpers.
    /// Uses per-thread TLAB fast path for non-pinned allocations.
    /// </summary>
    /// <param name="size">Requested object size in bytes.</param>
    /// <param name="pMT">MethodTable stored in the object's header.</param>
    /// <param name="length">Component count stored for an array or a string; ignored for any other type.</param>
    /// <param name="flags">Runtime allocation flags (e.g., pinned object heap).</param>
    /// <returns>Pointer to the allocated object, or <c>null</c> if allocation fails.</returns>
    internal static GCObject* AllocObject(nint size, MethodTable* pMT, int length, GC_ALLOC_FLAGS flags)
    {
        if (!s_initialized)
        {
            Initialize();
        }

        // Finalizers a collection left behind because it ran with interrupts masked. Before the
        // allocation, not after: nothing would keep the new object alive while they run. With
        // the finalizer thread on, the allocation only signals it: it never waits for a pass.
        if (s_finalizerQueueHead != s_finalizerQueue.Count)
        {
            SignalPendingFinalizers();
        }

        // The whole allocation must be atomic with respect to interrupts: IRQ
        // handlers (scheduler tick, input) allocate too, and an interleaved
        // refill or bump on the same AllocContext leaves AllocPtr/AllocLimit
        // from different TLABs — overlapping objects and AllocPtr past
        // AllocLimit (seen as delta=104 in #382 debugging). The mask is the
        // whole exclusion the fast path needs: a TLAB belongs to one thread and
        // a thread runs on one CPU at a time. The slow paths take the heap lock
        // on top, against the other CPUs; Collect stops them. The scope nests
        // via saved flags.
        // The header is stored inside the scope too: until its MethodTable is
        // set the object is a zeroed block, which a collection run by another
        // thread (after a tick preempts this one) neither marks nor walks as an
        // object, so the sweep folds it into a free run and a later allocation
        // overlaps it. An array's Length must be in place as well, or the heap
        // walk sizes it as an empty array.
        using (InternalCpu.DisableInterruptsScope())
        {
            GCObject* result = AllocObjectCore(size, flags);
            if (result != null)
            {
                result->MethodTable = pMT;
                if (pMT->HasComponentSize)
                {
                    result->Length = length;
                }

                RegisterNewObject(result, flags);
            }

            return result;
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
            return (GCObject*)result;
        }

        // Slow path: refill TLAB and retry
        return AllocObjectSlow(ref ac, allocSize);
    }

    /// <summary>
    /// Slow path for object allocation: refills the TLAB, then retries. If all else fails,
    /// collects and retries, unless another CPU collected while this one waited for the stop,
    /// in which case the refill is retried on what that collection freed.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static GCObject* AllocObjectSlow(ref AllocContext ac, uint allocSize)
    {
        int collectionsSeen = s_totalCollections;
        if (RefillAllocContext(ref ac, allocSize))
        {
            byte* result = ac.AllocPtr;
            ac.AllocPtr += allocSize;
            ac.AllocBytes += allocSize;
            return (GCObject*)result;
        }

        // Last resort: collect and retry. The heap lock is not held here: the refill released
        // it, so the stop is claimed with nothing another CPU's collection could wait for.
        CollectUnlessCollectedSince(collectionsSeen);

        // Re-acquire ref after Collect (ReturnAllAllocContexts resets them)
        ac = ref GetCurrentAllocContext();

        if (RefillAllocContext(ref ac, allocSize))
        {
            byte* result = ac.AllocPtr;
            ac.AllocPtr += allocSize;
            ac.AllocBytes += allocSize;
            return (GCObject*)result;
        }

        return null;
    }
}
