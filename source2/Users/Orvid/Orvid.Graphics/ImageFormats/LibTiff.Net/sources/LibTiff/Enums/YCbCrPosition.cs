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
    /// Subsample positioning.<br/>
    /// Possible values for <see cref="TiffTag"/>.YCBCRPOSITIONING tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum YCbCrPosition
    {
        /// <summary>
        /// As in PostScript Level 2
        /// </summary>
        CENTERED = 1,

        /// <summary>
        /// As in CCIR 601-1
        /// </summary>
        COSITED = 2,
    }
}
