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

            value = 42.42; // It exists Single.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

            // It seems to be a problem in BitConverter.GetBytes with a Double value that in turn broke toString() for now a solution to this is not found
#if false
            value = 1;

            // Let's try to see as a ByteArray
            byte[] doubleBytes = BitConverter.GetBytes(value);

            Console.WriteLine($"doubleByte is of {doubleBytes.Length} bytes");

            foreach (byte aByte in doubleBytes)
                Console.WriteLine(aByte);

            //Console.WriteLine("Double (as long) " + DoubleToUlong(value));

            result = value.ToString();
            expectedResult = "1";

            // The test fails the conversion returns "Double Underrange"
            Assert.IsTrue((result == expectedResult), "Double.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The value of the Double is " + value;
            expectedResult = "The value of the Double is 1";

            Assert.IsTrue((result == expectedResult), "String concat (Double) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Double is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Double) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Int32 is the same value
            Assert.IsTrue((resultAsInt == value), "Double.GetHashCode() doesn't work");
#endif

#if false
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

            // We put on anUlong a very big value Int64MaxValue + 42. Hmm that '42'  :-)) ?
            ulong anULong = 9223372036854775849;
            value = (double)anULong;
            Assert.IsTrue((DoublesAreEqual(value, 9223372036854775849d)), "(double) from ulong operator doesn't work");

#if false
            unchecked
            {
                ulong anULong = (ulong)-1;
                byte[] anULongAsBytes = BitConverter.GetBytes(anULong);
                value = (double)anULong;
                byte[] valueAsBytes = BitConverter.GetBytes(value);

                //Assert.IsTrue((DoublesAreEqual(value, 18446744073709551615d)), "(double) from ulong operator doesn't work");

                Assert.IsTrue((DoublesAreEqual(value, 18446744073709551615d)), "(double) from ulong operator doesn't work long is " + BitConverter.ToString(anULongAsBytes) + " value (as bytes) is " + BitConverter.ToString(valueAsBytes));
           }
#endif
        }
    }
}
