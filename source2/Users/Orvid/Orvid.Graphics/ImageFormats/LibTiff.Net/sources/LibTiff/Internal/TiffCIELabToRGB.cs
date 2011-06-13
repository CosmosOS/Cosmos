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

/*
* CIE L*a*b* to CIE XYZ and CIE XYZ to RGB conversion routines are taken
* from the VIPS library (http://www.vips.ecs.soton.ac.uk) with
* the permission of John Cupitt, the VIPS author.
*/

using System;

namespace BitMiracle.LibTiff.Classic.Internal
{
    /// <summary>
    /// CIE Lab 1976->RGB support
    /// </summary>
    class TiffCIELabToRGB
    {
        public const int CIELABTORGB_TABLE_RANGE = 1500;

        /// <summary>
        /// Size of conversion table
        /// </summary>
        private int range;

        private float rstep;
        private float gstep;
        private float bstep;

        // Reference white point
        private float X0;
        private float Y0;
        private float Z0;

        private TiffDisplay display;

        /// <summary>
        /// Conversion of Yr to r
        /// </summary>
        private float[] Yr2r = new float[CIELABTORGB_TABLE_RANGE + 1];

        /// <summary>
        /// Conversion of Yg to g
        /// </summary>
        private float[] Yg2g = new float[CIELABTORGB_TABLE_RANGE + 1];

        /// <summary>
        /// Conversion of Yb to b
        /// </summary>
        private float[] Yb2b = new float[CIELABTORGB_TABLE_RANGE + 1];

        /* 
        * Allocate conversion state structures and make look_up tables for
        * the Yr,Yb,Yg <=> r,g,b conversions.
        */
        public void Init(TiffDisplay refDisplay, float[] refWhite)
        {
            range = CIELABTORGB_TABLE_RANGE;

            display = refDisplay;

            /* Red */
            double gamma = 1.0 / display.d_gammaR;
            rstep = (display.d_YCR - display.d_Y0R) / range;
            for (int i = 0; i <= range; i++)
            {
                Yr2r[i] = display.d_Vrwr * ((float)Math.Pow((double)i / range, gamma));
            }

            /* Green */
            gamma = 1.0 / display.d_gammaG;
            gstep = (display.d_YCR - display.d_Y0R) / range;
            for (int i = 0; i <= range; i++)
            {
                Yg2g[i] = display.d_Vrwg * ((float)Math.Pow((double)i / range, gamma));
            }

            /* Blue */
            gamma = 1.0 / display.d_gammaB;
            bstep = (display.d_YCR - display.d_Y0R) / range;
            for (int i = 0; i <= range; i++)
            {
                Yb2b[i] = display.d_Vrwb * ((float)Math.Pow((double)i / range, gamma));
            }

            /* Init reference white point */
            X0 = refWhite[0];
            Y0 = refWhite[1];
            Z0 = refWhite[2];
        }

        /*
        * Convert color value from the CIE L*a*b* 1976 space to CIE XYZ.
        */
        public void CIELabToXYZ(int l, int a, int b, out float X, out float Y, out float Z)
        {
            float L = (float)l * 100.0F / 255.0F;
            float cby;

            if (L < 8.856F)
            {
                Y = (L * Y0) / 903.292F;
                cby = 7.787F * (Y / Y0) + 16.0F / 116.0F;
            }
            else
            {
                cby = (L + 16.0F) / 116.0F;
                Y = Y0 * cby * cby * cby;
            }

            float tmp = (float)a / 500.0F + cby;
            if (tmp < 0.2069F)
                X = X0 * (tmp - 0.13793F) / 7.787F;
            else
                X = X0 * tmp * tmp * tmp;

            tmp = cby - (float)b / 200.0F;
            if (tmp < 0.2069F)
                Z = Z0 * (tmp - 0.13793F) / 7.787F;
            else
                Z = Z0 * tmp * tmp * tmp;
        }

        /*
        * Convert color value from the XYZ space to RGB.
        */
        public void XYZToRGB(float X, float Y, float Z, out int r, out int g, out int b)
        {
            /* Multiply through the matrix to get luminosity values. */
            float Yr = display.d_mat[0][0] * X + display.d_mat[0][1] * Y + display.d_mat[0][2] * Z;
            float Yg = display.d_mat[1][0] * X + display.d_mat[1][1] * Y + display.d_mat[1][2] * Z;
            float Yb = display.d_mat[2][0] * X + display.d_mat[2][1] * Y + display.d_mat[2][2] * Z;

            /* Clip input */
            Yr = Math.Max(Yr, display.d_Y0R);
            Yg = Math.Max(Yg, display.d_Y0G);
            Yb = Math.Max(Yb, display.d_Y0B);

            /* Avoid overflow in case of wrong input values */
            Yr = Math.Min(Yr, display.d_YCR);
            Yg = Math.Min(Yg, display.d_YCG);
            Yb = Math.Min(Yb, display.d_YCB);

            /* Turn luminosity to color value. */
            int i = (int)((Yr - display.d_Y0R) / rstep);
            i = Math.Min(range, i);
            r = rInt(Yr2r[i]);

            i = (int)((Yg - display.d_Y0G) / gstep);
            i = Math.Min(range, i);
            g = rInt(Yg2g[i]);

            i = (int)((Yb - display.d_Y0B) / bstep);
            i = Math.Min(range, i);
            b = rInt(Yb2b[i]);

            /* Clip output. */
            r = Math.Min(r, display.d_Vrwr);
            g = Math.Min(g, display.d_Vrwg);
            b = Math.Min(b, display.d_Vrwb);
        }

        private static int rInt(float R)
        {
            return (int)(R > 0 ? (R + 0.5) : (R - 0.5));
        }
    }
}
