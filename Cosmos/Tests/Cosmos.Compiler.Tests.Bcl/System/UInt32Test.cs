using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class UInt32Test
    {
        public static void Execute()
        {
            UInt32 value;
            String result;
            String expectedResult;

            value = UInt32.MaxValue;

            result = value.ToString();
            expectedResult = "4294967295";

            //Assert.IsTrue((result == expectedResult), "UInt32.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an UInt32 is " + value;
            expectedResult = "The Maximum value of an UInt32 is 4294967295";

            Assert.IsTrue((result == expectedResult), "String concat (UInt32) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an UInt32 is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (UInt32) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();

            // actually the Hash Code of a Int32 is the same value (but expressed as Int32 so could have sign!)
            Assert.IsTrue((resultAsInt == (int)value), "UInt32.GetHashCode() doesn't work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
        }
    }
}
