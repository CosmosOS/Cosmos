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

// Transcendental math functions.
// Core functions (sin, cos, tan, exp, log, atan): ARM64 uses C# fdlibm,
//   x64 uses x87 FPU assembly in Cosmos.Kernel.Native.X64/Runtime/Runtime.s.
// Derived functions (asin, acos, atan2, pow, log2, log10): shared C# on both arches,
//   ported from fdlibm via Cosmos Gen2 (Cosmos/source/Cosmos.System2_Plugs/System/MathImpl.cs).

using System.Runtime;
using System.Runtime.CompilerServices;

namespace Cosmos.Kernel.Core.Runtime;

internal static class Math
{
    // =========================================================================
    // Shared helpers
    // =========================================================================

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe long DoubleToBits(double d)
    {
        return *(long*)&d;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe double BitsToDouble(long bits)
    {
        return *(double*)&bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Abs(double x)
    {
        return x < 0 ? -x : x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double RoundToNearest(double x)
    {
        return (double)(long)(x + (x >= 0 ? 0.5 : -0.5));
    }

    // IEEE 754 bit manipulation helpers — used by fdlibm functions on both arches
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int HighWord(double x)
    {
        long bits = *(long*)&x;
        return (int)(bits >> 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int LowWord(double x)
    {
        long bits = *(long*)&x;
        return (int)bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe double SetHighWord(double x, int hi)
    {
        long bits = *(long*)&x;
        bits = (bits & 0x00000000FFFFFFFFL) | ((long)hi << 32);
        return *(double*)&bits;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe double SetLowWord(double x, int lo)
    {
        long bits = *(long*)&x;
        bits = (bits & unchecked((long)0xFFFFFFFF00000000L)) | ((long)lo & 0xFFFFFFFFL);
        return *(double*)&bits;
    }

    // =========================================================================
    // Shared constants
    // =========================================================================

    private const double PI = 3.14159265358979323846;
    private const double PI_OVER_2 = 1.57079632679489661923;
    // Low word of the pi/2 double-double split (fdlibm pio2_lo, shared by asin/acos).
    private const double PI_OVER_2_LO = 6.12323399573676603587e-17; /* 0x3C91A626, 0x33145C07 */
    private const double PI_OVER_4 = 0.78539816339744830962;
    private const double LN2 = 0.69314718055994530942;
    // Hi/lo extended-precision split of ln 2 (fdlibm ln2_hi/ln2_lo, shared by exp/log);
    // deliberately distinct from the full-precision LN2 above.
    private const double LN2_HI = 6.93147180369123816490e-01; /* 0x3FE62E42, 0xFEE00000 */
    private const double LN2_LO = 1.90821492927058770002e-10; /* 0x3DEA39EF, 0x35793C76 */
    private const double LOG2_E = 1.44269504088896340736; /* 0x3FF71547, 0x652B82FE */
    private const double LOG10_E = 0.43429448190325182765;

    // =========================================================================
    // Unconditional functions (both arches use C#)
    // =========================================================================

    [RuntimeExport("ceil")]
    internal static double ceil(double x)
    {
        // Exact for every double, from floor (floor.c), which works on the bits: going
        // through long saturated from 2^63 up (ceil(-3e23) was long.MinValue), and lost
        // the sign of a negative result that rounds to zero (ceil(-0.5) is -0.0).
        return -System.Math.Floor(-x);
    }

    [RuntimeExport("ceilf")]
    internal static float ceilf(float x)
    {
        return (float)ceil(x);
    }

    [RuntimeExport("sqrt")]
    internal static double sqrt(double x)
    {
        if (double.IsNaN(x) || x < 0)
        {
            return double.NaN;
        }

        if (double.IsPositiveInfinity(x))
        {
            return double.PositiveInfinity;
        }

        if (x == 0)
        {
            return 0;
        }

        double guess = x;
        double epsilon = 1e-10;

        for (int i = 0; i < 50; i++)
        {
            double nextGuess = (guess + x / guess) / 2.0;
            if (Abs(nextGuess - guess) < epsilon)
            {
                break;
            }

            guess = nextGuess;
        }

        return guess;
    }

    [RuntimeExport("sqrtf")]
    internal static float sqrtf(float x)
    {
        return (float)sqrt(x);
    }

    [RuntimeExport("trunc")]
    internal static double trunc(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (double.IsInfinity(x))
        {
            return x;
        }

        // Not through long, which saturates from 2^63 up: see ceil
        return x < 0 ? ceil(x) : System.Math.Floor(x);
    }

    [RuntimeExport("truncf")]
    internal static float truncf(float x)
    {
        return (float)trunc(x);
    }

    [RuntimeExport("modf")]
    internal static unsafe double ModF(double x, double* intptr)
    {
        if (double.IsNaN(x))
        {
            *intptr = double.NaN;
            return double.NaN;
        }

        if (double.IsInfinity(x))
        {
            *intptr = x;
            return 0.0;
        }

        double intPart = trunc(x);
        *intptr = intPart;
        return x - intPart;
    }

    // MathF.Truncate lowers to modff. A float widens to double exactly, and
    // both parts of a float are floats, so the narrowing loses nothing.
    [RuntimeExport("modff")]
    internal static unsafe float modff(float x, float* intptr)
    {
        double intPart;
        double fraction = ModF(x, &intPart);
        *intptr = (float)intPart;
        return (float)fraction;
    }

    // --------------- fmod (fdlibm e_fmod.c) ---------------
    // Bit-exact remainder; NativeAOT emits calls to this for the C# `%`
    // operator on double (and fmodf for float).

    [RuntimeExport("fmod")]
    internal static double fmod(double x, double y)
    {
        int n, hx, hy, hz, ix, iy, sx, i;
        uint lx, ly, lz;

        hx = HighWord(x);
        lx = (uint)LowWord(x);
        hy = HighWord(y);
        ly = (uint)LowWord(y);
        sx = hx & unchecked((int)0x80000000); /* sign of x */
        hx ^= sx;                             /* |x| */
        hy &= 0x7fffffff;                     /* |y| */

        /* purge off exception values: y=0, x not finite, or y NaN */
        if ((hy | (int)ly) == 0 || hx >= 0x7ff00000 ||
            (hy | (ly != 0 ? 1 : 0)) > 0x7ff00000)
        {
            return (x * y) / (x * y);
        }

        if (hx <= hy)
        {
            if (hx < hy || lx < ly)
            {
                return x; /* |x| < |y| */
            }

            if (lx == ly)
            {
                return sx != 0 ? -0.0 : 0.0; /* |x| == |y| */
            }
        }

        /* determine ix = ilogb(x) */
        if (hx < 0x00100000)
        {
            if (hx == 0)
            {
                for (ix = -1043, i = (int)lx; i > 0; i <<= 1)
                {
                    ix -= 1;
                }
            }
            else
            {
                for (ix = -1022, i = hx << 11; i > 0; i <<= 1)
                {
                    ix -= 1;
                }
            }
        }
        else
        {
            ix = (hx >> 20) - 1023;
        }

        /* determine iy = ilogb(y) */
        if (hy < 0x00100000)
        {
            if (hy == 0)
            {
                for (iy = -1043, i = (int)ly; i > 0; i <<= 1)
                {
                    iy -= 1;
                }
            }
            else
            {
                for (iy = -1022, i = hy << 11; i > 0; i <<= 1)
                {
                    iy -= 1;
                }
            }
        }
        else
        {
            iy = (hy >> 20) - 1023;
        }

        /* set up {hx,lx}, {hy,ly} and align y to x */
        if (ix >= -1022)
        {
            hx = 0x00100000 | (0x000fffff & hx);
        }
        else
        {
            /* subnormal x, shift x to normal */
            n = -1022 - ix;
            if (n <= 31)
            {
                hx = (hx << n) | (int)(lx >> (32 - n));
                lx <<= n;
            }
            else
            {
                hx = (int)(lx << (n - 32));
                lx = 0;
            }
        }

        if (iy >= -1022)
        {
            hy = 0x00100000 | (0x000fffff & hy);
        }
        else
        {
            /* subnormal y, shift y to normal */
            n = -1022 - iy;
            if (n <= 31)
            {
                hy = (hy << n) | (int)(ly >> (32 - n));
                ly <<= n;
            }
            else
            {
                hy = (int)(ly << (n - 32));
                ly = 0;
            }
        }

        /* fixed-point fmod */
        n = ix - iy;
        while (n-- != 0)
        {
            hz = hx - hy;
            lz = lx - ly;
            if (lx < ly)
            {
                hz -= 1;
            }

            if (hz < 0)
            {
                hx = hx + hx + (int)(lx >> 31);
                lx += lx;
            }
            else
            {
                if ((hz | (int)lz) == 0)
                {
                    return sx != 0 ? -0.0 : 0.0;
                }

                hx = hz + hz + (int)(lz >> 31);
                lx = lz + lz;
            }
        }

        hz = hx - hy;
        lz = lx - ly;
        if (lx < ly)
        {
            hz -= 1;
        }

        if (hz >= 0)
        {
            hx = hz;
            lx = lz;
        }

        /* convert back to floating value and restore the sign */
        if ((hx | (int)lx) == 0)
        {
            return sx != 0 ? -0.0 : 0.0;
        }

        while (hx < 0x00100000)
        {
            /* normalize x */
            hx = hx + hx + (int)(lx >> 31);
            lx += lx;
            iy -= 1;
        }

        if (iy >= -1022)
        {
            /* normalize output */
            hx = (hx - 0x00100000) | ((iy + 1023) << 20);
            return BitsToDouble(((long)(hx | sx) << 32) | lx);
        }

        /* subnormal output */
        n = -1022 - iy;
        if (n <= 20)
        {
            lx = (lx >> n) | ((uint)hx << (32 - n));
            hx >>= n;
        }
        else if (n <= 31)
        {
            lx = (uint)((hx << (32 - n)) | (int)(lx >> n));
            hx = sx;
        }
        else
        {
            lx = (uint)(hx >> (n - 32));
            hx = sx;
        }

        return BitsToDouble(((long)(hx | sx) << 32) | lx) * 1.0;
    }

    [RuntimeExport("fmodf")]
    internal static float fmodf(float x, float y) => (float)fmod(x, y);

    [RuntimeExport("fma")]
    internal static double fma(double x, double y, double z)
    {
        return x * y + z;
    }

    [RuntimeExport("fmaf")]
    internal static float fmaf(float x, float y, float z)
    {
        return x * y + z;
    }

    // =========================================================================
    // Core fdlibm implementations (private, no RuntimeExport)
    // Used by shared derived functions on both arches.
    // On ARM64, also RuntimeExported below.
    // On x64, core symbols come from x87 asm; these C# versions serve only
    // as internal helpers for the shared derived functions.
    // =========================================================================

    // --------------- exp (fdlibm e_exp.c) ---------------

    private static double FdlibmExp(double x)
    {
        const double o_threshold = 7.09782712893383973096e+02;  /* 0x40862E42, 0xFEFA39EF */
        const double u_threshold = -7.45133219101941108420e+02; /* 0xC0874910, 0xD52D3051 */
        const double twom1000 = 9.33263618503218878990e-302;    /* 2^-1000 */
        const double P1 = 1.66666666666666019037e-01;           /* 0x3FC55555, 0x5555553E */
        const double P2 = -2.77777777770155933842e-03;          /* 0xBF66C16C, 0x16BEBD93 */
        const double P3 = 6.61375632143793436117e-05;           /* 0x3F11566A, 0xAF25DE2C */
        const double P4 = -1.65339022054652515390e-06;          /* 0xBEBBBD41, 0xC5D26BF1 */
        const double P5 = 4.13813679705723846039e-08;           /* 0x3E663769, 0x72BEA4D0 */
        const double huge = 1.0e+300;

        double y, hi = 0, lo = 0, t, c;
        int k = 0;

        int hx = HighWord(x);
        int xsb = (hx >> 31) & 1;
        hx &= 0x7fffffff;

        if (hx >= 0x40862E42)
        {
            if (hx >= 0x7ff00000)
            {
                if (((hx & 0xfffff) | LowWord(x)) != 0)
                {
                    return x;
                }

                return xsb == 0 ? x : 0.0;
            }

            if (x > o_threshold)
            {
                return double.PositiveInfinity;
            }

            if (x < u_threshold)
            {
                return 0;
            }
        }

        if (hx > 0x3fd62e42)
        {
            if (hx < 0x3FF0A2B2)
            {
                if (xsb == 0)
                {
                    hi = x - LN2_HI;
                    lo = LN2_LO;
                }
                else
                {
                    hi = x + LN2_HI;
                    lo = -LN2_LO;
                }

                k = 1 - xsb - xsb;
            }
            else
            {
                k = (int)(LOG2_E * x + (xsb == 0 ? 0.5 : -0.5));
                t = k;
                hi = x - t * LN2_HI;
                lo = t * LN2_LO;
            }

            x = hi - lo;
        }
        else if (hx < 0x3e300000)
        {
            if (huge + x > 1)
            {
                return 1 + x;
            }
        }
        else
        {
            k = 0;
        }

        t = x * x;
        c = x - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));

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
            long _y = DoubleToBits(y);
            _y += (long)k << 52;
            return BitsToDouble(_y);
        }
        else
        {
            long _y = DoubleToBits(y);
            _y += ((long)k + 1000) << 52;
            return BitsToDouble(_y) * twom1000;
        }
    }

    // --------------- log (fdlibm e_log.c) ---------------

    private static double FdlibmLog(double x)
    {
        const double two54 = 1.80143985094819840000e+16;
        const double Lg1 = 6.666666666666735130e-01;
        const double Lg2 = 3.999999999940941908e-01;
        const double Lg3 = 2.857142874366239149e-01;
        const double Lg4 = 2.222219843214978396e-01;
        const double Lg5 = 1.818357216161805012e-01;
        const double Lg6 = 1.531383769920937332e-01;
        const double Lg7 = 1.479819860511658591e-01;

        double hfsq, R, dk;

        int hx = HighWord(x);
        uint lx = (uint)LowWord(x);

        int k = 0;
        if (hx < 0x00100000)
        {
            if (x < 0 || double.IsNaN(x))
            {
                return double.NaN;
            }

            if (((hx & 0x7fffffff) | (int)lx) == 0)
            {
                return double.NegativeInfinity;
            }

            k -= 54;
            x *= two54;
            hx = HighWord(x);
        }

        if (hx >= 0x7ff00000)
        {
            return x + x;
        }

        k += (hx >> 20) - 1023;
        hx &= 0x000fffff;
        int i = (hx + 0x95f64) & 0x100000;
        x = SetHighWord(x, hx | (i ^ 0x3ff00000));
        k += i >> 20;
        double f = x - 1.0;

        if ((0x000fffff & (2 + hx)) < 3)
        {
            if (f == 0)
            {
                if (k == 0)
                {
                    return 0;
                }

                dk = k;
                return dk * LN2_HI + dk * LN2_LO;
            }

            R = f * f * (0.5 - 0.33333333333333333 * f);
            if (k == 0)
            {
                return f - R;
            }

            dk = k;
            return dk * LN2_HI - (R - dk * LN2_LO - f);
        }

        double s = f / (2.0 + f);
        dk = k;
        double z = s * s;
        i = hx - 0x6147a;
        double w = z * z;
        int j = 0x6b851 - hx;
        double t1 = w * (Lg2 + w * (Lg4 + w * Lg6));
        double t2 = z * (Lg1 + w * (Lg3 + w * (Lg5 + w * Lg7)));
        i |= j;
        R = t2 + t1;

        if (i > 0)
        {
            hfsq = 0.5 * f * f;
            if (k == 0)
            {
                return f - (hfsq - s * (hfsq + R));
            }

            return dk * LN2_HI - (hfsq - (s * (hfsq + R) + dk * LN2_LO) - f);
        }
        else
        {
            if (k == 0)
            {
                return f - s * (f - R);
            }

            return dk * LN2_HI - (s * (f - R) - dk * LN2_LO - f);
        }
    }

    // --------------- atan (fdlibm s_atan.c) ---------------

    private const double atanhi_0 = 4.63647609000806093515e-01; /* 0x3FDDAC67, 0x0561BB4F */
    private const double atanhi_1 = 7.85398163397448278999e-01; /* 0x3FE921FB, 0x54442D18 */
    private const double atanhi_2 = 9.82793723247329054082e-01; /* 0x3FEF730B, 0xD281F69B */
    private const double atanhi_3 = PI_OVER_2;                  /* 0x3FF921FB, 0x54442D18 */
    private const double atanlo_0 = 2.26987774529616870924e-17; /* 0x3C7A2B7F, 0x222F65E2 */
    private const double atanlo_1 = 3.06161699786838301793e-17; /* 0x3C81A626, 0x33145C07 */
    private const double atanlo_2 = 1.39033110312309984516e-17; /* 0x3C700788, 0x7AF0CBBD */
    private const double atanlo_3 = 6.12323399573676603587e-17; /* 0x3C91A626, 0x33145C07 */
    private const double aT_0 = 3.33333333333329318027e-01;     /* 0x3FD55555, 0x5555550D */
    private const double aT_1 = -1.99999999998764832476e-01;    /* 0xBFC99999, 0x9998EBC4 */
    private const double aT_2 = 1.42857142725034663711e-01;     /* 0x3FC24924, 0x920083FF */
    private const double aT_3 = -1.11111104054623557880e-01;    /* 0xBFBC71C6, 0xFE231671 */
    private const double aT_4 = 9.09088713343650656196e-02;     /* 0x3FB745CD, 0xC54C206E */
    private const double aT_5 = -7.69187620504482999495e-02;    /* 0xBFB3B0F2, 0xAF749A6D */
    private const double aT_6 = 6.66107313738753120669e-02;     /* 0x3FB10D66, 0xA0D03D51 */
    private const double aT_7 = -5.83357013379057348645e-02;    /* 0xBFADDE2D, 0x52DEFD9A */
    private const double aT_8 = 4.97687799461593236017e-02;     /* 0x3FA97B4B, 0x24760DEB */
    private const double aT_9 = -3.65315727442169155270e-02;    /* 0xBFA2B444, 0x2C6A6C2F */
    private const double aT_10 = 1.62858201153657823623e-02;    /* 0x3F90AD3A, 0xE322DA11 */

    private static double FdlibmAtan(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        int id;
        int hx = HighWord(x);
        int ix = hx & 0x7fffffff;

        if (ix >= 0x44100000)
        {
            if (ix > 0x7ff00000 || (ix == 0x7ff00000 && LowWord(x) != 0))
            {
                return x + x;
            }

            if (hx > 0)
            {
                return atanhi_3 + atanlo_3;
            }

            return -atanhi_3 - atanlo_3;
        }

        if (ix < 0x3fdc0000)
        {
            if (ix < 0x3e200000)
            {
                if (1.0e+300 + x > 1)
                {
                    return x;
                }
            }

            id = -1;
        }
        else
        {
            x = Abs(x);
            if (ix < 0x3ff30000)
            {
                if (ix < 0x3fe60000)
                {
                    id = 0;
                    x = (2.0 * x - 1) / (2.0 + x);
                }
                else
                {
                    id = 1;
                    x = (x - 1) / (x + 1);
                }
            }
            else
            {
                if (ix < 0x40038000)
                {
                    id = 2;
                    x = (x - 1.5) / (1 + 1.5 * x);
                }
                else
                {
                    id = 3;
                    x = -1.0 / x;
                }
            }
        }

        double z = x * x;
        double w = z * z;
        double s1 = z * (aT_0 + w * (aT_2 + w * (aT_4 + w * (aT_6 + w * (aT_8 + w * aT_10)))));
        double s2 = w * (aT_1 + w * (aT_3 + w * (aT_5 + w * (aT_7 + w * aT_9))));

        if (id < 0)
        {
            return x - x * (s1 + s2);
        }

        double ahi, alo;
        switch (id)
        {
            case 0: ahi = atanhi_0; alo = atanlo_0; break;
            case 1: ahi = atanhi_1; alo = atanlo_1; break;
            case 2: ahi = atanhi_2; alo = atanlo_2; break;
            default: ahi = atanhi_3; alo = atanlo_3; break;
        }

        z = ahi - (x * (s1 + s2) - alo - x);
        return hx < 0 ? -z : z;
    }

    // =========================================================================
    // ARM64-only: core transcendental RuntimeExports
    // On x64 these symbols come from x87 FPU assembly.
    // =========================================================================

#if ARCH_ARM64

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double SinCore(double x)
    {
        double x2 = x * x;
        double term = x;
        double sum = x;

        term *= -x2 / (2.0 * 3.0);     sum += term;
        term *= -x2 / (4.0 * 5.0);     sum += term;
        term *= -x2 / (6.0 * 7.0);     sum += term;
        term *= -x2 / (8.0 * 9.0);     sum += term;
        term *= -x2 / (10.0 * 11.0);   sum += term;
        term *= -x2 / (12.0 * 13.0);   sum += term;
        term *= -x2 / (14.0 * 15.0);   sum += term;
        term *= -x2 / (16.0 * 17.0);   sum += term;

        return sum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double CosCore(double x)
    {
        double x2 = x * x;
        double term = 1.0;
        double sum = 1.0;

        term *= -x2 / (1.0 * 2.0);     sum += term;
        term *= -x2 / (3.0 * 4.0);     sum += term;
        term *= -x2 / (5.0 * 6.0);     sum += term;
        term *= -x2 / (7.0 * 8.0);     sum += term;
        term *= -x2 / (9.0 * 10.0);    sum += term;
        term *= -x2 / (11.0 * 12.0);   sum += term;
        term *= -x2 / (13.0 * 14.0);   sum += term;
        term *= -x2 / (15.0 * 16.0);   sum += term;

        return sum;
    }

    [RuntimeExport("sin")]
    internal static double sin(double x)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            return double.NaN;
        }

        double sign = 1.0;
        if (x < 0)
        {
            sign = -1.0;
            x = -x;
        }

        double j = RoundToNearest(x / PI_OVER_2);
        double r = x - j * PI_OVER_2;
        int q = (int)j & 3;

        double result;
        if (q == 0)
        {
            result = SinCore(r);
        }
        else if (q == 1)
        {
            result = CosCore(r);
        }
        else if (q == 2)
        {
            result = -SinCore(r);
        }
        else
        {
            result = -CosCore(r);
        }

        return sign * result;
    }

    [RuntimeExport("sinf")]
    internal static float sinf(float x) => (float)sin(x);

    [RuntimeExport("cos")]
    internal static double cos(double x)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            return double.NaN;
        }

        x = x < 0 ? -x : x;

        double j = RoundToNearest(x / PI_OVER_2);
        double r = x - j * PI_OVER_2;
        int q = (int)j & 3;

        if (q == 0)
        {
            return CosCore(r);
        }

        if (q == 1)
        {
            return -SinCore(r);
        }

        if (q == 2)
        {
            return -CosCore(r);
        }

        return SinCore(r);
    }

    [RuntimeExport("cosf")]
    internal static float cosf(float x) => (float)cos(x);

    [RuntimeExport("tan")]
    internal static double tan(double x)
    {
        return sin(x) / cos(x);
    }

    [RuntimeExport("tanf")]
    internal static float tanf(float x) => (float)tan(x);

    [RuntimeExport("exp")]
    internal static double exp(double x) => FdlibmExp(x);

    [RuntimeExport("expf")]
    internal static float expf(float x) => (float)FdlibmExp(x);

    [RuntimeExport("log")]
    internal static double log(double x) => FdlibmLog(x);

    [RuntimeExport("logf")]
    internal static float logf(float x) => (float)FdlibmLog(x);

    [RuntimeExport("atan")]
    internal static double atan(double x) => FdlibmAtan(x);

    [RuntimeExport("atanf")]
    internal static float atanf(float x) => (float)FdlibmAtan(x);

#endif

    // =========================================================================
    // Shared derived functions (both arches use C#)
    // These call FdlibmExp/FdlibmLog/FdlibmAtan internally.
    // =========================================================================

    // --------------- log2 ---------------

    [RuntimeExport("log2")]
    internal static double log2(double x)
    {
        double ln = FdlibmLog(x);
        if (double.IsNaN(ln) || double.IsInfinity(ln))
        {
            return ln;
        }

        return ln / LN2;
    }

    [RuntimeExport("log2f")]
    internal static float log2f(float x) => (float)log2(x);

    // --------------- log10 ---------------

    [RuntimeExport("log10")]
    internal static double log10(double x)
    {
        double ln = FdlibmLog(x);
        if (double.IsNaN(ln) || double.IsInfinity(ln))
        {
            return ln;
        }

        return ln * LOG10_E;
    }

    [RuntimeExport("log10f")]
    internal static float log10f(float x) => (float)log10(x);

    // --------------- atan2 ---------------

    [RuntimeExport("atan2")]
    internal static double atan2(double y, double x)
    {
        if (double.IsNaN(x) || double.IsNaN(y))
        {
            return double.NaN;
        }

        if (double.IsPositiveInfinity(y))
        {
            if (double.IsPositiveInfinity(x))
            {
                return PI_OVER_4;
            }

            if (double.IsNegativeInfinity(x))
            {
                return 3.0 * PI_OVER_4;
            }

            return PI_OVER_2;
        }

        if (double.IsNegativeInfinity(y))
        {
            if (double.IsPositiveInfinity(x))
            {
                return -PI_OVER_4;
            }

            if (double.IsNegativeInfinity(x))
            {
                return -3.0 * PI_OVER_4;
            }

            return -PI_OVER_2;
        }

        if (double.IsPositiveInfinity(x))
        {
            return 0.0;
        }

        if (double.IsNegativeInfinity(x))
        {
            return y >= 0 ? PI : -PI;
        }

        if (x == 0)
        {
            if (y > 0)
            {
                return PI_OVER_2;
            }

            if (y < 0)
            {
                return -PI_OVER_2;
            }

            return 0.0;
        }

        double a = FdlibmAtan(y / x);

        if (x < 0)
        {
            return y >= 0 ? a + PI : a - PI;
        }

        return a;
    }

    [RuntimeExport("atan2f")]
    internal static float atan2f(float y, float x) => (float)atan2(y, x);

    // --------------- asin/acos shared coefficients ---------------
    // Rational approximation R(x^2) of fdlibm e_asin.c, reused verbatim by e_acos.c.

    private const double pS0 = 1.66666666666666657415e-01;
    private const double pS1 = -3.25565818622400915405e-01;
    private const double pS2 = 2.01212532134862925881e-01;
    private const double pS3 = -4.00555345006794114027e-02;
    private const double pS4 = 7.91534994289814532176e-04;
    private const double pS5 = 3.47933107596021167570e-05;
    private const double qS1 = -2.40339491173441421878e+00;
    private const double qS2 = 2.02094576023350569471e+00;
    private const double qS3 = -6.88283971605453293030e-01;
    private const double qS4 = 7.70381505559019352791e-02;

    // --------------- asin (fdlibm e_asin.c) ---------------

    [RuntimeExport("asin")]
    internal static double asin(double x)
    {
        const double huge = 1.000e+300;
        const double pio2_hi = PI_OVER_2;

        double t = 0, w, p, q, c, r, s;
        int hx = HighWord(x);
        int ix = hx & 0x7fffffff;

        if (ix >= 0x3ff00000)
        {
            if (((ix - 0x3ff00000) | LowWord(x)) == 0)
            {
                return x * pio2_hi + x * PI_OVER_2_LO;
            }

            return (x - x) / (x - x);
        }
        else if (ix < 0x3fe00000)
        {
            if (ix < 0x3e400000)
            {
                if (huge + x > 1)
                {
                    return x;
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

        w = 1 - Abs(x);
        t = w * 0.5;
        p = t * (pS0 + t * (pS1 + t * (pS2 + t * (pS3 + t * (pS4 + t * pS5)))));
        q = 1 + t * (qS1 + t * (qS2 + t * (qS3 + t * qS4)));
        s = sqrt(t);

        if (ix >= 0x3FEF3333)
        {
            w = p / q;
            t = pio2_hi - (2.0 * (s + s * w) - PI_OVER_2_LO);
        }
        else
        {
            w = s;
            w = SetLowWord(w, 0);
            c = (t - w * w) / (s + w);
            r = p / q;
            p = 2.0 * s * r - (PI_OVER_2_LO - 2.0 * c);
            q = PI_OVER_4 - 2.0 * w;
            t = PI_OVER_4 - (p - q);
        }

        return hx > 0 ? t : -t;
    }

    [RuntimeExport("asinf")]
    internal static float asinf(float x) => (float)asin(x);

    // --------------- acos (fdlibm e_acos.c) ---------------

    [RuntimeExport("acos")]
    internal static double acos(double x)
    {
        const double pio2_hi = PI_OVER_2;

        double z, p, q, r, w, s, c, df;
        int hx = HighWord(x);
        int ix = hx & 0x7fffffff;

        if (ix >= 0x3ff00000)
        {
            if (((ix - 0x3ff00000) | LowWord(x)) == 0)
            {
                if (hx > 0)
                {
                    return 0.0;
                }

                return PI + 2.0 * PI_OVER_2_LO;
            }

            return (x - x) / (x - x);
        }

        if (ix < 0x3fe00000)
        {
            if (ix <= 0x3c600000)
            {
                return pio2_hi + PI_OVER_2_LO;
            }

            z = x * x;
            p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
            q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
            r = p / q;
            return pio2_hi - (x - (PI_OVER_2_LO - x * r));
        }
        else if (hx < 0)
        {
            z = (1 + x) * 0.5;
            p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
            q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
            s = sqrt(z);
            r = p / q;
            w = r * s - PI_OVER_2_LO;
            return PI - 2.0 * (s + w);
        }
        else
        {
            z = (1 - x) * 0.5;
            s = sqrt(z);
            df = s;
            df = SetLowWord(df, 0);
            c = (z - df * df) / (s + df);
            p = z * (pS0 + z * (pS1 + z * (pS2 + z * (pS3 + z * (pS4 + z * pS5)))));
            q = 1 + z * (qS1 + z * (qS2 + z * (qS3 + z * qS4)));
            r = p / q;
            w = r * s + c;
            return 2.0 * (df + w);
        }
    }

    [RuntimeExport("acosf")]
    internal static float acosf(float x) => (float)acos(x);

    // --------------- pow (fdlibm e_pow.c) ---------------
    // Within 1 ulp, and exact whenever the result is representable (powers of
    // two, small integer powers), but for |y| > 2^31 with x within 2^-20 of 1,
    // where fdlibm's series for log(x) leaves some hundred ulps. Going through
    // exp(y*log(x)) amplified the rounding error of log by y, by up to 2^10 for
    // results near the overflow.

    [RuntimeExport("pow")]
    internal static double pow(double x, double y)
    {
        const double two53 = 9007199254740992.0;                /* 0x43400000, 0x00000000 */
        const double huge = 1.0e300;
        const double tiny = 1.0e-300;
        /* poly coefs for (3/2)*(log(x)-2s-2/3*s**3 */
        const double L1 = 5.99999999999994648725e-01;           /* 0x3FE33333, 0x33333303 */
        const double L2 = 4.28571428578550184252e-01;           /* 0x3FDB6DB6, 0xDB6FABFF */
        const double L3 = 3.33333329818377432918e-01;           /* 0x3FD55555, 0x518F264D */
        const double L4 = 2.72728123808534006489e-01;           /* 0x3FD17460, 0xA91D4101 */
        const double L5 = 2.30660745775561754067e-01;           /* 0x3FCD864A, 0x93C9DB65 */
        const double L6 = 2.06975017800338417784e-01;           /* 0x3FCA7E28, 0x4A454EEF */
        const double P1 = 1.66666666666666019037e-01;           /* 0x3FC55555, 0x5555553E */
        const double P2 = -2.77777777770155933842e-03;          /* 0xBF66C16C, 0x16BEBD93 */
        const double P3 = 6.61375632143793436117e-05;           /* 0x3F11566A, 0xAF25DE2C */
        const double P4 = -1.65339022054652515390e-06;          /* 0xBEBBBD41, 0xC5D26BF1 */
        const double P5 = 4.13813679705723846039e-08;           /* 0x3E663769, 0x72BEA4D0 */
        const double lg2 = 6.93147180559945286227e-01;          /* 0x3FE62E42, 0xFEFA39EF */
        const double lg2_h = 6.93147182464599609375e-01;        /* 0x3FE62E43, 0x00000000 */
        const double lg2_l = -1.90465429995776804525e-09;       /* 0xBE205C61, 0x0CA86C39 */
        const double ovt = 8.0085662595372944372e-17;           /* -(1024-log2(ovfl+.5ulp)) */
        const double cp = 9.61796693925975554329e-01;           /* 0x3FEEC709, 0xDC3A03FD =2/(3ln2) */
        const double cp_h = 9.61796700954437255859e-01;         /* 0x3FEEC709, 0xE0000000 =(float)cp */
        const double cp_l = -7.02846165095275826516e-09;        /* 0xBE3E2FE0, 0x145B01F5 =tail of cp_h*/
        const double ivln2 = 1.44269504088896338700e+00;        /* 0x3FF71547, 0x652B82FE =1/ln2 */
        const double ivln2_h = 1.44269502162933349609e+00;      /* 0x3FF71547, 0x60000000 =24b 1/ln2*/
        const double ivln2_l = 1.92596299112661746887e-08;      /* 0x3E54AE0B, 0xF85DDF44 =1/ln2 tail*/

        double z, ax, z_h, z_l, p_h, p_l;
        double y1, t1, t2, r, s, t, u, v, w;
        int i, j, k, yisint, n;
        int hx, hy, ix, iy;
        uint lx, ly;

        hx = HighWord(x);
        lx = (uint)LowWord(x);
        hy = HighWord(y);
        ly = (uint)LowWord(y);
        ix = hx & 0x7fffffff;
        iy = hy & 0x7fffffff;

        /* y==zero: x**0 = 1 */
        if ((iy | (int)ly) == 0)
        {
            return 1.0;
        }

        /* x==1: 1**y = 1, even if y is NaN (C99, as FreeBSD's e_pow.c) */
        if (hx == 0x3ff00000 && lx == 0)
        {
            return 1.0;
        }

        /* +-NaN return x+y */
        if (ix > 0x7ff00000 || ((ix == 0x7ff00000) && (lx != 0)) ||
            iy > 0x7ff00000 || ((iy == 0x7ff00000) && (ly != 0)))
        {
            return x + y;
        }

        /* determine if y is an odd int when x < 0
         * yisint = 0 ... y is not an integer
         * yisint = 1 ... y is an odd int
         * yisint = 2 ... y is an even int
         */
        yisint = 0;
        if (hx < 0)
        {
            if (iy >= 0x43400000)
            {
                yisint = 2; /* even integer y */
            }
            else if (iy >= 0x3ff00000)
            {
                k = (iy >> 20) - 0x3ff; /* exponent */
                if (k > 20)
                {
                    uint jl = ly >> (52 - k);
                    if ((jl << (52 - k)) == ly)
                    {
                        yisint = 2 - (int)(jl & 1);
                    }
                }
                else if (ly == 0)
                {
                    j = iy >> (20 - k);
                    if ((j << (20 - k)) == iy)
                    {
                        yisint = 2 - (j & 1);
                    }
                }
            }
        }

        /* special value of y */
        if (ly == 0)
        {
            if (iy == 0x7ff00000)
            {
                /* y is +-inf */
                if (((ix - 0x3ff00000) | (int)lx) == 0)
                {
                    return 1.0; /* (-1)**+-inf is 1 (C99, as FreeBSD's e_pow.c) */
                }
                else if (ix >= 0x3ff00000)
                {
                    return hy >= 0 ? y : 0.0; /* (|x|>1)**+-inf = inf,0 */
                }
                else
                {
                    return hy < 0 ? -y : 0.0; /* (|x|<1)**-,+inf = inf,0 */
                }
            }

            if (iy == 0x3ff00000)
            {
                /* y is +-1 */
                return hy < 0 ? 1.0 / x : x;
            }

            if (hy == 0x40000000)
            {
                return x * x; /* y is 2 */
            }

            if (hy == 0x3fe00000 && hx >= 0)
            {
                /* y is 0.5, x >= +0: the hardware square root, the export above is not exact */
                return System.Math.Sqrt(x);
            }
        }

        ax = SetHighWord(x, ix); /* fabs(x): Abs keeps the sign of -0 */
        /* special value of x */
        if (lx == 0 && (ix == 0x7ff00000 || ix == 0 || ix == 0x3ff00000))
        {
            z = ax; /* x is +-0,+-inf,+-1 */
            if (hy < 0)
            {
                z = 1.0 / z; /* z = (1/|x|) */
            }

            if (hx < 0)
            {
                if (((ix - 0x3ff00000) | yisint) == 0)
                {
                    z = (z - z) / (z - z); /* (-1)**non-int is NaN */
                }
                else if (yisint == 1)
                {
                    z = -z; /* (x<0)**odd = -(|x|**odd) */
                }
            }

            return z;
        }

        n = (hx >> 31) + 1;

        /* (x<0)**(non-int) is NaN */
        if ((n | yisint) == 0)
        {
            return (x - x) / (x - x);
        }

        s = 1.0; /* s (sign of result -ve**odd) = -1 else = 1 */
        if ((n | (yisint - 1)) == 0)
        {
            s = -1.0; /* (-ve)**(odd int) */
        }

        /* |y| is huge */
        if (iy > 0x41e00000)
        {
            /* if |y| > 2**31 */
            if (iy > 0x43f00000)
            {
                /* if |y| > 2**64, must o/uflow */
                if (ix <= 0x3fefffff)
                {
                    return hy < 0 ? huge * huge : tiny * tiny;
                }

                if (ix >= 0x3ff00000)
                {
                    return hy > 0 ? huge * huge : tiny * tiny;
                }
            }

            /* over/underflow if x is not close to one */
            if (ix < 0x3fefffff)
            {
                return hy < 0 ? s * huge * huge : s * tiny * tiny;
            }

            if (ix > 0x3ff00000)
            {
                return hy > 0 ? s * huge * huge : s * tiny * tiny;
            }

            /* now |1-x| is tiny <= 2**-20, suffice to compute
               log(x) by x-x^2/2+x^3/3-x^4/4 */
            t = ax - 1.0; /* t has 20 trailing zeros */
            w = (t * t) * (0.5 - t * (0.3333333333333333333333 - t * 0.25));
            u = ivln2_h * t; /* ivln2_h has 21 sig. bits */
            v = t * ivln2_l - w * ivln2;
            t1 = SetLowWord(u + v, 0);
            t2 = v - (t1 - u);
        }
        else
        {
            double ss, s2, s_h, s_l, t_h, t_l, bp, dp_h, dp_l;
            n = 0;
            /* take care subnormal number */
            if (ix < 0x00100000)
            {
                ax *= two53;
                n -= 53;
                ix = HighWord(ax);
            }

            n += (ix >> 20) - 0x3ff;
            j = ix & 0x000fffff;
            /* determine interval */
            ix = j | 0x3ff00000; /* normalize ix */
            if (j <= 0x3988E)
            {
                k = 0; /* |x|<sqrt(3/2) */
            }
            else if (j < 0xBB67A)
            {
                k = 1; /* |x|<sqrt(3) */
            }
            else
            {
                k = 0;
                n += 1;
                ix -= 0x00100000;
            }

            ax = SetHighWord(ax, ix);

            /* bp[k], dp_h[k], dp_l[k] */
            bp = k == 0 ? 1.0 : 1.5;
            dp_h = k == 0 ? 0.0 : 5.84962487220764160156e-01; /* 0x3FE2B803, 0x40000000 */
            dp_l = k == 0 ? 0.0 : 1.35003920212974897128e-08; /* 0x3E4CFDEB, 0x43CFD006 */

            /* compute ss = s_h+s_l = (x-1)/(x+1) or (x-1.5)/(x+1.5) */
            u = ax - bp;
            v = 1.0 / (ax + bp);
            ss = u * v;
            s_h = SetLowWord(ss, 0);
            /* t_h=ax+bp[k] High */
            t_h = SetHighWord(0.0, ((ix >> 1) | 0x20000000) + 0x00080000 + (k << 18));
            t_l = ax - (t_h - bp);
            s_l = v * ((u - s_h * t_h) - s_h * t_l);
            /* compute log(ax) */
            s2 = ss * ss;
            r = s2 * s2 * (L1 + s2 * (L2 + s2 * (L3 + s2 * (L4 + s2 * (L5 + s2 * L6)))));
            r += s_l * (s_h + ss);
            s2 = s_h * s_h;
            t_h = SetLowWord(3.0 + s2 + r, 0);
            t_l = r - ((t_h - 3.0) - s2);
            /* u+v = ss*(1+...) */
            u = s_h * t_h;
            v = s_l * t_h + t_l * ss;
            /* 2/(3log2)*(ss+...) */
            p_h = SetLowWord(u + v, 0);
            p_l = v - (p_h - u);
            z_h = cp_h * p_h; /* cp_h+cp_l = 2/(3*log2) */
            z_l = cp_l * p_h + p_l * cp + dp_l;
            /* log2(ax) = (ss+..)*2/(3*log2) = n + dp_h + z_h + z_l */
            t = n;
            t1 = SetLowWord(((z_h + z_l) + dp_h) + t, 0);
            t2 = z_l - (((t1 - t) - dp_h) - z_h);
        }

        /* split up y into y1+y2 and compute (y1+y2)*(t1+t2) */
        y1 = SetLowWord(y, 0);
        p_l = (y - y1) * t1 + y * t2;
        p_h = y1 * t1;
        z = p_l + p_h;
        j = HighWord(z);
        i = LowWord(z);
        if (j >= 0x40900000)
        {
            /* z >= 1024 */
            if (((j - 0x40900000) | i) != 0)
            {
                return s * huge * huge; /* overflow */
            }

            if (p_l + ovt > z - p_h)
            {
                return s * huge * huge; /* overflow */
            }
        }
        else if ((j & 0x7fffffff) >= 0x4090cc00)
        {
            /* z <= -1075 */
            if (((j - unchecked((int)0xc090cc00)) | i) != 0)
            {
                return s * tiny * tiny; /* underflow */
            }

            if (p_l <= z - p_h)
            {
                return s * tiny * tiny; /* underflow */
            }
        }

        /*
         * compute 2**(p_h+p_l)
         */
        i = j & 0x7fffffff;
        k = (i >> 20) - 0x3ff;
        n = 0;
        if (i > 0x3fe00000)
        {
            /* if |z| > 0.5, set n = [z+0.5] */
            n = j + (0x00100000 >> (k + 1));
            k = ((n & 0x7fffffff) >> 20) - 0x3ff; /* new k for n */
            t = SetHighWord(0.0, n & ~(0x000fffff >> k));
            n = ((n & 0x000fffff) | 0x00100000) >> (20 - k);
            if (j < 0)
            {
                n = -n;
            }

            p_h -= t;
        }

        t = SetLowWord(p_l + p_h, 0);
        u = t * lg2_h;
        v = (p_l - (t - p_h)) * lg2 + t * lg2_l;
        z = u + v;
        w = v - (z - u);
        t = z * z;
        t1 = z - t * (P1 + t * (P2 + t * (P3 + t * (P4 + t * P5))));
        r = (z * t1) / (t1 - 2.0) - (w + z * w);
        z = 1.0 - (r - z);
        j = HighWord(z) + (n << 20);
        if ((j >> 20) <= 0)
        {
            z = System.Math.ScaleB(z, n); /* subnormal output */
        }
        else
        {
            z = SetHighWord(z, j);
        }

        return s * z;
    }

    [RuntimeExport("powf")]
    internal static float powf(float x, float y) => (float)pow(x, y);

    // --------------- hyperbolics (after fdlibm e_cosh.c / e_sinh.c / s_tanh.c) ---------------
    // Structured like fdlibm but with exp(x)-1 standing in for expm1 (not ported),
    // so tiny-argument cases early-return before the precision of exp-1 matters.

    [RuntimeExport("cosh")]
    internal static double cosh(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (double.IsInfinity(x))
        {
            return double.PositiveInfinity;
        }

        double ax = Abs(x);

        /* |x| tiny: cosh(x) = 1 to double precision */
        if (ax < 7.45e-9) /* 2^-27 */
        {
            return 1.0;
        }

        if (ax < 22.0)
        {
            double t = FdlibmExp(ax);
            return 0.5 * t + 0.5 / t;
        }

        /* |x| in [22, log(maxdouble)]: cosh(x) = exp(|x|)/2 */
        if (ax < 7.09782712893383973096e+02)
        {
            return 0.5 * FdlibmExp(ax);
        }

        /* |x| in [log(maxdouble), overflowthreshold] */
        if (ax <= 7.10475860073943863426e+02)
        {
            double w = FdlibmExp(0.5 * ax);
            return 0.5 * w * w;
        }

        return double.PositiveInfinity;
    }

    [RuntimeExport("coshf")]
    internal static float coshf(float x) => (float)cosh(x);

    [RuntimeExport("sinh")]
    internal static double sinh(double x)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            return x + x; /* preserves NaN and signed infinity */
        }

        double ax = Abs(x);
        double h = x < 0 ? -0.5 : 0.5;

        if (ax < 22.0)
        {
            /* |x| tiny: sinh(x) = x to double precision */
            if (ax < 3.73e-9) /* 2^-28 */
            {
                return x;
            }

            double t = FdlibmExp(ax) - 1.0;
            if (ax < 1.0)
            {
                return h * (2.0 * t - t * t / (t + 1.0));
            }

            return h * (t + t / (t + 1.0));
        }

        /* |x| in [22, log(maxdouble)]: sinh(x) = sign(x)*exp(|x|)/2 */
        if (ax < 7.09782712893383973096e+02)
        {
            return h * FdlibmExp(ax);
        }

        /* |x| in [log(maxdouble), overflowthreshold] */
        if (ax <= 7.10475860073943863426e+02)
        {
            double w = FdlibmExp(0.5 * ax);
            return h * w * w;
        }

        return x > 0 ? double.PositiveInfinity : double.NegativeInfinity;
    }

    [RuntimeExport("sinhf")]
    internal static float sinhf(float x) => (float)sinh(x);

    [RuntimeExport("tanh")]
    internal static double tanh(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (double.IsInfinity(x))
        {
            return x > 0 ? 1.0 : -1.0;
        }

        double ax = Abs(x);
        double z;

        if (ax < 22.0)
        {
            /* |x| tiny: tanh(x) = x to double precision */
            if (ax < 2.78e-17) /* 2^-55 */
            {
                return x * (1.0 + x);
            }

            if (ax >= 1.0)
            {
                double t = FdlibmExp(2.0 * ax) - 1.0;
                z = 1.0 - 2.0 / (t + 2.0);
            }
            else
            {
                double t = FdlibmExp(-2.0 * ax) - 1.0;
                z = -t / (t + 2.0);
            }
        }
        else
        {
            z = 1.0; /* |x| >= 22: tanh saturates */
        }

        return x >= 0 ? z : -z;
    }

    [RuntimeExport("tanhf")]
    internal static float tanhf(float x) => (float)tanh(x);

    // --------------- inverse hyperbolics (after fdlibm s_asinh.c / e_acosh.c / e_atanh.c) ---------------
    // log1p is not ported: Log1p computes it from log (Goldberg's correction), close to
    // fdlibm's accuracy where these call it.

    /// <summary>log(1 + y), keeping the precision of a small y that 1 + y rounds.</summary>
    private static double Log1p(double y)
    {
        double u = 1.0 + y;
        if (u == 1.0)
        {
            return y;
        }

        return FdlibmLog(u) * (y / (u - 1.0));
    }

    [RuntimeExport("asinh")]
    internal static double asinh(double x)
    {
        if (double.IsNaN(x) || double.IsInfinity(x))
        {
            return x + x; /* preserves NaN and signed infinity */
        }

        double ax = Abs(x);
        double w;

        /* |x| < 2^-28: asinh(x) = x to double precision */
        if (ax < 3.7252902984619141e-09)
        {
            return x;
        }

        if (ax > 268435456.0) /* |x| > 2^28 */
        {
            w = FdlibmLog(ax) + LN2;
        }
        else if (ax > 2.0)
        {
            w = FdlibmLog(2.0 * ax + 1.0 / (sqrt(x * x + 1.0) + ax));
        }
        else
        {
            double t = x * x;
            w = Log1p(ax + t / (1.0 + sqrt(1.0 + t)));
        }

        return x > 0 ? w : -w;
    }

    [RuntimeExport("asinhf")]
    internal static float asinhf(float x) => (float)asinh(x);

    [RuntimeExport("acosh")]
    internal static double acosh(double x)
    {
        if (double.IsNaN(x) || x < 1.0)
        {
            return double.NaN;
        }

        if (double.IsPositiveInfinity(x))
        {
            return x;
        }

        if (x == 1.0)
        {
            return 0.0;
        }

        if (x > 268435456.0) /* x > 2^28: acosh(x) = log(2x) */
        {
            return FdlibmLog(x) + LN2;
        }

        if (x > 2.0)
        {
            return FdlibmLog(2.0 * x - 1.0 / (x + sqrt(x * x - 1.0)));
        }

        /* 1 < x <= 2 */
        double t = x - 1.0;
        return Log1p(t + sqrt(2.0 * t + t * t));
    }

    [RuntimeExport("acoshf")]
    internal static float acoshf(float x) => (float)acosh(x);

    [RuntimeExport("atanh")]
    internal static double atanh(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        double ax = Abs(x);

        if (ax > 1.0)
        {
            return double.NaN;
        }

        if (ax == 1.0)
        {
            return x > 0 ? double.PositiveInfinity : double.NegativeInfinity;
        }

        /* |x| < 2^-28: atanh(x) = x to double precision */
        if (ax < 3.7252902984619141e-09)
        {
            return x;
        }

        double t = ax < 0.5
            ? 0.5 * Log1p(2.0 * ax + 2.0 * ax * ax / (1.0 - ax))
            : 0.5 * Log1p((ax + ax) / (1.0 - ax));

        return x >= 0 ? t : -t;
    }

    [RuntimeExport("atanhf")]
    internal static float atanhf(float x) => (float)atanh(x);

    // --------------- cbrt (after fdlibm s_cbrt.c) ---------------

    [RuntimeExport("cbrt")]
    internal static double cbrt(double x)
    {
        const int B1 = 715094163; /* B1 = (682-0.03306235651)*2**20 */
        const int B2 = 696219795; /* B2 = (664-0.03306235651)*2**20 */
        const double C = 5.42857142857142815906e-01; /* 19/35 */
        const double D = -7.05306122448979611050e-01; /* -864/1225 */
        const double E = 1.41428571428571436819e+00; /* 99/70 */
        const double F = 1.60714285714285720630e+00; /* 45/28 */
        const double G = 3.57142857142857150787e-01; /* 5/14 */

        int hx = HighWord(x);
        int sign = hx & unchecked((int)0x80000000);
        hx ^= sign;

        if (hx >= 0x7ff00000)
        {
            return x + x; /* cbrt(NaN, INF) is itself */
        }

        if ((hx | LowWord(x)) == 0)
        {
            return x; /* cbrt(0) is itself */
        }

        x = SetHighWord(x, hx); /* x <- |x| */

        /* rough cbrt to 5 bits */
        double t;
        if (hx < 0x00100000) /* subnormal */
        {
            t = SetHighWord(0.0, 0x43500000); /* t = 2^54 */
            t *= x;
            t = SetHighWord(t, HighWord(t) / 3 + B2);
        }
        else
        {
            t = SetHighWord(0.0, hx / 3 + B1);
        }

        /* new cbrt to 23 bits */
        double r = t * t / x;
        double s = C + r * t;
        t *= G + F / (s + E + D / s);

        /* chop to 20 bits and make it larger than cbrt(x) */
        t = SetLowWord(t, 0);
        t = SetHighWord(t, HighWord(t) + 1);

        /* one Newton step to 53 bits, error under 0.667 ulps */
        s = t * t;
        r = x / s;
        double w = t + t;
        r = (r - t) / (w + r);
        t = t + t * r;

        return SetHighWord(t, HighWord(t) | sign); /* restore the sign */
    }

    [RuntimeExport("cbrtf")]
    internal static float cbrtf(float x) => (float)cbrt(x);
}
