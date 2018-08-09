using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Native = System.UInt32;

namespace Cosmos.Core.Memory.Test
{
    [TestClass]
    public class MemoryTests
    {
        [TestMethod]
        public unsafe void OldHeapTest()
        {
            var xRAM = new byte[128 * 1024 * 1024]; // 128 MB
            xRAM[0] = 1;
            fixed (byte* xPtr = xRAM)
            {
            }
            Assert.IsTrue(true);
        }

        [TestMethod]
        public unsafe void RATTest()
        {
            var xRAM = new byte[128 * 1024 * 1024]; // 128 MB
            xRAM[0] = 1;
            fixed (byte* xPtr = xRAM)
            {
                RAT.Debug = true;
                //RAT.Init(xPtr, (Native)xRAM.LongLength);
                RAT.Init(xPtr, (Native) xRAM.Length);

                Native xRatPages = RAT.GetPageCount(RAT.PageType.RAT);
                Assert.IsTrue(xRatPages > 0);

                var xFreePages = RAT.GetPageCount(RAT.PageType.Empty);
                Assert.IsTrue(xFreePages > 0);

                var x1 = (Int32*) Heap.Alloc(sizeof(Int32));
                var xFreePages2 = RAT.GetPageCount(RAT.PageType.Empty);
                Assert.IsTrue(xFreePages - xFreePages2 == 1);
                //
                Heap.Free(x1);
                var xFreePages3 = RAT.GetPageCount(RAT.PageType.Empty);
                Assert.IsTrue(xFreePages3 == xFreePages2 + 1);
            }
        }
    }
}
