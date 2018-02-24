using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class BitConverterTest
    {
        public static void Execute()
        {
            string result;
            string expectedResult;

            int anInt = 1;

            byte[] intBytes = BitConverter.GetBytes(anInt);

            result = BitConverter.ToString(intBytes, 0);
            expectedResult = "01-00-00-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(intBytes) doesn't work: result " + result + " != " + expectedResult);

            long aLong = 1;

            byte[] longBytes = BitConverter.GetBytes(aLong);

            result = BitConverter.ToString(longBytes, 0);
            expectedResult = "01-00-00-00-00-00-00-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(longBytes) doesn't work: result " + result + " != " + expectedResult);

            ulong anULong = 1;

            byte[] ulongBytes = BitConverter.GetBytes(anULong);

            result = BitConverter.ToString(ulongBytes, 0);
            expectedResult = "01-00-00-00-00-00-00-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(ulongBytes) doesn't work: result " + result + " != " + expectedResult);

            // This test works, what is the difference with double? That is saved as an Int32 in oly a register?
            float aFloat = 1.0f;

            byte[] floatBytes = BitConverter.GetBytes(aFloat);

            result = BitConverter.ToString(floatBytes, 0);
            expectedResult = "00-00-80-3F";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(floatBytes) doesn't work: result " + result + " != " + expectedResult);

            double Result;
            byte[] doubleBytes = BitConverter.GetBytes(0d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == 0f, "BitConverter.ToDouble works with 0");

            doubleBytes = BitConverter.GetBytes(1d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == 1f, "BitConverter.ToDouble works with 1");

            doubleBytes = BitConverter.GetBytes(2d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == 2f, "BitConverter.ToDouble works with 2");

            doubleBytes = BitConverter.GetBytes(101d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == 101f, "BitConverter.ToDouble works with 101");

            doubleBytes = BitConverter.GetBytes(-101d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == -101f, "BitConverter.ToDouble works with -101");

            doubleBytes = BitConverter.GetBytes(1.2345d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == 1.2345, "BitConverter.ToDouble works with 1.2345");

            doubleBytes = BitConverter.GetBytes(-1.2345d);
            Result = BitConverter.ToDouble(doubleBytes, 0);
            Assert.IsTrue(Result == -1.2345, "BitConverter.ToDouble works with -1.2345");

            double aDouble = 1.0;

            doubleBytes = BitConverter.GetBytes(aDouble);

            result = BitConverter.ToString(doubleBytes, 0);
            expectedResult = "00-00-00-00-00-00-F0-3F";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(doubleBytes) doesn't work: result " + result + " != " + expectedResult);
        }
    }
}
