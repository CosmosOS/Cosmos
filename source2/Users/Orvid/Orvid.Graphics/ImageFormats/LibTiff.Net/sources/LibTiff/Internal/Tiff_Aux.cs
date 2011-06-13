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
 * Auxiliary Support Routines.
 */

using System;
using System.Globalization;
using System.IO;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private static bool defaultTransferFunction(TiffDirectory td)
        {
            short[][] tf = td.td_transferfunction;
            tf[0] = null;
            tf[1] = null;
            tf[2] = null;

            if (td.td_bitspersample >= sizeof(int) * 8 - 2)
                return false;

            int n = 1 << td.td_bitspersample;
            tf[0] = new short [n];
            tf[0][0] = 0;
            for (int i = 1; i < n; i++)
            {
                double t = (double)i / ((double)n - 1.0);
                tf[0][i] = (short)Math.Floor(65535.0 * Math.Pow(t, 2.2) + 0.5);
            }

            if (td.td_samplesperpixel - td.td_extrasamples > 1)
            {
                tf[1] = new short [n];
                Buffer.BlockCopy(tf[0], 0, tf[1], 0, tf[0].Length * sizeof(short));

                tf[2] = new short [n];
                Buffer.BlockCopy(tf[0], 0, tf[2], 0, tf[0].Length * sizeof(short));
            }

            return true;
        }

        private static void defaultRefBlackWhite(TiffDirectory td)
        {
            td.td_refblackwhite = new float[6];
            if (td.td_photometric == Photometric.YCBCR)
            {
                // YCbCr (Class Y) images must have the ReferenceBlackWhite tag set. Fix the
                // broken images, which lacks that tag.
                td.td_refblackwhite[0] = 0.0F;
                td.td_refblackwhite[1] = td.td_refblackwhite[3] = td.td_refblackwhite[5] = 255.0F;
                td.td_refblackwhite[2] = td.td_refblackwhite[4] = 128.0F;
            }
            else
            {
                // Assume RGB (Class R)
                for (int i = 0; i < 3; i++)
                {
                    td.td_refblackwhite[2 * i + 0] = 0;
                    td.td_refblackwhite[2 * i + 1] = (float)((1L << td.td_bitspersample) - 1L);
                }
            }
        }

        internal static int readInt(byte[] buffer, int offset)
        {
            int value = buffer[offset++] & 0xFF;
            value += (buffer[offset++] & 0xFF) << 8;
            value += (buffer[offset++] & 0xFF) << 16;
            value += buffer[offset++] << 24;
            return value;
        }

        internal static void writeInt(int value, byte[] buffer, int offset)
        {
            buffer[offset++] = (byte)value;
            buffer[offset++] = (byte)(value >> 8);
            buffer[offset++] = (byte)(value >> 16);
            buffer[offset++] = (byte)(value >> 24);
        }

        internal static short readShort(byte[] buffer, int offset)
        {
            short value = (short)(buffer[offset] & 0xFF);
            value += (short)((buffer[offset + 1] & 0xFF) << 8);
            return value;
        }

        internal static void fprintf(Stream fd, string format, params object[] list)
        {
            string s = string.Format(CultureInfo.InvariantCulture, format, list);
            byte[] bytes = Latin1Encoding.GetBytes(s);
            fd.Write(bytes, 0, bytes.Length);
        }

        private static string encodeOctalString(byte value)
        {
            //convert to int, for cleaner syntax below. 
            int x = value;

            //return octal encoding \ddd of the character value. 
            return string.Format(CultureInfo.InvariantCulture, @"\{0}{1}{2}", (x >> 6) & 7, (x >> 3) & 7, x & 7);
        }
    }
}
