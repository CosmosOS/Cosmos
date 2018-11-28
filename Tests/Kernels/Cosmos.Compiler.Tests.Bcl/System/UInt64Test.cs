using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class UInt64Test
    {
        public static void Execute()
        {
            ulong value;
            string result;
            string expectedResult;

            value = UInt64.MaxValue;

            result = value.ToString();
            expectedResult = "18446744073709551615";

            Assert.IsTrue((result == expectedResult), "UInt64.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an UInt64 is " + value;
            expectedResult = "The Maximum value of an UInt64 is 18446744073709551615";

            Assert.IsTrue((result == expectedResult), "String concat (UInt64) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an UInt64 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (UInt64) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Int64 is the value interpolated with XOR to obtain an Int32... so not the same of 'value'!
            int expectedResultAsInt = ((int)value) ^ (int)(value >> 32);

            Assert.IsTrue((resultAsInt == expectedResultAsInt), "UInt64.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0xFFFFFFFFFFFFFFFF";


            Assert.IsTrue((result == expectedResult), "UInt64.ToString(X2) doesn't work");


            var xTest = TestMethod(0);
            Assert.IsTrue(xTest.Length == 0, "UInt64 test failed.");
#endif

            // basic bit operations

            ulong val2;

            value = 0x0C; // low-order bits: 0b0000_1100

            val2 = ~value; // low-order bits: val2 = ~value = 0b1111_0011
            Assert.IsTrue(val2 == 0xFFFFFFFFFFFFFFF3, "UInt64 bitwise not doesn't work got: " + val2);

            val2 = value & 0x06; // low-order bits: val2 = value & 0b0000_0110 = 0b0000_0100
            Assert.IsTrue(val2 == 0x04, "UInt64 bitwise and doesn't work got: " + val2);

            val2 = value | 0x06; // low-order bits: val2 = value | 0b0000_0110 = 0b0000_1110
            Assert.IsTrue(val2 == 0x0E, "UInt64 bitwise or doesn't work got: " + val2);

            val2 = value ^ 0x06; // low-order bits: val2 = value ^ 0b0000_0110 = 0b0000_1010
            Assert.IsTrue(val2 == 0x0A, "UInt64 bitwise xor doesn't work got: " + val2);

            val2 = value >> 0x02; // low-order bits: val2 = value >> 0b0000_0010 = 0b0000_0011
            Assert.IsTrue(val2 == 0x03, "UInt64 left shift doesn't work got: " + val2);

            val2 = value << 0x02; // low-order bits: val2 = value << 0b0000_0010 = 0b0011_0000
            Assert.IsTrue(val2 == 0x30, "UInt64 right shift doesn't work got: " + val2);

            // basic arithmetic operations

            value = 60;

            val2 = value + 5;
            Assert.IsTrue(val2 == 65, "UInt64 addition doesn't work got: " + val2);

            val2 = value - 5;
            Assert.IsTrue(val2 == 55, "UInt64 subtraction doesn't work got: " + val2);

            val2 = value * 5;
            Assert.IsTrue(val2 == 300, "UInt64 multiplication doesn't work got: " + val2);

            val2 = value / 5;
            Assert.IsTrue(val2 == 12, "UInt64 division doesn't work got: " + val2);

            val2 = value % 7;
            Assert.IsTrue(val2 == 4, "UInt64 remainder doesn't work got: " + val2);

            value = 1728000000000;

            val2 = value + 36000000000;
            Assert.IsTrue(val2 == 1764000000000, "UInt64 addition doesn't work got " + val2);

            val2 = value - 36000000000;
            Assert.IsTrue(val2 == 1692000000000, "UInt64 subtraction doesn't work got " + val2);

            val2 = value * 36000000000;
            Assert.IsTrue(val2 == 5578983451391950848, "UInt64 multiplication doesn't work got " + val2);

            val2 = value / 36000000000;
            Assert.IsTrue(val2 == 48, "UInt64 division doesn't work got " + val2);

            value = 3200000000000;

            val2 = value % 1300000000000;
            Assert.IsTrue(val2 == 600000000000, "UInt64 remainder doesn't work got " + val2);

            // Now test conversions

            ulong maxValue = UInt64.MaxValue;
            ulong minValue = UInt64.MinValue;

            // TODO: some convert instructions aren't being emitted, we should find other ways of getting them emitted

            // Test Conv_I1
            Assert.IsTrue((sbyte)maxValue == -0x01, "Conv_I1 for UInt64 doesn't work");
            Assert.IsTrue((sbyte)minValue == 0x00, "Conv_I1 for UInt64 doesn't work");

            // Test Conv_U1
            Assert.IsTrue((byte)maxValue == 0xFF, "Conv_U1 for UInt64 doesn't work");
            Assert.IsTrue((byte)minValue == 0x00, "Conv_U1 for UInt64 doesn't work");

            // Test Conv_I2
            Assert.IsTrue((short)maxValue == -0x0001, "Conv_I2 for UInt64 doesn't work");
            Assert.IsTrue((short)minValue == 0x0000, "Conv_I2 for UInt64 doesn't work");

            // Test Conv_U2
            Assert.IsTrue((ushort)maxValue == 0xFFFF, "Conv_U2 for UInt64 doesn't work");
            Assert.IsTrue((ushort)minValue == 0x0000, "Conv_U2 for UInt64 doesn't work");

            // Test Conv_I4
            Assert.IsTrue((int)maxValue == -0x00000001, "Conv_I4 for UInt64 doesn't work");
            Assert.IsTrue((int)minValue == 0x00000000, "Conv_I4 for UInt64 doesn't work");

            // Test Conv_U4
            Assert.IsTrue((uint)maxValue == 0xFFFFFFFF, "Conv_U4 for UInt64 doesn't work");
            Assert.IsTrue((uint)minValue == 0x00000000, "Conv_U4 for UInt64 doesn't work");

            // Test Conv_I8
            Assert.IsTrue((long)maxValue == -0x0000000000000001, "Conv_I8 for UInt64 doesn't work");
            Assert.IsTrue((long)minValue == 0x0000000000000000, "Conv_I8 for UInt64 doesn't work");

            // Test Conv_U8
            Assert.IsTrue((ulong)maxValue == 0xFFFFFFFFFFFFFFFF, "Conv_U8 for UInt64 doesn't work");
            Assert.IsTrue((ulong)minValue == 0x0000000000000000, "Conv_U8 for UInt64 doesn't work");

            // Test Conv_R4
            Assert.IsTrue((float)maxValue == UInt64.MaxValue, "Conv_R4 for UInt64 doesn't work");
            Assert.IsTrue((float)minValue == UInt64.MinValue, "Conv_R4 for UInt64 doesn't work");

            // Test Conv_R8
            Assert.IsTrue((double)maxValue == UInt64.MaxValue, "Conv_R8 for UInt64 doesn't work");
            Assert.IsTrue((double)minValue == UInt64.MinValue, "Conv_R8 for UInt64 doesn't work");

            // Test Methods

            value = 60;

            val2 = TestMethod(value);
            Assert.IsTrue(value == 60, "Passing an UInt64 as a method parameter doesn't work");
            Assert.IsTrue(val2 == 61, "Returning an UInt64 value from a method doesn't work");

            ByRefTestMethod(ref value);
            Assert.IsTrue(value == 61, "Passing an UInt64 by ref to a method doesn't work");

            //Test Overflow Exceptions
            ulong val3o = 10000;
            bool efuse = false;
            try
            {
                checked
                {
                    val3o += ulong.MaxValue;
                }
            }
            catch (OverflowException)
            {
                efuse = true;
            }
            Assert.IsTrue(efuse, "Add_Ovf for UInt64 doesn't work: " + val3o);

            efuse = false;
            val3o = 10000;
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
            Assert.IsTrue(efuse, "Sub_Ovf for UInt64 doesn't work: " + val3o);
        }

        public static ulong TestMethod(ulong aParam)
        {
            aParam++;
            return aParam;
        }

        public static void ByRefTestMethod(ref ulong aParam)
        {
            aParam++;
        }
    }
}
