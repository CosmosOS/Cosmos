namespace Cosmos.Common.Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Cosmos.Common.Extensions;

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
    }
}
