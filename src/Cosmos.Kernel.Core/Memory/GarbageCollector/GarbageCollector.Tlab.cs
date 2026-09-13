// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.Core.CPU;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Scheduler;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

/// <summary>
/// Thread-Local Allocation Buffer (TLAB) management: refill, return, and per-thread context access.
/// </summary>
internal static unsafe partial class GarbageCollector
{
    /// <summary>
    /// Default TLAB size in bytes (8KB).
    /// </summary>
    private const uint TlabSize = 8192;

    /// <summary>
    /// Returns a reference to the current thread's allocation context.
    /// Uses the scheduler's current thread when enabled, otherwise falls back to the static context.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ref AllocContext GetCurrentAllocContext()
    {
        // SchedulerManager.IsRunning is false during early boot (before scheduler init),
        // so we safely fall back to the static context. CosmosFeatures.SchedulerEnabled
        // alone is a compile-time flag that doesn't guarantee _cpuStates is allocated.
        if (CosmosFeatures.SchedulerEnabled && SchedulerManager.IsRunning)
        {
            PerCpuState? cpuState = SchedulerManager.CurrentCpuState;
            if (cpuState?.CurrentThread != null)
            {
                return ref cpuState.CurrentThread._allocContext;
            }
        }

        return ref s_fallbackAllocContext;
    }

    /// <summary>
    /// Refills a thread's TLAB from the GC heap (free list, then segment bump, then new segment).
    /// Returns the unused gap from the old TLAB to the free list before acquiring a new buffer.
    /// </summary>
    /// <param name="ac">The allocation context to refill.</param>
    /// <param name="size">Minimum allocation size that must fit in the new TLAB.</param>
    /// <returns><c>true</c> if the TLAB was successfully refilled; otherwise, <c>false</c>.</returns>
    internal static bool RefillAllocContext(ref AllocContext ac, uint size)
    {
        using (InternalCpu.DisableInterruptsScope())
        {
            // Return unused portion of old TLAB
            StampUnusedTlab(ref ac);

            // Determine TLAB request size: at least the requested size, ideally TlabSize
            uint requestSize = size > TlabSize ? size : TlabSize;

            // Try free list first (raw variant — already zeroes memory)
            void* buffer = AllocFromFreeListRaw(requestSize);
            if (buffer != null)
            {
                return SetupTlab(ref ac, buffer, requestSize);
            }

            // Try bump allocation from segments (raw variant — must zero)
            buffer = BumpAllocInSegmentRaw(s_lastSegment, requestSize);
            if (buffer != null)
            {
                MemoryOp.MemSet((byte*)buffer, 0, (int)requestSize);
                return SetupTlab(ref ac, buffer, requestSize);
            }

            // Fall back to the largest free-list block before growing the heap.
            // Post-sweep blocks are always smaller than TlabSize (gap stamps and
            // surviving objects cap them below 8192), so requiring a full-size
            // TLAB would leave every partially live segment's free space
            // unusable and burn a fresh segment per refill: one long-lived
            // 26-byte string then pins 3 pages forever.
            uint minSize = size > MinBlockSize ? size : MinBlockSize;
            buffer = AllocLargestFromFreeListRaw(minSize, out uint blockSize);
            if (buffer != null)
            {
                return SetupTlab(ref ac, buffer, blockSize);
            }

            // Try slow path: walk segments, allocate new segment if needed
            buffer = AllocateObjectSlowRaw(requestSize);
            if (buffer != null)
            {
                MemoryOp.MemSet((byte*)buffer, 0, (int)requestSize);
                return SetupTlab(ref ac, buffer, requestSize);
            }

            // If full TlabSize failed but we only need `size`, try exact size
            if (requestSize > size)
            {
                buffer = AllocFromFreeListRaw(size);
                if (buffer != null)
                {
                    return SetupTlab(ref ac, buffer, size);
                }

                buffer = AllocateObjectSlowRaw(size);
                if (buffer != null)
                {
                    MemoryOp.MemSet((byte*)buffer, 0, (int)size);
                    return SetupTlab(ref ac, buffer, size);
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Sets up a TLAB from an allocated buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool SetupTlab(ref AllocContext ac, void* buffer, uint bufferSize)
    {
        ac.AllocPtr = (byte*)buffer;
        ac.AllocLimit = (byte*)buffer + bufferSize;
        return true;
    }

    /// <summary>
    /// Returns a thread's TLAB to the GC heap. Stamps the unused gap as a FreeBlock
    /// and resets the allocation pointers.
    /// </summary>
    /// <param name="ac">The allocation context to return.</param>
    public static void ReturnAllocContext(ref AllocContext ac)
    {
        StampUnusedTlab(ref ac);
    }

    /// <summary>
    /// Returns all live threads' TLABs to the GC heap. Called at the start of GC collection.
    /// After this call, all threads have AllocPtr == AllocLimit == null and will refill on next alloc.
    /// </summary>
    internal static void ReturnAllAllocContexts()
    {
        if (CosmosFeatures.SchedulerEnabled)
        {
            SchedulerThread?[]? threads = SchedulerManager.Threads;
            if (threads != null)
            {
                int count = SchedulerManager.ThreadCount;
                for (int i = 0; i < threads.Length && count > 0; i++)
                {
                    SchedulerThread? thread = threads[i];
                    if (thread != null)
                    {
                        ReturnAllocContext(ref thread._allocContext);
                        count--;
                    }
                }
            }
        }

        ReturnAllocContext(ref s_fallbackAllocContext);
    }

    /// <summary>
    /// Stamps the unused portion of a TLAB [AllocPtr, AllocLimit) as a FreeBlock
    /// and adds it to the free list so it can be reused.
    /// </summary>
    private static void StampUnusedTlab(ref AllocContext ac)
    {
        if (ac.AllocPtr == null || ac.AllocLimit == null)
        {
            return;
        }

        // Canary for the alloc-path atomicity invariant (#382): an IRQ-interleaved
        // allocation used to leave AllocPtr past AllocLimit, which underflowed the
        // gap below into a ~4GB stamp. AllocObject now runs with interrupts
        // disabled, so this should never fire — if it does, drop the context
        // instead of stamping garbage and report it loudly.
        if (ac.AllocPtr > ac.AllocLimit)
        {
            Serial.WriteString("[TLAB] BUG Ptr>Limit ptr=");
            Serial.WriteHex((ulong)ac.AllocPtr);
            Serial.WriteString(" limit=");
            Serial.WriteHex((ulong)ac.AllocLimit);
            Serial.WriteString("\n");
            ac.AllocPtr = null;
            ac.AllocLimit = null;
            return;
        }

        uint gap = (uint)(ac.AllocLimit - ac.AllocPtr);
        if (gap >= MinBlockSize + ReservedHeaderSlotSize)
        {
            // Exclude the trailing header slot: for a TLAB carved from segment bump
            // space, the next bump allocation starts exactly at AllocLimit and its
            // runtime header (objRef-4) lands in the gap's last 4 bytes — a free
            // block must never own them (recycling would zero the stored value).
            gap -= ReservedHeaderSlotSize;
            SanitizeReservedHeaderSlot(ac.AllocPtr + gap);

            FreeBlock* freeBlock = (FreeBlock*)ac.AllocPtr;
            freeBlock->MethodTable = s_freeMethodTable;
            freeBlock->Size = (int)gap;
            freeBlock->Next = null;
            AddToFreeList(freeBlock, 't');
        }
        else if (gap > 0)
        {
            // Gap too small for a FreeBlock — zero it so sweep doesn't see
            // stale MethodTable pointers and break early.
            MemoryOp.MemSet(ac.AllocPtr, 0, (int)gap);
        }

        // The gap now belongs to the free list (or is dead space) — the context
        // MUST forget it. RefillAllocContext stamps before trying to acquire a
        // new buffer; if every attempt fails it returns with the context intact,
        // and the subsequent Collect() → ReturnAllAllocContexts() would stamp
        // the SAME gap again. That second head-insert makes the free block point
        // at itself (block->Next = head = block), and the next free-list walk —
        // GetCurrentFragmentation at the top of Collect() — spins forever with
        // interrupts disabled: the "hang at [GC] Collection #1" bug.
        ac.AllocPtr = null;
        ac.AllocLimit = null;
    }
}
