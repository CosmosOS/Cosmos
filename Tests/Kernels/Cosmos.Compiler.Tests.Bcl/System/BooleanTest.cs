using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class BooleanTest
    {
        public static void Execute()
        {
            Boolean value;
            String result;
            String expectedResult;

            value = true;

            result = value.ToString();
            expectedResult = "True";

            Assert.IsTrue((result == expectedResult), "Boolean.ToString doesn't work");

            // Cosmos blocks again and never returns (?)
            // Now let's try to concat to a String using '+' operator
            result = "The value of the Boolean is " + value;
            expectedResult = "The value of the Boolean is True";

            Assert.IsTrue((result == expectedResult), "String concat (Boolean) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The value of the Boolean is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Boolean) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Bool is 1 for true and 0 for false
            Assert.IsTrue((resultAsInt == 1), "Boolean.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
        }
    }
}
