﻿using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class Int32Test
    {
        public static void Execute()
        {
            bool efuse;
            int value;
            string result;
            string expectedResult;

            value = Int32.MaxValue;

            result = value.ToString();
            expectedResult = "2147483647";

            Assert.IsTrue((result == expectedResult), "Int32.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an Int32 is " + value;
            expectedResult = "The Maximum value of an Int32 is 2147483647";

            Assert.IsTrue((result == expectedResult), "String concat (Int32) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an Int32 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Int32) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of an Int32 is the same value
            Assert.IsTrue((resultAsInt == value), "Int32.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif

            // basic bit operations

            int val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "Int32 bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "Int32 bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "Int32 bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "Int32 bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "Int32 left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "Int32 right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "Int32 addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "Int32 subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "Int32 multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "Int32 division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "Int32 remainder doesn't work got: " + val2);

            // Now test conversions

            int maxValue = Int32.MaxValue;
            int minValue = Int32.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for Int32 doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for Int32 doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for Int32 doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for Int32 doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == -0x0001, "Conv_I2 for Int32 doesn't work");
            Assert.IsTrue((short)minValue == 0x0000, "Conv_I2 for Int32 doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0xFFFF, "Conv_U2 for Int32 doesn't work");
            Assert.IsTrue((ushort)minValue == 0x0000, "Conv_U2 for Int32 doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == 0x7FFFFFFF, "Conv_I4 for Int32 doesn't work");
            Assert.IsTrue((int)minValue == -0x80000000, "Conv_I4 for Int32 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0x7FFFFFFF, "Conv_U4 for Int32 doesn't work");
            Assert.IsTrue((uint)minValue == 0x80000000, "Conv_U4 for Int32 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x000000007FFFFFFF, "Conv_I8 for Int32 doesn't work");
            Assert.IsTrue((long)minValue == -0x0000000080000000, "Conv_I8 for Int32 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x00000007FFFFFFF, "Conv_U8 for Int32 doesn't work");
            Assert.IsTrue((ulong)minValue == 0xFFFFFFFF80000000, "Conv_U8 for Int32 doesn't work");

            // Test Conv_R4
            Assert.IsTrue((float)maxValue == Int32.MaxValue, "Conv_R4 for Int32 doesn't work");
            Assert.IsTrue((float)minValue == Int32.MinValue, "Conv_R4 for Int32 doesn't work");

            // Test Conv_R8
            Assert.IsTrue((double)maxValue == Int32.MaxValue, "Conv_R8 for Int32 doesn't work");
            Assert.IsTrue((double)minValue == Int32.MinValue, "Conv_R8 for Int32 doesn't work");

            //Test checked conversions
            long val = 1;
            long test = 125;
            // Test Conv_Ovf_I4
            checked
            {
                Assert.IsTrue((int)test == 0x7D, "Conv_Ovf_I4 doesn't work(throws incorrectly)");
                Assert.IsTrue((int)(test - 1) == 124, "Conv_Ovf_I4 doesn't work(throws incorrectly)");
                Assert.IsTrue((int)val == 1, "Conv_Ovf_I4 doesn't work(throws incorrectly)");
                Assert.IsTrue((int)(2 * val) == 2, "Conv_Ovf_I4 doesn't work(throws incorrectly)");
                long x = 0;
                bool error = false;
                try
                {
                    x = (int)(val + int.MaxValue);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I4 doesn't work(error was not thrown): " + x);
                try
                {
                    x = (int)(val + int.MinValue - 2);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I4 doesn't work(error was not thrown): " + x);
            }


            // Test Conv_Ovf_I4_Un
            checked
            {
                Assert.IsTrue((int)(uint)test == 0x7D, "Conv_Ovf_I4_Un doesn't work(throws incorrectly)");
                int x = 0;
                bool error = false;
                try
                {
                    x = (int)(uint)(val + int.MaxValue);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I4_Un doesn't work(error was not thrown): " + x);
            }

            // Test Methods
            val2 = TestMethod(value);
            Assert.IsTrue(value == 60, "Passing an Int32 as a method parameter doesn't work");
            Assert.IsTrue(val2 == 61, "Returning an Int32 value from a method doesn't work");

            ByRefTestMethod(ref value);
            Assert.IsTrue(value == 61, "Passing an Int32 by ref to a method doesn't work");

            //Test Overflow Exceptions
            int val3o = 10000;
            efuse = false;
            try
            {
                checked
                {
                    val3o += 2147483647;
                }
            }
            catch (OverflowException)
            {
                efuse = true;
            }
            Assert.IsTrue(efuse, "Add_Ovf for Int32 doesn't work: " + val3o);

            efuse = false;
            val3o = -10000;
            try
            {
                checked
                {
                    val3o -= 2147483647;
                }
            }
            catch (OverflowException)
            {
                efuse = true;
            }
            Assert.IsTrue(efuse, "Sub_Ovf for Int32 doesn't work: " + val3o);
        }

        public static int TestMethod(int aParam)
        {
            aParam++;
            return aParam;
        }

        public static void ByRefTestMethod(ref int aParam)
        {
            aParam++;
        }
    }
}
