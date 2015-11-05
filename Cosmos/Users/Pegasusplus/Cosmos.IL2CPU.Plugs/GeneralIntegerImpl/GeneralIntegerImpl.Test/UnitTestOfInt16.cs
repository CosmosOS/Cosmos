using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneralIntegerImpl.Test
{
    [TestClass]
    public class UnitTestOfInt16
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("+1"));

            // Parse -1
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("-1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("-0"));

            // Parse Int16.MinValue
            Assert.AreEqual(-32768, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("-32768"));

            // Parse Int16.MaxValue
            Assert.AreEqual(32767, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("32767"));

            // Parse +Int16.MaxValue
            Assert.AreEqual(32767, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("+32767"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 32768
            //short.Parse("32768");
            Assert.AreEqual(32768, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("32768"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -32769
            //short.Parse("-32769");
            Assert.AreEqual(-32769, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("-32769"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //short.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            short result;
            string strValue = "2A";
            Assert.AreEqual(false, short.TryParse(strValue, out result));
            Assert.AreEqual(0, result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.TryParse(strValue, out result));
            Assert.AreEqual(0, result);

            return;
        }

        [TestMethod]
        public void FormatSupport()
        {
            // Parse Exponent
            short result;
            Assert.AreEqual(true, short.TryParse("1E0", System.Globalization.NumberStyles.AllowExponent, null, out result));
            Assert.AreEqual(1, result);
            Assert.AreEqual(true, short.TryParse("1E1", System.Globalization.NumberStyles.AllowExponent, null, out result));
            Assert.AreEqual(10, result);

            Assert.AreEqual(1, short.Parse("1E0", System.Globalization.NumberStyles.AllowExponent));
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("1E0"));

            Assert.AreEqual(1, short.Parse("1E1", System.Globalization.NumberStyles.AllowExponent));
            Assert.AreEqual(10, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("1E1"));

            // Parse 1.0
            Assert.AreEqual(1, short.Parse("1.0", System.Globalization.NumberStyles.AllowDecimalPoint));
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int16Impl.Parse("1.0"));

            return;
        }
    }

    [TestClass]
    public class UnitTestOfUInt16
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("+1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("-0"));

            // Parse UInt16.MaxValue
            Assert.AreEqual(65535, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("65535"));

            // Parse +UInt16.MaxValue
            Assert.AreEqual(65535, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("+65535"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 65536
            //ushort.Parse("65536");
            Assert.AreEqual(65536, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("65536"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -1
            //short.Parse("-1");
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("-1"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //ushort.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            ushort result;
            Assert.AreEqual(false, ushort.TryParse("2A", out result));
            Assert.AreEqual(0, result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt16Impl.TryParse("2A", out result));
            Assert.AreEqual(0, result);

            return;
        }
    }
}
