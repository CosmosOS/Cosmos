using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.TestRunner;
using System.Runtime.CompilerServices;

namespace Cosmos.Compiler.Tests.Bcl.System
{

    unsafe class UnsafeCodeTest
    {

        static ulong Test(ref ulong a)
        {
            a = 12345678;
            return a;
        }

        private static unsafe bool StartsWithOrdinal(char* source, uint sourceLength, string value)
        {
            if (sourceLength < (uint)value.Length) return false;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] != source[i]) return false;
            }
            return true;
        }

        public static unsafe void TestFixed()
        {
            String pathAsString = @"0:\DiTest";
            int pathLength = pathAsString.Length;
            bool val1;
            fixed (char* path = pathAsString)
            {
                val1 = StartsWithOrdinal(path, (uint)pathLength, "\\\\?\\");
            }

            Assert.IsTrue(val1 == false, "Path is not UNC path");
        }

        public static void Execute()
        {
            long val = Int64.MaxValue;
            long* p = &val;
            long newVal = *p;
            Assert.IsTrue(val == newVal, $"Pointer deferencing failed expected {val} got {newVal}");

            /* This the same thing BitConverter does to convert a Double to a Long it should fail in this case too */
            double d = 1.0;
            long asLong = *(long*)(&d);

            byte[] hexDump;
            hexDump = BitConverter.GetBytes(asLong);

            Assert.IsTrue(asLong == 0x3FF0000000000000, "double asLong is wrong!");

            ulong aLong = 0;
            ulong retVal;
            retVal = Test(ref aLong);

            Assert.IsTrue(retVal == 12345678, "Ulong ref passing not works");

            TestFixed();
            TestCompilerServices();
            TestStackAlloc();
        }

        private static unsafe void TestCompilerServices()
        {
            Assert.AreEqual(1, Unsafe.SizeOf<byte>(), "Unsafe.SizeOf works for byte");
            Assert.AreEqual(2, Unsafe.SizeOf<char>(), "Unsafe.SizeOf works for char");
            Assert.AreEqual(2, Unsafe.SizeOf<ushort>(), "Unsafe.SizeOf works for ushort");
            Assert.AreEqual(4, Unsafe.SizeOf<int>(), "Unsafe.SizeOf works for int");
            Assert.AreEqual(8, Unsafe.SizeOf<long>(), "Unsafe.SizeOf works for long");
        }

        private static void TestStackAlloc()
        {
            Span<uint> uintArr = stackalloc uint[16];
            uintArr[0] = 10;
            Assert.AreEqual(10, uintArr[0], "Storing and reading from stackalloc allocated array works");
            uintArr[3] = 2;
            Assert.AreEqual(2, uintArr[3], "Storing and reading from stackalloc allocated array works");
            Span<uint> uintArr2 = stackalloc uint[16];
            Assert.AreEqual(0, uintArr2[0], "stackallocing a second span does not share memory");
            Assert.AreEqual(0, uintArr2[3], "stackallocing a second span does not share memory");
            uintArr2[0] = 3;
            Assert.AreEqual(3, uintArr2[0], "stackallocing a second span does not share memory and we can set the value in the second");
            Assert.AreEqual(10, uintArr[0], "stackallocing a second span does really does not share memory");
            Assert.AreEqual(2, uintArr[3], "stackallocing a second span does really does not share memory");
            Assert.AreNotEqual((uint)uintArr.GetPinnableReference(), (uint)uintArr2.GetPinnableReference(), "GetPinnableReference returns different values for both spans");

        }
    }
}
