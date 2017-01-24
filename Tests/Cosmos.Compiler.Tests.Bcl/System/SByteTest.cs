using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class SByteTest
    {
        public static void Execute()
        {
            sbyte value;
            string result;
            string expectedResult;

            value = sbyte.MaxValue;

            result = value.ToString();
            expectedResult = "127";

            Assert.IsTrue((result == expectedResult), "SByte.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of a SByte is " + value;
            expectedResult = "The Maximum value of a SByte is 127";

            Assert.IsTrue((result == expectedResult), "String concat (SByte) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of a SByte is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (SByte) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // The Hash Code of a SByte is not the same value expressed as int but some XOR tricks are done in the value
            int expectedResultAsInt = ((int)value ^ (int)value << 8);

            Assert.IsTrue((resultAsInt == expectedResultAsInt), "SByte.GetHashCode() doesn't work");

            // basic bit operations

            int val2;

            value = 0x0C; // 0b0000_1100

            val2 = ~value; // val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == -0x0D, "SByte bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "SByte bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "SByte bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "SByte bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "SByte left shift doesn't work got: " + val2);

            val2 = value << 0x02; // val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "SByte right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "SByte addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "SByte subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "SByte multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "SByte division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "SByte remainder doesn't work got: " + val2);

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FF";

            Assert.IsTrue((result == expectedResult), "SByte.ToString(X2) doesn't work");
#endif

            // Now test conversions

            sbyte maxValue = sbyte.MaxValue;
            sbyte minValue = sbyte.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == 0x7F, "Conv_I1 for SByte doesn't work");
            Assert.IsTrue((sbyte)minValue == -0x80, "Conv_I1 for SByte doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0x7F, "Conv_U1 for SByte doesn't work");
            Assert.IsTrue((byte)minValue == 0x80, "Conv_U1 for SByte doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == 0x7F, "Conv_I2 for SByte doesn't work");
            Assert.IsTrue((short)minValue == -0x80, "Conv_I2 for SByte doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0x7F, "Conv_U2 for SByte doesn't work");
            Assert.IsTrue((ushort)minValue == 0x80, "Conv_U2 for SByte doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == 0x7F, "Conv_I4 for SByte doesn't work");
            Assert.IsTrue((int)minValue == -0x80, "Conv_I4 for SByte doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0x7F, "Conv_U4 for SByte doesn't work");
            //Assert.IsTrue((uint)minValue == 0x80, "Conv_U4 for SByte doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == 0x7F, "Conv_I8 for SByte doesn't work");
            Assert.IsTrue((long)minValue == -0x80, "Conv_I8 for SByte doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0x7F, "Conv_U8 for SByte doesn't work");
            //Assert.IsTrue((ulong)minValue == 0x80, "Conv_U8 for SByte doesn't work");
        }
    }
}
