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
 * JPEG Compression support per TIFF Technical Note #2
 * (*not* per the original TIFF 6.0 spec).
 *
 * This file is simply an interface to the libjpeg library written by
 * the Independent JPEG Group.  You need release 5 or later of the IJG
 * code, which you can find on the Internet at ftp.uu.net:/graphics/jpeg/.
 *
 * Contributed by Tom Lane <tgl@sss.pgh.pa.us>.
 */

using System;
using System.Diagnostics;

using BitMiracle.LibJpeg;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class JpegCodec : TiffCodec
    {
        public const int FIELD_JPEGTABLES = (FieldBit.Codec + 0);
        public const int FIELD_RECVPARAMS = (FieldBit.Codec + 1);
        public const int FIELD_SUBADDRESS = (FieldBit.Codec + 2);
        public const int FIELD_RECVTIME = (FieldBit.Codec + 3);
        public const int FIELD_FAXDCS = (FieldBit.Codec + 4);

        internal JpegCompressor m_compression;
        internal JpegDecompressor m_decompression;
        internal JpegCommonBase m_common;

        internal int m_h_sampling; /* luminance sampling factors */
        internal int m_v_sampling;

        /* pseudo-tag fields */
        internal byte[] m_jpegtables; /* JPEGTables tag value, or null */
        internal int m_jpegtables_length; /* number of bytes in same */
        internal int m_jpegquality; /* Compression quality level */
        internal JpegColorMode m_jpegcolormode; /* Auto RGB<=>YCbCr convert? */
        internal JpegTablesMode m_jpegtablesmode; /* What to put in JPEGTables */

        internal bool m_ycbcrsampling_fetched;

        internal int m_recvparams; /* encoded Class 2 session params */
        internal string m_subaddress; /* subaddress string */
        internal int m_recvtime; /* time spent receiving (secs) */
        internal string m_faxdcs; /* encoded fax parameters (DCS, Table 2/T.30) */

        private static TiffFieldInfo[] jpegFieldInfo = 
        {
            new TiffFieldInfo(TiffTag.JPEGTABLES, -3, -3, TiffType.UNDEFINED, FIELD_JPEGTABLES, false, true, "JPEGTables"), 
            new TiffFieldInfo(TiffTag.JPEGQUALITY, 0, 0, TiffType.ANY, FieldBit.Pseudo, true, false, ""), 
            new TiffFieldInfo(TiffTag.JPEGCOLORMODE, 0, 0, TiffType.ANY, FieldBit.Pseudo, false, false, ""), 
            new TiffFieldInfo(TiffTag.JPEGTABLESMODE, 0, 0, TiffType.ANY, FieldBit.Pseudo, false, false, ""), 
            /* Specific for JPEG in faxes */
            new TiffFieldInfo(TiffTag.FAXRECVPARAMS, 1, 1, TiffType.LONG, FIELD_RECVPARAMS, true, false, "FaxRecvParams"), 
            new TiffFieldInfo(TiffTag.FAXSUBADDRESS, -1, -1, TiffType.ASCII, FIELD_SUBADDRESS, true, false, "FaxSubAddress"), 
            new TiffFieldInfo(TiffTag.FAXRECVTIME, 1, 1, TiffType.LONG, FIELD_RECVTIME, true, false, "FaxRecvTime"), 
            new TiffFieldInfo(TiffTag.FAXDCS, -1, -1, TiffType.ASCII, FIELD_FAXDCS, true, false, "FaxDcs"), 
        };

        private bool m_rawDecode;
        private bool m_rawEncode;

        private TiffTagMethods m_tagMethods;
        private TiffTagMethods m_parentTagMethods;

        private bool m_cinfo_initialized;

        private Photometric m_photometric; /* copy of PhotometricInterpretation */

        private int m_bytesperline; /* decompressed bytes per scanline */
        /* pointers to intermediate buffers when processing downsampled data */
        private byte[][][] m_ds_buffer = new byte[JpegConstants.MaxComponents][][];
        private int m_scancount; /* number of "scanlines" accumulated */
        private int m_samplesperclump;

        public JpegCodec(Tiff tif, Compression scheme, string name)
            : base(tif, scheme, name)
        {
            m_tagMethods = new JpegCodecTagMethods();
        }

        public override bool Init()
        {
            Debug.Assert(m_scheme == Compression.JPEG);

            /*
            * Merge codec-specific tag information and override parent get/set
            * field methods.
            */
            m_tif.MergeFieldInfo(jpegFieldInfo, jpegFieldInfo.Length);

            /*
             * Allocate state block so tag methods have storage to record values.
             */
            m_compression = null;
            m_decompression = null;
            m_photometric = 0;
            m_h_sampling = 0;
            m_v_sampling = 0;
            m_bytesperline = 0;
            m_scancount = 0;
            m_samplesperclump = 0;
            m_recvtime = 0;

            m_parentTagMethods = m_tif.m_tagmethods;
            m_tif.m_tagmethods = m_tagMethods;

            /* Default values for codec-specific fields */
            m_jpegtables = null;
            m_jpegtables_length = 0;
            m_jpegquality = 75; /* Default IJG quality */
            m_jpegcolormode = JpegColorMode.RGB;
            m_jpegtablesmode = JpegTablesMode.QUANT | JpegTablesMode.HUFF;

            m_recvparams = 0;
            m_subaddress = null;
            m_faxdcs = null;

            m_ycbcrsampling_fetched = false;

            m_rawDecode = false;
            m_rawEncode = false;
            m_tif.m_flags |= TiffFlags.NOBITREV; // no bit reversal, please

            m_cinfo_initialized = false;

            /*
             ** Create a JPEGTables field if no directory has yet been created. 
             ** We do this just to ensure that sufficient space is reserved for
             ** the JPEGTables field.  It will be properly created the right
             ** size later. 
             */
            if (m_tif.m_diroff == 0)
            {
                const int SIZE_OF_JPEGTABLES = 2000;
                
                // The following line assumes incorrectly that all JPEG-in-TIFF
                // files will have a JPEGTABLES tag generated and causes
                // null-filled JPEGTABLES tags to be written when the JPEG data
                // is placed with WriteRawStrip. The field bit should be 
                // set, anyway, later when actual JPEGTABLES header is
                // generated, so removing it here hopefully is harmless.
                //
                //       m_tif.setFieldBit(FIELD_JPEGTABLES);
                //

                m_jpegtables_length = SIZE_OF_JPEGTABLES;
                m_jpegtables = new byte[m_jpegtables_length];
            }

            /*
             * Mark the YCBCRSAMPLES as present even if it is not
             * see: JPEGFixupTestSubsampling().
             */
            m_tif.setFieldBit(FieldBit.YCbCrSubsampling);
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
        /// Setups the decoder part of the codec.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this codec successfully setup its decoder part and can decode data;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// 	<b>SetupDecode</b> is called once before
        /// <see cref="PreDecode"/>.</remarks>
        public override bool SetupDecode()
        {
            return JPEGSetupDecode();
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
        /// 	<b>PreDecode</b> is called after <see cref="SetupDecode"/> and before decoding.
        /// </remarks>
        public override bool PreDecode(short plane)
        {
            return JPEGPreDecode(plane);
        }

        /// <summary>
        /// Decodes one row of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool DecodeRow(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawDecode)
                return JPEGDecodeRaw(buffer, offset, count, plane);

            return JPEGDecode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Decodes one strip of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawDecode)
                return JPEGDecodeRaw(buffer, offset, count, plane);

            return JPEGDecode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Decodes one tile of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool DecodeTile(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawDecode)
                return JPEGDecodeRaw(buffer, offset, count, plane);

            return JPEGDecode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Setups the encoder part of the codec.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this codec successfully setup its encoder part and can encode data;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// 	<b>SetupEncode</b> is called once before
        /// <see cref="PreEncode"/>.</remarks>
        public override bool SetupEncode()
        {
            return JPEGSetupEncode();
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
        /// 	<b>PreEncode</b> is called after <see cref="SetupEncode"/> and before encoding.
        /// </remarks>
        public override bool PreEncode(short plane)
        {
            return JPEGPreEncode(plane);
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
            return JPEGPostEncode();
        }

        /// <summary>
        /// Encodes one row of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool EncodeRow(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawEncode)
                return JPEGEncodeRaw(buffer, offset, count, plane);

            return JPEGEncode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Encodes one strip of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawEncode)
                return JPEGEncodeRaw(buffer, offset, count, plane);

            return JPEGEncode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Encodes one tile of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/></param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public override bool EncodeTile(byte[] buffer, int offset, int count, short plane)
        {
            if (m_rawEncode)
                return JPEGEncodeRaw(buffer, offset, count, plane);

            return JPEGEncode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Cleanups the state of the codec.
        /// </summary>
        /// <remarks>
        /// 	<b>Cleanup</b> is called when codec is no longer needed (won't be used) and can be
        /// used for example to restore tag methods that were substituted.</remarks>
        public override void Cleanup()
        {
            JPEGCleanup();
        }

        /// <summary>
        /// Calculates and/or constrains a strip size.
        /// </summary>
        /// <param name="size">The proposed strip size (may be zero or negative).</param>
        /// <returns>A strip size to use.</returns>
        public override int DefStripSize(int size)
        {
            return JPEGDefaultStripSize(size);
        }

        /// <summary>
        /// Calculate and/or constrains a tile size
        /// </summary>
        /// <param name="width">The proposed tile width upon the call / tile width to use after the call.</param>
        /// <param name="height">The proposed tile height upon the call / tile height to use after the call.</param>
        public override void DefTileSize(ref int width, ref int height)
        {
            JPEGDefaultTileSize(ref width, ref height);
        }

        /*
         * The JPEG library initialized used to be done in TIFFInitJPEG(), but
         * now that we allow a TIFF file to be opened in update mode it is necessary
         * to have some way of deciding whether compression or decompression is
         * desired other than looking at tif.tif_mode.  We accomplish this by 
         * examining {TILE/STRIP}BYTECOUNTS to see if there is a non-zero entry.
         * If so, we assume decompression is desired. 
         *
         * This is tricky, because TIFFInitJPEG() is called while the directory is
         * being read, and generally speaking the BYTECOUNTS tag won't have been read
         * at that point.  So we try to defer jpeg library initialization till we
         * do have that tag ... basically any access that might require the compressor
         * or decompressor that occurs after the reading of the directory. 
         *
         * In an ideal world compressors or decompressors would be setup
         * at the point where a single tile or strip was accessed (for read or write)
         * so that stuff like update of missing tiles, or replacement of tiles could
         * be done. However, we aren't trying to crack that nut just yet ...
         *
         * NFW, Feb 3rd, 2003.
         */
        public bool InitializeLibJPEG(bool force_encode, bool force_decode)
        {
            int[] byte_counts = null;
            bool data_is_empty = true;
            bool decompress;

            if (m_cinfo_initialized)
            {
                if (force_encode && m_common.IsDecompressor)
                    TIFFjpeg_destroy();
                else if (force_decode && !m_common.IsDecompressor)
                    TIFFjpeg_destroy();
                else
                    return true;

                m_cinfo_initialized = false;
            }

            /*
             * Do we have tile data already?  Make sure we initialize the
             * the state in decompressor mode if we have tile data, even if we
             * are not in read-only file access mode. 
             */
            FieldValue[] result = m_tif.GetField(TiffTag.TILEBYTECOUNTS);
            if (m_tif.IsTiled() && result != null)
            {
                byte_counts = result[0].ToIntArray();
                if (byte_counts != null)
                    data_is_empty = byte_counts[0] == 0;
            }

            result = m_tif.GetField(TiffTag.STRIPBYTECOUNTS);
            if (!m_tif.IsTiled() && result != null)
            {
                byte_counts = result[0].ToIntArray();
                if (byte_counts != null)
                    data_is_empty = byte_counts[0] == 0;
            }

            if (force_decode)
                decompress = true;
            else if (force_encode)
                decompress = false;
            else if (m_tif.m_mode == Tiff.O_RDONLY)
                decompress = true;
            else if (data_is_empty)
                decompress = false;
            else
                decompress = true;

            // Initialize LibJpeg.Net
            if (decompress)
            {
                if (!TIFFjpeg_create_decompress())
                    return false;
            }
            else
            {
                if (!TIFFjpeg_create_compress())
                    return false;
            }

            m_cinfo_initialized = true;
            return true;
        }

        public Tiff GetTiff()
        {
            return m_tif;
        }

        public void JPEGResetUpsampled()
        {
            /*
            * Mark whether returned data is up-sampled or not so TIFFStripSize
            * and TIFFTileSize return values that reflect the true amount of
            * data.
            */
            m_tif.m_flags &= ~TiffFlags.UPSAMPLED;
            if (m_tif.m_dir.td_planarconfig == PlanarConfig.CONTIG)
            {
                if (m_tif.m_dir.td_photometric == Photometric.YCBCR && m_jpegcolormode == JpegColorMode.RGB)
                    m_tif.m_flags |= TiffFlags.UPSAMPLED;
            }

            /*
            * Must recalculate cached tile size in case sampling state changed.
            * Should we really be doing this now if image size isn't set? 
            */
            if (m_tif.m_tilesize > 0)
                m_tif.m_tilesize = m_tif.IsTiled() ? m_tif.TileSize() : -1;

            if (m_tif.m_scanlinesize > 0)
                m_tif.m_scanlinesize = m_tif.ScanlineSize();
        }

        /// <summary>
        /// Set encoding state at the start of a strip or tile.
        /// </summary>
        private bool JPEGPreEncode(short s)
        {
            const string module = "JPEGPreEncode";
            int segment_width;
            int segment_height;
            bool downsampled_input;

            Debug.Assert(!m_common.IsDecompressor);
            /*
             * Set encoding parameters for this strip/tile.
             */
            if (m_tif.IsTiled())
            {
                segment_width = m_tif.m_dir.td_tilewidth;
                segment_height = m_tif.m_dir.td_tilelength;
                m_bytesperline = m_tif.TileRowSize();
            }
            else
            {
                segment_width = m_tif.m_dir.td_imagewidth;
                segment_height = m_tif.m_dir.td_imagelength - m_tif.m_row;
                if (segment_height > m_tif.m_dir.td_rowsperstrip)
                    segment_height = m_tif.m_dir.td_rowsperstrip;
                m_bytesperline = m_tif.oldScanlineSize();
            }
            if (m_tif.m_dir.td_planarconfig == PlanarConfig.SEPARATE && s > 0)
            {
                /* for PC 2, scale down the strip/tile size
                 * to match a downsampled component
                 */
                segment_width = Tiff.howMany(segment_width, m_h_sampling);
                segment_height = Tiff.howMany(segment_height, m_v_sampling);
            }

            if (segment_width > 65535 || segment_height > 65535)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "Strip/tile too large for JPEG");
                return false;
            }

            m_compression.Image_width = segment_width;
            m_compression.Image_height = segment_height;
            downsampled_input = false;

            if (m_tif.m_dir.td_planarconfig == PlanarConfig.CONTIG)
            {
                m_compression.Input_components = m_tif.m_dir.td_samplesperpixel;
                if (m_photometric == Photometric.YCBCR)
                {
                    if (m_jpegcolormode == JpegColorMode.RGB)
                    {
                        m_compression.In_color_space = ColorSpace.RGB;
                    }
                    else
                    {
                        m_compression.In_color_space = ColorSpace.YCbCr;
                        if (m_h_sampling != 1 || m_v_sampling != 1)
                            downsampled_input = true;
                    }

                    if (!TIFFjpeg_set_colorspace(ColorSpace.YCbCr))
                        return false;

                    /*
                     * Set Y sampling factors;
                     * we assume jpeg_set_colorspace() set the rest to 1
                     */
                    m_compression.Component_info[0].H_samp_factor = m_h_sampling;
                    m_compression.Component_info[0].V_samp_factor = m_v_sampling;
                }
                else
                {
                    m_compression.In_color_space = ColorSpace.Unknown;
                    if (!TIFFjpeg_set_colorspace(ColorSpace.Unknown))
                        return false;
                    /* jpeg_set_colorspace set all sampling factors to 1 */
                }
            }
            else
            {
                m_compression.Input_components = 1;
                m_compression.In_color_space = ColorSpace.Unknown;
                if (!TIFFjpeg_set_colorspace(ColorSpace.Unknown))
                    return false;

                m_compression.Component_info[0].Component_id = s;
                /* jpeg_set_colorspace() set sampling factors to 1 */
                if (m_photometric == Photometric.YCBCR && s > 0)
                {
                    m_compression.Component_info[0].Quant_tbl_no = 1;
                    m_compression.Component_info[0].Dc_tbl_no = 1;
                    m_compression.Component_info[0].Ac_tbl_no = 1;
                }
            }

            // ensure LibJpeg.Net won't write any extraneous markers
            m_compression.Write_JFIF_header = false;
            m_compression.Write_Adobe_marker = false;

            /* set up table handling correctly */
            if (!TIFFjpeg_set_quality(m_jpegquality, false))
                return false;

            if ((m_jpegtablesmode & JpegTablesMode.QUANT) == 0)
            {
                unsuppress_quant_table(0);
                unsuppress_quant_table(1);
            }

            if ((m_jpegtablesmode & JpegTablesMode.HUFF) != 0)
                m_compression.Optimize_coding = false;
            else
                m_compression.Optimize_coding = true;

            if (downsampled_input)
            {
                // Need to use raw-data interface to LibJpeg.Net
                m_compression.Raw_data_in = true;
                m_rawEncode = true;
            }
            else
            {
                // Use normal interface to LibJpeg.Net
                m_compression.Raw_data_in = false;
                m_rawEncode = false;
            }

            /* Start JPEG compressor */
            if (!TIFFjpeg_start_compress(false))
                return false;

            /* Allocate downsampled-data buffers if needed */
            if (downsampled_input)
            {
                if (!alloc_downsampled_buffers(m_compression.Component_info, m_compression.Num_components))
                    return false;
            }

            m_scancount = 0;
            return true;
        }

        private bool JPEGSetupEncode()
        {
            const string module = "JPEGSetupEncode";

            InitializeLibJPEG(true, false);

            Debug.Assert(!m_common.IsDecompressor);

            /*
             * Initialize all JPEG parameters to default values.
             * Note that jpeg_set_defaults needs legal values for
             * in_color_space and input_components.
             */
            m_compression.In_color_space = ColorSpace.Unknown;
            m_compression.Input_components = 1;
            if (!TIFFjpeg_set_defaults())
                return false;

            /* Set per-file parameters */
            m_photometric = m_tif.m_dir.td_photometric;
            switch (m_photometric)
            {
                case Photometric.YCBCR:
                    m_h_sampling = m_tif.m_dir.td_ycbcrsubsampling[0];
                    m_v_sampling = m_tif.m_dir.td_ycbcrsubsampling[1];
                    /*
                     * A ReferenceBlackWhite field *must* be present since the
                     * default value is inappropriate for YCbCr.  Fill in the
                     * proper value if application didn't set it.
                     */
                    FieldValue[] result = m_tif.GetField(TiffTag.REFERENCEBLACKWHITE);
                    if (result == null)
                    {
                        float[] refbw = new float[6];
                        int top = 1 << m_tif.m_dir.td_bitspersample;
                        refbw[0] = 0;
                        refbw[1] = (float)(top - 1L);
                        refbw[2] = (float)(top >> 1);
                        refbw[3] = refbw[1];
                        refbw[4] = refbw[2];
                        refbw[5] = refbw[1];
                        m_tif.SetField(TiffTag.REFERENCEBLACKWHITE, refbw);
                    }
                    break;

                /* disallowed by Tech Note */
                case Photometric.PALETTE:
                case Photometric.MASK:
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "PhotometricInterpretation {0} not allowed for JPEG", m_photometric);
                    return false;

                default:
                    /* TIFF 6.0 forbids subsampling of all other color spaces */
                    m_h_sampling = 1;
                    m_v_sampling = 1;
                    break;
            }

            /* Verify miscellaneous parameters */

            // This would need work if LibTiff.Net ever supports different
            // depths for different components, or if LibJpeg.Net ever supports
            // run-time selection of depth.  Neither is imminent.
            if (m_tif.m_dir.td_bitspersample != JpegConstants.BitsInSample)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                    "BitsPerSample {0} not allowed for JPEG", m_tif.m_dir.td_bitspersample);
                return false;
            }

            m_compression.Data_precision = m_tif.m_dir.td_bitspersample;
            if (m_tif.IsTiled())
            {
                if ((m_tif.m_dir.td_tilelength % (m_v_sampling * JpegConstants.DCTSize)) != 0)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "JPEG tile height must be multiple of {0}", m_v_sampling * JpegConstants.DCTSize);
                    return false;
                }

                if ((m_tif.m_dir.td_tilewidth % (m_h_sampling * JpegConstants.DCTSize)) != 0)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "JPEG tile width must be multiple of {0}", m_h_sampling * JpegConstants.DCTSize);
                    return false;
                }
            }
            else
            {
                if (m_tif.m_dir.td_rowsperstrip < m_tif.m_dir.td_imagelength &&
                    (m_tif.m_dir.td_rowsperstrip % (m_v_sampling * JpegConstants.DCTSize)) != 0)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                        "RowsPerStrip must be multiple of {0} for JPEG", m_v_sampling * JpegConstants.DCTSize);
                    return false;
                }
            }

            /* Create a JPEGTables field if appropriate */
            if ((m_jpegtablesmode & (JpegTablesMode.QUANT | JpegTablesMode.HUFF)) != 0)
            {
                bool startsWithZeroes = true;
                if (m_jpegtables != null)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (m_jpegtables[i] != 0)
                        {
                            startsWithZeroes = false;
                            break;
                        }
                    }
                }
                else
                {
                    startsWithZeroes = false;
                }

                if (m_jpegtables == null || startsWithZeroes)
                {
                    if (!prepare_JPEGTables())
                        return false;

                    /* Mark the field present */
                    /* Can't use TIFFSetField since BEENWRITING is already set! */
                    m_tif.m_flags |= TiffFlags.DIRTYDIRECT;
                    m_tif.setFieldBit(FIELD_JPEGTABLES);
                }
            }
            else
            {
                /* We do not support application-supplied JPEGTables, */
                /* so mark the field not present */
                m_tif.clearFieldBit(FIELD_JPEGTABLES);
            }

            /* Direct LibJpeg.Net output to LibTiff.Net's output buffer */
            TIFFjpeg_data_dest();

            return true;
        }

        /// <summary>
        /// Finish up at the end of a strip or tile.
        /// </summary>
        /// <returns></returns>
        private bool JPEGPostEncode()
        {
            if (m_scancount > 0)
            {
                // Need to emit a partial bufferload of downsampled data. Pad the data vertically.
                for (int ci = 0; ci < m_compression.Num_components; ci++)
                {
                    int vsamp = m_compression.Component_info[ci].V_samp_factor;
                    int row_width = m_compression.Component_info[ci].Width_in_blocks * JpegConstants.DCTSize * sizeof(byte);
                    for (int ypos = m_scancount * vsamp; ypos < JpegConstants.DCTSize * vsamp; ypos++)
                        Buffer.BlockCopy(m_ds_buffer[ci][ypos - 1], 0, m_ds_buffer[ci][ypos], 0, row_width);
                }

                int n = m_compression.Max_v_samp_factor * JpegConstants.DCTSize;
                if (TIFFjpeg_write_raw_data(m_ds_buffer, n) != n)
                    return false;
            }

            return TIFFjpeg_finish_compress();
        }

        private void JPEGCleanup()
        {
            m_tif.m_tagmethods = m_parentTagMethods;

            if (m_cinfo_initialized)
            {
                // release LibJpeg.Net resources
                TIFFjpeg_destroy();
            }
        }

        /*
        * JPEG Decoding.
        */

        /*
        * Set up for decoding a strip or tile.
        */
        private bool JPEGPreDecode(short s)
        {
            TiffDirectory td = m_tif.m_dir;
            const string module = "JPEGPreDecode";
            int segment_width;
            int segment_height;
            int ci;

            Debug.Assert(m_common.IsDecompressor);

            /*
             * Reset decoder state from any previous strip/tile,
             * in case application didn't read the whole strip.
             */
            if (!TIFFjpeg_abort())
                return false;

            /*
             * Read the header for this strip/tile.
             */
            if (TIFFjpeg_read_header(true) != ReadResult.Header_Ok)
                return false;

            /*
             * Check image parameters and set decompression parameters.
             */
            segment_width = td.td_imagewidth;
            segment_height = td.td_imagelength - m_tif.m_row;
            if (m_tif.IsTiled())
            {
                segment_width = td.td_tilewidth;
                segment_height = td.td_tilelength;
                m_bytesperline = m_tif.TileRowSize();
            }
            else
            {
                if (segment_height > td.td_rowsperstrip)
                    segment_height = td.td_rowsperstrip;
                m_bytesperline = m_tif.oldScanlineSize();
            }

            if (td.td_planarconfig == PlanarConfig.SEPARATE && s > 0)
            {
                /*
                 * For PC 2, scale down the expected strip/tile size
                 * to match a downsampled component
                 */
                segment_width = Tiff.howMany(segment_width, m_h_sampling);
                segment_height = Tiff.howMany(segment_height, m_v_sampling);
            }

            if (m_decompression.Image_width < segment_width || m_decompression.Image_height < segment_height)
            {
                Tiff.WarningExt(m_tif, m_tif.m_clientdata, module,
                    "Improper JPEG strip/tile size, expected {0}x{1}, got {2}x{3}",
                    segment_width, segment_height, m_decompression.Image_width, m_decompression.Image_height);
            }

            if (m_decompression.Image_width > segment_width || m_decompression.Image_height > segment_height)
            {
                /*
                * This case could be dangerous, if the strip or tile size has
                * been reported as less than the amount of data jpeg will
                * return, some potential security issues arise. Catch this
                * case and error out.
                */
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                    "JPEG strip/tile size exceeds expected dimensions, expected {0}x{1}, got {2}x{3}",
                    segment_width, segment_height, m_decompression.Image_width, m_decompression.Image_height);
                return false;
            }

            if (m_decompression.Num_components != (td.td_planarconfig == PlanarConfig.CONTIG ? (int)td.td_samplesperpixel : 1))
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "Improper JPEG component count");
                return false;
            }

            if (m_decompression.Data_precision != td.td_bitspersample)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "Improper JPEG data precision");
                return false;
            }

            if (td.td_planarconfig == PlanarConfig.CONTIG)
            {
                /* Component 0 should have expected sampling factors */
                if (m_decompression.Comp_info[0].H_samp_factor != m_h_sampling ||
                    m_decompression.Comp_info[0].V_samp_factor != m_v_sampling)
                {
                    Tiff.WarningExt(m_tif, m_tif.m_clientdata, module,
                        "Improper JPEG sampling factors {0},{1}\nApparently should be {2},{3}.",
                        m_decompression.Comp_info[0].H_samp_factor,
                        m_decompression.Comp_info[0].V_samp_factor, m_h_sampling, m_v_sampling);

                    /*
                    * There are potential security issues here
                    * for decoders that have already allocated
                    * buffers based on the expected sampling
                    * factors. Lets check the sampling factors
                    * dont exceed what we were expecting.
                    */
                    if (m_decompression.Comp_info[0].H_samp_factor > m_h_sampling ||
                        m_decompression.Comp_info[0].V_samp_factor > m_v_sampling)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                            "Cannot honour JPEG sampling factors that exceed those specified.");
                        return false;
                    }

                    /*
                     * XXX: Files written by the Intergraph software
                     * has different sampling factors stored in the
                     * TIFF tags and in the JPEG structures. We will
                     * try to deduce Intergraph files by the presense
                     * of the tag 33918.
                     */
                    if (m_tif.FindFieldInfo((TiffTag)33918, TiffType.ANY) == null)
                    {
                        Tiff.WarningExt(m_tif, m_tif.m_clientdata, module,
                            "Decompressor will try reading with sampling {0},{1}.",
                            m_decompression.Comp_info[0].H_samp_factor,
                            m_decompression.Comp_info[0].V_samp_factor);

                        m_h_sampling = m_decompression.Comp_info[0].H_samp_factor;
                        m_v_sampling = m_decompression.Comp_info[0].V_samp_factor;
                    }
                }

                /* Rest should have sampling factors 1,1 */
                for (ci = 1; ci < m_decompression.Num_components; ci++)
                {
                    if (m_decompression.Comp_info[ci].H_samp_factor != 1 ||
                        m_decompression.Comp_info[ci].V_samp_factor != 1)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "Improper JPEG sampling factors");
                        return false;
                    }
                }
            }
            else
            {
                /* PC 2's single component should have sampling factors 1,1 */
                if (m_decompression.Comp_info[0].H_samp_factor != 1 ||
                    m_decompression.Comp_info[0].V_samp_factor != 1)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module, "Improper JPEG sampling factors");
                    return false;
                }
            }

            bool downsampled_output = false;
            if (td.td_planarconfig == PlanarConfig.CONTIG &&
                m_photometric == Photometric.YCBCR &&
                m_jpegcolormode == JpegColorMode.RGB)
            {
                /* Convert YCbCr to RGB */
                m_decompression.Jpeg_color_space = ColorSpace.YCbCr;
                m_decompression.Out_color_space = ColorSpace.RGB;
            }
            else
            {
                /* Suppress colorspace handling */
                m_decompression.Jpeg_color_space = ColorSpace.Unknown;
                m_decompression.Out_color_space = ColorSpace.Unknown;
                if (td.td_planarconfig == PlanarConfig.CONTIG &&
                    (m_h_sampling != 1 || m_v_sampling != 1))
                {
                    downsampled_output = true;
                }
                /* XXX what about up-sampling? */
            }

            if (downsampled_output)
            {
                // Need to use raw-data interface to LibJpeg.Net
                m_decompression.Raw_data_out = true;
                m_rawDecode = true;
            }
            else
            {
                // Use normal interface to LibJpeg.Net
                m_decompression.Raw_data_out = false;
                m_rawDecode = false;
            }

            /* Start JPEG decompressor */
            if (!TIFFjpeg_start_decompress())
                return false;

            /* Allocate downsampled-data buffers if needed */
            if (downsampled_output)
            {
                if (!alloc_downsampled_buffers(m_decompression.Comp_info, m_decompression.Num_components))
                    return false;

                m_scancount = JpegConstants.DCTSize; /* mark buffer empty */
            }

            return true;
        }

        private bool prepare_JPEGTables()
        {
            InitializeLibJPEG(false, false);

            /* Initialize quant tables for current quality setting */
            if (!TIFFjpeg_set_quality(m_jpegquality, false))
                return false;

            /* Mark only the tables we want for output */
            /* NB: chrominance tables are currently used only with YCbCr */
            if (!TIFFjpeg_suppress_tables(true))
                return false;

            if ((m_jpegtablesmode & JpegTablesMode.QUANT) != 0)
            {
                unsuppress_quant_table(0);
                if (m_photometric == Photometric.YCBCR)
                    unsuppress_quant_table(1);
            }

            if ((m_jpegtablesmode & JpegTablesMode.HUFF) != 0)
            {
                unsuppress_huff_table(0);
                if (m_photometric == Photometric.YCBCR)
                    unsuppress_huff_table(1);
            }

            // Direct LibJpeg.Net output into jpegtables
            if (!TIFFjpeg_tables_dest())
                return false;

            /* Emit tables-only datastream */
            if (!TIFFjpeg_write_tables())
                return false;

            return true;
        }

        private bool JPEGSetupDecode()
        {
            TiffDirectory td = m_tif.m_dir;

            InitializeLibJPEG(false, true);

            Debug.Assert(m_common.IsDecompressor);

            /* Read JPEGTables if it is present */
            if (m_tif.fieldSet(FIELD_JPEGTABLES))
            {
                m_decompression.Src = new JpegTablesSource(this);
                if (TIFFjpeg_read_header(false) != ReadResult.Header_Tables_Only)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, "JPEGSetupDecode", "Bogus JPEGTables field");
                    return false;
                }
            }

            /* Grab parameters that are same for all strips/tiles */
            m_photometric = td.td_photometric;
            switch (m_photometric)
            {
                case Photometric.YCBCR:
                    m_h_sampling = td.td_ycbcrsubsampling[0];
                    m_v_sampling = td.td_ycbcrsubsampling[1];
                    break;
                default:
                    /* TIFF 6.0 forbids subsampling of all other color spaces */
                    m_h_sampling = 1;
                    m_v_sampling = 1;
                    break;
            }

            /* Set up for reading normal data */
            m_decompression.Src = new JpegStdSource(this);
            m_tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmNone; /* override byte swapping */
            return true;
        }

        private int TIFFjpeg_read_scanlines(byte[][] scanlines, int max_lines)
        {
            int n = 0;
            try
            {
                n = m_decompression.jpeg_read_scanlines(scanlines, max_lines);
            }
            catch (Exception)
            {
                return -1;
            }

            return n;
        }

        /// <summary>
        /// Decode a chunk of pixels.
        /// "Standard" case: returned data is not downsampled.
        /// </summary>
        private bool JPEGDecode(byte[] buffer, int offset, int count, short plane)
        {
            int nrows = count / m_bytesperline;
            if ((count % m_bytesperline) != 0)
                Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline not read");

            if (nrows > (int)m_decompression.Image_height)
                nrows = m_decompression.Image_height;

            // data is expected to be read in multiples of a scanline
            if (nrows != 0)
            {
                byte[][] bufptr = new byte[1][];
                bufptr[0] = new byte[m_bytesperline];
                do
                {
                    // In the 8bit case.  We read directly into the TIFF buffer.
                    Array.Clear(bufptr[0], 0, m_bytesperline);
                    if (TIFFjpeg_read_scanlines(bufptr, 1) != 1)
                        return false;

                    ++m_tif.m_row;
                    Buffer.BlockCopy(bufptr[0], 0, buffer, offset, m_bytesperline);
                    offset += m_bytesperline;
                    count -= m_bytesperline;
                }
                while (--nrows > 0);
            }

            // Close down the decompressor if we've finished the strip or tile.
            return m_decompression.Output_scanline < m_decompression.Output_height || TIFFjpeg_finish_decompress();
        }

        /// <summary>
        /// Decode a chunk of pixels. 
        /// Returned data is downsampled per sampling factors.
        /// </summary>
        private bool JPEGDecodeRaw(byte[] buffer, int offset, int count, short plane)
        {
            // data is expected to be read in multiples of a scanline
            int nrows = m_decompression.Image_height;
            if (nrows != 0)
            {
                // Cb,Cr both have sampling factors 1, so this is correct
                int clumps_per_line = m_decompression.Comp_info[1].Downsampled_width;

                do
                {
                    // Reload downsampled-data buffer if needed
                    if (m_scancount >= JpegConstants.DCTSize)
                    {
                        int n = m_decompression.Max_v_samp_factor * JpegConstants.DCTSize;
                        if (TIFFjpeg_read_raw_data(m_ds_buffer, n) != n)
                            return false;

                        m_scancount = 0;
                    }

                    // Fastest way to unseparate data is to make one pass over the scanline for
                    // each row of each component.
                    int clumpoffset = 0; // first sample in clump
                    for (int ci = 0; ci < m_decompression.Num_components; ci++)
                    {
                        int hsamp = m_decompression.Comp_info[ci].H_samp_factor;
                        int vsamp = m_decompression.Comp_info[ci].V_samp_factor;

                        for (int ypos = 0; ypos < vsamp; ypos++)
                        {
                            byte[] inBuf = m_ds_buffer[ci][m_scancount * vsamp + ypos];
                            int inptr = 0;

                            int outptr = offset + clumpoffset;
                            if (outptr >= buffer.Length)
                                break;

                            if (hsamp == 1)
                            {
                                // fast path for at least Cb and Cr
                                for (int nclump = clumps_per_line; nclump-- > 0; )
                                {
                                    buffer[outptr] = inBuf[inptr];
                                    inptr++;
                                    outptr += m_samplesperclump;
                                }
                            }
                            else
                            {
                                // general case
                                for (int nclump = clumps_per_line; nclump-- > 0; )
                                {
                                    for (int xpos = 0; xpos < hsamp; xpos++)
                                    {
                                        buffer[outptr + xpos] = inBuf[inptr];
                                        inptr++;
                                    }

                                    outptr += m_samplesperclump;
                                }
                            }

                            clumpoffset += hsamp;
                        }
                    }

                    ++m_scancount;
                    m_tif.m_row += m_v_sampling;

                    // increment/decrement of buffer and count is still incorrect, but should not matter
                    // TODO: resolve this
                    offset += m_bytesperline;
                    count -= m_bytesperline;
                    nrows -= m_v_sampling;
                }
                while (nrows > 0);
            }

            // Close down the decompressor if done.
            return m_decompression.Output_scanline < m_decompression.Output_height || TIFFjpeg_finish_decompress();
        }

        /// <summary>
        /// Encode a chunk of pixels.
        /// "Standard" case: incoming data is not downsampled.
        /// </summary>
        private bool JPEGEncode(byte[] buffer, int offset, int count, short plane)
        {
            // data is expected to be supplied in multiples of a scanline
            int nrows = count / m_bytesperline;
            if ((count % m_bytesperline) != 0)
                Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline discarded");

            // The last strip will be limited to image size
            if (!m_tif.IsTiled() && m_tif.m_row + nrows > m_tif.m_dir.td_imagelength)
                nrows = m_tif.m_dir.td_imagelength - m_tif.m_row;

            byte[][] bufptr = new byte[1][];
            bufptr[0] = new byte[m_bytesperline];
            while (nrows-- > 0)
            {
                Buffer.BlockCopy(buffer, offset, bufptr[0], 0, m_bytesperline);
                if (TIFFjpeg_write_scanlines(bufptr, 1) != 1)
                    return false;

                if (nrows > 0)
                    m_tif.m_row++;

                offset += m_bytesperline;
            }

            return true;
        }

        /// <summary>
        /// Encode a chunk of pixels.
        /// Incoming data is expected to be downsampled per sampling factors.
        /// </summary>
        private bool JPEGEncodeRaw(byte[] buffer, int offset, int count, short plane)
        {
            // data is expected to be supplied in multiples of a clumpline
            // a clumpline is equivalent to v_sampling desubsampled scanlines

            // TODO: the following calculation of bytesperclumpline, should substitute
            //       calculation of bytesperline, except that it is per v_sampling lines
            int bytesperclumpline = (((m_compression.Image_width + m_h_sampling - 1) / m_h_sampling) *
                (m_h_sampling * m_v_sampling + 2) * m_compression.Data_precision + 7) / 8;

            int nrows = (count / bytesperclumpline) * m_v_sampling;
            if ((count % bytesperclumpline) != 0)
                Tiff.WarningExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "fractional scanline discarded");

            // Cb,Cr both have sampling factors 1, so this is correct
            int clumps_per_line = m_compression.Component_info[1].Downsampled_width;

            while (nrows > 0)
            {
                // Fastest way to separate the data is to make one pass over the scanline for
                // each row of each component.
                int clumpoffset = 0; // first sample in clump
                for (int ci = 0; ci < m_compression.Num_components; ci++)
                {
                    JpegComponent compptr = m_compression.Component_info[ci];
                    int hsamp = compptr.H_samp_factor;
                    int vsamp = compptr.V_samp_factor;
                    int padding = compptr.Width_in_blocks * JpegConstants.DCTSize - clumps_per_line * hsamp;
                    for (int ypos = 0; ypos < vsamp; ypos++)
                    {
                        int inptr = offset + clumpoffset;

                        byte[] outbuf = m_ds_buffer[ci][m_scancount * vsamp + ypos];
                        int outptr = 0;

                        if (hsamp == 1)
                        {
                            // fast path for at least Cb and Cr
                            for (int nclump = clumps_per_line; nclump-- > 0; )
                            {
                                outbuf[outptr] = buffer[inptr];
                                outptr++;
                                inptr += m_samplesperclump;
                            }
                        }
                        else
                        {
                            // general case
                            for (int nclump = clumps_per_line; nclump-- > 0; )
                            {
                                for (int xpos = 0; xpos < hsamp; xpos++)
                                {
                                    outbuf[outptr] = buffer[inptr + xpos];
                                    outptr++;
                                }

                                inptr += m_samplesperclump;
                            }
                        }

                        // pad each scanline as needed
                        for (int xpos = 0; xpos < padding; xpos++)
                        {
                            outbuf[outptr] = outbuf[outptr - 1];
                            outptr++;
                        }

                        clumpoffset += hsamp;
                    }
                }

                m_scancount++;
                if (m_scancount >= JpegConstants.DCTSize)
                {
                    int n = m_compression.Max_v_samp_factor * JpegConstants.DCTSize;
                    if (TIFFjpeg_write_raw_data(m_ds_buffer, n) != n)
                        return false;

                    m_scancount = 0;
                }

                m_tif.m_row += m_v_sampling;
                offset += m_bytesperline;
                nrows -= m_v_sampling;
            }

            return true;
        }

        private int JPEGDefaultStripSize(int s)
        {
            s = base.DefStripSize(s);
            if (s < m_tif.m_dir.td_imagelength)
                s = Tiff.roundUp(s, m_tif.m_dir.td_ycbcrsubsampling[1] * JpegConstants.DCTSize);

            return s;
        }

        private void JPEGDefaultTileSize(ref int tw, ref int th)
        {
            base.DefTileSize(ref tw, ref th);
            tw = Tiff.roundUp(tw, m_tif.m_dir.td_ycbcrsubsampling[0] * JpegConstants.DCTSize);
            th = Tiff.roundUp(th, m_tif.m_dir.td_ycbcrsubsampling[1] * JpegConstants.DCTSize);
        }

        /*
        * Interface routines.  This layer of routines exists
        * primarily to limit side-effects from LibJpeg.Net exceptions.
        * Also, normal/error returns are converted into return
        * values per LibTiff.Net practice.
        */
        private bool TIFFjpeg_create_compress()
        {
            /* initialize JPEG error handling */
            try
            {
                m_compression = new JpegCompressor();
                m_common = m_compression;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_create_decompress()
        {
            /* initialize JPEG error handling */
            try
            {
                m_decompression = new JpegDecompressor();
                m_common = m_decompression;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_set_defaults()
        {
            try
            {
                m_compression.jpeg_set_defaults();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_set_colorspace(ColorSpace colorspace)
        {
            try
            {
                m_compression.jpeg_set_colorspace(colorspace);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_set_quality(int quality, bool force_baseline)
        {
            try
            {
                m_compression.jpeg_set_quality(quality, force_baseline);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_suppress_tables(bool suppress)
        {
            try
            {
                m_compression.jpeg_suppress_tables(suppress);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_start_compress(bool write_all_tables)
        {
            try
            {
                m_compression.jpeg_start_compress(write_all_tables);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private int TIFFjpeg_write_scanlines(byte[][] scanlines, int num_lines)
        {
            int n = 0;
            try
            {
                n = m_compression.jpeg_write_scanlines(scanlines, num_lines);
            }
            catch (Exception)
            {
                return -1;
            }

            return n;
        }

        private int TIFFjpeg_write_raw_data(byte[][][] data, int num_lines)
        {
            int n = 0;
            try
            {
                n = m_compression.jpeg_write_raw_data(data, num_lines);
            }
            catch (Exception)
            {
                return -1;
            }

            return n;
        }

        private bool TIFFjpeg_finish_compress()
        {
            try
            {
                m_compression.jpeg_finish_compress();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_write_tables()
        {
            try
            {
                m_compression.jpeg_write_tables();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private ReadResult TIFFjpeg_read_header(bool require_image)
        {
            ReadResult res = ReadResult.Suspended;
            try
            {
                res = m_decompression.jpeg_read_header(require_image);
            }
            catch (Exception)
            {
                return ReadResult.Suspended;
            }

            return res;
        }

        private bool TIFFjpeg_start_decompress()
        {
            try
            {
                m_decompression.jpeg_start_decompress();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private int TIFFjpeg_read_raw_data(byte[][][] data, int max_lines)
        {
            int n = 0;
            try
            {
                n = m_decompression.jpeg_read_raw_data(data, max_lines);
            }
            catch (Exception)
            {
                return -1;
            }

            return n;
        }

        private bool TIFFjpeg_finish_decompress()
        {
            bool res = true;
            try
            {
                res = m_decompression.jpeg_finish_decompress();
            }
            catch (Exception)
            {
                return false;
            }

            return res;
        }

        private bool TIFFjpeg_abort()
        {
            try
            {
                m_common.jpeg_abort();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private bool TIFFjpeg_destroy()
        {
            try
            {
                m_common.jpeg_destroy();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        private static byte[][] TIFFjpeg_alloc_sarray(int samplesperrow, int numrows)
        {
            byte[][] result = new byte[numrows][];
            for (int i = 0; i < numrows; i++)
                result[i] = new byte[samplesperrow];

            return result;
        }

        /*
        * Allocate downsampled-data buffers needed for downsampled I/O.
        * We use values computed in jpeg_start_compress or jpeg_start_decompress.
        * We use LibJpeg.Net's allocator so that buffers will be released automatically
        * when done with strip/tile.
        * This is also a handy place to compute samplesperclump, bytesperline.
        */
        private bool alloc_downsampled_buffers(JpegComponent[] comp_info, int num_components)
        {
            int samples_per_clump = 0;
            for (int ci = 0; ci < num_components; ci++)
            {
                JpegComponent compptr = comp_info[ci];
                samples_per_clump += compptr.H_samp_factor * compptr.V_samp_factor;

                byte[][] buf = TIFFjpeg_alloc_sarray(
                    compptr.Width_in_blocks * JpegConstants.DCTSize,
                    compptr.V_samp_factor * JpegConstants.DCTSize);
                m_ds_buffer[ci] = buf;
            }

            m_samplesperclump = samples_per_clump;
            return true;
        }

        private void unsuppress_quant_table(int tblno)
        {
            JpegQuantizationTable qtbl = m_compression.Quant_tbl_ptrs[tblno];
            if (qtbl != null)
                qtbl.Sent_table = false;
        }

        private void unsuppress_huff_table(int tblno)
        {
            JpegHuffmanTable htbl = m_compression.Dc_huff_tbl_ptrs[tblno];

            if (htbl != null)
                htbl.Sent_table = false;

            htbl = m_compression.Ac_huff_tbl_ptrs[tblno];
            if (htbl != null)
                htbl.Sent_table = false;
        }

        private void TIFFjpeg_data_dest()
        {
            m_compression.Dest = new JpegStdDestination(m_tif);
        }

        private bool TIFFjpeg_tables_dest()
        {
            /*
             * Allocate a working buffer for building tables.
             * Initial size is 1000 bytes, which is usually adequate.
             */
            m_jpegtables_length = 1000;
            m_jpegtables = new byte[m_jpegtables_length];
            m_compression.Dest = new JpegTablesDestination(this);
            return true;
        }
    }
}
