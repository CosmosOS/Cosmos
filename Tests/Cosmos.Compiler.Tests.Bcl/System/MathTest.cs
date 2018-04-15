using System;

using Cosmos.TestRunner;

namespace Cosmos.Compiler.Tests.Bcl.System
{
    internal static class MathTest
    {
        public static void Execute()
        {
            double result;

            #region Math.Asin

            result = Math.Asin(1.1);
            Assert.IsTrue(double.IsNaN(result), "Math.Asin returns NaN for values larger than 1");

            result = Math.Asin(-1.1);
            Assert.IsTrue(double.IsNaN(result), "Math.Asin returns NaN for values smaller than -1");

            result = Math.Asin(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.5707963267949), "Asin returns correct value for 1");

            result = Math.Asin(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1.5707963267949), "Asin returns correct value for -1");

            result = Math.Asin(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Asin returns correct value for 9");

            #endregion Math.Asin

            #region Math.Acos

            result = Math.Acos(1.1);
            Assert.IsTrue(double.IsNaN(result), "Math.Acos returns NaN for values larger than 1");

            result = Math.Acos(-1.1);
            Assert.IsTrue(double.IsNaN(result), "Math.Acos returns NaN for values smaller than -1");

            result = Math.Acos(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Acos returns correct value for 1");

            result = Math.Acos(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, Math.PI), "Acos returns correct value for -1");

            result = Math.Acos(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.5707963267949), "Acos returns correct value for 9");

            #endregion Math.Acos

            #region Ceiling

            result = Math.Ceiling(0d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Ceiling gives correct value for 0");

            result = Math.Ceiling(1d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Ceiling gives correct value for 1");

            result = Math.Ceiling(-1d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1), "Ceiling gives correct value for -1");

            result = Math.Ceiling(2.5d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 3), "Ceiling gives correct value for 2.5");

            result = Math.Ceiling(-2.5d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -2), "Ceiling gives correct value for -2.5");

            result = Math.Ceiling(11.2d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 12), "Ceiling gives correct value for 11.2");

            result = Math.Ceiling(-11.2d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -11), "Ceiling gives correct value for -11.2");

            result = Math.Ceiling(32.8d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 33), "Ceiling gives correct value for 32.8");

            result = Math.Ceiling(-32.8d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -32), "Ceiling gives correct value for -32.8");

            result = Math.Ceiling(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Ceiling gives correct value for NaN");

            result = Math.Ceiling(double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Ceiling gives correct value for INF");

            result = Math.Ceiling(double.NegativeInfinity);
            Assert.IsTrue(double.IsNegativeInfinity(result), "Ceiling gives correct value for -INF");

            result = Math.Ceiling((double)int.MaxValue + 2.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, (double)int.MaxValue + 3), "Ceiling works for values larger than an int can hold. " + result + " expected " + (double)int.MaxValue + 3);

            #endregion Ceiling

            #region Floor

            result = Math.Floor(0d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Floor gives correct value for 0");

            result = Math.Floor(1d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Floor gives correct value for 1");

            result = Math.Floor(-1d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1), "Floor gives correct value for -1");

            result = Math.Floor(2.5d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2), "Floor gives correct value for 2.5");

            result = Math.Floor(-2.5d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -3), "Floor gives correct value for -2.5");

            result = Math.Floor(11.2d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 11), "Floor gives correct value for 11.2");

            result = Math.Floor(-11.2d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -12), "Floor gives correct value for -11.2");

            result = Math.Floor(32.8d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 32), "Floor gives correct value for 32.8");

            result = Math.Floor(-32.8d);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -33), "Floor gives correct value for -32.8");

            result = Math.Floor(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Floor gives correct value for NaN");

            result = Math.Floor(double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Floor gives correct value for INF");

            result = Math.Floor(double.NegativeInfinity);
            Assert.IsTrue(double.IsNegativeInfinity(result), "Floor gives correct value for -INF");

            result = Math.Floor((double)int.MaxValue + 2.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, (double)int.MaxValue + 2), "Floor works for values larger than an int can hold. " + result + " expected " + (double)int.MaxValue + 2);

            #endregion Floor

            #region Math.Cos

            result = Math.Cos(4);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.6536436208636), "Math.Cos works with positive number");

            result = Math.Cos(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Cos works with 0");

            result = Math.Cos(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.5403023058681), "Cos gives correct answer for 1");

            result = Math.Cos(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.5403023058681), "Cos gives correct answer for -1");

            result = Math.Cos(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Cos works with NaN");

            result = Math.Cos(double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Cos works with INF");

            result = Math.Cos(double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Cos works with -INF");

            result = Math.Cos(Math.PI);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1), "Cos gives correct answer for PI");

            result = Math.Cos(Math.PI / 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 6.12323399573677E-17), "Cos gives correct answer for PI / 2");

            result = Math.Cos(Math.PI / 3);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.5), "Cos gives correct answer for PI / 3");

            #endregion Math.Cos

            #region Math.Log

            result = Math.Log(10);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2.30258509299405), "Math.Log base e works with positive numbers");

            result = Math.Log(Math.E);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Math.Log base gives correct value for e");

            result = Math.Log(Math.E * Math.E);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2), "Math.Log base gives correct value for e^2");

            result = Math.Log(0);
            Assert.IsTrue(double.IsNegativeInfinity(result), "Math.Log base e gives correct value for 0");

            result = Math.Log(-1.5);
            Assert.IsTrue(double.IsNaN(result), "Math.Log base e gives correct answer for negative numbers");

            result = Math.Log(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Log base e returns NaN for NaN");

            result = Math.Log(double.PositiveInfinity);
            Assert.IsTrue(double.IsPositiveInfinity(result), "Log base e return INF for INF");

            result = Math.Log(double.NegativeInfinity);
            Assert.IsTrue(double.IsNaN(result), "Log base e return NaN for -INF");

            result = Math.Log10(100);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2), "Math.Log10 gives correct value for integer exponent");

            result = Math.Log(50);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 3.91202300542814), "Log10 gives correct value for double exponent");

            result = Math.Log10(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Log10 returns NaN when being called with NaN");

            result = Math.Log(4, 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2), "Log with base gives correct result with called with int values");

            result = Math.Log(7.5, 2.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 2.19897784671579), "Log with base gives correct result with double values");

            #endregion Math.Log

            #region Math.Sin

            result = Math.Sin(4);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.7568024953079), "Math.Sin works with positive number");

            result = Math.Sin(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Sin works with 0");

            result = Math.Sin(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.8414709848079), "Sin gives correct answer for 1");

            result = Math.Sin(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.8414709848079), "Sin gives correct answer for -1");

            result = Math.Sin(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Sin works with NaN");

            result = Math.Sin(double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Sin works with INF");

            result = Math.Sin(double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Sin works with -INF");

            result = Math.Sin(Math.PI);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.22464679914735E-16), "Sin gives correct answer for PI");

            result = Math.Sin(Math.PI / 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1), "Sin gives correct answer for PI / 2");

            result = Math.Sin(Math.PI / 3);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.866025403784439), "Sin gives correct answer for PI / 3");

            #endregion Math.Sin

            #region Math.Sqrt

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

            #endregion Math.Sqrt

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

            #region Math.Tan

            result = Math.Tan(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Tan works with 0");

            result = Math.Tan(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.5574077246549), "Tan works with 1");

            result = Math.Tan(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1.5574077246549), "Tan works with -1");

            result = Math.Tan(10);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.648360827459087), "Tan works with big numbers such as 10");

            result = Math.Tan(-10);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.648360827459087), "Tan works with larger negative numbers");

            result = Math.Tan(0.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.54630248984379), "Tan works with doubles");

            result = Math.Tan(-0.5);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.54630248984379), "Tan works with negative doubles");

            result = Math.Tan(Math.PI);
            Assert.IsTrue(result <= -.22464679914735E-16, "Tan gives matching result for Pi but mathematically inaccurate result. " + result);

            result = Math.Tan(Math.PI / 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.63312393531954E+16), "Tan gives result matching normal Math function but incorrect in mathematical sense");

            result = Math.Tan(Math.PI / 3);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, Math.Sqrt(3)), "Tan gives correct value for PI / 3");

            result = Math.Tan(double.NegativeInfinity);
            Assert.IsTrue(double.IsNaN(result), "Tan return Nan for -INF");

            result = Math.Tan(double.PositiveInfinity);
            Assert.IsTrue(double.IsNaN(result), "Tan returns Nan for INF");

            result = Math.Tan(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Tan returns Nan for Nan");

            #endregion Math.Tan

            #region Math.Atan

            result = Math.Atan(0);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0), "Atan works with 0");

            result = Math.Atan(1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.785398163397448), "Atan works with 1");

            result = Math.Atan(-1);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -0.785398163397448), "Atan works with -1");

            result = Math.Atan(Math.PI);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.26262725567891), "Atan works with PI");

            result = Math.Atan(Math.PI / 2);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.00388482185389), "Atan works with PI / 2");

            result = Math.Atan(Math.PI / 3);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 0.808448792630022), "Atan works with PI / 3");

            result = Math.Atan(double.NaN);
            Assert.IsTrue(double.IsNaN(result), "Atan returns NaN for NaN");

            result = Math.Atan(double.PositiveInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, 1.5707963267949), "Atan works with INF");

            result = Math.Atan(double.NegativeInfinity);
            Assert.IsTrue(EqualityHelper.DoublesAreEqual(result, -1.5707963267949), "Atan works with -INF");

            #endregion Math.Atan
        }
    }
}
