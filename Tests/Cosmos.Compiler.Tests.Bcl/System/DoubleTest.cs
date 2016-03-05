using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class DoubleTest
    {
        public static void Execute()
        {
            Double value;
            String result;
            String expectedResult;

            value = 42.42; // It exists Single.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

            result = value.ToString();
            expectedResult = "42.42";

            // The test fails the conversion returns "Double Underrange"
            Assert.IsTrue((result == expectedResult), "Double.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The value of the Double is " + value;
            expectedResult = "The value of the Double is 42.42";

            Assert.IsTrue((result == expectedResult), "String concat (Double) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Double is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Double) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Int32 is the same value
            Assert.IsTrue((resultAsInt == value), "Int32.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
        }
    }
}
