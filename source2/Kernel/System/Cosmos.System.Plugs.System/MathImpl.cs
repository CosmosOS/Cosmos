using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.IL2CPU.Plugs;
using Cosmos.System;

namespace Cosmos.System.Plugs.System
{
    [Plug(Target = typeof(global::System.Math))]
    public class MathImpl
    {

        #region Internal Constants
        private const double sq2p1 = 2.414213562373095048802e0F;
        private const double sq2m1 = .414213562373095048802e0F;
        private const double pio2 = 1.570796326794896619231e0F;
        private const double pio4 = .785398163397448309615e0F;
        private const double log2e = 1.4426950408889634073599247F;
        private const double sqrt2 = 1.4142135623730950488016887F;
        private const double ln2 = 6.93147180559945286227e-01F;
        private const double atan_p4 = .161536412982230228262e2F;
        private const double atan_p3 = .26842548195503973794141e3F;
        private const double atan_p2 = .11530293515404850115428136e4F;
        private const double atan_p1 = .178040631643319697105464587e4F;
        private const double atan_p0 = .89678597403663861959987488e3F;
        private const double atan_q4 = .5895697050844462222791e2F;
        private const double atan_q3 = .536265374031215315104235e3F;
        private const double atan_q2 = .16667838148816337184521798e4F;
        private const double atan_q1 = .207933497444540981287275926e4F;
        private const double atan_q0 = .89678597403663861962481162e3F;
        #endregion

        public const double PI = 3.1415926535897931;
        public const double E = 2.71828182845904523536;

        #region Abs
        public static double Abs(double value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }

        public static float Abs(float value)
        {
            if (value < 0)
            {
                return -value;
            }
            else
            {
                return value;
            }
        }
        #endregion

        #region Acos
        public static double Acos(double x)
        {
            if ((x > 1.0) || (x < -1.0))
                throw new ArgumentOutOfRangeException("Domain error");
            return (pio2 - Asin(x));
        }
        #endregion

        #region Asin
        public static double Asin(double x)
        {
            if (x > 1.0F)
            {
                throw new ArgumentOutOfRangeException("Domain error");
            }
            double sign = 1F, temp;
            if (x < 0.0F)
            {
                x = -x;
                sign = -1.0F;
            }
            temp = Sqrt(1.0F - (x * x));
            temp = ((x > 0.7) ? (pio2 - Atan(temp / x)) : (Atan(x / temp)));
            return (sign * temp);
        }
        #endregion

        #region Atan
        public static double Atan(double x)
        {
            return ((x > 0F) ? atans(x) : (-atans(-x)));
        }
        #endregion

        #region Atan2
        public static double Atan2(double y, double x)
        {
            return (((x + y) == x) ? (((x == 0F) & (y == 0F)) ? 0F : ((x >= 0F) ? pio2 : (-pio2))) : ((y < 0F) ? ((x >= 0F) ? ((pio2 * 2) - atans((-x) / y)) : (((-pio2) * 2) + atans(x / y))) : ((x > 0F) ? atans(x / y) : -atans((-x) / y))));
        }
        #endregion

        #region Ceiling
        public static double Ceiling(double a)
        { 
            // should be using assembler for bigger values than int or long max
            if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
                return a;
            int i = (a - (int)a > 0) ? (int)(a + 1) : (int)a;
            return i;
        }
        #endregion

        #region Cos
        public static double Cos(double x)
        {
            if (x < 0)
                x = -x;
            byte quadrand = 0;
            if ((x > (PI / 2F)) && (x < (PI)))
            {
                quadrand = 1;
                x = PI - x;
            }
            if ((x > (PI)) && (x < ((3F * PI) / 2)))
            {
                quadrand = 2;
                x = PI - x;
            }
            if ((x > ((3F * PI) / 2)))
            {
                quadrand = 3;
                x = (2F * PI) - x;
            }
            const double c1 = 0.99999999999925182;
            const double c2 = -0.49999999997024012;
            const double c3 = 0.041666666473384543;
            const double c4 = -0.001388888418000423;
            const double c5 = 0.0000248010406484558;
            const double c6 = -0.0000002752469638432;
            const double c7 = 0.0000000019907856854;
            double x2 = x * x; ;
            return (((quadrand == 0) || (quadrand == 3)) ? (c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2)))))) : (-(c1 + x2 * (c2 + x2 * (c3 + x2 * (c4 + x2 * (c5 + x2 * (c6 + c7 * x2))))))));
            //Cos(x) = Sin(90degrees - radians)
            //return Sin((Math.PI / 2) - a);
        }
        #endregion

        #region Cosh
        public static double Cosh(double x)
        {
            if (x < 0.0F)
                x = -x;
            return ((x == 0F) ? 1F : ((x <= (ln2 / 2)) ? (1 + (_power((Exp(x) - 1), 2) / (2 * Exp(x)))) : ((x <= 22F) ? ((Exp(x) + (1 / Exp(x))) / 2) : (0.5F * (Exp(x) + Exp(-x))))));
        }
        #endregion

        #region Exp
        public static double Exp(double x)
        {
            double c;
            int n = 1;
            double ex = 1F;
            double m = 1F;
            while (x > 10.000F)
            {
                m *= 22026.4657948067;
                x -= 10F;
            }
            while (x > 01.000F)
            {
                m *= E;
                x -= 1F;
            }
            while (x > 00.100F)
            {
                m *= 1.10517091807565; ;
                x -= 0.1F;
            }
            while (x > 00.010F)
            {
                m *= 1.01005016708417;
                x -= 0.01F;
            }
            for (int y = 1; y <= 4; y++)
            {
                c = _power(x, y);
                ex += c / (double)n;
                n *= (y + 1);
            }
            return ex * m;
        }
        #endregion

        #region Floor
        public static double Floor(double a)
        { 
            // should be using assembler for bigger values than int or long max
            if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
                return a;
            int i = (a - (int)a < 0) ? (int)(a - 1) : (int)a;
            return i;
        }
        #endregion

        #region Log (base e)
        public static double Log(double x)
        {
            return Log(x, E);
        }
        #endregion

        #region Log (base specified)
        public static double Log(double x, double newBase)
        {
            if (x == 0.0F)
            {
                return double.NegativeInfinity;
            }
            if ((x < 1.0F) && (newBase < 1.0F))
            {
                throw new ArgumentOutOfRangeException("can't compute Log");
            }

            double partial = 0.5F;
            double integer = 0F;
            double fractional = 0.0F;

            while (x < 1.0F)
            {
                integer -= 1F;
                x *= newBase;
            }

            while (x >= newBase)
            {
                integer += 1F;
                x /= newBase;
            }

            x *= x;

            while (partial >= 2.22045e-016)
            {
                if (x >= newBase)
                {
                    fractional += partial;
                    x = x / newBase;
                }
                partial *= 0.5F;
                x *= x;
            }

            return (integer + fractional);
        }
        #endregion

        #region Log10
        public static double Log10(double x)
        {
            return Log(x, 10F);
        }
        #endregion
        
        #region Pow
        public static double Pow(double x, double y)
        {
            if (x <= 0.0F)
            {
                double temp = 0F;
                long l;
                if (x == 0.0F && y <= 0.0F)
                    throw new ArgumentException();

                l = (long)Floor(y);
                if (l != y)
                    temp = Exp(y * Log(-x));
                if ((l % 2) == 1)
                    temp = -temp;

                return (temp);
            }

            return (Exp(y * Log(x)));
            //if (y == 0)
            //{
            //    return 1;
            //}
            //else if (y == 1)
            //{
            //    return x;
            //}
            //else
            //{
            //    double xResult = x;

            //    for (int i = 2; i <= y; i++)
            //    {
            //        xResult = xResult * x;
            //    }

            //    return xResult;
            //}
        }
        #endregion

        #region Sin
        public static double Sin(double x)
        {
            return Cos((PI / 2.0F) - x);
            
            //// should be using assembler instruction
            //bool signSwitch = false;
            //double result = 0;

            ////TO radians
            //double radians = a;// *(Math.PI / 180);

            //if (radians > Math.PI)
            //{
            //    radians = radians - Math.PI;
            //    signSwitch = true;
            //}
            //else if (a > Math.PI / 2)
            //{
            //    radians = radians - Math.PI;
            //    signSwitch = true;
            //}

            ////Temp function to increase precision make more factorial calculations
            //result = (radians) - (Math.Pow(radians, 3) / Factorial(3));
            //result += (Math.Pow(radians, 5) / Factorial(5)) - (Math.Pow(radians, 7) / Factorial(7)) + (Math.Pow(radians, 9) / Factorial(9));

            ///* USE WHEN Modulus Works
            // * int sign = 0;
            //for (int i = 3; i < 19; i += 2)
            //{
            //    if (sign % 2 == 0)
            //        result += -Math.Pow(radians, i) / fact(i);
            //    else
            //        result += Math.Pow(radiansa, i) / fact(i);
            //    sign++;
            //}*/

            //if (signSwitch)
            //    return result * -1;
            //else
            //    return result;
        }
        #endregion

        #region Sinh
        public static double Sinh(double x)
        {
            if (x < 0F)
                x = -x;

            if (x <= 22F)
            {
                double Ex_1 = Tanh(x / 2) * (Exp(x) + 1);
                return ((Ex_1 + (Ex_1 / (Ex_1 - 1))) / 2);
            }
            else
            {
                return (Exp(x) / 2);
            }
        }
        #endregion

        #region Sqrt
        public static double Sqrt(double x)
        {
            double i = 0;
            double x1 = 0.0F;
            double x2 = 0.0F;

            if (x == 0F)
                return 0F;

            while ((i * i) <= x)
            {
                i += 0.1F;
            }

            x1 = i;
            // this originally used another variable here, 
            // but the use of i was done, thus it's faster
            // to re-use the variable.
            for (i = 0; i < 10; i++)
            {
                x2 = x;
                x2 /= x1;
                x2 += x1;
                x2 /= 2;
                x1 = x2;
            }
            return x2;
        }
        #endregion

        #region Tan
        public static double Tan(double x)
        {
            return (Sin(x) / Cos(x));
        }
        #endregion

        #region Tanh
        public static double Tanh(double x)
        {
            return (expm1(2F * x) / (expm1(2F * x) + 2F));
        }
        #endregion

        #region Truncate
        public static double Truncate(double x)
        {
            return ((x == 0) ? 0F : ((x > 0F) ? Floor(x) : Ceiling(x)));
        }
        #endregion

        //#region Factorial (only used in Sin(), not plug )
        //public static int Factorial(int n)
        //{
        //    if (n == 0)
        //        return 1;
        //    else
        //        return n * Factorial(n - 1);
        //}
        //#endregion

        #region Internaly used functions

        #region expm1
        private static double expm1(double x)
        {
            double u = Exp(x);
            return ((u == 1.0F) ? x : ((u - 1.0F == -1.0F) ? -1.0F : ((u - 1.0F) * x / Log(u))));
        }
        #endregion

        #region _power
        private static double _power(double x, int c)
        {
            if (c == 0)
                return 1.0F;

            int _c;
            double ret = x;

            if (c >= 0f)
            {
                for (_c = 1; _c < c; _c++)
                    ret *= ret;
            }
            else
            {
                for (_c = 1; _c < c; _c++)
                    ret /= ret;
            }

            return ret;
        }
        #endregion

        #region atans
        private static double atans(double x)
        {
            return ((x < sq2m1) ? atanx(x) : ((x > sq2p1) ? (pio2 - atanx(1.0F / x)) : (pio4 + atanx((x - 1.0F) / (x + 1.0F)))));
        }
        #endregion

        #region atanx
        private static double atanx(double x)
        {
            double ArgSquared = x * x;
            return (((((atan_p4 * ArgSquared + atan_p3) * ArgSquared + atan_p2) * ArgSquared + atan_p1) * ArgSquared + atan_p0) / (((((ArgSquared + atan_q4) * ArgSquared + atan_q3) * ArgSquared + atan_q2) * ArgSquared + atan_q1) * ArgSquared + atan_q0) * x);
        }
        #endregion

        #endregion


    }
}