using Microsoft.VisualStudio.TestTools.UnitTesting;

using Cosmos.Common.Extensions;

namespace Cosmos.Common.Tests
{
    [TestClass]
    public class ToHexStringTest
    {
        /// <summary>
        /// Test to hex conversion method.
        /// </summary>
        [TestMethod]
        public void TestToHexUnit32()
        {
            uint testValue = 32;
            Assert.AreEqual("00000020", testValue.ToHex());
            Assert.AreEqual("00000020", testValue.ToHex(8));
            Assert.AreEqual("0000020", testValue.ToHex(7));
            Assert.AreEqual("0020", testValue.ToHex(4));
            Assert.AreEqual("20", testValue.ToHex(2));
            Assert.AreEqual(ToHex(testValue, 8), "0x" + testValue.ToHex(2));
        }

        /// <summary>
        /// Test to hex conversion method.
        /// </summary>
        [TestMethod]
        public void TestToHexUnit16()
        {
            ushort testValue = 34;
            Assert.AreEqual("0022", testValue.ToHex());
            Assert.AreEqual("0022", testValue.ToHex(4));
            Assert.AreEqual("22", testValue.ToHex(2));
        }

        /// <summary>
        /// Test to hex conversion method.
        /// </summary>
        [TestMethod]
        public void TestToHexUnit8()
        {
            byte testValue = 34;
            Assert.AreEqual("22", testValue.ToHex());
            Assert.AreEqual("0022", testValue.ToHex(4));
            Assert.AreEqual("22", testValue.ToHex(2));
        }

        private static string ToHex(uint aNumber, byte aBits)
        {
            //return "0x" + aNumber.ToHex(aBits / 4);
            string ret = "";
            uint xValue = aNumber;
            byte xCurrentBits = aBits;
            ret += "0x";
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        ret += xDigitString;
                        break;
                }
            }
            return ret;
        }
    }
}
