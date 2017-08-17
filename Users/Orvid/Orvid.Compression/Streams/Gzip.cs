// GzipInputStream.cs
//
// Copyright (C) 2001 Mike Krueger
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

    #region GZipOutputStream
    /// <summary>
    /// This filter stream is used to compress a stream into a "GZIP" stream.
    /// The "GZIP" format is described in RFC 1952.
    ///
    /// author of the original java version : John Leuner
    /// </summary>
    /// <example> This sample shows how to gzip a file
    /// <code>
    /// using System;
    /// using System.IO;
    /// 
    /// using ICSharpCode.SharpZipLib.GZip;
    /// using ICSharpCode.SharpZipLib.Core;
    /// 
    /// class MainClass
    /// {
    /// 	public static void Main(string[] args)
    /// 	{
    /// 			using (Stream s = new GZipOutputStream(File.Create(args[0] + ".gz")))
    /// 			using (FileStream fs = File.OpenRead(args[0])) {
    /// 				byte[] writeData = new byte[4096];
    /// 				Streamutils.Copy(s, fs, writeData);
    /// 			}
    /// 		}
    /// 	}
    /// }	
    /// </code>
    /// </example>
    public class GZipOutputStream : DeflaterOutputStream
    {
        enum OutputState
        {
            Header,
            Footer,
            Finished,
            Closed,
        };

        /// <summary>
        /// CRC-32 value for uncompressed data
        /// </summary>
        protected Crc32 crc = new Crc32();
        OutputState state_ = OutputState.Header;

        /// <summary>
        /// Creates a GzipOutputStream with the default buffer size
        /// </summary>
        /// <param name="baseOutputStream">
        /// The stream to read data (to be compressed) from
        /// </param>
        public GZipOutputStream(Stream baseOutputStream)
            : this(baseOutputStream, 4096)
        {
        }

        /// <summary>
        /// Creates a GZipOutputStream with the specified buffer size
        /// </summary>
        /// <param name="baseOutputStream">
        /// The stream to read data (to be compressed) from
        /// </param>
        /// <param name="size">
        /// Size of the buffer to use
        /// </param>
        public GZipOutputStream(Stream baseOutputStream, int size)
            : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true), size)
        {
        }

        /// <summary>
        /// Sets the active compression level (1-9).  The new level will be activated
        /// immediately.
        /// </summary>
        /// <param name="level">The compression level to set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Level specified is not supported.
        /// </exception>
        /// <see cref="Deflater"/>
        public void SetLevel(int level)
        {
            if (level < Deflater.BEST_SPEED)
            {
                throw new ArgumentOutOfRangeException("level");
            }
            deflater_.SetLevel(level);
        }

        /// <summary>
        /// Get the current compression level.
        /// </summary>
        /// <returns>The current compression level.</returns>
        public int GetLevel()
        {
            return deflater_.GetLevel();
        }

        /// <summary>
        /// Write given buffer to output updating crc
        /// </summary>
        /// <param name="buffer">Buffer to write</param>
        /// <param name="offset">Offset of first byte in buf to write</param>
        /// <param name="count">Number of bytes to write</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (state_ == OutputState.Header)
            {
                WriteHeader();
            }

            if (state_ != OutputState.Footer)
            {
                throw new InvalidOperationException("Write not permitted in current state");
            }

            crc.Update(buffer, offset, count);
            base.Write(buffer, offset, count);
        }

        /// <summary>
        /// Writes remaining compressed output data to the output stream
        /// and closes it.
        /// </summary>
        public override void Close()
        {
            try
            {
                Finish();
            }
            finally
            {
                if (state_ != OutputState.Closed)
                {
                    state_ = OutputState.Closed;
                    if (IsStreamOwner)
                    {
                        baseOutputStream_.Close();
                    }
                }
            }
        }

        /// <summary>
        /// Finish compression and write any footer information required to stream
        /// </summary>
        public override void Finish()
        {
            if (state_ == OutputState.Header)
            {
                WriteHeader();
            }

            if (state_ == OutputState.Footer)
            {
                state_ = OutputState.Finished;
                base.Finish();

                uint totalin = (uint)(deflater_.TotalIn & 0xffffffff);
                uint crcval = (uint)(crc.Value & 0xffffffff);

                byte[] gzipFooter;

                unchecked
                {
                    gzipFooter = new byte[] {
					(byte) crcval, (byte) (crcval >> 8),
					(byte) (crcval >> 16), (byte) (crcval >> 24),
					
					(byte) totalin, (byte) (totalin >> 8),
					(byte) (totalin >> 16), (byte) (totalin >> 24)
				};
                }

                baseOutputStream_.Write(gzipFooter, 0, gzipFooter.Length);
            }
        }

        void WriteHeader()
        {
            if (state_ == OutputState.Header)
            {
                state_ = OutputState.Footer;
                int mod_time = (int)((DateTime.Now.Ticks - new DateTime(1970, 1, 1).Ticks) / 10000000L);
                byte[] gzipHeader = {
					(byte) (0x1F8B >> 8), (byte) (0x1F8B & 0xff),
					(byte) Deflater.DEFLATED,
					0,
					(byte) mod_time, (byte) (mod_time >> 8),
					(byte) (mod_time >> 16), (byte) (mod_time >> 24),
					0,
					(byte) 255
				};
                baseOutputStream_.Write(gzipHeader, 0, gzipHeader.Length);
            }
        }
    }
    #endregion

    #region GZipInputStream
    /// <summary>
    /// This filter stream is used to decompress a "GZIP" format stream.
    /// The "GZIP" format is described baseInputStream RFC 1952.
    /// 
    /// author of the original java version : John Leuner
    /// </summary>
    /// <example> This sample shows how to unzip a gzipped file
    /// <code>
    /// using System;
    /// using System.IO;
    /// 
    /// using ICSharpCode.SharpZipLib.Core;
    /// using ICSharpCode.SharpZipLib.GZip;
    /// 
    /// class MainClass
    /// {
    /// 	public static void Main(string[] args)
    /// 	{
    ///			using (Stream inStream = new GZipInputStream(File.OpenRead(args[0])))
    ///			using (FileStream outStream = File.Create(Path.GetFileNameWithoutExtension(args[0]))) {
    ///				byte[] buffer = new byte[4096];
    ///				StreamUtils.Copy(inStream, outStream, buffer);
    /// 		}
    /// 	}
    /// }	
    /// </code>
    /// </example>
    public class GZipInputStream : DeflaterInputStream
    {
        #region Instance Fields
        /// <summary>
        /// CRC-32 value for uncompressed data
        /// </summary>
        protected Crc32 crc;

        /// <summary>
        /// Flag to indicate if we've read the GZIP header yet for the current member (block of compressed data).
        /// This is tracked per-block as the file is parsed.
        /// </summary>
        bool readGZIPHeader;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a GZipInputStream with the default buffer size
        /// </summary>
        /// <param name="baseInputStream">
        /// The stream to read compressed data from (baseInputStream GZIP format)
        /// </param>
        public GZipInputStream(Stream baseInputStream)
            : this(baseInputStream, 4096)
        {
        }

        /// <summary>
        /// Creates a GZIPInputStream with the specified buffer size
        /// </summary>
        /// <param name="baseInputStream">
        /// The stream to read compressed data from (baseInputStream GZIP format)
        /// </param>
        /// <param name="size">
        /// Size of the buffer to use
        /// </param>
        public GZipInputStream(Stream baseInputStream, int size)
            : base(baseInputStream, new Inflater(true), size)
        {
        }
        #endregion

        #region Stream overrides
        /// <summary>
        /// Reads uncompressed data into an array of bytes
        /// </summary>
        /// <param name="buffer">
        /// The buffer to read uncompressed data into
        /// </param>
        /// <param name="offset">
        /// The offset indicating where the data should be placed
        /// </param>
        /// <param name="count">
        /// The number of uncompressed bytes to be read
        /// </param>
        /// <returns>Returns the number of bytes actually read.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            // A GZIP file can contain multiple blocks of compressed data, although this is quite rare.
            // A compressed block could potentially be empty, so we need to loop until we reach EOF or
            // we find data.
            while (true)
            {

                // If we haven't read the header for this block, read it
                if (!readGZIPHeader)
                {

                    // Try to read header. If there is no header (0 bytes available), this is EOF. If there is
                    // an incomplete header, this will throw an exception.
                    if (!ReadHeader())
                    {
                        return 0;
                    }
                }

                // Try to read compressed data
                int bytesRead = base.Read(buffer, offset, count);
                if (bytesRead > 0)
                {
                    crc.Update(buffer, offset, bytesRead);
                }

                // If this is the end of stream, read the footer
                if (inf.IsFinished)
                {
                    ReadFooter();
                }

                if (bytesRead > 0)
                {
                    return bytesRead;
                }
            }
        }
        #endregion

        #region Support routines
        bool ReadHeader()
        {
            // Initialize CRC for this block
            crc = new Crc32();

            // Make sure there is data in file. We can't rely on ReadLeByte() to fill the buffer, as this could be EOF,
            // which is fine, but ReadLeByte() throws an exception if it doesn't find data, so we do this part ourselves.
            if (inputBuffer.Available <= 0)
            {
                inputBuffer.Fill();
                if (inputBuffer.Available <= 0)
                {
                    // No header, EOF.
                    return false;
                }
            }

            // 1. Check the two magic bytes
            Crc32 headCRC = new Crc32();
            int magic = inputBuffer.ReadLeByte();

            if (magic < 0)
            {
                throw new EndOfStreamException("EOS reading GZIP header");
            }

            headCRC.Update(magic);
            if (magic != (0x1F8B >> 8))
            {
                throw new Exception("Error GZIP header, first magic byte doesn't match");
            }

            //magic = baseInputStream.ReadByte();
            magic = inputBuffer.ReadLeByte();

            if (magic < 0)
            {
                throw new EndOfStreamException("EOS reading GZIP header");
            }

            if (magic != (0x1F8B & 0xFF))
            {
                throw new Exception("Error GZIP header,  second magic byte doesn't match");
            }

            headCRC.Update(magic);

            // 2. Check the compression type (must be 8)
            int compressionType = inputBuffer.ReadLeByte();

            if (compressionType < 0)
            {
                throw new EndOfStreamException("EOS reading GZIP header");
            }

            if (compressionType != 8)
            {
                throw new Exception("Error GZIP header, data not in deflate format");
            }
            headCRC.Update(compressionType);

            // 3. Check the flags
            int flags = inputBuffer.ReadLeByte();
            if (flags < 0)
            {
                throw new EndOfStreamException("EOS reading GZIP header");
            }
            headCRC.Update(flags);

            if ((flags & 0xE0) != 0)
            {
                throw new Exception("Reserved flag bits in GZIP header != 0");
            }

            for (int i = 0; i < 6; i++)
            {
                int readByte = inputBuffer.ReadLeByte();
                if (readByte < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }
                headCRC.Update(readByte);
            }

            if ((flags & 0x4) != 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    int readByte = inputBuffer.ReadLeByte();
                    if (readByte < 0)
                    {
                        throw new EndOfStreamException("EOS reading GZIP header");
                    }
                    headCRC.Update(readByte);
                }

                if (inputBuffer.ReadLeByte() < 0 || inputBuffer.ReadLeByte() < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }

                int len1, len2;
                len1 = inputBuffer.ReadLeByte();
                len2 = inputBuffer.ReadLeByte();
                if ((len1 < 0) || (len2 < 0))
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }
                headCRC.Update(len1);
                headCRC.Update(len2);

                int extraLen = (len1 << 8) | len2;
                for (int i = 0; i < extraLen; i++)
                {
                    int readByte = inputBuffer.ReadLeByte();
                    if (readByte < 0)
                    {
                        throw new EndOfStreamException("EOS reading GZIP header");
                    }
                    headCRC.Update(readByte);
                }
            }

            if ((flags & 0x8) != 0)
            {
                int readByte;
                while ((readByte = inputBuffer.ReadLeByte()) > 0)
                {
                    headCRC.Update(readByte);
                }

                if (readByte < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }
                headCRC.Update(readByte);
            }

            if ((flags & 0x10) != 0)
            {
                int readByte;
                while ((readByte = inputBuffer.ReadLeByte()) > 0)
                {
                    headCRC.Update(readByte);
                }

                if (readByte < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }

                headCRC.Update(readByte);
            }

            if ((flags & 0x2) != 0)
            {
                int tempByte;
                int crcval = inputBuffer.ReadLeByte();
                if (crcval < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }

                tempByte = inputBuffer.ReadLeByte();
                if (tempByte < 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP header");
                }

                crcval = (crcval << 8) | tempByte;
                if (crcval != ((int)headCRC.Value & 0xffff))
                {
                    throw new Exception("Header CRC value mismatch");
                }
            }

            readGZIPHeader = true;
            return true;
        }

        void ReadFooter()
        {
            byte[] footer = new byte[8];

            long bytesRead = inf.TotalOut & 0xffffffff;
            inputBuffer.Available += inf.RemainingInput;
            inf.Reset();

            int needed = 8;
            while (needed > 0)
            {
                int count = inputBuffer.ReadClearTextBuffer(footer, 8 - needed, needed);
                if (count <= 0)
                {
                    throw new EndOfStreamException("EOS reading GZIP footer");
                }
                needed -= count;
            }

            int crcval = (footer[0] & 0xff) | ((footer[1] & 0xff) << 8) | ((footer[2] & 0xff) << 16) | (footer[3] << 24);
            if (crcval != (int)crc.Value)
            {
                throw new Exception("GZIP crc sum mismatch, theirs \"" + crcval + "\" and ours \"" + (int)crc.Value);
            }

            uint total =
                (uint)((uint)footer[4] & 0xff) |
                (uint)(((uint)footer[5] & 0xff) << 8) |
                (uint)(((uint)footer[6] & 0xff) << 16) |
                (uint)((uint)footer[7] << 24);

            if (bytesRead != total)
            {
                throw new Exception("Number of bytes mismatch in footer");
            }

            readGZIPHeader = false;
        }
        #endregion
    }
    #endregion

}
