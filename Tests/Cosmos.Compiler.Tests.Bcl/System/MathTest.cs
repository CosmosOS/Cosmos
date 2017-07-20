using System;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    class MathTest
    {
        public static void Execute()
        {
            double acceptedError = 0.000001;
            double result;
            double expectedResult;
            double resultError;

            // Test with small number
            result = Math.Sqrt(16);
            expectedResult = 4;
            resultError = Math.Abs(result - expectedResult);

            Assert.IsTrue((resultError < acceptedError), "Sqrt does not produce accurate result with small input");

            // Test with large number
            result = Math.Sqrt(2432146.513);
            expectedResult = 1559.53406920143;
            resultError = Math.Abs(result - expectedResult);

            Assert.IsTrue((resultError < acceptedError), "Sqrt does not produce accurate result with large input");

            // Test with zero
            result = Math.Sqrt(0);
            expectedResult = 0;

            Assert.IsTrue((result == expectedResult), "Sqrt of zero must be zero");

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
