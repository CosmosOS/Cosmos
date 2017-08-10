using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class Int16Test
    {
        public static void Execute()
        {
            Int16 value;
            String result;
            String expectedResult;

            value = Int16.MaxValue;

            result = value.ToString();
            expectedResult = "32767";

            Assert.IsTrue((result == expectedResult), "Int16.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an Int16 is " + value;
            expectedResult = "The Maximum value of an Int16 is 32767";

            Assert.IsTrue((result == expectedResult), "String concat (Int16) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an Int16 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Int16) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Int16 is some strange XOR trick
            int expectedResultAsInt = ((int)((ushort)value) | (((int)value) << 16));
            
            Assert.IsTrue((resultAsInt == expectedResultAsInt), "Int16.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "7FFF";

            Assert.IsTrue((result == expectedResult), "Int16.ToString(X2) doesn't work");
#endif
        }
    }
}