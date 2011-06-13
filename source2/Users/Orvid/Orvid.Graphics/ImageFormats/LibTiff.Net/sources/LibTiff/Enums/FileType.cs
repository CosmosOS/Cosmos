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
    /// Subfile data descriptor.<br/>
    /// Possible values for <see cref="TiffTag"/>.SUBFILETYPE tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum FileType
    {
        /// <summary>
        /// Reduced resolution version.
        /// </summary>
        REDUCEDIMAGE = 0x1,
        
        /// <summary>
        /// One page of many.
        /// </summary>
        PAGE = 0x2,
        
        /// <summary>
        /// Transparency mask.
        /// </summary>
        MASK = 0x4
    }
}
