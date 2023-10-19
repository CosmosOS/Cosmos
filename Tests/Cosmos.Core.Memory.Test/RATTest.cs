using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test
{
    [TestClass]
    public class MemoryTests
    {


        [TestMethod]
        public unsafe void InitTest()
        {
            var xRAM = new byte[128 * 1024 * 1024]; // 128 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);

                Assert.IsTrue(HeapSmall.mMaxItemSize > 512);

                uint xRatPages = RAT.GetPageCount((byte)RAT.PageType.RAT);
                Assert.IsTrue(xRatPages > 0);

                var xFreePages = RAT.GetPageCount((byte)RAT.PageType.Empty);
                Assert.IsTrue(xFreePages > 0);

                Assert.IsTrue(RAT.GetPageCount((byte)RAT.PageType.HeapSmall) > 0);

                Assert.AreEqual(0, HeapSmall.GetAllocatedObjectCount());

                Assert.AreEqual((uint)8, RAT.GetPageCount((byte)RAT.PageType.RAT));
            }
        }

        [TestMethod]
        public unsafe void RATMethods()
        {
            var xRAM = new byte[1024 * 1024];
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;

                RAT.Init(xPtr, (uint)xRAM.Length);

                uint freePageCount = RAT.FreePageCount;
                Assert.IsTrue(freePageCount < RAT.TotalPageCount);
                Assert.AreEqual(freePageCount, RAT.GetPageCount((byte)RAT.PageType.Empty));

                var largePage = RAT.AllocPages(RAT.PageType.HeapLarge, 3);
                Assert.AreEqual(RAT.PageType.HeapLarge, RAT.GetPageType(largePage));
                Assert.AreEqual(RAT.GetFirstRATIndex(largePage), RAT.GetFirstRATIndex((byte*)largePage + 20));
                Assert.AreEqual(RAT.PageType.HeapLarge, RAT.GetPageType((byte*)largePage + RAT.PageSize));

                Assert.AreEqual(RAT.FreePageCount, freePageCount - 3);
            }
        }

        [TestMethod]
        public unsafe void SmallAllocTest()
        {
            var xRAM = new byte[32 * 1024 * 1024]; // 32 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;

                RAT.Init(xPtr, (uint)xRAM.Length);

                uint smallPages = RAT.GetPageCount((byte)RAT.PageType.HeapSmall);
                uint largePages = RAT.GetPageCount((byte)RAT.PageType.HeapLarge);

                // the following allocations should all go on the same page
                var ptr1 = Heap.Alloc(8);
                var ptr2 = Heap.Alloc(3);
                ptr2[0] = 12;
                ptr2[1] = 101;
                var ptr3 = Heap.Alloc(8);
                var ptr4 = Heap.Alloc(20);
                var ptr5 = Heap.Alloc(22);
                Assert.AreNotEqual((uint)ptr1, (uint)ptr2);
                Assert.AreEqual((uint)ptr2 - (uint)ptr1, (uint)ptr3 - (uint)ptr2);
                Assert.AreEqual(24 + HeapSmall.PrefixItemBytes, (uint)ptr5 - (uint)ptr4);
                Assert.AreEqual(RAT.PageSize, (uint)ptr4 - (uint)ptr1);
                Assert.AreEqual(12, ptr2[0]);
                Assert.AreEqual(smallPages, RAT.GetPageCount((byte)RAT.PageType.HeapSmall));
                Assert.AreEqual(largePages, RAT.GetPageCount((byte)RAT.PageType.HeapLarge));
                Assert.AreEqual(5, HeapSmall.GetAllocatedObjectCount());

                Heap.Free(ptr2);
                Assert.AreEqual(4, HeapSmall.GetAllocatedObjectCount());
                Assert.AreEqual(0, ptr2[0]);
                var nptr2 = Heap.Alloc(10);
                Heap.Alloc(10);
                Assert.AreEqual((uint)ptr2, (uint)nptr2); // we use the earliest free position
                Assert.AreEqual(6, HeapSmall.GetAllocatedObjectCount());
            }
        }

        [TestMethod]
        public unsafe void SmallHeapMultiPageAllocationTest()
        {
            var xRAM = new byte[1024 * 1024]; // 4 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;

                RAT.Init(xPtr, (uint)xRAM.Length);

                uint smallPages = RAT.GetPageCount((byte)RAT.PageType.HeapSmall);


                var ptr1 = Heap.Alloc(HeapSmall.mMaxItemSize); // 4 of them should fit on one page
                var ptr2 = Heap.Alloc(HeapSmall.mMaxItemSize);
                var ptr3 = Heap.Alloc(HeapSmall.mMaxItemSize);
                var ptr4 = Heap.Alloc(HeapSmall.mMaxItemSize);
                Assert.AreEqual((uint)ptr2 - (uint)ptr1, (uint)ptr4 - (uint)ptr3);
                Assert.AreEqual((uint)RAT.GetPagePtr(ptr1), (uint)RAT.GetPagePtr(ptr2));
                Assert.AreEqual(4, HeapSmall.GetAllocatedObjectCount());
                Heap.Free(ptr4);
                var nptr4 = Heap.Alloc(HeapSmall.mMaxItemSize);
                Assert.AreEqual((uint)RAT.GetPagePtr(ptr1), (uint)RAT.GetPagePtr(ptr4));
                var ptr5 = Heap.Alloc(HeapSmall.mMaxItemSize); // this should cause a new page to have to be created

                uint largePages = RAT.GetPageCount((byte)RAT.PageType.HeapLarge);
                Assert.AreEqual((uint)0, largePages);
                Assert.AreEqual(smallPages + 1, RAT.GetPageCount((byte)RAT.PageType.HeapSmall));
                Assert.AreNotEqual((uint)ptr1, (uint)ptr5);
                Assert.IsTrue(((uint)ptr5 - (uint)ptr1) % RAT.PageSize == 0);
                Assert.AreEqual(5, HeapSmall.GetAllocatedObjectCount());


                // now lets force them to allocate 2 more pages
                for (int i = 0; i < 8; i++)
                {
                    Heap.Alloc(HeapSmall.mMaxItemSize - 2);
                }
                Assert.AreEqual(smallPages + 3, RAT.GetPageCount((byte)RAT.PageType.HeapSmall));
                Assert.AreEqual(13, HeapSmall.GetAllocatedObjectCount());

            }
        }

        [TestMethod]
        public unsafe void SmallHeapTestExpansion()
        {
            var xRAM = new byte[1024 * 1024]; // 4 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);

                uint smallPages = RAT.GetPageCount((byte)RAT.PageType.HeapSmall);

                for (int i = 0; i < 9; i++)
                {
                    Heap.Alloc(600);
                }

                Assert.AreEqual(smallPages + 2, RAT.GetPageCount((byte)RAT.PageType.HeapSmall));
            }
        }

        [TestMethod]
        public unsafe void SmallHeapStressTest() // this test is important since it tests that the SMT table can grow to multiple pages
        {
            var xRAM = new byte[1024 * 1024]; // 4 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);


                Random random = new Random();

                for (int i = 0; i < 1000; i++)
                {
                    if (Heap.Alloc((uint)random.Next(4, (int)HeapSmall.mMaxItemSize)) == null)
                    {
                        Assert.Fail();
                    }
                }
                Assert.AreEqual(1000, HeapSmall.GetAllocatedObjectCount());
            }
        }

        [TestMethod]
        public unsafe void MediumLargeHeapTest() // as long as medium just does the same as the large heap, we can test them together
        {
            var xRAM = new byte[1024 * 1024]; // 4 MB
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);

                var largeCount = RAT.GetPageCount((byte)RAT.PageType.HeapLarge);
                var mediumCount = RAT.GetPageCount((byte)RAT.PageType.HeapMedium);

                var ptr1 = Heap.Alloc(HeapMedium.MaxItemSize); // this will allocate two pages,
                                                               // since we simplify the math by assuming we never want only one full page
                var ptr2 = Heap.Alloc(HeapMedium.MaxItemSize - 10);
                var ptr3 = Heap.Alloc(HeapMedium.MaxItemSize + 10);

                Assert.AreEqual(largeCount + 2, RAT.GetPageCount((byte)RAT.PageType.HeapLarge));
                Assert.AreEqual(mediumCount + 3, RAT.GetPageCount((byte)RAT.PageType.HeapMedium));

                var ptr4 = Heap.Alloc(RAT.PageSize * 5 - HeapLarge.PrefixBytes - 1);

                Assert.AreEqual(largeCount + 7, RAT.GetPageCount((byte)RAT.PageType.HeapLarge));
                Assert.AreEqual(6, (int)RAT.GetPageCount((byte)RAT.PageType.Extension));

                Heap.Free(ptr4);
                Assert.AreEqual(largeCount + 2, RAT.GetPageCount((byte)RAT.PageType.HeapLarge));
                Assert.AreEqual(2, (int)RAT.GetPageCount((byte)RAT.PageType.Extension));

                Heap.Free(ptr1);
                Heap.Free(ptr2);

                Assert.AreEqual(mediumCount, RAT.GetPageCount((byte)RAT.PageType.HeapMedium));
                Assert.AreEqual(largeCount + 2, RAT.GetPageCount((byte)RAT.PageType.HeapLarge));
            }
        }

        unsafe void FillRandom(byte* ptr, uint aSize) // fake new obj and fill object with something
        {
            Random random = new Random();
            for (int i = 0; i < aSize; i++)
            {
                ptr[i] = (byte)random.Next(16, 32);
            }
        }

        [TestMethod]
        public unsafe void TestAllocPages() // ensure that we fail gracefully when memory gets full
        {
            var xRAM = new byte[10 * RAT.PageSize]; // 10 Pages - 1 for RAT and 9 for values
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);
                Assert.AreEqual((uint)1, RAT.GetPageCount((byte)RAT.PageType.RAT));

                try
                {
                    for (int i = 0; i < 10000; i++)
                    {
                        byte* ptr = Heap.Alloc(40);
                        FillRandom(ptr, 40);
                        if (ptr == null)
                        {
                            Assert.Fail();
                        }
                        Assert.AreEqual((uint)1, RAT.GetPageCount((byte)RAT.PageType.RAT));
                    }

                }
                catch (Exception e)
                {
                    Assert.AreEqual("289", e.Message);
                }

            }
        }

        [TestMethod]
        public unsafe void TestRATHeapMethods()
        {
            var xRAM = new byte[10 * RAT.PageSize]; // 10 Pages - 1 for RAT and 9 for values
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                RAT.Init(xPtr, (uint)xRAM.Length);

                var ptr1 = Heap.Alloc(10);
                var ptr2 = Heap.Alloc(10);
                var ptr3 = Heap.Alloc(10);
                Assert.AreNotEqual((uint)ptr1, (uint)ptr2);
                Assert.AreNotEqual((uint)ptr1, (uint)ptr3);
                Assert.AreNotEqual((uint)ptr2, (uint)ptr3);
                Assert.AreEqual(RAT.GetFirstRATIndex(ptr1), RAT.GetFirstRATIndex(ptr2));
                Assert.AreEqual(RAT.GetFirstRATIndex(ptr1), RAT.GetFirstRATIndex(ptr3));
                Assert.AreEqual((uint)RAT.GetPagePtr(ptr1), (uint)RAT.GetPagePtr(ptr2));
                Assert.AreEqual((uint)RAT.GetPagePtr(ptr1), (uint)RAT.GetPagePtr(ptr3));
            }
        }
    }
}
