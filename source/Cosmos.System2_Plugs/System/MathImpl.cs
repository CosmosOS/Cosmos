using System;

using Cosmos.Debug.Kernel;
using Cosmos.IL2CPU.API;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Math))]
    public static class MathImpl
    {
        internal static Debugger mDebugger = new Debugger("System", "Math Plugs");

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

        public const double PI = 3.1415926535897932384626433832795;
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
            if (x > 0.7)
            {
                temp = pio2 - Atan(temp / x);
            }
            else
            {
                temp = Atan(x / temp);
            }
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
        public static double Atan2(double x, double y)
        {
            if ((x + y) == x)
            {
                if ((x == 0F) & (y == 0F))
                    return 0F;
                if (x >= 0.0F)
                    return pio2;
                return (-pio2);
            }
            if (y < 0.0F)
            {
                if (x >= 0.0F)
                    return ((pio2 * 2) - atans((-x) / y));
                return (((-pio2) * 2) + atans(x / y));
            }
            if (x > 0.0F)
            {
                return (atans(x / y));
            }
            return (-atans((-x) / y));

            //return (((x + y) == x) ? (((x == 0F) & (y == 0F)) ? 0F : ((x >= 0F) ? pio2 : (-pio2))) : ((y < 0F) ? ((x >= 0F) ? ((pio2 * 2) - atans((-x) / y)) : (((-pio2) * 2) + atans(x / y))) : ((x > 0F) ? atans(x / y) : -atans((-x) / y))));
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
            // First we need to anchor it to a valid range.
            while (x > (2 * PI))
            {
                x -= (2 * PI);
            }

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
                x = x - PI;
            }
            if ((x > ((3F * PI) / 2)))
            {
                quadrand = 3;
                x = (2F * PI) - x;
            }
            const double c1 = 0.9999999999999999999999914771;
            const double c2 = -0.4999999999999999999991637437;
            const double c3 = 0.04166666666666666665319411988;
            const double c4 = -0.00138888888888888880310186415;
            const double c5 = 0.00002480158730158702330045157;
            const double c6 = -0.000000275573192239332256421489;
            const double c7 = 0.000000002087675698165412591559;
            const double c8 = -0.0000000000114707451267755432394;
            const double c9 = 0.0000000000000477945439406649917;
            const double c10 = -0.00000000000000015612263428827781;
            const double c11 = 0.00000000000000000039912654507924;
            double x2 = x * x;
            if (quadrand == 0 || quadrand == 3)
            {
                return (c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * (c5 + (x2 * (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + (x2 * (c10 + (x2 * c11))))))))))))))))))));
            }
            else
            {
                return -(c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * (c5 + (x2 * (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + (x2 * (c10 + (x2 * c11))))))))))))))))))));
            }
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

        #region Round
        public static double Round(double d)
        {
            return ((Math.Floor(d) % 2 == 0) ? Math.Floor(d) : Math.Ceiling(d));
        }
        #endregion

        #region Sin
        public static double Sin(double x)
        {
            // First we need to anchor it to a valid range.
            while (x > (2 * PI))
            {
                x -= (2 * PI);
            }

            return Cos((PI / 2.0F) - x);
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
            long x1;
            double x2;
            int i;

            if (double.IsNaN(x) || x < 0)
                return double.NaN;

            if (double.IsPositiveInfinity(x))
                return double.PositiveInfinity;

            if (x == 0F)
                return 0F;

            // Approximating the square root value
            // This makes use of IEEE 754 double-precision floating point format
            // Sign: 1 bit, Exponent: 11 bits, Signficand: 52 bits
            x1 = BitConverter.DoubleToInt64Bits(x);
            x1 -= 1L << 53;
            x1 >>= 1;
            x1 += 1L << 61;

            x2 = BitConverter.Int64BitsToDouble(x1);

            // Use Newton's Method
            for(i = 0; i < 5; i++)
            {
                x2 = x2 - (x2 * x2 - x) / (2 * x2);
            }

            return x2;
        }
        #endregion

        #region Tan
        public static double Tan(double x)
        {
            // First we need to anchor it to a valid range.
            while (x > (2 * PI))
            {
                x -= (2 * PI);
            }

            byte octant = (byte)Floor(x * (1 / (PI / 4)));
            switch (octant)
            {
                case 0:
                    x = x * (4 / PI);
                    break;
                case 1:
                    x = ((PI / 2) - x) * (4 / PI);
                    break;
                case 2:
                    x = (x - (PI / 2)) * (4 / PI);
                    break;
                case 3:
                    x = (PI - x) * (4 / PI);
                    break;
                case 4:
                    x = (x - PI) * (4 / PI);
                    break;
                case 5:
                    x = ((3.5 * PI) - x) * (4 / PI);
                    break;
                case 6:
                    x = (x - (3.5 * PI)) * (4 / PI);
                    break;
                case 7:
                    x = ((2 * PI) - x) * (4 / PI);
                    break;
            }


            const double c1 = 4130240.588996024013440146267;
            const double c2 = -349781.8562517381616631012487;
            const double c3 = 6170.317758142494245331944348;
            const double c4 = -27.94920941380194872760036319;
            const double c5 = 0.0175143807040383602666563058;
            const double c6 = 5258785.647179987798541780825;
            const double c7 = -1526650.549072940686776259893;
            const double c8 = 54962.51616062905361152230566;
            const double c9 = -497.495460280917265024506937;
            double x2 = x * x;
            if (octant == 0 || octant == 4)
            {
                return ((x * (c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * c5))))))))) / (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + x2))))))));
            }
            else if (octant == 1 || octant == 5)
            {
                return (1 / ((x * (c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * c5))))))))) / (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + x2)))))))));
            }
            else if (octant == 2 || octant == 6)
            {
                return (-1 / ((x * (c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * c5))))))))) / (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + x2)))))))));
            }
            else // octant == 3 || octant == 7
            {
                return -((x * (c1 + (x2 * (c2 + (x2 * (c3 + (x2 * (c4 + (x2 * c5))))))))) / (c6 + (x2 * (c7 + (x2 * (c8 + (x2 * (c9 + x2))))))));
            }
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
            if (x < sq2m1)
            {
                return atanx(x);
            }
            else if (x > sq2p1)
            {
                return (pio2 - atanx(1.0F / x));
            }
            else
            {
                return (pio4 + atanx((x - 1.0F) / (x + 1.0F)));
            }
        }
        #endregion

        #region atanx
        private static double atanx(double x)
        {
            double argsq, value;

            /* get denormalized add in following if range arg**10 is much smaller
                than q1, so check for that case
            */
            if ((x > -.01) && (x < .01))
            {
                value = (atan_p0 / atan_q0);
            }
            else
            {
                argsq = x * x;
                value = ((((atan_p4 * argsq + atan_p3) * argsq + atan_p2) * argsq + atan_p1) * argsq + atan_p0) / (((((argsq + atan_q4) * argsq + atan_q3) * argsq + atan_q2) * argsq + atan_q1) * argsq + atan_q0);
            }
            return value * x;

            //if (-.01 < arg && arg < .01)
            //    value = p0 / q0;
            //double ArgSquared = x * x;
            //return
            //    (((((atan_p4 * ArgSquared + atan_p3) * ArgSquared + atan_p2) * ArgSquared + atan_p1) * ArgSquared + atan_p0)
            //    /
            //    (((((ArgSquared + atan_q4) * ArgSquared + atan_q3) * ArgSquared + atan_q2) * ArgSquared + atan_q1) * ArgSquared + atan_q0) * x);
        }
        #endregion

        #endregion
    }
}
