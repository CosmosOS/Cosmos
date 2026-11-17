// This code is licensed under the BSD 3-Clause license (see LICENSE for details)


using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.IO;

namespace Cosmos.Kernel.Core.Memory.GarbageCollector;

internal unsafe struct GCHandleSegmentStore
{
    private GCHandleSegment* _head;
    private GCHandleSegment* _tail;

    /// <summary>
    /// The lock of the segment chain: taken IRQ-safe to append a segment, and
    /// by the collector to walk the chain clearing dead targets. Slot
    /// allocation and free within a segment stay lock-free (a compare-exchange
    /// on the segment's packed word), and the chain only grows, so reading it
    /// to find a segment with room needs no lock. Nothing under it allocates a
    /// managed object: a new segment is a page from the page allocator.
    /// </summary>
    private Scheduler.SpinLock _lock;

    public ulong CommitedSize;
    public readonly ulong Count
    {
        get
        {
            ulong result = 0;

            for (var segment = _head; segment != null; segment = segment->Next)
            {
                result += (ulong)segment->AliveCount;
            }

            return result;
        }
    }

    /// <summary>Segments in the chain; each is one page of <see cref="CommitedSize"/>.</summary>
    public readonly int SegmentCount
    {
        get
        {
            int result = 0;

            for (var segment = _head; segment != null; segment = segment->Next)
            {
                result++;
            }

            return result;
        }
    }

    public void Initialize()
    {
        var segment = AllocateSegment();

        _head = segment;
        _tail = segment;
    }

    public GCHandle* AllocateHandle()
    {
        // Fast Path: Last Allocated Segment (tail).
        GCHandle* result = _tail->TryAllocate();

        if (result != null)
        {
            return result;
        }

        // Slow Path: Iterate over all GCHandle Segments.
        for (var segment = _head; segment != null; segment = segment->Next)
        {
            result = segment->TryAllocate();
            if (result != null)
            {
                return result;
            }
        }

        // Fallback: Allocate a new GCHandleSegment, use it to allocate the GCHandle.
        // Under the chain lock: two threads here at once each allocated a segment
        // and linked it onto the same tail, and the second link orphaned the first
        // segment, whose handles the collector then neither scanned nor cleared.
        // The loser of the race finds room in the segment the winner appended.
        using (_lock.AcquireIrqSafe())
        {
            result = _tail->TryAllocate();
            if (result != null)
            {
                return result;
            }

            var newSegment = AllocateSegment();

            if (newSegment == null)
            {
                return null;
            }

            // The segment is initialized before it is published: the barrier
            // orders its initialization before the link, so a reader walking
            // the chain without the lock sees it whole or not at all.
            Interlocked.MemoryBarrier();
            _tail->Next = newSegment;
            _tail = newSegment;

            return newSegment->TryAllocate();
        }
    }

    public void FreeHandle(GCHandle* handle)
    {
        for (GCHandleSegment* segment = _head; segment != null; segment = segment->Next)
        {
            if (segment->ContainsHandle(handle))
            {
                segment->Free(handle);
                return;
            }
        }
    }

    /// <summary>
    /// Clears the targets of this store that the mark phase did not reach, under
    /// the chain lock. Called from the collection with interrupts masked, so the
    /// lock only ever waits for another CPU's append to finish.
    /// </summary>
    /// <param name="dependent">Whether the store holds dependent handles, whose secondary dies with the primary.</param>
    public void ClearDeadTargets(bool dependent)
    {
        using (_lock.AcquireIrqSafe())
        {
            // The handle stays allocated: its owner still holds it, reads null from it, and frees it
            // or sets a new target (RuntimeType does so for its RuntimeTypeInfo cache). Freeing it
            // here put it back on the free list while still in use: a later handle shared its slot,
            // and a target set in it was no longer cleared, so it dangled once collected.
            Enumerator handleEnum = GetEnumerator();
            while (handleEnum.MoveNext())
            {
                GCHandle* handle = handleEnum.Current;

                // A frozen target is never marked, and never dies.
                if (handle->Object != null && !GarbageCollector.IsLive(handle->Object))
                {
                    handle->Object = null;
                    if (dependent)
                    {
                        handle->ExtraInfo = 0; // the secondary was alive only through the primary
                    }
                }
            }
        }
    }

    private GCHandleSegment* AllocateSegment()
    {
        void* segment = PageAllocator.AllocPages(PageType.Unmanaged, 1, true);

        if (segment == null)
        {
            return null;
        }

        CommitedSize += PageAllocator.PageSize;

        var gcHandlesegment = (GCHandleSegment*)segment;
        gcHandlesegment->Initialize();

        return gcHandlesegment;
    }

    public Enumerator GetEnumerator() => new(_head);

    public ref struct Enumerator(GCHandleSegment* segment)
    {
        private GCHandleSegment* _head = segment;
        public GCHandle* Current { get; private set; }
        private int _count = 0;

        public bool MoveNext()
        {

            while (_head != null)
            {
                // Skip Segment if we know there isn't any GCHandle
                if (_head->AliveCount > 0)
                {
                    GCHandle* ptr = (GCHandle*)(_head + 1) + _count;
                    for (int i = _count; i < GCHandleSegment.Capacity; i++)
                    {
                        if (ptr->Type != GCHandle.FreeHandleType)
                        {
                            // Save Next Index: past this handle, not past the free slots before
                            // it only, or the next call returns it again.
                            _count = i + 1;
                            Current = ptr;
                            return true;
                        }

                        ptr += 1;
                    }
                }
                _count = 0;
                _head = _head->Next;
            }
            Current = null;
            return false;
        }
    }
}

/// <summary>
/// Array of <see cref="GCHandleSegment"/> Controllers, the numeber of members in the array must be keep in sync with the number of values on <see cref="GCHandleType"/>
/// </summary>
[InlineArray(Size)]
internal struct GCHandleStoreList
{
    internal const int Size = 4;
    private GCHandleSegmentStore _element0;
}

internal unsafe struct GCHandleManager()
{
    private GCHandleStoreList _gcHandleControllers = new();
    public GCHandleStoreList GCHandleControllers => _gcHandleControllers;
    public GCHandleSegmentStore DependentHandleStore = new();

    /// <summary>
    /// Initializes the GC handle store by allocating a dedicated segment.
    /// </summary>
    public void InitializeGCHandleStore()
    {
        for (int i = 0; i < GCHandleStoreList.Size; i++)
        {
            _gcHandleControllers[i].Initialize();
        }

        DependentHandleStore.Initialize();
    }

    public readonly ulong GetCommitedSize()
    {
        ulong size = 0;

        for (int i = 0; i < GCHandleStoreList.Size; i++)
        {
            size += _gcHandleControllers[i].CommitedSize;
        }

        size += DependentHandleStore.CommitedSize;

        return size;
    }

    /// <summary>
    /// Allocates a new GC handle for the specified object.
    /// </summary>
    /// <param name="obj">Pointer to the managed object to track.</param>
    /// <param name="handleType">The type of handle to allocate.</param>
    /// <param name="extraInfo">Additional info associated with the handle.</param>
    /// <returns>An opaque handle value, or <see cref="IntPtr.Zero"/> if the store is full.</returns>
    internal GCHandle* AllocateHandler(GCObject* obj, GCHandleType handleType, nint extraInfo)
    {
        if (IsValidHandleType(handleType))
        {
            // By reference: the store carries the chain lock, which a copy would not share.
            ref GCHandleSegmentStore store = ref handleType == (GCHandleType)6
                        ? ref DependentHandleStore
                        : ref _gcHandleControllers[(int)handleType];
            GCHandle* handle = store.AllocateHandle();

            handle->Object = obj;
            handle->Type = handleType;
            handle->ExtraInfo = extraInfo;
            return handle;
        }

        return null;
    }

    /// <summary>
    /// Clears the short weak handles whose target objects were not marked during the mark phase.
    /// Called before the dead objects that have a finalizer are marked again: a short weak
    /// handle does not follow its target through finalization, even when the finalizer revives it.
    /// </summary>
    public void ClearShortWeakHandles()
    {
        _gcHandleControllers[(int)GCHandleType.Weak].ClearDeadTargets(false);
    }

    /// <summary>
    /// Clears the resurrection-tracking weak handles whose target objects are not marked, and
    /// the dependent handles whose primary is not. Called between the marking of the objects
    /// queued for finalization and the sweep: what is cleared here is about to be freed.
    /// </summary>
    public void ClearLongWeakHandles()
    {
        _gcHandleControllers[(int)GCHandleType.WeakTrackResurrection].ClearDeadTargets(false);
        DependentHandleStore.ClearDeadTargets(true);
    }

    /// <summary>
    /// Frees a previously allocated GC handle, releasing the slot for reuse.
    /// </summary>
    /// <param name="handle">The handle to free. No-op if <see cref="IntPtr.Zero"/>.</param>
    internal void FreeHandle(GCHandle* handle)
    {
        if (handle != null && IsValidHandleType(handle->Type))
        {
            // By reference: the store carries the chain lock, which a copy would not share.
            ref GCHandleSegmentStore store = ref handle->Type == (GCHandleType)6
                        ? ref DependentHandleStore
                        : ref _gcHandleControllers[(int)handle->Type];
            store.FreeHandle(handle);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidHandleType(GCHandleType type)
    {
        return type is GCHandleType.Weak
                    or GCHandleType.WeakTrackResurrection
                    or GCHandleType.Normal
                    or GCHandleType.Pinned
                    or (GCHandleType)6;
    }

}
