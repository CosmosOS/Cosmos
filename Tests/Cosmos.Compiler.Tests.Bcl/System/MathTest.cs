using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;
using Cosmos.Compiler.Tests.Bcl.Helper;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal class MathTest
    {
        public static void Execute()
        {
            double result;

            // Test with small number
            result = Math.Sqrt(16);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 4), "Sqrt does not produce accurate result with small input");

            // Test with large number
            result = Math.Sqrt(2432146.513);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1559.53406920143), "Sqrt does not produce accurate result with large input");

            // Test with zero
            result = Math.Sqrt(0);
            Assert.IsTrue((result == 0), "Sqrt of zero must be zero");

            // Test with negative number
            result = Math.Sqrt(-433);
            Assert.IsTrue(double.IsNaN(result), "Sqrt of negative must return NaN");

            // Test with NaN
            result = Math.Sqrt(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Sqrt of NaN must return NaN");

            // Test with positive infinity
            result = Math.Sqrt(double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Sqrt of PositiveInfinity must return PositiveInfinity");

            #region Math.Exp

            //Test with integer
            result = Math.Exp(2);
            Assert.IsTrue((result == 7.38905609893065), "e^2 is equal to 7.38905609893065");

            //Test with double exponent
            result = Math.Exp(1.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 4.48168907033806), "e^1.5 returns correct result");

            #endregion Math.Exp

            #region Math.Pow

            //Test with integer power
            result = Math.Pow(2, 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 4), "2^2 gives accurate result");

            //Test with decimal power
            result = Math.Pow(9, 0.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, Math.Sqrt(9)), "9^0.5 gives same answer as sqrt(9)");

            //Test with negative base
            result = Math.Pow(-2, 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 4), "Math.Pow gives correct result when raising negative number to even power");

            result = Math.Pow(-2, 3);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -8), "Math.Pow gives correct result when raising negative number to odd power");

            //Test with negative power
            result = Math.Pow(2, -1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.5), "Pow gives correct results when handling negative powers");

            #endregion Math.Pow
        }
    }
}
