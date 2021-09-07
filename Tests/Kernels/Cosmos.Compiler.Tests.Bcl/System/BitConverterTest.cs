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

            short aShort = 1;
            byte[] shortBytes = BitConverter.GetBytes(aShort);
            result = BitConverter.ToString(shortBytes);
            expectedResult = "01-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(shortBytes) doesn't work: result " + result + " != " + expectedResult);

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

            ushort anUShort = 1;
            byte[] ushortBytes = BitConverter.GetBytes(anUShort);
            result = BitConverter.ToString(ushortBytes);
            expectedResult = "01-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(ushortBytes) doesn't work: result " + result + " != " + expectedResult);

            uint anUInt = 1;

            byte[] uintBytes = BitConverter.GetBytes(anUInt);

            result = BitConverter.ToString(uintBytes, 0);
            expectedResult = "01-00-00-00";

            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(uintBytes) doesn't work: result " + result + " != " + expectedResult);

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

            double aDouble = 1.0;
            result = BitConverter.ToString(BitConverter.GetBytes(aDouble));
            expectedResult = "00-00-00-00-00-00-F0-3F";
            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(doubleBytes) doesn't work: result " + result + " != " + expectedResult);

            bool aBool = true;
            result = BitConverter.ToString(BitConverter.GetBytes(aBool));
            expectedResult = "01";
            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(bool) doesn't work: result " + result + " != " + expectedResult);

            char aChar = 'X';
            result = BitConverter.ToString(BitConverter.GetBytes(aChar));
            expectedResult = "58-00";
            Assert.IsTrue((result == expectedResult), "BitConverter.ToString(char) doesn't work: result " + result + " != " + expectedResult);

            //Tests for GetBytes and ToXXX
            aShort = 240;
            Assert.IsTrue(BitConverter.ToInt16(BitConverter.GetBytes(aShort), 0) == aShort, "BitConverter works with Int16");

            aShort = -240;
            Assert.IsTrue(BitConverter.ToInt16(BitConverter.GetBytes(aShort), 0) == aShort, "BitConverter works with negativ Int16");

            anInt = 1234;
            Assert.IsTrue(BitConverter.ToInt32(BitConverter.GetBytes(anInt), 0) == anInt, "BitConverter works with Int32");

            anInt = -1234;
            Assert.IsTrue(BitConverter.ToInt32(BitConverter.GetBytes(anInt), 0) == anInt, "BitConverter works with negative Int32");

            aLong = 123456789000;
            Assert.IsTrue(BitConverter.ToInt64(BitConverter.GetBytes(aLong), 0) == aLong, "BitConvert works with Int64");

            aLong = -123456789000;
            Assert.IsTrue(BitConverter.ToInt64(BitConverter.GetBytes(aLong), 0) == aLong, "BitConvert works with negative Int64");

            anUShort = 240;
            Assert.IsTrue(BitConverter.ToUInt16(BitConverter.GetBytes(anUShort), 0) == anUShort, "BitConverter works with UInt16");

            anUInt = 1233346;
            Assert.IsTrue(BitConverter.ToUInt32(BitConverter.GetBytes(anUInt), 0) == anUInt, "BitConverter works with UInt32");

            anULong = 123456789000;
            Assert.IsTrue(BitConverter.ToUInt64(BitConverter.GetBytes(anULong), 0) == anULong, "BitConverter works with UInt64");

            aBool = false;
            Assert.IsTrue(BitConverter.ToBoolean(BitConverter.GetBytes(aBool), 0) == aBool, "BitConverter works with Bool");
            
            aChar = 'C';
            Assert.IsTrue(BitConverter.ToChar(BitConverter.GetBytes(aChar), 0) == aChar, "BitConverter works with Char");
        
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

            //Conversion between doubles and long
            Assert.IsTrue(BitConverter.Int64BitsToDouble(1) == 4.94065645841247E-324, "BitConverter long bits to double works");
            Assert.IsTrue(BitConverter.DoubleToInt64Bits(4.94065645841247E-324) == 1, "BitConverter double to long bits works");
            
        }
    }
}
