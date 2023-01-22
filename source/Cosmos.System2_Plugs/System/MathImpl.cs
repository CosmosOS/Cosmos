using System;
using Cosmos.Debug.Kernel;
using IL2CPU.API;
using IL2CPU.API.Attribs;

namespace Cosmos.System_Plugs.System
{
    [Plug(Target = typeof(Math))]
    public static class MathImpl
    {
        /* @(#)fdlibm.h 1.5 04/04/22 */
        /*
         * ====================================================
         * Copyright (C) 2004 by Sun Microsystems, Inc. All rights reserved.
         *
         * Permission to use, copy, modify, and distribute this
         * software is freely granted, provided that this notice
         * is preserved.
         * ====================================================
         */

        //Following functions which have been implemented in this file are functions taken from http://www.netlib.org/fdlibm/ and have then be changed to work in C#
        //Acos, Asin, Cos, _cos,  __ieee754_rem_pio2, __kernel_rem_pio2, Scalbn, Log base e, Sin, _sin, exp, atan
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
            double pio2_hi = 1.57079632679489655800e+00, /* 0x3FF921FB, 0x54442D18 */
                pio2_lo = 6.12323399573676603587e-17, /* 0x3C91A626, 0x33145C07 */
                pS0 = 1.66666666666666657415e-01, /* 0x3FC55555, 0x55555555 */
                pS1 = -3.25565818622400915405e-01, /* 0xBFD4D612, 0x03EB6F7D */
                pS2 = 2.01212532134862925881e-01, /* 0x3FC9C155, 0x0E884455 */
                pS3 = -4.00555345006794114027e-02, /* 0xBFA48228, 0xB5688F3B */
                pS4 = 7.91534994289814532176e-04, /* 0x3F49EFE0, 0x7501B288 */
                pS5 = 3.47933107596021167570e-05, /* 0x3F023DE1, 0x0DFDF709 */
                qS1 = -2.40339491173441421878e+00, /* 0xC0033A27, 0x1C8A2D4B */
                qS2 = 2.02094576023350569471e+00, /* 0x40002AE5, 0x9C598AC8 */
                qS3 = -6.88283971605453293030e-01, /* 0xBFE6066C, 0x1B8D0159 */
                qS4 = 7.70381505559019352791e-02; /* 0x3FB3B8C5, 0xB12E9282 */

            double z, p, q, r, w, s, c, df;
            var hx = HighWord(x);
            var ix = hx & 0x7fffffff;
            if (ix >= 0x3ff00000)
            {   /* |x| >= 1 */
                if (((ix - 0x3ff00000) | LowWord(x)) == 0)
                {   /* |x|==1 */
                    if (hx > 0)
                    {
                        return 0.0;     /* acos(1) = 0  */
                    }
                    else
                    {
                        return Math.PI + 2.0 * pio2_lo; /* acos(-1)= pi */
                    }
                }
                return (x - x) / (x - x);       /* acos(|x|>1) is NaN */
            }
            if (ix < 0x3fe00000)
            {   /* |x| < 0.5 */
                if (ix <= 0x3c600000)
                {
                    return pio2_hi + pio2_lo;/*if|x|<2**-57*/
                }
                z = x * x;
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                r = p / q;
                return pio2_hi - (x - (pio2_lo - x * r));
            }
            else if (hx < 0)
            {       /* x < -0.5 */
                z = (1 + x) * 0.5;
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                s = Sqrt(z);
                r = p / q;
                w = r * s - pio2_lo;
                return Math.PI - 2.0 * (s + w);
            }
            else
            {           /* x > 0.5 */
                z = (1 - x) * 0.5;
                s = Sqrt(z);
                df = s;
                //__LO(df) = 0;
                byte[] bdf = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(df));
                for (int i = 0; i < 4; i++)
                {
                    bdf[i + (BitConverter.IsLittleEndian ? 4 : 0)] = 0;
                }
                df = BitConverter.ToDouble(bdf, 0);
                c = (z - df * df) / (s + df);
                p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
                q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
                r = p / q;
                w = r * s + c;
                return 2.0 * (df + w);
            }
        }

        #endregion Acos

        #region Asin

        public static double Asin(double x)
        {
            double huge = 1.000e+300,
               pio2_hi = 1.57079632679489655800e+00, /* 0x3FF921FB, 0x54442D18 */
               pio2_lo = 6.12323399573676603587e-17, /* 0x3C91A626, 0x33145C07 */
               pio4_hi = 7.85398163397448278999e-01, /* 0x3FE921FB, 0x54442D18 */
               /* coefficient for R(x^2) */
               pS0 = 1.66666666666666657415e-01, /* 0x3FC55555, 0x55555555 */
               pS1 = -3.25565818622400915405e-01, /* 0xBFD4D612, 0x03EB6F7D */
               pS2 = 2.01212532134862925881e-01, /* 0x3FC9C155, 0x0E884455 */
               pS3 = -4.00555345006794114027e-02, /* 0xBFA48228, 0xB5688F3B */
               pS4 = 7.91534994289814532176e-04, /* 0x3F49EFE0, 0x7501B288 */
               pS5 = 3.47933107596021167570e-05, /* 0x3F023DE1, 0x0DFDF709 */
               qS1 = -2.40339491173441421878e+00, /* 0xC0033A27, 0x1C8A2D4B */
               qS2 = 2.02094576023350569471e+00, /* 0x40002AE5, 0x9C598AC8 */
               qS3 = -6.88283971605453293030e-01, /* 0xBFE6066C, 0x1B8D0159 */
               qS4 = 7.70381505559019352791e-02; /* 0x3FB3B8C5, 0xB12E9282 */

            double t = 0, w, p, q, c, r;
            var hx = HighWord(x);
            var ix = hx & 0x7fffffff;
            if (ix >= 0x3ff00000)
            {       /* |x|>= 1 */
                if (((ix - 0x3ff00000) | LowWord(x)) == 0)
                {
                    /* asin(1)=+-pi/2 with inexact */
                    return x * pio2_hi + x * pio2_lo;
                }
                return (x - x) / (x - x);       /* asin(|x|>1) is NaN */
            }
            else if (ix < 0x3fe00000)
            {   /* |x|<0.5 */
                if (ix < 0x3e400000)
                {       /* if |x| < 2**-27 */
                    if (huge + x > 1)
                    {
                        return x;/* return x with inexact if x!=0*/
                    }
                }
                else
                {
                    t = x * x;
                }
                p = t * (pS0 + t * (pS1 + t * (pS2 + t * (pS3 + t * (pS4 + t * pS5)))));
                q = 1 + t * (qS1 + t * (qS2 + t * (qS3 + t * qS4)));
                w = p / q;
                return x + x * w;
            }
            /* 1> |x|>= 0.5 */
            w = 1 - Abs(x);
            t = w * 0.5;
            p = t * (pS0 + t * (pS1 + t * (pS2 + t * (pS3 + t * (pS4 + t * pS5)))));
            q = 1 + t * (qS1 + t * (qS2 + t * (qS3 + t * qS4)));
            var s = Sqrt(t);
            if (ix >= 0x3FEF3333)
            {   /* if |x| > 0.975 */
                w = p / q;
                t = pio2_hi - (2.0 * (s + s * w) - pio2_lo);
            }
            else
            {
                w = s;
                //__LO(w) = 0;
                byte[] bw = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(w));
                for (int i = 0; i < 4; i++)
                {
                    bw[i + (BitConverter.IsLittleEndian ? 4 : 0)] = 0;
                }
                w = BitConverter.ToDouble(bw, 0);
                c = (t - w * w) / (s + w);
                r = p / q;
                p = 2.0 * s * r - (pio2_lo - 2.0 * c);
                q = pio4_hi - 2.0 * w;
                t = pio4_hi - (p - q);
            }
            if (hx > 0)
            {
                return t;
            }
            else
            {
                return -t;
            }
        }

        #endregion Asin

        #region Atan

        public static double Atan(double x)
        {
            if (Double.IsNaN(x))
            {
                return Double.NaN;
            }
            if (Double.IsPositiveInfinity(x))
            {
                return Math.PI / 2;
            }
            if (Double.IsNegativeInfinity(x))
            {
                return -Math.PI / 2;
            }

            int id;

            double[] atanhi = {
              4.63647609000806093515e-01, /* atan(0.5)hi 0x3FDDAC67, 0x0561BB4F */
              7.85398163397448278999e-01, /* atan(1.0)hi 0x3FE921FB, 0x54442D18 */
              9.82793723247329054082e-01, /* atan(1.5)hi 0x3FEF730B, 0xD281F69B */
              1.57079632679489655800e+00, /* atan(inf)hi 0x3FF921FB, 0x54442D18 */
            };

            double[] atanlo = {
                2.26987774529616870924e-17, /* atan(0.5)lo 0x3C7A2B7F, 0x222F65E2 */
                3.06161699786838301793e-17, /* atan(1.0)lo 0x3C81A626, 0x33145C07 */
                1.39033110312309984516e-17, /* atan(1.5)lo 0x3C700788, 0x7AF0CBBD */
                6.12323399573676603587e-17, /* atan(inf)lo 0x3C91A626, 0x33145C07 */
            };

            double[] aT = {
              3.33333333333329318027e-01, /* 0x3FD55555, 0x5555550D */
             -1.99999999998764832476e-01, /* 0xBFC99999, 0x9998EBC4 */
              1.42857142725034663711e-01, /* 0x3FC24924, 0x920083FF */
             -1.11111104054623557880e-01, /* 0xBFBC71C6, 0xFE231671 */
              9.09088713343650656196e-02, /* 0x3FB745CD, 0xC54C206E */
             -7.69187620504482999495e-02, /* 0xBFB3B0F2, 0xAF749A6D */
              6.66107313738753120669e-02, /* 0x3FB10D66, 0xA0D03D51 */
             -5.83357013379057348645e-02, /* 0xBFADDE2D, 0x52DEFD9A */
              4.97687799461593236017e-02, /* 0x3FA97B4B, 0x24760DEB */
             -3.65315727442169155270e-02, /* 0xBFA2B444, 0x2C6A6C2F */
              1.62858201153657823623e-02, /* 0x3F90AD3A, 0xE322DA11 */
            };

            var hx = HighWord(x);
            var ix = hx & 0x7fffffff;
            if (ix >= 0x44100000)
            {   /* if |x| >= 2^66 */
                if (ix > 0x7ff00000 ||
                (ix == 0x7ff00000 && LowWord(x) != 0))
                {
                    return x + x;       /* NaN */
                }
                if (hx > 0)
                {
                    return atanhi[3] + atanlo[3];
                }
                else
                {
                    return -atanhi[3] - atanlo[3];
                }
            }
            if (ix < 0x3fdc0000)
            {   /* |x| < 0.4375 */
                if (ix < 0x3e200000)
                {   /* |x| < 2^-29 */
                    if (1.0e+300 + x > 1)
                    {
                        return x; /* raise inexact */
                    }
                }
                id = -1;
            }
            else
            {
                x = Math.Abs(x);
                if (ix < 0x3ff30000)
                {       /* |x| < 1.1875 */
                    if (ix < 0x3fe60000)
                    {   /* 7/16 <=|x|<11/16 */
                        id = 0; x = (2.0 * x - 1) / (2.0 + x);
                    }
                    else
                    {           /* 11/16<=|x|< 19/16 */
                        id = 1; x = (x - 1) / (x + 1);
                    }
                }
                else
                {
                    if (ix < 0x40038000)
                    {   /* |x| < 2.4375 */
                        id = 2; x = (x - 1.5) / (1 + 1.5 * x);
                    }
                    else
                    {           /* 2.4375 <= |x| < 2^66 */
                        id = 3; x = -1.0 / x;
                    }
                }
            }

            /* end of argument reduction */
            var z = x * x;
            var w = z * z;
            /* break sum from i=0 to 10 aT[i]z**(i+1) into odd and even poly */
            var s1 = z * (aT[0] + w * (aT[2] + w * (aT[4] + w * (aT[6] + w * (aT[8] + w * aT[10])))));
            var s2 = w * (aT[1] + w * (aT[3] + w * (aT[5] + w * (aT[7] + w * aT[9]))));
            if (id < 0)
            {
                return x - x * (s1 + s2);
            }
            else
            {
                z = atanhi[id] - (x * (s1 + s2) - atanlo[id] - x);
                return hx < 0 ? -z : z;
            }
        }

        #endregion Atan

        #region Atan2

        public static double Atan2(double x, double y)
        {
            if (x + y == x)
            {
                if ((x == 0F) & (y == 0F))
                {
                    return 0F;
                }
                if (x >= 0.0F)
                {
                    return pio2;
                }
                return -pio2;
            }
            if (y < 0.0F)
            {
                if (x >= 0.0F)
                {
                    return pio2 * 2 - atans(-x / y);
                }
                return -pio2 * 2 + atans(x / y);
            }
            if (x > 0.0F)
            {
                return atans(x / y);
            }
            return -atans(-x / y);

            //return (((x + y) == x) ? (((x == 0F) & (y == 0F)) ? 0F : ((x >= 0F) ? pio2 : (-pio2))) : ((y < 0F) ? ((x >= 0F) ? ((pio2 * 2) - atans((-x) / y)) : (((-pio2) * 2) + atans(x / y))) : ((x > 0F) ? atans(x / y) : -atans((-x) / y))));
        }

        #endregion Atan2

        #region Ceiling

        public static double Ceiling(double x)
        {
            if (Double.IsNaN(x) || Double.IsInfinity(x))
            {
                return x;
            }

            if(x > 0)
            {
                double val = (long)x;
                if(val == x)
                {
                    return x;
                }
                else
                {
                    return val + 1;
                }
            }
            else
            {
                return -1 * (double)(long)(-1 * x);
            }
        }

        #endregion Ceiling

        #region Cosh

        public static double Cosh(double x)
        {
            if (x < 0.0F)
            {
                x = -x;
            }
            return x == 0F ? 1F : x <= ln2 / 2 ? 1 + _power(Exp(x) - 1, 2) / (2 * Exp(x)) : x <= 22F ? (Exp(x) + 1 / Exp(x)) / 2 : 0.5F * (Exp(x) + Exp(-x));
        }

        #endregion Cosh

        #region Exp

        public static double Exp(double x)
        {
            double y, hi = 0, lo = 0, t;
            var k = 0;

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

            var xsb = ((int)hx >> 31) & 1; //Get sign of x
            hx &= 0x7fffffff; //Get the abs(x) of the highword

            //Check if non-finite argument
            if (hx >= 0x40862E42)
            {           /* if |x|>=709.78... */
                if (hx >= 0x7ff00000)
                {
                    if (((hx & 0xfffff) | LowWord(x)) != 0) //Assume that __Lo(x) is lower word of x
                    {
                        return x;       /* NaN */
                    }
                    else
                    {
                        return xsb == 0 ? x : 0.0;   /* exp(+-inf)={inf,0} */
                    }
                }
                if (x > o_threshold)
                {
                    return Double.PositiveInfinity; /* overflow */
                }

                if (x < u_threshold)
                {
                    return 0; /* underflow */
                }
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
                    {
                        k = (int)(invln2 * x + 0.5);
                    }
                    else
                    {
                        k = (int)(invln2 * x + -0.5);
                    }
                    t = k;
                    hi = x - t * 6.93147180369123816490e-01;
                    lo = t * 1.90821492927058770002e-10;
                }
                x = hi - lo;
            }
            else if (hx < 0x3e300000)
            {   /* when |x|<2**-28 */
                if (huge + x > 1)
                {
                    return 1 + x;/* trigger inexact */
                }
            }
            else
            {
                k = 0;
            }

            /* x is now in primary range */
            t = x * x;
            var c = x - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));

            if (k == 0)
            {
                return 1 - (x * c / (c - 2.0) - x);
            }
            else
            {
                y = 1 - (lo - x * c / (2.0 - c) - hi);
            }

            if (k >= -1021)
            {
                //The idea is to add hy to the exponent of y
                long _y = BitConverter.DoubleToInt64Bits(y);

                /* add k to y's exponent */
                if (BitConverter.IsLittleEndian)
                {
                    _y += (long)k << 52;
                }
                else
                {
                    _y += (long)k << 20;
                }

                y = BitConverter.Int64BitsToDouble(_y);
                return y;
            }
            else
            {
                //The idea is to add hy to the exponent of y
                long _y = BitConverter.DoubleToInt64Bits(y);

                if (BitConverter.IsLittleEndian)
                {
                    _y += (long)k + 1000 << 52;
                }
                else
                {
                    _y += (long)k + 1000 << 20;
                }
                y = BitConverter.Int64BitsToDouble(_y);
                return y * twom1000;
            }
        }

        #endregion Exp

        #region Floor

        public static double Floor(double x)
        {
            if (Double.IsInfinity(x) || Double.IsNaN(x))
            {
                return x;
            }

            if (x > 0)
            {
                return (long)x;
            }
            else
            {
                double val = -1 * (double)(long)(-1 * x);
                if(val == x)
                {
                    return x;
                }
                else
                {
                    return val - 1;
                }
            }
        }

        #endregion Floor

        #region Log (base e)

        public static double Log(double x)
        {
            double ln2_hi = 6.93147180369123816490e-01,    /* 3fe62e42 fee00000 */
            ln2_lo = 1.90821492927058770002e-10,    /* 3dea39ef 35793c76 */
            two54 = 1.80143985094819840000e+16,  /* 43500000 00000000 */
            Lg1 = 6.666666666666735130e-01,  /* 3FE55555 55555593 */
            Lg2 = 3.999999999940941908e-01,  /* 3FD99999 9997FA04 */
            Lg3 = 2.857142874366239149e-01,  /* 3FD24924 94229359 */
            Lg4 = 2.222219843214978396e-01,  /* 3FCC71C5 1D8E78AF */
            Lg5 = 1.818357216161805012e-01,  /* 3FC74664 96CB03DE */
            Lg6 = 1.531383769920937332e-01,  /* 3FC39A09 D078C69F */
            Lg7 = 1.479819860511658591e-01;  /* 3FC2F112 DF3E5244 */
            double hfsq, R, dk;

            var hx = HighWord(x) /* high word of x */;
            var lx = (uint)LowWord(x) /* low  word of x */;

            var k = 0;
            if (hx < 0x00100000)
            {           /* x < 2**-1022  */
                if (x < 0 || Double.IsNaN(x))
                {
                    return Double.NaN;  /* log(-#) = NaN */
                }

                if (((hx & (uint)0x7fff) | lx) == 0)
                {
                    return Double.NegativeInfinity;       /* log(+-0)=-inf */
                }

                k -= 54; x *= two54; /* subnormal number, scale up x */
                hx = HighWord(x);       /* high word of x */
            }
            if (hx >= 0x7ff00000)
            {
                return x + x;
            }

            k += (hx >> 20) - 1023;
            hx &= 0x000fffff;
            var i = (hx + 0x95f64) & 0x100000;
            //__HI(x) = hx | (i ^ 0x3ff00000);    /* normalize x or x/2 */
            var bx = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(x));
            var bv = BitConverter.GetBytes(hx | (i ^ 0x3ff00000));
            for (int _i = 0; _i < 4; _i++)
            {
                bx[_i + (BitConverter.IsLittleEndian ? 4 : 0)] = bv[_i];
            }
            x = BitConverter.ToDouble(bx, 0);
            k += i >> 20;
            var f = x - 1.0;
            if ((0x000fffff & (2 + hx)) < 3)
            {   /* |f| < 2**-20 */
                if (f == 0)
                {
                    if (k == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        dk = k;
                        return dk * ln2_hi + dk * ln2_lo;
                    }
                }
                R = f * f * (0.5 - 0.33333333333333333 * f);
                if (k == 0)
                {
                    return f - R;
                }
                else
                {
                    dk = k;
                    return dk * ln2_hi - (R - dk * ln2_lo - f);
                }
            }
            var s = f / (2.0 + f);
            dk = k;
            var z = s * s;
            i = hx - 0x6147a;
            var w = z * z;
            var j = 0x6b851 - hx;
            var t1 = w * (Lg2 + w * (Lg4 + w * Lg6));
            var t2 = z * (Lg1 + w * (Lg3 + w * (Lg5 + w * Lg7)));
            i |= j;
            R = t2 + t1;
            if (i > 0)
            {
                hfsq = 0.5 * f * f;
                if (k == 0)
                {
                    return f - (hfsq - s * (hfsq + R));
                }
                else
                {
                    return dk * ln2_hi - (hfsq - (s * (hfsq + R) + dk * ln2_lo) - f);
                }
            }
            else
            {
                if (k == 0)
                {
                    return f - s * (f - R);
                }
                else
                {
                    return dk * ln2_hi - (s * (f - R) - dk * ln2_lo - f);
                }
            }
        }

        #endregion Log (base e)

        #region Log (base specified)

        public static double Log(double Exponent, double Base)
        {
            if (Double.IsNaN(Exponent) || Exponent < 0)
            {
                return Double.NaN;
            }

            if (Exponent == 0)
            {
                return Double.NegativeInfinity;
            }
            return Log(Exponent) / Log(Base);
        }

        #endregion Log (base specified)

        #region Log10

        public static double Log10(double x)
        {
            //Use change of base formula
            return Log(x, 10F);
        }

        #endregion Log10

        #region Pow

        public static double Pow(double b, double e)
        {
            if (e == 0)
            {
                return 1;
            }

            if (e == 1)
            {
                return b;
            }

            if (Double.IsNaN(b) || Double.IsNaN(e))
            {
                return Double.NaN;
            }
            if (Double.IsNegativeInfinity(b))
            {
                if (e < 0)
                {
                    return 0;
                }
                if ((long)e % 2 == 0)
                {
                    return Double.PositiveInfinity;
                }
                else
                {
                    return Double.NegativeInfinity;
                }
            }
            if (Double.IsPositiveInfinity(b))
            {
                if (e < 0)
                {
                    return 0;
                }
                else
                {
                    return Double.PositiveInfinity;
                }
            }
            if (Double.IsInfinity(e))
            {
                bool t = -1 < b;
                bool t1 = 1 > b;
                if (t && t1)
                {
                    if (Double.IsPositiveInfinity(e))
                    {
                        return 0;
                    }
                    else
                    {
                        return Double.PositiveInfinity;
                    }
                }
                else
                {
                    bool v = b < -1;
                    bool v1 = 1 < b;
                    if (v || v1)
                    {
                        if (Double.IsPositiveInfinity(e))
                        {
                            return Double.PositiveInfinity;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        return Double.NaN;
                    }
                }
            }
            if (b < 0)
            {
                if (Abs(e) - Abs((int)e) > Double.Epsilon * 100)
                {
                    return Double.NaN;
                }
                double logedBase = Log(Abs(b));
                double pow = Exp(logedBase * e);
                if ((long)e % 2 == 0)
                {
                    return pow;
                }
                else
                {
                    return -1 * pow;
                }
            }
            else
            {
                if (e < 0)
                {
                    e = Abs(e);
                    double logedBase = Log(b);
                    return 1 / Exp(logedBase * e);
                }
                else
                {
                    double logedBase = Log(b);
                    return Exp(logedBase * e);
                }
            }
        }

        #endregion Pow

        #region Sinh

        public static double Sinh(double x)
        {
            if (x < 0F)
            {
                x = -x;
            }

            if (x <= 22F)
            {
                double Ex_1 = Tanh(x / 2) * (Exp(x) + 1);
                return (Ex_1 + Ex_1 / (Ex_1 - 1)) / 2;
            }
            else
            {
                return Exp(x) / 2;
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
            uint t1;
            int t, i;

            var ix0 = HighWord(x) /* high word of x */;
            var ix1 = (uint)LowWord(x) /* low word of x */;

            /* take care of Inf and NaN */
            if ((ix0 & 0x7ff00000) == 0x7ff00000)
            {
                return x * x + x;       /* sqrt(NaN)=NaN, sqrt(+inf)=+inf sqrt(-inf)=sNaN */
            }
            /* take care of zero */
            if (ix0 <= 0)
            {
                if (((ix0 & ~0x80000000) | ix1) == 0)
                {
                    return x;/* sqrt(+-0) = +-0 */
                }
                else if (ix0 < 0)
                {
                    return (x - x) / (x - x);       /* sqrt(-ve) = sNaN */
                }
            }

            /* normalize x */
            var m = ix0 >> 20;
            if (m == 0)
            {               /* subnormal x */
                while (ix0 == 0)
                {
                    m -= 21;
                    ix0 |= (int)ix1 >> 11; ix1 <<= 21;
                }
                for (i = 0; (ix0 & 0x00100000) == 0; i++)
                {
                    ix0 <<= 1;
                }

                m -= i - 1;
                ix0 |= (int)ix1 >> (32 - i);
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
            uint s1 = 0;
            var s0 = (int)s1;
            var q1 = (uint)s0;
            var q = (int)q1 /* [q,q1] = sqrt(x) */;
            uint r = 0x00200000 /* r = moving bit from right to left */;

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
                if (t < ix0 || (t == ix0 && t1 <= ix1))
                {
                    s1 = t1 + r;
                    if ((t1 & sign) == sign && (s1 & sign) == 0)
                    {
                        s0 += 1;
                    }

                    ix0 -= t;
                    if (ix1 < t1)
                    {
                        ix0 -= 1;
                    }

                    ix1 -= t1;
                    q1 += r;
                }
                ix0 += ix0 + ((int)(ix1 & sign) >> 31);
                ix1 += ix1;
                r >>= 1;
            }

            /* use floating add to find out rounding direction */
            if (((uint)ix0 | ix1) != 0)
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
                        {
                            q += 1;
                        }

                        q1 += 2;
                    }
                    else
                    {
                        q1 += q1 & 1;
                    }
                }
            }
            ix0 = (q >> 1) + 0x3fe00000;
            ix1 = q1 >> 1;
            if ((q & 1) == 1)
            {
                ix1 |= sign;
            }
            ix0 += m << 20;

            long value = BitConverter.DoubleToInt64Bits(x);
            var valueBytes = BitConverter.GetBytes(value);
            int offset = BitConverter.IsLittleEndian ? 4 : 0;
            var toAddHigher = BitConverter.GetBytes(ix0);
            var toAddLower = BitConverter.GetBytes(ix1);
            for (int I = 0; I < 4; I++)
            {
                valueBytes[I + offset] = toAddHigher[I];
                valueBytes[I] = toAddLower[I];
            }
            return BitConverter.ToDouble(valueBytes, 0);
        }

        #endregion Sqrt

        #region Tanh

        public static double Tanh(double x)
        {
            return expm1(2F * x) / (expm1(2F * x) + 2F);
        }

        #endregion Tanh

        #region Truncate

        public static double Truncate(double x)
        {
            return x == 0 ? 0F : x > 0F ? Floor(x) : Ceiling(x);
        }

        #endregion Truncate

        #region Internaly used functions

        private static int __ieee754_rem_pio2(double x, double[] y)
        {
            int highOffset = BitConverter.IsLittleEndian ? 4 : 0;
            int lowOffset = BitConverter.IsLittleEndian ? 4 : 0;

            int[] two_over_pi = {
            0xA2F983, 0x6E4E44, 0x1529FC, 0x2757D1, 0xF534DD, 0xC0DB62,
            0x95993C, 0x439041, 0xFE5163, 0xABDEBB, 0xC561B7, 0x246E3A,
            0x424DD2, 0xE00649, 0x2EEA09, 0xD1921C, 0xFE1DEB, 0x1CB129,
            0xA73EE8, 0x8235F5, 0x2EBB44, 0x84E99C, 0x7026B4, 0x5F7E41,
            0x3991D6, 0x398353, 0x39F49C, 0x845F8B, 0xBDF928, 0x3B1FF8,
            0x97FFDE, 0x05980F, 0xEF2F11, 0x8B5A0A, 0x6D1F6D, 0x367ECF,
            0x27CB09, 0xB74F46, 0x3F669E, 0x5FEA2D, 0x7527BA, 0xC7EBE5,
            0xF17B3D, 0x0739F7, 0x8A5292, 0xEA6BFB, 0x5FB11F, 0x8D5D08,
            0x560330, 0x46FC7B, 0x6BABF0, 0xCFBC20, 0x9AF436, 0x1DA9E3,
            0x91615E, 0xE61B08, 0x659985, 0x5F14A0, 0x68408D, 0xFFD880,
            0x4D7327, 0x310606, 0x1556CA, 0x73A8C9, 0x60E27B, 0xC08C6B,
            };

            int[] npio2_hw = new int[]{
            0x3FF921FB, 0x400921FB, 0x4012D97C, 0x401921FB, 0x401F6A7A, 0x4022D97C,
            0x4025FDBB, 0x402921FB, 0x402C463A, 0x402F6A7A, 0x4031475C, 0x4032D97C,
            0x40346B9C, 0x4035FDBB, 0x40378FDB, 0x403921FB, 0x403AB41B, 0x403C463A,
            0x403DD85A, 0x403F6A7A, 0x40407E4C, 0x4041475C, 0x4042106C, 0x4042D97C,
            0x4043A28C, 0x40446B9C, 0x404534AC, 0x4045FDBB, 0x4046C6CB, 0x40478FDB,
            0x404858EB, 0x404921FB,
            };

            const double two24 = 1.67772160000000000000e+07, /* 0x41700000, 0x00000000 */
                invpio2 = 6.36619772367581382433e-01, /* 0x3FE45F30, 0x6DC9C883 */
                pio2_1 = 1.57079632673412561417e+00, /* 0x3FF921FB, 0x54400000 */
                pio2_1t = 6.07710050650619224932e-11, /* 0x3DD0B461, 0x1A626331 */
                pio2_2 = 6.07710050630396597660e-11, /* 0x3DD0B461, 0x1A600000 */
                pio2_2t = 2.02226624879595063154e-21, /* 0x3BA3198A, 0x2E037073 */
                pio2_3 = 2.02226624871116645580e-21, /* 0x3BA3198A, 0x2E000000 */
                pio2_3t = 8.47842766036889956997e-32; /* 0x397B839A, 0x252049C1 */

            double z = 0, w, t, r, fn;
            double[] tx = new double[3];
            int i, j, n;

            var hx = HighWord(x) /* high word of x */;
            var ix = hx & 0x7fffffff;
            if (ix <= 0x3fe921fb)   /* |x| ~<= pi/4 , no need for reduction */
            { y[0] = x; y[1] = 0; return 0; }
            if (ix < 0x4002d97c)
            {  /* |x| < 3pi/4, special case with n=+-1 */
                if (hx > 0)
                {
                    z = x - pio2_1;
                    if (ix != 0x3ff921fb)
                    {   /* 33+53 bit pi is good enough */
                        y[0] = z - pio2_1t;
                        y[1] = z - y[0] - pio2_1t;
                    }
                    else
                    {       /* near pi/2, use 33+33+53 bit pi */
                        z -= pio2_2;
                        y[0] = z - pio2_2t;
                        y[1] = z - y[0] - pio2_2t;
                    }
                    return 1;
                }
                else
                {   /* negative x */
                    z = x + pio2_1;
                    if (ix != 0x3ff921fb)
                    {   /* 33+53 bit pi is good enough */
                        y[0] = z + pio2_1t;
                        y[1] = z - y[0] + pio2_1t;
                    }
                    else
                    {       /* near pi/2, use 33+33+53 bit pi */
                        z += pio2_2;
                        y[0] = z + pio2_2t;
                        y[1] = z - y[0] + pio2_2t;
                    }
                    return -1;
                }
            }
            if (ix <= 0x413921fb)
            { /* |x| ~<= 2^19*(pi/2), medium size */
                t = Abs(x);
                n = (int)(t * invpio2 + 0.5);
                fn = n;
                r = t - fn * pio2_1;
                w = fn * pio2_1t;   /* 1st round good to 85 bit */
                if (n < 32 && ix != npio2_hw[n - 1])
                {
                    y[0] = r - w;   /* quick check no cancellation */
                }
                else
                {
                    j = ix >> 20;
                    y[0] = r - w;
                    i = j - ((HighWord(y[0]) >> 20) & 0x7ff);
                    if (i > 16)
                    {  /* 2nd iteration needed, good to 118 */
                        t = r;
                        w = fn * pio2_2;
                        r = t - w;
                        w = fn * pio2_2t - (t - r - w);
                        y[0] = r - w;
                        i = j - ((HighWord(y[0]) >> 20) & 0x7ff);
                        if (i > 49)
                        {   /* 3rd iteration need, 151 bits acc */
                            t = r;  /* will cover all possible cases */
                            w = fn * pio2_3;
                            r = t - w;
                            w = fn * pio2_3t - (t - r - w);
                            y[0] = r - w;
                        }
                    }
                }
                y[1] = r - y[0] - w;
                if (hx < 0)
                {
                    y[0] = -y[0];
                    y[1] = -y[1];
                    return -n;
                }
                else
                {
                    return n;
                }
            }
            /*
             * all other (large) arguments
             */
            if (ix >= 0x7ff00000)
            {       /* x is inf or NaN */
                y[0] = y[1] = x - x;
                return 0;
            }

            /* set z = scalbn(|x|,ilogb(x)-23) */
            //__LO(z) = __LO(x);
            //e0 = (ix >> 20) - 1046; /* e0 = ilogb(z)-23; */
            //__HI(z) = ix - (e0 << 20);
            var e0 = (ix >> 20) - 1046 /* e0 = ilogb(z)-23; */;

            long lz = BitConverter.DoubleToInt64Bits(z);
            long lx = BitConverter.DoubleToInt64Bits(x);
            long lv = BitConverter.DoubleToInt64Bits(ix - (e0 << 20));
            var bz = BitConverter.GetBytes(lz);
            var bx = BitConverter.GetBytes(lx);
            var bv = BitConverter.GetBytes(lv);
            for (int l = 0; l < 4; l++)
            {
                bz[l + lowOffset] = bx[l + lowOffset];
                bz[l + highOffset] = bv[l + highOffset];
            }

            for (i = 0; i < 2; i++)
            {
                tx[i] = (int)z;
                z = (z - tx[i]) * two24;
            }
            tx[2] = z;
            var nx = 3;
            while (tx[nx - 1] == 0)
            {
                nx--;    /* skip zero term */
            }

            n = __kernel_rem_pio2(tx, y, e0, nx, 2, two_over_pi);
            if (hx < 0)
            {
                y[0] = -y[0];
                y[1] = -y[1];
                return -n;
            }
            return n;
        }

        private static int __kernel_rem_pio2(double[] x, double[] y, int e0, int nx, int prec, int[] ipio2)
        {
            int[] init_jk = { 2, 3, 4, 6 }; /* initial value for jk */
            double[] PIo2 = {
              1.57079625129699707031e+00, /* 0x3FF921FB, 0x40000000 */
              7.54978941586159635335e-08, /* 0x3E74442D, 0x00000000 */
              5.39030252995776476554e-15, /* 0x3CF84698, 0x80000000 */
              3.28200341580791294123e-22, /* 0x3B78CC51, 0x60000000 */
              1.27065575308067607349e-29, /* 0x39F01B83, 0x80000000 */
              1.22933308981111328932e-36, /* 0x387A2520, 0x40000000 */
              2.73370053816464559624e-44, /* 0x36E38222, 0x80000000 */
              2.16741683877804819444e-51, /* 0x3569F31D, 0x00000000 */
            };

            double two24 = 1.67772160000000000000e+07, /* 0x41700000, 0x00000000 */
                twon24 = 5.96046447753906250000e-08; /* 0x3E700000, 0x00000000 */

            int carry, i, k;
            int[] iq = new int[20];
            double z, fw;
            double[] f = new double[20];
            double[] fq = new double[20];
            double[] q = new double[20];

            /* initialize jk*/
            var jk = init_jk[prec];
            var jp = jk;

            /* determine jx,jv,q0, note that 3>q0 */
            var jx = nx - 1;
            var jv = (e0 - 3) / 24; if (jv < 0)
            {
                jv = 0;
            }

            var q0 = e0 - 24 * (jv + 1);

            /* set up f[0] to f[jx+jk] where f[jx+jk] = ipio2[jv+jk] */
            var j = jv - jx; var m = jx + jk;
            for (i = 0; i <= m; i++, j++)
            {
                f[i] = j < 0 ? 0 : ipio2[j];
            }

            /* compute q[0],q[1],...q[jk] */
            for (i = 0; i <= jk; i++)
            {
                for (j = 0, fw = 0.0; j <= jx; j++)
                {
                    fw += x[j] * f[jx + i - j];
                }

                q[i] = fw;
            }

            var jz = jk;
        recompute:
            /* distill q[] into iq[] reversingly */
            for (i = 0, j = jz, z = q[jz]; j > 0; i++, j--)
            {
                fw = (int)(twon24 * z);
                iq[i] = (int)(z - two24 * fw);
                z = q[j - 1] + fw;
            }

            /* compute n */
            z = Scalbn(z, q0);     /* actual value of z */
            z -= 8.0 * Floor(z * 0.125);       /* trim off integer >= 8 */
            var n = (int)z;
            z -= n;
            var ih = 0;
            if (q0 > 0)
            {   /* need iq[jz-1] to determine n */
                i = iq[jz - 1] >> (24 - q0); n += i;
                iq[jz - 1] -= i << (24 - q0);
                ih = iq[jz - 1] >> (23 - q0);
            }
            else if (q0 == 0)
            {
                ih = iq[jz - 1] >> 23;
            }
            else if (z >= 0.5)
            {
                ih = 2;
            }

            if (ih > 0)
            {   /* q > 0.5 */
                n += 1; carry = 0;
                for (i = 0; i < jz; i++)
                {   /* compute 1-q */
                    j = iq[i];
                    if (carry == 0)
                    {
                        if (j != 0)
                        {
                            carry = 1; iq[i] = 0x1000000 - j;
                        }
                    }
                    else
                    {
                        iq[i] = 0xffffff - j;
                    }
                }
                if (q0 > 0)
                {       /* rare case: chance is 1 in 12 */
                    switch (q0)
                    {
                        case 1:
                            iq[jz - 1] &= 0x7fffff; break;
                        case 2:
                            iq[jz - 1] &= 0x3fffff; break;
                    }
                }
                if (ih == 2)
                {
                    z = 1 - z;
                    if (carry != 0)
                    {
                        z -= Scalbn(1, q0);
                    }
                }
            }

            /* check if recomputation is needed */
            if (z == 0)
            {
                j = 0;
                for (i = jz - 1; i >= jk; i--)
                {
                    j |= iq[i];
                }

                if (j == 0)
                { /* need recomputation */
                    for (k = 1; iq[jk - k] == 0; k++)
                    {
                        ;   /* k = no. of terms needed */
                    }

                    for (i = jz + 1; i <= jz + k; i++)
                    {   /* add q[jz+1] to q[jz+k] */
                        f[jx + i] = ipio2[jv + i];
                        for (j = 0, fw = 0.0; j <= jx; j++)
                        {
                            fw += x[j] * f[jx + i - j];
                        }
                        q[i] = fw;
                    }
                    jz += k;
                    goto recompute;
                }
            }

            /* chop off zero terms */
            if (z == 0.0)
            {
                jz -= 1; q0 -= 24;
                while (iq[jz] == 0) { jz--; q0 -= 24; }
            }
            else
            { /* break z into 24-bit if necessary */
                z = Scalbn(z, -q0);
                if (z >= two24)
                {
                    fw = (int)(twon24 * z);
                    iq[jz] = (int)(z - two24 * fw);
                    jz += 1; q0 += 24;
                    iq[jz] = (int)fw;
                }
                else
                {
                    iq[jz] = (int)z;
                }
            }

            /* convert integer "bit" chunk to floating-point value */
            fw = Scalbn(1, q0);
            for (i = jz; i >= 0; i--)
            {
                q[i] = fw * iq[i];
                fw *= twon24;
            }

            /* compute PIo2[0,...,jp]*q[jz,...,0] */
            for (i = jz; i >= 0; i--)
            {
                for (fw = 0.0, k = 0; k <= jp && k <= jz - i; k++)
                {
                    fw += PIo2[k] * q[i + k];
                }
                fq[jz - i] = fw;
            }

            /* compress fq[] into y[] */
            switch (prec)
            {
                case 0:
                    fw = 0.0;
                    for (i = jz; i >= 0; i--)
                    {
                        fw += fq[i];
                    }
                    y[0] = ih == 0 ? fw : -fw;
                    break;

                case 1:
                case 2:
                    fw = 0.0;
                    for (i = jz; i >= 0; i--)
                    {
                        fw += fq[i];
                    }
                    y[0] = ih == 0 ? fw : -fw;
                    fw = fq[0] - fw;
                    for (i = 1; i <= jz; i++)
                    {
                        fw += fq[i];
                    }
                    y[1] = ih == 0 ? fw : -fw;
                    break;

                case 3: /* painful */
                    for (i = jz; i > 0; i--)
                    {
                        fw = fq[i - 1] + fq[i];
                        fq[i] += fq[i - 1] - fw;
                        fq[i - 1] = fw;
                    }
                    for (i = jz; i > 1; i--)
                    {
                        fw = fq[i - 1] + fq[i];
                        fq[i] += fq[i - 1] - fw;
                        fq[i - 1] = fw;
                    }
                    for (fw = 0.0, i = jz; i >= 2; i--)
                    {
                        fw += fq[i];
                    }
                    if (ih == 0)
                    {
                        y[0] = fq[0]; y[1] = fq[1];
                        y[2] = fw;
                    }
                    else
                    {
                        y[0] = -fq[0]; y[1] = -fq[1];
                        y[2] = -fw;
                    }
                    break;
            }
            return n & 7;
        }

        private static double Scalbn(double x, int n)
        {
            double two54 = 1.80143985094819840000e+16, /* 0x43500000, 0x00000000 */
                twom54 = 5.55111512312578270212e-17, /* 0x3C900000, 0x00000000 */
                huge = 1.0e+300,
                tiny = 1.0e-300;
            var hx = HighWord(x);
            var lx = LowWord(x);
            var k = (hx & 0x7ff00000) >> 20 /* extract exponent */;
            if (k == 0)
            {               /* 0 or subnormal x */
                if ((lx | (hx & 0x7fffffff)) == 0)
                {
                    return x; /* +-0 */
                }
                x *= two54;
                hx = HighWord(x);
                k = ((hx & 0x7ff00000) >> 20) - 54;
                if (n < -50000)
                {
                    return tiny * x;    /*underflow*/
                }
            }
            if (k == 0x7ff)
            {
                return x + x;       /* NaN or Inf */
            }
            k = k + n;
            if (k > 0x7fe)
            {
                return huge * (((long)x >> 31) & 1) * huge;// copysign(huge, x); /* overflow  */
            }
            if (k > 0)              /* normal result */
            {
                //__HI(x) = (hx & 0x800fffff) | (k << 20);
                var _bx = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(x));
                var _bv = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits((hx & 0x800) | (k << 20)));
                for (int i = 0; i < 4; i++)
                {
                    _bx[i + (BitConverter.IsLittleEndian ? 4 : 0)] = _bv[i];
                }
                return x;
            }
            if (k <= -54)
            {
                if (n > 50000)  /* in case integer overflow in n+k */
                {
                    return huge * (((long)x >> 31) & 1) * huge; //copysign(huge, x);    /*overflow*/
                }
                else
                {
                    return tiny * (((long)x >> 31) & 1) * tiny;//  copysign(tiny, x);    /*underflow*/
                }
            }
            k += 54;                /* subnormal result */
            //__HI(x) = (hx & 0x800) | (k << 20);
            var bx = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits(x));
            var bv = BitConverter.GetBytes(BitConverter.DoubleToInt64Bits((hx & 0x800) | (k << 20)));
            for (int i = 0; i < 4; i++)
            {
                bx[i + (BitConverter.IsLittleEndian ? 4 : 0)] = bv[i];
            }
            return x * twom54;
        }

        #region expm1

        private static double expm1(double x)
        {
            double u = Exp(x);
            return u == 1.0F ? x : u - 1.0F == -1.0F ? -1.0F : (u - 1.0F) * x / Log(u);
        }

        #endregion expm1

        #region _power

        private static double _power(double x, int c)
        {
            if (c == 0)
            {
                return 1.0F;
            }

            int _c;
            double ret = x;

            if (c >= 0f)
            {
                for (_c = 1; _c < c; _c++)
                {
                    ret *= ret;
                }
            }
            else
            {
                for (_c = 1; _c < c; _c++)
                {
                    ret /= ret;
                }
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
                return pio2 - atanx(1.0F / x);
            }
            else
            {
                return pio4 + atanx((x - 1.0F) / (x + 1.0F));
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
            if (x > -.01 && x < .01)
            {
                value = atan_p0 / atan_q0;
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
            var valueBytes = BitConverter.GetBytes(value);
            int offset = BitConverter.IsLittleEndian ? 4 : 0;
            return BitConverter.ToInt32(valueBytes, offset);
        }

        private static int LowWord(double x) //Opposite of high word
        {
            long value = BitConverter.DoubleToInt64Bits(x);
            var valueBytes = BitConverter.GetBytes(value);
            return BitConverter.ToInt32(valueBytes, BitConverter.IsLittleEndian ? 0 : 4);
        }

        #endregion Words

        #endregion Internaly used functions
    }
}
