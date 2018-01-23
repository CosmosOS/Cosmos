using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class MathTest
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

            result = Math.Exp(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "e^0 gives correct result");

            result = Math.Exp(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, Math.E), "e^1 gives correct result");

            result = Math.Exp(double.PositiveInfinity);
            Assert.IsTrue(result == double.PositiveInfinity, "e^Infinity gives correct result");

            result = Math.Exp(double.NegativeInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "e^-Infinity gives correct result");

            result = Math.Exp(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "e^NaN gives correct result");

            result = Math.Exp(double.MaxValue);
            Assert.IsTrue(double.IsPositiveInfinity(result), "e^0 gives correct result");

            result = Math.Exp(double.MinValue);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "e^0 gives correct result");

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

            //Have double as base
            result = Math.Pow(0.5, 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.25), "Pow gives correct solution with double base");

            //x = Nan
            result = Math.Pow(double.NaN, 2);
            Assert.IsTrue(double.IsNaN(result), "Pow gives correct solution when x is NaN");
            //Y = Nan
            result = Math.Pow(10, double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Pow gives correct solution when y is NaN");
            //y = 0
            result = Math.Pow(10, 0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Pow gives correct solution when y is 0");
            //x = -Inf y < 0 == 0
            result = Math.Pow(double.NegativeInfinity, -2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when X is -INF and y is negative");
            //x = -Inf y > 0 && y is even == Inf
            result = Math.Pow(double.NegativeInfinity, 2);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when x is -INF and y is even");
            //x is -INF and y is positive odd == -INF
            result = Math.Pow(double.NegativeInfinity, 3);
            Assert.IsTrue(double.IsNegativeInfinity(result), "Pow gives correct solution when x is -INF and y is odd");
            //x < 0 && y is not integer or special case
            result = Math.Pow(-3, 0.25);
            Assert.IsTrue(double.IsNaN(result), "Pow gives correct solution when x is negative and y is non integer");
            //x = -1 && y is Inf == Nan
            result = Math.Pow(-1, double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Pow gives correct solution when x is -1 and y is INF");
            //x = -1 && y is -Inf == Nan
            result = Math.Pow(-1, double.NegativeInfinity);
            Assert.IsTrue(double.IsNaN(result), "Pow gives correct solution when x is -1 and y is -INF");
            //-1 < x < 1 + y = -Inf
            result = Math.Pow(-0.25, double.NegativeInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when -1 < x < 0 and y = -INF");
            result = Math.Pow(0.25, double.NegativeInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when 0 < x < 1 and y = -INF");
            //-1 < x < 1 + y = Inf
            result = Math.Pow(-0.25, double.PositiveInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when -1 < x < 0 and y is INF");
            result = Math.Pow(0.25, double.PositiveInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when 0 < x < 1 and y is INF");
            //-1 > x || x > 1 + y = -Inf
            result = Math.Pow(-1.5, double.NegativeInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when x < -1 and y is -INF");
            result = Math.Pow(1.5, double.NegativeInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when x > 1 and y is -INF");
            //-1 > x || x > 1 + y = Inf
            result = Math.Pow(-1.25, double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when -1 > x and y = INF");
            result = Math.Pow(1.25, double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when x > 1 and y = INF");
            //x = 0 y > 0
            result = Math.Pow(0, 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when x = 0 any y > 0 ");
            //x = 0 y < 0
            result = Math.Pow(0, -3);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when x is 0 and y < 0 ");
            //x = inf y < 0
            result = Math.Pow(double.PositiveInfinity, -5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Pow gives correct solution when x is INF and y < 0 ");
            //x = inf y > 0
            result = Math.Pow(double.PositiveInfinity, 5);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Pow gives correct solution when x is INF and y > 0 ");

            #endregion Math.Pow
        }
    }
}
