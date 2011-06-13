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

/*
 * Builtin Compression Scheme Configuration Support.
 */

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        /// <summary>
        /// Compression schemes statically built into the library.
        /// </summary>
        private void setupBuiltInCodecs()
        {
            // change initial syntax of m_builtInCodecs, maintains easier.
            // San Chen <bigsan.chen@gmail.com>

            m_builtInCodecs = new TiffCodec[]
            {
                new TiffCodec(this, (Compression)(-1), "Not configured"),
                new DumpModeCodec(this, Compression.NONE, "None"),
                new LZWCodec(this, Compression.LZW, "LZW"),
                new PackBitsCodec(this, Compression.PACKBITS, "PackBits"),
                new TiffCodec(this, Compression.THUNDERSCAN, "ThunderScan"),
                new TiffCodec(this, Compression.NEXT, "NeXT"),
                new JpegCodec(this, Compression.JPEG, "JPEG"),
                new OJpegCodec(this, Compression.OJPEG, "Old-style JPEG"),
                new CCITTCodec(this, Compression.CCITTRLE, "CCITT RLE"),
                new CCITTCodec(this, Compression.CCITTRLEW, "CCITT RLE/W"),
                new CCITTCodec(this, Compression.CCITTFAX3, "CCITT Group 3"),
                new CCITTCodec(this, Compression.CCITTFAX4, "CCITT Group 4"),
                new TiffCodec(this, Compression.JBIG, "ISO JBIG"),
                new DeflateCodec(this, Compression.DEFLATE, "Deflate"),
                new DeflateCodec(this, Compression.ADOBE_DEFLATE, "AdobeDeflate"),
                new TiffCodec(this, Compression.PIXARLOG, "PixarLog"),
                new TiffCodec(this, Compression.SGILOG, "SGILog"),
                new TiffCodec(this, Compression.SGILOG24, "SGILog24"),
                null,
            };
        }
    }
}
