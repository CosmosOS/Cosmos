using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class Int64Test
    {
        public static void Execute()
        {
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

            // Let's try to convert a Long in a ULong
            long val2 = 42;
            ulong val2AsULong = (ulong)val2;

            Assert.IsTrue((val2AsULong == 42), "Int64 to UInt64 conversion doesn't work");

            val2 = long.Parse("42");
            Assert.IsTrue(val2 == 42, "Parsing Int64 doesn't work.");

            /* Let's test right shift */
            value = 4631166901565532406;

            val2 = value >> 20;
            Assert.IsTrue(val2 == 4416624929013, "Int64 right shift doesn't work");

            val2 = value >> 52;
            Assert.IsTrue(val2 == 1028, "Int64 right shift (count >=32) doesn't work");

            /* ... and now left shift */
            value = 4631166901565532406;

            val2 = value << 20;
            Assert.IsTrue(val2 == 6640827866535690240, "Int64 left shift doesn't work got " + val2);

            val2 = value << 52;
            Assert.IsTrue(val2 == -8115486528521633792, "Int64 left shift (count >=32) doesn't work got " + val2);

            // basic arithmetic operations
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

#if false

            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFFFFFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int64.ToString(X2) doesn't work");
#endif
        }
    }
}
