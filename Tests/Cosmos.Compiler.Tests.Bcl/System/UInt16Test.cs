using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class UInt16Test
    {
        public static void Execute()
        {
            UInt16 value;
            String result;
            String expectedResult;

            value = UInt16.MaxValue;

            result = value.ToString();
            expectedResult = "65535";

            Assert.IsTrue((result == expectedResult), "UInt16.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an UInt16 is " + value;
            expectedResult = "The Maximum value of an UInt16 is 65535";

            Assert.IsTrue((result == expectedResult), "String concat (UInt16) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an UInt16 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (UInt16) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Byte is the same value expressed as int
            Assert.IsTrue((resultAsInt == value), "UInt16.GetHashCode() doesn't work");

            // basic bit operations

            int val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "UInt16 bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "UInt16 bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "UInt16 bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "UInt16 bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "UInt16 left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "UInt16 right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "UInt16 addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "UInt16 subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "UInt16 multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "UInt16 division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "UInt16 remainder doesn't work got: " + val2);

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FFFF";

            Assert.IsTrue((result == expectedResult), "UInt16.ToString(X2) doesn't work");
#endif

            // Now test conversions

            ushort maxValue = ushort.MaxValue;
            ushort minValue = ushort.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for UInt16 doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for UInt16 doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for UInt16 doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for UInt16 doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == -0x0001, "Conv_I2 for UInt16 doesn't work");
            Assert.IsTrue((short)minValue == 0x0000, "Conv_I2 for UInt16 doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0xFFFF, "Conv_U2 for UInt16 doesn't work");
            Assert.IsTrue((ushort)minValue == 0x0000, "Conv_U2 for UInt16 doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == 0xFFFF, "Conv_I4 for UInt16 doesn't work");
            Assert.IsTrue((int)minValue == 0x0000, "Conv_I4 for UInt16 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0xFFFF, "Conv_U4 for UInt16 doesn't work");
            Assert.IsTrue((uint)minValue == 0x0000, "Conv_U4 for UInt16 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0xFFFF, "Conv_I8 for UInt16 doesn't work");
            Assert.IsTrue((long)minValue == 0x0000, "Conv_I8 for UInt16 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0xFFFF, "Conv_U8 for UInt16 doesn't work");
            Assert.IsTrue((ulong)minValue == 0x0000, "Conv_U8 for UInt16 doesn't work");
        }
    }
}
