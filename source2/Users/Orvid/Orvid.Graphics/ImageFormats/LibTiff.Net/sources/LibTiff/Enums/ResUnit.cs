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
    /// Units of resolutions.<br/>
    /// Possible values for <see cref="TiffTag"/>.RESOLUTIONUNIT tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum ResUnit
    {
        /// <summary>
        /// No meaningful units.
        /// </summary>
        NONE = 1,
        
        /// <summary>
        /// English.
        /// </summary>
        INCH = 2,
        
        /// <summary>
        /// Metric.
        /// </summary>
        CENTIMETER = 3,
    }
}
