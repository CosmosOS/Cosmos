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

using BitMiracle.LibJpeg;

namespace BitMiracle.LibTiff.Classic.Internal
{
    /// <summary>
    /// JPEG library destination data manager.
    /// These routines direct compressed data from LibJpeg.Net into the
    /// LibTiff.Net output buffer.
    /// </summary>
    class JpegStdDestination : DestinationManager
    {
        private Tiff m_tif;

        public JpegStdDestination(Tiff tif)
        {
            m_tif = tif;
        }

        public override void init_destination()
        {
            initInternalBuffer(m_tif.m_rawdata, 0);
        }

        public override bool empty_output_buffer()
        {
            /* the entire buffer has been filled */
            m_tif.m_rawcc = m_tif.m_rawdatasize;
            m_tif.flushData1();

            initInternalBuffer(m_tif.m_rawdata, 0);
            return true;
        }

        public override void term_destination()
        {
            m_tif.m_rawcp = m_tif.m_rawdatasize - freeInBuffer;
            m_tif.m_rawcc = m_tif.m_rawdatasize - freeInBuffer;
            /* NB: LibTiff.Net does the final buffer flush */
        }
    }
}
