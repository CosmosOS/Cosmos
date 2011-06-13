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

using System;
using System.IO;

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// A stream used by the library for TIFF reading and writing.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    class TiffStream
    {
        /// <summary>
        /// Reads a sequence of bytes from the stream and advances the position within the stream
        /// by the number of bytes read.
        /// </summary>
        /// <param name="clientData">A client data (by default, an underlying stream).</param>
        /// <param name="buffer">An array of bytes. When this method returns, the
        /// <paramref name="buffer"/> contains the specified byte array with the values between
        /// <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1)
        /// replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>The total number of bytes read into the <paramref name="buffer"/>. This can
        /// be less than the number of bytes requested if that many bytes are not currently
        /// available, or zero (0) if the end of the stream has been reached.</returns>
        public virtual int Read(object clientData, byte[] buffer, int offset, int count)
        {
            Stream stream = clientData as Stream;
            if (stream == null)
                throw new ArgumentException("Can't get underlying stream to read from");

            return stream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position
        /// within this stream by the number of bytes written.
        /// </summary>
        /// <param name="clientData">A client data (by default, an underlying stream).</param>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count"/>
        /// bytes from <paramref name="buffer"/> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public virtual void Write(object clientData, byte[] buffer, int offset, int count)
        {
            Stream stream = clientData as Stream;
            if (stream == null)
                throw new ArgumentException("Can't get underlying stream to write to");
            
            stream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="clientData">A client data (by default, an underlying stream).</param>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter.</param>
        /// <param name="origin">A value of type <see cref="System.IO.SeekOrigin"/> indicating the
        /// reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public virtual long Seek(object clientData, long offset, SeekOrigin origin)
        {
            // we use this as a special code, so avoid accepting it
            if (offset == -1)
                return -1; // was 0xFFFFFFFF

            Stream stream = clientData as Stream;
            if (stream == null)
                throw new ArgumentException("Can't get underlying stream to seek in");

            return stream.Seek(offset, origin);
        }

        /// <summary>
        /// Closes the current stream.
        /// </summary>
        /// <param name="clientData">A client data (by default, an underlying stream).</param>
        public virtual void Close(object clientData)
        {
            Stream stream = clientData as Stream;
            if (stream == null)
                throw new ArgumentException("Can't get underlying stream to close");

            stream.Close();
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <param name="clientData">A client data (by default, an underlying stream).</param>
        /// <returns>The length of the stream in bytes.</returns>
        public virtual long Size(object clientData)
        {
            Stream stream = clientData as Stream;
            if (stream == null)
                throw new ArgumentException("Can't get underlying stream to retrieve size from");

            return stream.Length;
        }
    }
}
