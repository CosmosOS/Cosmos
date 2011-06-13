/* Copyright (C) 2008-2011, Bit Miracle
 * http://www.bitmiracle.com
 * 
 * This software is based in part on the work of the Sam Leffler, Silicon 
 * Graphics, Inc. and contributors.
 *
 * Copyright (c) 1988-1997 Sam Leffler
 * Copyright (c) 1991-1997 Silicon Graphics, Inc.
 * For conditions of distribution and use, see the accompanying README file.
 */

using System;

namespace BitMiracle.LibTiff.Classic.Internal
{
    /// <summary>
    /// Convert color value from the YCbCr space to CIE XYZ.
    /// The colorspace conversion algorithm comes from the IJG v5a code;
    /// see below for more information on how it works.
    /// </summary>
    class TiffYCbCrToRGB
    {
        private const int clamptabOffset = 256;
        private const int SHIFT = 16;
        private const int ONE_HALF = 1 << (SHIFT - 1);

        /// <summary>
        /// range clamping table
        /// </summary>
        private byte[] clamptab;

        private int[] Cr_r_tab;
        private int[] Cb_b_tab;
        private int[] Cr_g_tab;
        private int[] Cb_g_tab;
        private int[] Y_tab;

        public TiffYCbCrToRGB()
        {
            clamptab = new byte[4 * 256];
            Cr_r_tab = new int[256];
            Cb_b_tab = new int[256];
            Cr_g_tab = new int[256];
            Cb_g_tab = new int[256];
            Y_tab = new int[256];
        }

        /*
        * Initialize the YCbCr->RGB conversion tables.  The conversion
        * is done according to the 6.0 spec:
        *
        *    R = Y + Cr * (2 - 2 * LumaRed)
        *    B = Y + Cb * (2 - 2 * LumaBlue)
        *    G =   Y
        *        - LumaBlue * Cb * (2 - 2 * LumaBlue) / LumaGreen
        *        - LumaRed * Cr * (2 - 2 * LumaRed) / LumaGreen
        *
        * To avoid floating point arithmetic the fractional constants that
        * come out of the equations are represented as fixed point values
        * in the range 0...2^16.  We also eliminate multiplications by
        * pre-calculating possible values indexed by Cb and Cr (this code
        * assumes conversion is being done for 8-bit samples).
        */
        public void Init(float[] luma, float[] refBlackWhite)
        {
            Array.Clear(clamptab, 0, 256); /* v < 0 => 0 */

            for (int i = 0; i < 256; i++)
                clamptab[clamptabOffset + i] = (byte)i;

            int start = clamptabOffset + 256;
            int stop = start + 2 * 256;

            for (int i = start; i < stop; i++)
                clamptab[i] = 255; /* v > 255 => 255 */

            float LumaRed = luma[0];
            float LumaGreen = luma[1];
            float LumaBlue = luma[2];

            float f1 = 2 - 2 * LumaRed;
            int D1 = fix(f1);

            float f2 = LumaRed * f1 / LumaGreen;
            int D2 = -fix(f2);

            float f3 = 2 - 2 * LumaBlue;
            int D3 = fix(f3);

            float f4 = LumaBlue * f3 / LumaGreen;
            int D4 = -fix(f4);

            /*
            * i is the actual input pixel value in the range 0..255
            * Cb and Cr values are in the range -128..127 (actually
            * they are in a range defined by the ReferenceBlackWhite
            * tag) so there is some range shifting to do here when
            * constructing tables indexed by the raw pixel data.
            */
            for (int i = 0, x = -128; i < 256; i++, x++)
            {
                int Cr = code2V(x, refBlackWhite[4] - 128.0F, refBlackWhite[5] - 128.0F, 127);
                int Cb = code2V(x, refBlackWhite[2] - 128.0F, refBlackWhite[3] - 128.0F, 127);

                Cr_r_tab[i] = (D1 * Cr + ONE_HALF) >> SHIFT;
                Cb_b_tab[i] = (D3 * Cb + ONE_HALF) >> SHIFT;
                Cr_g_tab[i] = D2 * Cr;
                Cb_g_tab[i] = D4 * Cb + ONE_HALF;
                Y_tab[i] = code2V(x + 128, refBlackWhite[0], refBlackWhite[1], 255);
            }
        }

        public void YCbCrtoRGB(int Y, int Cb, int Cr, out int r, out int g, out int b)
        {
            /* XXX: Only 8-bit YCbCr input supported for now */
            Y = hiClamp(Y, 255);
            Cb = clamp(Cb, 0, 255);
            Cr = clamp(Cr, 0, 255);

            r = clamptab[clamptabOffset + Y_tab[Y] + Cr_r_tab[Cr]];
            g = clamptab[clamptabOffset + Y_tab[Y] + ((Cb_g_tab[Cb] + Cr_g_tab[Cr]) >> SHIFT)];
            b = clamptab[clamptabOffset + Y_tab[Y] + Cb_b_tab[Cb]];
        }

        private static int fix(float x)
        {
            return (int)(x * (1L << SHIFT) + 0.5);
        }

        private static int code2V(int c, float RB, float RW, float CR)
        {
            return (int)(((c - (int)RB) * CR) / ((int)(RW - RB) != 0 ? (RW - RB) : 1.0f));
        }

        private static int clamp(int f, int min, int max)
        {
            return (f < min ? min : f > max ? max : f);
        }

        private static int hiClamp(int f, int max)
        {
            return (f > max ? max : f);
        }
    }
}
