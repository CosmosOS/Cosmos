using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class DecimalTest
    {
        // This does not compile: 
        public static void Execute()
        {
            Decimal value;
            String result;
            String expectedResult;

            value = 42.42M; // It exists Single.MaxValue but it is a too big value an can be represented only on Scientific notation but then how to confront with a String?

#if false
            // This does not compile: Error: Exception: System.Exception: Native code encountered, plug required. Please see https://github.com/CosmosOS/Cosmos/wiki/Plugs). System.String  System.Number.FormatDecimal(System.Decimal, System.String, System.Globalization.NumberFormatInfo
            result = value.ToString();
            expectedResult = "42.42";

            // The test fails the conversion returns "Double Underrange"
            Assert.IsTrue((result == expectedResult), "Decimal.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The value of the Decimal is " + value;
            expectedResult = "The value of the Decimal is 42.42";

            Assert.IsTrue((result == expectedResult), "String concat (Decimal) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Decimal is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Decimal) doesn't work");


            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // TODO What is the hashcode of 42.42?
            Assert.IsTrue((resultAsInt == value), "Int32.GetHashCode() doesn't work");

            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
        }
    }
}
