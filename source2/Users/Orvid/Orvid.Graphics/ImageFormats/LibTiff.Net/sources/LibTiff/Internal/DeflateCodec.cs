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
 * ZIP (aka Deflate) Compression Support
 *
 * This file is simply an interface to the zlib library written by
 * Jean-loup Gailly and Mark Adler.  You must use version 1.0 or later
 * of the library: this code assumes the 1.0 API and also depends on
 * the ability to write the zlib header multiple times (one per strip)
 * which was not possible with versions prior to 0.95.  Note also that
 * older versions of this codec avoided this bug by supressing the header
 * entirely.  This means that files written with the old library cannot
 * be read; they should be converted to a different compression scheme
 * and then reconverted.
 *
 * The data format used by the zlib library is described in the files
 * zlib-3.1.doc, deflate-1.1.doc and gzip-4.1.doc, available in the
 * directory ftp://ftp.uu.net/pub/archiving/zip/doc.  The library was
 * last found at ftp://ftp.uu.net/pub/archiving/zip/zlib/zlib-0.99.tar.gz.
 */

using System.Diagnostics;

using ComponentAce.Compression.Libs.zlib;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class DeflateCodec : CodecWithPredictor
    {
        public const int ZSTATE_INIT_DECODE = 0x01;
        public const int ZSTATE_INIT_ENCODE = 0x02;

        public ZStream m_stream = new ZStream();
        public int m_zipquality; /* compression level */
        public int m_state; /* state flags */

        private static TiffFieldInfo[] zipFieldInfo = 
        {
            new TiffFieldInfo(TiffTag.ZIPQUALITY, 0, 0, TiffType.ANY, FieldBit.Pseudo, true, false, ""), 
        };

        private TiffTagMethods m_tagMethods;

        public DeflateCodec(Tiff tif, Compression scheme, string name)
            : base(tif, scheme, name)
        {
            m_tagMethods = new DeflateCodecTagMethods();
        }

        public override bool Init()
        {
            Debug.Assert((m_scheme == Compression.DEFLATE) || 
                (m_scheme == Compression.ADOBE_DEFLATE));

            /*
            * Merge codec-specific tag information and
            * override parent get/set field methods.
            */
            m_tif.MergeFieldInfo(zipFieldInfo, zipFieldInfo.Length);

            /* Default values for codec-specific fields */
            m_zipquality = zlibConst.Z_DEFAULT_COMPRESSION; /* default comp. level */
            m_state = 0;

            /*
             * Setup predictor setup.
             */
            TIFFPredictorInit(m_tagMethods);
            return true;
        }


        /// <summary>
        /// Gets a value indicating whether this codec can encode data.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this codec can encode data; otherwise, <c>false</c>.
        /// </value>
        public override bool CanEncode
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this codec can decode data.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this codec can decode data; otherwise, <c>false</c>.
        /// </value>
        public override bool CanDecode
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Prepares the decoder part of the codec for a decoding.
        /// </summary>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if this codec successfully prepared its decoder part and ready
        /// to decode data; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// 	<b>PreDecode</b> is called after <see cref="TiffCodec.SetupDecode"/> and before decoding.
        /// </remarks>
        public override bool PreDecode(short plane)
        {
            return ZIPPreDecode(plane);
        }

        /// <summary>
        /// Prepares the encoder part of the codec for a encoding.
        /// </summary>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if this codec successfully prepared its encoder part and ready
        /// to encode data; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// 	<b>PreEncode</b> is called after <see cref="TiffCodec.SetupEncode"/> and before encoding.
        /// </remarks>
        public override bool PreEncode(short plane)
        {
            return ZIPPreEncode(plane);
        }

        /// <summary>
        /// Performs any actions after encoding required by the codec.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if all post-encode actions succeeded; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// 	<b>PostEncode</b> is called after encoding and can be used to release any external
        /// resources needed during encoding.
        /// </remarks>
        public override bool PostEncode()
        {
            return ZIPPostEncode();
        }

        /// <summary>
        /// Cleanups the state of the codec.
        /// </summary>
        /// <remarks>
        /// 	<b>Cleanup</b> is called when codec is no longer needed (won't be used) and can be
        /// used for example to restore tag methods that were substituted.</remarks>
        public override void Cleanup()
        {
            ZIPCleanup();
        }

        // CodecWithPredictor overrides

        public override bool predictor_setupdecode()
        {
            return ZIPSetupDecode();
        }

        public override bool predictor_decoderow(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPDecode(buffer, offset, count, plane);
        }

        public override bool predictor_decodestrip(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPDecode(buffer, offset, count, plane);
        }

        public override bool predictor_decodetile(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPDecode(buffer, offset, count, plane);
        }

        public override bool predictor_setupencode()
        {
            return ZIPSetupEncode();
        }

        public override bool predictor_encoderow(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPEncode(buffer, offset, count, plane);
        }

        public override bool predictor_encodestrip(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPEncode(buffer, offset, count, plane);
        }

        public override bool predictor_encodetile(byte[] buffer, int offset, int count, short plane)
        {
            return ZIPEncode(buffer, offset, count, plane);
        }

        private void ZIPCleanup()
        {
            base.TIFFPredictorCleanup();

            if ((m_state & ZSTATE_INIT_ENCODE) != 0)
            {
                m_stream.deflateEnd();
                m_state = 0;
            }
            else if ((m_state & ZSTATE_INIT_DECODE) != 0)
            {
                m_stream.inflateEnd();
                m_state = 0;
            }
        }

        private bool ZIPDecode(byte[] buffer, int offset, int count, short plane)
        {
            const string module = "ZIPDecode";

            Debug.Assert(m_state == ZSTATE_INIT_DECODE);
            m_stream.next_out = buffer;
            m_stream.next_out_index = offset;
            m_stream.avail_out = count;
            do
            {
                int state = m_stream.inflate(zlibConst.Z_PARTIAL_FLUSH);
                if (state == zlibConst.Z_STREAM_END)
                    break;

                if (state == zlibConst.Z_DATA_ERROR)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "{0}: Decoding error at scanline {1}, {2}",
                        m_tif.m_name, m_tif.m_row, m_stream.msg);

                    if (m_stream.inflateSync() != zlibConst.Z_OK)
                        return false;
                    
                    continue;
                }

                if (state != zlibConst.Z_OK)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "{0}: zlib error: {1}", m_tif.m_name, m_stream.msg);
                    return false;
                }
            }
            while (m_stream.avail_out > 0);

            if (m_stream.avail_out != 0)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                    "{0}: Not enough data at scanline {1} (short {2} bytes)",
                    m_tif.m_name, m_tif.m_row, m_stream.avail_out);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Encode a chunk of pixels.
        /// </summary>
        private bool ZIPEncode(byte[] buffer, int offset, int count, short plane)
        {
            const string module = "ZIPEncode";

            Debug.Assert(m_state == ZSTATE_INIT_ENCODE);

            m_stream.next_in = buffer;
            m_stream.next_in_index = offset;
            m_stream.avail_in = count;
            do
            {
                if (m_stream.deflate(zlibConst.Z_NO_FLUSH) != zlibConst.Z_OK)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "{0}: Encoder error: {1}", m_tif.m_name, m_stream.msg);
                    return false;
                }

                if (m_stream.avail_out == 0)
                {
                    m_tif.m_rawcc = m_tif.m_rawdatasize;
                    m_tif.flushData1();
                    m_stream.next_out = m_tif.m_rawdata;
                    m_stream.next_out_index = 0;
                    m_stream.avail_out = m_tif.m_rawdatasize;
                }
            }
            while (m_stream.avail_in > 0);

            return true;
        }

        /*
        * Finish off an encoded strip by flushing the last
        * string and tacking on an End Of Information code.
        */
        private bool ZIPPostEncode()
        {
            const string module = "ZIPPostEncode";
            int state;

            m_stream.avail_in = 0;
            do
            {
                state = m_stream.deflate(zlibConst.Z_FINISH);
                switch (state)
                {
                    case zlibConst.Z_STREAM_END:
                    case zlibConst.Z_OK:
                        if (m_stream.avail_out != m_tif.m_rawdatasize)
                        {
                            m_tif.m_rawcc = m_tif.m_rawdatasize - m_stream.avail_out;
                            m_tif.flushData1();
                            m_stream.next_out = m_tif.m_rawdata;
                            m_stream.next_out_index = 0;
                            m_stream.avail_out = m_tif.m_rawdatasize;
                        }
                        break;
                    default:
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                            "{0}: zlib error: {1}", m_tif.m_name, m_stream.msg);
                        return false;
                }
            }
            while (state != zlibConst.Z_STREAM_END);

            return true;
        }

        /*
        * Setup state for decoding a strip.
        */
        private bool ZIPPreDecode(short s)
        {
            if ((m_state & ZSTATE_INIT_DECODE) == 0)
                SetupDecode();

            m_stream.next_in = m_tif.m_rawdata;
            m_stream.next_in_index = 0;
            m_stream.avail_in = m_tif.m_rawcc;
            return (m_stream.inflateInit() == zlibConst.Z_OK);
        }

        /*
        * Reset encoding state at the start of a strip.
        */
        private bool ZIPPreEncode(short s)
        {
            if (m_state != ZSTATE_INIT_ENCODE)
                SetupEncode();

            m_stream.next_out = m_tif.m_rawdata;
            m_stream.next_out_index = 0;
            m_stream.avail_out = m_tif.m_rawdatasize;
            return (m_stream.deflateInit(m_zipquality) == zlibConst.Z_OK);
        }

        private bool ZIPSetupDecode()
        {
            const string module = "ZIPSetupDecode";

            /* if we were last encoding, terminate this mode */
            if ((m_state & ZSTATE_INIT_ENCODE) != 0)
            {
                m_stream.deflateEnd();
                m_state = 0;
            }

            if (m_stream.inflateInit() != zlibConst.Z_OK)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "{0}: {1}",
                    m_tif.m_name, m_stream.msg);
                return false;
            }

            m_state |= ZSTATE_INIT_DECODE;
            return true;
        }

        private bool ZIPSetupEncode()
        {
            const string module = "ZIPSetupEncode";

            if ((m_state & ZSTATE_INIT_DECODE) != 0)
            {
                m_stream.inflateEnd();
                m_state = 0;
            }

            if (m_stream.deflateInit(m_zipquality) != zlibConst.Z_OK)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "{0}: {1}",
                    m_tif.m_name, m_stream.msg);
                return false;
            }

            m_state |= ZSTATE_INIT_ENCODE;
            return true;
        }
    }
}
