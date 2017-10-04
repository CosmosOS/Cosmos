//
// Copyright (C) 2001 Mike Krueger
// Copyright (C) 2004 John Reilly
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

// HISTORY
//	11-08-2009	GeoffHart	T9121	Added Multi-member gzip support

using System;
using System.IO;
using Orvid.Compression.Checksums;

namespace Orvid.Compression.Streams
{

    #region DeflaterInputStream
    /// <summary>
    /// This filter stream is used to decompress data compressed using the "deflate"
    /// format. The "deflate" format is described in RFC 1951.
    ///
    /// This stream may form the basis for other decompression filters, such
    /// as the <see cref="ICSharpCode.SharpZipLib.GZip.GZipInputStream">GZipInputStream</see>.
    ///
    /// Author of the original java version : John Leuner.
    /// </summary>
    public class DeflaterInputStream : Stream
    {
        #region Constructors
        /// <summary>
        /// Create an DeflaterInputStream with the default decompressor
        /// and a default buffer size of 4KB.
        /// </summary>
        /// <param name = "baseInputStream">
        /// The InputStream to read bytes from
        /// </param>
        public DeflaterInputStream(Stream baseInputStream)
            : this(baseInputStream, new Inflater(), 4096)
        {
        }

        /// <summary>
        /// Create an DeflaterInputStream with the specified decompressor
        /// and the specified buffer size.
        /// </summary>
        /// <param name = "baseInputStream">
        /// The InputStream to read bytes from
        /// </param>
        /// <param name = "inflater">
        /// The decompressor to use
        /// </param>
        /// <param name = "bufferSize">
        /// Size of the buffer to use
        /// </param>
        internal DeflaterInputStream(Stream baseInputStream, Inflater inflater, int bufferSize)
        {
            if (baseInputStream == null)
            {
                throw new ArgumentNullException("baseInputStream");
            }

            if (inflater == null)
            {
                throw new ArgumentNullException("inflater");
            }

            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            this.baseInputStream = baseInputStream;
            this.inf = inflater;

            inputBuffer = new InflaterInputBuffer(baseInputStream, bufferSize);
        }

        #endregion

        /// <summary>
        /// Get/set flag indicating ownership of underlying stream.
        /// When the flag is true <see cref="Close"/> will close the underlying stream also.
        /// </summary>
        /// <remarks>
        /// The default value is true.
        /// </remarks>
        public bool IsStreamOwner
        {
            get { return isStreamOwner; }
            set { isStreamOwner = value; }
        }

        /// <summary>
        /// Skip specified number of bytes of uncompressed data
        /// </summary>
        /// <param name ="count">
        /// Number of bytes to skip
        /// </param>
        /// <returns>
        /// The number of bytes skipped, zero if the end of 
        /// stream has been reached
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="count">The number of bytes</paramref> to skip is less than or equal to zero.
        /// </exception>
        public long Skip(long count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            // v0.80 Skip by seeking if underlying stream supports it...
            if (baseInputStream.CanSeek)
            {
                baseInputStream.Seek(count, SeekOrigin.Current);
                return count;
            }
            else
            {
                int length = 2048;
                if (count < length)
                {
                    length = (int)count;
                }

                byte[] tmp = new byte[length];
                int readCount = 1;
                long toSkip = count;

                while ((toSkip > 0) && (readCount > 0))
                {
                    if (toSkip < length)
                    {
                        length = (int)toSkip;
                    }

                    readCount = baseInputStream.Read(tmp, 0, length);
                    toSkip -= readCount;
                }

                return count - toSkip;
            }
        }

        /// <summary>
        /// Returns 0 once the end of the stream (EOF) has been reached.
        /// Otherwise returns 1.
        /// </summary>
        public virtual int Available
        {
            get
            {
                return inf.IsFinished ? 0 : 1;
            }
        }

        /// <summary>
        /// Fills the buffer with more data to decompress.
        /// </summary>
        /// <exception cref="SharpZipBaseException">
        /// Stream ends early
        /// </exception>
        protected void Fill()
        {
            // Protect against redundant calls
            if (inputBuffer.Available <= 0)
            {
                inputBuffer.Fill();
                if (inputBuffer.Available <= 0)
                {
                    throw new Exception("Unexpected EOF");
                }
            }
            inputBuffer.SetInflaterInput(inf);
        }

        #region Stream Overrides
        /// <summary>
        /// Gets a value indicating whether the current stream supports reading
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return baseInputStream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value of false indicating seeking is not supported for this stream.
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value of false indicating that this stream is not writeable.
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// A value representing the length of the stream in bytes.
        /// </summary>
        public override long Length
        {
            get
            {
                return inputBuffer.RawLength;
            }
        }

        /// <summary>
        /// The current position within the stream.
        /// Throws a NotSupportedException when attempting to set the position
        /// </summary>
        /// <exception cref="NotSupportedException">Attempting to set the position</exception>
        public override long Position
        {
            get
            {
                return baseInputStream.Position;
            }
            set
            {
                throw new NotSupportedException("InflaterInputStream Position not supported");
            }
        }

        /// <summary>
        /// Flushes the baseInputStream
        /// </summary>
        public override void Flush()
        {
            baseInputStream.Flush();
        }

        /// <summary>
        /// Sets the position within the current stream
        /// Always throws a NotSupportedException
        /// </summary>
        /// <param name="offset">The relative offset to seek to.</param>
        /// <param name="origin">The <see cref="SeekOrigin"/> defining where to seek from.</param>
        /// <returns>The new position in the stream.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Seek not supported");
        }

        /// <summary>
        /// Set the length of the current stream
        /// Always throws a NotSupportedException
        /// </summary>
        /// <param name="value">The new length value for the stream.</param>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("InflaterInputStream SetLength not supported");
        }

        /// <summary>
        /// Writes a sequence of bytes to stream and advances the current position
        /// This method always throws a NotSupportedException
        /// </summary>
        /// <param name="buffer">Thew buffer containing data to write.</param>
        /// <param name="offset">The offset of the first byte to write.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("InflaterInputStream Write not supported");
        }

        /// <summary>
        /// Writes one byte to the current stream and advances the current position
        /// Always throws a NotSupportedException
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void WriteByte(byte value)
        {
            throw new NotSupportedException("InflaterInputStream WriteByte not supported");
        }

        /// <summary>
        /// Entry point to begin an asynchronous write.  Always throws a NotSupportedException.
        /// </summary>
        /// <param name="buffer">The buffer to write data from</param>
        /// <param name="offset">Offset of first byte to write</param>
        /// <param name="count">The maximum number of bytes to write</param>
        /// <param name="callback">The method to be called when the asynchronous write operation is completed</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests</param>
        /// <returns>An <see cref="System.IAsyncResult">IAsyncResult</see> that references the asynchronous write</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("InflaterInputStream BeginWrite not supported");
        }

        /// <summary>
        /// Closes the input stream.  When <see cref="IsStreamOwner"></see>
        /// is true the underlying stream is also closed.
        /// </summary>
        public override void Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                if (isStreamOwner)
                {
                    baseInputStream.Close();
                }
            }
        }

        /// <summary>
        /// Reads decompressed data into the provided buffer byte array
        /// </summary>
        /// <param name ="buffer">
        /// The array to read and decompress data into
        /// </param>
        /// <param name ="offset">
        /// The offset indicating where the data should be placed
        /// </param>
        /// <param name ="count">
        /// The number of bytes to decompress
        /// </param>
        /// <returns>The number of bytes read.  Zero signals the end of stream</returns>
        /// <exception cref="SharpZipBaseException">
        /// Inflater needs a dictionary
        /// </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (inf.IsNeedingDictionary)
            {
                throw new Exception("Need a dictionary");
            }

            int remainingBytes = count;
            while (true)
            {
                int bytesRead = inf.Inflate(buffer, offset, remainingBytes);
                offset += bytesRead;
                remainingBytes -= bytesRead;

                if (remainingBytes == 0 || inf.IsFinished)
                {
                    break;
                }

                if (inf.IsNeedingInput)
                {
                    Fill();
                }
                else if (bytesRead == 0)
                {
                    throw new Exception("Dont know what to do");
                }
            }
            return count - remainingBytes;
        }
        #endregion

        #region Instance Fields
        /// <summary>
        /// Decompressor for this stream
        /// </summary>
        internal Inflater inf;

        /// <summary>
        /// <see cref="InflaterInputBuffer">Input buffer</see> for this stream.
        /// </summary>
        internal InflaterInputBuffer inputBuffer;

        /// <summary>
        /// Base stream the inflater reads from.
        /// </summary>
        private Stream baseInputStream;

        /// <summary>
        /// The compressed size
        /// </summary>
        protected long csize;

        /// <summary>
        /// Flag indicating wether this instance has been closed or not.
        /// </summary>
        bool isClosed;

        /// <summary>
        /// Flag indicating wether this instance is designated the stream owner.
        /// When closing if this flag is true the underlying stream is closed.
        /// </summary>
        bool isStreamOwner = true;
        #endregion
    }
    #endregion

    #region DeflaterOutputStream
    /// <summary>
    /// A special stream deflating or compressing the bytes that are
    /// written to it.  It uses a Deflater to perform actual deflating.<br/>
    /// Authors of the original java version : Tom Tromey, Jochen Hoenicke 
    /// </summary>
    public class DeflaterOutputStream : Stream
    {
        #region Constructors
        /// <summary>
        /// Creates a new DeflaterOutputStream with a default Deflater and default buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// the output stream where deflated output should be written.
        /// </param>
        public DeflaterOutputStream(Stream baseOutputStream)
            : this(baseOutputStream, new Deflater(), 512)
        {
        }

        /// <summary>
        /// Creates a new DeflaterOutputStream with the given Deflater and
        /// buffer size.
        /// </summary>
        /// <param name="baseOutputStream">
        /// The output stream where deflated output is written.
        /// </param>
        /// <param name="deflater">
        /// The underlying deflater to use
        /// </param>
        /// <param name="bufferSize">
        /// The buffer size in bytes to use when deflating (minimum value 512)
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// bufsize is less than or equal to zero.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// baseOutputStream does not support writing
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// deflater instance is null
        /// </exception>
        internal DeflaterOutputStream(Stream baseOutputStream, Deflater deflater, int bufferSize)
        {
            if (baseOutputStream == null)
            {
                throw new ArgumentNullException("baseOutputStream");
            }

            if (baseOutputStream.CanWrite == false)
            {
                throw new ArgumentException("Must support writing", "baseOutputStream");
            }

            if (deflater == null)
            {
                throw new ArgumentNullException("deflater");
            }

            if (bufferSize < 512)
            {
                throw new ArgumentOutOfRangeException("bufferSize");
            }

            baseOutputStream_ = baseOutputStream;
            buffer_ = new byte[bufferSize];
            deflater_ = deflater;
        }
        #endregion

        #region Public API
        /// <summary>
        /// Finishes the stream by calling finish() on the deflater. 
        /// </summary>
        /// <exception cref="SharpZipBaseException">
        /// Not all input is deflated
        /// </exception>
        public virtual void Finish()
        {
            deflater_.Finish();
            while (!deflater_.IsFinished)
            {
                int len = deflater_.Deflate(buffer_, 0, buffer_.Length);
                if (len <= 0)
                {
                    break;
                }
                baseOutputStream_.Write(buffer_, 0, len);
            }

            if (!deflater_.IsFinished)
            {
                throw new Exception("Can't deflate all input?");
            }

            baseOutputStream_.Flush();

        }

        /// <summary>
        /// Get/set flag indicating ownership of the underlying stream.
        /// When the flag is true <see cref="Close"></see> will close the underlying stream also.
        /// </summary>
        public bool IsStreamOwner
        {
            get { return isStreamOwner_; }
            set { isStreamOwner_ = value; }
        }

        ///	<summary>
        /// Allows client to determine if an entry can be patched after its added
        /// </summary>
        public bool CanPatchEntries
        {
            get
            {
                return baseOutputStream_.CanSeek;
            }
        }

        #endregion


        #region Deflation Support
        /// <summary>
        /// Deflates everything in the input buffers.  This will call
        /// <code>def.deflate()</code> until all bytes from the input buffers
        /// are processed.
        /// </summary>
        protected void Deflate()
        {
            while (!deflater_.IsNeedingInput)
            {
                int deflateCount = deflater_.Deflate(buffer_, 0, buffer_.Length);

                if (deflateCount <= 0)
                {
                    break;
                }

                baseOutputStream_.Write(buffer_, 0, deflateCount);
            }

            if (!deflater_.IsNeedingInput)
            {
                throw new Exception("DeflaterOutputStream can't deflate all input?");
            }
        }
        #endregion

        #region Stream Overrides
        /// <summary>
        /// Gets value indicating stream can be read from
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a value indicating if seeking is supported for this stream
        /// This property always returns false
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Get value indicating if this stream supports writing
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return baseOutputStream_.CanWrite;
            }
        }

        /// <summary>
        /// Get current length of stream
        /// </summary>
        public override long Length
        {
            get
            {
                return baseOutputStream_.Length;
            }
        }

        /// <summary>
        /// Gets the current position within the stream.
        /// </summary>
        /// <exception cref="NotSupportedException">Any attempt to set position</exception>
        public override long Position
        {
            get
            {
                return baseOutputStream_.Position;
            }
            set
            {
                throw new NotSupportedException("Position property not supported");
            }
        }

        /// <summary>
        /// Sets the current position of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <param name="offset">The offset relative to the <paramref name="origin"/> to seek.</param>
        /// <param name="origin">The <see cref="SeekOrigin"/> to seek from.</param>
        /// <returns>The new position in the stream.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("DeflaterOutputStream Seek not supported");
        }

        /// <summary>
        /// Sets the length of this stream to the given value. Not supported by this class!
        /// </summary>
        /// <param name="value">The new stream length.</param>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("DeflaterOutputStream SetLength not supported");
        }

        /// <summary>
        /// Read a byte from stream advancing position by one
        /// </summary>
        /// <returns>The byte read cast to an int.  THe value is -1 if at the end of the stream.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int ReadByte()
        {
            throw new NotSupportedException("DeflaterOutputStream ReadByte not supported");
        }

        /// <summary>
        /// Read a block of bytes from stream
        /// </summary>
        /// <param name="buffer">The buffer to store read data in.</param>
        /// <param name="offset">The offset to start storing at.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <returns>The actual number of bytes read.  Zero if end of stream is detected.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("DeflaterOutputStream Read not supported");
        }

        /// <summary>
        /// Asynchronous reads are not supported a NotSupportedException is always thrown
        /// </summary>
        /// <param name="buffer">The buffer to read into.</param>
        /// <param name="offset">The offset to start storing data at.</param>
        /// <param name="count">The number of bytes to read</param>
        /// <param name="callback">The async callback to use.</param>
        /// <param name="state">The state to use.</param>
        /// <returns>Returns an <see cref="IAsyncResult"/></returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("DeflaterOutputStream BeginRead not currently supported");
        }

        /// <summary>
        /// Asynchronous writes arent supported, a NotSupportedException is always thrown
        /// </summary>
        /// <param name="buffer">The buffer to write.</param>
        /// <param name="offset">The offset to begin writing at.</param>
        /// <param name="count">The number of bytes to write.</param>
        /// <param name="callback">The <see cref="AsyncCallback"/> to use.</param>
        /// <param name="state">The state object.</param>
        /// <returns>Returns an IAsyncResult.</returns>
        /// <exception cref="NotSupportedException">Any access</exception>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException("BeginWrite is not supported");
        }

        /// <summary>
        /// Flushes the stream by calling <see cref="DeflaterOutputStream.Flush">Flush</see> on the deflater and then
        /// on the underlying stream.  This ensures that all bytes are flushed.
        /// </summary>
        public override void Flush()
        {
            deflater_.Flush();
            Deflate();
            baseOutputStream_.Flush();
        }

        /// <summary>
        /// Calls <see cref="Finish"/> and closes the underlying
        /// stream when <see cref="IsStreamOwner"></see> is true.
        /// </summary>
        public override void Close()
        {
            if (!isClosed_)
            {
                isClosed_ = true;

                try
                {
                    Finish();
                }
                finally
                {
                    if (isStreamOwner_)
                    {
                        baseOutputStream_.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Writes a single byte to the compressed output stream.
        /// </summary>
        /// <param name="value">
        /// The byte value.
        /// </param>
        public override void WriteByte(byte value)
        {
            byte[] b = new byte[1];
            b[0] = value;
            Write(b, 0, 1);
        }

        /// <summary>
        /// Writes bytes from an array to the compressed stream.
        /// </summary>
        /// <param name="buffer">
        /// The byte array
        /// </param>
        /// <param name="offset">
        /// The offset into the byte array where to start.
        /// </param>
        /// <param name="count">
        /// The number of bytes to write.
        /// </param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            deflater_.SetInput(buffer, offset, count);
            Deflate();
        }
        #endregion

        #region Instance Fields
        /// <summary>
        /// This buffer is used temporarily to retrieve the bytes from the
        /// deflater and write them to the underlying output stream.
        /// </summary>
        byte[] buffer_;

        /// <summary>
        /// The deflater which is used to deflate the stream.
        /// </summary>
        internal Deflater deflater_;

        /// <summary>
        /// Base stream the deflater depends on.
        /// </summary>
        protected Stream baseOutputStream_;

        bool isClosed_;

        bool isStreamOwner_ = true;
        #endregion
    }
    #endregion




    #region InflaterHuffmanTree
    internal class InflaterHuffmanTree
    {
        const int MAX_BITLEN = 15;
        short[] tree;
        public static InflaterHuffmanTree defLitLenTree;
        public static InflaterHuffmanTree defDistTree;

        static InflaterHuffmanTree()
        {
            try
            {
                byte[] codeLengths = new byte[288];
                int i = 0;
                while (i < 144)
                {
                    codeLengths[i++] = 8;
                }
                while (i < 256)
                {
                    codeLengths[i++] = 9;
                }
                while (i < 280)
                {
                    codeLengths[i++] = 7;
                }
                while (i < 288)
                {
                    codeLengths[i++] = 8;
                }
                defLitLenTree = new InflaterHuffmanTree(codeLengths);

                codeLengths = new byte[32];
                i = 0;
                while (i < 32)
                {
                    codeLengths[i++] = 5;
                }
                defDistTree = new InflaterHuffmanTree(codeLengths);
            }
            catch (Exception)
            {
                throw new Exception("InflaterHuffmanTree: static tree length illegal");
            }
        }

        public InflaterHuffmanTree(byte[] codeLengths)
        {
            BuildTree(codeLengths);
        }

        void BuildTree(byte[] codeLengths)
        {
            int[] blCount = new int[MAX_BITLEN + 1];
            int[] nextCode = new int[MAX_BITLEN + 1];

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits > 0)
                {
                    blCount[bits]++;
                }
            }

            int code = 0;
            int treeSize = 512;
            for (int bits = 1; bits <= MAX_BITLEN; bits++)
            {
                nextCode[bits] = code;
                code += blCount[bits] << (16 - bits);
                if (bits >= 10)
                {
                    int start = nextCode[bits] & 0x1ff80;
                    int end = code & 0x1ff80;
                    treeSize += (end - start) >> (16 - bits);
                }
            }

            tree = new short[treeSize];
            int treePtr = 512;
            for (int bits = MAX_BITLEN; bits >= 10; bits--)
            {
                int end = code & 0x1ff80;
                code -= blCount[bits] << (16 - bits);
                int start = code & 0x1ff80;
                for (int i = start; i < end; i += 1 << 7)
                {
                    tree[DeflaterHuffman.BitReverse(i)] = (short)((-treePtr << 4) | bits);
                    treePtr += 1 << (bits - 9);
                }
            }

            for (int i = 0; i < codeLengths.Length; i++)
            {
                int bits = codeLengths[i];
                if (bits == 0)
                {
                    continue;
                }
                code = nextCode[bits];
                int revcode = DeflaterHuffman.BitReverse(code);
                if (bits <= 9)
                {
                    do
                    {
                        tree[revcode] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < 512);
                }
                else
                {
                    int subTree = tree[revcode & 511];
                    int treeLen = 1 << (subTree & 15);
                    subTree = -(subTree >> 4);
                    do
                    {
                        tree[subTree | (revcode >> 9)] = (short)((i << 4) | bits);
                        revcode += 1 << bits;
                    } while (revcode < treeLen);
                }
                nextCode[bits] = code + (1 << (16 - bits));
            }

        }

        public int GetSymbol(StreamManipulator input)
        {
            int lookahead, symbol;
            if ((lookahead = input.PeekBits(9)) >= 0)
            {
                if ((symbol = tree[lookahead]) >= 0)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                int subtree = -(symbol >> 4);
                int bitlen = symbol & 15;
                if ((lookahead = input.PeekBits(bitlen)) >= 0)
                {
                    symbol = tree[subtree | (lookahead >> 9)];
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    int bits = input.AvailableBits;
                    lookahead = input.PeekBits(bits);
                    symbol = tree[subtree | (lookahead >> 9)];
                    if ((symbol & 15) <= bits)
                    {
                        input.DropBits(symbol & 15);
                        return symbol >> 4;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            else
            {
                int bits = input.AvailableBits;
                lookahead = input.PeekBits(bits);
                symbol = tree[lookahead];
                if (symbol >= 0 && (symbol & 15) <= bits)
                {
                    input.DropBits(symbol & 15);
                    return symbol >> 4;
                }
                else
                {
                    return -1;
                }
            }
        }
    }
    #endregion

    #region InflaterDynHeader
    internal class InflaterDynHeader
    {
        const int LNUM = 0;
        const int DNUM = 1;
        const int BLNUM = 2;
        const int BLLENS = 3;
        const int LENS = 4;
        const int REPS = 5;
        static readonly int[] repMin = { 3, 3, 11 };
        static readonly int[] repBits = { 2, 3, 7 };
        static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };


        public InflaterDynHeader() { }

        public bool Decode(StreamManipulator input)
        {
        decode_loop:
            for (; ; )
            {
                switch (mode)
                {
                    case LNUM:
                        lnum = input.PeekBits(5);
                        if (lnum < 0)
                        {
                            return false;
                        }
                        lnum += 257;
                        input.DropBits(5);
                        mode = DNUM;
                        goto case DNUM;
                    case DNUM:
                        dnum = input.PeekBits(5);
                        if (dnum < 0)
                        {
                            return false;
                        }
                        dnum++;
                        input.DropBits(5);
                        num = lnum + dnum;
                        litdistLens = new byte[num];
                        mode = BLNUM;
                        goto case BLNUM;
                    case BLNUM:
                        blnum = input.PeekBits(4);
                        if (blnum < 0)
                        {
                            return false;
                        }
                        blnum += 4;
                        input.DropBits(4);
                        blLens = new byte[19];
                        ptr = 0;
                        mode = BLLENS;
                        goto case BLLENS;
                    case BLLENS:
                        while (ptr < blnum)
                        {
                            int len = input.PeekBits(3);
                            if (len < 0)
                            {
                                return false;
                            }
                            input.DropBits(3);
                            blLens[BL_ORDER[ptr]] = (byte)len;
                            ptr++;
                        }
                        blTree = new InflaterHuffmanTree(blLens);
                        blLens = null;
                        ptr = 0;
                        mode = LENS;
                        goto case LENS;
                    case LENS:
                        {
                            int symbol;
                            while (((symbol = blTree.GetSymbol(input)) & ~15) == 0)
                            {
                                litdistLens[ptr++] = lastLen = (byte)symbol;

                                if (ptr == num)
                                {
                                    return true;
                                }
                            }

                            if (symbol < 0)
                            {
                                return false;
                            }

                            if (symbol >= 17)
                            {
                                lastLen = 0;
                            }
                            else
                            {
                                if (ptr == 0)
                                {
                                    throw new Exception();
                                }
                            }
                            repSymbol = symbol - 16;
                        }
                        mode = REPS;
                        goto case REPS;
                    case REPS:
                        {
                            int bits = repBits[repSymbol];
                            int count = input.PeekBits(bits);
                            if (count < 0)
                            {
                                return false;
                            }
                            input.DropBits(bits);
                            count += repMin[repSymbol];

                            if (ptr + count > num)
                            {
                                throw new Exception();
                            }
                            while (count-- > 0)
                            {
                                litdistLens[ptr++] = lastLen;
                            }

                            if (ptr == num)
                            {
                                return true;
                            }
                        }
                        mode = LENS;
                        goto decode_loop;
                }
            }
        }

        public InflaterHuffmanTree BuildLitLenTree()
        {
            byte[] litlenLens = new byte[lnum];
            Array.Copy(litdistLens, 0, litlenLens, 0, lnum);
            return new InflaterHuffmanTree(litlenLens);
        }

        public InflaterHuffmanTree BuildDistTree()
        {
            byte[] distLens = new byte[dnum];
            Array.Copy(litdistLens, lnum, distLens, 0, dnum);
            return new InflaterHuffmanTree(distLens);
        }

        byte[] blLens;
        byte[] litdistLens;

        InflaterHuffmanTree blTree;

        int mode;
        int lnum, dnum, blnum, num;
        int repSymbol;
        byte lastLen;
        int ptr;

    }
    #endregion

    #region StreamManipulator
    internal class StreamManipulator
    {
        public StreamManipulator() { }

        public int PeekBits(int bitCount)
        {
            if (bitsInBuffer_ < bitCount)
            {
                if (windowStart_ == windowEnd_)
                {
                    return -1;
                }
                buffer_ |= (uint)((window_[windowStart_++] & 0xff |
                                 (window_[windowStart_++] & 0xff) << 8) << bitsInBuffer_);
                bitsInBuffer_ += 16;
            }
            return (int)(buffer_ & ((1 << bitCount) - 1));
        }

        public void DropBits(int bitCount)
        {
            buffer_ >>= bitCount;
            bitsInBuffer_ -= bitCount;
        }

        public int GetBits(int bitCount)
        {
            int bits = PeekBits(bitCount);
            if (bits >= 0)
            {
                DropBits(bitCount);
            }
            return bits;
        }

        public int AvailableBits
        {
            get
            {
                return bitsInBuffer_;
            }
        }

        public int AvailableBytes
        {
            get
            {
                return windowEnd_ - windowStart_ + (bitsInBuffer_ >> 3);
            }
        }

        public void SkipToByteBoundary()
        {
            buffer_ >>= (bitsInBuffer_ & 7);
            bitsInBuffer_ &= ~7;
        }

        public bool IsNeedingInput
        {
            get
            {
                return windowStart_ == windowEnd_;
            }
        }

        public int CopyBytes(byte[] output, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            if ((bitsInBuffer_ & 7) != 0)
            {
                throw new InvalidOperationException("Bit buffer is not byte aligned!");
            }

            int count = 0;
            while ((bitsInBuffer_ > 0) && (length > 0))
            {
                output[offset++] = (byte)buffer_;
                buffer_ >>= 8;
                bitsInBuffer_ -= 8;
                length--;
                count++;
            }

            if (length == 0)
            {
                return count;
            }

            int avail = windowEnd_ - windowStart_;
            if (length > avail)
            {
                length = avail;
            }
            System.Array.Copy(window_, windowStart_, output, offset, length);
            windowStart_ += length;

            if (((windowStart_ - windowEnd_) & 1) != 0)
            {
                buffer_ = (uint)(window_[windowStart_++] & 0xff);
                bitsInBuffer_ = 8;
            }
            return count + length;
        }

        public void Reset()
        {
            buffer_ = 0;
            windowStart_ = windowEnd_ = bitsInBuffer_ = 0;
        }

        public void SetInput(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "Cannot be negative");
            }

            if (windowStart_ < windowEnd_)
            {
                throw new InvalidOperationException("Old input was not completely processed");
            }

            int end = offset + count;

            if ((offset > end) || (end > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if ((count & 1) != 0)
            {
                buffer_ |= (uint)((buffer[offset++] & 0xff) << bitsInBuffer_);
                bitsInBuffer_ += 8;
            }

            window_ = buffer;
            windowStart_ = offset;
            windowEnd_ = end;
        }

        private byte[] window_;
        private int windowStart_;
        private int windowEnd_;
        private uint buffer_;
        private int bitsInBuffer_;
    }
    #endregion

    #region PendingBuffer
    internal class PendingBuffer
    {
        #region Instance Fields
        byte[] buffer_;
        int start;
        int end;
        uint bits;
        int bitCount;
        #endregion

        #region Constructors
        public PendingBuffer() : this(4096)
        {
        }

        public PendingBuffer(int bufferSize)
        {
            buffer_ = new byte[bufferSize];
        }
        #endregion

        public void Reset()
        {
            start = end = bitCount = 0;
        }

        public void WriteByte(int value)
        {
            buffer_[end++] = unchecked((byte)value);
        }

        public void WriteShort(int value)
        {
            buffer_[end++] = unchecked((byte)value);
            buffer_[end++] = unchecked((byte)(value >> 8));
        }

        public void WriteInt(int value)
        {
            buffer_[end++] = unchecked((byte)value);
            buffer_[end++] = unchecked((byte)(value >> 8));
            buffer_[end++] = unchecked((byte)(value >> 16));
            buffer_[end++] = unchecked((byte)(value >> 24));
        }

        public void WriteBlock(byte[] block, int offset, int length)
        {
            System.Array.Copy(block, offset, buffer_, end, length);
            end += length;
        }

        public int BitCount
        {
            get
            {
                return bitCount;
            }
        }

        public void AlignToByte()
        {
            if (bitCount > 0)
            {
                buffer_[end++] = unchecked((byte)bits);
                if (bitCount > 8)
                {
                    buffer_[end++] = unchecked((byte)(bits >> 8));
                }
            }
            bits = 0;
            bitCount = 0;
        }

        public void WriteBits(int b, int count)
        {
            bits |= (uint)(b << bitCount);
            bitCount += count;
            if (bitCount >= 16)
            {
                buffer_[end++] = unchecked((byte)bits);
                buffer_[end++] = unchecked((byte)(bits >> 8));
                bits >>= 16;
                bitCount -= 16;
            }
        }

        public void WriteShortMSB(int s)
        {
            buffer_[end++] = unchecked((byte)(s >> 8));
            buffer_[end++] = unchecked((byte)s);
        }

        public bool IsFlushed
        {
            get
            {
                return end == 0;
            }
        }

        public int Flush(byte[] output, int offset, int length)
        {
            if (bitCount >= 8)
            {
                buffer_[end++] = unchecked((byte)bits);
                bits >>= 8;
                bitCount -= 8;
            }

            if (length > end - start)
            {
                length = end - start;
                System.Array.Copy(buffer_, start, output, offset, length);
                start = 0;
                end = 0;
            }
            else
            {
                System.Array.Copy(buffer_, start, output, offset, length);
                start += length;
            }
            return length;
        }

        public byte[] ToByteArray()
        {
            byte[] result = new byte[end - start];
            System.Array.Copy(buffer_, start, result, 0, result.Length);
            start = 0;
            end = 0;
            return result;
        }
    }
    #endregion

    #region DeflaterPending
    internal class DeflaterPending : PendingBuffer
    {
        public DeflaterPending() : base(DeflaterConstants.PENDING_BUF_SIZE)
        {
        }
    }
    #endregion

    #region DeflaterHuffman
    /// <summary>
    /// This is the DeflaterHuffman class.
    /// 
    /// This class is <i>not</i> thread safe.  This is inherent in the API, due
    /// to the split of Deflate and SetInput.
    /// 
    /// author of the original java version : Jochen Hoenicke
    /// </summary>
    internal class DeflaterHuffman
    {
        const int BUFSIZE = 1 << (DeflaterConstants.DEFAULT_MEM_LEVEL + 6);
        const int LITERAL_NUM = 286;
        const int DIST_NUM = 30;
        const int BITLEN_NUM = 19;
        const int REP_3_6 = 16;
        const int REP_3_10 = 17;
        const int REP_11_138 = 18;
        const int EOF_SYMBOL = 256;
        static readonly int[] BL_ORDER = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

        static readonly byte[] bit4Reverse = {
			0,
			8,
			4,
			12,
			2,
			10,
			6,
			14,
			1,
			9,
			5,
			13,
			3,
			11,
			7,
			15
		};

        static short[] staticLCodes;
        static byte[] staticLLength;
        static short[] staticDCodes;
        static byte[] staticDLength;

        class Tree
        {
            #region Instance Fields
            public short[] freqs;

            public byte[] length;

            public int minNumCodes;

            public int numCodes;

            short[] codes;
            int[] bl_counts;
            int maxLength;
            DeflaterHuffman dh;
            #endregion

            #region Constructors
            public Tree(DeflaterHuffman dh, int elems, int minCodes, int maxLength)
            {
                this.dh = dh;
                this.minNumCodes = minCodes;
                this.maxLength = maxLength;
                freqs = new short[elems];
                bl_counts = new int[maxLength];
            }

            #endregion

            /// <summary>
            /// Resets the internal state of the tree
            /// </summary>
            public void Reset()
            {
                for (int i = 0; i < freqs.Length; i++)
                {
                    freqs[i] = 0;
                }
                codes = null;
                length = null;
            }

            public void WriteSymbol(int code)
            {
                dh.pending.WriteBits(codes[code] & 0xffff, length[code]);
            }

            /// <summary>
            /// Check that all frequencies are zero
            /// </summary>
            /// <exception cref="SharpZipBaseException">
            /// At least one frequency is non-zero
            /// </exception>
            public void CheckEmpty()
            {
                bool empty = true;
                for (int i = 0; i < freqs.Length; i++)
                {
                    if (freqs[i] != 0)
                    {
                        empty = false;
                    }
                }

                if (!empty)
                {
                    throw new Exception("!Empty");
                }
            }

            /// <summary>
            /// Set static codes and length
            /// </summary>
            /// <param name="staticCodes">new codes</param>
            /// <param name="staticLengths">length for new codes</param>
            public void SetStaticCodes(short[] staticCodes, byte[] staticLengths)
            {
                codes = staticCodes;
                length = staticLengths;
            }

            /// <summary>
            /// Build dynamic codes and lengths
            /// </summary>
            public void BuildCodes()
            {
                int numSymbols = freqs.Length;
                int[] nextCode = new int[maxLength];
                int code = 0;

                codes = new short[freqs.Length];

                for (int bits = 0; bits < maxLength; bits++)
                {
                    nextCode[bits] = code;
                    code += bl_counts[bits] << (15 - bits);
                }
                for (int i = 0; i < numCodes; i++)
                {
                    int bits = length[i];
                    if (bits > 0)
                    {

                        codes[i] = BitReverse(nextCode[bits - 1]);
                        nextCode[bits - 1] += 1 << (16 - bits);
                    }
                }
            }

            public void BuildTree()
            {
                int numSymbols = freqs.Length;

                int[] heap = new int[numSymbols];
                int heapLen = 0;
                int maxCode = 0;
                for (int n = 0; n < numSymbols; n++)
                {
                    int freq = freqs[n];
                    if (freq != 0)
                    {
                        int pos = heapLen++;
                        int ppos;
                        while (pos > 0 && freqs[heap[ppos = (pos - 1) / 2]] > freq)
                        {
                            heap[pos] = heap[ppos];
                            pos = ppos;
                        }
                        heap[pos] = n;

                        maxCode = n;
                    }
                }

                while (heapLen < 2)
                {
                    int node = maxCode < 2 ? ++maxCode : 0;
                    heap[heapLen++] = node;
                }

                numCodes = Math.Max(maxCode + 1, minNumCodes);

                int numLeafs = heapLen;
                int[] childs = new int[4 * heapLen - 2];
                int[] values = new int[2 * heapLen - 1];
                int numNodes = numLeafs;
                for (int i = 0; i < heapLen; i++)
                {
                    int node = heap[i];
                    childs[2 * i] = node;
                    childs[2 * i + 1] = -1;
                    values[i] = freqs[node] << 8;
                    heap[i] = i;
                }

                do
                {
                    int first = heap[0];
                    int last = heap[--heapLen];

                    int ppos = 0;
                    int path = 1;

                    while (path < heapLen)
                    {
                        if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                        {
                            path++;
                        }

                        heap[ppos] = heap[path];
                        ppos = path;
                        path = path * 2 + 1;
                    }

                    int lastVal = values[last];
                    while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                    {
                        heap[path] = heap[ppos];
                    }
                    heap[path] = last;


                    int second = heap[0];

                    last = numNodes++;
                    childs[2 * last] = first;
                    childs[2 * last + 1] = second;
                    int mindepth = Math.Min(values[first] & 0xff, values[second] & 0xff);
                    values[last] = lastVal = values[first] + values[second] - mindepth + 1;

                    ppos = 0;
                    path = 1;

                    while (path < heapLen)
                    {
                        if (path + 1 < heapLen && values[heap[path]] > values[heap[path + 1]])
                        {
                            path++;
                        }

                        heap[ppos] = heap[path];
                        ppos = path;
                        path = ppos * 2 + 1;
                    }

                    while ((path = ppos) > 0 && values[heap[ppos = (path - 1) / 2]] > lastVal)
                    {
                        heap[path] = heap[ppos];
                    }
                    heap[path] = last;
                } while (heapLen > 1);

                if (heap[0] != childs.Length / 2 - 1)
                {
                    throw new Exception("Heap invariant violated");
                }

                BuildLength(childs);
            }

            /// <summary>
            /// Get encoded length
            /// </summary>
            /// <returns>Encoded length, the sum of frequencies * lengths</returns>
            public int GetEncodedLength()
            {
                int len = 0;
                for (int i = 0; i < freqs.Length; i++)
                {
                    len += freqs[i] * length[i];
                }
                return len;
            }

            /// <summary>
            /// Scan a literal or distance tree to determine the frequencies of the codes
            /// in the bit length tree.
            /// </summary>
            public void CalcBLFreq(Tree blTree)
            {
                int max_count;               /* max repeat count */
                int min_count;               /* min repeat count */
                int count;                   /* repeat count of the current code */
                int curlen = -1;             /* length of current code */

                int i = 0;
                while (i < numCodes)
                {
                    count = 1;
                    int nextlen = length[i];
                    if (nextlen == 0)
                    {
                        max_count = 138;
                        min_count = 3;
                    }
                    else
                    {
                        max_count = 6;
                        min_count = 3;
                        if (curlen != nextlen)
                        {
                            blTree.freqs[nextlen]++;
                            count = 0;
                        }
                    }
                    curlen = nextlen;
                    i++;

                    while (i < numCodes && curlen == length[i])
                    {
                        i++;
                        if (++count >= max_count)
                        {
                            break;
                        }
                    }

                    if (count < min_count)
                    {
                        blTree.freqs[curlen] += (short)count;
                    }
                    else if (curlen != 0)
                    {
                        blTree.freqs[REP_3_6]++;
                    }
                    else if (count <= 10)
                    {
                        blTree.freqs[REP_3_10]++;
                    }
                    else
                    {
                        blTree.freqs[REP_11_138]++;
                    }
                }
            }

            /// <summary>
            /// Write tree values
            /// </summary>
            /// <param name="blTree">Tree to write</param>
            public void WriteTree(Tree blTree)
            {
                int max_count;               // max repeat count
                int min_count;               // min repeat count
                int count;                   // repeat count of the current code
                int curlen = -1;             // length of current code

                int i = 0;
                while (i < numCodes)
                {
                    count = 1;
                    int nextlen = length[i];
                    if (nextlen == 0)
                    {
                        max_count = 138;
                        min_count = 3;
                    }
                    else
                    {
                        max_count = 6;
                        min_count = 3;
                        if (curlen != nextlen)
                        {
                            blTree.WriteSymbol(nextlen);
                            count = 0;
                        }
                    }
                    curlen = nextlen;
                    i++;

                    while (i < numCodes && curlen == length[i])
                    {
                        i++;
                        if (++count >= max_count)
                        {
                            break;
                        }
                    }

                    if (count < min_count)
                    {
                        while (count-- > 0)
                        {
                            blTree.WriteSymbol(curlen);
                        }
                    }
                    else if (curlen != 0)
                    {
                        blTree.WriteSymbol(REP_3_6);
                        dh.pending.WriteBits(count - 3, 2);
                    }
                    else if (count <= 10)
                    {
                        blTree.WriteSymbol(REP_3_10);
                        dh.pending.WriteBits(count - 3, 3);
                    }
                    else
                    {
                        blTree.WriteSymbol(REP_11_138);
                        dh.pending.WriteBits(count - 11, 7);
                    }
                }
            }

            void BuildLength(int[] childs)
            {
                this.length = new byte[freqs.Length];
                int numNodes = childs.Length / 2;
                int numLeafs = (numNodes + 1) / 2;
                int overflow = 0;

                for (int i = 0; i < maxLength; i++)
                {
                    bl_counts[i] = 0;
                }

                int[] lengths = new int[numNodes];
                lengths[numNodes - 1] = 0;

                for (int i = numNodes - 1; i >= 0; i--)
                {
                    if (childs[2 * i + 1] != -1)
                    {
                        int bitLength = lengths[i] + 1;
                        if (bitLength > maxLength)
                        {
                            bitLength = maxLength;
                            overflow++;
                        }
                        lengths[childs[2 * i]] = lengths[childs[2 * i + 1]] = bitLength;
                    }
                    else
                    {
                        int bitLength = lengths[i];
                        bl_counts[bitLength - 1]++;
                        this.length[childs[2 * i]] = (byte)lengths[i];
                    }
                }

                if (overflow == 0)
                {
                    return;
                }

                int incrBitLen = maxLength - 1;
                do
                {
                    while (bl_counts[--incrBitLen] == 0)
                        ;

                    do
                    {
                        bl_counts[incrBitLen]--;
                        bl_counts[++incrBitLen]++;
                        overflow -= 1 << (maxLength - 1 - incrBitLen);
                    } while (overflow > 0 && incrBitLen < maxLength - 1);
                } while (overflow > 0);

                bl_counts[maxLength - 1] += overflow;
                bl_counts[maxLength - 2] -= overflow;

                int nodePtr = 2 * numLeafs;
                for (int bits = maxLength; bits != 0; bits--)
                {
                    int n = bl_counts[bits - 1];
                    while (n > 0)
                    {
                        int childPtr = 2 * childs[nodePtr++];
                        if (childs[childPtr + 1] == -1)
                        {
                            length[childs[childPtr]] = (byte)bits;
                            n--;
                        }
                    }
                }
            }

        }

        #region Instance Fields
        /// <summary>
        /// Pending buffer to use
        /// </summary>
        public DeflaterPending pending;

        Tree literalTree;
        Tree distTree;
        Tree blTree;

        // Buffer for distances
        short[] d_buf;
        byte[] l_buf;
        int last_lit;
        int extra_bits;
        #endregion

        static DeflaterHuffman()
        {
            staticLCodes = new short[LITERAL_NUM];
            staticLLength = new byte[LITERAL_NUM];

            int i = 0;
            while (i < 144)
            {
                staticLCodes[i] = BitReverse((0x030 + i) << 8);
                staticLLength[i++] = 8;
            }

            while (i < 256)
            {
                staticLCodes[i] = BitReverse((0x190 - 144 + i) << 7);
                staticLLength[i++] = 9;
            }

            while (i < 280)
            {
                staticLCodes[i] = BitReverse((0x000 - 256 + i) << 9);
                staticLLength[i++] = 7;
            }

            while (i < LITERAL_NUM)
            {
                staticLCodes[i] = BitReverse((0x0c0 - 280 + i) << 8);
                staticLLength[i++] = 8;
            }

            staticDCodes = new short[DIST_NUM];
            staticDLength = new byte[DIST_NUM];
            for (i = 0; i < DIST_NUM; i++)
            {
                staticDCodes[i] = BitReverse(i << 11);
                staticDLength[i] = 5;
            }
        }

        /// <summary>
        /// Construct instance with pending buffer
        /// </summary>
        /// <param name="pending">Pending buffer to use</param>
        public DeflaterHuffman(DeflaterPending pending)
        {
            this.pending = pending;

            literalTree = new Tree(this, LITERAL_NUM, 257, 15);
            distTree = new Tree(this, DIST_NUM, 1, 15);
            blTree = new Tree(this, BITLEN_NUM, 4, 7);

            d_buf = new short[BUFSIZE];
            l_buf = new byte[BUFSIZE];
        }

        /// <summary>
        /// Reset internal state
        /// </summary>		
        public void Reset()
        {
            last_lit = 0;
            extra_bits = 0;
            literalTree.Reset();
            distTree.Reset();
            blTree.Reset();
        }

        /// <summary>
        /// Write all trees to pending buffer
        /// </summary>
        /// <param name="blTreeCodes">The number/rank of treecodes to send.</param>
        public void SendAllTrees(int blTreeCodes)
        {
            blTree.BuildCodes();
            literalTree.BuildCodes();
            distTree.BuildCodes();
            pending.WriteBits(literalTree.numCodes - 257, 5);
            pending.WriteBits(distTree.numCodes - 1, 5);
            pending.WriteBits(blTreeCodes - 4, 4);
            for (int rank = 0; rank < blTreeCodes; rank++)
            {
                pending.WriteBits(blTree.length[BL_ORDER[rank]], 3);
            }
            literalTree.WriteTree(blTree);
            distTree.WriteTree(blTree);

        }

        /// <summary>
        /// Compress current buffer writing data to pending buffer
        /// </summary>
        public void CompressBlock()
        {
            for (int i = 0; i < last_lit; i++)
            {
                int litlen = l_buf[i] & 0xff;
                int dist = d_buf[i];
                if (dist-- != 0)
                {
                    int lc = Lcode(litlen);
                    literalTree.WriteSymbol(lc);

                    int bits = (lc - 261) / 4;
                    if (bits > 0 && bits <= 5)
                    {
                        pending.WriteBits(litlen & ((1 << bits) - 1), bits);
                    }

                    int dc = Dcode(dist);
                    distTree.WriteSymbol(dc);

                    bits = dc / 2 - 1;
                    if (bits > 0)
                    {
                        pending.WriteBits(dist & ((1 << bits) - 1), bits);
                    }
                }
                else
                {
                    literalTree.WriteSymbol(litlen);
                }
            }
            literalTree.WriteSymbol(EOF_SYMBOL);

        }

        /// <summary>
        /// Flush block to output with no compression
        /// </summary>
        /// <param name="stored">Data to write</param>
        /// <param name="storedOffset">Index of first byte to write</param>
        /// <param name="storedLength">Count of bytes to write</param>
        /// <param name="lastBlock">True if this is the last block</param>
        public void FlushStoredBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            pending.WriteBits((DeflaterConstants.STORED_BLOCK << 1) + (lastBlock ? 1 : 0), 3);
            pending.AlignToByte();
            pending.WriteShort(storedLength);
            pending.WriteShort(~storedLength);
            pending.WriteBlock(stored, storedOffset, storedLength);
            Reset();
        }

        /// <summary>
        /// Flush block to output with compression
        /// </summary>		
        /// <param name="stored">Data to flush</param>
        /// <param name="storedOffset">Index of first byte to flush</param>
        /// <param name="storedLength">Count of bytes to flush</param>
        /// <param name="lastBlock">True if this is the last block</param>
        public void FlushBlock(byte[] stored, int storedOffset, int storedLength, bool lastBlock)
        {
            literalTree.freqs[EOF_SYMBOL]++;

            literalTree.BuildTree();
            distTree.BuildTree();

            literalTree.CalcBLFreq(blTree);
            distTree.CalcBLFreq(blTree);

            blTree.BuildTree();

            int blTreeCodes = 4;
            for (int i = 18; i > blTreeCodes; i--)
            {
                if (blTree.length[BL_ORDER[i]] > 0)
                {
                    blTreeCodes = i + 1;
                }
            }
            int opt_len = 14 + blTreeCodes * 3 + blTree.GetEncodedLength() +
                literalTree.GetEncodedLength() + distTree.GetEncodedLength() +
                extra_bits;

            int static_len = extra_bits;
            for (int i = 0; i < LITERAL_NUM; i++)
            {
                static_len += literalTree.freqs[i] * staticLLength[i];
            }
            for (int i = 0; i < DIST_NUM; i++)
            {
                static_len += distTree.freqs[i] * staticDLength[i];
            }
            if (opt_len >= static_len)
            {
                opt_len = static_len;
            }

            if (storedOffset >= 0 && storedLength + 4 < opt_len >> 3)
            {
                FlushStoredBlock(stored, storedOffset, storedLength, lastBlock);
            }
            else if (opt_len == static_len)
            {
                pending.WriteBits((DeflaterConstants.STATIC_TREES << 1) + (lastBlock ? 1 : 0), 3);
                literalTree.SetStaticCodes(staticLCodes, staticLLength);
                distTree.SetStaticCodes(staticDCodes, staticDLength);
                CompressBlock();
                Reset();
            }
            else
            {
                pending.WriteBits((DeflaterConstants.DYN_TREES << 1) + (lastBlock ? 1 : 0), 3);
                SendAllTrees(blTreeCodes);
                CompressBlock();
                Reset();
            }
        }

        /// <summary>
        /// Get value indicating if internal buffer is full
        /// </summary>
        /// <returns>true if buffer is full</returns>
        public bool IsFull()
        {
            return last_lit >= BUFSIZE;
        }

        /// <summary>
        /// Add literal to buffer
        /// </summary>
        /// <param name="literal">Literal value to add to buffer.</param>
        /// <returns>Value indicating internal buffer is full</returns>
        public bool TallyLit(int literal)
        {
            d_buf[last_lit] = 0;
            l_buf[last_lit++] = (byte)literal;
            literalTree.freqs[literal]++;
            return IsFull();
        }

        /// <summary>
        /// Add distance code and length to literal and distance trees
        /// </summary>
        /// <param name="distance">Distance code</param>
        /// <param name="length">Length</param>
        /// <returns>Value indicating if internal buffer is full</returns>
        public bool TallyDist(int distance, int length)
        {
            d_buf[last_lit] = (short)distance;
            l_buf[last_lit++] = (byte)(length - 3);

            int lc = Lcode(length - 3);
            literalTree.freqs[lc]++;
            if (lc >= 265 && lc < 285)
            {
                extra_bits += (lc - 261) / 4;
            }

            int dc = Dcode(distance - 1);
            distTree.freqs[dc]++;
            if (dc >= 4)
            {
                extra_bits += dc / 2 - 1;
            }
            return IsFull();
        }


        /// <summary>
        /// Reverse the bits of a 16 bit value.
        /// </summary>
        /// <param name="toReverse">Value to reverse bits</param>
        /// <returns>Value with bits reversed</returns>
        public static short BitReverse(int toReverse)
        {
            return (short)(bit4Reverse[toReverse & 0xF] << 12 |
                            bit4Reverse[(toReverse >> 4) & 0xF] << 8 |
                            bit4Reverse[(toReverse >> 8) & 0xF] << 4 |
                            bit4Reverse[toReverse >> 12]);
        }

        static int Lcode(int length)
        {
            if (length == 255)
            {
                return 285;
            }

            int code = 257;
            while (length >= 8)
            {
                code += 4;
                length >>= 1;
            }
            return code + length;
        }

        static int Dcode(int distance)
        {
            int code = 0;
            while (distance >= 4)
            {
                code += 2;
                distance >>= 1;
            }
            return code + distance;
        }
    }
    #endregion

    #region DeflateStrategy
    /// <summary>
    /// Strategies for deflater
    /// </summary>
    internal enum DeflateStrategy
    {
        /// <summary>
        /// The default strategy
        /// </summary>
        Default = 0,

        /// <summary>
        /// This strategy will only allow longer string repetitions.  It is
        /// useful for random data with a small character set.
        /// </summary>
        Filtered = 1,


        /// <summary>
        /// This strategy will not look for string repetitions at all.  It
        /// only encodes with Huffman trees (which means, that more common
        /// characters get a smaller encoding.
        /// </summary>
        HuffmanOnly = 2
    }
    #endregion

    #region DeflaterEngine
    /// <summary>
    /// Low level compression engine for deflate algorithm which uses a 32K sliding window
    /// with secondary compression from Huffman/Shannon-Fano codes.
    /// </summary>
    internal class DeflaterEngine : DeflaterConstants
    {
        #region Constants
        const int TooFar = 4096;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct instance with pending buffer
        /// </summary>
        /// <param name="pending">
        /// Pending buffer to use
        /// </param>>
        public DeflaterEngine(DeflaterPending pending)
        {
            this.pending = pending;
            huffman = new DeflaterHuffman(pending);
            adler = new Adler32();

            window = new byte[2 * WSIZE];
            head = new short[HASH_SIZE];
            prev = new short[WSIZE];

            // We start at index 1, to avoid an implementation deficiency, that
            // we cannot build a repeat pattern at index 0.
            blockStart = strstart = 1;
        }

        #endregion

        /// <summary>
        /// Deflate drives actual compression of data
        /// </summary>
        /// <param name="flush">True to flush input buffers</param>
        /// <param name="finish">Finish deflation with the current input.</param>
        /// <returns>Returns true if progress has been made.</returns>
        public bool Deflate(bool flush, bool finish)
        {
            bool progress;
            do
            {
                FillWindow();
                bool canFlush = flush && (inputOff == inputEnd);

                switch (compressionFunction)
                {
                    case DEFLATE_STORED:
                        progress = DeflateStored(canFlush, finish);
                        break;
                    case DEFLATE_FAST:
                        progress = DeflateFast(canFlush, finish);
                        break;
                    case DEFLATE_SLOW:
                        progress = DeflateSlow(canFlush, finish);
                        break;
                    default:
                        throw new InvalidOperationException("unknown compressionFunction");
                }
            } while (pending.IsFlushed && progress); // repeat while we have no pending output and progress was made
            return progress;
        }

        /// <summary>
        /// Sets input data to be deflated.  Should only be called when <code>NeedsInput()</code>
        /// returns true
        /// </summary>
        /// <param name="buffer">The buffer containing input data.</param>
        /// <param name="offset">The offset of the first byte of data.</param>
        /// <param name="count">The number of bytes of data to use as input.</param>
        public void SetInput(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (inputOff < inputEnd)
            {
                throw new InvalidOperationException("Old input was not completely processed");
            }

            int end = offset + count;

            /* We want to throw an ArrayIndexOutOfBoundsException early.  The
            * check is very tricky: it also handles integer wrap around.
            */
            if ((offset > end) || (end > buffer.Length))
            {
                throw new ArgumentOutOfRangeException("count");
            }

            inputBuf = buffer;
            inputOff = offset;
            inputEnd = end;
        }

        /// <summary>
        /// Determines if more <see cref="SetInput">input</see> is needed.
        /// </summary>		
        /// <returns>Return true if input is needed via <see cref="SetInput">SetInput</see></returns>
        public bool NeedsInput()
        {
            return (inputEnd == inputOff);
        }

        /// <summary>
        /// Set compression dictionary
        /// </summary>
        /// <param name="buffer">The buffer containing the dictionary data</param>
        /// <param name="offset">The offset in the buffer for the first byte of data</param>
        /// <param name="length">The length of the dictionary data.</param>
        public void SetDictionary(byte[] buffer, int offset, int length)
        {
            adler.Update(buffer, offset, length);
            if (length < MIN_MATCH)
            {
                return;
            }

            if (length > MAX_DIST)
            {
                offset += length - MAX_DIST;
                length = MAX_DIST;
            }

            System.Array.Copy(buffer, offset, window, strstart, length);

            UpdateHash();
            --length;
            while (--length > 0)
            {
                InsertString();
                strstart++;
            }
            strstart += 2;
            blockStart = strstart;
        }

        /// <summary>
        /// Reset internal state
        /// </summary>		
        public void Reset()
        {
            huffman.Reset();
            adler.Reset();
            blockStart = strstart = 1;
            lookahead = 0;
            totalIn = 0;
            prevAvailable = false;
            matchLen = MIN_MATCH - 1;

            for (int i = 0; i < HASH_SIZE; i++)
            {
                head[i] = 0;
            }

            for (int i = 0; i < WSIZE; i++)
            {
                prev[i] = 0;
            }
        }

        /// <summary>
        /// Reset Adler checksum
        /// </summary>		
        public void ResetAdler()
        {
            adler.Reset();
        }

        /// <summary>
        /// Get current value of Adler checksum
        /// </summary>		
        public int Adler
        {
            get
            {
                return unchecked((int)adler.Value);
            }
        }

        /// <summary>
        /// Total data processed
        /// </summary>		
        public long TotalIn
        {
            get
            {
                return totalIn;
            }
        }

        /// <summary>
        /// Get/set the <see cref="DeflateStrategy">deflate strategy</see>
        /// </summary>		
        public DeflateStrategy Strategy
        {
            get
            {
                return strategy;
            }
            set
            {
                strategy = value;
            }
        }

        /// <summary>
        /// Set the deflate level (0-9)
        /// </summary>
        /// <param name="level">The value to set the level to.</param>
        public void SetLevel(int level)
        {
            if ((level < 0) || (level > 9))
            {
                throw new ArgumentOutOfRangeException("level");
            }

            goodLength = DeflaterConstants.GOOD_LENGTH[level];
            max_lazy = DeflaterConstants.MAX_LAZY[level];
            niceLength = DeflaterConstants.NICE_LENGTH[level];
            max_chain = DeflaterConstants.MAX_CHAIN[level];

            if (DeflaterConstants.COMPR_FUNC[level] != compressionFunction)
            {
                switch (compressionFunction)
                {
                    case DEFLATE_STORED:
                        if (strstart > blockStart)
                        {
                            huffman.FlushStoredBlock(window, blockStart,
                                strstart - blockStart, false);
                            blockStart = strstart;
                        }
                        UpdateHash();
                        break;

                    case DEFLATE_FAST:
                        if (strstart > blockStart)
                        {
                            huffman.FlushBlock(window, blockStart, strstart - blockStart,
                                false);
                            blockStart = strstart;
                        }
                        break;

                    case DEFLATE_SLOW:
                        if (prevAvailable)
                        {
                            huffman.TallyLit(window[strstart - 1] & 0xff);
                        }
                        if (strstart > blockStart)
                        {
                            huffman.FlushBlock(window, blockStart, strstart - blockStart, false);
                            blockStart = strstart;
                        }
                        prevAvailable = false;
                        matchLen = MIN_MATCH - 1;
                        break;
                }
                compressionFunction = COMPR_FUNC[level];
            }
        }

        /// <summary>
        /// Fill the window
        /// </summary>
        public void FillWindow()
        {
            /* If the window is almost full and there is insufficient lookahead,
             * move the upper half to the lower one to make room in the upper half.
             */
            if (strstart >= WSIZE + MAX_DIST)
            {
                SlideWindow();
            }

            /* If there is not enough lookahead, but still some input left,
             * read in the input
             */
            while (lookahead < DeflaterConstants.MIN_LOOKAHEAD && inputOff < inputEnd)
            {
                int more = 2 * WSIZE - lookahead - strstart;

                if (more > inputEnd - inputOff)
                {
                    more = inputEnd - inputOff;
                }

                System.Array.Copy(inputBuf, inputOff, window, strstart + lookahead, more);
                adler.Update(inputBuf, inputOff, more);

                inputOff += more;
                totalIn += more;
                lookahead += more;
            }

            if (lookahead >= MIN_MATCH)
            {
                UpdateHash();
            }
        }

        void UpdateHash()
        {
            /*
                        if (DEBUGGING) {
                            Console.WriteLine("updateHash: "+strstart);
                        }
            */
            ins_h = (window[strstart] << HASH_SHIFT) ^ window[strstart + 1];
        }

        /// <summary>
        /// Inserts the current string in the head hash and returns the previous
        /// value for this hash.
        /// </summary>
        /// <returns>The previous hash value</returns>
        int InsertString()
        {
            short match;
            int hash = ((ins_h << HASH_SHIFT) ^ window[strstart + (MIN_MATCH - 1)]) & HASH_MASK;

            prev[strstart & WMASK] = match = head[hash];
            head[hash] = unchecked((short)strstart);
            ins_h = hash;
            return match & 0xffff;
        }

        void SlideWindow()
        {
            Array.Copy(window, WSIZE, window, 0, WSIZE);
            matchStart -= WSIZE;
            strstart -= WSIZE;
            blockStart -= WSIZE;

            // Slide the hash table (could be avoided with 32 bit values
            // at the expense of memory usage).
            for (int i = 0; i < HASH_SIZE; ++i)
            {
                int m = head[i] & 0xffff;
                head[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
            }

            // Slide the prev table.
            for (int i = 0; i < WSIZE; i++)
            {
                int m = prev[i] & 0xffff;
                prev[i] = (short)(m >= WSIZE ? (m - WSIZE) : 0);
            }
        }

        /// <summary>
        /// Find the best (longest) string in the window matching the 
        /// string starting at strstart.
        ///
        /// Preconditions:
        /// <code>
        /// strstart + MAX_MATCH &lt;= window.length.</code>
        /// </summary>
        /// <param name="curMatch"></param>
        /// <returns>True if a match greater than the minimum length is found</returns>
        bool FindLongestMatch(int curMatch)
        {
            int chainLength = this.max_chain;
            int niceLength = this.niceLength;
            short[] prev = this.prev;
            int scan = this.strstart;
            int match;
            int best_end = this.strstart + matchLen;
            int best_len = Math.Max(matchLen, MIN_MATCH - 1);

            int limit = Math.Max(strstart - MAX_DIST, 0);

            int strend = strstart + MAX_MATCH - 1;
            byte scan_end1 = window[best_end - 1];
            byte scan_end = window[best_end];

            // Do not waste too much time if we already have a good match:
            if (best_len >= this.goodLength)
            {
                chainLength >>= 2;
            }

            /* Do not look for matches beyond the end of the input. This is necessary
            * to make deflate deterministic.
            */
            if (niceLength > lookahead)
            {
                niceLength = lookahead;
            }

            do
            {

                if (window[curMatch + best_len] != scan_end ||
                    window[curMatch + best_len - 1] != scan_end1 ||
                    window[curMatch] != window[scan] ||
                    window[curMatch + 1] != window[scan + 1])
                {
                    continue;
                }

                match = curMatch + 2;
                scan += 2;

                /* We check for insufficient lookahead only every 8th comparison;
                * the 256th check will be made at strstart + 258.
                */
                while (
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    window[++scan] == window[++match] &&
                    (scan < strend))
                {
                    // Do nothing
                }

                if (scan > best_end)
                {
                    matchStart = curMatch;
                    best_end = scan;
                    best_len = scan - strstart;

                    if (best_len >= niceLength)
                    {
                        break;
                    }

                    scan_end1 = window[best_end - 1];
                    scan_end = window[best_end];
                }
                scan = strstart;
            } while ((curMatch = (prev[curMatch & WMASK] & 0xffff)) > limit && --chainLength != 0);

            matchLen = Math.Min(best_len, lookahead);
            return matchLen >= MIN_MATCH;
        }

        bool DeflateStored(bool flush, bool finish)
        {
            if (!flush && (lookahead == 0))
            {
                return false;
            }

            strstart += lookahead;
            lookahead = 0;

            int storedLength = strstart - blockStart;

            if ((storedLength >= DeflaterConstants.MAX_BLOCK_SIZE) || // Block is full
                (blockStart < WSIZE && storedLength >= MAX_DIST) ||   // Block may move out of window
                flush)
            {
                bool lastBlock = finish;
                if (storedLength > DeflaterConstants.MAX_BLOCK_SIZE)
                {
                    storedLength = DeflaterConstants.MAX_BLOCK_SIZE;
                    lastBlock = false;
                }

                huffman.FlushStoredBlock(window, blockStart, storedLength, lastBlock);
                blockStart += storedLength;
                return !lastBlock;
            }
            return true;
        }

        bool DeflateFast(bool flush, bool finish)
        {
            if (lookahead < MIN_LOOKAHEAD && !flush)
            {
                return false;
            }

            while (lookahead >= MIN_LOOKAHEAD || flush)
            {
                if (lookahead == 0)
                {
                    // We are flushing everything
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
                    blockStart = strstart;
                    return false;
                }

                if (strstart > 2 * WSIZE - MIN_LOOKAHEAD)
                {
                    /* slide window, as FindLongestMatch needs this.
                     * This should only happen when flushing and the window
                     * is almost full.
                     */
                    SlideWindow();
                }

                int hashHead;
                if (lookahead >= MIN_MATCH &&
                    (hashHead = InsertString()) != 0 &&
                    strategy != DeflateStrategy.HuffmanOnly &&
                    strstart - hashHead <= MAX_DIST &&
                    FindLongestMatch(hashHead))
                {
                    // longestMatch sets matchStart and matchLen

                    bool full = huffman.TallyDist(strstart - matchStart, matchLen);

                    lookahead -= matchLen;
                    if (matchLen <= max_lazy && lookahead >= MIN_MATCH)
                    {
                        while (--matchLen > 0)
                        {
                            ++strstart;
                            InsertString();
                        }
                        ++strstart;
                    }
                    else
                    {
                        strstart += matchLen;
                        if (lookahead >= MIN_MATCH - 1)
                        {
                            UpdateHash();
                        }
                    }
                    matchLen = MIN_MATCH - 1;
                    if (!full)
                    {
                        continue;
                    }
                }
                else
                {
                    // No match found
                    huffman.TallyLit(window[strstart] & 0xff);
                    ++strstart;
                    --lookahead;
                }

                if (huffman.IsFull())
                {
                    bool lastBlock = finish && (lookahead == 0);
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, lastBlock);
                    blockStart = strstart;
                    return !lastBlock;
                }
            }
            return true;
        }

        bool DeflateSlow(bool flush, bool finish)
        {
            if (lookahead < MIN_LOOKAHEAD && !flush)
            {
                return false;
            }

            while (lookahead >= MIN_LOOKAHEAD || flush)
            {
                if (lookahead == 0)
                {
                    if (prevAvailable)
                    {
                        huffman.TallyLit(window[strstart - 1] & 0xff);
                    }
                    prevAvailable = false;

                    // We are flushing everything
                    huffman.FlushBlock(window, blockStart, strstart - blockStart, finish);
                    blockStart = strstart;
                    return false;
                }

                if (strstart >= 2 * WSIZE - MIN_LOOKAHEAD)
                {
                    /* slide window, as FindLongestMatch needs this.
                     * This should only happen when flushing and the window
                     * is almost full.
                     */
                    SlideWindow();
                }

                int prevMatch = matchStart;
                int prevLen = matchLen;
                if (lookahead >= MIN_MATCH)
                {

                    int hashHead = InsertString();

                    if (strategy != DeflateStrategy.HuffmanOnly &&
                        hashHead != 0 &&
                        strstart - hashHead <= MAX_DIST &&
                        FindLongestMatch(hashHead))
                    {

                        // longestMatch sets matchStart and matchLen

                        // Discard match if too small and too far away
                        if (matchLen <= 5 && (strategy == DeflateStrategy.Filtered || (matchLen == MIN_MATCH && strstart - matchStart > TooFar)))
                        {
                            matchLen = MIN_MATCH - 1;
                        }
                    }
                }

                // previous match was better
                if ((prevLen >= MIN_MATCH) && (matchLen <= prevLen))
                {
                    huffman.TallyDist(strstart - 1 - prevMatch, prevLen);
                    prevLen -= 2;
                    do
                    {
                        strstart++;
                        lookahead--;
                        if (lookahead >= MIN_MATCH)
                        {
                            InsertString();
                        }
                    } while (--prevLen > 0);

                    strstart++;
                    lookahead--;
                    prevAvailable = false;
                    matchLen = MIN_MATCH - 1;
                }
                else
                {
                    if (prevAvailable)
                    {
                        huffman.TallyLit(window[strstart - 1] & 0xff);
                    }
                    prevAvailable = true;
                    strstart++;
                    lookahead--;
                }

                if (huffman.IsFull())
                {
                    int len = strstart - blockStart;
                    if (prevAvailable)
                    {
                        len--;
                    }
                    bool lastBlock = (finish && (lookahead == 0) && !prevAvailable);
                    huffman.FlushBlock(window, blockStart, len, lastBlock);
                    blockStart += len;
                    return !lastBlock;
                }
            }
            return true;
        }

        #region Instance Fields

        // Hash index of string to be inserted
        int ins_h;

        /// <summary>
        /// Hashtable, hashing three characters to an index for window, so
        /// that window[index]..window[index+2] have this hash code.  
        /// Note that the array should really be unsigned short, so you need
        /// to and the values with 0xffff.
        /// </summary>
        short[] head;

        /// <summary>
        /// <code>prev[index &amp; WMASK]</code> points to the previous index that has the
        /// same hash code as the string starting at index.  This way 
        /// entries with the same hash code are in a linked list.
        /// Note that the array should really be unsigned short, so you need
        /// to and the values with 0xffff.
        /// </summary>
        short[] prev;

        int matchStart;
        // Length of best match
        int matchLen;
        // Set if previous match exists
        bool prevAvailable;
        int blockStart;

        /// <summary>
        /// Points to the current character in the window.
        /// </summary>
        int strstart;

        /// <summary>
        /// lookahead is the number of characters starting at strstart in
        /// window that are valid.
        /// So window[strstart] until window[strstart+lookahead-1] are valid
        /// characters.
        /// </summary>
        int lookahead;

        /// <summary>
        /// This array contains the part of the uncompressed stream that 
        /// is of relevance.  The current character is indexed by strstart.
        /// </summary>
        byte[] window;

        DeflateStrategy strategy;
        int max_chain, max_lazy, niceLength, goodLength;

        /// <summary>
        /// The current compression function.
        /// </summary>
        int compressionFunction;

        /// <summary>
        /// The input data for compression.
        /// </summary>
        byte[] inputBuf;

        /// <summary>
        /// The total bytes of input read.
        /// </summary>
        long totalIn;

        /// <summary>
        /// The offset into inputBuf, where input data starts.
        /// </summary>
        int inputOff;

        /// <summary>
        /// The end offset of the input data.
        /// </summary>
        int inputEnd;

        DeflaterPending pending;
        DeflaterHuffman huffman;

        /// <summary>
        /// The adler checksum
        /// </summary>
        Adler32 adler;
        #endregion
    }
    #endregion

    #region DeflaterConstants
    /// <summary>
    /// This class contains constants used for deflation.
    /// </summary>
    internal class DeflaterConstants
    {
        /// <summary>
        /// Set to true to enable debugging
        /// </summary>
        public const bool DEBUGGING = false;

        /// <summary>
        /// Written to Zip file to identify a stored block
        /// </summary>
        public const int STORED_BLOCK = 0;

        /// <summary>
        /// Identifies static tree in Zip file
        /// </summary>
        public const int STATIC_TREES = 1;

        /// <summary>
        /// Identifies dynamic tree in Zip file
        /// </summary>
        public const int DYN_TREES = 2;

        /// <summary>
        /// Header flag indicating a preset dictionary for deflation
        /// </summary>
        public const int PRESET_DICT = 0x20;

        /// <summary>
        /// Sets internal buffer sizes for Huffman encoding
        /// </summary>
        public const int DEFAULT_MEM_LEVEL = 8;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int MAX_MATCH = 258;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int MIN_MATCH = 3;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int MAX_WBITS = 15;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int WSIZE = 1 << MAX_WBITS;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int WMASK = WSIZE - 1;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int HASH_BITS = DEFAULT_MEM_LEVEL + 7;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int HASH_SIZE = 1 << HASH_BITS;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int HASH_MASK = HASH_SIZE - 1;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int HASH_SHIFT = (HASH_BITS + MIN_MATCH - 1) / MIN_MATCH;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int MIN_LOOKAHEAD = MAX_MATCH + MIN_MATCH + 1;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int MAX_DIST = WSIZE - MIN_LOOKAHEAD;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int PENDING_BUF_SIZE = 1 << (DEFAULT_MEM_LEVEL + 8);

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int MAX_BLOCK_SIZE = Math.Min(65535, PENDING_BUF_SIZE - 5);

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int DEFLATE_STORED = 0;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int DEFLATE_FAST = 1;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public const int DEFLATE_SLOW = 2;

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int[] GOOD_LENGTH = { 0, 4, 4, 4, 4, 8, 8, 8, 32, 32 };

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int[] MAX_LAZY = { 0, 4, 5, 6, 4, 16, 16, 32, 128, 258 };

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int[] NICE_LENGTH = { 0, 8, 16, 32, 16, 32, 128, 128, 258, 258 };

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int[] MAX_CHAIN = { 0, 4, 8, 32, 16, 32, 128, 256, 1024, 4096 };

        /// <summary>
        /// Internal compression engine constant
        /// </summary>		
        public static int[] COMPR_FUNC = { 0, 1, 1, 1, 1, 2, 2, 2, 2, 2 };

    }
    #endregion

    #region Deflater
    /// <summary>
    /// This is the Deflater class.  The deflater class compresses input
    /// with the deflate algorithm described in RFC 1951.  It has several
    /// compression levels and three different strategies described below.
    ///
    /// This class is <i>not</i> thread safe.  This is inherent in the API, due
    /// to the split of deflate and setInput.
    /// 
    /// author of the original java version : Jochen Hoenicke
    /// </summary>
    internal class Deflater
    {
        #region Deflater Documentation
        /*
		* The Deflater can do the following state transitions:
		*
		* (1) -> INIT_STATE   ----> INIT_FINISHING_STATE ---.
		*        /  | (2)      (5)                          |
		*       /   v          (5)                          |
		*   (3)| SETDICT_STATE ---> SETDICT_FINISHING_STATE |(3)
		*       \   | (3)                 |        ,--------'
		*        |  |                     | (3)   /
		*        v  v          (5)        v      v
		* (1) -> BUSY_STATE   ----> FINISHING_STATE
		*                                | (6)
		*                                v
		*                           FINISHED_STATE
		*    \_____________________________________/
		*                    | (7)
		*                    v
		*               CLOSED_STATE
		*
		* (1) If we should produce a header we start in INIT_STATE, otherwise
		*     we start in BUSY_STATE.
		* (2) A dictionary may be set only when we are in INIT_STATE, then
		*     we change the state as indicated.
		* (3) Whether a dictionary is set or not, on the first call of deflate
		*     we change to BUSY_STATE.
		* (4) -- intentionally left blank -- :)
		* (5) FINISHING_STATE is entered, when flush() is called to indicate that
		*     there is no more INPUT.  There are also states indicating, that
		*     the header wasn't written yet.
		* (6) FINISHED_STATE is entered, when everything has been flushed to the
		*     internal pending output buffer.
		* (7) At any time (7)
		*
		*/
        #endregion
        #region Public Constants
        /// <summary>
        /// The best and slowest compression level.  This tries to find very
        /// long and distant string repetitions.
        /// </summary>
        public const int BEST_COMPRESSION = 9;

        /// <summary>
        /// The worst but fastest compression level.
        /// </summary>
        public const int BEST_SPEED = 1;

        /// <summary>
        /// The default compression level.
        /// </summary>
        public const int DEFAULT_COMPRESSION = -1;

        /// <summary>
        /// This level won't compress at all but output uncompressed blocks.
        /// </summary>
        public const int NO_COMPRESSION = 0;

        /// <summary>
        /// The compression method.  This is the only method supported so far.
        /// There is no need to use this constant at all.
        /// </summary>
        public const int DEFLATED = 8;
        #endregion
        #region Local Constants
        private const int IS_SETDICT = 0x01;
        private const int IS_FLUSHING = 0x04;
        private const int IS_FINISHING = 0x08;

        private const int INIT_STATE = 0x00;
        private const int SETDICT_STATE = 0x01;
        //		private static  int INIT_FINISHING_STATE    = 0x08;
        //		private static  int SETDICT_FINISHING_STATE = 0x09;
        private const int BUSY_STATE = 0x10;
        private const int FLUSHING_STATE = 0x14;
        private const int FINISHING_STATE = 0x1c;
        private const int FINISHED_STATE = 0x1e;
        private const int CLOSED_STATE = 0x7f;
        #endregion
        #region Constructors
        /// <summary>
        /// Creates a new deflater with default compression level.
        /// </summary>
        public Deflater()
            : this(DEFAULT_COMPRESSION, false)
        {

        }

        /// <summary>
        /// Creates a new deflater with given compression level.
        /// </summary>
        /// <param name="level">
        /// the compression level, a value between NO_COMPRESSION
        /// and BEST_COMPRESSION, or DEFAULT_COMPRESSION.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">if lvl is out of range.</exception>
        public Deflater(int level)
            : this(level, false)
        {

        }

        /// <summary>
        /// Creates a new deflater with given compression level.
        /// </summary>
        /// <param name="level">
        /// the compression level, a value between NO_COMPRESSION
        /// and BEST_COMPRESSION.
        /// </param>
        /// <param name="noZlibHeaderOrFooter">
        /// true, if we should suppress the Zlib/RFC1950 header at the
        /// beginning and the adler checksum at the end of the output.  This is
        /// useful for the GZIP/PKZIP formats.
        /// </param>
        /// <exception cref="System.ArgumentOutOfRangeException">if lvl is out of range.</exception>
        public Deflater(int level, bool noZlibHeaderOrFooter)
        {
            if (level == DEFAULT_COMPRESSION)
            {
                level = 6;
            }
            else if (level < NO_COMPRESSION || level > BEST_COMPRESSION)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            pending = new DeflaterPending();
            engine = new DeflaterEngine(pending);
            this.noZlibHeaderOrFooter = noZlibHeaderOrFooter;
            SetStrategy(DeflateStrategy.Default);
            SetLevel(level);
            Reset();
        }
        #endregion

        /// <summary>
        /// Resets the deflater.  The deflater acts afterwards as if it was
        /// just created with the same compression level and strategy as it
        /// had before.
        /// </summary>
        public void Reset()
        {
            state = (noZlibHeaderOrFooter ? BUSY_STATE : INIT_STATE);
            totalOut = 0;
            pending.Reset();
            engine.Reset();
        }

        /// <summary>
        /// Gets the current adler checksum of the data that was processed so far.
        /// </summary>
        public int Adler
        {
            get
            {
                return engine.Adler;
            }
        }

        /// <summary>
        /// Gets the number of input bytes processed so far.
        /// </summary>
        public long TotalIn
        {
            get
            {
                return engine.TotalIn;
            }
        }

        /// <summary>
        /// Gets the number of output bytes so far.
        /// </summary>
        public long TotalOut
        {
            get
            {
                return totalOut;
            }
        }

        /// <summary>
        /// Flushes the current input block.  Further calls to deflate() will
        /// produce enough output to inflate everything in the current input
        /// block.  This is not part of Sun's JDK so I have made it package
        /// private.  It is used by DeflaterOutputStream to implement
        /// flush().
        /// </summary>
        public void Flush()
        {
            state |= IS_FLUSHING;
        }

        /// <summary>
        /// Finishes the deflater with the current input block.  It is an error
        /// to give more input after this method was called.  This method must
        /// be called to force all bytes to be flushed.
        /// </summary>
        public void Finish()
        {
            state |= (IS_FLUSHING | IS_FINISHING);
        }

        /// <summary>
        /// Returns true if the stream was finished and no more output bytes
        /// are available.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return (state == FINISHED_STATE) && pending.IsFlushed;
            }
        }

        /// <summary>
        /// Returns true, if the input buffer is empty.
        /// You should then call setInput(). 
        /// NOTE: This method can also return true when the stream
        /// was finished.
        /// </summary>
        public bool IsNeedingInput
        {
            get
            {
                return engine.NeedsInput();
            }
        }

        /// <summary>
        /// Sets the data which should be compressed next.  This should be only
        /// called when needsInput indicates that more input is needed.
        /// If you call setInput when needsInput() returns false, the
        /// previous input that is still pending will be thrown away.
        /// The given byte array should not be changed, before needsInput() returns
        /// true again.
        /// This call is equivalent to <code>setInput(input, 0, input.length)</code>.
        /// </summary>
        /// <param name="input">
        /// the buffer containing the input data.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// if the buffer was finished() or ended().
        /// </exception>
        public void SetInput(byte[] input)
        {
            SetInput(input, 0, input.Length);
        }

        /// <summary>
        /// Sets the data which should be compressed next.  This should be
        /// only called when needsInput indicates that more input is needed.
        /// The given byte array should not be changed, before needsInput() returns
        /// true again.
        /// </summary>
        /// <param name="input">
        /// the buffer containing the input data.
        /// </param>
        /// <param name="offset">
        /// the start of the data.
        /// </param>
        /// <param name="count">
        /// the number of data bytes of input.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// if the buffer was Finish()ed or if previous input is still pending.
        /// </exception>
        public void SetInput(byte[] input, int offset, int count)
        {
            if ((state & IS_FINISHING) != 0)
            {
                throw new InvalidOperationException("Finish() already called");
            }
            engine.SetInput(input, offset, count);
        }

        /// <summary>
        /// Sets the compression level.  There is no guarantee of the exact
        /// position of the change, but if you call this when needsInput is
        /// true the change of compression level will occur somewhere near
        /// before the end of the so far given input.
        /// </summary>
        /// <param name="level">
        /// the new compression level.
        /// </param>
        public void SetLevel(int level)
        {
            if (level == DEFAULT_COMPRESSION)
            {
                level = 6;
            }
            else if (level < NO_COMPRESSION || level > BEST_COMPRESSION)
            {
                throw new ArgumentOutOfRangeException("level");
            }

            if (this.level != level)
            {
                this.level = level;
                engine.SetLevel(level);
            }
        }

        /// <summary>
        /// Get current compression level
        /// </summary>
        /// <returns>Returns the current compression level</returns>
        public int GetLevel()
        {
            return level;
        }

        /// <summary>
        /// Sets the compression strategy. Strategy is one of
        /// DEFAULT_STRATEGY, HUFFMAN_ONLY and FILTERED.  For the exact
        /// position where the strategy is changed, the same as for
        /// SetLevel() applies.
        /// </summary>
        /// <param name="strategy">
        /// The new compression strategy.
        /// </param>
        public void SetStrategy(DeflateStrategy strategy)
        {
            engine.Strategy = strategy;
        }

        /// <summary>
        /// Deflates the current input block with to the given array.
        /// </summary>
        /// <param name="output">
        /// The buffer where compressed data is stored
        /// </param>
        /// <returns>
        /// The number of compressed bytes added to the output, or 0 if either
        /// IsNeedingInput() or IsFinished returns true or length is zero.
        /// </returns>
        public int Deflate(byte[] output)
        {
            return Deflate(output, 0, output.Length);
        }

        /// <summary>
        /// Deflates the current input block to the given array.
        /// </summary>
        /// <param name="output">
        /// Buffer to store the compressed data.
        /// </param>
        /// <param name="offset">
        /// Offset into the output array.
        /// </param>
        /// <param name="length">
        /// The maximum number of bytes that may be stored.
        /// </param>
        /// <returns>
        /// The number of compressed bytes added to the output, or 0 if either
        /// needsInput() or finished() returns true or length is zero.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// If Finish() was previously called.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// If offset or length don't match the array length.
        /// </exception>
        public int Deflate(byte[] output, int offset, int length)
        {
            int origLength = length;

            if (state == CLOSED_STATE)
            {
                throw new InvalidOperationException("Deflater closed");
            }

            if (state < BUSY_STATE)
            {
                // output header
                int header = (DEFLATED +
                    ((DeflaterConstants.MAX_WBITS - 8) << 4)) << 8;
                int level_flags = (level - 1) >> 1;
                if (level_flags < 0 || level_flags > 3)
                {
                    level_flags = 3;
                }
                header |= level_flags << 6;
                if ((state & IS_SETDICT) != 0)
                {
                    // Dictionary was set
                    header |= DeflaterConstants.PRESET_DICT;
                }
                header += 31 - (header % 31);

                pending.WriteShortMSB(header);
                if ((state & IS_SETDICT) != 0)
                {
                    int chksum = engine.Adler;
                    engine.ResetAdler();
                    pending.WriteShortMSB(chksum >> 16);
                    pending.WriteShortMSB(chksum & 0xffff);
                }

                state = BUSY_STATE | (state & (IS_FLUSHING | IS_FINISHING));
            }

            for (; ; )
            {
                int count = pending.Flush(output, offset, length);
                offset += count;
                totalOut += count;
                length -= count;

                if (length == 0 || state == FINISHED_STATE)
                {
                    break;
                }

                if (!engine.Deflate((state & IS_FLUSHING) != 0, (state & IS_FINISHING) != 0))
                {
                    if (state == BUSY_STATE)
                    {
                        // We need more input now
                        return origLength - length;
                    }
                    else if (state == FLUSHING_STATE)
                    {
                        if (level != NO_COMPRESSION)
                        {
                            /* We have to supply some lookahead.  8 bit lookahead
                             * is needed by the zlib inflater, and we must fill
                             * the next byte, so that all bits are flushed.
                             */
                            int neededbits = 8 + ((-pending.BitCount) & 7);
                            while (neededbits > 0)
                            {
                                /* write a static tree block consisting solely of
                                 * an EOF:
                                 */
                                pending.WriteBits(2, 10);
                                neededbits -= 10;
                            }
                        }
                        state = BUSY_STATE;
                    }
                    else if (state == FINISHING_STATE)
                    {
                        pending.AlignToByte();

                        // Compressed data is complete.  Write footer information if required.
                        if (!noZlibHeaderOrFooter)
                        {
                            int adler = engine.Adler;
                            pending.WriteShortMSB(adler >> 16);
                            pending.WriteShortMSB(adler & 0xffff);
                        }
                        state = FINISHED_STATE;
                    }
                }
            }
            return origLength - length;
        }

        /// <summary>
        /// Sets the dictionary which should be used in the deflate process.
        /// This call is equivalent to <code>setDictionary(dict, 0, dict.Length)</code>.
        /// </summary>
        /// <param name="dictionary">
        /// the dictionary.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// if SetInput () or Deflate () were already called or another dictionary was already set.
        /// </exception>
        public void SetDictionary(byte[] dictionary)
        {
            SetDictionary(dictionary, 0, dictionary.Length);
        }

        /// <summary>
        /// Sets the dictionary which should be used in the deflate process.
        /// The dictionary is a byte array containing strings that are
        /// likely to occur in the data which should be compressed.  The
        /// dictionary is not stored in the compressed output, only a
        /// checksum.  To decompress the output you need to supply the same
        /// dictionary again.
        /// </summary>
        /// <param name="dictionary">
        /// The dictionary data
        /// </param>
        /// <param name="index">
        /// The index where dictionary information commences.
        /// </param>
        /// <param name="count">
        /// The number of bytes in the dictionary.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// If SetInput () or Deflate() were already called or another dictionary was already set.
        /// </exception>
        public void SetDictionary(byte[] dictionary, int index, int count)
        {
            if (state != INIT_STATE)
            {
                throw new InvalidOperationException();
            }

            state = SETDICT_STATE;
            engine.SetDictionary(dictionary, index, count);
        }

        #region Instance Fields
        /// <summary>
        /// Compression level.
        /// </summary>
        int level;

        /// <summary>
        /// If true no Zlib/RFC1950 headers or footers are generated
        /// </summary>
        bool noZlibHeaderOrFooter;

        /// <summary>
        /// The current state.
        /// </summary>
        int state;

        /// <summary>
        /// The total bytes of output written.
        /// </summary>
        long totalOut;

        /// <summary>
        /// The pending output.
        /// </summary>
        DeflaterPending pending;

        /// <summary>
        /// The deflater engine.
        /// </summary>
        DeflaterEngine engine;
        #endregion
    }
    #endregion

    #region OutputWindow
    /// <summary>
    /// Contains the output from the Inflation process.
    /// We need to have a window so that we can refer backwards into the output stream
    /// to repeat stuff.<br/>
    /// Author of the original java version : John Leuner
    /// </summary>
    internal class OutputWindow
    {
        #region Constants
        const int WindowSize = 1 << 15;
        const int WindowMask = WindowSize - 1;
        #endregion

        #region Instance Fields
        byte[] window = new byte[WindowSize]; //The window is 2^15 bytes
        int windowEnd;
        int windowFilled;
        #endregion

        /// <summary>
        /// Write a byte to this output window
        /// </summary>
        /// <param name="value">value to write</param>
        /// <exception cref="InvalidOperationException">
        /// if window is full
        /// </exception>
        public void Write(int value)
        {
            if (windowFilled++ == WindowSize)
            {
                throw new InvalidOperationException("Window full");
            }
            window[windowEnd++] = (byte)value;
            windowEnd &= WindowMask;
        }


        private void SlowRepeat(int repStart, int length, int distance)
        {
            while (length-- > 0)
            {
                window[windowEnd++] = window[repStart++];
                windowEnd &= WindowMask;
                repStart &= WindowMask;
            }
        }

        /// <summary>
        /// Append a byte pattern already in the window itself
        /// </summary>
        /// <param name="length">length of pattern to copy</param>
        /// <param name="distance">distance from end of window pattern occurs</param>
        /// <exception cref="InvalidOperationException">
        /// If the repeated data overflows the window
        /// </exception>
        public void Repeat(int length, int distance)
        {
            if ((windowFilled += length) > WindowSize)
            {
                throw new InvalidOperationException("Window full");
            }

            int repStart = (windowEnd - distance) & WindowMask;
            int border = WindowSize - length;
            if ((repStart <= border) && (windowEnd < border))
            {
                if (length <= distance)
                {
                    System.Array.Copy(window, repStart, window, windowEnd, length);
                    windowEnd += length;
                }
                else
                {
                    // We have to copy manually, since the repeat pattern overlaps.
                    while (length-- > 0)
                    {
                        window[windowEnd++] = window[repStart++];
                    }
                }
            }
            else
            {
                SlowRepeat(repStart, length, distance);
            }
        }

        /// <summary>
        /// Copy from input manipulator to internal window
        /// </summary>
        /// <param name="input">source of data</param>
        /// <param name="length">length of data to copy</param>
        /// <returns>the number of bytes copied</returns>
        public int CopyStored(StreamManipulator input, int length)
        {
            length = Math.Min(Math.Min(length, WindowSize - windowFilled), input.AvailableBytes);
            int copied;

            int tailLen = WindowSize - windowEnd;
            if (length > tailLen)
            {
                copied = input.CopyBytes(window, windowEnd, tailLen);
                if (copied == tailLen)
                {
                    copied += input.CopyBytes(window, 0, length - tailLen);
                }
            }
            else
            {
                copied = input.CopyBytes(window, windowEnd, length);
            }

            windowEnd = (windowEnd + copied) & WindowMask;
            windowFilled += copied;
            return copied;
        }

        /// <summary>
        /// Copy dictionary to window
        /// </summary>
        /// <param name="dictionary">source dictionary</param>
        /// <param name="offset">offset of start in source dictionary</param>
        /// <param name="length">length of dictionary</param>
        /// <exception cref="InvalidOperationException">
        /// If window isnt empty
        /// </exception>
        public void CopyDict(byte[] dictionary, int offset, int length)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException("dictionary");
            }

            if (windowFilled > 0)
            {
                throw new InvalidOperationException();
            }

            if (length > WindowSize)
            {
                offset += length - WindowSize;
                length = WindowSize;
            }
            System.Array.Copy(dictionary, offset, window, 0, length);
            windowEnd = length & WindowMask;
        }

        /// <summary>
        /// Get remaining unfilled space in window
        /// </summary>
        /// <returns>Number of bytes left in window</returns>
        public int GetFreeSpace()
        {
            return WindowSize - windowFilled;
        }

        /// <summary>
        /// Get bytes available for output in window
        /// </summary>
        /// <returns>Number of bytes filled</returns>
        public int GetAvailable()
        {
            return windowFilled;
        }

        /// <summary>
        /// Copy contents of window to output
        /// </summary>
        /// <param name="output">buffer to copy to</param>
        /// <param name="offset">offset to start at</param>
        /// <param name="len">number of bytes to count</param>
        /// <returns>The number of bytes copied</returns>
        /// <exception cref="InvalidOperationException">
        /// If a window underflow occurs
        /// </exception>
        public int CopyOutput(byte[] output, int offset, int len)
        {
            int copyEnd = windowEnd;
            if (len > windowFilled)
            {
                len = windowFilled;
            }
            else
            {
                copyEnd = (windowEnd - windowFilled + len) & WindowMask;
            }

            int copied = len;
            int tailLen = len - copyEnd;

            if (tailLen > 0)
            {
                System.Array.Copy(window, WindowSize - tailLen, output, offset, tailLen);
                offset += tailLen;
                len = copyEnd;
            }
            System.Array.Copy(window, copyEnd - len, output, offset, len);
            windowFilled -= copied;
            if (windowFilled < 0)
            {
                throw new InvalidOperationException();
            }
            return copied;
        }

        /// <summary>
        /// Reset by clearing window so <see cref="GetAvailable">GetAvailable</see> returns 0
        /// </summary>
        public void Reset()
        {
            windowFilled = windowEnd = 0;
        }
    }
    #endregion

    #region Inflater
    /// <summary>
    /// Inflater is used to decompress data that has been compressed according
    /// to the "deflate" standard described in rfc1951.
    /// 
    /// By default Zlib (rfc1950) headers and footers are expected in the input.
    /// You can use constructor <code> public Inflater(bool noHeader)</code> passing true
    /// if there is no Zlib header information
    ///
    /// The usage is as following.  First you have to set some input with
    /// <code>SetInput()</code>, then Inflate() it.  If inflate doesn't
    /// inflate any bytes there may be three reasons:
    /// <ul>
    /// <li>IsNeedingInput() returns true because the input buffer is empty.
    /// You have to provide more input with <code>SetInput()</code>.
    /// NOTE: IsNeedingInput() also returns true when, the stream is finished.
    /// </li>
    /// <li>IsNeedingDictionary() returns true, you have to provide a preset
    ///    dictionary with <code>SetDictionary()</code>.</li>
    /// <li>IsFinished returns true, the inflater has finished.</li>
    /// </ul>
    /// Once the first output byte is produced, a dictionary will not be
    /// needed at a later stage.
    ///
    /// author of the original java version : John Leuner, Jochen Hoenicke
    /// </summary>
    internal class Inflater
    {
        #region Constants/Readonly
        /// <summary>
        /// Copy lengths for literal codes 257..285
        /// </summary>
        static readonly int[] CPLENS = {
								  3, 4, 5, 6, 7, 8, 9, 10, 11, 13, 15, 17, 19, 23, 27, 31,
								  35, 43, 51, 59, 67, 83, 99, 115, 131, 163, 195, 227, 258
							  };

        /// <summary>
        /// Extra bits for literal codes 257..285
        /// </summary>
        static readonly int[] CPLEXT = {
								  0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 2, 2,
								  3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 5, 0
							  };

        /// <summary>
        /// Copy offsets for distance codes 0..29
        /// </summary>
        static readonly int[] CPDIST = {
								1, 2, 3, 4, 5, 7, 9, 13, 17, 25, 33, 49, 65, 97, 129, 193,
								257, 385, 513, 769, 1025, 1537, 2049, 3073, 4097, 6145,
								8193, 12289, 16385, 24577
							  };

        /// <summary>
        /// Extra bits for distance codes
        /// </summary>
        static readonly int[] CPDEXT = {
								0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6,
								7, 7, 8, 8, 9, 9, 10, 10, 11, 11,
								12, 12, 13, 13
							  };

        /// <summary>
        /// These are the possible states for an inflater
        /// </summary>
        const int DECODE_HEADER = 0;
        const int DECODE_DICT = 1;
        const int DECODE_BLOCKS = 2;
        const int DECODE_STORED_LEN1 = 3;
        const int DECODE_STORED_LEN2 = 4;
        const int DECODE_STORED = 5;
        const int DECODE_DYN_HEADER = 6;
        const int DECODE_HUFFMAN = 7;
        const int DECODE_HUFFMAN_LENBITS = 8;
        const int DECODE_HUFFMAN_DIST = 9;
        const int DECODE_HUFFMAN_DISTBITS = 10;
        const int DECODE_CHKSUM = 11;
        const int FINISHED = 12;
        #endregion

        #region Instance Fields
        /// <summary>
        /// This variable contains the current state.
        /// </summary>
        int mode;

        /// <summary>
        /// The adler checksum of the dictionary or of the decompressed
        /// stream, as it is written in the header resp. footer of the
        /// compressed stream. 
        /// Only valid if mode is DECODE_DICT or DECODE_CHKSUM.
        /// </summary>
        int readAdler;

        /// <summary>
        /// The number of bits needed to complete the current state.  This
        /// is valid, if mode is DECODE_DICT, DECODE_CHKSUM,
        /// DECODE_HUFFMAN_LENBITS or DECODE_HUFFMAN_DISTBITS.
        /// </summary>
        int neededBits;
        int repLength;
        int repDist;
        int uncomprLen;

        /// <summary>
        /// True, if the last block flag was set in the last block of the
        /// inflated stream.  This means that the stream ends after the
        /// current block.
        /// </summary>
        bool isLastBlock;

        /// <summary>
        /// The total number of inflated bytes.
        /// </summary>
        long totalOut;

        /// <summary>
        /// The total number of bytes set with setInput().  This is not the
        /// value returned by the TotalIn property, since this also includes the
        /// unprocessed input.
        /// </summary>
        long totalIn;

        /// <summary>
        /// This variable stores the noHeader flag that was given to the constructor.
        /// True means, that the inflated stream doesn't contain a Zlib header or 
        /// footer.
        /// </summary>
        bool noHeader;

        StreamManipulator input;
        OutputWindow outputWindow;
        InflaterDynHeader dynHeader;
        InflaterHuffmanTree litlenTree, distTree;
        //Adler32 adler;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new inflater or RFC1951 decompressor
        /// RFC1950/Zlib headers and footers will be expected in the input data
        /// </summary>
        public Inflater()
            : this(false)
        {
        }

        /// <summary>
        /// Creates a new inflater.
        /// </summary>
        /// <param name="noHeader">
        /// True if no RFC1950/Zlib header and footer fields are expected in the input data
        /// 
        /// This is used for GZIPed/Zipped input.
        /// 
        /// For compatibility with
        /// Sun JDK you should provide one byte of input more than needed in
        /// this case.
        /// </param>
        public Inflater(bool noHeader)
        {
            this.noHeader = noHeader;
            //this.adler = new Adler32();
            input = new StreamManipulator();
            outputWindow = new OutputWindow();
            mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
        }
        #endregion

        /// <summary>
        /// Resets the inflater so that a new stream can be decompressed.  All
        /// pending input and output will be discarded.
        /// </summary>
        public void Reset()
        {
            mode = noHeader ? DECODE_BLOCKS : DECODE_HEADER;
            totalIn = 0;
            totalOut = 0;
            input.Reset();
            outputWindow.Reset();
            dynHeader = null;
            litlenTree = null;
            distTree = null;
            isLastBlock = false;
            //adler.Reset();
        }

        /// <summary>
        /// Decodes a zlib/RFC1950 header.
        /// </summary>
        /// <returns>
        /// False if more input is needed.
        /// </returns>
        /// <exception cref="SharpZipBaseException">
        /// The header is invalid.
        /// </exception>
        private bool DecodeHeader()
        {
            int header = input.PeekBits(16);
            if (header < 0)
            {
                return false;
            }
            input.DropBits(16);

            // The header is written in "wrong" byte order
            header = ((header << 8) | (header >> 8)) & 0xffff;
            if (header % 31 != 0)
            {
                throw new Exception("Header checksum illegal");
            }

            if ((header & 0x0f00) != (Deflater.DEFLATED << 8))
            {
                throw new Exception("Compression Method unknown");
            }

            /* Maximum size of the backwards window in bits.
            * We currently ignore this, but we could use it to make the
            * inflater window more space efficient. On the other hand the
            * full window (15 bits) is needed most times, anyway.
            int max_wbits = ((header & 0x7000) >> 12) + 8;
            */

            if ((header & 0x0020) == 0)
            { // Dictionary flag?
                mode = DECODE_BLOCKS;
            }
            else
            {
                mode = DECODE_DICT;
                neededBits = 32;
            }
            return true;
        }

        /// <summary>
        /// Decodes the dictionary checksum after the deflate header.
        /// </summary>
        /// <returns>
        /// False if more input is needed.
        /// </returns>
        private bool DecodeDict()
        {
            while (neededBits > 0)
            {
                int dictByte = input.PeekBits(8);
                if (dictByte < 0)
                {
                    return false;
                }
                input.DropBits(8);
                readAdler = (readAdler << 8) | dictByte;
                neededBits -= 8;
            }
            return false;
        }

        /// <summary>
        /// Decodes the huffman encoded symbols in the input stream.
        /// </summary>
        /// <returns>
        /// false if more input is needed, true if output window is
        /// full or the current block ends.
        /// </returns>
        /// <exception cref="SharpZipBaseException">
        /// if deflated stream is invalid.
        /// </exception>
        private bool DecodeHuffman()
        {
            int free = outputWindow.GetFreeSpace();
            while (free >= 258)
            {
                int symbol;
                switch (mode)
                {
                    case DECODE_HUFFMAN:
                        // This is the inner loop so it is optimized a bit
                        while (((symbol = litlenTree.GetSymbol(input)) & ~0xff) == 0)
                        {
                            outputWindow.Write(symbol);
                            if (--free < 258)
                            {
                                return true;
                            }
                        }

                        if (symbol < 257)
                        {
                            if (symbol < 0)
                            {
                                return false;
                            }
                            else
                            {
                                // symbol == 256: end of block
                                distTree = null;
                                litlenTree = null;
                                mode = DECODE_BLOCKS;
                                return true;
                            }
                        }

                        try
                        {
                            repLength = CPLENS[symbol - 257];
                            neededBits = CPLEXT[symbol - 257];
                        }
                        catch (Exception)
                        {
                            throw new Exception("Illegal rep length code");
                        }
                        goto case DECODE_HUFFMAN_LENBITS; // fall through

                    case DECODE_HUFFMAN_LENBITS:
                        if (neededBits > 0)
                        {
                            mode = DECODE_HUFFMAN_LENBITS;
                            int i = input.PeekBits(neededBits);
                            if (i < 0)
                            {
                                return false;
                            }
                            input.DropBits(neededBits);
                            repLength += i;
                        }
                        mode = DECODE_HUFFMAN_DIST;
                        goto case DECODE_HUFFMAN_DIST; // fall through

                    case DECODE_HUFFMAN_DIST:
                        symbol = distTree.GetSymbol(input);
                        if (symbol < 0)
                        {
                            return false;
                        }

                        try
                        {
                            repDist = CPDIST[symbol];
                            neededBits = CPDEXT[symbol];
                        }
                        catch (Exception)
                        {
                            throw new Exception("Illegal rep dist code");
                        }

                        goto case DECODE_HUFFMAN_DISTBITS; // fall through

                    case DECODE_HUFFMAN_DISTBITS:
                        if (neededBits > 0)
                        {
                            mode = DECODE_HUFFMAN_DISTBITS;
                            int i = input.PeekBits(neededBits);
                            if (i < 0)
                            {
                                return false;
                            }
                            input.DropBits(neededBits);
                            repDist += i;
                        }

                        outputWindow.Repeat(repLength, repDist);
                        free -= repLength;
                        mode = DECODE_HUFFMAN;
                        break;

                    default:
                        throw new Exception("Inflater unknown mode");
                }
            }
            return true;
        }

        /// <summary>
        /// Decodes the adler checksum after the deflate stream.
        /// </summary>
        /// <returns>
        /// false if more input is needed.
        /// </returns>
        /// <exception cref="SharpZipBaseException">
        /// If checksum doesn't match.
        /// </exception>
        private bool DecodeChksum()
        {
            while (neededBits > 0)
            {
                int chkByte = input.PeekBits(8);
                if (chkByte < 0)
                {
                    return false;
                }
                input.DropBits(8);
                readAdler = (readAdler << 8) | chkByte;
                neededBits -= 8;
            }

            //if ((int)adler.Value != readAdler)
            //{
            //    throw new Exception("Adler chksum doesn't match: " + (int)adler.Value + " vs. " + readAdler);
            //}

            mode = FINISHED;
            return false;
        }

        /// <summary>
        /// Decodes the deflated stream.
        /// </summary>
        /// <returns>
        /// false if more input is needed, or if finished.
        /// </returns>
        /// <exception cref="SharpZipBaseException">
        /// if deflated stream is invalid.
        /// </exception>
        private bool Decode()
        {
            switch (mode)
            {
                case DECODE_HEADER:
                    return DecodeHeader();

                case DECODE_DICT:
                    return DecodeDict();

                case DECODE_CHKSUM:
                    return DecodeChksum();

                case DECODE_BLOCKS:
                    if (isLastBlock)
                    {
                        if (noHeader)
                        {
                            mode = FINISHED;
                            return false;
                        }
                        else
                        {
                            input.SkipToByteBoundary();
                            neededBits = 32;
                            mode = DECODE_CHKSUM;
                            return true;
                        }
                    }

                    int type = input.PeekBits(3);
                    if (type < 0)
                    {
                        return false;
                    }
                    input.DropBits(3);

                    if ((type & 1) != 0)
                    {
                        isLastBlock = true;
                    }
                    switch (type >> 1)
                    {
                        case DeflaterConstants.STORED_BLOCK:
                            input.SkipToByteBoundary();
                            mode = DECODE_STORED_LEN1;
                            break;
                        case DeflaterConstants.STATIC_TREES:
                            litlenTree = InflaterHuffmanTree.defLitLenTree;
                            distTree = InflaterHuffmanTree.defDistTree;
                            mode = DECODE_HUFFMAN;
                            break;
                        case DeflaterConstants.DYN_TREES:
                            dynHeader = new InflaterDynHeader();
                            mode = DECODE_DYN_HEADER;
                            break;
                        default:
                            throw new Exception("Unknown block type " + type);
                    }
                    return true;

                case DECODE_STORED_LEN1:
                    {
                        if ((uncomprLen = input.PeekBits(16)) < 0)
                        {
                            return false;
                        }
                        input.DropBits(16);
                        mode = DECODE_STORED_LEN2;
                    }
                    goto case DECODE_STORED_LEN2; // fall through

                case DECODE_STORED_LEN2:
                    {
                        int nlen = input.PeekBits(16);
                        if (nlen < 0)
                        {
                            return false;
                        }
                        input.DropBits(16);
                        if (nlen != (uncomprLen ^ 0xffff))
                        {
                            throw new Exception("broken uncompressed block");
                        }
                        mode = DECODE_STORED;
                    }
                    goto case DECODE_STORED; // fall through

                case DECODE_STORED:
                    {
                        int more = outputWindow.CopyStored(input, uncomprLen);
                        uncomprLen -= more;
                        if (uncomprLen == 0)
                        {
                            mode = DECODE_BLOCKS;
                            return true;
                        }
                        return !input.IsNeedingInput;
                    }

                case DECODE_DYN_HEADER:
                    if (!dynHeader.Decode(input))
                    {
                        return false;
                    }

                    litlenTree = dynHeader.BuildLitLenTree();
                    distTree = dynHeader.BuildDistTree();
                    mode = DECODE_HUFFMAN;
                    goto case DECODE_HUFFMAN; // fall through

                case DECODE_HUFFMAN:
                case DECODE_HUFFMAN_LENBITS:
                case DECODE_HUFFMAN_DIST:
                case DECODE_HUFFMAN_DISTBITS:
                    return DecodeHuffman();

                case FINISHED:
                    return false;

                default:
                    throw new Exception("Inflater.Decode unknown mode");
            }
        }

        /// <summary>
        /// Sets the preset dictionary.  This should only be called, if
        /// needsDictionary() returns true and it should set the same
        /// dictionary, that was used for deflating.  The getAdler()
        /// function returns the checksum of the dictionary needed.
        /// </summary>
        /// <param name="buffer">
        /// The dictionary.
        /// </param>
        public void SetDictionary(byte[] buffer)
        {
            SetDictionary(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sets the preset dictionary.  This should only be called, if
        /// needsDictionary() returns true and it should set the same
        /// dictionary, that was used for deflating.  The getAdler()
        /// function returns the checksum of the dictionary needed.
        /// </summary>
        /// <param name="buffer">
        /// The dictionary.
        /// </param>
        /// <param name="index">
        /// The index into buffer where the dictionary starts.
        /// </param>
        /// <param name="count">
        /// The number of bytes in the dictionary.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// No dictionary is needed.
        /// </exception>
        /// <exception cref="SharpZipBaseException">
        /// The adler checksum for the buffer is invalid
        /// </exception>
        public void SetDictionary(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            if (!IsNeedingDictionary)
            {
                throw new InvalidOperationException("Dictionary is not needed");
            }

            //adler.Update(buffer, index, count);

            //if ((int)adler.Value != readAdler)
            //{
            //    throw new Exception("Wrong adler checksum");
            //}
            //adler.Reset();
            outputWindow.CopyDict(buffer, index, count);
            mode = DECODE_BLOCKS;
        }

        /// <summary>
        /// Sets the input.  This should only be called, if needsInput()
        /// returns true.
        /// </summary>
        /// <param name="buffer">
        /// the input.
        /// </param>
        public void SetInput(byte[] buffer)
        {
            SetInput(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Sets the input.  This should only be called, if needsInput()
        /// returns true.
        /// </summary>
        /// <param name="buffer">
        /// The source of input data
        /// </param>
        /// <param name="index">
        /// The index into buffer where the input starts.
        /// </param>
        /// <param name="count">
        /// The number of bytes of input to use.
        /// </param>
        /// <exception cref="System.InvalidOperationException">
        /// No input is needed.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// The index and/or count are wrong.
        /// </exception>
        public void SetInput(byte[] buffer, int index, int count)
        {
            input.SetInput(buffer, index, count);
            totalIn += (long)count;
        }

        /// <summary>
        /// Inflates the compressed stream to the output buffer.  If this
        /// returns 0, you should check, whether IsNeedingDictionary(),
        /// IsNeedingInput() or IsFinished() returns true, to determine why no
        /// further output is produced.
        /// </summary>
        /// <param name="buffer">
        /// the output buffer.
        /// </param>
        /// <returns>
        /// The number of bytes written to the buffer, 0 if no further
        /// output can be produced.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// if buffer has length 0.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// if deflated stream is invalid.
        /// </exception>
        public int Inflate(byte[] buffer)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }

            return Inflate(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Inflates the compressed stream to the output buffer.  If this
        /// returns 0, you should check, whether needsDictionary(),
        /// needsInput() or finished() returns true, to determine why no
        /// further output is produced.
        /// </summary>
        /// <param name="buffer">
        /// the output buffer.
        /// </param>
        /// <param name="offset">
        /// the offset in buffer where storing starts.
        /// </param>
        /// <param name="count">
        /// the maximum number of bytes to output.
        /// </param>
        /// <returns>
        /// the number of bytes written to the buffer, 0 if no further output can be produced.
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// if count is less than 0.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// if the index and / or count are wrong.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// if deflated stream is invalid.
        /// </exception>
        public int Inflate(byte[] buffer, int offset, int count)
        {
            //if (buffer == null)
            //{
            //    throw new ArgumentNullException("buffer");
            //}

            //if (count < 0)
            //{
            //    throw new ArgumentOutOfRangeException("count", "count cannot be negative");
            //}

            //if (offset < 0)
            //{
            //    throw new ArgumentOutOfRangeException("offset", "offset cannot be negative");
            //}

            //if (offset + count > buffer.Length)
            //{
            //    throw new ArgumentException("count exceeds buffer bounds");
            //}

            // Special case: count may be zero
            if (count == 0)
            {
                if (!IsFinished)
                { // -jr- 08-Nov-2003 INFLATE_BUG fix..
                    Decode();
                }
                return 0;
            }

            int bytesCopied = 0;

            do
            {
                if (mode != DECODE_CHKSUM)
                {
                    /* Don't give away any output, if we are waiting for the
                    * checksum in the input stream.
                    *
                    * With this trick we have always:
                    *   IsNeedingInput() and not IsFinished()
                    *   implies more output can be produced.
                    */
                    int more = outputWindow.CopyOutput(buffer, offset, count);
                    if (more > 0)
                    {
                        //adler.Update(buffer, offset, more);
                        offset += more;
                        bytesCopied += more;
                        totalOut += (long)more;
                        count -= more;
                        if (count == 0)
                        {
                            return bytesCopied;
                        }
                    }
                }
            } while (Decode() || ((outputWindow.GetAvailable() > 0) && (mode != DECODE_CHKSUM)));
            return bytesCopied;
        }

        /// <summary>
        /// Returns true, if the input buffer is empty.
        /// You should then call setInput(). 
        /// NOTE: This method also returns true when the stream is finished.
        /// </summary>
        public bool IsNeedingInput
        {
            get
            {
                return input.IsNeedingInput;
            }
        }

        /// <summary>
        /// Returns true, if a preset dictionary is needed to inflate the input.
        /// </summary>
        public bool IsNeedingDictionary
        {
            get
            {
                return mode == DECODE_DICT && neededBits == 0;
            }
        }

        /// <summary>
        /// Returns true, if the inflater has finished.  This means, that no
        /// input is needed and no output can be produced.
        /// </summary>
        public bool IsFinished
        {
            get
            {
                return mode == FINISHED && outputWindow.GetAvailable() == 0;
            }
        }

        ///// <summary>
        ///// Gets the adler checksum.  This is either the checksum of all
        ///// uncompressed bytes returned by inflate(), or if needsDictionary()
        ///// returns true (and thus no output was yet produced) this is the
        ///// adler checksum of the expected dictionary.
        ///// </summary>
        ///// <returns>
        ///// the adler checksum.
        ///// </returns>
        //public int Adler
        //{
        //    get
        //    {
        //        return IsNeedingDictionary ? readAdler : (int)adler.Value;
        //    }
        //}

        /// <summary>
        /// Gets the total number of output bytes returned by Inflate().
        /// </summary>
        /// <returns>
        /// the total number of output bytes.
        /// </returns>
        public long TotalOut
        {
            get
            {
                return totalOut;
            }
        }

        /// <summary>
        /// Gets the total number of processed compressed input bytes.
        /// </summary>
        /// <returns>
        /// The total number of bytes of processed input bytes.
        /// </returns>
        public long TotalIn
        {
            get
            {
                return totalIn - (long)RemainingInput;
            }
        }

        /// <summary>
        /// Gets the number of unprocessed input bytes.  Useful, if the end of the
        /// stream is reached and you want to further process the bytes after
        /// the deflate stream.
        /// </summary>
        /// <returns>
        /// The number of bytes of the input which have not been processed.
        /// </returns>
        public int RemainingInput
        {
            // TODO: This should be a long?
            get
            {
                return input.AvailableBytes;
            }
        }
    }
    #endregion

    #region InflaterInputBuffer
    /// <summary>
    /// An input buffer customised for use by <see cref="DeflaterInputStream"/>
    /// </summary>
    /// <remarks>
    /// The buffer supports decryption of incoming data.
    /// </remarks>
    internal class InflaterInputBuffer
    {
        /// <summary>
        /// Initialise a new instance of <see cref="InflaterInputBuffer"/> with a default buffer size
        /// </summary>
        /// <param name="stream">The stream to buffer.</param>
        public InflaterInputBuffer(Stream stream)
            : this(stream, 4096)
        {
        }

        /// <summary>
        /// Initialise a new instance of <see cref="InflaterInputBuffer"/>
        /// </summary>
        /// <param name="stream">The stream to buffer.</param>
        /// <param name="bufferSize">The size to use for the buffer</param>
        /// <remarks>A minimum buffer size of 1KB is permitted.  Lower sizes are treated as 1KB.</remarks>
        public InflaterInputBuffer(Stream stream, int bufferSize)
        {
            inputStream = stream;
            if (bufferSize < 1024)
            {
                bufferSize = 1024;
            }
            rawData = new byte[bufferSize];
            clearText = rawData;
        }

        /// <summary>
        /// Get the length of bytes bytes in the <see cref="RawData"/>
        /// </summary>
        public int RawLength
        {
            get
            {
                return rawLength;
            }
        }

        /// <summary>
        /// Get the contents of the raw data buffer.
        /// </summary>
        /// <remarks>This may contain encrypted data.</remarks>
        public byte[] RawData
        {
            get
            {
                return rawData;
            }
        }

        /// <summary>
        /// Get the number of useable bytes in <see cref="ClearText"/>
        /// </summary>
        public int ClearTextLength
        {
            get
            {
                return clearTextLength;
            }
        }

        /// <summary>
        /// Get the contents of the clear text buffer.
        /// </summary>
        public byte[] ClearText
        {
            get
            {
                return clearText;
            }
        }

        /// <summary>
        /// Get/set the number of bytes available
        /// </summary>
        public int Available
        {
            get { return available; }
            set { available = value; }
        }

        /// <summary>
        /// Call <see cref="Inflater.SetInput(byte[], int, int)"/> passing the current clear text buffer contents.
        /// </summary>
        /// <param name="inflater">The inflater to set input for.</param>
        public void SetInflaterInput(Inflater inflater)
        {
            if (available > 0)
            {
                inflater.SetInput(clearText, clearTextLength - available, available);
                available = 0;
            }
        }

        /// <summary>
        /// Fill the buffer from the underlying input stream.
        /// </summary>
        public void Fill()
        {
            rawLength = 0;
            int toRead = rawData.Length;

            while (toRead > 0)
            {
                int count = inputStream.Read(rawData, rawLength, toRead);
                if (count <= 0)
                {
                    break;
                }
                rawLength += count;
                toRead -= count;
            }
            clearTextLength = rawLength;

            available = clearTextLength;
        }

        /// <summary>
        /// Read a buffer directly from the input stream
        /// </summary>
        /// <param name="buffer">The buffer to fill</param>
        /// <returns>Returns the number of bytes read.</returns>
        public int ReadRawBuffer(byte[] buffer)
        {
            return ReadRawBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Read a buffer directly from the input stream
        /// </summary>
        /// <param name="outBuffer">The buffer to read into</param>
        /// <param name="offset">The offset to start reading data into.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>Returns the number of bytes read.</returns>
        public int ReadRawBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            int currentOffset = offset;
            int currentLength = length;

            while (currentLength > 0)
            {
                if (available <= 0)
                {
                    Fill();
                    if (available <= 0)
                    {
                        return 0;
                    }
                }
                int toCopy = Math.Min(currentLength, available);
                System.Array.Copy(rawData, rawLength - (int)available, outBuffer, currentOffset, toCopy);
                currentOffset += toCopy;
                currentLength -= toCopy;
                available -= toCopy;
            }
            return length;
        }

        /// <summary>
        /// Read clear text data from the input stream.
        /// </summary>
        /// <param name="outBuffer">The buffer to add data to.</param>
        /// <param name="offset">The offset to start adding data at.</param>
        /// <param name="length">The number of bytes to read.</param>
        /// <returns>Returns the number of bytes actually read.</returns>
        public int ReadClearTextBuffer(byte[] outBuffer, int offset, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException("length");
            }

            int currentOffset = offset;
            int currentLength = length;

            while (currentLength > 0)
            {
                if (available <= 0)
                {
                    Fill();
                    if (available <= 0)
                    {
                        return 0;
                    }
                }

                int toCopy = Math.Min(currentLength, available);
                Array.Copy(clearText, clearTextLength - (int)available, outBuffer, currentOffset, toCopy);
                currentOffset += toCopy;
                currentLength -= toCopy;
                available -= toCopy;
            }
            return length;
        }

        /// <summary>
        /// Read a <see cref="byte"/> from the input stream.
        /// </summary>
        /// <returns>Returns the byte read.</returns>
        public int ReadLeByte()
        {
            if (available <= 0)
            {
                Fill();
                if (available <= 0)
                {
                    throw new Exception("EOF in header");
                }
            }
            byte result = rawData[rawLength - available];
            available -= 1;
            return result;
        }

        /// <summary>
        /// Read an <see cref="short"/> in little endian byte order.
        /// </summary>
        /// <returns>The short value read case to an int.</returns>
        public int ReadLeShort()
        {
            return ReadLeByte() | (ReadLeByte() << 8);
        }

        /// <summary>
        /// Read an <see cref="int"/> in little endian byte order.
        /// </summary>
        /// <returns>The int value read.</returns>
        public int ReadLeInt()
        {
            return ReadLeShort() | (ReadLeShort() << 16);
        }

        /// <summary>
        /// Read a <see cref="long"/> in little endian byte order.
        /// </summary>
        /// <returns>The long value read.</returns>
        public long ReadLeLong()
        {
            return (uint)ReadLeInt() | ((long)ReadLeInt() << 32);
        }

        #region Instance Fields
        int rawLength;
        byte[] rawData;
        int clearTextLength;
        byte[] clearText;
        int available;
        Stream inputStream;
        #endregion
    }
    #endregion
    
}
