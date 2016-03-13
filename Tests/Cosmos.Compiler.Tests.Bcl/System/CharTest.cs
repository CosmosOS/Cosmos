using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class CharTest
    {
        public static void Execute()
        {
            Char value;
            String result;
            String expectedResult;

            value = 'A';

            result = value.ToString();
            expectedResult = "A";

            Assert.IsTrue((result == expectedResult), "Char.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            // This test is not working in a strange way: Cosmos never returns!
            result = "The first letter of the Alphabeth is " + value;
            expectedResult = "The first letter of the Alphabeth is A";

            Assert.IsTrue((result == expectedResult), "String concat (Char) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The first letter of the Alphabeth is {value}";
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Char) doesn't work");


            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Char is a strange XOR trick...
            int expectedResultAsInt = (int)value | ((int)value << 16);
                      
            Assert.IsTrue((resultAsInt == expectedResultAsInt), "Char.GetHashCode() doesn't work");
#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int32.ToString(X2) doesn't work");
#endif
        }
    }
}
