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
 * CCITT Group 3 (T.4) and Group 4 (T.6) Compression Support.
 *
 * This file contains support for decoding and encoding TIFF
 * compression algorithms 2, 3, 4, and 32771.
 *
 * Decoder support is derived, with permission, from the code
 * in Frank Cringle's viewfax program;
 *      Copyright (C) 1990, 1995  Frank D. Cringle.
 */

using System;
using System.Diagnostics;

namespace BitMiracle.LibTiff.Classic.Internal
{
    partial class CCITTCodec : TiffCodec
    {
        public const int FIELD_BADFAXLINES = (FieldBit.Codec + 0);
        public const int FIELD_CLEANFAXDATA = (FieldBit.Codec + 1);
        public const int FIELD_BADFAXRUN = (FieldBit.Codec + 2);
        public const int FIELD_RECVPARAMS = (FieldBit.Codec + 3);
        public const int FIELD_SUBADDRESS = (FieldBit.Codec + 4);
        public const int FIELD_RECVTIME = (FieldBit.Codec + 5);
        public const int FIELD_FAXDCS = (FieldBit.Codec + 6);
        public const int FIELD_OPTIONS = (FieldBit.Codec + 7);

        internal FaxMode m_mode; /* operating mode */
        internal Group3Opt m_groupoptions; /* Group 3/4 options tag */
        internal CleanFaxData m_cleanfaxdata; /* CleanFaxData tag */
        internal int m_badfaxlines; /* BadFaxLines tag */
        internal int m_badfaxrun; /* BadFaxRun tag */
        internal int m_recvparams; /* encoded Class 2 session params */
        internal string m_subaddress; /* subaddress string */
        internal int m_recvtime; /* time spent receiving (secs) */
        internal string m_faxdcs; /* Table 2/T.30 encoded session params */

        /* Decoder state info */
        internal Tiff.FaxFillFunc fill; /* fill routine */

        private const int EOL_CODE = 0x001;   /* EOL code value - 0000 0000 0000 1 */

        /* finite state machine codes */
        private const byte S_Null = 0;
        private const byte S_Pass = 1;
        private const byte S_Horiz = 2;
        private const byte S_V0 = 3;
        private const byte S_VR = 4;
        private const byte S_VL = 5;
        private const byte S_Ext = 6;
        private const byte S_TermW = 7;
        private const byte S_TermB = 8;
        private const byte S_MakeUpW = 9;
        private const byte S_MakeUpB = 10;
        private const byte S_MakeUp = 11;
        private const byte S_EOL = 12;

        /* status values returned instead of a run length */
        private const short G3CODE_EOL = -1;  /* NB: ACT_EOL - ACT_WRUNT */
        private const short G3CODE_INVALID = -2;  /* NB: ACT_INVALID - ACT_WRUNT */
        private const short G3CODE_EOF = -3;  /* end of input data */
        private const short G3CODE_INCOMP = -4;  /* incomplete run code */

        /*
        * CCITT T.4 1D Huffman runlength codes and
        * related definitions.  Given the small sizes
        * of these tables it does not seem
        * worthwhile to make code & length 8 bits.
        */
        private struct tableEntry
        {
            public short length; /* bit length of g3 code */
            public short code; /* g3 code */
            public short runlen; /* run length in bits */

            public tableEntry(short _length, short _code, short _runlen)
            {
                length = _length;
                code = _code;
                runlen = _runlen;
            }

            public static tableEntry FromArray(short[] array, int entryNumber)
            {
                int offset = entryNumber * 3; // we have 3 elements in entry
                return new tableEntry(array[offset], array[offset + 1], array[offset + 2]);
            }
        };

        private struct faxTableEntry
        {
            public faxTableEntry(byte _State, byte _Width, int _Param)
            {
                State = _State;
                Width = _Width;
                Param = _Param;
            }

            public static faxTableEntry FromArray(int[] array, int entryNumber)
            {
                int offset = entryNumber * 3; // we have 3 elements in entry
                return new faxTableEntry((byte)array[offset], (byte)array[offset + 1], array[offset + 2]);
            }

            /* state table entry */
            public byte State; /* see above */
            public byte Width; /* width of code in bits */
            public int Param; /* unsigned 32-bit run length in bits */
        };

        private enum Decoder
        {
            useFax3_1DDecoder,
            useFax3_2DDecoder,
            useFax4Decoder,
            useFax3RLEDecoder
        };
        
        private enum Fax3Encoder
        {
            useFax1DEncoder, 
            useFax2DEncoder
        };

        private static TiffFieldInfo[] m_faxFieldInfo =
        {
            new TiffFieldInfo(TiffTag.FAXMODE, 0, 0, TiffType.ANY, FieldBit.Pseudo, false, false, "FaxMode"), 
            new TiffFieldInfo(TiffTag.FAXFILLFUNC, 0, 0, TiffType.ANY, FieldBit.Pseudo, false, false, "FaxFillFunc"), 
            new TiffFieldInfo(TiffTag.BADFAXLINES, 1, 1, TiffType.LONG, FIELD_BADFAXLINES, true, false, "BadFaxLines"), 
            new TiffFieldInfo(TiffTag.BADFAXLINES, 1, 1, TiffType.SHORT, FIELD_BADFAXLINES, true, false, "BadFaxLines"), 
            new TiffFieldInfo(TiffTag.CLEANFAXDATA, 1, 1, TiffType.SHORT, FIELD_CLEANFAXDATA, true, false, "CleanFaxData"), 
            new TiffFieldInfo(TiffTag.CONSECUTIVEBADFAXLINES, 1, 1, TiffType.LONG, FIELD_BADFAXRUN, true, false, "ConsecutiveBadFaxLines"), 
            new TiffFieldInfo(TiffTag.CONSECUTIVEBADFAXLINES, 1, 1, TiffType.SHORT, FIELD_BADFAXRUN, true, false, "ConsecutiveBadFaxLines"), 
            new TiffFieldInfo(TiffTag.FAXRECVPARAMS, 1, 1, TiffType.LONG, FIELD_RECVPARAMS, true, false, "FaxRecvParams"), 
            new TiffFieldInfo(TiffTag.FAXSUBADDRESS, -1, -1, TiffType.ASCII, FIELD_SUBADDRESS, true, false, "FaxSubAddress"), 
            new TiffFieldInfo(TiffTag.FAXRECVTIME, 1, 1, TiffType.LONG, FIELD_RECVTIME, true, false, "FaxRecvTime"), 
            new TiffFieldInfo(TiffTag.FAXDCS, -1, -1, TiffType.ASCII, FIELD_FAXDCS, true, false, "FaxDcs"), 
        };
        
        private static TiffFieldInfo[] m_fax3FieldInfo = 
        {
            new TiffFieldInfo(TiffTag.GROUP3OPTIONS, 1, 1, TiffType.LONG, FIELD_OPTIONS, false, false, "Group3Options"), 
        };

        private static TiffFieldInfo[] m_fax4FieldInfo = 
        {
            new TiffFieldInfo(TiffTag.GROUP4OPTIONS, 1, 1, TiffType.LONG, FIELD_OPTIONS, false, false, "Group4Options"), 
        };

        private TiffTagMethods m_parentTagMethods;
        private TiffTagMethods m_tagMethods;

        private int m_rw_mode; /* O_RDONLY for decode, else encode */
        private int m_rowbytes; /* bytes in a decoded scanline */
        private int m_rowpixels; /* pixels in a scanline */

        /* Decoder state info */
        private Decoder m_decoder;
        private byte[] m_bitmap; /* bit reversal table */
        private int m_data; /* current i/o byte/word */
        private int m_bit; /* current i/o bit in byte */
        private int m_EOLcnt; /* count of EOL codes recognized */
        private int[] m_runs; /* b&w runs for current/previous row */
        private int m_refruns; /* runs for reference line (index in m_runs) */
        private int m_curruns; /* runs for current line (index in m_runs) */

        private int m_a0; /* reference element */
        private int m_RunLength; /* length of current run */
        private int m_thisrun; /* current row's run array (index in m_runs) */
        private int m_pa; /* place to stuff next run (index in m_runs) */
        private int m_pb; /* next run in reference line (index in m_runs) */

        /* Encoder state info */
        private Fax3Encoder m_encoder; /* encoding state */
        private bool m_encodingFax4; // if false, G3 will be used
        private byte[] m_refline; /* reference line for 2d decoding */
        private int m_k; /* #rows left that can be 2d encoded */
        private int m_maxk; /* max #rows that can be 2d encoded */
        private int m_line;

        private byte[] m_buffer; // buffer with data to encode
        private int m_offset;   // current position in m_buffer

        public CCITTCodec(Tiff tif, Compression scheme, string name)
            : base(tif, scheme, name)
        {
            m_tagMethods = new CCITTCodecTagMethods();
        }

        public override bool Init()
        {
            switch (m_scheme)
            {
                case Compression.CCITTRLE:
                    return TIFFInitCCITTRLE();
                case Compression.CCITTRLEW:
                    return TIFFInitCCITTRLEW();
                case Compression.CCITTFAX3:
                    return TIFFInitCCITTFax3();
                case Compression.CCITTFAX4:
                    return TIFFInitCCITTFax4();
            }

            return false;
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

        public override bool SetupDecode()
        {
            // same for all types
            return setupState();
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
            m_bit = 0; // force initial read
            m_data = 0;
            m_EOLcnt = 0; // force initial scan for EOL

            // Decoder assumes lsb-to-msb bit order. Note that we select this here rather than in
            // setupState so that viewers can hold the image open, fiddle with the FillOrder tag
            // value, and then re-decode the image. Otherwise they'd need to close and open the
            // image to get the state reset.
            m_bitmap = Tiff.GetBitRevTable(m_tif.m_dir.td_fillorder != FillOrder.LSB2MSB);
            if (m_refruns >= 0)
            {
                // init reference line to white
                m_runs[m_refruns] = m_rowpixels;
                m_runs[m_refruns + 1] = 0;
            }
            
            m_line = 0;
            return true;
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
            switch (m_decoder)
            {
                case Decoder.useFax3_1DDecoder:
                    return Fax3Decode1D(buffer, offset, count);
                case Decoder.useFax3_2DDecoder:
                    return Fax3Decode2D(buffer, offset, count);
                case Decoder.useFax4Decoder:
                    return Fax4Decode(buffer, offset, count);
                case Decoder.useFax3RLEDecoder:
                    return Fax3DecodeRLE(buffer, offset, count);
            }

            return false;
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
            return DecodeRow(buffer, offset, count, plane);
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
            return DecodeRow(buffer, offset, count, plane);
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
            // same for all types
            return setupState();
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
            m_bit = 8;
            m_data = 0;
            m_encoder = Fax3Encoder.useFax1DEncoder;

            /*
            * This is necessary for Group 4; otherwise it isn't
            * needed because the first scanline of each strip ends
            * up being copied into the refline.
            */
            if (m_refline != null)
                Array.Clear(m_refline, 0, m_refline.Length);

            if (is2DEncoding())
            {
                float res = m_tif.m_dir.td_yresolution;
                /*
                * The CCITT spec says that when doing 2d encoding, you
                * should only do it on K consecutive scanlines, where K
                * depends on the resolution of the image being encoded
                * (2 for <= 200 lpi, 4 for > 200 lpi).  Since the directory
                * code initializes td_yresolution to 0, this code will
                * select a K of 2 unless the YResolution tag is set
                * appropriately.  (Note also that we fudge a little here
                * and use 150 lpi to avoid problems with units conversion.)
                */
                if (m_tif.m_dir.td_resolutionunit == ResUnit.CENTIMETER)
                {
                    /* convert to inches */
                    res *= 2.54f;
                }

                m_maxk = (res > 150 ? 4 : 2);
                m_k = m_maxk - 1;
            }
            else
            {
                m_maxk = 0;
                m_k = 0;
            }

            m_line = 0;
            return true;
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
            if (m_encodingFax4)
                return Fax4PostEncode();

            return Fax3PostEncode();
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
            if (m_encodingFax4)
                return Fax4Encode(buffer, offset, count);

            return Fax3Encode(buffer, offset, count);
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
            return EncodeRow(buffer, offset, count, plane);
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
            return EncodeRow(buffer, offset, count, plane);
        }

        /// <summary>
        /// Flushes any internal data buffers and terminates current operation.
        /// </summary>
        public override void Close()
        {
            if ((m_mode & FaxMode.NORTC) == 0)
            {
                int code = EOL_CODE;
                int length = 12;
                if (is2DEncoding())
                {
                    bool b = ((code << 1) != 0) | (m_encoder == Fax3Encoder.useFax1DEncoder);
                    if (b)
                        code = 1;
                    else
                        code = 0;

                    length++;
                }

                for (int i = 0; i < 6; i++)
                    putBits(code, length);

                flushBits();
            }
        }

        /// <summary>
        /// Cleanups the state of the codec.
        /// </summary>
        /// <remarks>
        /// 	<b>Cleanup</b> is called when codec is no longer needed (won't be used) and can be
        /// used for example to restore tag methods that were substituted.</remarks>
        public override void Cleanup()
        {
            m_tif.m_tagmethods = m_parentTagMethods;
        }

        private bool is2DEncoding()
        {
            return (m_groupoptions & Group3Opt.ENCODING2D) != 0;
        }

        /*
        * Update the value of b1 using the array
        * of runs for the reference line.
        */
        private void CHECK_b1(ref int b1)
        {
            if (m_pa != m_thisrun)
            {
                while (b1 <= m_a0 && b1 < m_rowpixels)
                {
                    b1 += m_runs[m_pb] + m_runs[m_pb + 1];
                    m_pb += 2;
                }
            }
        }

        private static void SWAP(ref int a, ref int b)
        {
            int x = a;
            a = b;
            b = x;
        }

        private static bool isLongAligned(int offset)
        {
            return (offset % sizeof(int) == 0);
        }

        private static bool isShortAligned(int offset)
        {
            return (offset % sizeof(short) == 0);
        }

        /*
        * The FILL macro must handle spans < 2*sizeof(int) bytes.
        * This is <8 bytes.
        */
        private static void FILL(int n, byte[] cp, ref int offset, byte value)
        {
            const int max = 7;

            if (n <= max && n > 0)
            {
                for (int i = n; i > 0; i--)
                    cp[offset + i - 1] = value;

                offset += n;
            }
        }

        /*
        * Bit-fill a row according to the white/black
        * runs generated during G3/G4 decoding.
        * The default run filler; made public for other decoders.
        */
        private static void fax3FillRuns(byte[] buffer, int offset, int[] runs,
            int thisRunOffset, int nextRunOffset, int width)
        {
            if (((nextRunOffset - thisRunOffset) & 1) != 0)
            {
                runs[nextRunOffset] = 0;
                nextRunOffset++;
            }

            int x = 0;
            for (; thisRunOffset < nextRunOffset; thisRunOffset += 2)
            {
                int run = runs[thisRunOffset];

                // should cast 'run' to unsigned in order to discard values bigger than int.MaxValue
                // for such value 'run' become negative and following condition is not met
                if ((uint)x + (uint)run > (uint)width || (uint)run > (uint)width)
                {
                    runs[thisRunOffset] = width - x;
                    run = runs[thisRunOffset];
                }

                if (run != 0)
                {
                    int cp = offset + (x >> 3);
                    int bx = x & 7;
                    if (run > 8 - bx)
                    {
                        if (bx != 0)
                        {
                            // align to byte boundary
                            buffer[cp] &= (byte)(0xff << (8 - bx));
                            cp++;
                            run -= 8 - bx;
                        }

                        int n = run >> 3;
                        if (n != 0)
                        {
                            // multiple bytes to fill
                            if ((n / sizeof(int)) > 1)
                            {
                                // Align to longword boundary and fill.
                                for ( ; n != 0 && !isLongAligned(cp); n--)
                                {
                                    buffer[cp] = 0x00;
                                    cp++;
                                }

                                int bytesToFill = n - (n % sizeof(int));
                                n -= bytesToFill;
                                
                                int stop = bytesToFill + cp;
                                for ( ; cp < stop; cp++)
                                    buffer[cp] = 0;
                            }

                            FILL(n, buffer, ref cp, 0);
                            run &= 7;
                        }

                        if (run != 0)
                            buffer[cp] &= (byte)(0xff >> run);
                    }
                    else
                        buffer[cp] &= (byte)(~(fillMasks[run] >> bx));

                    x += runs[thisRunOffset];
                }

                run = runs[thisRunOffset + 1];

                // should cast 'run' to unsigned in order to discard values bigger than int.MaxValue
                // for such value 'run' become negative and following condition is not met
                if ((uint)x + (uint)run > (uint)width || (uint)run > (uint)width)
                {
                    runs[thisRunOffset + 1] = width - x;
                    run = runs[thisRunOffset + 1];
                }
                
                if (run != 0)
                {
                    int cp = offset + (x >> 3);
                    int bx = x & 7;
                    if (run > 8 - bx)
                    {
                        if (bx != 0)
                        {
                            // align to byte boundary
                            buffer[cp] |= (byte)(0xff >> bx);
                            cp++;
                            run -= 8 - bx;
                        }

                        int n = run >> 3;
                        if (n != 0)
                        {
                            // multiple bytes to fill
                            if ((n / sizeof(int)) > 1)
                            {
                                // Align to longword boundary and fill.
                                for ( ; n != 0 && !isLongAligned(cp); n--)
                                {
                                    buffer[cp] = 0xff;
                                    cp++;
                                }
                                
                                int bytesToFill = n - (n % sizeof(int));
                                n -= bytesToFill;
                                
                                int stop = bytesToFill + cp;
                                for ( ; cp < stop; cp++)
                                    buffer[cp] = 0xff;
                            }

                            FILL(n, buffer, ref cp, 0xff);
                            run &= 7;
                        }

                        if (run != 0)
                            buffer[cp] |= (byte)(0xff00 >> run);
                    }
                    else
                        buffer[cp] |= (byte)(fillMasks[run] >> bx);

                    x += runs[thisRunOffset + 1];
                }
            }

            Debug.Assert(x == width);
        }

        /*
        * Find a span of ones or zeros using the supplied
        * table.  The ``base'' of the bit string is supplied
        * along with the start+end bit indices.
        */
        private static int find0span(byte[] bp, int bpOffset, int bs, int be)
        {
            int offset = bpOffset + (bs >> 3);

            /*
             * Check partial byte on lhs.
             */
            int bits = be - bs;
            int n = bs & 7;
            int span = 0;
            if (bits > 0 && n != 0)
            {
                span = m_zeroruns[(bp[offset] << n) & 0xff];

                if (span > 8 - n)
                {
                    /* table value too generous */
                    span = 8 - n;
                }

                if (span > bits)
                {
                    /* constrain span to bit range */
                    span = bits;
                }

                if (n + span < 8)
                {
                    /* doesn't extend to edge of byte */
                    return span;
                }

                bits -= span;
                offset++;
            }

            if (bits >= (2 * 8 * sizeof(int)))
            {
                /*
                 * Align to longword boundary and check longwords.
                 */
                while (!isLongAligned(offset))
                {
                    if (bp[offset] != 0x00)
                        return (span + m_zeroruns[bp[offset]]);

                    span += 8;
                    bits -= 8;
                    offset++;
                }

                while (bits >= 8 * sizeof(int))
                {
                    bool allZeros = true;
                    for (int i = 0; i < sizeof(int); i++)
                    {
                        if (bp[offset + i] != 0)
                        {
                            allZeros = false;
                            break;
                        }
                    }

                    if (allZeros)
                    {
                        span += 8 * sizeof(int);
                        bits -= 8 * sizeof(int);
                        offset += sizeof(int);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            /*
             * Scan full bytes for all 0's.
             */
            while (bits >= 8)
            {
                if (bp[offset] != 0x00)
                {
                    /* end of run */
                    return (span + m_zeroruns[bp[offset]]);
                }

                span += 8;
                bits -= 8;
                offset++;
            }

            /*
             * Check partial byte on rhs.
             */
            if (bits > 0)
            {
                n = m_zeroruns[bp[offset]];
                span += (n > bits ? bits : n);
            }

            return span;
        }

        private static int find1span(byte[] bp, int bpOffset, int bs, int be)
        {
            int offset = bpOffset + (bs >> 3);

            /*
             * Check partial byte on lhs.
             */
            int n = bs & 7;
            int span = 0;
            int bits = be - bs;
            if (bits > 0 && n != 0)
            {
                span = m_oneruns[(bp[offset] << n) & 0xff];
                if (span > 8 - n)
                {
                    /* table value too generous */
                    span = 8 - n;
                }

                if (span > bits)
                {
                    /* constrain span to bit range */
                    span = bits;
                }

                if (n + span < 8)
                {
                    /* doesn't extend to edge of byte */
                    return (span);
                }

                bits -= span;
                offset++;
            }

            if (bits >= (2 * 8 * sizeof(int)))
            {
                /*
                 * Align to longword boundary and check longwords.
                 */
                while (!isLongAligned(offset))
                {
                    if (bp[offset] != 0xff)
                        return (span + m_oneruns[bp[offset]]);

                    span += 8;
                    bits -= 8;
                    offset++;
                }

                while (bits >= 8 * sizeof(int))
                {
                    bool allOnes = true;
                    for (int i = 0; i < sizeof(int); i++)
                    {
                        if (bp[offset + i] != 0xff)
                        {
                            allOnes = false;
                            break;
                        }
                    }

                    if (allOnes)
                    {
                        span += 8 * sizeof(int);
                        bits -= 8 * sizeof(int);
                        offset += sizeof(int);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            /*
             * Scan full bytes for all 1's.
             */
            while (bits >= 8)
            {
                if (bp[offset] != 0xff)
                {
                    /* end of run */
                    return (span + m_oneruns[bp[offset]]);
                }

                span += 8;
                bits -= 8;
                offset++;
            }

            /*
             * Check partial byte on rhs.
             */
            if (bits > 0)
            {
                n = m_oneruns[bp[offset]];
                span += (n > bits ? bits : n);
            }

            return span;
        }

        /*
        * Return the offset of the next bit in the range
        * [bs..be] that is different from the specified
        * color.  The end, be, is returned if no such bit
        * exists.
        */
        private static int finddiff(byte[] bp, int bpOffset, int _bs, int _be, int _color)
        {
            if (_color != 0)
                return (_bs + find1span(bp, bpOffset, _bs, _be));

            return (_bs + find0span(bp, bpOffset, _bs, _be));
        }

        /*
        * Like finddiff, but also check the starting bit
        * against the end in case start > end.
        */
        private static int finddiff2(byte[] bp, int bpOffset, int _bs, int _be, int _color)
        {
            if (_bs < _be)
                return finddiff(bp, bpOffset, _bs, _be, _color);

            return _be;
        }

        /*
        * Group 3 and Group 4 Decoding.
        */

        /*
        * The following macros define the majority of the G3/G4 decoder
        * algorithm using the state tables defined elsewhere.  To build
        * a decoder you need some setup code and some glue code. Note
        * that you may also need/want to change the way the NeedBits*
        * macros get input data if, for example, you know the data to be
        * decoded is properly aligned and oriented (doing so before running
        * the decoder can be a big performance win).
        *
        * Consult the decoder in the TIFF library for an idea of what you
        * need to define and setup to make use of these definitions.
        *
        * NB: to enable a debugging version of these macros define FAX3_DEBUG
        *     before including this file.  Trace output goes to stdout.
        */

        private bool EndOfData()
        {
            return (m_tif.m_rawcp >= m_tif.m_rawcc);
        }

        private int GetBits(int n)
        {
            return (m_data & ((1 << n) - 1));
        }

        private void ClrBits(int n)
        {
            m_bit -= n;
            m_data >>= n;
        }

        /*
        * Need <=8 or <=16 bits of input data.  Unlike viewfax we
        * cannot use/assume a word-aligned, properly bit swizzled
        * input data set because data may come from an arbitrarily
        * aligned, read-only source such as a memory-mapped file.
        * Note also that the viewfax decoder does not check for
        * running off the end of the input data buffer.  This is
        * possible for G3-encoded data because it prescans the input
        * data to count EOL markers, but can cause problems for G4
        * data.  In any event, we don't prescan and must watch for
        * running out of data since we can't permit the library to
        * scan past the end of the input data buffer.
        *
        * Finally, note that we must handle remaindered data at the end
        * of a strip specially.  The coder asks for a fixed number of
        * bits when scanning for the next code.  This may be more bits
        * than are actually present in the data stream.  If we appear
        * to run out of data but still have some number of valid bits
        * remaining then we makeup the requested amount with zeros and
        * return successfully.  If the returned data is incorrect then
        * we should be called again and get a premature EOF error;
        * otherwise we should get the right answer.
        */
        private bool NeedBits8(int n)
        {
            if (m_bit < n)
            {
                if (EndOfData())
                {
                    if (m_bit == 0)
                    {
                        /* no valid bits */
                        return false;
                    }

                    m_bit = n; /* pad with zeros */
                }
                else
                {
                    m_data |= m_bitmap[m_tif.m_rawdata[m_tif.m_rawcp]] << m_bit;
                    m_tif.m_rawcp++;
                    m_bit += 8;
                }
            }

            return true;
        }

        private bool NeedBits16(int n)
        {
            if (m_bit < n)
            {
                if (EndOfData())
                {
                    if (m_bit == 0)
                    {
                        /* no valid bits */
                        return false;
                    }

                    m_bit = n; /* pad with zeros */
                }
                else
                {
                    m_data |= m_bitmap[m_tif.m_rawdata[m_tif.m_rawcp]] << m_bit;
                    m_tif.m_rawcp++;
                    m_bit += 8;
                    if (m_bit < n)
                    {
                        if (EndOfData())
                        {
                            /* NB: we know BitsAvail is non-zero here */
                            m_bit = n; /* pad with zeros */
                        }
                        else
                        {
                            m_data |= m_bitmap[m_tif.m_rawdata[m_tif.m_rawcp]] << m_bit;
                            m_tif.m_rawcp++;
                            m_bit += 8;
                        }
                    }
                }
            }

            return true;
        }

        private bool LOOKUP8(out faxTableEntry TabEnt, int wid)
        {
            if (!NeedBits8(wid))
            {
                TabEnt = new faxTableEntry();
                return false;
            }

            TabEnt = faxTableEntry.FromArray(m_faxMainTable, GetBits(wid));
            ClrBits(TabEnt.Width);

            return true;
        }

        private bool LOOKUP16(out faxTableEntry TabEnt, int wid, bool useBlack)
        {
            if (!NeedBits16(wid))
            {
                TabEnt = new faxTableEntry();
                return false;
            }

            if (useBlack)
                TabEnt = faxTableEntry.FromArray(m_faxBlackTable, GetBits(wid));
            else
                TabEnt = faxTableEntry.FromArray(m_faxWhiteTable, GetBits(wid));

            ClrBits(TabEnt.Width);

            return true;
        }

        /*
        * Synchronize input decoding at the start of each
        * row by scanning for an EOL (if appropriate) and
        * skipping any trash data that might be present
        * after a decoding error.  Note that the decoding
        * done elsewhere that recognizes an EOL only consumes
        * 11 consecutive zero bits.  This means that if EOLcnt
        * is non-zero then we still need to scan for the final flag
        * bit that is part of the EOL code.
        */
        private bool SYNC_EOL()
        {
            if (m_EOLcnt == 0)
            {
                for ( ; ; )
                {
                    if (!NeedBits16(11))
                        return false;

                    if (GetBits(11) == 0)
                        break;

                    ClrBits(1);
                }
            }

            for ( ; ; )
            {
                if (!NeedBits8(8))
                    return false;

                if (GetBits(8) != 0)
                    break;

                ClrBits(8);
            }

            while (GetBits(1) == 0)
                ClrBits(1);

            ClrBits(1); /* EOL bit */
            m_EOLcnt = 0; /* reset EOL counter/flag */

            return true;
        }

        /*
        * Setup G3/G4-related compression/decompression state
        * before data is processed.  This routine is called once
        * per image -- it sets up different state based on whether
        * or not decoding or encoding is being done and whether
        * 1D- or 2D-encoded data is involved.
        */
        private bool setupState()
        {
            if (m_tif.m_dir.td_bitspersample != 1)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "Bits/sample must be 1 for Group 3/4 encoding/decoding");
                return false;
            }

            /*
             * Calculate the scanline/tile widths.
             */
            int rowbytes = 0;
            int rowpixels = 0;
            if (m_tif.IsTiled())
            {
                rowbytes = m_tif.TileRowSize();
                rowpixels = m_tif.m_dir.td_tilewidth;
            }
            else
            {
                rowbytes = m_tif.ScanlineSize();
                rowpixels = m_tif.m_dir.td_imagewidth;
            }
            
            m_rowbytes = rowbytes;
            m_rowpixels = rowpixels;
            
            /*
             * Allocate any additional space required for decoding/encoding.
             */
            bool needsRefLine = ((m_groupoptions & Group3Opt.ENCODING2D) != 0 ||
                m_tif.m_dir.td_compression == Compression.CCITTFAX4);

            // Assure that allocation computations do not overflow.
            m_runs = null;
            int nruns = Tiff.roundUp(rowpixels, 32);
            if (needsRefLine)
            {
                long multiplied = (long)nruns * 2;
                if (multiplied > int.MaxValue)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "Row pixels integer overflow (rowpixels {0})", rowpixels);
                    return false;
                }
                else
                {
                    nruns = (int)multiplied;
                }
            }

            if (nruns == 0 || ((long)nruns * 2) > int.MaxValue)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "Row pixels integer overflow (rowpixels {0})", rowpixels);
                return false;
            }

            m_runs = new int[2 * nruns];
            m_curruns = 0;

            if (needsRefLine)
                m_refruns = nruns;
            else
                m_refruns = -1;
            
            if (m_tif.m_dir.td_compression == Compression.CCITTFAX3 && is2DEncoding())
            {
                /* NB: default is 1D routine */
                m_decoder = Decoder.useFax3_2DDecoder;
            }

            if (needsRefLine)
            {
                /* 2d encoding */
                /*
                 * 2d encoding requires a scanline
                 * buffer for the "reference line"; the
                 * scanline against which delta encoding
                 * is referenced.  The reference line must
                 * be initialized to be "white" (done elsewhere).
                 */
                m_refline = new byte [rowbytes + 1];
            }
            else
            {
                /* 1d encoding */
                m_refline = null;
            }

            return true;
        }

        /*
        * Routine for handling various errors/conditions.
        * Note how they are "glued into the decoder" by
        * overriding the definitions used by the decoder.
        */
        private void Fax3Unexpected(string module)
        {
            Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                "{0}: Bad code word at line {1} of {2} {3} (x {4})", 
                m_tif.m_name, m_line, m_tif.IsTiled() ? "tile" : "strip", 
                (m_tif.IsTiled() ? m_tif.m_curtile : m_tif.m_curstrip), m_a0);
        }

        private void Fax3Extension(string module)
        {
            Tiff.ErrorExt(m_tif, m_tif.m_clientdata, module,
                "{0}: Uncompressed data (not supported) at line {1} of {2} {3} (x {4})",
                m_tif.m_name, m_line, m_tif.IsTiled() ? "tile" : "strip", 
                (m_tif.IsTiled() ? m_tif.m_curtile : m_tif.m_curstrip), m_a0);
        }

        private void Fax3BadLength(string module)
        {
            Tiff.WarningExt(m_tif, m_tif.m_clientdata, module,
                "{0}: {1} at line {2} of {3} {4} (got {5}, expected {6})",
                m_tif.m_name, m_a0 < m_rowpixels ? "Premature EOL" : "Line length mismatch",
                m_line, m_tif.IsTiled() ? "tile" : "strip", 
                (m_tif.IsTiled() ? m_tif.m_curtile : m_tif.m_curstrip), m_a0, m_rowpixels);
        }

        private void Fax3PrematureEOF(string module)
        {
            Tiff.WarningExt(m_tif, m_tif.m_clientdata, module,
                "{0}: Premature EOF at line {1} of {2} {3} (x {4})",
                m_tif.m_name, m_line, m_tif.IsTiled() ? "tile" : "strip", 
                (m_tif.IsTiled() ? m_tif.m_curtile : m_tif.m_curstrip), m_a0);
        }

        /// <summary>
        /// Decode the requested amount of G3 1D-encoded data.
        /// </summary>
        private bool Fax3Decode1D(byte[] buffer, int offset, int count)
        {
            const string module = "Fax3Decode1D";
    
            // current row's run array
            m_thisrun = m_curruns;
            while (count > 0)
            {
                m_a0 = 0;
                m_RunLength = 0;
                m_pa = m_thisrun;

                if (!SYNC_EOL())
                {
                    // premature EOF
                    CLEANUP_RUNS(module);
                }
                else
                {
                    bool expandSucceeded = EXPAND1D(module);
                    if (expandSucceeded)
                    {
                        fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                        offset += m_rowbytes;
                        count -= m_rowbytes;
                        m_line++;
                        continue;
                    }
                }

                // premature EOF
                fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decode the requested amount of G3 2D-encoded data.
        /// </summary>
        private bool Fax3Decode2D(byte[] buffer, int offset, int count)
        {
            const string module = "Fax3Decode2D";

            while (count > 0)
            {
                m_a0 = 0;
                m_RunLength = 0;
                m_pa = m_curruns;
                m_thisrun = m_curruns;

                bool prematureEOF = false;
                if (!SYNC_EOL())
                    prematureEOF = true;

                if (!prematureEOF && !NeedBits8(1))
                    prematureEOF = true;

                if (!prematureEOF)
                {
                    int is1D = GetBits(1); // 1D/2D-encoding tag bit
                    ClrBits(1);
                    m_pb = m_refruns;
                    int b1 = m_runs[m_pb];
                    m_pb++; // next change on prev line

                    bool expandSucceeded = false;
                    if (is1D != 0)
                        expandSucceeded = EXPAND1D(module);
                    else
                        expandSucceeded = EXPAND2D(module, b1);

                    if (expandSucceeded)
                    {
                        fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                        SETVALUE(0); // imaginary change for reference
                        SWAP(ref m_curruns, ref m_refruns);
                        offset += m_rowbytes;
                        count -= m_rowbytes;
                        m_line++;
                        continue;
                    }
                }
                else
                {
                    // premature EOF
                    CLEANUP_RUNS(module);
                }

                // premature EOF
                fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                return false;
            }

            return true;
        }

        /*
        * 1d-encode a row of pixels.  The encoding is
        * a sequence of all-white or all-black spans
        * of pixels encoded with Huffman codes.
        */
        private bool Fax3Encode1DRow()
        {
            int bs = 0;
            for ( ; ; )
            {
                int span = find0span(m_buffer, m_offset, bs, m_rowpixels); /* white span */
                putspan(span, false);
                bs += span;
                if (bs >= m_rowpixels)
                    break;

                span = find1span(m_buffer, m_offset, bs, m_rowpixels); /* black span */
                putspan(span, true);
                bs += span;
                if (bs >= m_rowpixels)
                    break;
            }

            if ((m_mode & (FaxMode.BYTEALIGN | FaxMode.WORDALIGN)) != 0)
            {
                if (m_bit != 8)
                {
                    /* byte-align */
                    flushBits();
                }

                if ((m_mode & FaxMode.WORDALIGN) != 0 && !isShortAligned(m_tif.m_rawcp))
                    flushBits();
            }

            return true;
        }

        /*
        * 2d-encode a row of pixels.  Consult the CCITT
        * documentation for the algorithm.
        */
        private bool Fax3Encode2DRow()
        {
            int a0 = 0;
            int a1 = (Fax3Encode2DRow_Pixel(m_buffer, m_offset, 0) != 0 ? 0 : finddiff(m_buffer, m_offset, 0, m_rowpixels, 0));
            int b1 = (Fax3Encode2DRow_Pixel(m_refline, 0, 0) != 0 ? 0 : finddiff(m_refline, 0, 0, m_rowpixels, 0));

            for (; ; )
            {
                int b2 = finddiff2(m_refline, 0, b1, m_rowpixels, Fax3Encode2DRow_Pixel(m_refline, 0, b1));
                if (b2 >= a1)
                {
                    int d = b1 - a1;
                    if (!(-3 <= d && d <= 3))
                    {
                        /* horizontal mode */
                        int a2 = finddiff2(m_buffer, m_offset, a1, m_rowpixels, Fax3Encode2DRow_Pixel(m_buffer, m_offset, a1));
                        putcode(m_horizcode);

                        if (a0 + a1 == 0 || Fax3Encode2DRow_Pixel(m_buffer, m_offset, a0) == 0)
                        {
                            putspan(a1 - a0, false);
                            putspan(a2 - a1, true);
                        }
                        else
                        {
                            putspan(a1 - a0, true);
                            putspan(a2 - a1, false);
                        }

                        a0 = a2;
                    }
                    else
                    {
                        /* vertical mode */
                        putcode(m_vcodes[d + 3]);
                        a0 = a1;
                    }
                }
                else
                {
                    /* pass mode */
                    putcode(m_passcode);
                    a0 = b2;
                }

                if (a0 >= m_rowpixels)
                    break;

                a1 = finddiff(m_buffer, m_offset, a0, m_rowpixels, Fax3Encode2DRow_Pixel(m_buffer, m_offset, a0));

                int color = Fax3Encode2DRow_Pixel(m_buffer, m_offset, a0);
                if (color == 0)
                    color = 1;
                else
                    color = 0;

                b1 = finddiff(m_refline, 0, a0, m_rowpixels, color);
                b1 = finddiff(m_refline, 0, b1, m_rowpixels, Fax3Encode2DRow_Pixel(m_buffer, m_offset, a0));
            }

            return true;
        }

        private static int Fax3Encode2DRow_Pixel(byte[] buf, int bufOffset, int ix)
        {
            // some images caused out-of-bounds exception here. not sure why. maybe the images are
            // malformed or implementation is buggy. original libtiff does not produce exceptions
            // here. it's just read after the end of the buffer.

            // it's a fast fix (use last byte when requested any byte beyond buffer end) for
            // the problem that possibly should be reviewed.
            // (it's weird but produced output is byte-to-byte equal to libtiff's one)
            return (((buf[Math.Min(bufOffset + (ix >> 3), buf.Length - 1)]) >> (7 - (ix & 7))) & 1);
        }

        /// <summary>
        /// Encode a buffer of pixels.
        /// </summary>
        private bool Fax3Encode(byte[] buffer, int offset, int count)
        {
            m_buffer = buffer;
            m_offset = offset;

            while (count > 0)
            {
                if ((m_mode & FaxMode.NOEOL) == 0)
                    Fax3PutEOL();

                if (is2DEncoding())
                {
                    if (m_encoder == Fax3Encoder.useFax1DEncoder)
                    {
                        if (!Fax3Encode1DRow())
                            return false;

                        m_encoder = Fax3Encoder.useFax2DEncoder;
                    }
                    else
                    {
                        if (!Fax3Encode2DRow())
                            return false;

                        m_k--;
                    }

                    if (m_k == 0)
                    {
                        m_encoder = Fax3Encoder.useFax1DEncoder;
                        m_k = m_maxk - 1;
                    }
                    else
                    {
                        Buffer.BlockCopy(m_buffer, m_offset, m_refline, 0, m_rowbytes);
                    }
                }
                else
                {
                    if (!Fax3Encode1DRow())
                        return false;
                }

                m_offset += m_rowbytes;
                count -= m_rowbytes;
            }

            return true;
        }

        private bool Fax3PostEncode()
        {
            if (m_bit != 8)
                flushBits();

            return true;
        }

        private void InitCCITTFax3()
        {
            /*
            * Merge codec-specific tag information and
            * override parent get/set field methods.
            */
            m_tif.MergeFieldInfo(m_faxFieldInfo, m_faxFieldInfo.Length);

            /*
             * Allocate state block so tag methods have storage to record values.
             */
            m_rw_mode = m_tif.m_mode;

            m_parentTagMethods = m_tif.m_tagmethods;
            m_tif.m_tagmethods = m_tagMethods;
            
            m_groupoptions = 0;
            m_recvparams = 0;
            m_subaddress = null;
            m_faxdcs = null;

            if (m_rw_mode == Tiff.O_RDONLY)
            {
                // FIXME: improve for in place update
                m_tif.m_flags |= TiffFlags.NOBITREV;
                // decoder does bit reversal
            }

            m_runs = null;
            m_tif.SetField(TiffTag.FAXFILLFUNC, new Tiff.FaxFillFunc(fax3FillRuns));
            m_refline = null;

            m_decoder = Decoder.useFax3_1DDecoder;
            m_encodingFax4 = false;
        }

        private bool TIFFInitCCITTFax3()
        {
            InitCCITTFax3();
            m_tif.MergeFieldInfo(m_fax3FieldInfo, m_fax3FieldInfo.Length);

            /*
             * The default format is Class/F-style w/o RTC.
             */
            return m_tif.SetField(TiffTag.FAXMODE, FaxMode.CLASSF);
        }

        /*
        * CCITT Group 3 FAX Encoding.
        */
        private void flushBits()
        {
            if (m_tif.m_rawcc >= m_tif.m_rawdatasize)
                m_tif.flushData1();

            m_tif.m_rawdata[m_tif.m_rawcp] = (byte)m_data;
            m_tif.m_rawcp++;
            m_tif.m_rawcc++;
            m_data = 0;
            m_bit = 8;
        }
        
        /*
        * Write a variable-length bit-value to
        * the output stream.  Values are
        * assumed to be at most 16 bits.
        */
        private void putBits(int bits, int length)
        {
            while (length > m_bit)
            {
                m_data |= bits >> (length - m_bit);
                length -= m_bit;
                flushBits();
            }

            m_data |= (bits & m_msbmask[length]) << (m_bit - length);
            m_bit -= length;
            if (m_bit == 0)
                flushBits();
        }
        
        /*
        * Write a code to the output stream.
        */
        private void putcode(tableEntry te)
        {
            putBits(te.code, te.length);
        }

        /*
        * Write the sequence of codes that describes
        * the specified span of zero's or one's.  The
        * appropriate table that holds the make-up and
        * terminating codes is supplied.
        */
        private void putspan(int span, bool useBlack)
        {
            short[] entries = null;
            if (useBlack)
                entries = m_faxBlackCodes;
            else
                entries = m_faxWhiteCodes;

            tableEntry te = tableEntry.FromArray(entries, 63 + (2560 >> 6));
            while (span >= 2624)
            {
                putBits(te.code, te.length);
                span -= te.runlen;
            }

            if (span >= 64)
            {
                te = tableEntry.FromArray(entries, 63 + (span >> 6));
                Debug.Assert(te.runlen == 64 * (span >> 6));
                putBits(te.code, te.length);
                span -= te.runlen;
            }

            te = tableEntry.FromArray(entries, span);
            putBits(te.code, te.length);
        }

        /*
        * Write an EOL code to the output stream.  The zero-fill
        * logic for byte-aligning encoded scanlines is handled
        * here.  We also handle writing the tag bit for the next
        * scanline when doing 2d encoding.
        */
        private void Fax3PutEOL()
        {
            if ((m_groupoptions & Group3Opt.FILLBITS) != 0)
            {
                /*
                 * Force bit alignment so EOL will terminate on
                 * a byte boundary.  That is, force the bit alignment
                 * to 16-12 = 4 before putting out the EOL code.
                 */
                int align = 8 - 4;
                if (align != m_bit)
                {
                    if (align > m_bit)
                        align = m_bit + (8 - align);
                    else
                        align = m_bit - align;

                    putBits(0, align);
                }
            }

            int code = EOL_CODE;
            int length = 12;
            if (is2DEncoding())
            {
                code = (code << 1);
                if (m_encoder == Fax3Encoder.useFax1DEncoder)
                    code++;

                length++;
            }
            
            putBits(code, length);
        }

        /*
        * Append a run to the run length array for the
        * current row and reset decoding state.
        */
        private void SETVALUE(int x)
        {
            m_runs[m_pa] = m_RunLength + x;
            m_pa++;
            m_a0 += x;
            m_RunLength = 0;
        }

        /*
        * Cleanup the array of runs after decoding a row.
        * We adjust final runs to insure the user buffer is not
        * overwritten and/or undecoded area is white filled.
        */
        private void CLEANUP_RUNS(string module)
        {
            if (m_RunLength != 0)
                SETVALUE(0);

            if (m_a0 != m_rowpixels)
            {
                Fax3BadLength(module);

                while (m_a0 > m_rowpixels && m_pa > m_thisrun)
                {
                    m_pa--;
                    m_a0 -= m_runs[m_pa];
                }

                if (m_a0 < m_rowpixels)
                {
                    if (m_a0 < 0)
                        m_a0 = 0;

                    if (((m_pa - m_thisrun) & 1) != 0)
                        SETVALUE(0);

                    SETVALUE(m_rowpixels - m_a0);
                }
                else if (m_a0 > m_rowpixels)
                {
                    SETVALUE(m_rowpixels);
                    SETVALUE(0);
                }
            }
        }

        private void handlePrematureEOFinExpand2D(string module)
        {
            Fax3PrematureEOF(module);
            CLEANUP_RUNS(module);
        }

        /*
        * Decode a line of 1D-encoded data.
        */
        private bool EXPAND1D(string module)
        {
            faxTableEntry TabEnt;
            bool decodingDone = false;
            bool whiteDecodingDone = false;
            bool blackDecodingDone = false;

            for ( ; ; )
            {
                for ( ; ; )
                {
                    if (!LOOKUP16(out TabEnt, 12, false))
                    {
                        Fax3PrematureEOF(module);
                        CLEANUP_RUNS(module);
                        return false;
                    }

                    switch (TabEnt.State)
                    {
                        case S_EOL:
                            m_rowpixels = 1;
                            decodingDone = true;
                            break;

                        case S_TermW:
                            SETVALUE(TabEnt.Param);
                            whiteDecodingDone = true;
                            break;

                        case S_MakeUpW:
                        case S_MakeUp:
                            m_a0 += TabEnt.Param;
                            m_RunLength += TabEnt.Param;
                            break;

                        default:
                            /* "WhiteTable" */
                            Fax3Unexpected(module);
                            decodingDone = true;
                            break;
                    }

                    if (decodingDone || whiteDecodingDone)
                        break;
                }

                if (decodingDone)
                    break;

                if (m_a0 >= m_rowpixels)
                    break;

                for ( ; ; )
                {
                    if (!LOOKUP16(out TabEnt, 13, true))
                    {
                        Fax3PrematureEOF(module);
                        CLEANUP_RUNS(module);
                        return false;
                    }

                    switch (TabEnt.State)
                    {
                        case S_EOL:
                            m_EOLcnt = 1;
                            decodingDone = true;
                            break;

                        case S_TermB:
                            SETVALUE(TabEnt.Param);
                            blackDecodingDone = true;
                            break;

                        case S_MakeUpB:
                        case S_MakeUp:
                            m_a0 += TabEnt.Param;
                            m_RunLength += TabEnt.Param;
                            break;

                        default:
                            /* "BlackTable" */
                            Fax3Unexpected(module);
                            decodingDone = true;
                            break;
                    }

                    if (decodingDone || blackDecodingDone)
                        break;
                }

                if (decodingDone)
                    break;

                if (m_a0 >= m_rowpixels)
                    break;

                if (m_runs[m_pa - 1] == 0 && m_runs[m_pa - 2] == 0)
                    m_pa -= 2;

                whiteDecodingDone = false;
                blackDecodingDone = false;
            }

            CLEANUP_RUNS(module);
            return true;
        }

        /*
        * Expand a row of 2D-encoded data.
        */
        private bool EXPAND2D(string module, int b1)
        {
            faxTableEntry TabEnt;
            bool decodingDone = false;

            while (m_a0 < m_rowpixels)
            {
                if (!LOOKUP8(out TabEnt, 7))
                {
                    handlePrematureEOFinExpand2D(module);
                    return false;
                }

                switch (TabEnt.State)
                {
                    case S_Pass:
                        CHECK_b1(ref b1);
                        b1 += m_runs[m_pb];
                        m_pb++;
                        m_RunLength += b1 - m_a0;
                        m_a0 = b1;
                        b1 += m_runs[m_pb];
                        m_pb++;
                        break;

                    case S_Horiz:
                        if (((m_pa - m_thisrun) & 1) != 0)
                        {
                            for ( ; ; )
                            {
                                /* black first */
                                if (!LOOKUP16(out TabEnt, 13, true))
                                {
                                    handlePrematureEOFinExpand2D(module);
                                    return false;
                                }

                                bool doneWhite2d = false;
                                switch (TabEnt.State)
                                {
                                    case S_TermB:
                                        SETVALUE(TabEnt.Param);
                                        doneWhite2d = true;
                                        break;

                                    case S_MakeUpB:
                                    case S_MakeUp:
                                        m_a0 += TabEnt.Param;
                                        m_RunLength += TabEnt.Param;
                                        break;

                                    default:
                                        /* "BlackTable" */
                                        Fax3Unexpected(module);
                                        decodingDone = true;
                                        break;
                                }

                                if (doneWhite2d || decodingDone)
                                    break;
                            }

                            if (decodingDone)
                                break;

                            for ( ; ; )
                            {
                                /* then white */
                                if (!LOOKUP16(out TabEnt, 12, false))
                                {
                                    handlePrematureEOFinExpand2D(module);
                                    return false;
                                }

                                bool doneBlack2d = false;
                                switch (TabEnt.State)
                                {
                                    case S_TermW:
                                        SETVALUE(TabEnt.Param);
                                        doneBlack2d = true;
                                        break;

                                    case S_MakeUpW:
                                    case S_MakeUp:
                                        m_a0 += TabEnt.Param;
                                        m_RunLength += TabEnt.Param;
                                        break;

                                    default:
                                        /* "WhiteTable" */
                                        Fax3Unexpected(module);
                                        decodingDone = true;
                                        break;
                                }

                                if (doneBlack2d || decodingDone)
                                    break;
                            }

                            if (decodingDone)
                                break;
                        }
                        else
                        {
                            for ( ; ; )
                            {
                                /* white first */
                                if (!LOOKUP16(out TabEnt, 12, false))
                                {
                                    handlePrematureEOFinExpand2D(module);
                                    return false;
                                }

                                bool doneWhite2d = false;
                                switch (TabEnt.State)
                                {
                                    case S_TermW:
                                        SETVALUE(TabEnt.Param);
                                        doneWhite2d = true;
                                        break;

                                    case S_MakeUpW:
                                    case S_MakeUp:
                                        m_a0 += TabEnt.Param;
                                        m_RunLength += TabEnt.Param;
                                        break;

                                    default:
                                        /* "WhiteTable" */
                                        Fax3Unexpected(module);
                                        decodingDone = true;
                                        break;
                                }

                                if (doneWhite2d || decodingDone)
                                    break;
                            }

                            if (decodingDone)
                                break;

                            for ( ; ; )
                            {
                                /* then black */
                                if (!LOOKUP16(out TabEnt, 13, true))
                                {
                                    handlePrematureEOFinExpand2D(module);
                                    return false;
                                }

                                bool doneBlack2d = false;
                                switch (TabEnt.State)
                                {
                                    case S_TermB:
                                        SETVALUE(TabEnt.Param);
                                        doneBlack2d = true;
                                        break;

                                    case S_MakeUpB:
                                    case S_MakeUp:
                                        m_a0 += TabEnt.Param;
                                        m_RunLength += TabEnt.Param;
                                        break;

                                    default:
                                        /* "BlackTable" */
                                        Fax3Unexpected(module);
                                        decodingDone = true;
                                        break;
                                }

                                if (doneBlack2d || decodingDone)
                                    break;
                            }
                        }

                        if (decodingDone)
                            break;

                        CHECK_b1(ref b1);
                        break;

                    case S_V0:
                        CHECK_b1(ref b1);
                        SETVALUE(b1 - m_a0);
                        b1 += m_runs[m_pb];
                        m_pb++;
                        break;

                    case S_VR:
                        CHECK_b1(ref b1);
                        SETVALUE(b1 - m_a0 + TabEnt.Param);
                        b1 += m_runs[m_pb];
                        m_pb++;
                        break;

                    case S_VL:
                        CHECK_b1(ref b1);
                        SETVALUE(b1 - m_a0 - TabEnt.Param);
                        m_pb--;
                        b1 -= m_runs[m_pb];
                        break;

                    case S_Ext:
                        m_runs[m_pa] = m_rowpixels - m_a0;
                        m_pa++;
                        Fax3Extension(module);
                        decodingDone = true;
                        break;

                    case S_EOL:
                        m_runs[m_pa] = m_rowpixels - m_a0;
                        m_pa++;

                        if (!NeedBits8(4))
                        {
                            handlePrematureEOFinExpand2D(module);
                            return false;
                        }

                        if (GetBits(4) != 0)
                        {
                            /* "EOL" */
                            Fax3Unexpected(module);
                        }

                        ClrBits(4);
                        m_EOLcnt = 1;
                        decodingDone = true;
                        break;

                    default:
                        Fax3Unexpected(module);
                        decodingDone = true;
                        break;
                }
            }

            if (!decodingDone && m_RunLength != 0)
            {
                if (m_RunLength + m_a0 < m_rowpixels)
                {
                    /* expect a final V0 */
                    if (!NeedBits8(1))
                    {
                        handlePrematureEOFinExpand2D(module);
                        return false;
                    }

                    if (GetBits(1) == 0)
                    {
                        /* "MainTable" */
                        Fax3Unexpected(module);
                        decodingDone = true;
                    }

                    if (!decodingDone)
                        ClrBits(1);
                }

                if (!decodingDone)
                    SETVALUE(0);
            }

            CLEANUP_RUNS(module);
            return true;
        }

        /*
        * CCITT Group 3 1-D Modified Huffman RLE Compression Support.
        * (Compression algorithms 2 and 32771)
        */

        private bool TIFFInitCCITTRLE()
        {
            /* reuse G3 support */
            InitCCITTFax3();

            m_decoder = Decoder.useFax3RLEDecoder;

            /*
             * Suppress RTC+EOLs when encoding and byte-align data.
             */
            return m_tif.SetField(TiffTag.FAXMODE, 
                FaxMode.NORTC | FaxMode.NOEOL | FaxMode.BYTEALIGN);
        }

        private bool TIFFInitCCITTRLEW()
        {
            /* reuse G3 support */
            InitCCITTFax3();

            m_decoder = Decoder.useFax3RLEDecoder;

            /*
             * Suppress RTC+EOLs when encoding and word-align data.
             */
            return m_tif.SetField(TiffTag.FAXMODE, 
                FaxMode.NORTC | FaxMode.NOEOL | FaxMode.WORDALIGN);
        }

        /// <summary>
        /// Decode the requested amount of RLE-encoded data.
        /// </summary>
        private bool Fax3DecodeRLE(byte[] buffer, int offset, int count)
        {
            const string module = "Fax3DecodeRLE";

            int thisrun = m_curruns; // current row's run array

            while (count > 0)
            {
                m_a0 = 0;
                m_RunLength = 0;
                m_pa = thisrun;

                bool expandSucceeded = EXPAND1D(module);
                if (expandSucceeded)
                {
                    fill(buffer, offset, m_runs, thisrun, m_pa, m_rowpixels);

                    // Cleanup at the end of the row.
                    if ((m_mode & FaxMode.BYTEALIGN) != 0)
                    {
                        int n = m_bit - (m_bit & ~7);
                        ClrBits(n);
                    }
                    else if ((m_mode & FaxMode.WORDALIGN) != 0)
                    {
                        int n = m_bit - (m_bit & ~15);
                        ClrBits(n);
                        if (m_bit == 0 && !isShortAligned(m_tif.m_rawcp))
                            m_tif.m_rawcp++;
                    }

                    offset += m_rowbytes;
                    count -= m_rowbytes;
                    m_line++;
                    continue;
                }

                // premature EOF
                fill(buffer, offset, m_runs, thisrun, m_pa, m_rowpixels);
                return false;
            }

            return true;
        }

        /*
        * CCITT Group 4 (T.6) Facsimile-compatible
        * Compression Scheme Support.
        */

        private bool TIFFInitCCITTFax4()
        {
            /* reuse G3 support */
            InitCCITTFax3();

            m_tif.MergeFieldInfo(m_fax4FieldInfo, m_fax4FieldInfo.Length);

            m_decoder = Decoder.useFax4Decoder;
            m_encodingFax4 = true;

            /*
             * Suppress RTC at the end of each strip.
             */
            return m_tif.SetField(TiffTag.FAXMODE, FaxMode.NORTC);
        }

        /// <summary>
        /// Decode the requested amount of G4-encoded data.
        /// </summary>
        private bool Fax4Decode(byte[] buffer, int offset, int count)
        {
            const string module = "Fax4Decode";

            while (count > 0)
            {
                m_a0 = 0;
                m_RunLength = 0;
                m_thisrun = m_curruns;
                m_pa = m_curruns;
                m_pb = m_refruns;
                int b1 = m_runs[m_pb];
                m_pb++; // next change on prev line

                bool expandSucceeded = EXPAND2D(module, b1);
                if (expandSucceeded && m_EOLcnt != 0)
                    expandSucceeded = false;

                if (expandSucceeded)
                {
                    fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                    SETVALUE(0); // imaginary change for reference
                    SWAP(ref m_curruns, ref m_refruns);
                    offset += m_rowbytes;
                    count -= m_rowbytes;
                    m_line++;
                    continue;
                }

                NeedBits16(13);
                ClrBits(13);
                fill(buffer, offset, m_runs, m_thisrun, m_pa, m_rowpixels);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Encode the requested amount of data.
        /// </summary>
        private bool Fax4Encode(byte[] buffer, int offset, int count)
        {
            m_buffer = buffer;
            m_offset = offset;

            while (count > 0)
            {
                if (!Fax3Encode2DRow())
                    return false;

                Buffer.BlockCopy(m_buffer, m_offset, m_refline, 0, m_rowbytes);
                m_offset += m_rowbytes;
                count -= m_rowbytes;
            }

            return true;
        }

        private bool Fax4PostEncode()
        {
            // terminate strip w/ EOFB
            putBits(EOL_CODE, 12);
            putBits(EOL_CODE, 12);

            if (m_bit != 8)
                flushBits();

            return true;
        }
    }
}
