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
    /// Options for CCITT Group 3/4 fax encoding.<br/>
    /// Possible values for <see cref="TiffTag"/>.GROUP3OPTIONS / TiffTag.T4OPTIONS and
    /// TiffTag.GROUP4OPTIONS / TiffTag.T6OPTIONS tags.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum Group3Opt
    {
        /// <summary>
        /// Unknown (uninitialized).
        /// </summary>
        UNKNOWN = -1,
        
        /// <summary>
        /// 2-dimensional coding.
        /// </summary>
        ENCODING2D = 0x1,
        
        /// <summary>
        /// Data not compressed.
        /// </summary>
        UNCOMPRESSED = 0x2,
        
        /// <summary>
        /// Fill to byte boundary.
        /// </summary>
        FILLBITS = 0x4,
    }
}
