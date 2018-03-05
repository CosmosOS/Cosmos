using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class SingleTest
    {
        public static void Execute()
        {
            Single value;
            String result;
            String expectedResult;

            /* First start with some weird value (not really numbers) that the IEEE standard has */
            value = Single.PositiveInfinity;

            result = value.ToString();
            expectedResult = "∞";

            Assert.IsTrue((result == expectedResult), "Single.ToString of INF doesn't work");

            value = Single.NegativeInfinity;

            result = value.ToString();
            expectedResult = "-∞";

            Assert.IsTrue((result == expectedResult), "Single.ToString of -INF doesn't work");

            value = Single.NaN;

            result = value.ToString();
            expectedResult = "NaN";

            Assert.IsTrue((result == expectedResult), "Single.ToString of -NaN doesn't work");

            /* Another special value is '0' */
            value = 0f;

            result = value.ToString();
            expectedResult = "0";

            Assert.IsTrue((result == expectedResult), "Single.ToString of 0 doesn't work");

            /* A negative value */
            value = -42.42f;

            result = value.ToString();
            expectedResult = "-42.42";

            Assert.IsTrue((result == expectedResult), "Single.ToString of negative number doesn't work");

            /* A big value (to be correct toString should convert it in scientific notation) */
            value = 9223372036854775808f;

            result = value.ToString();
            expectedResult = "9223372036854775808";

            Assert.IsTrue((result == expectedResult), "Single.ToString of big number doesn't work");

            /* OK now a normal value */
            value = 42.42F; // It exists Single.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

            result = value.ToString();
            expectedResult = "42.42";

            Assert.IsTrue((result == expectedResult), "Single.ToString of normal number doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The value of the Single is " + value;
            expectedResult = "The value of the Single is 42.42";

            Assert.IsTrue((result == expectedResult), "String concat (Single) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Single is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Single) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // C# interactive says that the HashCode of 42.42 is this beast: 1110027796. It should be the same for Cosmos!
            Assert.IsTrue((resultAsInt == 1110027796), "Single.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
            // OK now some mathematical operations as if we were in school!

            // First test that == works, please note that as we talking of floating point value usually does NOT works! It seems I've chosen a number (42.42) that is representable in binay form...
            Assert.IsTrue((value == 42.42f), "float operator== doesn't work");

            // Now test for greaterThan
            Assert.IsTrue((value > 20.15f), "float operator> doesn't work");

            // Now test for greaterThanEqual
            Assert.IsTrue((value >= 42.42f), "float operator>= doesn't work");

            // Now test for inequality
            Assert.IsTrue((value != 69.69f), "float operator!= doesn't work");

            // Now test lessThan
            Assert.IsTrue((value < 69.69f), "float operator< doesn't work");

            // Now test lessThanEqual
            Assert.IsTrue((value <= 42.42f), "float operator<= doesn't work");

            // Now test addition, in this case == does not work anymore evidently 44.62 is not representable in binary we resort to test it using ToString()
            Single OperationResult;
            Single otherValue = 2.20f;

            OperationResult = value + otherValue;

            Assert.IsTrue((EqualityHelper.SinglesAreEqual(OperationResult, 44.62f)), "float operator+ doesn't work");

            // Now test subtraction
            OperationResult = value - otherValue;
            //expectedResult = "40.22";

            Assert.IsTrue((EqualityHelper.SinglesAreEqual(OperationResult, 40.22f)), "float operator- doesn't work");

            // Now test multiplication
            otherValue = 2.00f; // I'll change 'otherValue' to 2.00f because if not the result is too much wrong to make sense...
            OperationResult = value * otherValue;

            Assert.IsTrue((EqualityHelper.SinglesAreEqual(OperationResult, 84.84f)), "float operator* doesn't work");

            // Now test division
            OperationResult = value / otherValue;

            Assert.IsTrue((EqualityHelper.SinglesAreEqual(OperationResult, 21.21f)), "float operator/ doesn't work");

            // Now test division again but with dividend 0 the expected result should be Double.PositiveInfinity
            OperationResult = value / 0.00f;

            Assert.IsTrue((OperationResult == Single.PositiveInfinity), "flot operator/0 doesn't work");

            // Now test division again but with all values as 0 the expected result should be Double.NaN
            OperationResult = 0.00f / 0.00f;

            Assert.IsTrue((Single.IsNaN(OperationResult)), "float operator/(0/0) doesn't work");

            // Now test some castings operations

            sbyte valueAsSByte = (sbyte)value;
            Assert.IsTrue((valueAsSByte == (sbyte)42), "float (sbyte) operator doesn't work");

            byte valueAsByte = (byte)value;
            Assert.IsTrue((valueAsByte == (byte)42), "float (byte) operator doesn't work");

            short valueAsShort = (short)value;
            Assert.IsTrue((valueAsByte == (short)42), "float (short) operator doesn't work");

            ushort valueAsUShort = (ushort)value;
            Assert.IsTrue((valueAsUShort == (ushort)42), "float (ushort) operator doesn't work");

            int valueAsInt = (int)value;
            Assert.IsTrue((valueAsInt == (int)42), "float (int) operator doesn't work");

            uint valueAsUInt = (uint)value;
            Assert.IsTrue((valueAsUInt == (uint)42), "float (uint) operator doesn't work");

            long valueAsLong = (long)value;
            Assert.IsTrue((valueAsLong == (long)42), "float (long) operator doesn't work");

            ulong valueAsULong = (ulong)value;
            Assert.IsTrue((valueAsULong == (ulong)42), "float (ulong) operator doesn't work");

            // Let's continue with casting but the other way around
            valueAsInt = 69;
            value = (float)valueAsInt;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 69f)), "(float) from int operator doesn't work");

            valueAsLong = 69;
            value = (float)valueAsLong;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 69f)), "(float) from long operator doesn't work");

            double valueAsDouble = 69.69;
            value = (float)valueAsDouble;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 69.69f)), "(float) from double operator doesn't work");

            int anInt = 69;
            value = (float)anInt;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 69f)), "(float) from int operator doesn't work");

            // We put on anUInt a very big value Int32.MaxValue + 42. Why all this 42 :-) ?
            uint anUInt = 2147483689;
            value = (float)anUInt;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 2147483689f)), "(float) from uint operator doesn't work");

            // We put on anUlong a very big value Int64MaxValue + 42. Hmm that '42' again :-)) ?
            ulong anULong = 9223372036854775849;
            value = (float)anULong;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(value, 9223372036854775849f)), "(float) from ulong operator doesn't work");

            value = -42.0f;
            float valueNegated = -value;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(valueNegated, 42.0f)), "(float) negation doesn't work");

            // Let's try if it works in the other way too
            value = 42.0f;
            valueNegated = -value;
            Assert.IsTrue((EqualityHelper.SinglesAreEqual(valueNegated, -42.0f)), "(float) negation of positive float doesn't work");

            #region Parsing

            value = Single.Parse("0.4");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.4), "simple parsing of float works");

            value = Single.Parse("+0.3");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.3), "parsing of float with positive sign works!");

            value = Single.Parse("-0.4");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, -0.4), "parsing of negative float works!");

            value = Single.Parse("    0.7     ");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.7), "float parsing ignores leading and trailing whitespaces");

            value = Single.Parse("0.4E1");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 4), "float parsing takes in account E");

            value = Single.Parse("0.4E-1");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.04), "float parsing works with negative E");

            Assert.IsFalse(Single.TryParse("asd4", out value), "float TryParse returns false when it fails");

            Assert.IsTrue(Single.TryParse("2.3", out value), " float TryParse returns true when it works");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 2.3), "float TryParse returns correct result when it works");

            #endregion Parsing
        }
    }
}
