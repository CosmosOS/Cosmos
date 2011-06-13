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
 * "Null" Compression Algorithm Support.
 */

using System;
using System.Diagnostics;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class DumpModeCodec : TiffCodec
    {
        public DumpModeCodec(Tiff tif, Compression scheme, string name)
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
            return DumpModeDecode(buffer, offset, count, plane);
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
            return DumpModeDecode(buffer, offset, count, plane);
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
            return DumpModeDecode(buffer, offset, count, plane);
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
            return DumpModeEncode(buffer, offset, count, plane);
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
            return DumpModeEncode(buffer, offset, count, plane);
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
            return DumpModeEncode(buffer, offset, count, plane);
        }

        /// <summary>
        /// Seeks the specified row in the strip being processed.
        /// </summary>
        /// <param name="row">The row to seek.</param>
        /// <returns>
        /// 	<c>true</c> if specified row was successfully found; otherwise, <c>false</c>
        /// </returns>
        public override bool Seek(int row)
        {
            m_tif.m_rawcp += row * m_tif.m_scanlinesize;
            m_tif.m_rawcc -= row * m_tif.m_scanlinesize;
            return true;
        }
        
        /// <summary>
        /// Encode a hunk of pixels.
        /// </summary>
        private bool DumpModeEncode(byte[] buffer, int offset, int count, short plane)
        {
            while (count > 0)
            {
                int n = count;
                if (m_tif.m_rawcc + n > m_tif.m_rawdatasize)
                    n = m_tif.m_rawdatasize - m_tif.m_rawcc;

                Debug.Assert(n > 0);

                Buffer.BlockCopy(buffer, offset, m_tif.m_rawdata, m_tif.m_rawcp, n);
                m_tif.m_rawcp += n;
                m_tif.m_rawcc += n;

                offset += n;
                count -= n;
                if (m_tif.m_rawcc >= m_tif.m_rawdatasize && !m_tif.flushData1())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Decode a hunk of pixels.
        /// </summary>
        private bool DumpModeDecode(byte[] buffer, int offset, int count, short plane)
        {
            if (m_tif.m_rawcc < count)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "DumpModeDecode: Not enough data for scanline {0}", m_tif.m_row);
                return false;
            }

            Buffer.BlockCopy(m_tif.m_rawdata, m_tif.m_rawcp, buffer, offset, count);
            m_tif.m_rawcp += count;
            m_tif.m_rawcc -= count;
            return true;
        }
    }
}
