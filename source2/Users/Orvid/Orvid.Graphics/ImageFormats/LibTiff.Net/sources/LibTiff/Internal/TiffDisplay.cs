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

namespace BitMiracle.LibTiff.Classic.Internal
{
    /// <summary>
    /// Structure for holding information about a display device.
    /// </summary>
    class TiffDisplay
    {
        /// <summary>
        /// XYZ -> luminance matrix
        /// </summary>
        internal float[][] d_mat;

        // Light o/p for reference white
        internal float d_YCR;
        internal float d_YCG;
        internal float d_YCB;

        // Pixel values for ref. white
        internal int d_Vrwr;
        internal int d_Vrwg;
        internal int d_Vrwb;

        // Residual light for black pixel
        internal float d_Y0R;
        internal float d_Y0G;
        internal float d_Y0B;

        // Gamma values for the three guns
        internal float d_gammaR;
        internal float d_gammaG;
        internal float d_gammaB;

        public TiffDisplay()
        {
        }

        public TiffDisplay(float[] mat0, float[] mat1, float[] mat2,
            float YCR, float YCG, float YCB, int Vrwr, int Vrwg,
            int Vrwb, float Y0R, float Y0G, float Y0B,
            float gammaR, float gammaG, float gammaB)
        {
            d_mat = new float[3][] { mat0, mat1, mat2 };
            d_YCR = YCR;
            d_YCG = YCG;
            d_YCB = YCB;
            d_Vrwr = Vrwr;
            d_Vrwg = Vrwg;
            d_Vrwb = Vrwb;
            d_Y0R = Y0R;
            d_Y0G = Y0G;
            d_Y0B = Y0B;
            d_gammaR = gammaR;
            d_gammaG = gammaG;
            d_gammaB = gammaB;
        }
    }
}
