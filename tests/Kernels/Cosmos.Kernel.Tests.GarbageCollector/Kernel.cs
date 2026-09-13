using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.Core.Memory.GarbageCollector.GcInfo;
using Cosmos.Kernel.Core.Memory.Heap;
using Cosmos.Kernel.Core.Runtime.GcInfo;
using Cosmos.Kernel.System.Timer;
using Cosmos.TestRunner.Framework;
using CoreGC = Cosmos.Kernel.Core.Memory.GarbageCollector.GarbageCollector;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

// Note: the enclosing namespace name "GarbageCollector" would shadow the core
// GarbageCollector type via parent-namespace lookup, so we alias it as CoreGC.
namespace Cosmos.Kernel.Tests.GarbageCollector;

public class Kernel : Sys.Kernel
{
    protected override void BeforeRun()
    {
        Serial.WriteString("[GarbageCollector] BeforeRun() reached!\n");
        Serial.WriteString("[GarbageCollector] Starting tests...\n");

        TR.Start("GarbageCollector Tests", expectedTests: 46);

        // Garbage Collection Tests
        TR.Run("GC_IsEnabled", TestGCIsEnabled);
        TR.Run("GC_GetStats", TestGCGetStats);
        TR.Run("GC_CollectBasic", TestGCCollectBasic);
        TR.Run("GC_StatsIncrement", TestGCStatsIncrement);
        TR.Run("GC_ExactCollectionCount", TestGCExactCollectionCount);
        TR.Run("GC_ObjectSurvival", TestGCObjectSurvival);
        TR.Run("GC_StringSurvival", TestGCStringSurvival);
        TR.Run("GC_ArraySurvival", TestGCArraySurvival);
        TR.Run("GC_ListSurvival", TestGCListSurvival);
        TR.Run("GC_UnreachableExactCount", TestGCUnreachableExactCount);
        TR.Run("GC_ObjectGraphSurvival", TestGCObjectGraphSurvival);
        TR.Run("GC_MixedTypeSurvival", TestGCMixedTypeSurvival);
        TR.Run("GC_AllocAfterCollect", TestGCAllocAfterCollect);
        TR.Run("GC_WeakReference", TestGCWeakReference);
        TR.Run("GC_LargeAllocCollect", TestGCLargeAllocationAndCollect);
        TR.Run("GC_StructArraySurvival", TestGCStructArraySurvival);
        TR.Run("GC_DictSurvival", TestGCDictionarySurvival);
        TR.Run("GC_PageAccounting", TestGCPageAccounting);
        TR.Run("GC_DependentHandle", TestGCDependentHandle);
        TR.Run("GC_DependentHandleCleanup", TestGCDependentHandleCleanup);
        TR.Run("GC_DependentHandleNullValue", TestGCDependentHandleNullValue);
        TR.Run("GC_HandleStoreIntegrity", TestGCHandleStoreIntegrity);
        TR.Run("GC_PinnedHeapReuse", TestGCPinnedHeapReuse);
        TR.Run("GC_StackScanPaddingStress", TestGCStackScanPaddingStress);
        TR.Run("GC_GcInfoDecoder", TestGCGcInfoDecoder);
        TR.Run("GC_PreciseStackScan", TestGCPreciseStackScan);
        TR.Run("GC_FuncletNoFalseRoot", TestGCFuncletNoFalseRoot);
        TR.Run("GC_FuncletNoCrashOnAllocInCatch", TestGCFuncletNoCrashOnAllocInCatch);
        TR.Run("GC_ThrowThroughDeepChain", TestGCThrowThroughDeepChain);

        // GC Soundness Tests. GC_InteriorPointerRoot FAILS until interior-pointer
        // support (#376) lands — it is the acceptance test for #384.
        TR.Run("GC_InteriorPointerRoot", TestGCInteriorPointerRoot);
        TR.Run("GC_StaticOnlyReachability", TestGCStaticOnlyReachability);
        TR.Run("GC_MultithreadChurnUnderCollect", TestGCMultithreadChurnUnderCollect);
        TR.Run("GC_MallocHeapNotSwept", TestGCMallocHeapNotSwept);

        // GC Info & Configuration Tests
        TR.Run("GC_Info_SimpleMemoryInfo", TestGCInfoSimpleMemoryInfo);
        TR.Run("GC_Info_HeapAndCommittedRelations", TestGCInfoHeapAndCommittedRelations);
        TR.Run("GC_Info_GenerationSizeAndFragmentation", TestGCInfoGenerationSizeAndFragmentation);
        TR.Run("GC_Info_LastGenInfoUpdated", TestGCInfoLastGenInfoUpdated);
        TR.Run("GC_Info_GetObjectGeneration", TestGCInfoGetObjectGeneration);
        TR.Run("GC_Info_GCSegmentSizeAndPercent", TestGCInfoGCSegmentSizeAndPercent);
        TR.Run("GC_Info_RhGetMemoryInfoWiring", TestGCInfoRhGetMemoryInfoWiring);
        TR.Run("GC_Info_MemoryInfoFrozenBetweenCollections", TestGCInfoMemoryInfoFrozenBetweenCollections);
        TR.Run("GC_Variables", TestGCVariables);

        // TLAB (Thread-Local Allocation Buffer) Tests
        TR.Run("GC_TLAB_AllocBytesNonZero", TestTlabAllocBytesNonZero);
        TR.Run("GC_TLAB_AllocBytesIncrease", TestTlabAllocBytesIncrease);
        TR.Run("GC_TLAB_PreciseLessOrEqualTotal", TestTlabPreciseLessOrEqualTotal);
        TR.Run("GC_TLAB_SurvivalAfterCollect", TestTlabSurvivalAfterCollect);
        TR.Run("GC_TLAB_GapStampedOnCollect", TestTlabGapStampedOnCollect);

        TR.Finish();

        Serial.WriteString("\n[Tests Complete - System Halting]\n");
    }

    protected override void Run()
    {
        // All tests ran in BeforeRun; stop the main loop after one iteration
        Stop();
    }

    protected override void AfterRun()
    {
        // Flush coverage data and signal QEMU to terminate
        TR.Complete();
        Cosmos.Kernel.System.Power.Halt();
    }

    // ==================== Garbage Collection Tests ====================

    private static void TestGCIsEnabled()
    {
        Assert.True(CoreGC.IsEnabled, "GC: IsEnabled should be true");
    }

    private static void TestGCGetStats()
    {
        CoreGC.GetStats(out int totalCollections, out int totalObjectsFreed);
        // After running all previous tests, some collections may have already happened
        // (from allocation pressure). Both counters must be non-negative.
        Assert.True(totalCollections >= 0, "GC: totalCollections must be non-negative");
        Assert.True(totalObjectsFreed >= 0, "GC: totalObjectsFreed must be non-negative");
    }

    private static void TestGCCollectBasic()
    {
        // Snapshot before
        CoreGC.GetStats(out int collsBefore, out int freedBefore);

        int freed = CoreGC.Collect();
        Assert.True(freed >= 0, "GC: Collect must return non-negative freed count");

        // Verify stats incremented by exactly 1 collection
        CoreGC.GetStats(out int collsAfter, out int freedAfter);
        Assert.Equal(collsBefore + 1, collsAfter, "GC: Collect must increment collection count by exactly 1");
        Assert.Equal(freedBefore + freed, freedAfter, "GC: totalObjectsFreed must increase by exact freed count");
    }

    private static void TestGCStatsIncrement()
    {
        // Run 3 collections and verify exact increments
        CoreGC.GetStats(out int collsBefore, out int freedBefore);

        int freed1 = CoreGC.Collect();
        int freed2 = CoreGC.Collect();
        int freed3 = CoreGC.Collect();

        CoreGC.GetStats(out int collsAfter, out int freedAfter);

        // Exactly 3 collections must have been recorded
        Assert.Equal(collsBefore + 3, collsAfter, "GC: 3 successive Collects must increment count by exactly 3");

        // Total freed must be exact sum
        int expectedFreed = freedBefore + freed1 + freed2 + freed3;
        Assert.Equal(expectedFreed, freedAfter, "GC: totalObjectsFreed must equal sum of all Collect return values");
    }

    private static void TestGCExactCollectionCount()
    {
        // Verify that even a collection that frees nothing still increments the counter
        // First collect to clean up any existing garbage
        CoreGC.Collect();

        CoreGC.GetStats(out int collsBefore, out int freedBefore);

        // Second collect on a clean heap — likely frees 0 objects
        int freed = CoreGC.Collect();

        CoreGC.GetStats(out int collsAfter, out int freedAfter);

        // Collection count must always increment by 1, even if freed == 0
        Assert.Equal(collsBefore + 1, collsAfter, "GC: collection count increments even when freed == 0");
        Assert.Equal(freedBefore + freed, freedAfter, "GC: freed accounting exact even for zero-freed collection");
    }

    private static void TestGCObjectSurvival()
    {
        // Reachable boxed values must survive collection
        object boxed = 42;
        CoreGC.Collect();
        Assert.True((int)boxed == 42, "GC: boxed int survives collection");
    }

    private static void TestGCStringSurvival()
    {
        // Dynamically created strings must survive when reachable
        string s1 = "Hello";
        string s2 = "World";
        string concat = s1 + " " + s2;
        CoreGC.Collect();
        Assert.True(concat == "Hello World", "GC: concatenated string survives collection");
    }

    private static void TestGCArraySurvival()
    {
        // Arrays must survive collection when reachable
        int[] arr = new int[10];
        for (int i = 0; i < 10; i++)
        {
            arr[i] = i * 10;
        }

        CoreGC.Collect();
        Assert.Equal(10, arr.Length, "GC: array length survives collection");
        Assert.True(arr[0] == 0 && arr[5] == 50 && arr[9] == 90, "GC: array contents survive collection");
    }

    private static void TestGCListSurvival()
    {
        // List<T> with internal array must survive collection
        List<int> list = new List<int>();
        list.Add(100);
        list.Add(200);
        list.Add(300);
        CoreGC.Collect();
        Assert.Equal(3, list.Count, "GC: list count survives collection");
        Assert.True(list[0] == 100 && list[1] == 200 && list[2] == 300, "GC: list contents survive collection");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateExactUnreachable(int count, int arraySize)
    {
        // Each iteration creates exactly 1 unreachable byte[] object
        for (int i = 0; i < count; i++)
        {
            object obj = new byte[arraySize];
            // Prevent optimizer from removing the allocation
            if (obj == null)
            {
                break;
            }
        }
    }

    private static void TestGCUnreachableExactCount()
    {
        // Collect twice to stabilize — clear any prior garbage
        CoreGC.Collect();
        CoreGC.Collect();

        CoreGC.GetStats(out int _, out int freedBefore);

        // Allocate exactly 50 unreachable byte[64] arrays
        // Each byte[64]: BaseSize(24) + 64*1 = 88, Align(88) = 88
        const int objectCount = 50;
        AllocateExactUnreachable(objectCount, 64);

        // Collect — must free exactly those 50 objects (plus possibly some
        // internal allocations from the loop itself, e.g., enumerator objects)
        int freed = CoreGC.Collect();

        CoreGC.GetStats(out int _2, out int freedAfter);

        // freed must be at least objectCount - 2 (conservative scanning may
        // falsely retain a few objects when stack values coincidentally look like
        // GC heap pointers)
        Assert.True(freed >= objectCount - 2,
            "GC: must free at least " + (objectCount - 2) + " unreachable byte[64] objects, freed: " + freed);

        // freedAfter - freedBefore must match the Collect return value exactly
        Assert.Equal(freed, freedAfter - freedBefore,
            "GC: totalObjectsFreed delta must match Collect return value exactly");
    }

    private static void TestGCObjectGraphSurvival()
    {
        // Object graph: list holding strings must keep everything alive
        List<string> strings = new List<string>();
        strings.Add("Alpha");
        strings.Add("Beta");
        strings.Add("Gamma");

        CoreGC.Collect();

        Assert.Equal(3, strings.Count, "GC: object graph list count survives");
        Assert.True(strings[0] == "Alpha", "GC: object graph first element survives");
        Assert.True(strings[2] == "Gamma", "GC: object graph last element survives");
    }

    private static void TestGCMixedTypeSurvival()
    {
        // Various types allocated and kept alive across GC
        byte[] byteArr = new byte[] { 0xAA, 0xBB, 0xCC };
        int[] intArr = new int[] { 1, 2, 3 };
        string str = "MixedTest";
        object boxedLong = 9876543210L;
        TestPoint point = new TestPoint { X = 42, Y = 99 };
        object boxedPoint = point;

        CoreGC.Collect();

        Assert.True(byteArr[0] == 0xAA && byteArr[2] == 0xCC, "GC: byte array survives mixed collection");
        Assert.Equal(2, intArr[1], "GC: int array survives mixed collection");
        Assert.True(str == "MixedTest", "GC: string survives mixed collection");
        Assert.True((long)boxedLong == 9876543210L, "GC: boxed long survives mixed collection");
        TestPoint unboxed = (TestPoint)boxedPoint;
        Assert.True(unboxed.X == 42 && unboxed.Y == 99, "GC: boxed struct survives mixed collection");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateGarbage(int count, int size)
    {
        for (int i = 0; i < count; i++)
        {
            byte[] temp = new byte[size];
            if (temp == null)
            {
                break;
            }
        }
    }

    private static void TestGCAllocAfterCollect()
    {
        // Generate garbage then collect, then allocate again
        AllocateGarbage(50, 256);

        CoreGC.GetStats(out int collsBefore, out int _);
        int freed = CoreGC.Collect();
        CoreGC.GetStats(out int collsAfter, out int _2);

        // Exactly 1 collection must have been recorded
        Assert.Equal(collsBefore + 1, collsAfter, "GC: exactly 1 collection after AllocateGarbage");
        // Must have freed at least 50 objects
        Assert.True(freed >= 50, "GC: must free at least 50 garbage byte[256] arrays, freed: " + freed);

        // Must be able to allocate after GC reclaims memory
        int[] newArr = new int[100];
        for (int i = 0; i < 100; i++)
        {
            newArr[i] = i;
        }

        Assert.Equal(99, newArr[99], "GC: allocation works after collection");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateWeakRef()
    {
        object target = new byte[128];
        return new WeakReference(target);
    }

    private static void TestGCWeakReference()
    {
        // Create a weak reference to an object that becomes unreachable
        WeakReference weakRef = CreateWeakRef();

        // After collection, the weak handle's target must be cleared
        CoreGC.Collect();
        CoreGC.Collect(); // Second pass to ensure weak handles are freed

        Assert.True(!weakRef.IsAlive, "GC: weak reference cleared after collection");
        Assert.True(weakRef.Target == null, "GC: weak reference target is null after collection");
    }

    private static void TestGCLargeAllocationAndCollect()
    {
        // Allocate 20 × byte[4096], let them become garbage, then collect
        const int count = 20;
        AllocateGarbage(count, 4096);

        CoreGC.GetStats(out int _, out int freedBefore);
        int freed = CoreGC.Collect();
        CoreGC.GetStats(out int _2, out int freedAfter);

        // Must free at least the 20 large arrays
        Assert.True(freed >= count,
            "GC: must free at least " + count + " large byte[4096] arrays, freed: " + freed);
        Assert.Equal(freed, freedAfter - freedBefore,
            "GC: freedAfter - freedBefore must match freed count exactly");

        // Must be able to allocate again after large garbage is collected
        byte[] postGC = new byte[8192];
        postGC[0] = 0xDE;
        postGC[8191] = 0xAD;
        Assert.Equal((byte)0xDE, postGC[0], "GC: post-GC large alloc byte 0");
        Assert.Equal((byte)0xAD, postGC[8191], "GC: post-GC large alloc last byte");
    }

    private static void TestGCStructArraySurvival()
    {
        // Array of structs with value-type fields must survive GC
        TestPoint[] points = new TestPoint[5];
        for (int i = 0; i < 5; i++)
        {
            points[i] = new TestPoint { X = i * 10, Y = i * 20 };
        }

        CoreGC.Collect();

        Assert.Equal(5, points.Length, "GC: struct array length survives");
        Assert.Equal(0, points[0].X, "GC: struct array [0].X survives");
        Assert.Equal(0, points[0].Y, "GC: struct array [0].Y survives");
        Assert.Equal(40, points[4].X, "GC: struct array [4].X survives");
        Assert.Equal(80, points[4].Y, "GC: struct array [4].Y survives");
    }

    private static void TestGCDictionarySurvival()
    {
        // Dictionary with string keys and int values must survive GC
        Dictionary<string, int> dict = new Dictionary<string, int>();
        dict.Add("One", 1);
        dict.Add("Two", 2);
        dict.Add("Three", 3);

        CoreGC.Collect();

        Assert.Equal(3, dict.Count, "GC: dictionary count survives collection");
        Assert.Equal(1, dict["One"], "GC: dictionary value 'One' survives");
        Assert.Equal(3, dict["Three"], "GC: dictionary value 'Three' survives");
        Assert.True(dict.ContainsKey("Two"), "GC: dictionary ContainsKey after collection");
    }

    private static void TestGCPageAccounting()
    {
        // Verify PageAllocator accounting is consistent before and after GC
        ulong totalPages = PageAllocator.TotalPageCount;
        ulong freePagesBefore = PageAllocator.FreePageCount;
        ulong usedPagesBefore = totalPages - freePagesBefore;

        // Total pages must be positive
        Assert.True(totalPages > 0, "GC: TotalPageCount must be > 0");

        // Used pages must not exceed total
        Assert.True(usedPagesBefore <= totalPages,
            "GC: used pages (" + usedPagesBefore + ") must be <= total (" + totalPages + ")");

        // Allocate garbage to consume pages, then collect to reclaim
        AllocateGarbage(30, 4096);
        CoreGC.Collect();

        ulong freePagesAfter = PageAllocator.FreePageCount;

        // Total pages must stay constant (physical memory doesn't change)
        Assert.Equal((int)totalPages, (int)PageAllocator.TotalPageCount,
            "GC: TotalPageCount must not change after GC");

        // Free pages should stay the same or increase after GC (segments released)
        Assert.True(freePagesAfter >= freePagesBefore,
            "GC: free pages after collect (" + freePagesAfter + ") must be >= before (" + freePagesBefore + ")");
    }

    // ==================== Dependent Handle & Handle Store Tests ====================

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (object key, ConditionalWeakTable<object, byte[]> table) CreateDependentHandleScenario()
    {
        object key = new object();
        var table = new ConditionalWeakTable<object, byte[]>();
        table.AddOrUpdate(key, new byte[64]);
        return (key, table);
    }

    private static void TestGCDependentHandle()
    {
        // A ConditionalWeakTable keeps its values alive as long as the key is alive.
        // The value has NO direct reference from the key - only through the dependent handle.
        var (key, table) = CreateDependentHandleScenario();

        CoreGC.Collect();

        // The value should survive because key is still alive
        bool found = table.TryGetValue(key, out byte[] value);
        Assert.True(found, "GC: ConditionalWeakTable value must survive when key is alive");
        Assert.True(value != null && value.Length == 64, "GC: ConditionalWeakTable value data intact");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ConditionalWeakTable<object, byte[]> CreateOrphanedDepHandle()
    {
        var table = new ConditionalWeakTable<object, byte[]>();
        object key = new object();
        table.AddOrUpdate(key, new byte[128]);
        // key becomes unreachable after return
        return table;
    }

    private static void TestGCDependentHandleCleanup()
    {
        var table = CreateOrphanedDepHandle();

        CoreGC.Collect();
        CoreGC.Collect();

        // Table should be empty now - the key is dead, so the entry should be removed
        int count = 0;
        foreach (var kv in table)
        {
            count++;
        }

        Assert.Equal(0, count, "GC: ConditionalWeakTable entries cleared when key is dead");
    }

    private static void TestGCDependentHandleNullValue()
    {
        // A null value is a dependent handle with no secondary. SharedArrayPool registers
        // its thread-local buckets that way, so the first formatted interpolated string
        // leaves one behind; the mark phase must skip it instead of reading a mark bit
        // through the null secondary.
        ConditionalWeakTable<object, object?> table = new();
        object key = new();
        table.Add(key, null);

        CoreGC.Collect();

        bool found = table.TryGetValue(key, out object? value);
        Assert.True(found && value is null, "GC: dependent handle with no secondary survives a collection");
        GC.KeepAlive(key);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (WeakReference, WeakReference, WeakReference) CreateThreeWeakRefs()
    {
        // NoInlining ensures the byte[] pointers don't remain on the caller's stack
        return (
            new WeakReference(new byte[32]),
            new WeakReference(new byte[32]),
            new WeakReference(new byte[32])
        );
    }

    private static void TestGCHandleStoreIntegrity()
    {
        // Allocate handles via NoInlining helper to avoid conservative scan false positives
        var (wr1, wr2, wr3) = CreateThreeWeakRefs();

        // Force collection to exercise handle scanning
        CoreGC.Collect();
        CoreGC.Collect();

        // All weak refs should be cleared (targets are unreachable)
        Assert.True(!wr1.IsAlive, "GC: handle store wr1 cleared");
        Assert.True(!wr2.IsAlive, "GC: handle store wr2 cleared");
        Assert.True(!wr3.IsAlive, "GC: handle store wr3 cleared");

        // Allocate one more handle to verify store still works. The final assertion compares
        // wr4.Target against the strong local `alive` — this forces `alive` to be live across
        // the Collect (NativeAOT/RyuJIT would otherwise drop it after the WeakReference ctor,
        // since weak handles don't root the target, and GCInfo would omit it at the safepoint —
        // making the byte[64] unreachable under precise stack scan, issue #346). Conservative
        // scan happened to over-root the dead spill slot, hiding the missing real use.
        object alive = new byte[64];
        WeakReference wr4 = new WeakReference(alive);
        CoreGC.Collect();
        Assert.True(ReferenceEquals(wr4.Target, alive),
            "GC: handle store works after cleanup (weak ref still resolves to the strong local)");
    }

    private static void TestGCPinnedHeapReuse()
    {
        // Verify pinned allocations don't crash after GC
        GC.AllocateArray<byte>(32, pinned: true);
        CoreGC.Collect();
        byte[] arr = GC.AllocateArray<byte>(32, pinned: true);
        Assert.True(arr != null, "GC: pinned allocation works after collection");
    }

    // ==================== Stack-Scan Padding Stress (issue #346) ====================

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateWeakRefBare()
    {
        // The byte[128] strong ref only lives on this frame. On return the
        // compiler-chosen spill slot becomes a stale pointer — exactly the
        // shape conservative-scan false-rooting feeds on (issue #346).
        object target = new byte[128];
        return new WeakReference(target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RunStackScanPaddingShape(int shape)
    {
        // `shape + 1` stack-resident longs change the frame size in 8-byte
        // increments per call, shifting spill-slot offsets across runs.
        Span<long> pad = stackalloc long[shape + 1];
        for (int i = 0; i < pad.Length; i++)
        {
            // Sentinel values stay below the kernel higher-half (0xFFFF8000...)
            // so the scanner's MethodTable check rejects them even if they
            // accidentally satisfy IsInGCHeap.
            pad[i] = (long)(0x00C0DE00L | (long)((shape << 4) | i));
        }

        WeakReference wr = CreateWeakRefBare();

        CoreGC.Collect();
        CoreGC.Collect();

        // Keep `pad` live across the collect so the JIT can't shrink its
        // lifetime and undo the padding before the GC scan runs.
        long sink = 0;
        for (int i = 0; i < pad.Length; i++)
        {
            sink ^= pad[i];
        }
        if (sink == -1L)
        {
            Serial.WriteString("");
        }

        Assert.True(!wr.IsAlive,
            "GC: padding shape " + shape + " must clear weak ref (issue #346)");
    }

    private static void TestGCStackScanPaddingStress()
    {
        // The mark phase walks every word of each thread's stack and treats
        // anything that looks like a heap pointer as a root. TestGCWeakReference
        // already covers one frame shape; this test widens the net by varying
        // the count of stack-resident locals across 8 shapes so a stale spill
        // from CreateWeakRefBare lands at different offsets per run. All shapes
        // must clear the weak reference — a flake on any shape means
        // conservative-scan false-rooting has crept back (the failure mode that
        // commit 2f1b6d17 reverted).
        for (int shape = 0; shape < 8; shape++)
        {
            RunStackScanPaddingShape(shape);
        }
    }

    // ==================== Precise GCInfo decoder (issue #346, phase 1) ====================

    private static unsafe void TestGCGcInfoDecoder()
    {
        // --- 1. Bit-stream reader on synthetic data (alignment-independent: assertions are
        //        phrased in terms of the logical bit stream, which the reader normalizes). ---
        // buf[0] = 0xA5 (LSB-first 1,0,1,0 | 0,1,0,1): Read(4)==5, Read(4)==10.
        // buf[1] = 0x55 (0,1,0,1 | 0,1,0,1): DecodeVarLengthUnsigned(3) reads 4 bits == 5 (no
        //          extension bit); DecodeVarLengthSigned(3) reads 4 bits, payload 0b101 -> -3.
        // buf[2..7] = 0: Read(48) of those bytes == 0. buf[8] = 0xAB: the next Read(8) crosses the
        //          internal word boundary and yields 0xAB.
        byte* buf = stackalloc byte[24];
        for (int i = 0; i < 24; i++)
        {
            buf[i] = 0;
        }
        buf[0] = 0xA5;
        buf[1] = 0x55;
        buf[8] = 0xAB;

        GcInfoBitStreamReader r = new GcInfoBitStreamReader(buf);
        Assert.True(r.Read(4) == 5UL, "GcInfo.BitReader: Read(4) of 0xA5 low nibble == 5");
        Assert.True(r.Read(4) == 10UL, "GcInfo.BitReader: Read(4) of 0xA5 high nibble == 10");
        Assert.True(r.DecodeVarLengthUnsigned(3) == 5UL, "GcInfo.BitReader: DecodeVarLengthUnsigned(3) == 5");
        Assert.True(r.DecodeVarLengthSigned(3) == -3L, "GcInfo.BitReader: DecodeVarLengthSigned(3) == -3");
        Assert.True(r.Read(48) == 0UL, "GcInfo.BitReader: Read(48) over zero bytes == 0");
        Assert.True(r.Read(8) == 0xABUL, "GcInfo.BitReader: Read(8) across the word boundary == 0xAB");

        // --- 2. Decode real GCInfo for a known method via the .eh_frame -> LSDA walk ---
        delegate*<int, string> probe = &DescribeShape;
        nuint probeIp = (nuint)(void*)probe;

        bool found = MethodGcInfoLookup.TryGetMethodGcInfo(probeIp, out MethodGcInfoLookup.MethodGcInfo mi);
        Assert.True(found, "GcInfo: TryGetMethodGcInfo must locate the probe method's GCInfo");
        Assert.True(mi.GcInfo != null, "GcInfo: resolved GCInfo pointer must be non-null");
        Assert.False(mi.IsFunclet, "GcInfo: probe method entry must resolve to a root function");
        Assert.Equal(0u, mi.CodeOffset, "GcInfo: probe ip == method entry -> code offset 0");

        GcInfoDecoder headerDecoder = new GcInfoDecoder(mi.GcInfo, GcInfoEncoding.GCINFO_VERSION, GcInfoDecoderFlags.DECODE_EVERYTHING, 0);
        Assert.Equal(GcInfoEncoding.GCINFO_VERSION, headerDecoder.Version, "GcInfo: decoded version must be GCINFO_VERSION (4)");
        uint fdeRange = (uint)(mi.MethodEnd - mi.MethodStart);
        Assert.Equal(fdeRange, headerDecoder.CodeLength, "GcInfo: decoded code length must equal the FDE PC-range for a root method");

        // The probe holds object references live across calls, so its GCInfo describes >= 1 slot.
        GcInfoDecoder lifetimesDecoder = new GcInfoDecoder(mi.GcInfo, GcInfoEncoding.GCINFO_VERSION, GcInfoDecoderFlags.DECODE_GC_LIFETIMES, mi.CodeOffset);
        uint slotCount = lifetimesDecoder.GetGcSlotCount();
        Assert.True(slotCount > 0u, "GcInfo: a method with object refs must have at least one GC slot");

        // --- 3. EnumerateLiveSlots must run to completion at the prolog without faulting ---
        GcInfoDecoder enumDecoder = new GcInfoDecoder(mi.GcInfo, GcInfoEncoding.GCINFO_VERSION, GcInfoDecoderFlags.DECODE_GC_LIFETIMES, 0);
        Cosmos.Kernel.Core.Runtime.REGDISPLAY rd = default;
        int reportCount = 0;
        bool ok = enumDecoder.EnumerateLiveSlots(&rd, reportScratchSlots: false, CodeManagerFlags.None, &CountGcRefCallback, &reportCount);
        Assert.True(ok, "GcInfo: EnumerateLiveSlots must return true (slot table fit, no fault)");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string DescribeShape(int shape)
    {
        // A non-trivial method whose locals/args/return are object references, used only as a stable
        // managed code address for GCInfo decoding (it is never actually invoked for its result).
        string[] names = new string[] { "alpha", "beta", "gamma" };
        string s = names[((uint)shape) % 3u];
        return s + "-" + shape.ToString();
    }

    private static unsafe void CountGcRefCallback(void* ctx, nuint* pObjRef, uint gcRefFlags)
    {
        // Do NOT dereference pObjRef — the REGDISPLAY passed by the test is a zeroed stub; only count.
        *(int*)ctx = *(int*)ctx + 1;
    }

    // ==================== Precise GCInfo stack scan (issue #346, phase 2) ====================

    private static unsafe void TestGCPreciseStackScan()
    {
        // 1. The shared .eh_frame / LSDA parser: TryGetMethodLSDA must agree with TryGetMethodGcInfo
        //    (ExceptionHelper.TryGetMethodLSDA now delegates to MethodGcInfoLookup — one parser).
        delegate*<int, string> probe = &DescribeShape;
        nuint probeIp = (nuint)(void*)probe;

        bool gotGcInfo = MethodGcInfoLookup.TryGetMethodGcInfo(probeIp, out MethodGcInfoLookup.MethodGcInfo mi);
        Assert.True(gotGcInfo, "GC: TryGetMethodGcInfo must locate the probe method");
        bool gotLsda = MethodGcInfoLookup.TryGetMethodLSDA(probeIp, out nuint methodStart, out byte* pLsda);
        Assert.True(gotLsda, "GC: TryGetMethodLSDA must locate the probe method");
        Assert.True(pLsda != null, "GC: TryGetMethodLSDA must return a non-null LSDA pointer");
        Assert.True(methodStart == mi.MethodStart, "GC: TryGetMethodLSDA and TryGetMethodGcInfo agree on method start");

        // 2. Spill-aliasing repro (issue #346): a byte[128] whose only strong ref outlives the local
        //    in a compiler-chosen spill slot. The precise scan ignores that dead slot, so the weak
        //    ref must be cleared after collection — this is the shape that false-rooted under the old
        //    conservative scan when InterruptScope's layout shifted.
        for (int shape = 0; shape < 6; shape++)
        {
            RunPreciseProbeShape(shape);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RunPreciseProbeShape(int shape)
    {
        // `shape + 1` stack-resident longs shift spill-slot offsets across runs (see #346).
        Span<long> pad = stackalloc long[shape + 1];
        for (int i = 0; i < pad.Length; i++)
        {
            // Sentinels stay below the kernel higher-half so the scanner's MethodTable check
            // rejects them even if they happen to satisfy IsInGCHeap.
            pad[i] = (long)(0x00BEEF00L | (long)((shape << 4) | i));
        }

        WeakReference wr = CreateWeakRefBare();

        CoreGC.Collect();
        CoreGC.Collect();

        // Keep `pad` live across the collect so the JIT can't shrink its lifetime.
        long sink = 0;
        for (int i = 0; i < pad.Length; i++)
        {
            sink ^= pad[i];
        }
        if (sink == -1L)
        {
            Serial.WriteString("");
        }

        Assert.True(!wr.IsAlive,
            "GC: precise scan must clear weak ref (shape " + shape + ", issue #346)");
    }

    // ==================== Precise funclet-frame stack scan (issue #346, phase 3) ====================

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowFuncletSentinel(int shape)
    {
        throw new Exception("funclet-sentinel-" + shape);
    }

    // A real catch funclet that does work: allocates, writes through the `out` byref (which ILC may
    // keep in a callee-saved register across the try), and references `ex`. Exercises that
    // RhpCallCatchFunclet sets the establisher frame's registers up for the funclet AND resumes the
    // catching method properly (jump to the funclet's returned IP at the body SP, run the epilogue)
    // rather than force-returning from it. `result` (= the caller's local) is written here and read
    // after this returns; once the catch exits, `dead`'s only strong ref is gone.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PopulateFuncletWeakRef(int shape, out WeakReference result)
    {
        result = null!;
        try
        {
            ThrowFuncletSentinel(shape);
        }
        catch (Exception ex)
        {
            byte[] dead = new byte[128];
            dead[0] = (byte)shape;
            result = new WeakReference(dead);
            GC.KeepAlive(dead);   // force dead's GCInfo slot live to exactly this IP, then dead
            _ = ex;               // keep a real funclet frame (ex slot reported by GCInfo)
        }
    }

    private static void TestGCFuncletNoFalseRoot()
    {
        // GC.Collect is called AFTER the catch exits so no funclet frame is on the stack during
        // the scan. `dead` had its only strong ref inside the funclet; once the funclet returns,
        // the precise scan of the parent frame must not resurrect it from a stale slot.
        // This is the funclet-frame analogue of GC_PreciseStackScan (issue #346).
        for (int shape = 0; shape < 4; shape++)
        {
            PopulateFuncletWeakRef(shape, out WeakReference wr);
            CoreGC.Collect();
            CoreGC.Collect();
            Assert.True(!wr.IsAlive,
                "GC: a value whose only ref lived in a catch funclet must be collected after the catch exits (shape " + shape + ", issue #346)");
        }
    }

    // A catch handler that does real work while its funclet frame is live on the stack: allocates,
    // forces a collection from *inside* the catch, then reads its allocation and the exception back.
    // The GC's stack walk meets the funclet frame, the RhpCallCatchFunclet trampoline, and the
    // exception-dispatch frames mid-dispatch. `keep` and `ex` are referenced after the collect so
    // their GCInfo slots stay live across it (and so the precise funclet scan must report them).
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static int AllocAndCollectInCatch(int shape)
    {
        try
        {
            ThrowFuncletSentinel(shape);
            return -1;   // unreachable — ThrowFuncletSentinel always throws
        }
        catch (Exception ex)
        {
            byte[] keep = new byte[256];
            for (int i = 0; i < keep.Length; i++)
            {
                keep[i] = (byte)(shape + i);
            }
            CoreGC.Collect();
            CoreGC.Collect();
            int sum = 0;
            for (int i = 0; i < keep.Length; i++)
            {
                sum += keep[i];
            }
            return ex != null ? sum : -1;
        }
    }

    private static void TestGCFuncletNoCrashOnAllocInCatch()
    {
        // GC.Collect() forced from inside a catch funclet must run to completion: the precise stack
        // scan reaches the funclet frame (decoded from the main method's GCInfo at the synthetic
        // funclet offset, against the establisher RBP the funclet shares), steps through the
        // RhpCallCatchFunclet trampoline, and precisely scans the managed exception-dispatch frames
        // below it — without faulting, and with the funclet's live locals surviving. Issue #227 /
        // epic #348 phase 3.
        for (int shape = 1; shape <= 4; shape++)
        {
            int sum = AllocAndCollectInCatch(shape);
            // (shape + i) mod 256 over i in [0, 256) is a permutation of [0, 256) → sum is always 32640.
            Assert.Equal(32640, sum,
                "GC: a catch funclet's allocations must survive a collection forced from inside the catch (shape " + shape + ", issue #227)");
        }
    }

    // ==================== EH dispatch through FP-omitted intermediate frames ===================
    //
    // ILC's RyuJIT-based codegen omits the frame pointer on managed methods that don't need it
    // (no EH clauses, no stackalloc, no large frame, no address-taken locals) — the same effect
    // C's `-fomit-frame-pointer` has on a function. The five DeepThrowLevel* helpers below are
    // deliberately starved of every characteristic that forces RBP/X29 to be kept; on at least
    // some of them RyuJIT is free to use the frame register as a general-purpose scratch.
    //
    // If the EH dispatcher's frame-pointer-chain walker (`ExceptionHelper.UnwindOneFrame`) is
    // the only thing stepping between the throw site and the catching method, an FP-omitted
    // intermediate breaks the chain — `[RBP]` no longer points at the caller's saved RBP, the
    // walker reads scratch, and either `TryFindHandler` runs against a garbage return address or
    // the loop bottoms out without finding the handler. Either way the kernel halts and this
    // test never returns.
    //
    // The catching method (`CatchAtopDeepChain`) has a try/catch, so RyuJIT must keep its own
    // RBP — that part of the unwind is sound. The fragility, if any, is entirely in the chain
    // of intermediates between the throw and the catch.

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DeepThrowLevel0()
    {
        throw new Exception("deep-throw");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DeepThrowLevel1()
    {
        DeepThrowLevel0();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DeepThrowLevel2()
    {
        DeepThrowLevel1();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DeepThrowLevel3()
    {
        DeepThrowLevel2();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DeepThrowLevel4()
    {
        DeepThrowLevel3();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static string CatchAtopDeepChain()
    {
        try
        {
            DeepThrowLevel4();
            return "unreached";
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private static void TestGCThrowThroughDeepChain()
    {
        // The throw happens five managed frames below the catch. If the EH dispatch survives any
        // of those frames omitting RBP/X29, this round-trips the exception message; otherwise the
        // kernel halts and the test runner reports the test as failed (or never completes).
        string caught = CatchAtopDeepChain();
        Assert.True(caught == "deep-throw",
            "EH: a throw must propagate through 5 intermediate frames that may omit RBP/X29, got '" + caught + "'");
    }

    // ==================== GC Soundness Tests ====================

    /// <summary>
    /// Allocates a TLAB-sized array and returns a byref into it. NoInlining guarantees the
    /// array reference itself dies with this frame — the returned interior pointer is the only
    /// root left. TLAB-sized (8416+ bytes) so that, if wrongly freed, the churn's TLAB refill
    /// takes the free block and zeroes it, making the collection observable.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ref int LeakInteriorRef()
    {
        int[] arr = new int[2100];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = 0x5A5A0000 + i;
        }

        return ref arr[8];
    }

    private static void TestGCInteriorPointerRoot()
    {
        ref int r = ref LeakInteriorRef();

        Assert.Equal(0x5A5A0008, r, "GC: interior-pointer array must be intact before collect");

        CoreGC.Collect();
        int afterCollect = r;

        // Churn: TLAB refills reuse freed >=8KB blocks and zero them, so a wrongly freed
        // array reads back 0 (or junk) here instead of the 0x5A5A pattern.
        for (int i = 0; i < 512; i++)
        {
            int[] junk = new int[32];
            junk[0] = i;
        }

        Serial.WriteString("[Test] interior byref after collect: ");
        Serial.WriteHex((uint)afterCollect);
        Serial.WriteString(", after churn: ");
        Serial.WriteHex((uint)r);
        Serial.WriteString("\n");

        Assert.Equal(0x5A5A0008, r,
            "GC: an object whose only root is an interior pointer (byref/Span) must survive collection (#384)");
    }

    /// <summary>Holds the only reference to the statics-reachability test array.</summary>
    private static int[]? s_staticOnlyArray;

    /// <summary>
    /// Stores a fresh array into a static field. NoInlining so the array reference dies with
    /// this frame — after return, the static field is the only root.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateStaticOnlyArray()
    {
        int[] arr = new int[2100];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = 0x0BAD0000 + i;
        }

        s_staticOnlyArray = arr;
    }

    private static void TestGCStaticOnlyReachability()
    {
        AllocateStaticOnlyArray();

        CoreGC.Collect();

        for (int i = 0; i < 512; i++)
        {
            int[] junk = new int[32];
            junk[0] = i;
        }

        int[]? arr = s_staticOnlyArray;
        Assert.True(arr != null && arr.Length == 2100 && arr[8] == 0x0BAD0008 && arr[2099] == 0x0BAD0000 + 2099,
            "GC: an object whose only root is a static field must survive collection");

        s_staticOnlyArray = null;
    }

    private static volatile bool s_churnStop;
    private static int s_churnStartedCount;
    private static int s_churnDoneCount;
    private static int s_churnErrorCount;
    private static int s_churnIterations;

    /// <summary>
    /// Worker: allocates patterned arrays in a ring while the main thread forces collections.
    /// While this thread is parked, the GC scans its stack and saved registers; if the scan
    /// misses a live survivor, the recycled memory loses the 0x7C pattern and the worker
    /// records an error.
    /// </summary>
    private static void GCChurnWorker()
    {
        Interlocked.Increment(ref s_churnStartedCount);

        int[][] keep = new int[8][];
        int iteration = 0;
        while (!s_churnStop)
        {
            int[] fresh = new int[64];
            for (int i = 0; i < fresh.Length; i++)
            {
                fresh[i] = 0x7C000000 + i;
            }

            keep[iteration & 7] = fresh;

            int[] survivor = keep[(iteration >> 2) & 7];
            if (survivor != null && (survivor[0] != 0x7C000000 || survivor[63] != 0x7C00003F))
            {
                Interlocked.Increment(ref s_churnErrorCount);
            }

            Interlocked.Increment(ref s_churnIterations);
            iteration++;
        }

        Interlocked.Increment(ref s_churnDoneCount);
    }

    private static void TestGCMultithreadChurnUnderCollect()
    {
        s_churnStop = false;
        s_churnStartedCount = 0;
        s_churnDoneCount = 0;
        s_churnErrorCount = 0;
        s_churnIterations = 0;

        Thread worker1 = new Thread(GCChurnWorker);
        Thread worker2 = new Thread(GCChurnWorker);
        worker1.Start();
        worker2.Start();

        for (int i = 0; i < 200 && s_churnStartedCount < 2; i++)
        {
            TimerManager.Wait(10);
        }

        Assert.Equal(2, s_churnStartedCount, "GC: both churn workers must start");

        // Force collections while the workers allocate. The workers are parked during each
        // Collect, so their survivors are only reachable through the parked-thread stack scan.
        for (int c = 0; c < 10; c++)
        {
            for (int i = 0; i < 64; i++)
            {
                int[] garbage = new int[48];
                garbage[0] = i;
            }

            CoreGC.Collect();
            TimerManager.Wait(5);
        }

        s_churnStop = true;
        for (int i = 0; i < 400 && s_churnDoneCount < 2; i++)
        {
            TimerManager.Wait(10);
        }

        Serial.WriteString("[Test] churn iterations: ");
        Serial.WriteNumber((uint)s_churnIterations);
        Serial.WriteString(", errors: ");
        Serial.WriteNumber((uint)s_churnErrorCount);
        Serial.WriteString("\n");

        Assert.Equal(2, s_churnDoneCount, "GC: both churn workers must finish after stop is signaled");
        Assert.True(s_churnIterations > 0, "GC: churn workers must have completed at least one iteration");
        Assert.Equal(0, s_churnErrorCount,
            "GC: parked-thread survivors must never be corrupted by collections (" + s_churnErrorCount + " errors)");
    }

    /// <summary>
    /// A live <see cref="Heap"/> allocation whose first word happens to hold a pointer into
    /// the GC heap (a native struct caching a managed buffer address). The GC must never free
    /// it: managed objects cannot live in the malloc heaps, so the sweep has no business there.
    /// Issue #386 — the malloc-heap sweepers' inverted MethodTable check treated exactly this
    /// shape as an unmarked managed object and freed it while live.
    /// </summary>
    private static unsafe void TestGCMallocHeapNotSwept()
    {
        byte[] managedBuffer = new byte[128];

        byte* small = Heap.Alloc(64);     // SmallHeap (SMT slot)
        byte* medium = Heap.Alloc(3000);  // MediumHeap (page + header)
        byte* large = Heap.Alloc(8192);   // LargeHeap (multi-page + header)

        // First word: a real GC-heap pointer with the LSB clear — reads as an
        // "unmarked object whose MethodTable is inside the GC heap" to the sweeper.
        fixed (byte* bp = managedBuffer)
        {
            *(byte**)small = bp;
            *(byte**)medium = bp;
            *(byte**)large = bp;
        }

        small[8] = 0xC5;

        CoreGC.Collect();

        // SmallHeap.Free zeroes the slot header's size and the data;
        // Medium/LargeHeap.Free return the pages to the page allocator (RAT -> Empty).
        bool smallAlive = SmallHeap.GetHeader(small)->Size != 0;
        bool mediumAlive = PageAllocator.GetPageType(medium) == PageType.HeapMedium;
        bool largeAlive = PageAllocator.GetPageType(large) == PageType.HeapLarge;

        Assert.True(smallAlive,
            "GC: live small-heap block must remain allocated across a collect (issue #386)");
        Assert.True(smallAlive && small[8] == 0xC5,
            "GC: live small-heap block data must be intact after a collect (issue #386)");
        Assert.True(mediumAlive,
            "GC: live medium-heap block must remain allocated across a collect (issue #386)");
        Assert.True(largeAlive,
            "GC: live large-heap block must remain allocated across a collect (issue #386)");

        GC.KeepAlive(managedBuffer);

        // Free only what survived — Heap.Free on an already-freed block panics the kernel.
        if (smallAlive)
        {
            Heap.Free(small);
        }

        if (mediumAlive)
        {
            Heap.Free(medium);
        }

        if (largeAlive)
        {
            Heap.Free(large);
        }
    }

    // ==================== GC Info & Configuration Tests ====================

    private static void TestGCInfoSimpleMemoryInfo()
    {
        // GetSimpleMemoryInfo() must report values that match each direct getter,
        // since it is built by calling them in sequence.
        CoreGC.SimpleMemoryInfo info = CoreGC.GetSimpleMemoryInfo();

        Assert.True(info.HeapSizeBytes == CoreGC.GetHeapSizeBytes(),
            "GC.Info: SimpleMemoryInfo.HeapSizeBytes must match GetHeapSizeBytes");
        Assert.True(info.FragmentedBytes == CoreGC.GetFragmentedBytes(),
            "GC.Info: SimpleMemoryInfo.FragmentedBytes must match GetFragmentedBytes");
        Assert.True(info.TotalCommittedBytes == CoreGC.GetTotalCommittedBytes(),
            "GC.Info: SimpleMemoryInfo.TotalCommittedBytes must match GetTotalCommittedBytes");
        Assert.True(info.MemoryLoadBytes == CoreGC.GetMemoryLoadBytes(),
            "GC.Info: SimpleMemoryInfo.MemoryLoadBytes must match GetMemoryLoadBytes");
        Assert.True(info.PinnedObjectsCount == CoreGC.GetPinnedObjectsCount(),
            "GC.Info: SimpleMemoryInfo.PinnedObjectsCount must match GetPinnedObjectsCount");

        // Non-generational GC: PromotedBytes is always 0 and CondemnedGeneration is always 0.
        Assert.True(info.PromotedBytes == 0UL,
            "GC.Info: PromotedBytes must be 0 (non-generational)");
        Assert.Equal(0, info.CondemnedGeneration,
            "GC.Info: CondemnedGeneration must be 0 (gen0 only)");

        // CollectionIndex must agree with GetCollectionIndex and the legacy GetStats counter.
        Assert.Equal(CoreGC.GetCollectionIndex(), info.CollectionIndex,
            "GC.Info: CollectionIndex must match GetCollectionIndex");
        CoreGC.GetStats(out int totalCollections, out int _);
        Assert.Equal(totalCollections, info.CollectionIndex,
            "GC.Info: CollectionIndex must equal GetStats totalCollections");
    }

    private static void TestGCInfoHeapAndCommittedRelations()
    {
        ulong heap = CoreGC.GetHeapSizeBytes();
        ulong committedSegs = CoreGC.GetCommittedGcSegmentsBytes();
        ulong totalCommitted = CoreGC.GetTotalCommittedBytes();

        // After dozens of allocation tests, both heap usage and segment commit must be non-zero.
        Assert.True(heap > 0UL, "GC.Info: heap size must be > 0 after prior tests");
        Assert.True(committedSegs > 0UL, "GC.Info: committed GC segments must be > 0");

        // Heap usage cannot exceed committed segment capacity.
        Assert.True(heap <= committedSegs,
            "GC.Info: heap size must be <= committed GC segments");

        // Total committed includes regular segments plus pinned/frozen/handler/etc.
        Assert.True(totalCommitted >= committedSegs,
            "GC.Info: total committed must be >= committed GC segments");

        // MemoryLoadBytes is page-based and must be > 0 (the kernel itself uses pages).
        Assert.True(CoreGC.GetMemoryLoadBytes() > 0UL,
            "GC.Info: memory load must be > 0");
    }

    private static void TestGCInfoGenerationSizeAndFragmentation()
    {
        // Gen 0 size covers regular segments only; heap size includes pinned segments too.
        ulong gen0Size = CoreGC.GetGenerationSize(0);
        ulong heapSize = CoreGC.GetHeapSizeBytes();
        Assert.True(gen0Size > 0UL, "GC.Info: gen0 size must be > 0");
        Assert.True(gen0Size <= heapSize,
            "GC.Info: GetGenerationSize(0) must be <= GetHeapSizeBytes (heap includes pinned)");

        // Non-generational GC: any non-zero generation reports 0.
        Assert.Equal(0UL, CoreGC.GetGenerationSize(1),
            "GC.Info: gen1 size must be 0");
        Assert.Equal(0UL, CoreGC.GetGenerationSize(2),
            "GC.Info: gen2 size must be 0");
        Assert.Equal(0UL, CoreGC.GetGenerationSize(-1),
            "GC.Info: invalid generation index must return 0");

        // GetCurrentFragmentation(0) must agree with GetFragmentedBytes; other gens return 0.
        Assert.True(CoreGC.GetCurrentFragmentation(0) == CoreGC.GetFragmentedBytes(),
            "GC.Info: GetCurrentFragmentation(0) must match GetFragmentedBytes");
        Assert.True(CoreGC.GetCurrentFragmentation(1) == 0UL,
            "GC.Info: gen1 fragmentation must be 0");
        Assert.True(CoreGC.GetCurrentFragmentation(2) == 0UL,
            "GC.Info: gen2 fragmentation must be 0");
    }

    private static void TestGCInfoLastGenInfoUpdated()
    {
        // Allocate garbage so the gen0 snapshot has meaningful data after the next collect.
        AllocateGarbage(40, 256);

        CoreGC.Collect();

        // After collect, gen0 last-snapshot must reflect the live heap state.
        ulong sizeBefore = CoreGC.GetLastGenSizeBefore(0);
        ulong sizeAfter = CoreGC.GetLastGenSizeAfter(0);

        Assert.True(sizeBefore > 0UL,
            "GC.Info: GetLastGenSizeBefore(0) must be > 0 after a collection on a non-empty heap");
        Assert.True(sizeAfter > 0UL,
            "GC.Info: GetLastGenSizeAfter(0) must be > 0");

        // Non-generational GC: any non-zero generation must return 0 for all four accessors.
        Assert.True(CoreGC.GetLastGenSizeBefore(1) == 0UL,
            "GC.Info: gen1 LastGenSizeBefore must be 0");
        Assert.True(CoreGC.GetLastGenSizeAfter(1) == 0UL,
            "GC.Info: gen1 LastGenSizeAfter must be 0");
        Assert.True(CoreGC.GetLastGenFragmentationBefore(1) == 0UL,
            "GC.Info: gen1 LastGenFragmentationBefore must be 0");
        Assert.True(CoreGC.GetLastGenFragmentationAfter(1) == 0UL,
            "GC.Info: gen1 LastGenFragmentationAfter must be 0");
    }

    private static void TestGCInfoGetObjectGeneration()
    {
        // Null pointer is the explicit sentinel for "not in any GC generation".
        Assert.Equal(int.MaxValue, CoreGC.GetObjectGeneration((nint)0),
            "GC.Info: null pointer must return int.MaxValue");

        // A live managed allocation must report generation 0.
        object alive = new byte[64];
        nint aliveAddr = Unsafe.As<object, nint>(ref alive);
        Assert.Equal(0, CoreGC.GetObjectGeneration(aliveAddr),
            "GC.Info: live managed object must report gen 0");

        // Keep the object reachable so the address stays valid for the duration of the test.
        Assert.NotNull(alive, "GC.Info: alive sentinel must not be null");
    }

    private static void TestGCInfoGCSegmentSizeAndPercent()
    {
        ulong segSize = CoreGC.GetGCSegmentSizeBytes();
        Assert.True(segSize > 0UL, "GC.Info: GC segment size must be > 0");

        int pct = CoreGC.GetLastGCPercentTimeInGC();
        Assert.True(pct >= 0 && pct <= 100,
            "GC.Info: GetLastGCPercentTimeInGC must be in [0, 100]");
    }

    private static void TestGCInfoRhGetMemoryInfoWiring()
    {
        // System.GC.GetGCMemoryInfo() routes through RhGetMemoryInfo, which fills a
        // GCMemoryInfoData struct from CoreGC.GetLastGCMemoryInfo() plus the
        // per-generation last-collect snapshots. The struct layout must match exactly.
        //
        // Both sides read the same frozen snapshot, so the order of these two calls no
        // longer matters: the GCMemoryInfoData instance that GC.GetGCMemoryInfo() allocates
        // may still consume a free-list block, but that cannot shift the reported values.
        CoreGC.Collect();

        GCMemoryInfo runtimeInfo = GC.GetGCMemoryInfo();
        CoreGC.SimpleMemoryInfo direct = CoreGC.GetLastGCMemoryInfo();

        Assert.Equal((long)direct.HeapSizeBytes, runtimeInfo.HeapSizeBytes,
            "GC.Info: GCMemoryInfo.HeapSizeBytes must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.FragmentedBytes, runtimeInfo.FragmentedBytes,
            "GC.Info: GCMemoryInfo.FragmentedBytes must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.TotalCommittedBytes, runtimeInfo.TotalCommittedBytes,
            "GC.Info: GCMemoryInfo.TotalCommittedBytes must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.MemoryLoadBytes, runtimeInfo.MemoryLoadBytes,
            "GC.Info: GCMemoryInfo.MemoryLoadBytes must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.PromotedBytes, runtimeInfo.PromotedBytes,
            "GC.Info: GCMemoryInfo.PromotedBytes must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.PinnedObjectsCount, runtimeInfo.PinnedObjectsCount,
            "GC.Info: GCMemoryInfo.PinnedObjectsCount must mirror GetLastGCMemoryInfo");
        Assert.Equal((long)direct.CollectionIndex, runtimeInfo.Index,
            "GC.Info: GCMemoryInfo.Index must equal CollectionIndex");
        Assert.Equal(direct.CondemnedGeneration, runtimeInfo.Generation,
            "GC.Info: GCMemoryInfo.Generation must equal CondemnedGeneration");

        // RamSize is the total available memory; the high-load threshold is half of it.
        Assert.Equal((long)PageAllocator.RamSize, runtimeInfo.TotalAvailableMemoryBytes,
            "GC.Info: TotalAvailableMemoryBytes must equal PageAllocator.RamSize");
        Assert.Equal((long)(PageAllocator.RamSize / 2), runtimeInfo.HighMemoryLoadThresholdBytes,
            "GC.Info: HighMemoryLoadThresholdBytes must equal RamSize / 2");

        // Generation 0 last-collect snapshot must mirror CoreGC.GetLastGen* getters.
        Assert.Equal((long)CoreGC.GetLastGenSizeBefore(0), runtimeInfo.GenerationInfo[0].SizeBeforeBytes,
            "GC.Info: GenerationInfo[0].SizeBeforeBytes must mirror GetLastGenSizeBefore(0)");
        Assert.Equal((long)CoreGC.GetLastGenSizeAfter(0), runtimeInfo.GenerationInfo[0].SizeAfterBytes,
            "GC.Info: GenerationInfo[0].SizeAfterBytes must mirror GetLastGenSizeAfter(0)");
        Assert.Equal((long)CoreGC.GetLastGenFragmentationBefore(0), runtimeInfo.GenerationInfo[0].FragmentationBeforeBytes,
            "GC.Info: GenerationInfo[0].FragmentationBeforeBytes must mirror GetLastGenFragmentationBefore(0)");
        Assert.Equal((long)CoreGC.GetLastGenFragmentationAfter(0), runtimeInfo.GenerationInfo[0].FragmentationAfterBytes,
            "GC.Info: GenerationInfo[0].FragmentationAfterBytes must mirror GetLastGenFragmentationAfter(0)");
    }

    private static void TestGCInfoMemoryInfoFrozenBetweenCollections()
    {
        // GCMemoryInfo describes the heap as of the last collection, so allocating must not
        // move any of its values: only the next collection may. Regression guard for the
        // live readings that used to be reported through RhGetMemoryInfo.
        CoreGC.Collect();

        GCMemoryInfo before = GC.GetGCMemoryInfo();

        // This changes the live heap: it consumes free-list blocks to refill TLABs, which is
        // exactly what used to shift FragmentedBytes between two reads.
        AllocateGarbage(40, 256);

        GCMemoryInfo after = GC.GetGCMemoryInfo();

        // A heap-pressure collection may have run on its own during the allocations above;
        // the freeze only has to hold as long as no collection happened in between.
        if (before.Index == after.Index)
        {
            Assert.Equal(before.FragmentedBytes, after.FragmentedBytes,
                "GC.Info: FragmentedBytes must not change without a collection");
            Assert.Equal(before.HeapSizeBytes, after.HeapSizeBytes,
                "GC.Info: HeapSizeBytes must not change without a collection");
            Assert.Equal(before.TotalCommittedBytes, after.TotalCommittedBytes,
                "GC.Info: TotalCommittedBytes must not change without a collection");
            Assert.Equal(before.MemoryLoadBytes, after.MemoryLoadBytes,
                "GC.Info: MemoryLoadBytes must not change without a collection");
        }

        // This GC only has generation 0, so the heap-wide fragmentation is by construction
        // the fragmentation gen0 was left with by the last collection.
        Assert.Equal(after.GenerationInfo[0].FragmentationAfterBytes, after.FragmentedBytes,
            "GC.Info: FragmentedBytes must equal GenerationInfo[0].FragmentationAfterBytes");

        // A collection is what publishes new values.
        CoreGC.Collect();

        GCMemoryInfo collected = GC.GetGCMemoryInfo();
        Assert.True(collected.Index > after.Index,
            "GC.Info: Index must advance after a collection");
    }

    private static void TestGCVariables()
    {
        IReadOnlyDictionary<string, object> vars = CoreGC.Variables;
        Assert.NotNull(vars, "GC.Info: Variables must be non-null after GC initialization");

        // Standalone GC identification.
        Assert.Equal("OrionGC", (string)vars["GCName"],
            "GC.Info: GCName must be \"OrionGC\"");
        Assert.Equal("", (string)vars["GCPath"],
            "GC.Info: GCPath must be empty string");

        // All boolean GC mode flags default to false in this kernel.
        Assert.Equal(false, (bool)vars["gcServer"],
            "GC.Info: gcServer must be false");
        Assert.Equal(false, (bool)vars["gcConcurrent"],
            "GC.Info: gcConcurrent must be false");
        Assert.Equal(false, (bool)vars["GCRetainVM"],
            "GC.Info: GCRetainVM must be false");
        Assert.Equal(false, (bool)vars["GCNoAffinitize"],
            "GC.Info: GCNoAffinitize must be false");
        Assert.Equal(false, (bool)vars["GCCpuGroup"],
            "GC.Info: GCCpuGroup must be false");
        Assert.Equal(false, (bool)vars["GCLargePages"],
            "GC.Info: GCLargePages must be false");
    }

    // ==================== TLAB (Thread-Local Allocation Buffer) Tests ====================

    private static void TestTlabAllocBytesNonZero()
    {
        // After all previous tests, per-thread cumulative allocated bytes must be > 0
        long threadBytes = GC.GetAllocatedBytesForCurrentThread();
        Assert.True(threadBytes > 0,
            "TLAB: GetAllocatedBytesForCurrentThread must be > 0 after allocations, got: " + threadBytes);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[] AllocByteArray(int size)
    {
        return new byte[size];
    }

    private static void TestTlabAllocBytesIncrease()
    {
        // Allocating objects must increase the global total allocated bytes counter
        long before = GC.GetTotalAllocatedBytes(precise: false);

        // Allocate via NoInlining helper to prevent compiler elision
        byte[] a1 = AllocByteArray(128);
        byte[] a2 = AllocByteArray(256);
        byte[] a3 = AllocByteArray(512);

        long after = GC.GetTotalAllocatedBytes(precise: false);

        // Keep references alive so they're not optimized away
        Assert.True(a1 != null && a2 != null && a3 != null, "TLAB: allocations must not be null");
        Assert.True(after > before,
            "TLAB: GetTotalAllocatedBytes must increase after allocations");
    }

    private static void TestTlabPreciseLessOrEqualTotal()
    {
        // Precise total subtracts unused TLAB space; must be <= non-precise total
        long total = GC.GetTotalAllocatedBytes(precise: false);
        long precise = GC.GetTotalAllocatedBytes(precise: true);
        Assert.True(total > 0, "TLAB: GetTotalAllocatedBytes must be > 0");
        Assert.True(precise > 0, "TLAB: GetTotalAllocatedBytes(precise) must be > 0");
        Assert.True(precise <= total,
            "TLAB: precise (" + precise + ") must be <= non-precise (" + total + ")");
    }

    private static void TestTlabSurvivalAfterCollect()
    {
        // Allocate objects, trigger GC (which returns all TLABs), then verify
        // objects survive and we can allocate again (TLAB refill works)
        List<int> list = new List<int>();
        for (int i = 0; i < 100; i++)
        {
            list.Add(i);
        }

        CoreGC.Collect();

        // List must survive
        Assert.Equal(100, list.Count, "TLAB: list count must survive GC");
        Assert.Equal(99, list[99], "TLAB: list last element must survive GC");

        // Post-GC allocation must work (TLAB refill after ReturnAllAllocContexts)
        byte[] postGC = new byte[256];
        postGC[0] = 0xAB;
        Assert.Equal((byte)0xAB, postGC[0], "TLAB: allocation after GC must work");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void AllocateTlabGarbage(int count)
    {
        for (int i = 0; i < count; i++)
        {
            byte[] temp = new byte[64];
            if (temp == null)
            {
                break;
            }
        }
    }

    private static void TestTlabGapStampedOnCollect()
    {
        // After GC, unused TLAB gaps are stamped as FreeBlocks.
        // Verify that a collection doesn't corrupt the heap by allocating
        // garbage, collecting, then allocating more and verifying correctness.
        AllocateTlabGarbage(100);

        CoreGC.GetStats(out int collsBefore, out int _);
        int freed = CoreGC.Collect();
        CoreGC.GetStats(out int collsAfter, out int _2);

        Assert.Equal(collsBefore + 1, collsAfter,
            "TLAB: exactly 1 collection recorded");
        Assert.True(freed >= 0,
            "TLAB: freed count must be >= 0");

        // Allocate after collect to verify heap integrity (TLAB gaps properly handled)
        int[] arr = new int[50];
        for (int i = 0; i < 50; i++)
        {
            arr[i] = i * 3;
        }

        Assert.Equal(147, arr[49], "TLAB: post-collect allocation data integrity");
    }
}

// Test struct for boxing and collection tests
internal struct TestPoint
{
    public int X;
    public int Y;
}
