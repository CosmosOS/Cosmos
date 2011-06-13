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
 * Rev 5.0 Lempel-Ziv & Welch Compression Support
 *
 * This code is derived from the compress program whose code is
 * derived from software contributed to Berkeley by James A. Woods,
 * derived from original work by Spencer Thomas and Joseph Orost.
 *
 * The original Berkeley copyright notice appears below in its entirety.
 */

/*
 * NB: The 5.0 spec describes a different algorithm than Aldus
 *     implements.  Specifically, Aldus does code length transitions
 *     one code earlier than should be done (for real LZW).
 *     Earlier versions of this library implemented the correct
 *     LZW algorithm, but emitted codes in a bit order opposite
 *     to the TIFF spec.  Thus, to maintain compatibility w/ Aldus
 *     we interpret MSB-LSB ordered codes to be images written w/
 *     old versions of this library, but otherwise adhere to the
 *     Aldus "off by one" algorithm.
 *
 * Future revisions to the TIFF spec are expected to "clarify this issue".
 */

using System;
using System.Diagnostics;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class LZWCodec : CodecWithPredictor
    {
        /*
        * Each strip of data is supposed to be terminated by a CODE_EOI.
        * If the following #define is included, the decoder will also
        * check for end-of-strip w/o seeing this code.  This makes the
        * library more robust, but also slower.
        */
        private bool LZW_CHECKEOS = true; /* include checks for strips w/o EOI code */

        /*
        * The TIFF spec specifies that encoded bit
        * strings range from 9 to 12 bits.
        */
        private const short BITS_MIN = 9;       /* start with 9 bits */
        private const short BITS_MAX = 12;      /* max of 12 bit strings */

        /* predefined codes */
        private const short CODE_CLEAR = 256;     /* code to clear string table */
        private const short CODE_EOI = 257;     /* end-of-information code */
        private const short CODE_FIRST = 258;     /* first free code entry */
        private const short CODE_MAX = ((1 << BITS_MAX) - 1);
        private const short CODE_MIN = ((1 << BITS_MIN) - 1);

        private const int HSIZE = 9001;       /* 91% occupancy */
        private const int HSHIFT = (13 - 8);
        /* NB: +1024 is for compatibility with old files */
        private const int CSIZE = (((1 << BITS_MAX) - 1) + 1024);

        private const int CHECK_GAP = 10000;       /* enc_ratio check interval */

        /*
        * Decoding-specific state.
        */
        private struct code_t
        {
            public int next;
            public short length; /* string len, including this token */
            public byte value; /* data value */
            public byte firstchar; /* first token of string */
        };

        /*
        * Encoding-specific state.
        */
        private struct hash_t
        {
            public int hash;
            public short code;
        };

        private bool m_compatDecode;

        private short m_nbits; /* # of bits/code */
        private short m_maxcode; /* maximum code for base.nbits */
        private short m_free_ent; /* next free entry in hash table */
        private int m_nextdata; /* next bits of i/o */
        private int m_nextbits; /* # of valid bits in base.nextdata */

        private int m_rw_mode; /* preserve rw_mode from init */

        /* Decoding specific data */
        private int m_dec_nbitsmask; /* lzw_nbits 1 bits, right adjusted */
        private int m_dec_restart; /* restart count */
        private int m_dec_bitsleft; /* available bits in raw data */
        private bool m_oldStyleCodeFound; /* if true, old style LZW code found*/
        private int m_dec_codep; /* current recognized code */
        private int m_dec_oldcodep; /* previously recognized code */
        private int m_dec_free_entp; /* next free entry */
        private int m_dec_maxcodep; /* max available entry */
        private code_t[] m_dec_codetab; /* kept separate for small machines */

        /* Encoding specific data */
        private int m_enc_oldcode; /* last code encountered */
        private int m_enc_checkpoint; /* point at which to clear table */
        private int m_enc_ratio; /* current compression ratio */
        private int m_enc_incount; /* (input) data bytes encoded */
        private int m_enc_outcount; /* encoded (output) bytes */
        private int m_enc_rawlimit; /* bound on tif_rawdata buffer */
        private hash_t[] m_enc_hashtab; /* kept separate for small machines */

        public LZWCodec(Tiff tif, Compression scheme, string name)
            : base(tif, scheme, name)
        {
        }

        public override bool Init()
        {
            Debug.Assert(m_scheme == Compression.LZW);

            m_dec_codetab = null;
            m_oldStyleCodeFound = false;
            m_enc_hashtab = null;
            m_rw_mode = m_tif.m_mode;
            m_compatDecode = false;

            /*
             * Setup predictor setup.
             */
            TIFFPredictorInit(null);
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
            return LZWPreDecode(plane);
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
            return LZWPreEncode(plane);
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
            return LZWPostEncode();
        }

        /// <summary>
        /// Cleanups the state of the codec.
        /// </summary>
        /// <remarks>
        /// 	<b>Cleanup</b> is called when codec is no longer needed (won't be used) and can be
        /// used for example to restore tag methods that were substituted.</remarks>
        public override void Cleanup()
        {
            LZWCleanup();
            m_tif.m_mode = m_rw_mode;
        }

        // CodecWithPredictor overrides

        public override bool predictor_setupdecode()
        {
            return LZWSetupDecode();
        }

        public override bool predictor_decoderow(byte[] buffer, int offset, int count, short plane)
        {
            if (m_compatDecode)
                return LZWDecodeCompat(buffer, offset, count, plane);

            return LZWDecode(buffer, offset, count, plane);
        }

        public override bool predictor_decodestrip(byte[] buffer, int offset, int count, short plane)
        {
            if (m_compatDecode)
                return LZWDecodeCompat(buffer, offset, count, plane);

            return LZWDecode(buffer, offset, count, plane);
        }

        public override bool predictor_decodetile(byte[] buffer, int offset, int count, short plane)
        {
            if (m_compatDecode)
                return LZWDecodeCompat(buffer, offset, count, plane);

            return LZWDecode(buffer, offset, count, plane);
        }

        public override bool predictor_setupencode()
        {
            return LZWSetupEncode();
        }

        public override bool predictor_encoderow(byte[] buffer, int offset, int count, short plane)
        {
            return LZWEncode(buffer, offset, count, plane);
        }

        public override bool predictor_encodestrip(byte[] buffer, int offset, int count, short plane)
        {
            return LZWEncode(buffer, offset, count, plane);
        }

        public override bool predictor_encodetile(byte[] buffer, int offset, int count, short plane)
        {
            return LZWEncode(buffer, offset, count, plane);
        }

        private bool LZWSetupDecode()
        {
            if (m_dec_codetab == null)
            {
                m_dec_codetab = new code_t [CSIZE];

                /*
                 * Pre-load the table.
                 */
                int code = 255;
                do
                {
                    m_dec_codetab[code].value = (byte)code;
                    m_dec_codetab[code].firstchar = (byte)code;
                    m_dec_codetab[code].length = 1;
                    m_dec_codetab[code].next = -1;
                }
                while (code-- != 0);

                /*
                * Zero-out the unused entries
                */
                Array.Clear(m_dec_codetab, CODE_CLEAR, CODE_FIRST - CODE_CLEAR);
            }

            return true;
        }

        /*
         * Setup state for decoding a strip.
         */
        private bool LZWPreDecode(short s)
        {
            if (m_dec_codetab == null)
                SetupDecode();

            /*
             * Check for old bit-reversed codes.
             */
            if (m_tif.m_rawdata[0] == 0 && (m_tif.m_rawdata[1] & 0x1) != 0)
            {
                if (!m_oldStyleCodeFound)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name, "Old-style LZW codes, convert file");
                    m_compatDecode = true;

                    /*
                     * If doing horizontal differencing, must
                     * re-setup the predictor logic since we
                     * switched the basic decoder methods...
                     */
                    SetupDecode();
                    m_oldStyleCodeFound = true;
                }

                m_maxcode = CODE_MIN;
            }
            else
            {
                m_maxcode = CODE_MIN - 1;
                m_oldStyleCodeFound = false;
            }

            m_nbits = BITS_MIN;
            m_nextbits = 0;
            m_nextdata = 0;

            m_dec_restart = 0;
            m_dec_nbitsmask = CODE_MIN;
            m_dec_bitsleft = m_tif.m_rawcc << 3;
            m_dec_free_entp = CODE_FIRST;

            /*
             * Zero entries that are not yet filled in.  We do
             * this to guard against bogus input data that causes
             * us to index into undefined entries.  If you can
             * come up with a way to safely bounds-check input codes
             * while decoding then you can remove this operation.
             */
            Array.Clear(m_dec_codetab, m_dec_free_entp, CSIZE - CODE_FIRST);
            m_dec_oldcodep = -1;
            m_dec_maxcodep = m_dec_nbitsmask - 1;
            return true;
        }

        private bool LZWDecode(byte[] buffer, int offset, int count, short plane)
        {
            Debug.Assert(m_dec_codetab != null);

            // Restart interrupted output operation.
            if (m_dec_restart != 0)
            {
                int codep = m_dec_codep;
                int residue = m_dec_codetab[codep].length - m_dec_restart;
                if (residue > count)
                {
                    // Residue from previous decode is sufficient to satisfy decode request. Skip
                    // to the start of the decoded string, place decoded values in the output
                    // buffer, and return.
                    m_dec_restart += count;

                    do
                    {
                        codep = m_dec_codetab[codep].next;
                    }
                    while (--residue > count && codep != -1);

                    if (codep != -1)
                    {
                        int tp = count;
                        do
                        {
                            tp--;
                            buffer[offset + tp] = m_dec_codetab[codep].value;
                            codep = m_dec_codetab[codep].next;
                        }
                        while (--count != 0 && codep != -1);
                    }

                    return true;
                }

                // Residue satisfies only part of the decode request.
                offset += residue;
                count -= residue;
                int ttp = 0;
                do
                {
                    --ttp;
                    int t = m_dec_codetab[codep].value;
                    codep = m_dec_codetab[codep].next;
                    buffer[offset + ttp] = (byte)t;
                }
                while (--residue != 0 && codep != -1);

                m_dec_restart = 0;
            }

            while (count > 0)
            {
                short code;
                NextCode(out code, false);
                if (code == CODE_EOI)
                    break;

                if (code == CODE_CLEAR)
                {
                    m_dec_free_entp = CODE_FIRST;
                    Array.Clear(m_dec_codetab, m_dec_free_entp, CSIZE - CODE_FIRST);

                    m_nbits = BITS_MIN;
                    m_dec_nbitsmask = CODE_MIN;
                    m_dec_maxcodep = m_dec_nbitsmask - 1;
                    NextCode(out code, false);
                    
                    if (code == CODE_EOI)
                        break;
                    
                    if (code == CODE_CLEAR)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "LZWDecode: Corrupted LZW table at scanline {0}", m_tif.m_row);
                        return false;
                    }

                    buffer[offset] = (byte)code;
                    offset++;
                    count--;
                    m_dec_oldcodep = code;
                    continue;
                }

                int codep = code;

                // Add the new entry to the code table.
                if (m_dec_free_entp < 0 || m_dec_free_entp >= CSIZE)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "LZWDecode: Corrupted LZW table at scanline {0}", m_tif.m_row);
                    return false;
                }

                m_dec_codetab[m_dec_free_entp].next = m_dec_oldcodep;
                if (m_dec_codetab[m_dec_free_entp].next < 0 || m_dec_codetab[m_dec_free_entp].next >= CSIZE)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "LZWDecode: Corrupted LZW table at scanline {0}", m_tif.m_row);
                    return false;
                }

                m_dec_codetab[m_dec_free_entp].firstchar = m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].firstchar;
                m_dec_codetab[m_dec_free_entp].length = (short)(m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].length + 1);
                m_dec_codetab[m_dec_free_entp].value = (codep < m_dec_free_entp) ? m_dec_codetab[codep].firstchar : m_dec_codetab[m_dec_free_entp].firstchar;

                if (++m_dec_free_entp > m_dec_maxcodep)
                {
                    if (++m_nbits > BITS_MAX)
                    {
                        // should not happen
                        m_nbits = BITS_MAX;
                    }

                    m_dec_nbitsmask = MAXCODE(m_nbits);
                    m_dec_maxcodep = m_dec_nbitsmask - 1;
                }

                m_dec_oldcodep = code;
                if (code >= 256)
                {
                    // Code maps to a string, copy string value to output (written in reverse).
                    if (m_dec_codetab[codep].length == 0)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "LZWDecode: Wrong length of decoded string: data probably corrupted at scanline {0}",
                            m_tif.m_row);
                        return false;
                    }

                    if (m_dec_codetab[codep].length > count)
                    {
                        // String is too long for decode buffer, locate portion that will fit,
                        // copy to the decode buffer, and setup restart logic for the next
                        // decoding call.
                        m_dec_codep = code;
                        do
                        {
                            codep = m_dec_codetab[codep].next;
                        }
                        while (codep != -1 && m_dec_codetab[codep].length > count);

                        if (codep != -1)
                        {
                            m_dec_restart = count;
                            int tp = count;
                            do
                            {
                                tp--;
                                buffer[offset + tp] = m_dec_codetab[codep].value;
                                codep = m_dec_codetab[codep].next;
                            }
                            while (--count != 0 && codep != -1);

                            if (codep != -1)
                                codeLoop();
                        }
                        break;
                    }

                    int len = m_dec_codetab[codep].length;
                    int ttp = len;
                    do
                    {
                        --ttp;
                        int t = m_dec_codetab[codep].value;
                        codep = m_dec_codetab[codep].next;
                        buffer[offset + ttp] = (byte)t;
                    }
                    while (codep != -1 && ttp > 0);

                    if (codep != -1)
                    {
                        codeLoop();
                        break;
                    }

                    offset += len;
                    count -= len;
                }
                else
                {
                    buffer[offset] = (byte)code;
                    offset++;
                    count--;
                }
            }

            if (count > 0)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "LZWDecode: Not enough data at scanline {0} (short {1} bytes)",
                    m_tif.m_row, count);
                return false;
            }

            return true;
        }

        private bool LZWDecodeCompat(byte[] buffer, int offset, int count, short plane)
        {
            // Restart interrupted output operation.
            if (m_dec_restart != 0)
            {
                int residue;

                int codep = m_dec_codep;
                residue = m_dec_codetab[codep].length - m_dec_restart;
                if (residue > count)
                {
                    // Residue from previous decode is sufficient to satisfy decode request.
                    // Skip to the start of the decoded string, place decoded values in the output
                    // buffer, and return.
                    m_dec_restart += count;
                    do
                    {
                        codep = m_dec_codetab[codep].next;
                    }
                    while (--residue > count);

                    int tp = count;
                    do
                    {
                        --tp;
                        buffer[offset + tp] = m_dec_codetab[codep].value;
                        codep = m_dec_codetab[codep].next;
                    }
                    while (--count != 0);

                    return true;
                }

                // Residue satisfies only part of the decode request.
                offset += residue;
                count -= residue;
                int ttp = 0;
                do
                {
                    --ttp;
                    buffer[offset + ttp] = m_dec_codetab[codep].value;
                    codep = m_dec_codetab[codep].next;
                }
                while (--residue != 0);

                m_dec_restart = 0;
            }

            while (count > 0)
            {
                short code;
                NextCode(out code, true);
                if (code == CODE_EOI)
                    break;
                
                if (code == CODE_CLEAR)
                {
                    m_dec_free_entp = CODE_FIRST;
                    Array.Clear(m_dec_codetab, m_dec_free_entp, CSIZE - CODE_FIRST);

                    m_nbits = BITS_MIN;
                    m_dec_nbitsmask = CODE_MIN;
                    m_dec_maxcodep = m_dec_nbitsmask;
                    NextCode(out code, true);
                    
                    if (code == CODE_EOI)
                        break;

                    if (code == CODE_CLEAR)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "LZWDecode: Corrupted LZW table at scanline {0}", m_tif.m_row);
                        return false;
                    }

                    buffer[offset] = (byte)code;
                    offset++;
                    count--;
                    m_dec_oldcodep = code;
                    continue;
                }

                int codep = code;

                // Add the new entry to the code table.
                if (m_dec_free_entp < 0 || m_dec_free_entp >= CSIZE)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "LZWDecodeCompat: Corrupted LZW table at scanline {0}", m_tif.m_row);
                    return false;
                }

                m_dec_codetab[m_dec_free_entp].next = m_dec_oldcodep;
                if (m_dec_codetab[m_dec_free_entp].next < 0 || m_dec_codetab[m_dec_free_entp].next >= CSIZE)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "LZWDecodeCompat: Corrupted LZW table at scanline {0}", m_tif.m_row);
                    return false;
                }

                m_dec_codetab[m_dec_free_entp].firstchar = m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].firstchar;
                m_dec_codetab[m_dec_free_entp].length = (short)(m_dec_codetab[m_dec_codetab[m_dec_free_entp].next].length + 1);
                m_dec_codetab[m_dec_free_entp].value = (codep < m_dec_free_entp) ? m_dec_codetab[codep].firstchar : m_dec_codetab[m_dec_free_entp].firstchar;
                if (++m_dec_free_entp > m_dec_maxcodep)
                {
                    if (++m_nbits > BITS_MAX)
                    {
                        // should not happen
                        m_nbits = BITS_MAX;
                    }
                    m_dec_nbitsmask = MAXCODE(m_nbits);
                    m_dec_maxcodep = m_dec_nbitsmask;
                }

                m_dec_oldcodep = code;
                if (code >= 256)
                {
                    int op_orig = offset;

                    // Code maps to a string, copy string value to output (written in reverse).
                    if (m_dec_codetab[codep].length == 0)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "LZWDecodeCompat: Wrong length of decoded string: data probably corrupted at scanline {0}",
                            m_tif.m_row);
                        return false;
                    }

                    if (m_dec_codetab[codep].length > count)
                    {
                        // String is too long for decode buffer, locate portion that will fit,
                        // copy to the decode buffer, and setup restart logic for the next
                        // decoding call.
                        m_dec_codep = code;
                        do
                        {
                            codep = m_dec_codetab[codep].next;
                        }
                        while (m_dec_codetab[codep].length > count);

                        m_dec_restart = count;
                        int tp = count;
                        do
                        {
                            --tp;
                            buffer[offset + tp] = m_dec_codetab[codep].value;
                            codep = m_dec_codetab[codep].next;
                        }
                        while (--count != 0);

                        break;
                    }

                    offset += m_dec_codetab[codep].length;
                    count -= m_dec_codetab[codep].length;
                    int ttp = offset;
                    do
                    {
                        --ttp;
                        buffer[ttp] = m_dec_codetab[codep].value;
                        codep = m_dec_codetab[codep].next;
                    }
                    while (codep != -1 && ttp > op_orig);
                }
                else
                {
                    buffer[offset] = (byte)code;
                    offset++;
                    count--;
                }
            }

            if (count > 0)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "LZWDecodeCompat: Not enough data at scanline {0} (short {1} bytes)",
                    m_tif.m_row, count);
                return false;
            }

            return true;
        }
        
        private bool LZWSetupEncode()
        {
            m_enc_hashtab = new hash_t [HSIZE];
            return true;
        }

        /*
         * Reset encoding state at the start of a strip.
         */
        private bool LZWPreEncode(short s)
        {
            if (m_enc_hashtab == null)
                SetupEncode();

            m_nbits = BITS_MIN;
            m_maxcode = CODE_MIN;
            m_free_ent = CODE_FIRST;
            m_nextbits = 0;
            m_nextdata = 0;
            m_enc_checkpoint = CHECK_GAP;
            m_enc_ratio = 0;
            m_enc_incount = 0;
            m_enc_outcount = 0;

            /*
             * The 4 here insures there is space for 2 max-sized
             * codes in LZWEncode and LZWPostDecode.
             */
            m_enc_rawlimit = m_tif.m_rawdatasize - 1 - 4;
            cl_hash(); /* clear hash table */
            m_enc_oldcode = -1; /* generates CODE_CLEAR in LZWEncode */
            return true;
        }

        /*
         * Finish off an encoded strip by flushing the last
         * string and tacking on an End Of Information code.
         */
        private bool LZWPostEncode()
        {
            if (m_tif.m_rawcp > m_enc_rawlimit)
            {
                m_tif.m_rawcc = m_tif.m_rawcp;
                m_tif.flushData1();
                m_tif.m_rawcp = 0;
            }

            if (m_enc_oldcode != -1)
            {
                PutNextCode(m_enc_oldcode);
                m_enc_oldcode = -1;
            }

            PutNextCode(CODE_EOI);

            if (m_nextbits > 0)
            {
                m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata << (8 - m_nextbits));
                m_tif.m_rawcp++;
            }

            m_tif.m_rawcc = m_tif.m_rawcp;
            return true;
        }

        /// <summary>
        /// Encode a chunk of pixels.
        /// </summary>
        /// <remarks>
        /// Uses an open addressing double hashing (no chaining) on the prefix code/next character
        /// combination. We do a variant of Knuth's algorithm D (vol. 3, sec. 6.4) along with
        /// G. Knott's relatively-prime secondary probe. Here, the modular division first probe is
        /// gives way to a faster exclusive-or manipulation. Also do block compression with an
        /// adaptive reset, whereby the code table is cleared when the compression ratio
        /// decreases, but after the table fills. The variable-length output codes are re-sized at
        /// this point, and a CODE_CLEAR is generated for the decoder. 
        /// </remarks>
        private bool LZWEncode(byte[] buffer, int offset, int count, short plane)
        {
            Debug.Assert(m_enc_hashtab != null);
            if (m_enc_oldcode == -1 && count > 0)
            {
                // NB: This is safe because it can only happen at the start of a strip where we
                //     know there is space in the data buffer.
                PutNextCode(CODE_CLEAR);
                m_enc_oldcode = buffer[offset];
                offset++;
                count--;
                m_enc_incount++;
            }

            while (count > 0)
            {
                int c = buffer[offset];
                offset++;
                count--;
                m_enc_incount++;
                int fcode = (c << BITS_MAX) + m_enc_oldcode;
                int h = (c << HSHIFT) ^ m_enc_oldcode; // xor hashing

                // Check hash index for an overflow.
                if (h >= HSIZE)
                    h -= HSIZE;

                if (m_enc_hashtab[h].hash == fcode)
                {
                    m_enc_oldcode = m_enc_hashtab[h].code;
                    continue;
                }

                bool hit = false;

                if (m_enc_hashtab[h].hash >= 0)
                {
                    // Primary hash failed, check secondary hash.
                    int disp = HSIZE - h;
                    if (h == 0)
                        disp = 1;
                    do
                    {
                        h -= disp;
                        if (h < 0)
                            h += HSIZE;

                        if (m_enc_hashtab[h].hash == fcode)
                        {
                            m_enc_oldcode = m_enc_hashtab[h].code;
                            hit = true;
                            break;
                        }
                    }
                    while (m_enc_hashtab[h].hash >= 0);
                }

                if (!hit)
                {
                    // New entry, emit code and add to table.
                    // Verify there is space in the buffer for the code and any potential Clear
                    // code that might be emitted below. The value of limit is setup so that there
                    // are at least 4 bytes free - room for 2 codes.
                    if (m_tif.m_rawcp > m_enc_rawlimit)
                    {
                        m_tif.m_rawcc = m_tif.m_rawcp;
                        m_tif.flushData1();
                        m_tif.m_rawcp = 0;
                    }

                    PutNextCode(m_enc_oldcode);
                    m_enc_oldcode = c;
                    m_enc_hashtab[h].code = m_free_ent;
                    m_free_ent++;
                    m_enc_hashtab[h].hash = fcode;
                    if (m_free_ent == CODE_MAX - 1)
                    {
                        // table is full, emit clear code and reset
                        cl_hash();
                        m_enc_ratio = 0;
                        m_enc_incount = 0;
                        m_enc_outcount = 0;
                        m_free_ent = CODE_FIRST;
                        PutNextCode(CODE_CLEAR);
                        m_nbits = BITS_MIN;
                        m_maxcode = CODE_MIN;
                    }
                    else
                    {
                        // If the next entry is going to be too big for the code size, then
                        // increase it, if possible.
                        if (m_free_ent > m_maxcode)
                        {
                            m_nbits++;
                            Debug.Assert(m_nbits <= BITS_MAX);
                            m_maxcode = (short)MAXCODE(m_nbits);
                        }
                        else if (m_enc_incount >= m_enc_checkpoint)
                        {
                            // Check compression ratio and, if things seem to be slipping, clear
                            // the hash table and reset state. The compression ratio is
                            // a 24 + 8-bit fractional number.
                            m_enc_checkpoint = m_enc_incount + CHECK_GAP;

                            int rat;
                            if (m_enc_incount > 0x007fffff)
                            {
                                // NB: shift will overflow
                                rat = m_enc_outcount >> 8;
                                rat = (rat == 0 ? 0x7fffffff : m_enc_incount / rat);
                            }
                            else
                                rat = (m_enc_incount << 8) / m_enc_outcount;

                            if (rat <= m_enc_ratio)
                            {
                                cl_hash();
                                m_enc_ratio = 0;
                                m_enc_incount = 0;
                                m_enc_outcount = 0;
                                m_free_ent = CODE_FIRST;
                                PutNextCode(CODE_CLEAR);
                                m_nbits = BITS_MIN;
                                m_maxcode = CODE_MIN;
                            }
                            else
                                m_enc_ratio = rat;
                        }
                    }
                }
            }

            return true;
        }

        private void LZWCleanup()
        {
            m_dec_codetab = null;
            m_enc_hashtab = null;
        }

        private static int MAXCODE(int n)
        {
            return ((1 << n) - 1);
        }

        private void PutNextCode(int c)
        {
            m_nextdata = (m_nextdata << m_nbits) | c;
            m_nextbits += m_nbits;
            m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata >> (m_nextbits - 8));
            m_tif.m_rawcp++;
            m_nextbits -= 8;
            if (m_nextbits >= 8)
            {
                m_tif.m_rawdata[m_tif.m_rawcp] = (byte)(m_nextdata >> (m_nextbits - 8));
                m_tif.m_rawcp++;
                m_nextbits -= 8;
            }

            m_enc_outcount += m_nbits;
        }

        /*
         * Reset encoding hash table.
         */
        private void cl_hash()
        {
            int hp = HSIZE - 1;
            int i = HSIZE - 8;

            do
            {
                i -= 8;
                m_enc_hashtab[hp - 7].hash = -1;
                m_enc_hashtab[hp - 6].hash = -1;
                m_enc_hashtab[hp - 5].hash = -1;
                m_enc_hashtab[hp - 4].hash = -1;
                m_enc_hashtab[hp - 3].hash = -1;
                m_enc_hashtab[hp - 2].hash = -1;
                m_enc_hashtab[hp - 1].hash = -1;
                m_enc_hashtab[hp].hash = -1;
                hp -= 8;
            }
            while (i >= 0);

            for (i += 8; i > 0; i--, hp--)
                m_enc_hashtab[hp].hash = -1;
        }

        private void NextCode(out short _code, bool compat)
        {
            if (LZW_CHECKEOS)
            {
                if (m_dec_bitsleft < m_nbits)
                {
                    Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                        "LZWDecode: Strip {0} not terminated with EOI code", m_tif.m_curstrip);
                    _code = CODE_EOI;
                }
                else
                {
                    if (compat)
                        GetNextCodeCompat(out _code);
                    else
                        GetNextCode(out _code);

                    m_dec_bitsleft -= m_nbits;
                }
            }
            else
            {
                if (compat)
                    GetNextCodeCompat(out _code);
                else
                    GetNextCode(out _code);
            }
        }

        private void GetNextCode(out short code)
        {
            m_nextdata = (m_nextdata << 8) | m_tif.m_rawdata[m_tif.m_rawcp];
            m_tif.m_rawcp++;
            m_nextbits += 8;
            if (m_nextbits < m_nbits)
            {
                m_nextdata = (m_nextdata << 8) | m_tif.m_rawdata[m_tif.m_rawcp];
                m_tif.m_rawcp++;
                m_nextbits += 8;
            }
            code = (short)((m_nextdata >> (m_nextbits - m_nbits)) & m_dec_nbitsmask);
            m_nextbits -= m_nbits;
        }

        private void GetNextCodeCompat(out short code)
        {
            m_nextdata |= m_tif.m_rawdata[m_tif.m_rawcp] << m_nextbits;
            m_tif.m_rawcp++;
            m_nextbits += 8;
            if (m_nextbits < m_nbits)
            {
                m_nextdata |= m_tif.m_rawdata[m_tif.m_rawcp] << m_nextbits;
                m_tif.m_rawcp++;
                m_nextbits += 8;
            }
            code = (short)(m_nextdata & m_dec_nbitsmask);
            m_nextdata >>= m_nbits;
            m_nextbits -= m_nbits;
        }

        private void codeLoop()
        {
            Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                "LZWDecode: Bogus encoding, loop in the code table; scanline {0}", m_tif.m_row);
        }
    }
}

/*
 * Copyright (c) 1985, 1986 The Regents of the University of California.
 * All rights reserved.
 *
 * This code is derived from software contributed to Berkeley by
 * James A. Woods, derived from original work by Spencer Thomas
 * and Joseph Orost.
 *
 * Redistribution and use in source and binary forms are permitted
 * provided that the above copyright notice and this paragraph are
 * duplicated in all such forms and that any documentation,
 * advertising materials, and other materials related to such
 * distribution and use acknowledge that the software was developed
 * by the University of California, Berkeley.  The name of the
 * University may not be used to endorse or promote products derived
 * from this software without specific prior written permission.
 * THIS SOFTWARE IS PROVIDED ``AS IS'' AND WITHOUT ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, WITHOUT LIMITATION, THE IMPLIED
 * WARRANTIES OF MERCHANTIBILITY AND FITNESS FOR A PARTICULAR PURPOSE.
 */
