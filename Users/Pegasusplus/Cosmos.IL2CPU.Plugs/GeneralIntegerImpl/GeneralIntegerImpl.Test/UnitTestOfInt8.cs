using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneralIntegerImpl.Test
{
    [TestClass]
    public class UnitTestOfInt8
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("+1"));

            // Parse -1
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("-1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("-0"));

            // Parse Int8.MinValue
            Assert.AreEqual(-128, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("-128"));

            // Parse Int8.MaxValue
            Assert.AreEqual(127, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("127"));

            // Parse +Int8.MaxValue
            Assert.AreEqual(127, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("+127"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 128
            //sbyte.Parse("128");
            Assert.AreEqual(128, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("128"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -129
            //sbyte.Parse("-129");
            Assert.AreEqual(-129, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("-129"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //sbyte.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            sbyte result;
            string strValue = "2A";
            Assert.AreEqual(false, sbyte.TryParse(strValue, out result));
            Assert.AreEqual(0, result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.TryParse(strValue, out result));
            Assert.AreEqual(0, result);

            return;
        }

        [TestMethod]
        public void FormatSupport()
        {
            // Parse Exponent
            sbyte result;
            Assert.AreEqual(true, sbyte.TryParse("1E0", System.Globalization.NumberStyles.AllowExponent, null, out result));
            Assert.AreEqual(1, result);
            Assert.AreEqual(true, sbyte.TryParse("1E1", System.Globalization.NumberStyles.AllowExponent, null, out result));
            Assert.AreEqual(10, result);

            Assert.AreEqual(1, sbyte.Parse("1E0", System.Globalization.NumberStyles.AllowExponent));
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("1E0"));

            Assert.AreEqual(1, sbyte.Parse("1E1", System.Globalization.NumberStyles.AllowExponent));
            Assert.AreEqual(10, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("1E1"));

            // Parse 1.0
            Assert.AreEqual(1, sbyte.Parse("1.0", System.Globalization.NumberStyles.AllowDecimalPoint));
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("1.0"));

            return;
        }
    }

    [TestClass]
    public class UnitTestOfUInt8
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("+1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("-0"));

            // Parse UInt8.MaxValue
            Assert.AreEqual(255, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("255"));

            // Parse +UInt8.MaxValue
            Assert.AreEqual(255, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("+255"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 256
            //sbyte.Parse("256");
            Assert.AreEqual(256, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("256"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -1
            //sbyte.Parse("-1");
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.Parse("-1"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //sbyte.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.Int8Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            byte result;
            Assert.AreEqual(false, byte.TryParse("2A", out result));
            Assert.AreEqual(0, result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt8Impl.TryParse("2A", out result));
            Assert.AreEqual(0, result);

            return;
        }
    }
}
