using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class ByteTest
    {
        public static void Execute()
        {
            byte value;
            string result;
            string expectedResult;

            value = Byte.MaxValue;

            result = value.ToString();
            expectedResult = "255";

            Assert.IsTrue((result == expectedResult), "Byte.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of a Byte is " + value;
            expectedResult = "The Maximum value of a Byte is 255";

            Assert.IsTrue((result == expectedResult), "String concat (Byte) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of a Byte is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Byte) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Byte is the same value expressed as int
            Assert.IsTrue((resultAsInt == value), "Byte.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FF";

            Assert.IsTrue((result == expectedResult), "Byte.ToString(X2) doesn't work");
#endif

            // basic bit operations

            int val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "Byte bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "Byte bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "Byte bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "Byte bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "Byte left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "Byte right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "Byte addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "Byte subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "Byte multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "Byte division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "Byte remainder doesn't work got: " + val2);

            // Now test conversions

            byte maxValue = Byte.MaxValue;
            byte minValue = Byte.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for Byte doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for Byte doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for Byte doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for Byte doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == 0x00FF, "Conv_I2 for Byte doesn't work");
            Assert.IsTrue((short)minValue == 0x0000, "Conv_I2 for Byte doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0x00FF, "Conv_U2 for Byte doesn't work");
            Assert.IsTrue((ushort)minValue == 0x0000, "Conv_U2 for Byte doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == 0x000000FF, "Conv_I4 for Byte doesn't work");
            Assert.IsTrue((int)minValue == 0x00000000, "Conv_I4 for Byte doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0x000000FF, "Conv_U4 for Byte doesn't work");
            Assert.IsTrue((uint)minValue == 0x00000000, "Conv_U4 for Byte doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x00000000000000FF, "Conv_I8 for Byte doesn't work");
            Assert.IsTrue((long)minValue == 0x0000000000000000, "Conv_I8 for Byte doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x00000000000000FF, "Conv_U8 for Byte doesn't work");
            Assert.IsTrue((ulong)minValue == 0x0000000000000000, "Conv_U8 for Byte doesn't work");

            // Test Conv_R4
            Assert.IsTrue((float)maxValue == Byte.MaxValue, "Conv_R4 for Byte doesn't work" + (float)maxValue);
            Assert.IsTrue((float)minValue == Byte.MinValue, "Conv_R4 for Byte doesn't work");

            // Test Conv_R8
            Assert.IsTrue((double)maxValue == Byte.MaxValue, "Conv_R8 for Byte doesn't work");
            Assert.IsTrue((double)minValue == Byte.MinValue, "Conv_R8 for Byte doesn't work");

            // Test Methods
            val2 = TestMethod(value);
            Assert.IsTrue(value == 60, "Passing a Byte as a method parameter doesn't work");
            Assert.IsTrue(val2 == 61, "Returning a Byte value from a method doesn't work");

            ByRefTestMethod(ref value);
            Assert.IsTrue(value == 61, "Passing a Byte by ref to a method doesn't work");
        }

        public static byte TestMethod(byte aParam)
        {
            aParam++;
            return aParam;
        }

        public static void ByRefTestMethod(ref byte aParam)
        {
            aParam++;
        }
    }
}
