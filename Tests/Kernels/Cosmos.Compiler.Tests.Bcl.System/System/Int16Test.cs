﻿using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class Int16Test
    {
        public static void Execute()
        {
            short value;
            string result;
            string expectedResult;

            value = Int16.MaxValue;

            result = value.ToString();
            expectedResult = "32767";

            Assert.IsTrue((result == expectedResult), "Int16.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an Int16 is " + value;
            expectedResult = "The Maximum value of an Int16 is 32767";

            Assert.IsTrue((result == expectedResult), "String concat (Int16) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an Int16 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Int16) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Int16 is some strange XOR trick
            int expectedResultAsInt = ((int)((ushort)value) | (((int)value) << 16));

            Assert.IsTrue((resultAsInt == expectedResultAsInt), "Int16.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "7FFF";

            Assert.IsTrue((result == expectedResult), "Int16.ToString(X2) doesn't work");
#endif

            // basic bit operations

            int val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "Int16 bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "Int16 bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "Int16 bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "Int16 bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "Int16 left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "Int16 right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "Int16 addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "Int16 subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "Int16 multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "Int16 division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "Int16 remainder doesn't work got: " + val2);

            // Now test conversions

            short maxValue = Int16.MaxValue;
            short minValue = Int16.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for Int16 doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for Int16 doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for Int16 doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for Int16 doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == 0x7FFF, "Conv_I2 for Int16 doesn't work");
            Assert.IsTrue((short)minValue == -0x8000, "Conv_I2 for Int16 doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0x7FFF, "Conv_U2 for Int16 doesn't work");
            Assert.IsTrue((ushort)minValue == 0x8000, "Conv_U2 for Int16 doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == 0x00007FFF, "Conv_I4 for Int16 doesn't work");
            Assert.IsTrue((int)minValue == -0x00008000, "Conv_I4 for Int16 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0x00007FFF, "Conv_U4 for Int16 doesn't work");
            Assert.IsTrue((uint)minValue == 0xFFFF8000, "Conv_U4 for Int16 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x0000000000007FFF, "Conv_I8 for Int16 doesn't work");
            Assert.IsTrue((long)minValue == -0x0000000000008000, "Conv_I8 for Int16 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x0000000000007FFF, "Conv_U8 for Int16 doesn't work");
            Assert.IsTrue((ulong)minValue == 0xFFFFFFFFFFFF8000, "Conv_U8 for Int16 doesn't work");

            // Test Conv_R4
            Assert.IsTrue((float)maxValue == Int16.MaxValue, "Conv_R4 for Int16 doesn't work");
            Assert.IsTrue((float)minValue == Int16.MinValue, "Conv_R4 for Int16 doesn't work");

            // Test Conv_R8
            Assert.IsTrue((double)maxValue == Int16.MaxValue, "Conv_R8 for Int16 doesn't work");
            Assert.IsTrue((double)minValue == Int16.MinValue, "Conv_R8 for Int16 doesn't work");

            //Test checked conversions
            int val = 1;
            long test = 125;

            // Test Conv_Ovf_I2
            checked
            {
                Assert.IsTrue((short)val == 1, "Conv_Ovf_I2 doesn't work(throws incorrectly)");
                short x = 0;
                bool error = false;
                try
                {
                    x = (short)(val + short.MaxValue);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I2 doesn't work(error was not thrown): " + x);
                Assert.IsTrue((short)test == 0x7D, "Conv_Ovf_I2 for long to short doesn't work(throws incorrectly)");
                error = false;
                try
                {
                    x = (short)(val + 0x8_0000_0000);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I2 for from positive long to short doesn't work(error was not thrown): " + x);
                error = false;
                try
                {
                    x = (short)(val + -0x8_0000_0001);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I2 for from negative long to short doesn't work(error was not thrown): " + x);
            }


            // Test Conv_Ovf_I2_Un
            checked
            {
                Assert.IsTrue((short)(uint)125 == 0x7D, "Conv_Ovf_I2_Un doesn't work(throws incorrectly)");
                short x = 0;
                bool error = false;
                try
                {
                    x = (short)(uint)(val + short.MaxValue);
                }
                catch (Exception)
                {
                    error = true;
                }
                Assert.IsTrue(error, "Conv_Ovf_I2_Un doesn't work(error was not thrown): " + x);
            }

            // Test Methods
            val2 = TestMethod(value);
            Assert.IsTrue(value == 60, "Passing an Int16 as a method parameter doesn't work");
            Assert.IsTrue(val2 == 61, "Returning an Int16 value from a method doesn't work");

            ByRefTestMethod(ref value);
            Assert.IsTrue(value == 61, "Passing an Int16 by ref to a method doesn't work");
        }

        public static short TestMethod(short aParam)
        {
            aParam++;
            return aParam;
        }

        public static void ByRefTestMethod(ref short aParam)
        {
            aParam++;
        }
    }
}
