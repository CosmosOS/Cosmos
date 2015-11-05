using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cosmos.Build.Common.Tests
{
    [TestClass]
    public class EnumValueTest
    {
        [TestMethod]
        public void TestParsing()
        {
            var actual = EnumValue.Parse("bin", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Bin, actual); 
            actual = EnumValue.Parse("", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Bin, actual);
            actual = EnumValue.Parse(" 1", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Bin, actual);
            actual = EnumValue.Parse("elf", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Elf, actual);
            actual = EnumValue.Parse("Elf", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Elf, actual);
            actual = EnumValue.Parse("Bin", BinFormat.Bin);
            Assert.AreEqual(BinFormat.Bin, actual);
        }
    }
}
