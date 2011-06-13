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

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Compression scheme.<br/>
    /// Possible values for <see cref="TiffTag"/>.COMPRESSION tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum Compression
    {
        /// <summary>
        /// Dump mode.
        /// </summary>
        NONE = 1,

        /// <summary>
        /// CCITT modified Huffman RLE.
        /// </summary>
        CCITTRLE = 2,
        
        /// <summary>
        /// CCITT Group 3 fax encoding.
        /// </summary>
        CCITTFAX3 = 3,
        
        /// <summary>
        /// CCITT T.4 (TIFF 6 name for CCITT Group 3 fax encoding).
        /// </summary>
        CCITT_T4 = 3,
        
        /// <summary>
        /// CCITT Group 4 fax encoding.
        /// </summary>
        CCITTFAX4 = 4,
        
        /// <summary>
        /// CCITT T.6 (TIFF 6 name for CCITT Group 4 fax encoding).
        /// </summary>
        CCITT_T6 = 4,
        
        /// <summary>
        /// Lempel-Ziv &amp; Welch.
        /// </summary>
        LZW = 5,
        
        /// <summary>
        /// Original JPEG / Old-style JPEG (6.0).
        /// </summary>
        OJPEG = 6,
        
        /// <summary>
        /// JPEG DCT compression. Introduced post TIFF rev 6.0.
        /// </summary>
        JPEG = 7,
        
        /// <summary>
        /// NeXT 2-bit RLE.
        /// </summary>
        NEXT = 32766,
        
        /// <summary>
        /// CCITT RLE.
        /// </summary>
        CCITTRLEW = 32771,
        
        /// <summary>
        /// Macintosh RLE.
        /// </summary>
        PACKBITS = 32773,
        
        /// <summary>
        /// ThunderScan RLE.
        /// </summary>
        THUNDERSCAN = 32809,
        
        /// <summary>
        /// IT8 CT w/padding. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8CTPAD = 32895,
        
        /// <summary>
        /// IT8 Linework RLE. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8LW = 32896,
        
        /// <summary>
        /// IT8 Monochrome picture. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8MP = 32897,
        
        /// <summary>
        /// IT8 Binary line art. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8BL = 32898,
        
        /// <summary>
        /// Pixar companded 10bit LZW. Reserved for Pixar.
        /// </summary>
        PIXARFILM = 32908,
        
        /// <summary>
        /// Pixar companded 11bit ZIP. Reserved for Pixar.
        /// </summary>
        PIXARLOG = 32909,
        
        /// <summary>
        /// Deflate compression.
        /// </summary>
        DEFLATE = 32946,
        
        /// <summary>
        /// Deflate compression, as recognized by Adobe.
        /// </summary>
        ADOBE_DEFLATE = 8,
        
        /// <summary>
        /// Kodak DCS encoding.
        /// Reserved for Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>).
        /// </summary>
        DCS = 32947,
        
        /// <summary>
        /// ISO JBIG.
        /// </summary>
        JBIG = 34661,
        
        /// <summary>
        /// SGI Log Luminance RLE.
        /// </summary>
        SGILOG = 34676,
        
        /// <summary>
        /// SGI Log 24-bit packed.
        /// </summary>
        SGILOG24 = 34677,
        
        /// <summary>
        /// Leadtools JPEG2000.
        /// </summary>
        JP2000 = 34712,
    }
}
