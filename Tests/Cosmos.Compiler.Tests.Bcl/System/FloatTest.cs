using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;
using Cosmos.Compiler.Tests.Bcl.Helper;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal class FloatTest
    {
        public static void Execute()
        {
            float value;


            #region Parsing
            value = float.Parse("0.4");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.4), "simple parsing of float works");

            value = float.Parse("+0.3");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.3), "parsing of float with positive sign works!");

            value = float.Parse("-0.4");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, -0.4), "parsing of negative float works!");

            value = float.Parse("    0.7     ");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.7), "float parsing ignores leading and trailing whitespaces");

            value = float.Parse("0.4E1");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 4), "float parsing takes in account E");

            value = float.Parse("0.4E-1");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 0.04), "float parsing works with negative E");

            Assert.IsFalse(float.TryParse("asd4", out value), "float TryParse returns false when it fails");

            Assert.IsTrue(float.TryParse("2.3", out value), " float TryParse returns true when it works");
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(value, 2.3), "float TryParse returns correct result when it works");
            #endregion
        }
    }
}
