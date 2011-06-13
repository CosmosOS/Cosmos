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

namespace BitMiracle.LibTiff.Classic.Internal
{
    struct TiffHeader
    {
        public const int TIFF_MAGIC_SIZE = 2;
        public const int TIFF_VERSION_SIZE = 2;
        public const int TIFF_DIROFFSET_SIZE = 4;

        public const int SizeInBytes = TIFF_MAGIC_SIZE + TIFF_VERSION_SIZE + TIFF_DIROFFSET_SIZE;

        /// <summary>
        /// magic number (defines byte order)
        /// </summary>
        public short tiff_magic;

        /// <summary>
        /// TIFF version number
        /// </summary>
        public short tiff_version;

        /// <summary>
        /// byte offset to first directory
        /// </summary>
        public uint tiff_diroff;
    }
}
