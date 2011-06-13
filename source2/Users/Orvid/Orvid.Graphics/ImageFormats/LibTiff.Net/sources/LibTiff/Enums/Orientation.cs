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
    /// Image orientation.<br/>
    /// Possible values for <see cref="TiffTag"/>.ORIENTATION tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum Orientation
    {
        /// <summary>
        /// Row 0 top, Column 0 lhs.
        /// </summary>
        TOPLEFT = 1,
        
        /// <summary>
        /// Row 0 top, Column 0 rhs.
        /// </summary>
        TOPRIGHT = 2,

        /// <summary>
        /// Row 0 bottom, Column 0 rhs.
        /// </summary>
        BOTRIGHT = 3,
        
        /// <summary>
        /// Row 0 bottom, Column 0 lhs.
        /// </summary>
        BOTLEFT = 4,
        
        /// <summary>
        /// Row 0 lhs, Column 0 top.
        /// </summary>
        LEFTTOP = 5,
        
        /// <summary>
        /// Row 0 rhs, Column 0 top.
        /// </summary>
        RIGHTTOP = 6,
        
        /// <summary>
        /// Row 0 rhs, Column 0 bottom.
        /// </summary>
        RIGHTBOT = 7,
        
        /// <summary>
        /// Row 0 lhs, Column 0 bottom.
        /// </summary>
        LEFTBOT = 8,
    }
}
