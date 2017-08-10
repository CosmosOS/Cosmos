using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class SByteTest
    {
        public static void Execute()
        {
            sbyte value;
            String result;
            String expectedResult;

            value = SByte.MaxValue;

            result = value.ToString();
            expectedResult = "127";

            Assert.IsTrue((result == expectedResult), "SByte.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of a SByte is " + value;
            expectedResult = "The Maximum value of a SByte is 127";

            Assert.IsTrue((result == expectedResult), "String concat (SByte) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of a SByte is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (SByte) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // The Hash Code of a SByte is not the same value expressed as int but some XOR tricks are done in the value
            int expectedResultAsInt = ((int)value ^ (int)value << 8);

            Assert.IsTrue((resultAsInt == expectedResultAsInt), "SByte.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "FF";

            Assert.IsTrue((result == expectedResult), "Byte.ToString(X2) doesn't work");
#endif
        }
    }
}
