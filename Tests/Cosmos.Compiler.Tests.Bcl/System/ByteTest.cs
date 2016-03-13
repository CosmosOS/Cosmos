using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class ByteTest
    {
        public static void Execute()
        {
            byte value;
            String result;
            String expectedResult;

            value = Byte.MaxValue;

            result = value.ToString();
            expectedResult = "255";

            Assert.IsTrue((result == expectedResult), "Byte.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of a Byte is " + value;
            expectedResult = "The Maximum value of a Byte is 255";

            Assert.IsTrue((result == expectedResult), "String concat (Byte) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of a Byte is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Byte) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Byte is the same value expressed as int
            Assert.IsTrue((resultAsInt == value), "Byte.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FF";

            Assert.IsTrue((result == expectedResult), "Byte.ToString(X2) doesn't work");
#endif
        }
    }
}
