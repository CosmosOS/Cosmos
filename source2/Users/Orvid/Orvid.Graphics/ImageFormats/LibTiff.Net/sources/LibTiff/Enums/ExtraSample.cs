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
    /// Information about extra samples.<br/>
    /// Possible values for <see cref="TiffTag"/>.EXTRASAMPLES tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum ExtraSample
    {
        /// <summary>
        /// Unspecified data.
        /// </summary>
        UNSPECIFIED = 0,
        
        /// <summary>
        /// Associated alpha data.
        /// </summary>
        ASSOCALPHA = 1,
        
        /// <summary>
        /// Unassociated alpha data.
        /// </summary>
        UNASSALPHA = 2,
    }
}
