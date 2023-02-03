using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.ScanMaps;
using Cosmos.Core.Memory;
using Cosmos.Core;
using System.Runtime.InteropServices;

/*
 * Please note this is an atypical TestRunner:
 * - it is subverting the ring system (calling MemoryOperations from user ring!)
 * - it is slow because AreArrayEquals is slow (Fill and Copy are really faster actually)
 *
 * it exists to make easier tests while changing low level stuff (it would be better and faster to use the Demo kernel but
 * sometimes it is a problem to make it see modifications done at low level)
 *
 * Remember to comment this test again on TestKernelSets.cs when you are ready to merge your modifications!
 */
namespace MemoryOperationsTest
{
    public class Kernel : Sys.Kernel
    {
        protected override void BeforeRun()
        {
            Console.WriteLine("Cosmos booted successfully. Let's Test MemoryOperations!");
        }

        private void TestIntArrayCopy(int size)
        {
            int[] src = new int[size];
            int[] dst = new int[size];

            mDebugger.Send($"Start Copy of {size} integers");

            MemoryOperations.Fill(src, 42);

            mDebugger.Send("Copy Start");
            MemoryOperations.Copy(dst, src);
            mDebugger.Send("Copy End");

            Assert.AreEqual(src, dst, $"Copy failed Array src and dst with size {size} are not equals");

            mDebugger.Send("End");
        }

        private void TestByteArrayCopy(int size)
        {
            byte[] src = new byte[size];
            byte[] dst = new byte[size];

            mDebugger.Send($"Start Copy of {size} bytes");

            MemoryOperations.Fill(src, 42);

            mDebugger.Send("Copy Start");
            MemoryOperations.Copy(dst, src);
            mDebugger.Send("Copy End");

            Assert.AreEqual(src, dst, $"Copy failed Array src and dst with size {size} are not equals");

            mDebugger.Send("End");
        }

        private void TestCopy()
        {
            TestByteArrayCopy(8);
            TestByteArrayCopy(16);
            TestByteArrayCopy(24);
            TestByteArrayCopy(32);
            TestByteArrayCopy(48);
            TestByteArrayCopy(64);
            TestByteArrayCopy(80);
            TestByteArrayCopy(128);
            TestByteArrayCopy(256);
            TestByteArrayCopy(264);
            TestByteArrayCopy(1024 * 768); // XVGA resolution
            TestByteArrayCopy(1920 * 1080); // HDTV resolution

            TestIntArrayCopy(4);
            TestIntArrayCopy(16);
            TestIntArrayCopy(24);
            TestIntArrayCopy(32);
            TestIntArrayCopy(48);
            TestIntArrayCopy(64);
            TestIntArrayCopy(80);
            TestIntArrayCopy(128);
            TestIntArrayCopy(256);
            TestIntArrayCopy(264);
            TestIntArrayCopy(1024 * 768); // XVGA resolution
            TestIntArrayCopy(1920 * 1080); // HDTV resolution
        }
        static unsafe void TestManagedMemoryBlock(ManagedMemoryBlock memoryBlock)
        {
            memoryBlock.Write32(0, 1);
            Assert.AreEqual(1, memoryBlock[0], "ManagedMemoryBlock write int at index 0 works");
            Assert.AreEqual(1, memoryBlock.Read32(0), "ManagedMemoryBlock read/write at index 0 works");
            memoryBlock.Write32(1, 101);
            Assert.AreEqual(101, memoryBlock[1], "ManagedMemoryBlock read/write at index 1 works");
            Assert.AreEqual(25857, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            memoryBlock.Write32(2, 2 ^ 16 + 2);
            Assert.AreEqual(16, memoryBlock[2], "ManagedMemoryBlock write int at index 2 works");
            Assert.AreEqual(0, memoryBlock[3], "ManagedMemoryBlock write int at index 2 works");
            Assert.AreEqual(1074433, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            memoryBlock.Write32(3, int.MaxValue);
            Assert.AreEqual(255, memoryBlock[3], "ManagedMemoryBlock write int at index 3 works");
            Assert.AreEqual(0xFF106501, memoryBlock.Read32(0), "ManagedMemoryBlock read int at index 0 works");
            Assert.AreEqual(0xFFFF1065, memoryBlock.Read32(1), "ManagedMemoryBlock read int at index 1 works");
            Assert.AreEqual(0xFFFFFF10, memoryBlock.Read32(2), "ManagedMemoryBlock read int at index 2 works");
            Assert.AreEqual(int.MaxValue, memoryBlock.Read32(3), "ManagedMemoryBlock read/write at index 3 works");

            memoryBlock.Fill(101);
            Assert.AreEqual(101, memoryBlock.Read32(0), "ManagedMemoryBlock fill works at index 0");
            Assert.AreEqual(0, memoryBlock[1], "ManagedMemoryBlock fill fills entire ints");
            Assert.AreEqual(6619136, memoryBlock.Read32(10), "ManagedMemoryBlock fill works at index 10");

            memoryBlock.Write8(0, 101);
            Assert.AreEqual(101, memoryBlock[0], "ManagedMemoryBlock write byte works at index 0");
            memoryBlock.Fill(1, 1, 987893745);
            Assert.AreEqual(101, memoryBlock[0], "ManagedMemoryBlock Fill(1, int, int) skips index 0");
            Assert.AreEqual(987893745, memoryBlock.Read32(1), "ManagedMemoryBlock Fill(int, int, int) works at index 1");
        }

        static unsafe void TestMemoryBlock(MemoryBlock memoryBlock)
        {
            uint[] values = new uint[] { 1, 101, 2 ^ 16 + 2, int.MaxValue };
            memoryBlock.Write32(values);
            uint[] read = new uint[4];
            memoryBlock.Read32(read);
            for (int i = 0; i < 4; i++)
            {
                if(values[i] != read[i])
                {
                    Assert.Fail($"Values read differ at {i}. Expected: {values[i]} Actual: {read[i]}");
                }
            }
            Assert.Succeed("Writing and reading uints works");
            byte* ptr = (byte*)memoryBlock.Base;
            Assert.AreEqual(1, *ptr, "Expected 1 in first byte of memory block when checking using pointer");
            Assert.AreEqual(0, *(ptr + 3), "Expected 0 in fourth byte of memory block when checking using pointer");
            byte[] valueBytes = new byte[] { 1, 0, 0, 0 };
            byte[] readByte = new byte[4];
            memoryBlock.Read8(readByte);
            Assert.AreEqual(valueBytes, readByte, "Reading bytes works");
            valueBytes[0] = 65;
            valueBytes[1] = 127;
            memoryBlock.Write8(valueBytes);
            memoryBlock.Read8(readByte);
            Assert.AreEqual(valueBytes, readByte, "Writing bytes works");
            memoryBlock.Fill(101);
            memoryBlock.Read8(readByte);
            Assert.AreEqual(new byte[] { 101, 101, 101, 101 }, readByte, "Filling works");
            values = new uint[] { 0x65656565, 987893745, 0x65656565, 0x65656565 };
            memoryBlock.Fill(4, 1, 987893745);
            memoryBlock.Read32(read);
            Assert.AreEqual(values, read, "Using Fill(int, int, int) works");
        }
        static unsafe void TestRealloc()
		{
            // Allocate initial pointer and fill with value 32
            byte* aPtr = Heap.Alloc(16);
            MemoryOperations.Fill(aPtr, (byte)32, 16);

            // Resize/realloc to 17 bytes
            aPtr = Heap.Realloc(aPtr, 17);

            // Test for first 16 being 32 and last being 0
            for (int i = 0; i < 15; i++)
			{
                Assert.AreEqual(aPtr[i], 32, $"Expected value 32 not found in index {i} of aPtr.");
			}
            Assert.AreEqual(aPtr[16], 0, "Expected value 0 not found at the end of aPtr.");
		}

        protected override void Run()
        {
            try
            {
                TestCopy();
                TestMemoryBlock(new MemoryBlock(0x60000, 128)); //we are testing in SVGA video memory which should not be in use
                TestManagedMemoryBlock(new ManagedMemoryBlock(128));
                TestRealloc();
                SpanTest.Execute();
                TestController.Completed();
            }
            catch (Exception e)
            {
                mDebugger.Send("Exception occurred: " + e.Message);
                mDebugger.Send(e.Message);
                TestController.Failed();
            }
        }
    }
}
