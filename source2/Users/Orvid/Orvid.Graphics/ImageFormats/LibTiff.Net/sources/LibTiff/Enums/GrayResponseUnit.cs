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
    /// Gray scale curve accuracy.<br/>
    /// Possible values for <see cref="TiffTag"/>.GRAYRESPONSEUNIT tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum GrayResponseUnit
    {
        /// <summary>
        /// Tenths of a unit.
        /// </summary>
        GRU10S = 1,
        
        /// <summary>
        /// Hundredths of a unit.
        /// </summary>
        GRU100S = 2,
        
        /// <summary>
        /// Thousandths of a unit.
        /// </summary>
        GRU1000S = 3,
        
        /// <summary>
        /// Ten-thousandths of a unit.
        /// </summary>
        GRU10000S = 4,
        
        /// <summary>
        /// Hundred-thousandths.
        /// </summary>
        GRU100000S = 5,
    }
}
