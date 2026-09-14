using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector;
using Internal.Runtime;
using static Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;

namespace Cosmos.Kernel.Core.Runtime;

internal static unsafe class GC
{
    // Copy past of GCMemoryInfoData & GCGenerationInfo
    // https://github.com/dotnet/runtime/blob/51cfb0e382532e43500216c755d8bd8e1a8f371d/src/libraries/System.Private.CoreLib/src/System/GCMemoryInfo.cs#L16C28-L16C44

    // The original struct do not set a layout, for safety i put one. Is it a bad idea ?
    [StructLayout(LayoutKind.Sequential)]
    public struct GCGenerationInfo
    {
        public long sizeBeforeBytes;
        public long fragmentationBeforeBytes;
        public long sizeAfterBytes;
        public long fragmentationAfterBytes;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct GCMemoryInfoDataStruct
    {
        internal long _highMemoryLoadThresholdBytes;
        internal long _totalAvailableMemoryBytes;
        internal long _memoryLoadBytes;
        internal long _heapSizeBytes;
        internal long _fragmentedBytes;
        internal long _totalCommittedBytes;
        internal long _promotedBytes;
        internal long _pinnedObjectsCount;
        internal long _finalizationPendingCount;
        internal long _index;
        internal int _generation;
        internal int _pauseTimePercentage;
        internal byte _compacted;
        internal byte _concurrent;

        internal GCGenerationInfo _generationInfo0;
        internal GCGenerationInfo _generationInfo1;
        internal GCGenerationInfo _generationInfo2;
        internal GCGenerationInfo _generationInfo3;
        internal GCGenerationInfo _generationInfo4;

        internal TimeSpan _pauseDuration0;
        internal TimeSpan _pauseDuration1;
    }


    [RuntimeExport("RhGetMemoryInfo")]
    internal unsafe static void RhGetMemoryInfo(ref byte rawData, GCKind kind)
    {
        fixed (byte* pRawData = &rawData)
        {
            var data = (GCMemoryInfoDataStruct*)pRawData;

            // I did the mistake once, the doc clearly specify that the memory info should not change bettewin GC.
            // It's not writen in the public API, it's writen in System.Private.CoreLib
            GarbageCollector.SimpleMemoryInfo info = GarbageCollector.GetLastGCMemoryInfo();

            // the ration 0.5 is arbitrary, should be change in the future
            data->_highMemoryLoadThresholdBytes = (long)(PageAllocator.RamSize / 2);
            data->_totalAvailableMemoryBytes = (long)PageAllocator.RamSize;
            data->_memoryLoadBytes = (long)info.MemoryLoadBytes;
            data->_heapSizeBytes = (long)info.HeapSizeBytes;
            data->_fragmentedBytes = (long)info.FragmentedBytes;
            data->_totalCommittedBytes = (long)info.TotalCommittedBytes;
            data->_promotedBytes = (long)info.PromotedBytes;
            data->_pinnedObjectsCount = (long)info.PinnedObjectsCount;
            data->_index = info.CollectionIndex;
            data->_generation = info.CondemnedGeneration;

            // not tracked currently
            data->_finalizationPendingCount = 0;
            data->_pauseTimePercentage = 0;
            data->_compacted = 0;
            data->_concurrent = 0;

            // Populate generation info. GC is non-generational; report generation 0
            // Use last recorded before/after metrics from the GC (recorded at Collect())
            data->_generationInfo0.sizeBeforeBytes = (long)GarbageCollector.GetLastGenSizeBefore(0);
            data->_generationInfo0.fragmentationBeforeBytes = (long)GarbageCollector.GetLastGenFragmentationBefore(0);
            data->_generationInfo0.sizeAfterBytes = (long)GarbageCollector.GetLastGenSizeAfter(0);
            data->_generationInfo0.fragmentationAfterBytes = (long)GarbageCollector.GetLastGenFragmentationAfter(0);

            // Other generation infos remain zero
            data->_generationInfo1 = default;
            data->_generationInfo2 = default;
            data->_generationInfo3 = default;
            data->_generationInfo4 = default;

            data->_pauseDuration0 = TimeSpan.Zero;
            data->_pauseDuration1 = TimeSpan.Zero;
        }
    }

    [RuntimeExport("RhGetGcTotalMemory")]
    internal static long RhGetGcTotalMemory()
    {
        ulong heapBytes = GarbageCollector.GetHeapSizeBytes();
        if (heapBytes > long.MaxValue)
        {
            return long.MaxValue;
        }

        return (long)heapBytes;
    }

    [RuntimeExport("RhCollect")]
    internal static void RhCollect(int generation, InternalGCCollectionMode mode)
    {
        // Do not support generation for now nor modes
        GarbageCollector.Collect();
    }

    [RuntimeExport("RhGetGeneration")]
    internal static int RhGetGeneration(object obj)
    {
        nint addr = Unsafe.As<object, nint>(ref obj);
        return GarbageCollector.GetObjectGeneration(addr);
    }

    [RuntimeExport("RhGetGenerationSize")]
    internal static int RhGetGenerationSize(int gen)
    {
        // Our GC is currently non-generational. Delegate to the GC helper which
        // returns the total used bytes for generation 0 and 0 for others.
        ulong size = GarbageCollector.GetGenerationSize(gen);
        if (size > int.MaxValue)
        {
            return int.MaxValue;
        }

        return (int)size;
    }

    [RuntimeExport("RhGetLastGCPercentTimeInGC")]
    internal static int RhGetLastGCPercentTimeInGC()
    {
        return GarbageCollector.GetLastGCPercentTimeInGC();
    }

    // In the runtime the handle is a pointer-to-a-pointer to an object which can be cast to the object in different ways.
    // Use HandleGetPrimary to avoid having to handle whether the USE_CHECKED_OBJECTREFS macros are defined or not.
    [RuntimeExport("RhHandleGet")]
    internal static object? RhHandleGet(IntPtr handle)
    {
        GCObject* primary = GarbageCollector.HandleGetPrimary(handle);
        if (primary == null)
        {
            return null;
        }

        nint asInt = (nint)primary;
        return Unsafe.As<nint, object>(ref asInt);
    }

    [RuntimeExport("RhGetGcCollectionCount")]
    internal static int RhGetGcCollectionCount(int generation, bool getSpecialGCCount)
    {
        if (generation != 0)
        {
            return 0;
        }

        return GarbageCollector.GetCollectionIndex();
    }

    [RuntimeExport("RhIsPromoted")]
    internal static bool RhIsPromoted(object obj)
    {
        // Only gen 0 exist can't be promoted
        return false;
    }

    [RuntimeExport("RhIsServerGc")]
    internal static bool RhIsServerGc()
    {
        return false;
    }

    [RuntimeExport("RhGetGCSegmentSize")]
    internal static ulong RhGetGCSegmentSize()
    {
        return GarbageCollector.GetGCSegmentSizeBytes();
    }

    [RuntimeExport("RhGetAllocatedBytesForCurrentThread")]
    internal static long RhGetAllocatedBytesForCurrentThread()
    {
        // Cumulative bytes allocated by this thread. Does not subtract unused TLAB space
        // (that would cause the value to decrease on TLAB refill).
        ref AllocContext ac = ref GarbageCollector.GetCurrentAllocContext();
        return (long)(ac.AllocBytes + ac.AllocBytesUoh);
    }

    [RuntimeExport("RhGetTotalAllocatedBytes")]
    internal static long RhGetTotalAllocatedBytes()
    {
        ulong allocated = GarbageCollector.GetTotalAllocatedBytes();
        if (allocated > long.MaxValue)
        {
            return long.MaxValue;
        }

        return (long)allocated;
    }

    [RuntimeExport("RhGetTotalAllocatedBytesPrecise")]
    internal static long RhGetTotalAllocatedBytesPrecise()
    {
        return (long)GarbageCollector.GetTotalAllocatedBytesPrecise();
    }

    [RuntimeExport("RhRegisterForGCReporting")]
    internal static unsafe void RhRegisterForGCReporting(void* pRegistration)
    {

    }

    [RuntimeExport("RhUnregisterForGCReporting")]
    internal static unsafe void RhUnregisterForGCReporting(void* pRegistration)
    {

    }

    [RuntimeExport("RhGetGCDescSize")]
    internal static unsafe int RhGetGCDescSize(MethodTable* pMT)
    {
        if (!pMT->ContainsGCPointers)
        {
            return 0;
        }

        int numSeries = (int)((nint*)pMT)[-1];

        if (numSeries > 0)
        {
            // [GCDescSeriesN, ..., GCDescSeries1, nint numSeries]
            return IntPtr.Size + numSeries * sizeof(GCDescSeries);
        }
        else
        {
            // [ValSerieItemN, ..., ValSerieItem1, nint offset, nint numSeries]
            return IntPtr.Size * 2 + (-numSeries * sizeof(ValSerieItem));
        }
    }
}
