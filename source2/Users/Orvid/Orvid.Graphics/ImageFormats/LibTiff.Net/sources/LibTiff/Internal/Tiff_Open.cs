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

using System.IO;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private static readonly uint[] typemask = 
        {
            0,              // TIFF_NOTYPE
            0x000000ff,     // TIFF_BYTE
            0xffffffff,     // TIFF_ASCII
            0x0000ffff,     // TIFF_SHORT
            0xffffffff,     // TIFF_LONG
            0xffffffff,     // TIFF_RATIONAL
            0x000000ff,     // TIFF_SBYTE
            0x000000ff,     // TIFF_UNDEFINED
            0x0000ffff,     // TIFF_SSHORT
            0xffffffff,     // TIFF_SLONG
            0xffffffff,     // TIFF_SRATIONAL
            0xffffffff,     // TIFF_FLOAT
            0xffffffff,     // TIFF_DOUBLE
        };

        private static readonly int[] bigTypeshift = 
        {
            0,      // TIFF_NOTYPE
            24,     // TIFF_BYTE
            0,      // TIFF_ASCII
            16,     // TIFF_SHORT
            0,      // TIFF_LONG
            0,      // TIFF_RATIONAL
            24,     // TIFF_SBYTE
            24,     // TIFF_UNDEFINED
            16,     // TIFF_SSHORT
            0,      // TIFF_SLONG
            0,      // TIFF_SRATIONAL
            0,      // TIFF_FLOAT
            0,      // TIFF_DOUBLE
        };

        private static readonly int[] litTypeshift = 
        {
            0,  // TIFF_NOTYPE
            0,  // TIFF_BYTE
            0,  // TIFF_ASCII
            0,  // TIFF_SHORT
            0,  // TIFF_LONG
            0,  // TIFF_RATIONAL
            0,  // TIFF_SBYTE
            0,  // TIFF_UNDEFINED
            0,  // TIFF_SSHORT
            0,  // TIFF_SLONG
            0,  // TIFF_SRATIONAL
            0,  // TIFF_FLOAT
            0,  // TIFF_DOUBLE
        };

        /*
        * Initialize the shift & mask tables, and the
        * byte swapping state according to the file
        * contents and the machine architecture.
        */
        private void initOrder(int magic)
        {
            m_typemask = typemask;
            if (magic == TIFF_BIGENDIAN)
            {
                m_typeshift = bigTypeshift;
                m_flags |= TiffFlags.SWAB;
            }
            else
            {
                m_typeshift = litTypeshift;
            }
        }

        private static int getMode(string mode, string module, out FileMode m, out FileAccess a)
        {
            m = 0;
            a = 0;
            int tiffMode = -1;

            if (mode.Length == 0)
                return tiffMode;

            switch (mode[0])
            {
                case 'r':
                    m = FileMode.Open;
                    a = FileAccess.Read;
                    tiffMode = O_RDONLY;
                    if (mode.Length > 1 && mode[1] == '+')
                    {
                        a = FileAccess.ReadWrite;
                        tiffMode = O_RDWR;
                    }
                    break;

                case 'w':
                    m = FileMode.Create;
                    a = FileAccess.ReadWrite;
                    tiffMode = O_RDWR | O_CREAT | O_TRUNC;
                    break;

                case 'a':
                    m = FileMode.Open;
                    a = FileAccess.ReadWrite;
                    tiffMode = O_RDWR | O_CREAT;
                    break;

                default:
                    ErrorExt(null, 0, module, "\"{0}\": Bad mode", mode);
                    break;
            }

            return tiffMode;
        }
    }
}
