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
    /// Tag data type.
    /// </summary>
    /// <remarks>Note: RATIONALs are the ratio of two 32-bit integer values.</remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    enum TiffType : short
    {
        /// <summary>
        /// Placeholder.
        /// </summary>
        NOTYPE = 0,

        /// <summary>
        /// For field descriptor searching.
        /// </summary>
        ANY = NOTYPE,
        
        /// <summary>
        /// 8-bit unsigned integer.
        /// </summary>
        BYTE = 1,
        
        /// <summary>
        /// 8-bit bytes with last byte <c>null</c>.
        /// </summary>
        ASCII = 2,
        
        /// <summary>
        /// 16-bit unsigned integer.
        /// </summary>
        SHORT = 3,
        
        /// <summary>
        /// 32-bit unsigned integer.
        /// </summary>
        LONG = 4,
        
        /// <summary>
        /// 64-bit unsigned fraction.
        /// </summary>
        RATIONAL = 5,
        
        /// <summary>
        /// 8-bit signed integer.
        /// </summary>
        SBYTE = 6,
        
        /// <summary>
        /// 8-bit untyped data.
        /// </summary>
        UNDEFINED = 7,
        
        /// <summary>
        /// 16-bit signed integer.
        /// </summary>
        SSHORT = 8,
        
        /// <summary>
        /// 32-bit signed integer.
        /// </summary>
        SLONG = 9,
        
        /// <summary>
        /// 64-bit signed fraction.
        /// </summary>
        SRATIONAL = 10,
        
        /// <summary>
        /// 32-bit IEEE floating point.
        /// </summary>
        FLOAT = 11,
        
        /// <summary>
        /// 64-bit IEEE floating point.
        /// </summary>
        DOUBLE = 12,
        
        /// <summary>
        /// 32-bit unsigned integer (offset)
        /// </summary>
        IFD = 13
    }
}
