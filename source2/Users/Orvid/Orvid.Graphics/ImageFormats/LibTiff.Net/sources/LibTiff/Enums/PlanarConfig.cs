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
    /// Storage organization.<br/>
    /// Possible values for <see cref="TiffTag"/>.PLANARCONFIG tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum PlanarConfig
    {
        /// <summary>
        /// Unknown (uninitialized).
        /// </summary>
        UNKNOWN = 0,
        
        /// <summary>
        /// Single image plane.
        /// </summary>
        CONTIG = 1,
        
        /// <summary>
        /// Separate planes of data.
        /// </summary>
        SEPARATE = 2
    }
}
