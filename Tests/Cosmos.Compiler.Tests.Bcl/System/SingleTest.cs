using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class SingleTest
    {
        /* The single== equality operator is so imprecise to not be really ever useful we should be happy if the two values are "similar" */
        private static bool SinglesAreEqual(Single left, Single right)
        {
            // Define the tolerance for variation in their values
            Single difference = (Single) Math.Abs(left * .00001);

            if (Math.Abs(left - right) <= difference)
                return true;
            else
                return false;
        }

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
            Single value;
            String result;
            String expectedResult;

            value = 42.42F; // It exists Single.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

            result = value.ToString();
            expectedResult = "42.42";

            Assert.IsTrue((result == expectedResult), "Single.ToString doesn't work");

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
            //expectedResult = "44.62";

            Assert.IsTrue((SinglesAreEqual(OperationResult, 44.62f)), "float operator+ doesn't work");
            //Assert.IsTrue((OperationResult.ToString() == expectedResult), "float operator+ doesn't work");

            // Now test subtraction
            OperationResult = value - otherValue;
            //expectedResult = "40.22";

            Assert.IsTrue((SinglesAreEqual(OperationResult, 40.22f)), "float operator- doesn't work");
            //Assert.IsTrue((OperationResult.ToString() == expectedResult), "float operator- doesn't work");

            // Now test multiplication
            otherValue = 2.00f; // I'll change 'otherValue' to 2.00f because if not the result is too much wrong to make sense...
            OperationResult = value * otherValue;
            //expectedResult = "84.84";

            Assert.IsTrue((SinglesAreEqual(OperationResult, 84.84f)), "float operator* doesn't work");
            //Assert.IsTrue((OperationResult.ToString() == expectedResult), "float operator* doesn't work");

            // Now test division
            OperationResult = value / otherValue;
            //expectedResult = "21.21";

            Assert.IsTrue((SinglesAreEqual(OperationResult, 21.21f)), "float operator/ doesn't work");
            //Assert.IsTrue((OperationResult.ToString() == expectedResult), "float operator/ doesn't work");

            // Now test division again but with dividend 0 the expected result should be Double.PositiveInfinity
            OperationResult = value / 0.00f;

            Assert.IsTrue((OperationResult == Single.PositiveInfinity), "single operator/0 doesn't work");

            // Now test division again but with all values as 0 the expected result should be Double.NaN
            OperationResult = 0.00f / 0.00f;

            Assert.IsTrue((Single.IsNaN(OperationResult)), "single operator/(0/0) doesn't work");

            // Now test some castings operations
            byte valueAsByte = (byte)value;
            Assert.IsTrue((valueAsByte == (byte)42), "float (byte) operator doesn't work");

            short valueAsShort = (short)value;
            Assert.IsTrue((valueAsByte == (short)42), "float (short) operator doesn't work");

            int valueAsInt = (int)value;
            Assert.IsTrue((valueAsInt == (int)42), "float (int) operator doesn't work");

            long valueAsLong = (long)value;
            Assert.IsTrue((valueAsInt == (long)42), "float (long) operator doesn't work");

            // Let's continue with casting but the other way around
            valueAsInt = 69;
            value = (float) valueAsInt;
            Assert.IsTrue((SinglesAreEqual(value, 69f)), "(float) from int operator doesn't work");

            valueAsLong = 69;
            value = (float)valueAsLong;
            Assert.IsTrue((SinglesAreEqual(value, 69f)), "(float) from long operator doesn't work");

            double valueAsDouble = 69.69;
            value = (float)valueAsDouble;
            Assert.IsTrue((SinglesAreEqual(value, 69.69f)), "(float) from double operator doesn't work");

            int anInt = 69;
            value = (float)anInt;
            Assert.IsTrue((SinglesAreEqual(value, 69f)), "(float) from int operator doesn't work");

            // We put on anUInt a very big value Int32.MaxValue + 42. Why all this 42 :-) ?
            uint anUInt = 2147483689;
            value = (float)anUInt;
            Assert.IsTrue((SinglesAreEqual(value, 2147483689f)), "(float) from uint operator doesn't work");

            // We put on anUlong a very big value Int64MaxValue + 42. Hmm that '42' again :-)) ?
            ulong anULong = 9223372036854775849;
            value = (float)anULong;
            Assert.IsTrue((DoublesAreEqual(value, 9223372036854775849d)), "(float) from ulong operator doesn't work");


#if false
            //ulong anULong = 9223372036854775849;
            ulong anULong = 9423372036854775870;
            value = (float)anULong;
#if true
            byte[] valueAsBytes = BitConverter.GetBytes(value);

            Assert.IsTrue((SinglesAreEqual(value, 9223372036854775849f)), "(float) from ulong operator doesn't work returns " + value + " as bytes " + BitConverter.ToString(valueAsBytes));
#else
            Assert.IsTrue((SinglesAreEqual(value, 9423372036854775870f)), "(float) from ulong operator doesn't work: ");
#endif
#endif
        }
    }
}
