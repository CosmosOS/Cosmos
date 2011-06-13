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
    /// Jpeg Tables Mode.<br/>
    /// Possible values for <see cref="TiffTag"/>.JPEGTABLESMODE tag.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    enum JpegTablesMode
    {
        /// <summary>
        /// None.
        /// </summary>
        NONE = 0,
        
        /// <summary>
        /// Include quantization tables.
        /// </summary>
        QUANT = 0x0001,
        
        /// <summary>
        /// Include Huffman tables.
        /// </summary>
        HUFF = 0x0002,
    }
}
