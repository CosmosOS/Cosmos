using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class Int64Test
    {
        public static void Execute()
        {
            Int64 value;
            String result;
            String expectedResult;

            value = Int64.MaxValue;

            result = value.ToString();
            expectedResult = "9223372036854775807";

            Assert.IsTrue((result == expectedResult), "Int64.ToString doesn't work");

            // Now let's try to concat to a String using '+' operator
            result = "The Maximum value of an Int64 is " + value;
            expectedResult = "The Maximum value of an Int64 is 9223372036854775807";

            Assert.IsTrue((result == expectedResult), "String concat (Int64) doesn't work");

            // Now let's try to use '$ instead of '+'
            result = $"The Maximum value of an Int64 is {value}";
            
            // Actually 'expectedResult' should be the same so...
            Assert.IsTrue((result == expectedResult), "String format (Int64) doesn't work");

            // Now let's Get the HashCode of a value
            int resultAsInt = value.GetHashCode();
            // actually the Hash Code of a Int64 is the value interpolated with XOR to obtain an Int32... so not the same of 'value'!
            int expectedResultAsInt = (unchecked((int)((long)value)) ^ (int)(value >> 32));
  
            Assert.IsTrue((resultAsInt == expectedResultAsInt), "Int64.GetHashCode() doesn't work"); 

            // Let's try to convert a Long in a ULong
            Int64 val2 = 42;
            UInt64 val2AsULong = (ulong)val2;

            Assert.IsTrue((val2AsULong == 42), "Int64 to UInt64 conversion does not work");

#if false
            // Now let's try ToString() again but printed in hex (this test fails for now!)
            result = value.ToString("X2");
            expectedResult = "0x7FFFFFFFFFFFFFFF";

            Assert.IsTrue((result == expectedResult), "Int64.ToString(X2) doesn't work");
#endif
        }
    }
}
