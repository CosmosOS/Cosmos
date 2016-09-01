using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    using Cosmos.TestRunner;

    public static class UInt64Test
    {
        public static void Execute()
        {
            UInt64 value;
            String result;
            String expectedResult;

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

            // Let's try to convert an ULong in a Long
            UInt64 val2 = 42;
            Int64 val2AsLong = (long)val2;

            Assert.IsTrue((val2AsLong == 42), "UInt64 to Int64 conversion does not work");

            // Let's try to convert a float to an ULong
            float aFloat = 9223372036854775808;
            value = (ulong)aFloat;

            Assert.IsTrue((value == 9223372036854775808), "Float to UInt64 conversion doesn't work");

            // If the double is negative the conversion to ulong is the same to have casted the value from long to ulong (that is -1 becames a real big number)
            aFloat = -1;

            value = (ulong)aFloat;

            Assert.IsTrue((value == 18446744073709551615), "Negative Float to UInt64 conversion doesn't work");

            // Let's try to convert a double to an ULong
            double aDouble = 9223372036854775808;
            value = (ulong)aDouble;

            Assert.IsTrue((value == 9223372036854775808), "Double to UInt64 conversion doesn't work");

            // If the double is negative the conversion to ulong is the same to have casted the value from long to ulong (that is -1 becames a real big number)
            aDouble = -1;

            value = (ulong)aDouble;

            Assert.IsTrue((value == 18446744073709551615), "Negative Double to UInt64 conversion doesn't work");

            value = 4631166901565532406u;

            val2 = value >> 20;
            Assert.IsTrue(val2 == 4416624929013, "ulong right shift does not work");

            val2 = value >> 52;
            Assert.IsTrue(val2 == 1028, "ulong right shift (count >=32) does not work");

            /* ... and now left shift */
            value = 4631166901565532406;

            val2 = value << 20;
            Assert.IsTrue(val2 == 6640827866535690240, "ulong left shift does not work");

            val2 = value << 52;
            Assert.IsTrue(val2 == 10331257545187917824, "ulong left shift (count >=32) does not work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0xFFFFFFFFFFFFFFFF";


            Assert.IsTrue((result == expectedResult), "UInt64.ToString(X2) doesn't work");


            var xTest = TestMethod(0);
            Assert.IsTrue(xTest.Length == 0, "UInt64 test failed.");
#endif
        }

        public static ulong[] TestMethod(ulong aParam1, uint aParam2 = 0)
        {
            var xReturn = new ulong[0];
            ulong xParam1 = aParam1;
            ulong xValue;

            TestMethod2(xParam1, out xValue);
            Array.Resize(ref xReturn, xReturn.Length + 1);
            xReturn[xReturn.Length - 1] = xValue;

            return xReturn;
        }

        public static void TestMethod2(ulong aParam1, out ulong aValue)
        {
            aValue = 8;
            switch (aParam1)
            {
                case 1:
                    aValue = 8 & 0x0FFF;
                    break;
                case 2:
                    aValue = 8;
                    break;
                case 3:
                    aValue = 8 & 0x0FFFFFFF;
                    break;
            }
        }

    }
}
