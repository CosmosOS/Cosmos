using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class ConvertTests
    {
        public static void Execute()
        {
            var number = 5;
            var numberToString = Convert.ToString(number);

            Assert.IsTrue(numberToString == "5", $"Convert.ToString(Int32) doesn't work. Result: {numberToString}");

            var numberToByte = Convert.ToByte(number);

            Assert.IsTrue(numberToByte == 5, $"Convert.ToByte(Int32) doesn't work. Result: {numberToByte}");

            var byteToSingle = Convert.ToSingle(numberToByte);

            Assert.IsTrue(EqualityHelper.SinglesAreEqual(byteToSingle, 5.0f), $"Convert.ToSingle(Byte) doesn't work. Result: {byteToSingle}");
            
            var numberToBase64 = Convert.ToBase64String(BitConverter.GetBytes(number));

            Assert.IsTrue(numberToBase64 == "BQAAAA==", $"Convert.ToBase64String(byte[]) doesn't work. Result: {numberToBase64}");
        }
    }
}
