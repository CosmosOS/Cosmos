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
    /// Auto RGB&lt;=&gt;YCbCr convert.<br/>
    /// Possible values for <see cref="TiffTag"/>.JPEGCOLORMODE tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum JpegColorMode
    {
        /// <summary>
        /// No conversion (default).
        /// </summary>
        RAW = 0x0000,
        
        /// <summary>
        /// Do auto conversion.
        /// </summary>
        RGB = 0x0001,
    }
}
