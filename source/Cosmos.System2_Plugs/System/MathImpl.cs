using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

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

        #endregion Internal Constants

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

        #endregion Abs

        #region Acos

        public static double Acos(double x)
        {
            if ((x > 1.0) || (x < -1.0))
                throw new ArgumentOutOfRangeException("Domain error");
            return (pio2 - Asin(x));
        }

        #endregion Acos

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

        #endregion Asin

        #region Atan

        public static double Atan(double x)
        {
            return ((x > 0F) ? atans(x) : (-atans(-x)));
        }

        #endregion Atan

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

        #endregion Atan2

        #region Ceiling

        public static double Ceiling(double a)
        {
            // should be using assembler for bigger values than int or long max
            if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
                return a;
            int i = (a - (int)a > 0) ? (int)(a + 1) : (int)a;
            return i;
        }

        #endregion Ceiling

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

        #endregion Cos

        #region Cosh

        public static double Cosh(double x)
        {
            if (x < 0.0F)
                x = -x;
            return ((x == 0F) ? 1F : ((x <= (ln2 / 2)) ? (1 + (_power((Exp(x) - 1), 2) / (2 * Exp(x)))) : ((x <= 22F) ? ((Exp(x) + (1 / Exp(x))) / 2) : (0.5F * (Exp(x) + Exp(-x))))));
        }

        #endregion Cosh

        #region Exp

        /*
        * ====================================================
        * Copyright (C) 2004 by Sun Microsystems, Inc. All rights reserved.
        *
        * Permission to use, copy, modify, and distribute this
        * software is freely granted, provided that this notice
        * is preserved.
        * ====================================================
        */

        // Look at http://www.netlib.org/fdlibm/e_exp.c for more a in deth explanation

        public static double Exp(double x)
        {
            double y, hi = 0, lo = 0, c, t;
            int k = 0, xsb;

            const double o_threshold = 7.09782712893383973096e+02;
            const double u_threshold = -7.45133219101941108420e+02;
            const double invln2 = 1.44269504088896338700e+00;
            const double twom1000 = 9.33263618503218878990e-302;
            const double P1 = 1.66666666666666019037e-01;
            const double P2 = -2.77777777770155933842e-03;
            const double P3 = 6.61375632143793436117e-05;
            const double P4 = -1.65339022054652515390e-06;
            const double P5 = 4.13813679705723846039e-08;
            const double huge = 1.0e+300;

            int hx = HighWord(x); //Highword of x

            xsb = ((int)hx >> 31) & 1; //Get sign of x
            hx &= 0x7fffffff; //Get the abs(x) of the highword

            //Check if non-finite argument
            if (hx >= 0x40862E42)
            {           /* if |x|>=709.78... */
                if (hx >= 0x7ff00000)
                {
                    if (((hx & 0xfffff) | LowWord(x)) != 0) //Assume that __Lo(x) is lower word of x
                        return x;       /* NaN */
                    else
                        return (xsb == 0) ? x : 0.0;   /* exp(+-inf)={inf,0} */
                }
                if (x > o_threshold)
                    return double.PositiveInfinity; /* overflow */
                if (x < u_threshold)
                    return 0; /* underflow */
            }

            /* argument reduction */
            if (hx > 0x3fd62e42)
            {       /* if  |x| > 0.5 ln2 */
                if (hx < 0x3FF0A2B2)
                {   /* and |x| < 1.5 ln2 */
                    if (xsb == 0)
                    {
                        hi = x - 6.93147180369123816490e-01;
                        lo = 1.90821492927058770002e-10;
                    }
                    else
                    {
                        hi = x - -6.93147180369123816490e-01;
                        lo = -1.90821492927058770002e-10;
                    }
                    k = 1 - xsb - xsb;
                }
                else
                {
                    if (xsb == 0)
                        k = (int)(invln2 * x + 0.5);
                    else
                        k = (int)(invln2 * x + -0.5);
                    t = k;
                    hi = x - t * 6.93147180369123816490e-01;
                    lo = t * 1.90821492927058770002e-10;
                }
                x = hi - lo;
            }
            else if (hx < 0x3e300000)
            {   /* when |x|<2**-28 */
                if (huge + x > 1)
                    return 1 + x;/* trigger inexact */
            }
            else
                k = 0;

            /* x is now in primary range */
            t = x * x;
            c = x - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));

            if (k == 0)
                return 1 - ((x * c) / (c - 2.0) - x);
            else
                y = 1 - ((lo - (x * c) / (2.0 - c)) - hi);

            if (k >= -1021)
            {
                //The idea is to add hy to the exponent of y
                long _y = BitConverter.DoubleToInt64Bits(y);

                /* add k to y's exponent */
                if (BitConverter.IsLittleEndian)
                    _y += ((long)k << 52);
                else
                    _y += ((long)k << 20);
                y = BitConverter.Int64BitsToDouble(_y);
                return y;
            }
            else
            {
                //The idea is to add hy to the exponent of y
                long _y = BitConverter.DoubleToInt64Bits(y);

                if (BitConverter.IsLittleEndian)
                    _y += ((long)k + 1000 << 52);
                else
                    _y += ((long)k + 1000 << 20);
                y = BitConverter.Int64BitsToDouble(_y);
                return y * twom1000;
            }
        }

        #endregion Exp

        #region Floor

        public static double Floor(double a)
        {
            // should be using assembler for bigger values than int or long max
            if (a == Double.NaN || a == Double.NegativeInfinity || a == Double.PositiveInfinity)
                return a;
            int i = (a - (int)a < 0) ? (int)(a - 1) : (int)a;
            return i;
        }

        #endregion Floor

        #region Log (base e)

        public static double Log(double x)
        {
            return Log(x, E);
        }

        #endregion Log (base e)

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

        #endregion Log (base specified)

        #region Log10

        public static double Log10(double x)
        {
            return Log(x, 10F);
        }

        #endregion Log10

        #region Pow

        public static double Pow(double b, double e)
        {
            if (e == 0) return 1;
            if (e == 1) return b;
            if (double.IsNaN(b) || double.IsNaN(e)) return double.NaN;
            if (double.IsNegativeInfinity(b))
            {
                if (e < 0)
                    return 0;
                if ((long)e % 2 == 0)
                    return double.PositiveInfinity;
                else
                    return double.NegativeInfinity;
            }
            if (double.IsPositiveInfinity(b))
            {
                if (e < 0)
                    return 0;
                else
                    return double.PositiveInfinity;
            }
            if (double.IsInfinity(e))
            {
                bool t = -1 < b;
                bool t1 = 1 > b;
                if (t && t1)

                    if (double.IsPositiveInfinity(e))
                        return 0;
                    else
                        return double.PositiveInfinity;
                else
                {
                    bool v = b < -1;
                    bool v1 = 1 < b;
                    if (v || v1)
                    {
                        if (double.IsPositiveInfinity(e))
                            return double.PositiveInfinity;
                        else
                            return 0;
                    }
                    else
                        return double.NaN;
                }
            }
            if (b < 0)
            {
                if (Math.Abs(e) - Math.Abs((int)e) > (Double.Epsilon * 100)) return double.NaN;
                double logedBase = Math.Log(Math.Abs(b));
                double pow = Exp(logedBase * e);
                if ((long)e % 2 == 0) return pow;
                else return -1 * pow;
            }
            else
            {
                double logedBase = Math.Log(b);
                return Exp(logedBase * e);
            }
        }

        #endregion Pow

        #region Round

        public static double Round(double d)
        {
            return ((Math.Floor(d) % 2 == 0) ? Math.Floor(d) : Math.Ceiling(d));
        }

        #endregion Round

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

        #endregion Sin

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

        #endregion Sinh

        #region Sqrt

        /*
         * ====================================================
         * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
         *
         * Developed at SunSoft, a Sun Microsystems, Inc. business.
         * Permission to use, copy, modify, and distribute this
         * software is freely granted, provided that this notice
         * is preserved.
         * ====================================================
         */

        public static double Sqrt(double x)
        {
            double z = 0;
            const uint sign = 0x80000000;
            uint r, t1, s1, ix1, q1;
            int ix0, s0, q, m, t, i;

            ix0 = HighWord(x);          /* high word of x */
            ix1 = (uint)LowWord(x);      /* low word of x */

            /* take care of Inf and NaN */
            if ((ix0 & 0x7ff00000) == 0x7ff00000)
                return x * x + x;       /* sqrt(NaN)=NaN, sqrt(+inf)=+inf sqrt(-inf)=sNaN */
            /* take care of zero */
            if (ix0 <= 0)
            {
                if (((ix0 & (~0x80000000)) | ix1) == 0)
                    return x;/* sqrt(+-0) = +-0 */
                else if (ix0 < 0)
                    return (x - x) / (x - x);       /* sqrt(-ve) = sNaN */
            }
            /* normalize x */
            m = (ix0 >> 20);
            if (m == 0)
            {               /* subnormal x */
                while (ix0 == 0)
                {
                    m -= 21;
                    ix0 |= ((int)ix1 >> 11); ix1 <<= 21;
                }
                for (i = 0; (ix0 & 0x00100000) == 0; i++) ix0 <<= 1;
                m -= i - 1;
                ix0 |= ((int)ix1 >> (32 - i));
                ix1 <<= i;
            }
            m -= 1023;  /* unbias exponent */
            ix0 = (ix0 & 0x000fffff) | 0x00100000;
            if ((m & 1) % 2 != 0)
            {   /* odd m, double x to make it even */
                ix0 += ix0 + (int)((long)(ix1 & sign) >> 31);
                ix1 += ix1;
            }
            m >>= 1;    /* m = [m/2] */

            /* generate sqrt(x) bit by bit */
            ix0 += ix0 + (int)((long)(ix1 & sign) >> 31);
            ix1 += ix1;
            s1 = 0;
            s0 = (int)s1;
            q1 = (uint)s0;
            q = (int)q1;   /* [q,q1] = sqrt(x) */
            r = 0x00200000;     /* r = moving bit from right to left */

            while (r != 0)
            {
                t = s0 + (int)r;
                if (t <= ix0)
                {
                    s0 = t + (int)r;
                    ix0 -= t;
                    q += (int)r;
                }
                ix0 += ix0 + ((int)(ix1 & sign) >> 31);
                ix1 += ix1;
                r >>= 1;
            }

            r = sign;
            while (r != 0)
            {
                t1 = s1 + r;
                t = s0;
                if ((t < ix0) || ((t == ix0) && (t1 <= ix1)))
                {
                    s1 = t1 + r;
                    if (((t1 & sign) == sign) && (s1 & sign) == 0) s0 += 1;
                    ix0 -= t;
                    if (ix1 < t1) ix0 -= 1;
                    ix1 -= t1;
                    q1 += r;
                }
                ix0 += ix0 + ((int)(ix1 & sign) >> 31);
                ix1 += ix1;
                r >>= 1;
            }

            /* use floating add to find out rounding direction */
            if ((ix0 | ix1) != 0)
            {
                z = 1 - 1.0e-300; /* trigger inexact flag */
                if (z >= 1)
                {
                    z = 1 + 1.0e-300;
                    if (q1 == 0xffffffff)
                    {
                        q1 = 0; q += 1;
                    }
                    else if (z > 1)
                    {
                        if (q1 == 0xfffffffe)
                            q += 1;
                        q1 += 2;
                    }
                    else
                        q1 += (q1 & 1);
                }
            }
            ix0 = (q >> 1) + 0x3fe00000;
            ix1 = q1 >> 1;
            if ((q & 1) == 1) ix1 |= sign;
            ix0 += (m << 20);

            long value = BitConverter.DoubleToInt64Bits(x);
            Byte[] valueBytes = BitConverter.GetBytes(value);
            int offset = BitConverter.IsLittleEndian ? 4 : 0;
            Byte[] toAddHigher = BitConverter.GetBytes(ix0);
            Byte[] toAddLower = BitConverter.GetBytes(ix1);
            for (int I = 0; I < 4; I++)
            {
                valueBytes[I + offset] = toAddHigher[I];
                valueBytes[I] = toAddLower[I];
            }
            long _bits = BitConverter.ToInt64(valueBytes, 0);
            return BitConverter.Int64BitsToDouble(_bits);
        }

        #endregion Sqrt

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

        #endregion Tan

        #region Tanh

        public static double Tanh(double x)
        {
            return (expm1(2F * x) / (expm1(2F * x) + 2F));
        }

        #endregion Tanh

        #region Truncate

        public static double Truncate(double x)
        {
            return ((x == 0) ? 0F : ((x > 0F) ? Floor(x) : Ceiling(x)));
        }

        #endregion Truncate

        #region Internaly used functions

        #region expm1

        private static double expm1(double x)
        {
            double u = Exp(x);
            return ((u == 1.0F) ? x : ((u - 1.0F == -1.0F) ? -1.0F : ((u - 1.0F) * x / Log(u))));
        }

        #endregion expm1

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

        #endregion _power

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

        #endregion atans

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

        #endregion atanx

        #region Words

        private static int HighWord(double x)
        {
            long value = BitConverter.DoubleToInt64Bits(x);
            Byte[] valueBytes = BitConverter.GetBytes(value);
            int offset = BitConverter.IsLittleEndian ? 4 : 0;
            return BitConverter.ToInt32(valueBytes, offset);
        }

        private static int LowWord(double x) //Opposite of high word
        {
            long value = BitConverter.DoubleToInt64Bits(x);
            Byte[] valueBytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(valueBytes, BitConverter.IsLittleEndian ? 0 : 4);
        }

        #endregion Words

        #endregion Internaly used functions
    }
}
