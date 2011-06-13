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
    /// Prediction scheme w/ LZW.<br/>
    /// Possible values for <see cref="TiffTag"/>.PREDICTOR tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum Predictor
    {
        /// <summary>
        /// No prediction scheme used.
        /// </summary>
        NONE = 1,
        
        /// <summary>
        /// Horizontal differencing.
        /// </summary>
        HORIZONTAL = 2,
        
        /// <summary>
        /// Floating point predictor.
        /// </summary>
        FLOATINGPOINT = 3,
    }
}
