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
 * Strip-organized Image Support Routines.
 */

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private int summarize(int summand1, int summand2, string where)
        {
            int bytes = summand1 + summand2;
            if (bytes - summand1 != summand2)
            {
                ErrorExt(this, m_clientdata, m_name, "Integer overflow in {0}", where);
                bytes = 0;
            }

            return bytes;
        }

        private int multiply(int nmemb, int elem_size, string where)
        {
            int bytes = nmemb * elem_size;
            if (elem_size != 0 && bytes / elem_size != nmemb)
            {
                ErrorExt(this, m_clientdata, m_name, "Integer overflow in {0}", where);
                bytes = 0;
            }

            return bytes;
        }

        /*
        * Return the number of bytes to read/write in a call to
        * one of the scanline-oriented i/o routines.  Note that
        * this number may be 1/samples-per-pixel if data is
        * stored as separate planes.
        * The ScanlineSize in case of YCbCrSubsampling is defined as the
        * strip size divided by the strip height, i.e. the size of a pack of vertical
        * subsampling lines divided by vertical subsampling. It should thus make
        * sense when multiplied by a multiple of vertical subsampling.
        * Some stuff depends on this newer version of TIFFScanlineSize
        * TODO: resolve this
        */
        internal int newScanlineSize()
        {
            int scanline;
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
            {
                if (m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
                {
                    FieldValue[] result = GetField(TiffTag.YCBCRSUBSAMPLING);
                    ushort ycbcrsubsampling0 = result[0].ToUShort();
                    ushort ycbcrsubsampling1 = result[1].ToUShort();

                    if (ycbcrsubsampling0 * ycbcrsubsampling1 == 0)
                    {
                        ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
                        return 0;
                    }

                    return ((((m_dir.td_imagewidth + ycbcrsubsampling0 - 1) / ycbcrsubsampling0) * (ycbcrsubsampling0 * ycbcrsubsampling1 + 2) * m_dir.td_bitspersample + 7) / 8) / ycbcrsubsampling1;
                }
                else
                {
                    scanline = multiply(m_dir.td_imagewidth, m_dir.td_samplesperpixel, "TIFFScanlineSize");
                }
            }
            else
            {
                scanline = m_dir.td_imagewidth;
            }

            return howMany8(multiply(scanline, m_dir.td_bitspersample, "TIFFScanlineSize"));
        }

        /*
        * Some stuff depends on this older version of TIFFScanlineSize
        * TODO: resolve this
        */
        internal int oldScanlineSize()
        {
            int scanline = multiply(m_dir.td_bitspersample, m_dir.td_imagewidth, "TIFFScanlineSize");
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
                scanline = multiply(scanline, m_dir.td_samplesperpixel, "TIFFScanlineSize");

            return howMany8(scanline);
        }
    }
}
