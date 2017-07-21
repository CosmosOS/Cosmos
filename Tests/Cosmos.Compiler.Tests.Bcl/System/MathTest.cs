using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;
using Cosmos.Compiler.Tests.Bcl.Helper;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class MathTest
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
        }
    }
}
