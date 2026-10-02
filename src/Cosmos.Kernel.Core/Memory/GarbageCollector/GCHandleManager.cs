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
        var newSegment = AllocateSegment();

        if (newSegment == null)
        {
            return null;
        }

        _tail->Next = newSegment;
        _tail = newSegment;

        return newSegment->TryAllocate();
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
            GCHandle* handle = handleType == (GCHandleType)6
                        ? DependentHandleStore.AllocateHandle()
                        : _gcHandleControllers[(int)handleType].AllocateHandle();

            handle->Object = obj;
            handle->Type = handleType;
            handle->ExtraInfo = extraInfo;
            return handle;
        }

        return null;
    }

    /// <summary>
    /// Clears the weak handles whose target objects were not marked during the mark phase, and
    /// the dependent handles whose primary was not. Called between mark and sweep to allow weak
    /// references to be collected.
    /// </summary>
    public readonly void ClearWeakHandles()
    {
        // Both weak kinds: with no finalization, tracking resurrection changes nothing.
        ClearDeadTargets(_gcHandleControllers[(int)GCHandleType.Weak], false);
        ClearDeadTargets(_gcHandleControllers[(int)GCHandleType.WeakTrackResurrection], false);
        ClearDeadTargets(DependentHandleStore, true);

        // The handle stays allocated: its owner still holds it, reads null from it, and frees it
        // or sets a new target (RuntimeType does so for its RuntimeTypeInfo cache). Freeing it
        // here put it back on the free list while still in use: a later handle shared its slot,
        // and a target set in it was no longer cleared, so it dangled once collected.
        static void ClearDeadTargets(GCHandleSegmentStore store, bool dependent)
        {
            var handleEnum = store.GetEnumerator();
            while (handleEnum.MoveNext())
            {
                GCHandle* handle = handleEnum.Current;
                if (handle->Object != null && !handle->Object->IsMarked)
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

    /// <summary>
    /// Frees a previously allocated GC handle, releasing the slot for reuse.
    /// </summary>
    /// <param name="handle">The handle to free. No-op if <see cref="IntPtr.Zero"/>.</param>
    internal void FreeHandle(GCHandle* handle)
    {
        if (handle != null && IsValidHandleType(handle->Type))
        {

            var store = handle->Type == (GCHandleType)6
                        ? DependentHandleStore
                        : _gcHandleControllers[(int)handle->Type];
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
