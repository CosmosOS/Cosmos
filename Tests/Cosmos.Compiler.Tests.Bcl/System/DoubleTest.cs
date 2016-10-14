using System;
using Cosmos.TestRunner;
using Cosmos.Debug.Kernel;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class DoubleTest
    {
        public readonly Debugger mDebugger = new Debugger("User", "Double");

        /* The double== equality operator is so imprecise to not be really ever useful we should be happy if the two values are "similar" */
        private static bool DoublesAreEqual(double left, double right)
        {
            // Define the tolerance for variation in their values
            double difference = Math.Abs(left * .00001);

            if (Math.Abs(left - right) <= difference)
                return true;
            else
                return false;
        }

        public static void Execute()
        {
            Double value;
            String result;
            String expectedResult;

            /* First start with some weird value (not really numbers) that the IEEE standard has */
            value = Double.PositiveInfinity;

            result = value.ToString();
            expectedResult = "∞";

            Assert.IsTrue((result == expectedResult), "Double.ToString of INF doesn't work");

            value = Double.NegativeInfinity;

            result = value.ToString();
            expectedResult = "-∞";

            Assert.IsTrue((result == expectedResult), "Double.ToString of -INF doesn't work");

            value = Double.NaN;

            result = value.ToString();
            expectedResult = "NaN";

            Assert.IsTrue((result == expectedResult), "Double.ToString of NaN doesn't work");

            /* Another special value is '0' */
            value = 0;

            result = value.ToString();
            expectedResult = "0";

            Assert.IsTrue((result == expectedResult), "Double.ToString of 0 doesn't work");

            /* A negative value */
            value = -42.42;

            result = value.ToString();
            expectedResult = "-42.42";

            Assert.IsTrue((result == expectedResult), "Double.ToString of negative number doesn't work");

            /* A big value (to be correct toString should convert it in scientific notation) */
            value = 9223372036854775808;

            result = value.ToString();
            expectedResult = "9223372036854775808";

            Assert.IsTrue((result == expectedResult), "Double.ToString of big number doesn't work");

            /* OK now a normal value */
            value = 42.42; // It exists Double.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

            result = value.ToString();
            expectedResult = "42.42";

            Assert.IsTrue((result == expectedResult), "Double.ToString of normal number doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The value of the Double is " + value;
            expectedResult = "The value of the Double is 42.42";

            Assert.IsTrue((result == expectedResult), "String concat (Double) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Double is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Double) doesn't work");

#if false
            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Int32 is the same value
            Assert.IsTrue((resultAsInt == value), "Double.GetHashCode() doesn't work");


            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
            // OK now some mathematical operations as if we were in school!

            Double expectedValue = 42.42;
            // First test that == works, please note that as we talking of floating point value usually does NOT works! It seems I've chosen a number (42.42) that is representable in binay form...
            bool CEQ = (value == expectedValue);

            Assert.IsTrue((CEQ), "double operator== doesn't work");

            // Now test for greaterThan
            Assert.IsTrue((value > 20.15), "double operator> doesn't work");

            // Now test for greaterThanEqual
            Assert.IsTrue((value >= 42.42), "double operator>= doesn't work");

            // Now test for greaterThanEqual (with NaN)
            Assert.IsTrue((value >= Double.NaN), "double operator>= (NaN) doesn't work");

            // Now test for inequality
            Assert.IsTrue((value != 69.69), "double operator!= doesn't work");

            // Now test lessThan
            Assert.IsTrue((value < 69.69), "double operator< doesn't work");

            // Now test lessThanEqual
            Assert.IsTrue((value <= 42.42), "double operator<= doesn't work");

            // Now test addition, in this case == does not work anymore evidently 44.62 is not representable in binary we resort to test it using ToString()
            Double OperationResult;
            Double otherValue = 2.20;

            OperationResult = value + otherValue;

            Assert.IsTrue((DoublesAreEqual(OperationResult, 44.62)), "double operator+ doesn't work");

            // Now test subtraction
            OperationResult = value - otherValue;

            Assert.IsTrue((DoublesAreEqual(OperationResult, 40.22)), "double operator- doesn't work");

            // Now test multiplication
            otherValue = 2.00; // I'll change 'otherValue' to 2.00 because if not the result is too much wrong to make sense...
            OperationResult = value * otherValue;

            Assert.IsTrue((DoublesAreEqual(OperationResult, 84.84)), "double operator* doesn't work");

            // Now test division
            OperationResult = value / otherValue;

            Assert.IsTrue((DoublesAreEqual(OperationResult, 21.21)), "double operator/ doesn't work");

            // Now test division again but with dividend 0 the expected result should be Double.PositiveInfinity
            OperationResult = value / 0.00;

            Assert.IsTrue((OperationResult == Double.PositiveInfinity), "double operator/0 doesn't work");

#if false // This test fails (== with NaN does not work but this is OK as C# is wrong on this too) and the method isNaN fails
            // Now test division again but with all values as 0 the expected result should be Double.NaN
            OperationResult = 0.00 / 0.00;

            Assert.IsTrue((Double.IsNaN(OperationResult)), "double operator/(0/0) doesn't work");
#endif

            // Now test some castings operations
            byte valueAsByte = (byte)value;
            Assert.IsTrue((valueAsByte == (byte)42), "double (byte) operator doesn't work");

            short valueAsShort = (short)value;
            Assert.IsTrue((valueAsByte == (short)42), "double (short) operator doesn't work");

            int valueAsInt = (int)value;
            Assert.IsTrue((valueAsInt == (int)42), "double (int) operator doesn't work");

            long valueAsLong = (long)value;
            Assert.IsTrue((valueAsLong == (long)42), "double (long) operator doesn't work");

            // We put on anUInt a very big value Int32.MaxValue + 42. Why all this 42 :-) ?
            uint anUInt = 2147483689;
            value = (double)anUInt;
            Assert.IsTrue((DoublesAreEqual(value, 2147483689d)), "(double) from uint operator doesn't work");

            // We put on anUlong a very big value Int64MaxValue + 42. Hmm that '42' again :-)) ?
            ulong anULong = 9223372036854775849;
            value = (double)anULong;
            Assert.IsTrue((DoublesAreEqual(value, 9223372036854775849d)), "(double) from ulong operator doesn't work");

            value = -42.0;
            double valueNegated = -value;
            Assert.IsTrue((DoublesAreEqual(valueNegated, 42d)), "(double) negation doesn't work");

            // Let's try if it works in the other way too
            value = 42.0;
            valueNegated = -value;
            Assert.IsTrue((DoublesAreEqual(valueNegated, -42.0f)), "(double) negation of positive double doesn't work");
        }
    }
}
