using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using KernelHeap = Cosmos.Kernel.Core.Memory.Heap.Heap;

namespace Cosmos.Kernel.System.Diagnostics;

/// <summary>
/// Diagnostic view of the kernel's physical-memory and garbage-collector
/// statistics, plus a forced collection. Every figure is a property, and
/// each is a plain field read with no allocation, so they are safe to poll
/// from a monitor loop at display refresh rate. The two collector counters
/// are read independently rather than as a pair, so a collection landing
/// between two reads shows up in one of them and not the other.
/// Managed-heap figures beyond these counters come from the BCL
/// (<see cref="global::System.GC.GetGCMemoryInfo()"/>).
/// </summary>
public static class MemoryInfo
{
    /// <summary>
    /// Size of a physical page in bytes.
    /// </summary>
    public static ulong PageSizeBytes => PageAllocator.PageSize;

    /// <summary>
    /// Total number of physical pages managed by the page allocator.
    /// </summary>
    public static ulong TotalPages => PageAllocator.TotalPageCount;

    /// <summary>
    /// Number of physical pages currently free.
    /// </summary>
    public static ulong FreePages => PageAllocator.FreePageCount;

    /// <summary>
    /// Total usable RAM in bytes, as reported by the bootloader memory map.
    /// </summary>
    public static ulong RamSizeBytes => PageAllocator.RamSize;

    /// <summary>
    /// Percentage of time spent in the garbage collector during the most
    /// recent collection window, from 0 to 100.
    /// </summary>
    public static int GcTimePercent => GarbageCollector.GetLastGCPercentTimeInGC();

    /// <summary>
    /// Number of collections the kernel collector has run since boot.
    /// </summary>
    public static int TotalCollections => GarbageCollector.GetCollectionIndex();

    /// <summary>
    /// Number of objects the sweep phase has freed since boot, summed over
    /// every collection.
    /// </summary>
    public static int TotalObjectsFreed => GarbageCollector.GetTotalObjectsFreed();

    /// <summary>
    /// Forces an immediate garbage collection and returns the number of
    /// objects freed. Unlike <see cref="global::System.GC.Collect()"/>,
    /// the count of freed objects is reported back to the caller.
    /// </summary>
    /// <returns>Number of objects freed by the collection.</returns>
    public static int Collect() => KernelHeap.Collect();
}
