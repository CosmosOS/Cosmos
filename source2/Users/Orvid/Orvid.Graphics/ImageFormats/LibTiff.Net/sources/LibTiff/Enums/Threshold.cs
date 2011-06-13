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
    /// Thresholding used on data.<br/>
    /// Possible values for <see cref="TiffTag"/>.THRESHHOLDING tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum Threshold
    {
        /// <summary>
        /// B&amp;W art scan.
        /// </summary>
        BILEVEL = 1,
        
        /// <summary>
        /// Dithered scan.
        /// </summary>
        HALFTONE = 2,
        
        /// <summary>
        /// Usually Floyd-Steinberg.
        /// </summary>
        ERRORDIFFUSE = 3,
    }
}
