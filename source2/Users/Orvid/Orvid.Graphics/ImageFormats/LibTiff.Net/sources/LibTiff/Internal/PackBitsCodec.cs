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
 * PackBits Compression Algorithm Support
 */

using System;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class PackBitsCodec : TiffCodec
    {
        private enum EncodingState
        {
            BASE,
            LITERAL,
            RUN,
            LITERAL_RUN
        };

        private int m_rowsize;

        public PackBitsCodec(Tiff tif, Compression scheme, string name)
            : base(tif, scheme, name)
        {
        }

        public override bool Init()
        {
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
            return PackBitsDecode(buffer, offset, count, plane);
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
            return PackBitsDecode(buffer, offset, count, plane);
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
            return PackBitsDecode(buffer, offset, count, plane);
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
            return PackBitsPreEncode(plane);
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
            return PackBitsEncode(buffer, offset, count, plane);
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
            return PackBitsEncodeChunk(buffer, offset, count, plane);
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
            return PackBitsEncodeChunk(buffer, offset, count, plane);
        }

        private bool PackBitsPreEncode(short s)
        {
            /*
             * Calculate the scanline/tile-width size in bytes.
             */
            if (m_tif.IsTiled())
                m_rowsize = m_tif.TileRowSize();
            else
                m_rowsize = m_tif.ScanlineSize();
            return true;
        }

        /*
        * Encode a run of pixels.
        */
        private bool PackBitsEncode(byte[] buf, int offset, int cc, short s)
        {
            int op = m_tif.m_rawcp;
            EncodingState state = EncodingState.BASE;
            int lastliteral = 0;
            int bp = offset;
            while (cc > 0)
            {
                /*
                 * Find the longest string of identical bytes.
                 */
                int b = buf[bp];
                bp++;
                cc--;
                int n = 1;
                for (; cc > 0 && b == buf[bp]; cc--, bp++)
                    n++;

                bool stop = false;
                while (!stop)
                {
                    if (op + 2 >= m_tif.m_rawdatasize)
                    {
                        /* insure space for new data */
                        /*
                         * Be careful about writing the last
                         * literal.  Must write up to that point
                         * and then copy the remainder to the
                         * front of the buffer.
                         */
                        if (state == EncodingState.LITERAL || state == EncodingState.LITERAL_RUN)
                        {
                            int slop = op - lastliteral;
                            m_tif.m_rawcc += lastliteral - m_tif.m_rawcp;
                            if (!m_tif.flushData1())
                                return false;
                            op = m_tif.m_rawcp;
                            while (slop-- > 0)
                            {
                                m_tif.m_rawdata[op] = m_tif.m_rawdata[lastliteral];
                                lastliteral++;
                                op++;
                            }

                            lastliteral = m_tif.m_rawcp;
                        }
                        else
                        {
                            m_tif.m_rawcc += op - m_tif.m_rawcp;
                            if (!m_tif.flushData1())
                                return false;
                            op = m_tif.m_rawcp;
                        }
                    }

                    switch (state)
                    {
                        case EncodingState.BASE:
                            /* initial state, set run/literal */
                            if (n > 1)
                            {
                                state = EncodingState.RUN;
                                if (n > 128)
                                {
                                    int temp = -127;
                                    m_tif.m_rawdata[op] = (byte)temp;
                                    op++;
                                    m_tif.m_rawdata[op] = (byte)b;
                                    op++;
                                    n -= 128;
                                    continue;
                                }

                                m_tif.m_rawdata[op] = (byte)(-n + 1);
                                op++;
                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                            }
                            else
                            {
                                lastliteral = op;
                                m_tif.m_rawdata[op] = 0;
                                op++;
                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                                state = EncodingState.LITERAL;
                            }
                            stop = true;
                            break;

                        case EncodingState.LITERAL:
                            /* last object was literal string */
                            if (n > 1)
                            {
                                state = EncodingState.LITERAL_RUN;
                                if (n > 128)
                                {
                                    int temp = -127;
                                    m_tif.m_rawdata[op] = (byte)temp;
                                    op++;
                                    m_tif.m_rawdata[op] = (byte)b;
                                    op++;
                                    n -= 128;
                                    continue;
                                }

                                m_tif.m_rawdata[op] = (byte)(-n + 1); /* encode run */
                                op++;
                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                            }
                            else
                            {
                                /* extend literal */
                                m_tif.m_rawdata[lastliteral]++;
                                if (m_tif.m_rawdata[lastliteral] == 127)
                                    state = EncodingState.BASE;

                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                            }
                            stop = true;
                            break;

                        case EncodingState.RUN:
                            /* last object was run */
                            if (n > 1)
                            {
                                if (n > 128)
                                {
                                    int temp = -127;
                                    m_tif.m_rawdata[op] = (byte)temp;
                                    op++;
                                    m_tif.m_rawdata[op] = (byte)b;
                                    op++;
                                    n -= 128;
                                    continue;
                                }

                                m_tif.m_rawdata[op] = (byte)(-n + 1);
                                op++;
                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                            }
                            else
                            {
                                lastliteral = op;
                                m_tif.m_rawdata[op] = 0;
                                op++;
                                m_tif.m_rawdata[op] = (byte)b;
                                op++;
                                state = EncodingState.LITERAL;
                            }
                            stop = true;
                            break;

                        case EncodingState.LITERAL_RUN:
                            /* literal followed by a run */
                            /*
                             * Check to see if previous run should
                             * be converted to a literal, in which
                             * case we convert literal-run-literal
                             * to a single literal.
                             */
                            int atemp = -1;
                            if (n == 1 && m_tif.m_rawdata[op - 2] == (byte)atemp && m_tif.m_rawdata[lastliteral] < 126)
                            {
                                m_tif.m_rawdata[lastliteral] += 2;
                                state = (m_tif.m_rawdata[lastliteral] == 127 ? EncodingState.BASE : EncodingState.LITERAL);
                                m_tif.m_rawdata[op - 2] = m_tif.m_rawdata[op - 1]; /* replicate */
                            }
                            else
                                state = EncodingState.RUN;
                            continue;
                    }
                }
            }

            m_tif.m_rawcc += op - m_tif.m_rawcp;
            m_tif.m_rawcp = op;
            return true;
        }

        /// <summary>
        /// Encode a rectangular chunk of pixels. We break it up into row-sized pieces to insure
        /// that encoded runs do not span rows. Otherwise, there can be problems with the decoder
        /// if data is read, for example, by scanlines when it was encoded by strips.
        /// </summary>
        private bool PackBitsEncodeChunk(byte[] buffer, int offset, int count, short plane)
        {
            while (count > 0)
            {
                int chunk = m_rowsize;
                if (count < chunk)
                    chunk = count;

                if (!PackBitsEncode(buffer, offset, chunk, plane))
                    return false;

                offset += chunk;
                count -= chunk;
            }

            return true;
        }

        private bool PackBitsDecode(byte[] buffer, int offset, int count, short plane)
        {
            int bp = m_tif.m_rawcp;
            int cc = m_tif.m_rawcc;
            while (cc > 0 && count > 0)
            {
                int n = m_tif.m_rawdata[bp];
                bp++;
                cc--;

                // Watch out for compilers that don't sign extend chars...
                if (n >= 128)
                    n -= 256;

                if (n < 0)
                {
                    // replicate next byte (-n + 1) times
                    if (n == -128)
                    {
                        // nop
                        continue;
                    }

                    n = -n + 1;
                    if (count < n)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "PackBitsDecode: discarding {0} bytes to avoid buffer overrun",
                            n - count);

                        n = count;
                    }
                    count -= n;
                    int b = m_tif.m_rawdata[bp];
                    bp++;
                    cc--;
                    while (n-- > 0)
                    {
                        buffer[offset] = (byte)b;
                        offset++;
                    }
                }
                else
                {
                    // copy next (n + 1) bytes literally
                    if (count < n + 1)
                    {
                        Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                            "PackBitsDecode: discarding {0} bytes to avoid buffer overrun",
                            n - count + 1);

                        n = count - 1;
                    }

                    Buffer.BlockCopy(m_tif.m_rawdata, bp, buffer, offset, ++n);
                    offset += n;
                    count -= n;
                    bp += n;
                    cc -= n;
                }
            }

            m_tif.m_rawcp = bp;
            m_tif.m_rawcc = cc;
            if (count > 0)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "PackBitsDecode: Not enough data for scanline {0}", m_tif.m_row);
                return false;
            }

            return true;
        }
    }
}
