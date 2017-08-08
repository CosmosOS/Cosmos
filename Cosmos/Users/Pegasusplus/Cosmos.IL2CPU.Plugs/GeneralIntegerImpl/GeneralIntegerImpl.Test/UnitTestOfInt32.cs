using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GeneralIntegerImpl.Test
{
    [TestClass]
    public class UnitTestOfInt32
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("+1"));

            // Parse -1
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("-1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("-0"));

            // Parse Int32.MinValue
            Assert.AreEqual(-2147483648, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("-2147483648"));

            // Parse Int32.MaxValue
            Assert.AreEqual(2147483647, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("2147483647"));

            // Parse +Int32.MaxValue
            Assert.AreEqual(2147483647, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("+2147483647"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 2147483648
            //short.Parse("2147483648");
            Assert.AreEqual(2147483648, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("2147483648"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -2147483649
            //short.Parse("-2147483649");
            Assert.AreEqual(-2147483649, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("-2147483649"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //int.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            int result;
            string strValue = "2A";
            Assert.AreEqual(false, int.TryParse(strValue, out result));
            Assert.AreEqual(0, result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.TryParse(strValue, out result));
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
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("1E0"));

            Assert.AreEqual(1, short.Parse("1E1", System.Globalization.NumberStyles.AllowExponent));
            Assert.AreEqual(10, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("1E1"));

            // Parse 1.0
            Assert.AreEqual(1, short.Parse("1.0", System.Globalization.NumberStyles.AllowDecimalPoint));
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.Int32Impl.Parse("1.0"));

            return;
        }
    }

    [TestClass]
    public class UnitTestOfUInt32
    {
        [TestMethod]
        public void ParseSpecialValues()
        {
            // Parse 1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("1"));

            // Parse +1
            Assert.AreEqual(1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("+1"));

            // Parse 0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("0"));

            // Parse +0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("+0"));

            // Parse -0
            Assert.AreEqual(0, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("-0"));

            // Parse UInt32.MaxValue
            Assert.AreEqual(4294967295, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("4294967295"));

            // Parse +UInt32.MaxValue
            Assert.AreEqual(4294967295, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("+4294967295"));

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseOverflowValue()
        {
            // Parse 4294967296
            //uint.Parse("4294967296");
            Assert.AreEqual(4294967296, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("4294967296"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.OverflowException))]
        public void ParseUnderflowValue()
        {
            // Parse -1
            //short.Parse("-1");
            Assert.AreEqual(-1, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("-1"));
            Assert.Fail("Should generate System.OverflowException");

            return;
        }

        [TestMethod]
        [ExpectedException(typeof(System.FormatException))]
        public void ParseInvalidValue()
        {
            // Parse 2A
            //uint.Parse("2A");
            Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.Parse("2A");
            Assert.Fail("Should generate System.FormatException");

            return;
        }

        [TestMethod]
        public void TryParseInvalidValue()
        {
            // Parse 2A
            uint result;
            Assert.AreEqual(false, uint.TryParse("2A", out result));
            // Can not pass, since Int32.0 != UInt32.0, :(
            // Assert.AreEqual(0, result);
            Assert.IsTrue(0 == result);

            Assert.AreEqual(false, Cosmos.IL2CPU.IL.CustomImplementations.System.UInt32Impl.TryParse("2A", out result));
            // Can not pass, since Int32.0 != UInt32.0, :(
            // Assert.AreEqual(0, result);
            Assert.IsTrue(0 == result);

            return;
        }
    }
}
