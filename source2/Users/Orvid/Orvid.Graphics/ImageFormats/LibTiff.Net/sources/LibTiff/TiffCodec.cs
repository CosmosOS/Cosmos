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

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Base class for all codecs within the library.
    /// </summary>
    /// <remarks><para>
    /// A codec is a class that implements decoding, encoding, or decoding and encoding of a
    /// compression algorithm.
    /// </para><para>
    /// The library provides a collection of builtin codecs. More codecs may be registered
    /// through calls to the library and/or the builtin implementations may be overridden.
    /// </para></remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    class TiffCodec
    {
        /// <summary>
        /// An instance of <see cref="Tiff"/>.
        /// </summary>
        protected Tiff m_tif;

        /// <summary>
        /// Compression scheme this codec impelements.
        /// </summary>
        protected internal Compression m_scheme;

        /// <summary>
        /// Codec name.
        /// </summary>
        protected internal string m_name;

        /// <summary>
        /// Initializes a new instance of the <see cref="TiffCodec"/> class.
        /// </summary>
        /// <param name="tif">An instance of <see cref="Tiff"/> class.</param>
        /// <param name="scheme">The compression scheme for the codec.</param>
        /// <param name="name">The name of the codec.</param>
        public TiffCodec(Tiff tif, Compression scheme, string name)
        {
            m_scheme = scheme;
            m_tif = tif;
            m_name = name;
        }

        /// <summary>
        /// Gets a value indicating whether this codec can encode data.
        /// </summary>
        /// <value>
        /// <c>true</c> if this codec can encode data; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanEncode
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this codec can decode data.
        /// </summary>
        /// <value>
        /// <c>true</c> if this codec can decode data; otherwise, <c>false</c>.
        /// </value>
        public virtual bool CanDecode
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        /// <returns><c>true</c> if initialized successfully</returns>
        public virtual bool Init()
        {
            return true;
        }

        /// <summary>
        /// Setups the decoder part of the codec.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this codec successfully setup its decoder part and can decode data;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <b>SetupDecode</b> is called once before
        /// <see cref="PreDecode"/>.</remarks>
        public virtual bool SetupDecode()
        {
            return true;
        }

        /// <summary>
        /// Prepares the decoder part of the codec for a decoding.
        /// </summary>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns><c>true</c> if this codec successfully prepared its decoder part and ready
        /// to decode data; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <b>PreDecode</b> is called after <see cref="SetupDecode"/> and before decoding.
        /// </remarks>
        public virtual bool PreDecode(short plane)
        {
            return true;
        }

        /// <summary>
        /// Decodes one row of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool DecodeRow(byte[] buffer, int offset, int count, short plane)
        {
            return noDecode("scanline");
        }

        /// <summary>
        /// Decodes one strip of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool DecodeStrip(byte[] buffer, int offset, int count, short plane)
        {
            return noDecode("strip");
        }

        /// <summary>
        /// Decodes one tile of image data.
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="count">The number of decoded bytes that should be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was decoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool DecodeTile(byte[] buffer, int offset, int count, short plane)
        {
            return noDecode("tile");
        }

        /// <summary>
        /// Setups the encoder part of the codec.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this codec successfully setup its encoder part and can encode data;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// <b>SetupEncode</b> is called once before
        /// <see cref="PreEncode"/>.</remarks>
        public virtual bool SetupEncode()
        {
            return true;
        }

        /// <summary>
        /// Prepares the encoder part of the codec for a encoding.
        /// </summary>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns><c>true</c> if this codec successfully prepared its encoder part and ready
        /// to encode data; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <b>PreEncode</b> is called after <see cref="SetupEncode"/> and before encoding.
        /// </remarks>
        public virtual bool PreEncode(short plane)
        {
            return true;
        }

        /// <summary>
        /// Performs any actions after encoding required by the codec.
        /// </summary>
        /// <returns><c>true</c> if all post-encode actions succeeded; otherwise, <c>false</c></returns>
        /// <remarks>
        /// <b>PostEncode</b> is called after encoding and can be used to release any external 
        /// resources needed during encoding.
        /// </remarks>
        public virtual bool PostEncode()
        {
            return true;
        }

        /// <summary>
        /// Encodes one row of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool EncodeRow(byte[] buffer, int offset, int count, short plane)
        {
            return noEncode("scanline");
        }

        /// <summary>
        /// Encodes one strip of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool EncodeStrip(byte[] buffer, int offset, int count, short plane)
        {
            return noEncode("strip");
        }

        /// <summary>
        /// Encodes one tile of image data.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin read image data.</param>
        /// <param name="count">The maximum number of encoded bytes that can be placed
        /// to <paramref name="buffer"/>.</param>
        /// <param name="plane">The zero-based sample plane index.</param>
        /// <returns>
        /// 	<c>true</c> if image data was encoded successfully; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool EncodeTile(byte[] buffer, int offset, int count, short plane)
        {
            return noEncode("tile");
        }

        /// <summary>
        /// Flushes any internal data buffers and terminates current operation.
        /// </summary>
        public virtual void Close()
        {
        }

        /// <summary>
        /// Seeks the specified row in the strip being processed.
        /// </summary>
        /// <param name="row">The row to seek.</param>
        /// <returns><c>true</c> if specified row was successfully found; otherwise, <c>false</c></returns>
        public virtual bool Seek(int row)
        {
            Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                "Compression algorithm does not support random access");
            return false;
        }

        /// <summary>
        /// Cleanups the state of the codec.
        /// </summary>
        /// <remarks>
        /// <b>Cleanup</b> is called when codec is no longer needed (won't be used) and can be
        /// used for example to restore tag methods that were substituted.</remarks>
        public virtual void Cleanup()
        {
        }

        /// <summary>
        /// Calculates and/or constrains a strip size.
        /// </summary>
        /// <param name="size">The proposed strip size (may be zero or negative).</param>
        /// <returns>A strip size to use.</returns>
        public virtual int DefStripSize(int size)
        {
            if (size < 1)
            {
                // If RowsPerStrip is unspecified, try to break the image up into strips that are
                // approximately STRIP_SIZE_DEFAULT bytes long.
                int scanline = m_tif.ScanlineSize();
                size = Tiff.STRIP_SIZE_DEFAULT / (scanline == 0 ? 1 : scanline);
                if (size == 0)
                {
                    // very wide images
                    size = 1;
                }
            }

            return size;
        }

        /// <summary>
        /// Calculate and/or constrains a tile size
        /// </summary>
        /// <param name="width">The proposed tile width upon the call / tile width to use after the call.</param>
        /// <param name="height">The proposed tile height upon the call / tile height to use after the call.</param>
        public virtual void DefTileSize(ref int width, ref int height)
        {
            if (width < 1)
                width = 256;
            
            if (height < 1)
                height = 256;
            
            // roundup to a multiple of 16 per the spec
            if ((width & 0xf) != 0)
                width = Tiff.roundUp(width, 16);

            if ((height & 0xf) != 0)
                height = Tiff.roundUp(height, 16);
        }

        private bool noEncode(string method)
        {
            TiffCodec c = m_tif.FindCodec(m_tif.m_dir.td_compression);
            if (c != null)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "{0} {1} encoding is not implemented", c.m_name, method);
            }
            else
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "Compression scheme {0} {1} encoding is not implemented",
                    m_tif.m_dir.td_compression, method);
            }

            return false;
        }

        private bool noDecode(string method)
        {
            TiffCodec c = m_tif.FindCodec(m_tif.m_dir.td_compression);
            if (c != null)
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "{0} {1} decoding is not implemented", c.m_name, method);
            }
            else
            {
                Tiff.ErrorExt(m_tif, m_tif.m_clientdata, m_tif.m_name,
                    "Compression scheme {0} {1} decoding is not implemented",
                    m_tif.m_dir.td_compression, method);
            }

            return false;
        }
    }
}
