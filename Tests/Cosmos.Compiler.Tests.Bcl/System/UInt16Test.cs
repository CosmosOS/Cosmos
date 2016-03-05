using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class UInt16Test
    {
        public static void Execute()
        {
            UInt16 value;
            String result;
            String expectedResult;

            value = UInt16.MaxValue;

            result = value.ToString();
            expectedResult = "65535";

            Assert.IsTrue((result == expectedResult), "UInt16.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an UInt16 is " + value;
            expectedResult = "The Maximum value of an UInt16 is 65535";

            Assert.IsTrue((result == expectedResult), "String concat (UInt16) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an UInt16 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (UInt16) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Byte is the same value expressed as int
            Assert.IsTrue((resultAsInt == value), "UInt16.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FFFF";

            Assert.IsTrue((result == expectedResult), "UInt16.ToString(X2) doesn't work");
#endif
        }
    }
}
