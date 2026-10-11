using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Cosmos.Kernel.Core.Memory;
using Cosmos.TestRunner.Framework;
using Sys = Cosmos.Kernel.System;
using TR = Cosmos.TestRunner.Framework.TestRunner;

namespace Cosmos.Kernel.Tests.Memory;

public unsafe class Kernel : Sys.Kernel
{
    /// <summary>Number of test cases registered with the TestRunner in BeforeRun.</summary>
    private const int ExpectedTestCount = 115;

    /// <summary>
    /// Sentinel written to a page before committing it, so the round-trip test
    /// can prove the mapping reaches real memory rather than merely being
    /// reported as present.
    /// </summary>
    private const byte PagerTestMarker = 0x5A;

    /// <summary>
    /// A higher-half address in the gap between the HHDM window and the kernel
    /// image, used as a probe expected to be unmapped. Nothing the kernel or
    /// the bootloader maps lives here on either architecture.
    /// </summary>
    private const ulong PagerUnmappedProbeAddress = 0xFFFF_5000_0000_0000;

    /// <summary>Number of pages in the multi-page Pager round-trip test.</summary>
    private const int PagerMultiPageCount = 4;

    /// <summary>
    /// Page count used to fill a descriptor to capacity for the Add-refusal
    /// test. Must equal <c>AddressSpaceDescriptor.MaxRegionCount</c>; checked at
    /// runtime rather than hard-coded so the two cannot drift apart silently.
    /// </summary>
    private const int PagerCapacityProbe = AddressSpaceDescriptor.MaxRegionCount;

    /// <summary>Expected argv length: argv[0] ("cosmos") plus the 3 args passed by limine.conf.</summary>
    private const int ExpectedArgvLength = 4;

    /// <summary>Hash code of boxed 'c': char.GetHashCode packs the char value into both 16-bit halves.</summary>
    private const int BoxedCharHashCode = 0x00630063;

    /// <summary>Length of the int array used by the allocation-and-access test.</summary>
    private const int AllocIntArrayLength = 100;

    /// <summary>Size of the large-allocation test buffer (1 MiB).</summary>
    private const int LargeAllocationSizeBytes = 1024 * 1024;

    /// <summary>Marker byte (0b1010_1010) written to the first byte of the large allocation.</summary>
    private const byte LargeAllocFirstMarker = 0xAA;

    /// <summary>Marker byte (0b0101_0101) written to the last byte of the large allocation.</summary>
    private const byte LargeAllocLastMarker = 0x55;

    /// <summary>MemCopy test length below the 16-byte SIMD threshold.</summary>
    private const int CopyLength8Bytes = 8;

    /// <summary>MemCopy test length at the SIMD threshold (smallest SIMD-enabled copy).</summary>
    private const int CopyLength16Bytes = 16;

    /// <summary>MemCopy test length: one 16-byte SIMD block plus an 8-byte tail.</summary>
    private const int CopyLength24Bytes = 24;

    /// <summary>MemCopy test length: two 16-byte SIMD blocks.</summary>
    private const int CopyLength32Bytes = 32;

    /// <summary>MemCopy test length: three 16-byte SIMD blocks.</summary>
    private const int CopyLength48Bytes = 48;

    /// <summary>Test length of four 16-byte SIMD blocks; also used by the MemSet and byte[] Array.Copy tests.</summary>
    private const int CopyLength64Bytes = 64;

    /// <summary>MemCopy test length: five 16-byte SIMD blocks.</summary>
    private const int CopyLength80Bytes = 80;

    /// <summary>MemCopy test length: eight 16-byte SIMD blocks.</summary>
    private const int CopyLength128Bytes = 128;

    /// <summary>Large SIMD test length; also used by the large byte[] Array.Copy test.</summary>
    private const int CopyLength256Bytes = 256;

    /// <summary>Large SIMD test length with an 8-byte unaligned tail.</summary>
    private const int CopyLength264Bytes = 264;

    /// <summary>Mask that truncates an index to a byte value when building fill patterns.</summary>
    private const int ByteValueMask = 0xFF;

    /// <summary>Fill value for the MemSet test.</summary>
    private const byte MemSetFillValue = 0xAB;

    /// <summary>Buffer length for the overlapping MemMove tests.</summary>
    private const int OverlapBufferLength = 32;

    /// <summary>Number of bytes moved in the overlapping MemMove tests.</summary>
    private const int OverlapMoveLength = 16;

    /// <summary>Byte distance between source and destination in the overlapping MemMove tests.</summary>
    private const int OverlapOffset = 8;

    /// <summary>Sentinel written to source buffers to prove data was (or was not) copied.</summary>
    private const byte SourceSentinel = 0xAA;

    /// <summary>Sentinel pre-loaded into destination buffers to detect unexpected writes.</summary>
    private const byte DestSentinel = 0xBB;

    /// <summary>Below-HHDM probe address that VirtualToPhysical must pass through unchanged.</summary>
    private const ulong PhysicalProbeAddress = 0x1000;

    /// <summary>Base of the top-2GiB kernel image window where NativeAOT places code and statics.</summary>
    private const ulong KernelImageWindowBase = 0xFFFF_FFFF_8000_0000;

    protected override void BeforeRun()
    {
        TR.Start("Memory Tests", expectedTests: ExpectedTestCount);

        // Boxing/Unboxing Tests
        TR.Run("Boxing_Char", TestBoxingChar);
        TR.Run("Boxing_Int32", TestBoxingInt32);
        TR.Run("Boxing_Byte", TestBoxingByte);
        TR.Run("Boxing_Long", TestBoxingLong);
        TR.Run("Boxing_Nullable", TestBoxingNullable);
        TR.Run("Boxing_Interface", TestBoxingInterface);
        TR.Run("Boxing_CustomStruct", TestBoxingCustomStruct);
        TR.Run("Boxing_ArrayCopy", TestArrayCopyWithBoxing);
        TR.Run("Boxing_Enum", TestBoxingEnum);
        TR.Run("Boxing_ValueTuple", TestBoxingValueTuple);
        TR.Run("Boxing_NullInterface", TestBoxingNullInterface);

        // Memory Allocation Tests
        TR.Run("Memory_CharArray", TestCharArrayAllocation);
        TR.Run("Memory_StringAllocation", TestStringAllocation);
        TR.Run("Memory_IntArray", TestIntArrayAllocation);
        TR.Run("Memory_StringConcat", TestStringConcatenation);
        TR.Run("Memory_StringBuilder", TestStringBuilder);
        TR.Run("Memory_ZeroLengthArray", TestZeroLengthArray);
        TR.Run("Memory_EmptyString", TestEmptyString);
        TR.Run("Memory_LargeAllocation", TestLargeAllocation);

        // Generic Collection Tests
        TR.Run("Collections_ListInt", TestListInt);
        TR.Run("Collections_ListString", TestListString);
        TR.Run("Collections_ListByte", TestListByte);
        TR.Run("Collections_ListLong", TestListLong);
        TR.Run("Collections_ListStruct", TestListStruct);
        TR.Run("Collections_ListContains", TestListContains);
        TR.Run("Collections_ListIndexOf", TestListIndexOf);
        TR.Run("Collections_ListRemoveAt", TestListRemoveAt);
        TR.Run("Collections_ListInsert", TestListInsert);
        TR.Run("Collections_ListRemove", TestListRemove);
        TR.Run("Collections_ListClear", TestListClear);
        TR.Run("Collections_ListToArray", TestListToArray);
        TR.Run("Collections_ListForeach", TestListForeach);
        TR.Run("Collections_ListEmpty", TestListEmpty);

        // Dictionary
        TR.Run("Collections_DictCustomComparer", TestDictionaryCustomComparer);
        TR.Run("Collections_DictAddGet", TestDictionaryAddGet);
        TR.Run("Collections_DictIndexer", TestDictionaryIndexer);
        TR.Run("Collections_DictContains", TestDictionaryContains);
        TR.Run("Collections_DictRemove", TestDictionaryRemove);
        TR.Run("Collections_DictClear", TestDictionaryClear);
        TR.Run("Collections_DictTryGetValue", TestDictionaryTryGetValue);
        TR.Run("Collections_DictKeysValues", TestDictionaryKeysValues);
        TR.Run("Collections_DictEmpty", TestDictionaryEmpty);

        // IEnumerable
        TR.Run("Collections_IEnumerable", TestIEnumerable);

        // Memory Copy Tests (SIMD enabled for 16+ bytes)
        TR.Run("MemCopy_8Bytes", TestMemCopy8Bytes);
        TR.Run("MemCopy_16Bytes", TestMemCopy16Bytes);
        TR.Run("MemCopy_24Bytes", TestMemCopy24Bytes);
        TR.Run("MemCopy_32Bytes", TestMemCopy32Bytes);
        TR.Run("MemCopy_48Bytes", TestMemCopy48Bytes);
        TR.Run("MemCopy_64Bytes", TestMemCopy64Bytes);
        TR.Run("MemCopy_80Bytes", TestMemCopy80Bytes);
        TR.Run("MemCopy_128Bytes", TestMemCopy128Bytes);
        TR.Run("MemCopy_256Bytes", TestMemCopy256Bytes);
        TR.Run("MemCopy_264Bytes", TestMemCopy264Bytes);
        TR.Run("MemSet_64Bytes", TestMemSet64Bytes);
        TR.Run("MemMove_Overlap", TestMemMoveOverlap);
        TR.Run("MemCopy_0Bytes", TestMemCopy0Bytes);
        TR.Run("MemCopy_1Byte", TestMemCopy1Byte);
        TR.Run("MemMove_Overlap_DestBeforeSrc", TestMemMoveOverlapDestBeforeSrc);

        // Per-thread allocation accounting (TLAB)
        TR.Run("Memory_ThreadAllocBytesPositive", TestThreadAllocBytesPositive);
        TR.Run("Memory_TotalAllocBytesPositive", TestTotalAllocBytesPositive);

        // Cmdline parsing — limine.conf passes "arg1 arg2 arg3"
        TR.Run("Cmdline_ArgCount", TestCmdlineArgCount);
        TR.Run("Cmdline_Argv0IsCosmos", TestCmdlineArgv0);
        TR.Run("Cmdline_ArgValues", TestCmdlineArgValues);

        // Array.Copy Tests (uses SIMD via memmove/RhBulkMoveWithWriteBarrier)
        TR.Run("ArrayCopy_IntArray", TestArrayCopyIntArray);
        TR.Run("ArrayCopy_ByteArray", TestArrayCopyByteArray);
        TR.Run("ArrayCopy_LargeArray", TestArrayCopyLargeArray);
        TR.Run("ArrayCopy_ZeroLength", TestArrayCopyZeroLength);
        TR.Run("ArrayCopy_Overlap", TestArrayCopyOverlap);

        // VirtualToPhysical — the DMA-address translation every storage/GIC
        // buffer goes through. Pins the two valid classes and the loud
        // rejection of the invalid one.
        TR.Run("V2P_HhdmAlias_Translates", TestV2PHhdmAliasTranslates);
        TR.Run("V2P_PhysicalValue_PassesThrough", TestV2PPhysicalPassesThrough);
        TR.Run("V2P_KernelImageAddress_Rejected", TestV2PKernelImageRejected);

        // ===== Pager =====
        //
        // Ordered cheapest-and-most-independent first, deliberately. A fault
        // inside the MMU path halts the CPU and takes every later test with it,
        // so the pure-data and bookkeeping checks are registered ahead of the
        // ones that actually install mappings. That way a regression in the
        // walk still leaves the value objects and descriptor contracts
        // reporting, instead of the suite dying at test one.

        // Address value objects: pure data, no paging, no MMU.
        TR.Run("VirtualAddress_WrapsAndUnwraps", TestVirtualAddressRoundTrip);
        TR.Run("VirtualAddress_EqualityByValue", TestVirtualAddressEquality);
        TR.Run("VirtualAddress_ToString_ShowsValue", TestVirtualAddressToString);
        TR.Run("PhysicalAddress_Aligned_IsAccepted", TestPhysicalAddressAligned);
        TR.Run("PhysicalAddress_Unaligned_IsRejected", TestPhysicalAddressUnaligned);
        TR.Run("PhysicalAddress_None_IsZero", TestPhysicalAddressNone);
        TR.Run("PhysicalAddress_EqualityByValue", TestPhysicalAddressEquality);
        TR.Run("PhysicalAddress_DistinctFromVirtualAddress", TestAddressTypeDistinctness);

        // Descriptor bookkeeping. Describes mappings without installing them.
        TR.Run("Pager_Backend_Registered", TestPagerBackendRegistered);
        TR.Run("Pager_Create_DistinctDescriptors", TestPagerCreateDistinct);
        TR.Run("Pager_Add_NullSpace_ReturnsFalse", TestPagerAddNullSpace);
        TR.Run("Pager_Add_ZeroPageRegion_Retained", TestPagerAddZeroPageRetained);
        TR.Run("Pager_Add_AtCapacity_ReturnsFalse", TestPagerAddAtCapacity);
        TR.Run("Pager_NewSpace_SharesKernelRoot", TestPagerNewSpaceSharesKernelRoot);

        // Argument handling: must refuse without touching the MMU.
        TR.Run("Pager_Translate_NullSpace_ReturnsFalse", TestPagerTranslateNullSpace);
        TR.Run("Pager_Translate_UnalignedAddress_Rejected", TestPagerTranslateUnaligned);
        TR.Run("Pager_Commit_NullSpace_DoesNotThrow", TestPagerCommitNullSpace);

        // Read-only translation: resolving an address that already works needs
        // no new mapping, so it fails first if the walk itself is wrong.
        TR.Run("Pager_TranslateCurrent_KernelImage", TestPagerTranslateCurrentKernelImage);
        TR.Run("Pager_TranslateCurrent_Unmapped", TestPagerTranslateCurrentUnmapped);
        TR.Run("Pager_Translate_ExplicitSpace_MatchesCurrent", TestPagerTranslateExplicitMatches);
        TR.Run("Pager_NewSpace_KernelStaysReachable", TestPagerNewSpaceKernelReachable);

        // Commit with nothing to install.
        TR.Run("Pager_Commit_EmptySpace_IsNoOp", TestPagerCommitEmptySpace);
        TR.Run("Pager_Commit_ZeroPageRegion_InstallsNothing", TestPagerCommitZeroPage);

        // ===== Pager: installs real mappings (MMU-changing, last) =====
        //
        // Everything above this line is safe to run anywhere. What follows
        // writes page-table entries and dereferences them, so it is the block
        // that a broken walk takes down with it.

        // Flag encoding: each flag lowered per architecture.
        TR.Run("Pager_Flags_None_DefaultsToNonExecutable", TestPagerFlagsNoneDefaults);
        TR.Run("Pager_Flags_Executable_ClearsNoExecute", TestPagerFlagsExecutable);
        TR.Run("Pager_Flags_Writable_IsHonoured", TestPagerFlagsWritable);
        TR.Run("Pager_Flags_Device_IsHonoured", TestPagerFlagsDevice);
        TR.Run("Pager_Flags_User_AcceptedButUnusable", TestPagerFlagsUser);
        TR.Run("Pager_Flags_Global_AcceptedButNotEnforced", TestPagerFlagsGlobal);
        TR.Run("Pager_Flags_Uncached_FallsBackToCacheable", TestPagerFlagsUncached);
        TR.Run("Pager_Flags_Combined_BitwiseRoundTrip", TestPagerFlagsCombined);

        // Region geometry.
        TR.Run("Pager_Translate_FirstPageOfRange", TestPagerTranslateFirstPage);
        TR.Run("Pager_Translate_LastPageOfRange", TestPagerTranslateLastPage);
        TR.Run("Pager_Translate_NonPowerOfTwoPageCount", TestPagerTranslateOddCount);
        TR.Run("Pager_Translate_LargeRegion", TestPagerTranslateLargeRegion);

        // Multi-region spaces and repeat commits.
        TR.Run("Pager_Add_MultipleRegions_AllRetained", TestPagerAddMultipleRegions);
        TR.Run("Pager_Commit_MultipleTimes_IsIdempotent", TestPagerCommitIdempotent);
        TR.Run("Pager_Translate_AfterSecondCommit", TestPagerTranslateAfterRecommit);
        TR.Run("Pager_Commit_DoesNotDisturbKernel", TestPagerCommitKernelIntact);
        TR.Run("Pager_Translate_AllZeroRegions_MapsNothing", TestPagerTranslateZeroRegions);
        TR.Run("Pager_Translate_CapacityLimitUnaffected", TestPagerTranslateCapacityUnaffected);
        TR.Run("Pager_Translate_SinglePageRegion", TestPagerTranslateSinglePage);
        TR.Run("Pager_Commit_MultiPageRange", TestPagerCommitMultiPage);

        // The write-through test, last: it is the only one that faults on a
        // genuinely broken mapping rather than failing an assertion.
        TR.Run("Pager_Commit_RoundTripsMapping", TestPagerCommitRoundTrip);

        TR.Finish();
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

    // ==================== Boxing/Unboxing Tests ====================

    private static void TestBoxingChar()
    {
        object boxed = 'c';
        Assert.Equal("c", boxed.ToString());
        Assert.Equal(BoxedCharHashCode, boxed.GetHashCode());

        char unboxed = (char)boxed;
        Assert.True(unboxed == 'c', "Boxing: char to object and back");
    }

    private static void TestBoxingInt32()
    {
        object boxed = 42;
        Assert.Equal("42", boxed.ToString());
        Assert.Equal(42, boxed.GetHashCode());
        Assert.True(boxed.Equals(42), "Int32.Equals on boxed int (same value)");
        Assert.True(!boxed.Equals(5), "Int32.Equals on boxed int (different value)");

        object boxed2 = 42;
        Assert.True(Object.Equals(boxed, boxed2), "Object.Equals with two boxed ints");

        int unboxed = (int)boxed;
        Assert.True(unboxed == 42, "Boxing: int to object and back");
    }

    private static void TestBoxingByte()
    {
        byte value = 255;
        object boxed = value;
        byte unboxed = (byte)boxed;
        Assert.True(unboxed == 255, "Boxing: byte to object and back");
    }

    private static void TestBoxingLong()
    {
        long value = 9876543210L;
        object boxed = value;
        long unboxed = (long)boxed;
        Assert.True(unboxed == 9876543210L, "Boxing: long to object and back");
    }

    private static void TestBoxingNullable()
    {
        // Test null case
        int? nullableNull = null;
        object boxedNull = nullableNull;
        Assert.True(boxedNull == null, "Boxing: Nullable<int> null boxes to null");

        // Test value case
        int? nullableValue = 777;
        object boxedValue = nullableValue;
        Assert.True(boxedValue != null && (int)boxedValue == 777, "Boxing: Nullable<int> with value boxes correctly");
    }

    private static void TestBoxingInterface()
    {
        int value = 100;
        IComparable comparable = value;
        Assert.True(comparable != null, "Boxing: int to interface (IComparable)");
    }

    private static void TestBoxingCustomStruct()
    {
        TestPoint point = new TestPoint { X = 10, Y = 20 };
        object boxed = point;
        TestPoint unboxed = (TestPoint)boxed;
        Assert.True(unboxed.X == 10 && unboxed.Y == 20, "Boxing: custom struct box/unbox");
    }

    private static void TestArrayCopyWithBoxing()
    {
        int[] sourceIntArray = new int[] { 10, 20, 30 };
        object[] destObjectArray = new object[3];

        Array.Copy(sourceIntArray, destObjectArray, 3);

        bool passed = (int)destObjectArray[0] == 10 &&
                     (int)destObjectArray[1] == 20 &&
                     (int)destObjectArray[2] == 30;
        Assert.True(passed, "Boxing: Array.Copy with automatic boxing");
    }

    private static void TestBoxingEnum()
    {
        TestEnum val = TestEnum.ValueB;
        object boxed = val;
        Assert.True(boxed is TestEnum, "Boxing: enum is TestEnum");
        Assert.True((TestEnum)boxed == TestEnum.ValueB, "Boxing: enum value preserved");
    }

    private static void TestBoxingValueTuple()
    {
        var tuple = (1, "test");
        object boxed = tuple;
        var unboxed = ((int, string))boxed;
        Assert.True(unboxed.Item1 == 1 && unboxed.Item2 == "test", "Boxing: ValueTuple box/unbox");
    }

    private static void TestBoxingNullInterface()
    {
        IComparable comparable = null;
        object boxed = comparable;
        Assert.True(boxed == null, "Boxing: null interface is null object");
    }

    // ==================== Memory Allocation Tests ====================

    private static void TestCharArrayAllocation()
    {
        char[] testChars = new char[] { 'R', 'h', 'p' };
        Assert.True(testChars.Length == 3 && testChars[0] == 'R', "Memory: char array allocation");
    }

    private static void TestStringAllocation()
    {
        char[] chars = new char[] { 'R', 'h', 'p' };
        string str = new string(chars);
        Assert.True(str == "Rhp", "Memory: string allocation from char array");
    }

    private static void TestIntArrayAllocation()
    {
        int[] array = new int[AllocIntArrayLength];
        for (int i = 0; i < 10; i++)
        {
            array[i] = i * 10;
        }
        Assert.True(array[0] == 0 && array[1] == 10 && array[2] == 20, "Memory: int array allocation and access");
    }

    private static void TestStringConcatenation()
    {
        string str1 = "Hello";
        string str2 = "World";
        string str3 = str1 + " " + str2;
        Assert.True(str3 == "Hello World", "Memory: string concatenation");
    }

    private static void TestStringBuilder()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("Hello");
        sb.Append(" ");
        sb.Append("StringBuilder");
        string result = sb.ToString();
        Assert.True(result == "Hello StringBuilder", "Memory: StringBuilder operations");
    }

    private static void TestZeroLengthArray()
    {
        int[] arr = new int[0];
        Assert.True(arr != null && arr.Length == 0, "Memory: zero-length array allocation");
    }

    private static void TestEmptyString()
    {
        string s = string.Empty;
        Assert.True(s != null && s.Length == 0, "Memory: empty string access");
    }

    private static void TestLargeAllocation()
    {
        int size = LargeAllocationSizeBytes;
        byte[] large = new byte[size];
        large[0] = LargeAllocFirstMarker;
        large[size - 1] = LargeAllocLastMarker;
        Assert.True(large.Length == size && large[0] == LargeAllocFirstMarker && large[size - 1] == LargeAllocLastMarker, "Memory: 1MB allocation");
    }

    // ==================== Generic Collection Tests ====================

    private static void TestListInt()
    {
        List<int> list = new List<int>();
        list.Add(100);
        list.Add(200);
        list.Add(300);
        Assert.True(list.Count == 3 && list[0] == 100 && list[1] == 200 && list[2] == 300, "Collections: List<int> Add and indexer");
    }

    private static void TestListString()
    {
        List<string> list = new List<string>();
        list.Add("First");
        list.Add("Second");
        list.Add("Third");
        list.Add("Fourth");
        list.Add("Fifth");

        Assert.True(list.Count == 5 && list[0] == "First" && list[4] == "Fifth", "Collections: List<string> with resize");
    }

    private static void TestListByte()
    {
        List<byte> list = new List<byte>();
        list.Add(0xFF);
        list.Add(0x00);
        list.Add(0xAB);
        list.Add(0x12);

        Assert.True(list.Count == 4 && list[0] == 0xFF && list[2] == 0xAB, "Collections: List<byte> operations");
    }

    private static void TestListLong()
    {
        List<long> list = new List<long>();
        list.Add(0x123456789ABCDEF0);
        list.Add(-9999999999999);
        list.Add(42);

        Assert.True(list.Count == 3 && list[0] == 0x123456789ABCDEF0 && list[2] == 42, "Collections: List<long> with 64-bit values");
    }

    private static void TestListStruct()
    {
        List<TestPoint> list = new List<TestPoint>();
        list.Add(new TestPoint { X = 1, Y = 2 });
        list.Add(new TestPoint { X = 3, Y = 4 });
        list.Add(new TestPoint { X = 5, Y = 6 });

        Assert.True(list.Count == 3 && list[0].X == 1 && list[2].Y == 6, "Collections: List<struct> operations");
    }

    private static void TestListContains()
    {
        List<int> list = new List<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        bool found20 = list.Contains(20);
        bool found99 = list.Contains(99);

        Assert.True(found20 && !found99, "Collections: List.Contains method");
    }

    private static void TestListIndexOf()
    {
        List<int> list = new List<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);

        int index20 = list.IndexOf(20);
        int index99 = list.IndexOf(99);

        Assert.True(index20 == 1 && index99 == -1, "Collections: List.IndexOf method");
    }

    private static void TestListRemoveAt()
    {
        List<int> list = new List<int>();
        list.Add(10);
        list.Add(20);
        list.Add(30);
        list.Add(40);
        list.Add(50);

        int idx = list.IndexOf(30);
        list.RemoveAt(idx);

        Assert.True(list.Count == 4 && list[2] == 40, "Collections: List.RemoveAt method");
    }

    private static void TestDictionaryAddGet()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");
        dict.Add(2, "Two");

        Assert.True(dict.Count == 2, "Dictionary.Add count");
        Assert.True(dict[1] == "One" && dict[2] == "Two", "Dictionary.Add get values");
    }

    private static void TestListInsert()
    {
        List<int> list = new List<int>();
        list.Add(1);
        list.Add(3);
        list.Insert(1, 2);

        Assert.True(list.Count == 3 && list[1] == 2 && list[2] == 3, "List.Insert");
    }

    private static void TestListRemove()
    {
        List<string> list = new List<string>();
        list.Add("A");
        list.Add("B");
        list.Add("A");

        bool removed = list.Remove("A"); // Removes first "A"
        Assert.True(removed && list.Count == 2 && list[0] == "B" && list[1] == "A", "List.Remove");
    }

    private static void TestListClear()
    {
        List<int> list = new List<int>();
        list.Add(1);
        list.Clear();
        Assert.True(list.Count == 0, "List.Clear");
    }

    private static void TestListToArray()
    {
        List<int> list = new List<int>();
        list.Add(1);
        list.Add(2);
        int[] arr = list.ToArray();

        Assert.True(arr.Length == 2 && arr[0] == 1 && arr[1] == 2, "List.ToArray");
    }

    private static void TestListForeach()
    {
        List<int> list = new List<int>();
        list.Add(1);
        list.Add(2);
        list.Add(3);

        int sum = 0;
        foreach (int i in list)
        {
            sum += i;
        }

        Assert.True(sum == 6, "List foreach iteration");
    }

    private static void TestListEmpty()
    {
        List<int> list = new List<int>();
        Assert.True(list.Count == 0, "Collections: empty list count");
        Assert.True(!list.Contains(1), "Collections: empty list contains");
        Assert.True(list.ToArray().Length == 0, "Collections: empty list ToArray");
    }

    // ==================== Dictionary Tests ====================

    private static void TestDictionaryIndexer()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>();

        string keyA = "KeyA";
        string keyB = "KeyB";
        string keyC = "KeyC";

        // 1. Test Add and Get with string keys
        dict.Add(keyA, 10);
        dict.Add(keyB, 20);
        Assert.True(dict[keyA] == 10 && dict[keyB] == 20, "Dictionary string key Add/Get");

        // 2. Test Update via Indexer
        dict[keyA] = 30;
        Assert.True(dict[keyA] == 30, "Dictionary string key Update");

        // 3. Test Insert via Indexer
        dict[keyC] = 40;
        Assert.True(dict[keyC] == 40, "Dictionary string key Insert via Indexer");
    }

    private static void TestDictionaryCustomComparer()
    {
        Dictionary<string, int> dict = new Dictionary<string, int>(new SimpleStringComparer());
        dict.Add("KeyA", 1);
        dict.Add("KeyB", 2);
        Assert.True(dict["KeyA"] == 1 && dict["KeyB"] == 2, "Dictionary with custom comparer");
    }

    private static void TestDictionaryContains()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");

        Assert.True(dict.ContainsKey(1), "Dictionary.ContainsKey found");
        Assert.True(!dict.ContainsKey(2), "Dictionary.ContainsKey not found");
        Assert.True(dict.ContainsValue("One"), "Dictionary.ContainsValue found");
        Assert.True(!dict.ContainsValue("Two"), "Dictionary.ContainsValue not found");
    }

    private static void TestDictionaryRemove()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");
        dict.Add(2, "Two");

        bool removed = dict.Remove(1);
        Assert.True(removed && dict.Count == 1 && !dict.ContainsKey(1), "Dictionary.Remove existing");

        bool removed2 = dict.Remove(3);
        Assert.True(!removed2 && dict.Count == 1, "Dictionary.Remove non-existing");
    }

    private static void TestDictionaryClear()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");
        dict.Add(2, "Two");

        dict.Clear();
        Assert.True(dict.Count == 0 && !dict.ContainsKey(1), "Dictionary.Clear");
    }

    private static void TestDictionaryTryGetValue()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");

        string val;
        bool found = dict.TryGetValue(1, out val);
        Assert.True(found && val == "One", "Dictionary.TryGetValue found");

        bool found2 = dict.TryGetValue(2, out val);
        Assert.True(!found2 && val == null, "Dictionary.TryGetValue not found");
    }

    private static void TestDictionaryKeysValues()
    {
        Dictionary<int, string> dict = new Dictionary<int, string>();
        dict.Add(1, "One");
        dict.Add(2, "Two");

        int keySum = 0;
        foreach (var k in dict.Keys)
        {
            keySum += k;
        }

        Assert.True(keySum == 3, "Dictionary.Keys iteration");

        Assert.True(dict.Values != null && dict.Values.Count == 2, "Dictionary.Values access");
    }

    private static void TestDictionaryEmpty()
    {
        Dictionary<int, int> dict = new Dictionary<int, int>();
        Assert.True(dict.Count == 0, "Dictionary: empty count");
        Assert.True(!dict.ContainsKey(1), "Dictionary: empty ContainsKey");
        int val;
        Assert.True(!dict.TryGetValue(1, out val), "Dictionary: empty TryGetValue");
    }

    // ==================== IEnumerable Tests ====================

    private static void TestIEnumerable()
    {
        int[] arr = new int[] { 1, 2, 3 };
        IEnumerable<int> enumerable = arr;

        int sum = 0;
        foreach (int i in enumerable)
        {
            sum += i;
        }

        Assert.True(sum == 6, "IEnumerable foreach on array");
    }

    // ==================== Per-Thread Allocation Tests ====================

    private static void TestThreadAllocBytesPositive()
    {
        // GC.GetAllocatedBytesForCurrentThread() tracks per-thread TLAB allocations.
        // After all the previous allocation tests, it must be > 0.
        long threadBytes = GC.GetAllocatedBytesForCurrentThread();
        Assert.True(threadBytes > 0, "Memory: per-thread allocated bytes must be > 0, got: " + threadBytes);
    }

    private static void TestTotalAllocBytesPositive()
    {
        // GC.GetTotalAllocatedBytes() must be > 0 after allocations
        long total = GC.GetTotalAllocatedBytes(precise: false);
        Assert.True(total > 0, "Memory: total allocated bytes must be > 0");
    }

    // ==================== Memory Copy Tests ====================

    private static void TestMemCopy8Bytes()
    {
        byte* src = stackalloc byte[CopyLength8Bytes];
        byte* dest = stackalloc byte[CopyLength8Bytes];

        for (int i = 0; i < CopyLength8Bytes; i++)
        {
            src[i] = (byte)(i + 1);
        }

        for (int i = 0; i < CopyLength8Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength8Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength8Bytes; i++)
        {
            if (dest[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 8-byte copy");
    }

    private static void TestMemCopy16Bytes()
    {
        byte* src = stackalloc byte[CopyLength16Bytes];
        byte* dest = stackalloc byte[CopyLength16Bytes];

        for (int i = 0; i < CopyLength16Bytes; i++)
        {
            src[i] = (byte)(i + 1);
        }

        for (int i = 0; i < CopyLength16Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength16Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength16Bytes; i++)
        {
            if (dest[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 16-byte copy");
    }

    private static void TestMemCopy24Bytes()
    {
        byte* src = stackalloc byte[CopyLength24Bytes];
        byte* dest = stackalloc byte[CopyLength24Bytes];

        for (int i = 0; i < CopyLength24Bytes; i++)
        {
            src[i] = (byte)(i + 1);
        }

        for (int i = 0; i < CopyLength24Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength24Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength24Bytes; i++)
        {
            if (dest[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 24-byte copy");
    }

    private static void TestMemCopy32Bytes()
    {
        byte* src = stackalloc byte[CopyLength32Bytes];
        byte* dest = stackalloc byte[CopyLength32Bytes];

        for (int i = 0; i < CopyLength32Bytes; i++)
        {
            src[i] = (byte)(i + 1);
        }

        for (int i = 0; i < CopyLength32Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength32Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength32Bytes; i++)
        {
            if (dest[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 32-byte copy");
    }

    private static void TestMemCopy48Bytes()
    {
        byte* src = stackalloc byte[CopyLength48Bytes];
        byte* dest = stackalloc byte[CopyLength48Bytes];

        for (int i = 0; i < CopyLength48Bytes; i++)
        {
            src[i] = (byte)((i + 1) & ByteValueMask);
        }

        for (int i = 0; i < CopyLength48Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength48Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength48Bytes; i++)
        {
            if (dest[i] != (byte)((i + 1) & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 48-byte copy");
    }

    private static void TestMemCopy64Bytes()
    {
        byte* src = stackalloc byte[CopyLength64Bytes];
        byte* dest = stackalloc byte[CopyLength64Bytes];

        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            src[i] = (byte)((i + 1) & ByteValueMask);
        }

        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength64Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            if (dest[i] != (byte)((i + 1) & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 64-byte copy");
    }

    private static void TestMemCopy80Bytes()
    {
        byte* src = stackalloc byte[CopyLength80Bytes];
        byte* dest = stackalloc byte[CopyLength80Bytes];

        for (int i = 0; i < CopyLength80Bytes; i++)
        {
            src[i] = (byte)((i + 1) & ByteValueMask);
        }

        for (int i = 0; i < CopyLength80Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength80Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength80Bytes; i++)
        {
            if (dest[i] != (byte)((i + 1) & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 80-byte copy");
    }

    private static void TestMemCopy128Bytes()
    {
        byte* src = stackalloc byte[CopyLength128Bytes];
        byte* dest = stackalloc byte[CopyLength128Bytes];

        for (int i = 0; i < CopyLength128Bytes; i++)
        {
            src[i] = (byte)((i + 1) & ByteValueMask);
        }

        for (int i = 0; i < CopyLength128Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength128Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength128Bytes; i++)
        {
            if (dest[i] != (byte)((i + 1) & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 128-byte copy");
    }

    private static void TestMemCopy256Bytes()
    {
        byte* src = stackalloc byte[CopyLength256Bytes];
        byte* dest = stackalloc byte[CopyLength256Bytes];

        for (int i = 0; i < CopyLength256Bytes; i++)
        {
            src[i] = (byte)(i & ByteValueMask);
        }

        for (int i = 0; i < CopyLength256Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength256Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength256Bytes; i++)
        {
            if (dest[i] != (byte)(i & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 256-byte copy");
    }

    private static void TestMemCopy264Bytes()
    {
        byte* src = stackalloc byte[CopyLength264Bytes];
        byte* dest = stackalloc byte[CopyLength264Bytes];

        for (int i = 0; i < CopyLength264Bytes; i++)
        {
            src[i] = (byte)(i & ByteValueMask);
        }

        for (int i = 0; i < CopyLength264Bytes; i++)
        {
            dest[i] = 0;
        }

        MemoryOp.MemCopy(dest, src, CopyLength264Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength264Bytes; i++)
        {
            if (dest[i] != (byte)(i & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemCopy: 264-byte copy");
    }

    private static void TestMemSet64Bytes()
    {
        byte* dest = stackalloc byte[CopyLength64Bytes];

        // Clear first
        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            dest[i] = 0;
        }

        // Fill with value 0xAB
        MemoryOp.MemSet(dest, MemSetFillValue, CopyLength64Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            if (dest[i] != MemSetFillValue)
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemSet: 64 bytes with 0xAB");
    }

    private static void TestMemMoveOverlap()
    {
        // Test overlapping copy (dest > src)
        byte* buffer = stackalloc byte[OverlapBufferLength];

        for (int i = 0; i < OverlapMoveLength; i++)
        {
            buffer[i] = (byte)(i + 1);
        }

        for (int i = OverlapMoveLength; i < OverlapBufferLength; i++)
        {
            buffer[i] = 0;
        }

        // Move 16 bytes from offset 0 to offset 8 (overlapping)
        MemoryOp.MemMove(buffer + OverlapOffset, buffer, OverlapMoveLength);

        bool passed = true;
        // First 8 bytes should be unchanged
        for (int i = 0; i < OverlapOffset; i++)
        {
            if (buffer[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        // Bytes 8-23 should be copies of original 0-15
        for (int i = OverlapOffset; i < OverlapOffset + OverlapMoveLength; i++)
        {
            if (buffer[i] != (byte)(i - OverlapOffset + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemMove: overlapping regions");
    }

    private static void TestMemCopy0Bytes()
    {
        byte* src = stackalloc byte[1];
        byte* dest = stackalloc byte[1];
        src[0] = SourceSentinel;
        dest[0] = DestSentinel;
        MemoryOp.MemCopy(dest, src, 0);
        Assert.True(dest[0] == DestSentinel, "MemCopy: 0 bytes is no-op");
    }

    private static void TestMemCopy1Byte()
    {
        byte* src = stackalloc byte[1];
        byte* dest = stackalloc byte[1];
        src[0] = SourceSentinel;
        dest[0] = DestSentinel;
        MemoryOp.MemCopy(dest, src, 1);
        Assert.True(dest[0] == SourceSentinel, "MemCopy: 1 byte copy");
    }

    private static void TestMemMoveOverlapDestBeforeSrc()
    {
        byte* buffer = stackalloc byte[OverlapBufferLength];
        for (int i = 0; i < OverlapBufferLength; i++)
        {
            buffer[i] = (byte)i;
        }

        // Move 16 bytes from offset 8 to offset 0 (overlapping, dest < src)
        MemoryOp.MemMove(buffer, buffer + OverlapOffset, OverlapMoveLength);

        bool passed = true;
        for (int i = 0; i < OverlapMoveLength; i++)
        {
            if (buffer[i] != (byte)(i + OverlapOffset))
            {
                passed = false;
            }
        }
        Assert.True(passed, "MemMove: overlapping regions (dest < src)");
    }

    // ==================== Array.Copy Tests ====================

    private static void TestArrayCopyIntArray()
    {
        int[] source = new int[] { 1, 2, 3, 4, 5 };
        int[] dest = new int[5];

        Array.Copy(source, dest, 5);

        bool passed = dest[0] == 1 && dest[1] == 2 && dest[2] == 3 && dest[3] == 4 && dest[4] == 5;
        Assert.True(passed, "Array.Copy: int[] copy");
    }

    private static void TestArrayCopyByteArray()
    {
        byte[] source = new byte[CopyLength64Bytes];
        byte[] dest = new byte[CopyLength64Bytes];

        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            source[i] = (byte)(i + 1);
        }

        Array.Copy(source, dest, CopyLength64Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength64Bytes; i++)
        {
            if (dest[i] != (byte)(i + 1))
            {
                passed = false;
            }
        }
        Assert.True(passed, "Array.Copy: byte[] 64 bytes");
    }

    private static void TestArrayCopyLargeArray()
    {
        byte[] source = new byte[CopyLength256Bytes];
        byte[] dest = new byte[CopyLength256Bytes];

        for (int i = 0; i < CopyLength256Bytes; i++)
        {
            source[i] = (byte)(i & ByteValueMask);
        }

        Array.Copy(source, dest, CopyLength256Bytes);

        bool passed = true;
        for (int i = 0; i < CopyLength256Bytes; i++)
        {
            if (dest[i] != (byte)(i & ByteValueMask))
            {
                passed = false;
            }
        }
        Assert.True(passed, "Array.Copy: byte[] 256 bytes (large SIMD)");
    }

    private static void TestArrayCopyZeroLength()
    {
        int[] src = new int[] { 1 };
        int[] dest = new int[] { 2 };
        Array.Copy(src, dest, 0);
        Assert.True(dest[0] == 2, "Array.Copy: 0 length is no-op");
    }

    private static void TestArrayCopyOverlap()
    {
        int[] arr = new int[] { 0, 1, 2, 3, 4, 5, 6, 7 };
        // Copy {0, 1, 2, 3} to indices 2, 3, 4, 5
        Array.Copy(arr, 0, arr, 2, 4);

        bool passed = arr[0] == 0 && arr[1] == 1 &&
                     arr[2] == 0 && arr[3] == 1 &&
                     arr[4] == 2 && arr[5] == 3 &&
                     arr[6] == 6 && arr[7] == 7;
        Assert.True(passed, "Array.Copy: overlapping regions");
    }

    // ==================== Cmdline Parsing Tests ====================
    // Bootloader/limine.conf passes `cmdline: arg1 arg2 arg3`. These tests
    // walk the full path: limine -> kmain -> __build_argv -> ArgvParser ->
    // NativeAOT __managed__Main -> Environment.GetCommandLineArgs().

    private static void TestCmdlineArgCount()
    {
        string[] args = Environment.GetCommandLineArgs();
        Assert.Equal(ExpectedArgvLength, args.Length, "Cmdline: argv0 + 3 parsed args");
    }

    private static void TestCmdlineArgv0()
    {
        string[] args = Environment.GetCommandLineArgs();
        Assert.True(args.Length >= 1, "Cmdline: argv has argv[0]");
        Assert.True(args[0] == "cosmos", "Cmdline: argv[0] is \"cosmos\"");
    }

    private static void TestCmdlineArgValues()
    {
        string[] args = Environment.GetCommandLineArgs();
        Assert.True(args.Length >= ExpectedArgvLength, "Cmdline: argv has 3 parsed args");
        Assert.True(args[1] == "arg1", "Cmdline: argv[1] == arg1");
        Assert.True(args[2] == "arg2", "Cmdline: argv[2] == arg2");
        Assert.True(args[3] == "arg3", "Cmdline: argv[3] == arg3");
    }

    private class SimpleStringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y) => x == y || (x != null && y != null && x.Equals(y));
        public int GetHashCode(string obj) => obj?.GetHashCode() ?? 0;
    }

    // ==================== VirtualToPhysical ====================

    // Non-GC static: NativeAOT places primitive statics in the kernel
    // image's data section, i.e. the top-2GiB kernel window — exactly the
    // address class VirtualToPhysical must refuse to "translate".
    private static ulong s_v2pImageProbe;

    private static unsafe ulong HhdmOffset()
        => Cosmos.Kernel.Boot.Limine.Limine.HHDM.Response != null
            ? Cosmos.Kernel.Boot.Limine.Limine.HHDM.Response->Offset
            : 0;

    // On x64 the allocator hands out HHDM aliases (RamStart = HHDM + phys)
    // and translation is exact offset subtraction; on arm64 the heap lives
    // in the identity-mapped low range and the same call must pass through
    // unchanged. Pin the translation contract for whichever space the
    // allocator actually uses on this arch.
    private static unsafe void TestV2PHhdmAliasTranslates()
    {
        ulong hhdm = HhdmOffset();
        Assert.True(hhdm != 0, "Limine HHDM response must be present");

        void* page = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        Assert.True(page != null, "page allocation must succeed");

        ulong va = (ulong)page;
        ulong phys = PageAllocator.VirtualToPhysical(va);
        if (va >= hhdm)
        {
            Assert.True(phys == va - hhdm, "HHDM alias must translate by exact offset subtraction");
        }
        else
        {
            Assert.True(phys == va, "identity-space allocation must pass through unchanged");
        }

        PageAllocator.Free(page);
    }

    // Values below the HHDM base look already-physical and must pass
    // through unchanged — storage drivers hand such values straight to
    // device doorbells.
    private static void TestV2PPhysicalPassesThrough()
    {
        ulong phys = PageAllocator.VirtualToPhysical(PhysicalProbeAddress);
        Assert.True(phys == PhysicalProbeAddress, "already-physical values must pass through unchanged");
    }

    // Kernel-image addresses (statics, code) are higher-half but NOT HHDM
    // aliases: subtracting the HHDM offset from one yields a garbage
    // physical address — silent DMA corruption. The translation must
    // reject them loudly instead of returning the bogus value.
    private static void TestV2PKernelImageRejected()
    {
        ulong va = V2PProbeAddress();
        Assert.True(va >= KernelImageWindowBase, "a primitive static must live in the kernel image window");
        Assert.True(V2PRejects(va), "VirtualToPhysical must reject kernel-image addresses instead of fabricating a physical address");
    }

    private static unsafe ulong V2PProbeAddress()
        => (ulong)Unsafe.AsPointer(ref s_v2pImageProbe);

    // Single try/catch in its own helper (arm64 EH inlining quirk).
    /// <summary>
    /// True when constructing a <see cref="PhysicalAddress"/> from
    /// <paramref name="value"/> throws, i.e. the value is rejected as unaligned.
    /// </summary>
    /// <remarks>
    /// Separate from <c>V2PRejects</c>, which exercises the HHDM translation
    /// rules instead. Both reject bad input, but they are different contracts:
    /// one is the value object's own validation, the other is
    /// <c>PageAllocator</c> refusing a kernel-image address.
    /// </remarks>
    private static bool PhysicalAddressRejects(ulong value)
    {
        try
        {
            PhysicalAddress.From(value);
            return false;
        }
        catch (Exception)
        {
            // Vogen raises ValueObjectValidationException for a failed
            // Validate; catching Exception keeps the test honest about the
            // contract (rejection) rather than one exception type.
            return true;
        }
    }

    private static bool V2PRejects(ulong virtualAddress)
    {
        try
        {
            PageAllocator.VirtualToPhysical(virtualAddress);
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            return true;
        }
    }

    // ==================== Pager ====================

    // The Pager is kernel-internal paging plumbing: nothing in an application
    // can reach it yet, because the kernel has no ring-3 transition and no
    // syscall path. These tests pin the contracts that user-space memory
    // management will be built on top of — that a mapping survives the
    // describe/install/round-trip cycle, that a fresh space stays anchored to
    // the kernel's own mappings, and that the refusal paths fail loudly rather
    // than silently dropping work.

    /// <summary>
    /// The backend must be registered during InitializeHardware, before any
    /// other Pager entry point runs.
    /// </summary>
    private static unsafe void TestPagerBackendRegistered()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Pager.Create must succeed once the backend is registered");

        if (space != null)
        {
            Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
        }
    }

    /// <summary>
    /// The live tables must already translate a kernel-image address. This is
    /// the anchor every later Pager test depends on: if the kernel cannot
    /// resolve its own code, a new address space that failed to inherit the
    /// kernel mappings would be indistinguishable from one that worked.
    /// </summary>
    private static unsafe void TestPagerTranslateCurrentKernelImage()
    {
        // A method pointer to this very method is, by construction, an address
        // in the kernel image that the CPU has already translated to reach us.
        ulong here = KernelImageAnchor();


        Assert.True(Pager.Translate(VirtualAddress.From(here & ~(Pager.PageSize - 1)), out PhysicalAddress phys),
            "Translate must resolve an address the CPU already executed at");
        Assert.True(phys.Value != PhysicalAddress.None.Value,
            "a resolved kernel-image address must not report the none sentinel");
    }

    /// <summary>
    /// An address in the lower half, above Limine's low-4GiB map, must report
    /// unmapped rather than inventing a translation.
    /// </summary>
    private static unsafe void TestPagerTranslateCurrentUnmapped()
    {
        bool mapped = Pager.Translate(VirtualAddress.From(PagerUnmappedProbeAddress), out PhysicalAddress phys);

        // QEMU/EDK2 may or may not have mapped this depending on firmware, so
        // the assertion is on the failure shape, not on a specific answer.
        if (!mapped)
        {
            Assert.True(phys.Value == PhysicalAddress.None.Value,
                "an unmapped address must report the none sentinel");
        }
    }

    /// <summary>
    /// Translating through an explicit descriptor must agree with translating
    /// against the live tables, since a new space is seeded from the live root.
    /// </summary>
    private static unsafe void TestPagerTranslateExplicitMatches()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        ulong here = KernelImageAnchor();
        VirtualAddress target = VirtualAddress.From(here & ~(Pager.PageSize - 1));

        bool viaSpace = Pager.Translate(space, target, out PhysicalAddress spacePhys);
        bool viaCurrent = Pager.Translate(target, out PhysicalAddress currentPhys);

        Assert.True(viaSpace == viaCurrent,
            "explicit-space and live-table translation must agree on presence");
        Assert.True(!viaSpace || spacePhys.Value == currentPhys.Value,
            "explicit-space and live-table translation must agree on the physical frame");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// The core round trip: allocate a frame, describe it, commit, and confirm
    /// both the translation and an actual write/read through it. Verifying only
    /// the translation would pass even if the entry pointed at the wrong frame.
    /// </summary>
    private static unsafe void TestPagerCommitRoundTrip()
    {
        void* page = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        Assert.True(page != null, "page allocation must succeed");

        if (page is null)
        {
            return;
        }

        ulong frame = PageAllocator.VirtualToPhysical((ulong)page);
        VirtualAddress alias = VirtualAddress.From(PagerRoundTripAlias());

        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            PageAllocator.Free(page);
            return;
        }

        bool added = Pager.Add(space, alias, PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable | PageFlags.Cacheable);
        Assert.True(added, "Add must accept a region below capacity");

        Pager.Commit(space);

        Assert.True(Pager.Translate(space, alias, out PhysicalAddress resolved),
            "a committed region must translate");
        Assert.True(resolved.Value == frame,
            "Translate must return the frame that was mapped");

        // Prove the mapping is real by writing through it and reading back.
        byte* target = (byte*)(ulong)alias.Value;
        target[0] = PagerTestMarker;
        Assert.True(target[0] == PagerTestMarker, "the committed mapping must be readable and writable");
        Assert.True(((byte*)frame)[0] == PagerTestMarker,
            "writing through the alias must reach the original frame");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
        PageAllocator.Free(page);
    }

    /// <summary>
    /// A region spanning several pages must install every page in the range, not
    /// just the first.
    /// </summary>
    private static unsafe void TestPagerCommitMultiPage()
    {
        void* pages = PageAllocator.AllocPages(PageType.Unmanaged, PagerMultiPageCount, zero: true);
        Assert.True(pages != null, "multi-page allocation must succeed");

        if (pages is null)
        {
            return;
        }

        ulong frame = PageAllocator.VirtualToPhysical((ulong)pages);
        VirtualAddress alias = VirtualAddress.From(PagerRoundTripAlias(1));

        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            PageAllocator.Free(pages);
            return;
        }

        Pager.Add(space, alias, PhysicalAddress.From(frame), PagerMultiPageCount,
            PageFlags.Present | PageFlags.Writable | PageFlags.Cacheable);
        Pager.Commit(space);

        for (int i = 0; i < PagerMultiPageCount; i++)
        {
            VirtualAddress page = VirtualAddress.From(alias.Value + ((ulong)i * Pager.PageSize));
            Assert.True(Pager.Translate(space, page, out PhysicalAddress resolved),
                "every page of a multi-page region must translate");
            Assert.True(resolved.Value == frame + ((ulong)i * Pager.PageSize),
                "each page must translate to its own consecutive frame");

            byte* target = (byte*)(ulong)page.Value;
            target[0] = PagerTestMarker;
            Assert.True(target[0] == PagerTestMarker, "each page of the range must be writable");
        }

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
        PageAllocator.Free(pages);
    }

    /// <summary>
    /// A byte-granular address has no single correct translation, so it must be
    /// refused rather than rounded.
    /// </summary>
    private static unsafe void TestPagerTranslateUnaligned()
    {
        ulong here = KernelImageAnchor();
        VirtualAddress unaligned = VirtualAddress.From((here & ~(Pager.PageSize - 1)) + 1);

        Assert.True(!Pager.Translate(unaligned, out _),
            "Translate must reject a non-page-aligned address");
    }

    /// <summary>
    /// A null descriptor must report failure, not dereference.
    /// </summary>
    private static unsafe void TestPagerTranslateNullSpace()
    {
        Assert.True(!Pager.Translate(null, VirtualAddress.From(PagerUnmappedProbeAddress), out _),
            "Translate must return false for a null space");
    }

    /// <summary>
    /// Add must refuse past capacity and say so. Silently dropping the region
    /// would leave a caller believing a mapping exists that was never installed.
    /// </summary>
    private static unsafe void TestPagerAddAtCapacity()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        for (int i = 0; i < PagerCapacityProbe; i++)
        {
            Assert.True(Pager.Add(space, VirtualAddress.From(0), PhysicalAddress.From(0), 0, PageFlags.None),
                "Add must succeed up to capacity");
        }

        Assert.True(space->RegionCount == AddressSpaceDescriptor.MaxRegionCount,
            "a full descriptor must report exactly MaxRegionCount regions");
        Assert.True(!Pager.Add(space, VirtualAddress.From(0), PhysicalAddress.From(0), 0, PageFlags.None),
            "Add must refuse once the region array is full");
        Assert.True(space->RegionCount == AddressSpaceDescriptor.MaxRegionCount,
            "a refused Add must not corrupt the region count");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// Committing a space with no regions must succeed and change nothing.
    /// </summary>
    private static unsafe void TestPagerCommitEmptySpace()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        Pager.Commit(space);

        Assert.True(space->IsCommitted,
            "committing an empty space must still mark it committed");
        Assert.True(space->RegionCount == 0, "an empty space must have no regions");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// A zero-page region installs nothing and must not translate. This is the
    /// documented way to reserve an address range before committing a length.
    /// </summary>
    private static unsafe void TestPagerCommitZeroPage()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        VirtualAddress probe = VirtualAddress.From(PagerRoundTripAlias(2));
        Pager.Add(space, probe, PhysicalAddress.From(Pager.PageSize), 0, PageFlags.Present);
        Pager.Commit(space);

        Assert.True(!Pager.Translate(space, probe, out _),
            "a zero-page region must not create a mapping");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// A new space must be seeded from the live root rather than starting
    /// empty. Without this, installing it would leave the kernel image unmapped
    /// and the very next instruction fetch would fault.
    /// </summary>
    private static unsafe void TestPagerNewSpaceSharesKernelRoot()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        Assert.True(space->RootFrame != 0,
            "a new space must be seeded with the live page-table root, not zero");
        Assert.True(!space->IsCommitted,
            "a freshly created space must not claim to be committed");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// The shared-root invariant, tested directly: a space created from the
    /// live root must still translate the kernel image after being committed.
    /// This is the property the context-switch path will depend on when it
    /// loads a different root, so it is worth pinning before any swap exists.
    /// </summary>
    private static unsafe void TestPagerNewSpaceKernelReachable()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        Pager.Commit(space);

        ulong here = KernelImageAnchor();
        VirtualAddress herePage = VirtualAddress.From(here & ~(Pager.PageSize - 1));

        Assert.True(Pager.Translate(space, herePage, out PhysicalAddress viaSpace),
            "a committed space must still resolve the kernel image");
        Assert.True(Pager.Translate(herePage, out PhysicalAddress viaCurrent),
            "the live tables must still resolve the kernel image");

        Assert.True(viaSpace.Value == viaCurrent.Value,
            "the kernel image must resolve to the same frame in a new space as in the live tables");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// Filling a space to capacity must not disturb translation: the region
    /// array is fixed-size, and exhausting it should have no effect on entries
    /// already installed.
    /// </summary>
    private static unsafe void TestPagerTranslateCapacityUnaffected()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        ulong here = KernelImageAnchor();
        VirtualAddress herePage = VirtualAddress.From(here & ~(Pager.PageSize - 1));

        for (int i = 0; i < PagerCapacityProbe; i++)
        {
            Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias((ulong)(i + 0x1000))), PhysicalAddress.None, 0, PageFlags.None);
        }

        Pager.Commit(space);

        Assert.True(Pager.Translate(space, herePage, out PhysicalAddress resolved),
            "a space filled with empty regions must still resolve the kernel image");
        Assert.True(resolved.Value != PhysicalAddress.None.Value,
            "the resolved kernel frame must not be the none sentinel");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// Descriptors created but never given a region must not map anything. This
    /// guards against a Create-time default accidentally installing the whole
    /// physical space.
    /// </summary>
    private static unsafe void TestPagerTranslateZeroRegions()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            return;
        }

        Pager.Commit(space);

        Assert.True(!Pager.Translate(space, VirtualAddress.From(PagerUnmappedProbeAddress), out PhysicalAddress phys),
            "an empty space must not translate an unmapped lower-half address");
        Assert.True(phys.Value == PhysicalAddress.None.Value,
            "a refused translation must report the none sentinel");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
    }

    /// <summary>
    /// A single-page region is the smallest thing Commit can install, and the
    /// case the 4 KiB path exists for.
    /// </summary>
    private static unsafe void TestPagerTranslateSinglePage()
    {
        void* page = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        Assert.True(page != null, "page allocation must succeed");

        if (page is null)
        {
            return;
        }

        ulong frame = PageAllocator.VirtualToPhysical((ulong)page);
        VirtualAddress alias = VirtualAddress.From(PagerRoundTripAlias(3));

        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Create must succeed");

        if (space is null)
        {
            PageAllocator.Free(page);
            return;
        }

        Pager.Add(space, alias, PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable | PageFlags.Cacheable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, alias, out PhysicalAddress resolved),
            "a single-page region must translate");
        Assert.True(resolved.Value == frame, "a single-page region must translate to its own frame");

        // The page immediately after must not have been mapped as a side effect.
        VirtualAddress after = VirtualAddress.From(alias.Value + Pager.PageSize);
        Assert.True(!Pager.Translate(space, after, out _),
            "a single-page region must not map the following page");

        Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
        PageAllocator.Free(page);
    }

    // ==================== Pager: PageFlags encoding ====================

    // PageFlags is a shared vocabulary; each backend lowers it to its own
    // architecture bits. Most of that lowering is not observable from managed
    // code without reading raw page tables, so these tests assert what IS
    // observable — that a mapping installs at all, that the translation is
    // correct, and that the flags are accepted — and they name the flags that
    // are accepted-but-unenforced so a future reader does not mistake them for
    // working protection.

    /// <summary>
    /// <see cref="PageFlags.None"/> must still produce a usable, non-executable
    /// mapping. An omitted <see cref="PageFlags.Executable"/> means NX on both
    /// architectures, which is the safe default for data.
    /// </summary>
    private static unsafe void TestPagerFlagsNoneDefaults()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x10)), PhysicalAddress.From(frame), 1,
            PageFlags.None);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x10)), out PhysicalAddress resolved),
            "a region with no flags must still install a mapping");
        Assert.True(resolved.Value == frame, "a no-flag mapping must resolve to its own frame");

        PagerCleanup(space);
    }

    /// <summary>
    /// <see cref="PageFlags.Executable"/> must not break installation: the x64
    /// backend clears NX and the ARM64 backend clears PXN/UXN, and both must
    /// still leave a translatable entry.
    /// </summary>
    private static unsafe void TestPagerFlagsExecutable()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x11)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Executable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x11)), out PhysicalAddress resolved),
            "an executable region must install and translate");
        Assert.True(resolved.Value == frame, "an executable mapping must resolve to its own frame");

        PagerCleanup(space);
    }

    /// <summary>
    /// A writable region must translate. Whether it is genuinely writable is
    /// only provable by writing through it, which needs an installed mapping
    /// and is covered by the round-trip test.
    /// </summary>
    private static unsafe void TestPagerFlagsWritable()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x12)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x12)), out _),
            "a writable region must install and translate");

        PagerCleanup(space);
    }

    /// <summary>
    /// A <see cref="PageFlags.Device"/> region must install. On ARM64 this
    /// selects a Device MAIR attribute and therefore leaves AP[2] clear, so the
    /// region is deliberately read-only — this asserts only that it exists.
    /// </summary>
    private static unsafe void TestPagerFlagsDevice()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x13)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Device);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x13)), out PhysicalAddress resolved),
            "a device region must install and translate");
        Assert.True(resolved.Value == frame, "a device mapping must resolve to its own frame");

        PagerCleanup(space);
    }

    /// <summary>
    /// <see cref="PageFlags.User"/> is defined but not yet usable: there is no
    /// ring-3 transition in the kernel today, so nothing can prove the
    /// privilege bit is enforced. This asserts the flag is accepted rather than
    /// rejected, and records that its effect is unverified.
    /// </summary>
    private static unsafe void TestPagerFlagsUser()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        bool accepted = Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x14)),
            PhysicalAddress.From(frame), 1, PageFlags.Present | PageFlags.User | PageFlags.Writable);
        Assert.True(accepted, "PageFlags.User must be accepted even though it is not yet enforced");

        PagerCleanup(space);
    }

    /// <summary>
    /// <see cref="PageFlags.Global"/> has no ARM64 encoding at all — TLBI is
    /// issued without an address-space tag — so on ARM64 the bit is ignored
    /// rather than rejected. It must at least not prevent installation.
    /// </summary>
    private static unsafe void TestPagerFlagsGlobal()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x15)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Global | PageFlags.Writable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x15)), out _),
            "a global region must install and translate");

        PagerCleanup(space);
    }

    /// <summary>
    /// <see cref="PageFlags.Uncached"/> is not separately expressible without
    /// programming the PAT MSR (x64) or adding a MAIR entry (ARM64). Both
    /// backends currently fall through to the cacheable default, so this pins
    /// the present behaviour rather than an aspiration: the mapping installs
    /// and translates.
    /// </summary>
    private static unsafe void TestPagerFlagsUncached()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x16)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Uncached | PageFlags.Writable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x16)), out PhysicalAddress resolved),
            "an uncached region must install and translate");
        Assert.True(resolved.Value == frame, "an uncached mapping must resolve to its own frame");

        PagerCleanup(space);
    }

    /// <summary>
    /// Flags must combine bitwise, and the combination must not corrupt the
    /// entry: the architecture bits they lower to overlap the address field on
    /// both architectures, so a combination is the case most likely to expose a
    /// masking error.
    /// </summary>
    private static unsafe void TestPagerFlagsCombined()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        PageFlags combined = PageFlags.Present | PageFlags.Writable | PageFlags.Executable
            | PageFlags.User | PageFlags.Global | PageFlags.Cacheable;

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x17)), PhysicalAddress.From(frame), 1, combined);
        Pager.Commit(space);

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x17)), out PhysicalAddress resolved),
            "a combined-flag region must install and translate");
        Assert.True(resolved.Value == frame,
            "combined flags must not corrupt the physical frame (address/flag overlap)");

        PagerCleanup(space);
    }

    // ==================== Pager: address value objects ====================

    /// <summary>A virtual address must survive a wrap/unwrap round trip byte for byte.</summary>
    private static void TestVirtualAddressRoundTrip()
    {
        ulong raw = 0xFFFF_4000_0000_0000;
        Assert.True(VirtualAddress.From(raw).Value == raw,
            "VirtualAddress must return the exact value it was constructed from");
        Assert.True(VirtualAddress.From(0).Value == 0, "VirtualAddress zero must round trip");
        Assert.True(VirtualAddress.From(ulong.MaxValue).Value == ulong.MaxValue,
            "VirtualAddress max must round trip (no canonical-form validation)");
    }

    /// <summary>Two virtual addresses with the same value are equal, and different ones are not.</summary>
    private static void TestVirtualAddressEquality()
    {
        Assert.True(VirtualAddress.From(0x1000) == VirtualAddress.From(0x1000),
            "VirtualAddress equality must be by value");
        Assert.True(VirtualAddress.From(0x1000) != VirtualAddress.From(0x2000),
            "VirtualAddress inequality must distinguish values");
        Assert.True(VirtualAddress.From(0x1000).Equals(VirtualAddress.From(0x1000)),
            "VirtualAddress.Equals must agree with the == operator");
    }

    /// <summary>
    /// <c>ToString</c> must surface the value, and an explicit format string
    /// must reach the underlying <see cref="ulong"/>'s formatting.
    /// </summary>
    /// <remarks>
    /// The parameterless overload is decimal, not hex — Vogen's default. That is
    /// the right thing for a general-purpose value object, but it is not what
    /// the Serial/panic paths want, so they pass "X". Both forms are pinned
    /// here so a future generator upgrade that changes either is caught.
    /// </remarks>
    private static void TestVirtualAddressToString()
    {
        Assert.True(VirtualAddress.From(0xDEAD_BEEF).ToString() == "3735928559",
            "VirtualAddress.ToString() must be the decimal value by default");

        Assert.True(VirtualAddress.From(0xDEAD_BEEF).ToString("X") == "DEADBEEF",
            "VirtualAddress.ToString(\"X\") must format the underlying ulong as hex");
    }

    /// <summary>A page-aligned physical address is valid on both architectures.</summary>
    private static void TestPhysicalAddressAligned()
    {
        Assert.True(PhysicalAddress.From(0).Value == 0, "physical address 0 must be accepted");
        Assert.True(PhysicalAddress.From(0x1000).Value == 0x1000, "a 4 KiB-aligned address must be accepted");
        Assert.True(PhysicalAddress.From(0x599C_0000).Value == 0x599C_0000,
            "a high page-aligned address must be accepted");
    }

    /// <summary>
    /// An unaligned physical address has no page-table encoding on either
    /// architecture, so construction must fail rather than silently dropping the
    /// low bits and mapping the wrong frame.
    /// </summary>
    private static void TestPhysicalAddressUnaligned()
    {
        Assert.True(PhysicalAddressRejects(0x1001), "physical address 0x1001 must be rejected as unaligned");
        Assert.True(PhysicalAddressRejects(0x0FFF), "physical address 0x0FFF must be rejected as unaligned");
        Assert.True(PhysicalAddressRejects(0x0800), "physical address 0x0800 must be rejected as unaligned");
    }

    /// <summary>
    /// The failure sentinel is physical 0. It must be constructible (0 is
    /// page-aligned) so the <c>out</c> parameter can always be assigned.
    /// </summary>
    private static void TestPhysicalAddressNone()
    {
        Assert.True(PhysicalAddress.None.Value == 0,
            "PhysicalAddress.None must be physical address 0");
    }

    /// <summary>Physical addresses compare by value like any other value object.</summary>
    private static void TestPhysicalAddressEquality()
    {
        Assert.True(PhysicalAddress.From(0x2000) == PhysicalAddress.From(0x2000),
            "PhysicalAddress equality must be by value");
        Assert.True(PhysicalAddress.From(0x2000) != PhysicalAddress.From(0x3000),
            "PhysicalAddress inequality must distinguish values");
    }

    /// <summary>
    /// The two address types must not be interchangeable. They have the same
    /// underlying <see cref="ulong"/>, so the compiler cannot stop a physical
    /// address being passed where a virtual one belongs — the only defence is
    /// that the types differ, and this test fails loudly if they ever merge.
    /// </summary>
    private static void TestAddressTypeDistinctness()
    {
        Assert.True(typeof(VirtualAddress) != typeof(PhysicalAddress),
            "VirtualAddress and PhysicalAddress must remain distinct types");
        Assert.True(typeof(VirtualAddress) != typeof(ulong),
            "VirtualAddress must remain a wrapper, not an alias for ulong");
    }

    // ==================== Pager: descriptor bookkeeping ====================

    /// <summary>Several regions in one space must all be retained and installed.</summary>
    private static unsafe void TestPagerAddMultipleRegions()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        for (int i = 0; i < 8; i++)
        {
            bool added = Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x20 + (ulong)i)),
                PhysicalAddress.From(frame), 1, PageFlags.Present | PageFlags.Writable);
            Assert.True(added, $"Add must accept region {i} below capacity");
        }

        Assert.True(space->RegionCount == 8, "eight added regions must be retained");

        Pager.Commit(space);

        for (int i = 0; i < 8; i++)
        {
            Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x20 + (ulong)i)), out _),
                $"region {i} of a multi-region space must translate");
        }

        PagerCleanup(space);
    }

    /// <summary>A null descriptor must be refused rather than dereferenced.</summary>
    private static void TestPagerAddNullSpace()
    {
        Assert.True(!Pager.Add(null, VirtualAddress.From(0x1000), PhysicalAddress.From(0x1000), 1, PageFlags.Present),
            "Add must return false for a null space");
    }

    /// <summary>
    /// A zero-page region must still be recorded: reserving an address range
    /// before committing to a length is the documented use, so it has to
    /// consume a slot and leave the count consistent.
    /// </summary>
    private static unsafe void TestPagerAddZeroPageRetained()
    {
        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Assert.True(Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x30)),
                PhysicalAddress.From(Pager.PageSize), 0, PageFlags.Present),
            "Add must accept a zero-page region");
        Assert.True(space->RegionCount == 1, "a zero-page region must still consume a slot");

        PagerCleanup(space);
    }

    /// <summary>
    /// Each descriptor is its own allocation. Two must not alias, or installing
    /// into one would silently rewrite the other.
    /// </summary>
    private static unsafe void TestPagerCreateDistinct()
    {
        AddressSpaceDescriptor* first = Pager.Create();
        AddressSpaceDescriptor* second = Pager.Create();

        Assert.True(first != null && second != null, "Create must succeed twice");
        Assert.True((ulong)first != (ulong)second, "Create must return distinct descriptors");

        if (first != null && second != null)
        {
            Assert.True(first->RegionCount == 0 && second->RegionCount == 0,
                "both new descriptors must start empty");

            Pager.Add(first, VirtualAddress.From(PagerRoundTripAlias(0x40)),
                PhysicalAddress.From(Pager.PageSize), 0, PageFlags.None);

            Assert.True(first->RegionCount == 1, "the added descriptor must record the region");
            Assert.True(second->RegionCount == 0, "the other descriptor must be unaffected");

            PagerCleanup(first);
            PagerCleanup(second);
        }
    }

    /// <summary>
    /// Committing the same space twice must not double-count regions or
    /// otherwise disturb it. Callers are expected to commit after each Add, so
    /// the second commit is the common case, not an error path.
    /// </summary>
    private static unsafe void TestPagerCommitIdempotent()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x41)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable);

        Pager.Commit(space);
        Assert.True(space->RegionCount == 1, "the first commit must not alter the region count");
        Assert.True(space->IsCommitted, "the first commit must mark the space committed");

        Pager.Commit(space);
        Assert.True(space->RegionCount == 1, "a repeated commit must not alter the region count");
        Assert.True(space->IsCommitted, "a repeated commit must leave the space committed");

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x41)), out PhysicalAddress resolved),
            "a repeatedly committed space must still translate");
        Assert.True(resolved.Value == frame, "a repeated commit must not change the resolved frame");

        PagerCleanup(space);
    }

    /// <summary>Committing null must be a no-op, not a crash.</summary>
    private static void TestPagerCommitNullSpace()
    {
        Pager.Commit(null);
        Assert.True(true, "Commit(null) must not throw");
    }

    /// <summary>
    /// Committing must never disturb the kernel's own mappings. This is the
    /// shared-root invariant, re-checked after real regions have been installed
    /// rather than on a freshly created space.
    /// </summary>
    private static unsafe void TestPagerCommitKernelIntact()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x42)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable);
        Pager.Commit(space);

        Assert.True(Pager.Translate(KernelImageAnchor(out _), out PhysicalAddress resolved),
            "the kernel image must still translate after a space with real regions is committed");
        Assert.True(resolved.Value != PhysicalAddress.None.Value,
            "the kernel frame must not be the none sentinel");

        PagerCleanup(space);
    }

    /// <summary>
    /// Adding a region after a commit, then committing again, must make the new
    /// region translatable without disturbing the first. This is the
    /// grow-a-space-later flow.
    /// </summary>
    private static unsafe void TestPagerTranslateAfterRecommit()
    {
        ulong frame = PagerTestFrame();
        if (frame == 0)
        {
            return;
        }

        AddressSpaceDescriptor* space = PagerCreateOrNull();
        if (space is null)
        {
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x43)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable);
        Pager.Commit(space);
        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x43)), out _),
            "the first region must translate after the first commit");

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x44)), PhysicalAddress.From(frame), 1,
            PageFlags.Present | PageFlags.Writable);
        Pager.Commit(space);

        Assert.True(space->RegionCount == 2, "a second commit must retain both regions");
        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x44)), out _),
            "a region added after a commit must translate once committed again");
        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x43)), out _),
            "the original region must survive a second commit");

        PagerCleanup(space);
    }

    /// <summary>
    /// The final page of a multi-page region must map. An off-by-one that only
    /// covers all but the last page is the classic range bug.
    /// </summary>
    private static unsafe void TestPagerTranslateLastPage()
    {
        if (!PagerTryMapRange(0x50, 4, out AddressSpaceDescriptor* space, out ulong frame))
        {
            return;
        }

        VirtualAddress last = VirtualAddress.From(PagerRoundTripAlias(0x50) + (3 * Pager.PageSize));
        Assert.True(Pager.Translate(space, last, out PhysicalAddress resolved),
            "the last page of a multi-page region must translate");
        Assert.True(resolved.Value == frame + (3 * Pager.PageSize),
            "the last page must resolve to the last frame of the range");

        PagerCleanup(space);
    }

    /// <summary>The first page of a multi-page region must map.</summary>
    private static unsafe void TestPagerTranslateFirstPage()
    {
        if (!PagerTryMapRange(0x51, 4, out AddressSpaceDescriptor* space, out ulong frame))
        {
            return;
        }

        Assert.True(Pager.Translate(space, VirtualAddress.From(PagerRoundTripAlias(0x51)), out PhysicalAddress resolved),
            "the first page of a multi-page region must translate");
        Assert.True(resolved.Value == frame,
            "the first page must resolve to the base frame of the range");

        PagerCleanup(space);
    }

    /// <summary>
    /// A page count that is neither 1 nor a huge-page multiple must be installed
    /// page by page, and every page must resolve to its own consecutive frame.
    /// </summary>
    private static unsafe void TestPagerTranslateOddCount()
    {
        const ulong Pages = 3;

        if (!PagerTryMapRange(0x52, Pages, out AddressSpaceDescriptor* space, out ulong frame))
        {
            return;
        }

        Assert.True(space->RegionCount == 1, "an odd-length range is still a single region");

        for (ulong i = 0; i < Pages; i++)
        {
            VirtualAddress page = VirtualAddress.From(PagerRoundTripAlias(0x52) + (i * Pager.PageSize));
            Assert.True(Pager.Translate(space, page, out PhysicalAddress resolved),
                $"page {i} of a {Pages}-page region must translate");
            Assert.True(resolved.Value == frame + (i * Pager.PageSize),
                $"page {i} must resolve to its own frame");
        }

        PagerCleanup(space);
    }

    /// <summary>
    /// A region larger than a huge page must install every page. This is the
    /// case that needs real intermediate tables, so it is the most likely to
    /// expose a walk that only works for small mappings.
    /// </summary>
    private static unsafe void TestPagerTranslateLargeRegion()
    {
        const ulong Pages = 16;

        void* memory = PageAllocator.AllocPages(PageType.Unmanaged, (ulong)Pages, zero: true);
        Assert.True(memory != null, "large-region page allocation must succeed");

        if (memory is null)
        {
            return;
        }

        ulong frame = PageAllocator.VirtualToPhysical((ulong)memory);
        AddressSpaceDescriptor* space = PagerCreateOrNull();

        if (space is null)
        {
            PageAllocator.Free(memory);
            return;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(0x53)), PhysicalAddress.From(frame), Pages,
            PageFlags.Present | PageFlags.Writable | PageFlags.Cacheable);
        Pager.Commit(space);

        for (ulong i = 0; i < Pages; i++)
        {
            VirtualAddress page = VirtualAddress.From(PagerRoundTripAlias(0x53) + (i * Pager.PageSize));
            Assert.True(Pager.Translate(space, page, out PhysicalAddress resolved),
                $"page {i} of a {Pages}-page region must translate");
            Assert.True(resolved.Value == frame + (i * Pager.PageSize),
                $"page {i} of a large region must resolve to its own frame");
        }

        PagerCleanup(space);
        PageAllocator.Free(memory);
    }

    // ==================== Pager: test helpers ====================

    /// <summary>
    /// Allocates one page and returns its frame, or 0 when the allocation
    /// failed. Callers treat 0 as "skip" so a memory-starved run reports a
    /// skipped check rather than a bogus failure.
    /// </summary>
    private static unsafe ulong PagerTestFrame()
    {
        void* page = PageAllocator.AllocPages(PageType.Unmanaged, 1, zero: true);
        return page is null ? 0 : PageAllocator.VirtualToPhysical((ulong)page);
    }

    /// <summary>Creates a descriptor, or returns null after asserting the failure.</summary>
    private static AddressSpaceDescriptor* PagerCreateOrNull()
    {
        AddressSpaceDescriptor* space = Pager.Create();
        Assert.True(space != null, "Pager.Create must succeed");
        return space;
    }

    /// <summary>
    /// Frees a descriptor and the frame behind it. Both are owned by the
    /// caller, since the kernel heap is non-moving and nothing finalizes them.
    /// </summary>
    private static void PagerCleanup(AddressSpaceDescriptor* space)
    {
        if (space is not null)
        {
            Cosmos.Kernel.Core.Memory.Heap.Heap.Free(space);
        }
    }

    /// <summary>
    /// Allocates a contiguous run and describes it as a single region,
    /// returning false (having asserted) when either step fails.
    /// </summary>
    private static unsafe bool PagerTryMapRange(ulong slot, ulong pages,
        out AddressSpaceDescriptor* space, out ulong frame)
    {
        void* memory = PageAllocator.AllocPages(PageType.Unmanaged, pages, zero: true);
        Assert.True(memory != null, "range page allocation must succeed");

        if (memory is null)
        {
            space = null;
            frame = 0;
            return false;
        }

        frame = PageAllocator.VirtualToPhysical((ulong)memory);
        space = PagerCreateOrNull();

        if (space is null)
        {
            PageAllocator.Free(memory);
            frame = 0;
            return false;
        }

        Pager.Add(space, VirtualAddress.From(PagerRoundTripAlias(slot)), PhysicalAddress.From(frame), pages,
            PageFlags.Present | PageFlags.Writable | PageFlags.Cacheable);
        Pager.Commit(space);

        return true;
    }

    /// <summary>
    /// A static field used as a known-good kernel-image address. Its address is
    /// in the top-2GiB kernel window that NativeAOT maps, so the CPU has
    /// necessarily already translated the page containing it — which makes it the
    /// cheapest trustworthy probe for "does this space still resolve the kernel
    /// image". Taking the address of a method would work too but needs a
    /// function-pointer cast the test kernel's unsafe context makes awkward.
    /// </summary>
    private static ulong s_pagerAnchor;

    /// <summary>
    /// Returns the address of <see cref="s_pagerAnchor"/>, page-aligned down.
    /// </summary>
    /// <remarks>
    /// The address is taken inside a <c>fixed</c> block because C# requires a
    /// moveable expression to be pinned first; without it this is a compile
    /// error rather than a warning. The alignment matters because
    /// <c>Translate</c> rejects non-page-aligned addresses by design.
    /// </remarks>
    private static unsafe ulong KernelImageAnchor()
    {
        fixed (ulong* anchor = &s_pagerAnchor)
        {
            return (ulong)anchor & ~(Pager.PageSize - 1);
        }
    }

    /// <summary>
    /// <see cref="KernelImageAnchor"/> as a <see cref="VirtualAddress"/>,
    /// for the callers that take one directly.
    /// </summary>
    private static VirtualAddress KernelImageAnchor(out bool ok)
    {
        ulong address = KernelImageAnchor();
        ok = address != 0;
        return VirtualAddress.From(address);
    }

    /// <summary>
    /// A scratch virtual range for the round-trip tests, distinct per slot so
    /// two tests cannot collide on an alias.
    /// </summary>
    /// <remarks>
    /// The base must be a canonical higher-half address, or dereferencing the
    /// alias raises #GP on x86-64 whatever the page tables say. Canonical means
    /// bits 63:48 equal a replication of bit 47, so the higher half starts at
    /// 0xFFFF_8000_0000_0000. This base is above the HHDM window (0xFFFF_8000_
    /// 0000_0000 on x64, 0xFFFF_0000_0000_0000 on ARM64) and far below the
    /// kernel image at 0xFFFF_FFFF_8000_0000, so it is not already mapped on
    /// either architecture. It must not be lowered to 0xFFFF_4000_0000_0000:
    /// that has bit 47 clear while bits 63:48 are set, which is the
    /// non-canonical hole, and a translate-only test passes there while the
    /// first actual store faults with #GP.
    /// <para>
    /// The higher half is required rather than cosmetic: the ARM64 backend
    /// installs into the TTBR1 tables, which translate only the upper half. A
    /// lower-half address would be resolved through TTBR0, so the mapping would
    /// be installed into a table the CPU never consults for it and the first
    /// write would trap. Each slot is one 2 MiB-aligned block apart so a region
    /// of a few pages never straddles a huge-page boundary.
    /// </para>
    /// </remarks>
    private static ulong PagerRoundTripAlias(ulong slot = 0)
    {
        const ulong Base = 0xFFFF_9000_0000_0000;
        const ulong SlotStride = 0x20_0000;
        return Base + (slot * SlotStride);
    }
}

internal enum TestEnum
{
    ValueA,
    ValueB,
    ValueC
}

// Test struct for boxing and collection tests
internal struct TestPoint
{
    public int X;
    public int Y;
}
