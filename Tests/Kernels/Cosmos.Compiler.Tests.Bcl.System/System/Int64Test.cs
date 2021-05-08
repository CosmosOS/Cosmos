﻿using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class Int64Test
    {
        public static void Execute()
        {
            bool efuse;
            long value;
            string result;
            string expectedResult;

            value = long.MaxValue;

            result = value.ToString();
            expectedResult = "9223372036854775807";
            Assert.IsTrue((result == expectedResult), "Int64.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an Int64 is " + value;
            expectedResult = "The Maximum value of an Int64 is 9223372036854775807";

            Assert.IsTrue((result == expectedResult), "String concat (Int64) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an Int64 is {value}";

            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Int64) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Int64 is the value interpolated with XOR to obtain an Int32... so not the same of 'value'!
            int expectedResultAsInt = (unchecked((int)((long)value)) ^ (int)(value >> 32));

            Assert.IsTrue((resultAsInt == expectedResultAsInt), "Int64.GetHashCode() doesn't work");

            value = long.Parse("42");
            Assert.IsTrue(value == 42, "Parsing Int64 doesn't work.");

#if false

            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFFFFFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int64.ToString(X2) doesn't work");
#endif

            // basic bit operations

            long val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "Int64 bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "Int64 bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "Int64 bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "Int64 bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "Int64 left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "Int64 right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "Int64 addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "Int64 subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "Int64 multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "Int64 division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "Int64 remainder doesn't work got: " + val2);

            value = 1728000000000;

            val2 = value + 36000000000;
            Assert.IsTrue(val2 == 1764000000000, "Int64 addition doesn't work got " + val2);

            val2 = value - 36000000000;
            Assert.IsTrue(val2 == 1692000000000, "Int64 subtraction doesn't work got " + val2);

            val2 = value * 36000000000;
            Assert.IsTrue(val2 == 5578983451391950848, "Int64 multiplication doesn't work got " + val2);

            val2 = value / 36000000000;
            Assert.IsTrue(val2 == 48, "Int64 division doesn't work got " + val2);

            val2 = value / -36000000000;
            Assert.IsTrue(val2 == -48, "Int64 division doesn't work got " + val2);

            val2 = -value / 36000000000;
            Assert.IsTrue(val2 == -48, "Int64 division doesn't work got " + val2);

            val2 = -value / -36000000000;
            Assert.IsTrue(val2 == 48, "Int64 division doesn't work got " + val2);

            value = 3200000000000;

            val2 = value % 1300000000000;
            Assert.IsTrue(val2 == 600000000000, "Int64 remainder doesn't work got " + val2);

            val2 = value % -1300000000000;
            Assert.IsTrue(val2 == 600000000000, "Int64 remainder doesn't work got " + val2);

            val2 = -value % 1300000000000;
            Assert.IsTrue(val2 == -600000000000, "Int64 remainder doesn't work got " + val2);

            val2 = -value % -1300000000000;
            Assert.IsTrue(val2 == -600000000000, "Int64 remainder doesn't work got " + val2);

            // Now test conversions

            long maxValue = Int64.MaxValue;
            long minValue = Int64.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for Int64 doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for Int64 doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for Int64 doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for Int64 doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == -0x0001, "Conv_I2 for Int64 doesn't work");
            Assert.IsTrue((short)minValue == 0x0000, "Conv_I2 for Int64 doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0xFFFF, "Conv_U2 for Int64 doesn't work");
            Assert.IsTrue((ushort)minValue == 0x0000, "Conv_U2 for Int64 doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == -0x00000001, "Conv_I4 for Int64 doesn't work");
            Assert.IsTrue((int)minValue == 0x00000000, "Conv_I4 for Int64 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0xFFFFFFFF, "Conv_U4 for Int64 doesn't work");
            Assert.IsTrue((uint)minValue == 0x00000000, "Conv_U4 for Int64 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x7FFFFFFFFFFFFFFF, "Conv_I8 for Int64 doesn't work");
            Assert.IsTrue((long)minValue == -0x8000000000000000, "Conv_I8 for Int64 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x7FFFFFFFFFFFFFFF, "Conv_U8 for Int64 doesn't work");
            Assert.IsTrue((ulong)minValue == 0x8000000000000000, "Conv_U8 for Int64 doesn't work");

            // Test Conv_R4
            Assert.IsTrue((float)maxValue == Int64.MaxValue, "Conv_R4 for Int64 doesn't work");
            Assert.IsTrue((float)minValue == Int64.MinValue, "Conv_R4 for Int64 doesn't work");

            // Test Conv_R8
            Assert.IsTrue((double)maxValue == Int64.MaxValue, "Conv_R8 for Int64 doesn't work");
            Assert.IsTrue((double)minValue == Int64.MinValue, "Conv_R8 for Int64 doesn't work");

            //Test checked conversions
            long val = 1;

            // Test Conv_Ovf_I8_Un
            checked
            {
                Assert.IsTrue((long)(ulong)125 == 0x7D, "Conv_Ovf_I8_Un doesn't work(throws incorrectly)");
                long x = 0;
                bool error = false;
                try
                {
                    x = (long)((ulong)val + ulong.MaxValue - 1);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I8_Un doesn't work(error was not thrown): " + x);
            }


            // Test Methods

            value = 60;

            val2 = TestMethod(value);
            Assert.IsTrue(value == 60, "Passing an Int64 as a method parameter doesn't work");
            Assert.IsTrue(val2 == 61, "Returning an Int64 value from a method doesn't work");

            ByRefTestMethod(ref value);
            Assert.IsTrue(value == 61, "Passing an Int64 by ref to a method doesn't work");

            //Test Overflow Exceptions
            long val3o = 1000000;
            efuse = false;
            try
            {
                checked
                {
                    val3o += long.MaxValue;
                }
            }
            catch (OverflowException)
            {
                efuse = true;
            }
            Assert.IsTrue(efuse, "Add_Ovf for Int64 doesn't work");

            efuse = false;
            val3o = -10000;
            try
            {
                checked
                {
                    val3o -= long.MaxValue;
                }
            }
            catch (OverflowException)
            {
                efuse = true;
            }
            Assert.IsTrue(efuse, "Sub_Ovf for Int64 doesn't work");
        }

        public static long TestMethod(long aParam)
        {
            aParam++;
            return aParam;
        }

        public static void ByRefTestMethod(ref long aParam)
        {
            aParam++;
        }
    }
}
