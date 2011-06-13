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
 * Scanline-oriented Write Support
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
        private bool writeCheckStrips(string module)
        {
            return ((m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING || WriteCheck(false, module));
        }

        private bool writeCheckTiles(string module)
        {
            return ((m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING || WriteCheck(true, module));
        }

        private void bufferCheck()
        {
            if (!((m_flags & TiffFlags.BUFFERSETUP) == TiffFlags.BUFFERSETUP && m_rawdata != null))
                WriteBufferSetup(null, -1);
        }

        private bool writeOK(byte[] buffer, int offset, int count)
        {
            try
            {
                m_stream.Write(m_clientdata, buffer, offset, count);
            }
            catch (Exception)
            {
                Tiff.Warning(this, "writeOK", "Failed to write {0} bytes", count);
                return false;
            }

            return true;
        }

        private bool writeHeaderOK(TiffHeader header)
        {
            bool res = writeShortOK(header.tiff_magic);

            if (res)
                res = writeShortOK(header.tiff_version);

            if (res)
                res = writeIntOK((int)header.tiff_diroff);

            return res;
        }

        private bool writeDirEntryOK(TiffDirEntry[] entries, int count)
        {
            bool res = true;
            for (int i = 0; i < count; i++)
            {
                res = writeShortOK((short)entries[i].tdir_tag);

                if (res)
                    res = writeShortOK((short)entries[i].tdir_type);

                if (res)
                    res = writeIntOK(entries[i].tdir_count);

                if (res)
                    res = writeIntOK((int)entries[i].tdir_offset);

                if (!res)
                    break;
            }

            return res;
        }

        private bool writeShortOK(short value)
        {
            byte[] cp = new byte[2];
            cp[0] = (byte)value;
            cp[1] = (byte)(value >> 8);

            return writeOK(cp, 0, 2);
        }

        private bool writeIntOK(int value)
        {
            byte[] cp = new byte[4];
            cp[0] = (byte)value;
            cp[1] = (byte)(value >> 8);
            cp[2] = (byte)(value >> 16);
            cp[3] = (byte)(value >> 24);

            return writeOK(cp, 0, 4);
        }

        private bool isUnspecified(int f)
        {
            return (fieldSet(f) && m_dir.td_imagelength == 0);
        }

        /*
        * Grow the strip data structures by delta strips.
        */
        private bool growStrips(int delta)
        {
            Debug.Assert(m_dir.td_planarconfig == PlanarConfig.CONTIG);
            uint[] new_stripoffset = Realloc(m_dir.td_stripoffset, m_dir.td_nstrips, m_dir.td_nstrips + delta);
            uint[] new_stripbytecount = Realloc(m_dir.td_stripbytecount, m_dir.td_nstrips, m_dir.td_nstrips + delta);
            m_dir.td_stripoffset = new_stripoffset;
            m_dir.td_stripbytecount = new_stripbytecount;
            Array.Clear(m_dir.td_stripoffset, m_dir.td_nstrips, delta);
            Array.Clear(m_dir.td_stripbytecount, m_dir.td_nstrips, delta);
            m_dir.td_nstrips += delta;
            return true;
        }

        /// <summary>
        /// Appends the data to the specified strip.
        /// </summary>
        private bool appendToStrip(int strip, byte[] buffer, int offset, int count)
        {
            const string module = "appendToStrip";

            if (m_dir.td_stripoffset[strip] == 0 || m_curoff == 0)
            {
                Debug.Assert(m_dir.td_nstrips > 0);

                if (m_dir.td_stripbytecount[strip] != 0 &&
                    m_dir.td_stripoffset[strip] != 0 &&
                    m_dir.td_stripbytecount[strip] >= count)
                {
                    // There is already tile data on disk, and the new tile 
                    // data we have to will fit in the same space. The only
                    // aspect of this that is risky is that there could be
                    // more data to append to this strip before we are done
                    // depending on how we are getting called.
                    if (!seekOK(m_dir.td_stripoffset[strip]))
                    {
                        ErrorExt(this, m_clientdata, module, "Seek error at scanline {0}", m_row);
                        return false;
                    }
                }
                else
                {
                    // Seek to end of file, and set that as our location
                    // to write this strip.
                    m_dir.td_stripoffset[strip] = (uint)seekFile(0, SeekOrigin.End);
                }

                m_curoff = m_dir.td_stripoffset[strip];

                // We are starting a fresh strip/tile, so set the size to zero.
                m_dir.td_stripbytecount[strip] = 0;
            }

            if (!writeOK(buffer, offset, count))
            {
                ErrorExt(this, m_clientdata, module, "Write error at scanline {0}", m_row);
                return false;
            }

            m_curoff += (uint)count;
            m_dir.td_stripbytecount[strip] += (uint)count;
            return true;
        }

        /*
        * Internal version of FlushData that can be
        * called by ``encodestrip routines'' w/o concern
        * for infinite recursion.
        */
        internal bool flushData1()
        {
            if (m_rawcc > 0)
            {
                if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
                    ReverseBits(m_rawdata, m_rawcc);

                if (!appendToStrip(IsTiled() ? m_curtile : m_curstrip, m_rawdata, 0, m_rawcc))
                    return false;

                m_rawcc = 0;
                m_rawcp = 0;
            }

            return true;
        }
    }
}
