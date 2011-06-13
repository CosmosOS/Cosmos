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
 * Scanline-oriented Read Support
 */

using System;
using System.Diagnostics;
using System.IO;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        /// <summary>
        /// undefined state
        /// </summary>
        private const int NOSTRIP = -1;

        /// <summary>
        /// undefined state
        /// </summary>
        private const int NOTILE = -1;

        internal const int O_RDONLY = 0;
        internal const int O_WRONLY = 0x0001;
        internal const int O_CREAT = 0x0100;
        internal const int O_TRUNC = 0x0200;
        internal const int O_RDWR = 0x0002;

        //
        // Default Read/Seek/Write definitions.
        //

        private int readFile(byte[] buf, int offset, int size)
        {
            return m_stream.Read(m_clientdata, buf, offset, size);
        }

        private long seekFile(long off, SeekOrigin whence)
        {
            return m_stream.Seek(m_clientdata, off, whence);
        }

        private long getFileSize()
        {
            return m_stream.Size(m_clientdata);
        }

        private bool readOK(byte[] buf, int size)
        {
            return (readFile(buf, 0, size) == size);
        }

        private bool readShortOK(out short value)
        {
            byte[] bytes = new byte[2];
            bool res = readOK(bytes, 2);
            value = 0;
            if (res)
            {
                value = (short)(bytes[0] & 0xFF);
                value += (short)((bytes[1] & 0xFF) << 8);
            }

            return res;
        }

        private bool readUIntOK(out uint value)
        {
            int temp;
            bool res = readIntOK(out temp);
            if (res)
                value = (uint)temp;
            else
                value = 0;

            return res;
        }

        private bool readIntOK(out int value)
        {
            byte[] cp = new byte[4];
            bool res = readOK(cp, 4);
            value = 0;
            if (res)
            {
                value = cp[0] & 0xFF;
                value += (cp[1] & 0xFF) << 8;
                value += (cp[2] & 0xFF) << 16;
                value += cp[3] << 24;
            }

            return res;
        }

        private bool readDirEntryOk(TiffDirEntry[] dir, short dircount)
        {
            int entrySize = sizeof(short) * 2 + sizeof(int) * 2;
            int totalSize = entrySize * dircount;
            byte[] bytes = new byte[totalSize];
            bool res = readOK(bytes, totalSize);
            if (res)
                readDirEntry(dir, dircount, bytes, 0);

            return res;
        }

        private static void readDirEntry(TiffDirEntry[] dir, short dircount, byte[] bytes, int offset)
        {
            int pos = offset;
            for (int i = 0; i < dircount; i++)
            {
                TiffDirEntry entry = new TiffDirEntry();
                entry.tdir_tag = (TiffTag)(ushort)readShort(bytes, pos);
                pos += sizeof(short);
                entry.tdir_type = (TiffType)readShort(bytes, pos);
                pos += sizeof(short);
                entry.tdir_count = readInt(bytes, pos);
                pos += sizeof(int);
                entry.tdir_offset = (uint)readInt(bytes, pos);
                pos += sizeof(int);
                dir[i] = entry;
            }
        }

        private bool readHeaderOk(ref TiffHeader header)
        {
            bool res = readShortOK(out header.tiff_magic);

            if (res)
                res = readShortOK(out header.tiff_version);

            if (res)
                res = readUIntOK(out header.tiff_diroff);

            return res;
        }

        private bool seekOK(long off)
        {
            return (seekFile(off, SeekOrigin.Begin) == off);
        }

        /*
        * Seek to a random row+sample in a file.
        */
        private bool seek(int row, short sample)
        {
            if (row >= m_dir.td_imagelength)
            {
                /* out of range */
                ErrorExt(this, m_clientdata, m_name,
                    "{0}: Row out of range, max {1}", row, m_dir.td_imagelength);
                return false;
            }

            int strip;
            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
            {
                if (sample >= m_dir.td_samplesperpixel)
                {
                    ErrorExt(this, m_clientdata, m_name,
                        "{0}: Sample out of range, max {1}", sample, m_dir.td_samplesperpixel);
                    return false;
                }

                if (m_dir.td_rowsperstrip != -1)
                    strip = sample * m_dir.td_stripsperimage + row / m_dir.td_rowsperstrip;
                else
                    strip = 0;
            }
            else
            {
                if (m_dir.td_rowsperstrip != -1)
                    strip = row / m_dir.td_rowsperstrip;
                else
                    strip = 0;
            }
            
            if (strip != m_curstrip)
            {
                /* different strip, refill */
                if (!fillStrip(strip))
                    return false;
            }
            else if (row < m_row)
            {
                /*
                 * Moving backwards within the same strip: backup
                 * to the start and then decode forward (below).
                 *
                 * NB: If you're planning on lots of random access within a
                 * strip, it's better to just read and decode the entire
                 * strip, and then access the decoded data in a random fashion.
                 */
                if (!startStrip(strip))
                    return false;
            }

            if (row != m_row)
            {
                /*
                 * Seek forward to the desired row.
                 */
                if (!m_currentCodec.Seek(row - m_row))
                    return false;

                m_row = row;
            }

            return true;
        }

        private int readRawStrip1(int strip, byte[] buf, int offset, int size, string module)
        {
            Debug.Assert((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW);

            if (!seekOK(m_dir.td_stripoffset[strip]))
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Seek error at scanline {1}, strip {2}", m_name, m_row, strip);
                return -1;
            }

            int cc = readFile(buf, offset, size);
            if (cc != size)
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Read error at scanline {1}; got {2} bytes, expected {3}",
                    m_name, m_row, cc, size);
                return -1;
            }

            return size;
        }

        private int readRawTile1(int tile, byte[] buf, int offset, int size, string module)
        {
            Debug.Assert((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW);

            if (!seekOK(m_dir.td_stripoffset[tile]))
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Seek error at row {1}, col {2}, tile {3}", m_name, m_row, m_col, tile);
                return -1;
            }

            int cc = readFile(buf, offset, size);
            if (cc != size)
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Read error at row {1}, col {2}; got {3} bytes, expected {4}",
                    m_name, m_row, m_col, cc, size);
                return -1;
            }

            return size;
        }

        /// <summary>
        /// Set state to appear as if a strip has just been read in.
        /// </summary>
        private bool startStrip(int strip)
        {
            if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
            {
                if (!m_currentCodec.SetupDecode())
                    return false;

                m_flags |= TiffFlags.CODERSETUP;
            }

            m_curstrip = strip;
            m_row = (strip % m_dir.td_stripsperimage) * m_dir.td_rowsperstrip;
            m_rawcp = 0;

            if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
                m_rawcc = 0;
            else
                m_rawcc = (int)m_dir.td_stripbytecount[strip];

            return m_currentCodec.PreDecode((short)(strip / m_dir.td_stripsperimage));
        }

        /*
        * Set state to appear as if a
        * tile has just been read in.
        */
        private bool startTile(int tile)
        {
            if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
            {
                if (!m_currentCodec.SetupDecode())
                    return false;

                m_flags |= TiffFlags.CODERSETUP;
            }

            m_curtile = tile;
            m_row = (tile % howMany(m_dir.td_imagewidth, m_dir.td_tilewidth)) * m_dir.td_tilelength;
            m_col = (tile % howMany(m_dir.td_imagelength, m_dir.td_tilelength)) * m_dir.td_tilewidth;
            m_rawcp = 0;

            if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
                m_rawcc = 0;
            else
                m_rawcc = (int)m_dir.td_stripbytecount[tile];

            return m_currentCodec.PreDecode((short)(tile / m_dir.td_stripsperimage));
        }

        private bool checkRead(bool tiles)
        {
            if (m_mode == O_WRONLY)
            {
                ErrorExt(this, m_clientdata, m_name, "File not open for reading");
                return false;
            }

            if (tiles ^ IsTiled())
            {
                ErrorExt(this, m_clientdata, m_name, tiles ?
                    "Can not read tiles from a stripped image" :
                    "Can not read scanlines from a tiled image");
                return false;
            }

            return true;
        }

        private static void swab16BitData(byte[] buffer, int offset, int count)
        {
            Debug.Assert((count & 1) == 0);
            short[] swabee = ByteArrayToShorts(buffer, offset, count);
            SwabArrayOfShort(swabee, count / 2);
            ShortsToByteArray(swabee, 0, count / 2, buffer, offset);
        }

        private static void swab24BitData(byte[] buffer, int offset, int count)
        {
            Debug.Assert((count % 3) == 0);
            SwabArrayOfTriples(buffer, offset, count / 3);
        }

        private static void swab32BitData(byte[] buffer, int offset, int count)
        {
            Debug.Assert((count & 3) == 0);
            int[] swabee = ByteArrayToInts(buffer, offset, count);
            SwabArrayOfLong(swabee, count / 4);
            IntsToByteArray(swabee, 0, count / 4, buffer, offset);
        }

        private static void swab64BitData(byte[] buffer, int offset, int count)
        {
            Debug.Assert((count & 7) == 0);

            int doubleCount = count / 8;
            double[] doubles = new double[doubleCount];
            int byteOffset = offset;
            for (int i = 0; i < doubleCount; i++)
            {
                doubles[i] = BitConverter.ToDouble(buffer, byteOffset);
                byteOffset += 8;
            }

            SwabArrayOfDouble(doubles, doubleCount);

            byteOffset = offset;
            for (int i = 0; i < doubleCount; i++)
            {
                byte[] bytes = BitConverter.GetBytes(doubles[i]);
                Buffer.BlockCopy(bytes, 0, buffer, byteOffset, bytes.Length);
                byteOffset += bytes.Length;
            }
        }

        /// <summary>
        /// Read the specified strip and setup for decoding.
        /// The data buffer is expanded, as necessary, to hold the strip's data.
        /// </summary>
        internal bool fillStrip(int strip)
        {
            const string module = "fillStrip";

            if ((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW)
            {
                int bytecount = (int)m_dir.td_stripbytecount[strip];
                if (bytecount <= 0)
                {
                    ErrorExt(this, m_clientdata, m_name,
                        "{0}: Invalid strip byte count, strip {1}", bytecount, strip);
                    return false;
                }

                /*
                 * Expand raw data buffer, if needed, to
                 * hold data strip coming from file
                 * (perhaps should set upper bound on
                 *  the size of a buffer we'll use?).
                 */
                if (bytecount > m_rawdatasize)
                {
                    m_curstrip = NOSTRIP;
                    if ((m_flags & TiffFlags.MYBUFFER) != TiffFlags.MYBUFFER)
                    {
                        ErrorExt(this, m_clientdata, module,
                            "{0}: Data buffer too small to hold strip {1}", m_name, strip);
                        return false;
                    }

                    ReadBufferSetup(null, roundUp(bytecount, 1024));
                }
                
                if (readRawStrip1(strip, m_rawdata, 0, bytecount, module) != bytecount)
                    return false;

                if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
                    ReverseBits(m_rawdata, bytecount);
            }

            return startStrip(strip);
        }

        /// <summary>
        /// Read the specified tile and setup for decoding. 
        /// The data buffer is expanded, as necessary, to hold the tile's data.
        /// </summary>
        internal bool fillTile(int tile)
        {
            const string module = "fillTile";

            if ((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW)
            {
                int bytecount = (int)m_dir.td_stripbytecount[tile];
                if (bytecount <= 0)
                {
                    ErrorExt(this, m_clientdata, m_name,
                        "{0}: Invalid tile byte count, tile {1}", bytecount, tile);
                    return false;
                }

                /*
                 * Expand raw data buffer, if needed, to
                 * hold data tile coming from file
                 * (perhaps should set upper bound on
                 *  the size of a buffer we'll use?).
                 */
                if (bytecount > m_rawdatasize)
                {
                    m_curtile = NOTILE;
                    if ((m_flags & TiffFlags.MYBUFFER) != TiffFlags.MYBUFFER)
                    {
                        ErrorExt(this, m_clientdata, module,
                            "{0}: Data buffer too small to hold tile {1}", m_name, tile);
                        return false;
                    }

                    ReadBufferSetup(null, roundUp(bytecount, 1024));
                }

                if (readRawTile1(tile, m_rawdata, 0, bytecount, module) != bytecount)
                    return false;

                if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
                    ReverseBits(m_rawdata, bytecount);
            }

            return startTile(tile);
        }
    }
}
