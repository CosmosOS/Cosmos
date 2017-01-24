using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class Int16Test
    {
        public static void Execute()
        {
            short value;
            string result;
            string expectedResult;

            value = short.MaxValue;

            result = value.ToString();
            expectedResult = "32767";
            Debug.Kernel.Debugger d = new Debug.Kernel.Debugger("", "");
            d.SendNumber(value);
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

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "7FFF";

            Assert.IsTrue((result == expectedResult), "Int16.ToString(X2) doesn't work");
#endif

            // Now test conversions

            short maxValue = short.MaxValue;
            short minValue = short.MinValue;

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
            Assert.IsTrue((int)maxValue == 0x7FFF, "Conv_I4 for Int16 doesn't work");
            Assert.IsTrue((int)minValue == -0x8000, "Conv_I4 for Int16 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0x7FFF, "Conv_U4 for Int16 doesn't work");
            //Assert.IsTrue((uint)minValue == 0x8000, "Conv_U4 for Int16 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x7FFF, "Conv_I8 for Int16 doesn't work");
            Assert.IsTrue((long)minValue == -0x8000, "Conv_I8 for Int16 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x7FFF, "Conv_U8 for Int16 doesn't work");
            //Assert.IsTrue((ulong)minValue == 0x8000, "Conv_U8 for Int16 doesn't work");
        }
    }
}
