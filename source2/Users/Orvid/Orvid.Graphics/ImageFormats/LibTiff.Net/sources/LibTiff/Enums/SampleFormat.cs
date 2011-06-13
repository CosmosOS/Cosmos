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
    /// Data sample format.<br/>
    /// Possible values for <see cref="TiffTag"/>.SAMPLEFORMAT tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum SampleFormat
    {
        /// <summary>
        /// Unsigned integer data
        /// </summary>
        UINT = 1,
        
        /// <summary>
        /// Signed integer data
        /// </summary>
        INT = 2,
        
        /// <summary>
        /// IEEE floating point data
        /// </summary>
        IEEEFP = 3,
        
        /// <summary>
        /// Untyped data
        /// </summary>
        VOID = 4,
        
        /// <summary>
        /// Complex signed int
        /// </summary>
        COMPLEXINT = 5,
        
        /// <summary>
        /// Complex ieee floating
        /// </summary>
        COMPLEXIEEEFP = 6,
    }
}
