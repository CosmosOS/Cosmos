using System;
using Sys = Cosmos.System;
using Cosmos.TestRunner;
using Cosmos.System.Graphics;
using System.Text;
using Cosmos.System.ExtendedASCII;
using Cosmos.System.ScanMaps;
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

        /*
         * It checks 32 byte any time to make it more faster, for now we need unsafe to do this
         */ 
        public static unsafe bool AreArrayEquals(byte* b0, byte* b1, int length)
        {
            byte* lastAddr = b0 + length;
            byte* lastAddrMinus32 = lastAddr - 32;
            while (b0 < lastAddrMinus32) // unroll the loop so that we are comparing 32 bytes at a time.
            {
                if (*(ulong*)b0 != *(ulong*)b1)
                    return false;
                if (*(ulong*)(b0 + 8) != *(ulong*)(b1 + 8))
                    return false;
                if (*(ulong*)(b0 + 16) != *(ulong*)(b1 + 16))
                    return false;
                if (*(ulong*)(b0 + 24) != *(ulong*)(b1 + 24))
                    return false;
                b0 += 32;
                b1 += 32;
            }
            while (b0 < lastAddr)
            {
                if (*b0 != *b1)
                    return false;
                b0++;
                b1++;
            }
            return true;
        }

        public static unsafe bool AreArrayEquals(byte[] arr0, byte[] arr1)
        {
            fixed (byte* b0 = arr0, b1 = arr1)
            {
                return b0 == b1 || AreArrayEquals(b0, b1, arr0.Length);
            }
        }

        public static unsafe bool AreArrayEquals(int[] arr0, int[] arr1)
        {
            int lenght = arr0.Length * 4;
            fixed (int* b0 = arr0, b1 = arr1)
            {
                return b0 == b1 || AreArrayEquals((byte *)b0, (byte*)b1, lenght);
            }
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

            Assert.IsTrue(AreArrayEquals(src, dst), $"Copy failed Array src and dst with size {size} are not equals");

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

            Assert.IsTrue(AreArrayEquals(src, dst), $"Copy failed Array src and dst with size {size} are not equals");

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

        protected override void Run()
        {
            try
            {
                TestCopy();

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
