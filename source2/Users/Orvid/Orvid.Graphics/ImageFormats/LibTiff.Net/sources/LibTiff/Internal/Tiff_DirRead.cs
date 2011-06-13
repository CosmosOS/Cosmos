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
 * Directory Read Support Routines.
 */

using System;
using System.Diagnostics;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private int extractData(TiffDirEntry dir)
        {
            int type = (int)dir.tdir_type;
            if (m_header.tiff_magic == TIFF_BIGENDIAN)
                return (int)((dir.tdir_offset >> m_typeshift[type]) & m_typemask[type]);

            return (int)(dir.tdir_offset & m_typemask[type]);
        }

        private bool byteCountLooksBad(TiffDirectory td)
        {
            /* 
             * Assume we have wrong StripByteCount value (in case of single strip) in
             * following cases:
             *   - it is equal to zero along with StripOffset;
             *   - it is larger than file itself (in case of uncompressed image);
             *   - it is smaller than the size of the bytes per row multiplied on the
             *     number of rows.  The last case should not be checked in the case of
             *     writing new image, because we may do not know the exact strip size
             *     until the whole image will be written and directory dumped out.
             */
            return
            (
                (td.td_stripbytecount[0] == 0 && td.td_stripoffset[0] != 0) ||
                (td.td_compression == Compression.NONE && td.td_stripbytecount[0] > getFileSize() - td.td_stripoffset[0]) ||
                (m_mode == O_RDONLY && td.td_compression == Compression.NONE && td.td_stripbytecount[0] < ScanlineSize() * td.td_imagelength)
            );
        }

        private static int howMany8(int x)
        {
            return ((x & 0x07) != 0 ? (x >> 3) + 1 : x >> 3);
        }

        private bool estimateStripByteCounts(TiffDirEntry[] dir, short dircount)
        {
            const string module = "estimateStripByteCounts";

            m_dir.td_stripbytecount = new uint [m_dir.td_nstrips];

            if (m_dir.td_compression != Compression.NONE)
            {
                long space = TiffHeader.SizeInBytes + sizeof(short) + (dircount * TiffDirEntry.SizeInBytes) + sizeof(int);
                long filesize = getFileSize();

                // calculate amount of space used by indirect values
                for (short n = 0; n < dircount; n++)
                {
                    int cc = DataWidth((TiffType)dir[n].tdir_type);
                    if (cc == 0)
                    {
                        ErrorExt(this, m_clientdata, module,
                            "{0}: Cannot determine size of unknown tag type {1}",
                            m_name, dir[n].tdir_type);
                        return false;
                    }

                    cc = cc * dir[n].tdir_count;
                    if (cc > sizeof(int))
                        space += cc;
                }

                space = filesize - space;
                if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                    space /= m_dir.td_samplesperpixel;
                
                int strip = 0;
                for ( ; strip < m_dir.td_nstrips; strip++)
                    m_dir.td_stripbytecount[strip] = (uint)space;
                
                // This gross hack handles the case were the offset to the last
                // strip is past the place where we think the strip should begin.
                // Since a strip of data must be contiguous, it's safe to assume
                // that we've overestimated the amount of data in the strip and
                // trim this number back accordingly.
                strip--;
                if ((m_dir.td_stripoffset[strip] + m_dir.td_stripbytecount[strip]) > filesize)
                    m_dir.td_stripbytecount[strip] = (uint)(filesize - m_dir.td_stripoffset[strip]);
            }
            else if (IsTiled()) 
            {
                int bytespertile = TileSize();
                for (int strip = 0; strip < m_dir.td_nstrips; strip++)
                    m_dir.td_stripbytecount[strip] = (uint)bytespertile;
            }
            else
            {
                int rowbytes = ScanlineSize();
                int rowsperstrip = m_dir.td_imagelength / m_dir.td_stripsperimage;
                for (int strip = 0; strip < m_dir.td_nstrips; strip++)
                    m_dir.td_stripbytecount[strip] = (uint)(rowbytes * rowsperstrip);
            }
            
            setFieldBit(FieldBit.StripByteCounts);
            if (!fieldSet(FieldBit.RowsPerStrip))
                m_dir.td_rowsperstrip = m_dir.td_imagelength;

            return true;
        }

        private void missingRequired(string tagname)
        {
            const string module = "missingRequired";
            ErrorExt(this, m_clientdata, module,
                "{0}: TIFF directory is missing required \"{1}\" field",
                m_name, tagname);
        }

        private int fetchFailed(TiffDirEntry dir)
        {
            ErrorExt(this, m_clientdata, m_name,
                "Error fetching data for field \"{0}\"", FieldWithTag(dir.tdir_tag).Name);
            return 0;
        }

        private static int readDirectoryFind(TiffDirEntry[] dir, short dircount, TiffTag tagid)
        {
            for (short n = 0; n < dircount; n++)
            {
                if (dir[n].tdir_tag == tagid)
                    return n;
            }

            return -1;
        }

        /// <summary>
        /// Checks the directory offset against the list of already seen directory
        /// offsets.
        /// </summary>
        /// <remarks> This is a trick to prevent IFD looping. The one can
        /// create TIFF file with looped directory pointers. We will maintain a
        /// list of already seen directories and check every IFD offset against
        /// that list.</remarks>
        private bool checkDirOffset(uint diroff)
        {
            if (diroff == 0)
            {
                // no more directories
                return false;
            }

            for (short n = 0; n < m_dirnumber && m_dirlist != null; n++)
            {
                if (m_dirlist[n] == diroff)
                    return false;
            }

            m_dirnumber++;

            if (m_dirnumber > m_dirlistsize)
            {
                // XXX: Reduce memory allocation granularity of the dirlist array.
                uint[] new_dirlist = Realloc(m_dirlist, m_dirnumber - 1, 2 * m_dirnumber);
                m_dirlistsize = 2 * m_dirnumber;
                m_dirlist = new_dirlist;
            }

            m_dirlist[m_dirnumber - 1] = diroff;
            return true;
        }
        
        /// <summary>
        /// Reads IFD structure from the specified offset.
        /// </summary>
        /// <returns>The number of fields in the directory or 0 if failed.</returns>
        private short fetchDirectory(uint diroff, out TiffDirEntry[] pdir, out uint nextdiroff)
        {
            const string module = "fetchDirectory";

            m_diroff = diroff;
            nextdiroff = 0;

            short dircount;
            TiffDirEntry[] dir = null;
            pdir = null;

            if (!seekOK(m_diroff)) 
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Seek error accessing TIFF directory", m_name);
                return 0;
            }
            
            if (!readShortOK(out dircount)) 
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Can not read TIFF directory count", m_name);
                return 0;
            }

            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabShort(ref dircount);

            dir = new TiffDirEntry [dircount];
            if (!readDirEntryOk(dir, dircount))
            {
                ErrorExt(this, m_clientdata, module, "{0}: Can not read TIFF directory", m_name);
                return 0;
            }

            // Read offset to next directory for sequential scans.
            int temp;
            readIntOK(out temp);
            nextdiroff = (uint)temp;

            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
            {
                temp = (int)nextdiroff;
                SwabLong(ref temp);
                nextdiroff = (uint)temp;
            }

            pdir = dir;
            return dircount;
        }

        /*
        * Fetch and set the SubjectDistance EXIF tag.
        */
        private bool fetchSubjectDistance(TiffDirEntry dir)
        {
            if (dir.tdir_count != 1 || dir.tdir_type != TiffType.RATIONAL)
            {
                Tiff.WarningExt(this, m_clientdata, m_name,
                    "incorrect count or type for SubjectDistance, tag ignored");

                return false;
            }

            bool ok = false;
            
            byte[] b = new byte[2 * sizeof(int)];
            int read = fetchData(dir, b);
            if (read != 0)
            {
                int[] l = new int[2];
                l[0] = readInt(b, 0);
                l[1] = readInt(b, sizeof(int));

                float v;
                if (cvtRational(dir, l[0], l[1], out v)) 
                {
                    /*
                    * XXX: Numerator -1 means that we have infinite
                    * distance. Indicate that with a negative floating point
                    * SubjectDistance value.
                    */
                    ok = SetField(dir.tdir_tag, (l[0] != -1) ? v : -v);
                }
            }

            return ok;
        }

        /*
        * Check the count field of a directory
        * entry against a known value.  The caller
        * is expected to skip/ignore the tag if
        * there is a mismatch.
        */
        private bool checkDirCount(TiffDirEntry dir, int count)
        {
            if (count > dir.tdir_count)
            {
                WarningExt(this, m_clientdata, m_name,
                    "incorrect count for field \"{0}\" ({1}, expecting {2}); tag ignored",
                    FieldWithTag(dir.tdir_tag).Name, dir.tdir_count, count);
                return false;
            }
            else if (count < dir.tdir_count)
            {
                WarningExt(this, m_clientdata, m_name,
                    "incorrect count for field \"{0}\" ({1}, expecting {2}); tag trimmed",
                    FieldWithTag(dir.tdir_tag).Name, dir.tdir_count, count);
                return true;
            }

            return true;
        }

        /// <summary>
        /// Fetches a contiguous directory item.
        /// </summary>
        private int fetchData(TiffDirEntry dir, byte[] buffer)
        {
            int width = DataWidth(dir.tdir_type);
            int count = (int)dir.tdir_count * width;

            // Check for overflow.
            if (dir.tdir_count == 0 || width == 0 || (count / width) != dir.tdir_count)
                fetchFailed(dir);

            if (!seekOK(dir.tdir_offset))
                fetchFailed(dir);

            if (!readOK(buffer, count))
                fetchFailed(dir);

            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
            {
                switch (dir.tdir_type)
                {
                    case TiffType.SHORT:
                    case TiffType.SSHORT:
                        short[] s = ByteArrayToShorts(buffer, 0, count);
                        SwabArrayOfShort(s, dir.tdir_count);
                        ShortsToByteArray(s, 0, dir.tdir_count, buffer, 0);
                        break;
                    
                    case TiffType.LONG:
                    case TiffType.SLONG:
                    case TiffType.FLOAT:
                        int[] l = ByteArrayToInts(buffer, 0, count);
                        SwabArrayOfLong(l, dir.tdir_count);
                        IntsToByteArray(l, 0, dir.tdir_count, buffer, 0);
                        break;
                    
                    case TiffType.RATIONAL:
                    case TiffType.SRATIONAL:
                        int[] r = ByteArrayToInts(buffer, 0, count);
                        SwabArrayOfLong(r, 2 * dir.tdir_count);
                        IntsToByteArray(r, 0, 2 * dir.tdir_count, buffer, 0);
                        break;
                    
                    case TiffType.DOUBLE:
                        swab64BitData(buffer, 0, count);
                        break;
                }
            }

            return count;
        }

        /// <summary>
        /// Fetches an ASCII item from the file.
        /// </summary>
        private int fetchString(TiffDirEntry dir, out string cp)
        {
            byte[] bytes = null;

            if (dir.tdir_count <= 4)
            {
                int l = (int)dir.tdir_offset;
                if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    SwabLong(ref l);

                bytes = new byte[sizeof(int)];
                writeInt(l, bytes, 0);
                cp = Latin1Encoding.GetString(bytes, 0, dir.tdir_count);
                return 1;
            }

            bytes = new byte[dir.tdir_count];
            int res = fetchData(dir, bytes);
            cp = Latin1Encoding.GetString(bytes, 0, dir.tdir_count);
            return res;
        }

        /*
        * Convert numerator+denominator to float.
        */
        private bool cvtRational(TiffDirEntry dir, int num, int denom, out float rv)
        {
            if (denom == 0)
            {
                ErrorExt(this, m_clientdata, m_name,
                    "{0}: Rational with zero denominator (num = {1})",
                    FieldWithTag(dir.tdir_tag).Name, num);
                rv = float.NaN;
                return false;
            }
            else
            {
                rv = ((float)num / (float)denom);
                return true;
            }
        }

        /*
        * Fetch a rational item from the file
        * at offset off and return the value
        * as a floating point number.
        */
        private float fetchRational(TiffDirEntry dir)
        {
            byte[] bytes = new byte[sizeof(int) * 2];
            int read = fetchData(dir, bytes);
            if (read != 0)
            {
                int[] l = new int[2];
                l[0] = readInt(bytes, 0);
                l[1] = readInt(bytes, sizeof(int));

                float v;
                bool res = cvtRational(dir, l[0], l[1], out v);
                if (res)
                    return v;
            }

            return 1.0f;
        }

        /// <summary>
        /// Fetch a single floating point value from the offset field and
        /// return it as a native float.
        /// </summary>
        private float fetchFloat(TiffDirEntry dir)
        {
            int l = extractData(dir);
            return BitConverter.ToSingle(BitConverter.GetBytes(l), 0);
        }

        /// <summary>
        /// Fetches an array of BYTE or SBYTE values.
        /// </summary>
        private bool fetchByteArray(TiffDirEntry dir, byte[] v)
        {
            if (dir.tdir_count <= 4)
            {
                // Extract data from offset field.
                int count = dir.tdir_count;

                if (m_header.tiff_magic == TIFF_BIGENDIAN)
                {
                    if (count == 4)
                        v[3] = (byte)(dir.tdir_offset & 0xff);

                    if (count >= 3)
                        v[2] = (byte)((dir.tdir_offset >> 8) & 0xff);

                    if (count >= 2)
                        v[1] = (byte)((dir.tdir_offset >> 16) & 0xff);

                    if (count >= 1)
                        v[0] = (byte)(dir.tdir_offset >> 24);
                }
                else
                {
                    if (count == 4)
                        v[3] = (byte)(dir.tdir_offset >> 24);

                    if (count >= 3)
                        v[2] = (byte)((dir.tdir_offset >> 16) & 0xff);

                    if (count >= 2)
                        v[1] = (byte)((dir.tdir_offset >> 8) & 0xff);

                    if (count >= 1)
                        v[0] = (byte)(dir.tdir_offset & 0xff);
                }

                return true;
            }

            return (fetchData(dir, v) != 0);
        }

        /// <summary>
        /// Fetch an array of SHORT or SSHORT values.
        /// </summary>
        private bool fetchShortArray(TiffDirEntry dir, short[] v)
        {
            if (dir.tdir_count <= 2)
            {
                int count = dir.tdir_count;

                if (m_header.tiff_magic == TIFF_BIGENDIAN)
                {
                    if (count == 2)
                        v[1] = (short)(dir.tdir_offset & 0xffff);

                    if (count >= 1)
                        v[0] = (short)(dir.tdir_offset >> 16);
                }
                else
                {
                    if (count == 2)
                        v[1] = (short)(dir.tdir_offset >> 16);

                    if (count >= 1)
                        v[0] = (short)(dir.tdir_offset & 0xffff);
                }

                return true;
            }

            int cc = dir.tdir_count * sizeof(short);
            byte[] b = new byte[cc];
            int read = fetchData(dir, b);
            if (read != 0)
                Buffer.BlockCopy(b, 0, v, 0, b.Length);

            return (read != 0);
        }

        /*
        * Fetch a pair of SHORT or BYTE values. Some tags may have either BYTE
        * or SHORT type and this function works with both ones.
        */
        private bool fetchShortPair(TiffDirEntry dir)
        {
            /*
            * Prevent overflowing arrays below by performing a sanity
            * check on tdir_count, this should never be greater than two.
            */
            if (dir.tdir_count > 2) 
            {
                WarningExt(this, m_clientdata, m_name,
                    "unexpected count for field \"{0}\", {1}, expected 2; ignored",
                    FieldWithTag(dir.tdir_tag).Name, dir.tdir_count);
                return false;
            }

            switch (dir.tdir_type)
            {
                case TiffType.BYTE:
                case TiffType.SBYTE:
                    byte[] bytes = new byte[4];
                    return fetchByteArray(dir, bytes) && SetField(dir.tdir_tag, bytes[0], bytes[1]);

                case TiffType.SHORT:
                case TiffType.SSHORT:
                    short[] shorts = new short[2];
                    return fetchShortArray(dir, shorts) && SetField(dir.tdir_tag, shorts[0], shorts[1]);
            }

            return false;
        }

        /// <summary>
        /// Fetches an array of LONG or SLONG values.
        /// </summary>
        private bool fetchLongArray(TiffDirEntry dir, int[] v)
        {
            if (dir.tdir_count == 1)
            {
                v[0] = (int)dir.tdir_offset;
                return true;
            }

            int cc = dir.tdir_count * sizeof(int);
            byte[] b = new byte[cc];
            int read = fetchData(dir, b);
            if (read != 0)
                Buffer.BlockCopy(b, 0, v, 0, b.Length);

            return (read != 0);
        }

        /// <summary>
        /// Fetch an array of RATIONAL or SRATIONAL values.
        /// </summary>
        private bool fetchRationalArray(TiffDirEntry dir, float[] v)
        {
            Debug.Assert(sizeof(float) == sizeof(int));

            bool ok = false;
            byte[] l = new byte [dir.tdir_count * DataWidth(dir.tdir_type)];
            if (fetchData(dir, l) != 0)
            {
                int offset = 0;
                int[] pair = new int[2];
                for (int i = 0; i < dir.tdir_count; i++)
                {
                    pair[0] = readInt(l, offset);
                    offset += sizeof(int);
                    pair[1] = readInt(l, offset);
                    offset += sizeof(int);

                    ok = cvtRational(dir, pair[0], pair[1], out v[i]);
                    if (!ok)
                        break;
                }
            }

            return ok;
        }

        /// <summary>
        /// Fetches an array of FLOAT values.
        /// </summary>
        private bool fetchFloatArray(TiffDirEntry dir, float[] v)
        {
            if (dir.tdir_count == 1)
            {
                v[0] = BitConverter.ToSingle(BitConverter.GetBytes(dir.tdir_offset), 0);
                return true;
            }

            int w = DataWidth(dir.tdir_type);
            int cc = dir.tdir_count * w;
            byte[] b = new byte [cc];
            int read = fetchData(dir, b);
            if (read != 0)
            {
                int byteOffset = 0;
                for (int i = 0; i < read / 4; i++)
                {
                    v[i] = BitConverter.ToSingle(b, byteOffset);
                    byteOffset += 4;
                }
            }

            return (read != 0);
        }

        /// <summary>
        /// Fetches an array of DOUBLE values.
        /// </summary>
        private bool fetchDoubleArray(TiffDirEntry dir, double[] v)
        {
            int w = DataWidth(dir.tdir_type);
            int cc = dir.tdir_count * w;
            byte[] b = new byte [cc];
            int read = fetchData(dir, b);
            if (read != 0)
            {
                int byteOffset = 0;
                for (int i = 0; i < read / 8; i++)
                {
                    v[i] = BitConverter.ToDouble(b, byteOffset);
                    byteOffset += 8;
                }
            }

            return (read != 0);
        }

        /// <summary>
        /// Fetches an array of ANY values.
        /// </summary>
        /// <remarks>The actual values are returned as doubles which should be
        /// able hold all the types. Note in particular that we assume that the
        /// double return value vector is large enough to read in any
        /// fundamental type. We use that vector as a buffer to read in the base
        /// type vector and then convert it in place to double (from end to
        /// front of course).</remarks>
        private bool fetchAnyArray(TiffDirEntry dir, double[] v)
        {
            int i = 0;
            bool res = false;
            switch (dir.tdir_type)
            {
                case TiffType.BYTE:
                case TiffType.SBYTE:
                    byte[] b = new byte[dir.tdir_count];
                    res = fetchByteArray(dir, b);
                    if (res)
                    {
                        for (i = dir.tdir_count - 1; i >= 0; i--)
                            v[i] = b[i];
                    }
                    
                    if (!res)
                        return false;

                    break;
                case TiffType.SHORT:
                case TiffType.SSHORT:
                    short[] u = new short[dir.tdir_count];
                    res = fetchShortArray(dir, u);
                    if (res)
                    {
                        for (i = dir.tdir_count - 1; i >= 0; i--)
                            v[i] = u[i];
                    }

                    if (!res)
                        return false;

                    break;
                case TiffType.LONG:
                case TiffType.SLONG:
                    int[] l = new int[dir.tdir_count];
                    res = fetchLongArray(dir, l);
                    if (res)
                    {
                        for (i = dir.tdir_count - 1; i >= 0; i--)
                            v[i] = l[i];
                    }

                    if (!res)
                        return false;

                    break;
                case TiffType.RATIONAL:
                case TiffType.SRATIONAL:
                    float[] r = new float[dir.tdir_count];
                    res = fetchRationalArray(dir, r);
                    if (res)
                    {
                        for (i = dir.tdir_count - 1; i >= 0; i--)
                            v[i] = r[i];
                    }

                    if (!res)
                        return false;

                    break;
                case TiffType.FLOAT:
                    float[] f = new float[dir.tdir_count];
                    res = fetchFloatArray(dir, f);
                    if (res)
                    {
                        for (i = dir.tdir_count - 1; i >= 0; i--)
                            v[i] = f[i];
                    }

                    if (!res)
                        return false;

                    break;
                case TiffType.DOUBLE:
                    return fetchDoubleArray(dir, v);
                default:
                    // NOTYPE
                    // ASCII
                    // UNDEFINED
                    ErrorExt(this, m_clientdata, m_name,
                        "cannot read TIFF_ANY type {0} for field \"{1}\"",
                        dir.tdir_type, FieldWithTag(dir.tdir_tag).Name);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Fetches a tag that is not handled by special case code.
        /// </summary>
        private bool fetchNormalTag(TiffDirEntry dir)
        {
            bool ok = false;
            TiffFieldInfo fip = FieldWithTag(dir.tdir_tag);

            if (dir.tdir_count > 1)
            {
                switch (dir.tdir_type)
                {
                    case TiffType.BYTE:
                    case TiffType.SBYTE:
                        byte[] bytes = new byte [dir.tdir_count];
                        ok = fetchByteArray(dir, bytes);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, bytes);
                            else
                                ok = SetField(dir.tdir_tag, bytes);
                        }
                        break;

                    case TiffType.SHORT:
                    case TiffType.SSHORT:
                        short[] shorts = new short [dir.tdir_count];
                        ok = fetchShortArray(dir, shorts);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, shorts);
                            else
                                ok = SetField(dir.tdir_tag, shorts);
                        }
                        break;

                    case TiffType.LONG:
                    case TiffType.SLONG:
                        int[] ints = new int [dir.tdir_count];
                        ok = fetchLongArray(dir, ints);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, ints);
                            else
                                ok = SetField(dir.tdir_tag, ints);
                        }
                        break;

                    case TiffType.RATIONAL:
                    case TiffType.SRATIONAL:
                        float[] rs = new float [dir.tdir_count];
                        ok = fetchRationalArray(dir, rs);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, rs);
                            else
                                ok = SetField(dir.tdir_tag, rs);
                        }
                        break;

                    case TiffType.FLOAT:
                        float[] fs = new float [dir.tdir_count];
                        ok = fetchFloatArray(dir, fs);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, fs);
                            else
                                ok = SetField(dir.tdir_tag, fs);
                        }
                        break;

                    case TiffType.DOUBLE:
                        double[] ds = new double [dir.tdir_count];
                        ok = fetchDoubleArray(dir, ds);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, ds);
                            else
                                ok = SetField(dir.tdir_tag, ds);
                        }
                        break;

                    case TiffType.ASCII:
                    case TiffType.UNDEFINED:
                        // bit of a cheat...

                        // Some vendors write strings w/o the trailing null
                        // byte, so always append one just in case.
                        string cp;
                        ok = fetchString(dir, out cp) != 0;
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, dir.tdir_count, cp);
                            else
                                ok = SetField(dir.tdir_tag, cp);
                        }
                        break;
                }        
            }
            else if (checkDirCount(dir, 1))
            {
                int v32 = 0;
                // singleton value
                switch (dir.tdir_type)
                {
                    case TiffType.BYTE:
                    case TiffType.SBYTE:
                    case TiffType.SHORT:
                    case TiffType.SSHORT:
                        // If the tag is also acceptable as a LONG or SLONG
                        // then SetField will expect an int parameter
                        // passed to it. 
                        //
                        // NB: We use FieldWithTag here knowing that
                        //     it returns us the first entry in the table
                        //     for the tag and that that entry is for the
                        //     widest potential data type the tag may have.
                        TiffType type = fip.Type;
                        if (type != TiffType.LONG && type != TiffType.SLONG)
                        {
                            short v = (short)extractData(dir);
                            if (fip.PassCount)
                            {
                                short[] a = new short[1];
                                a[0] = v;
                                ok = SetField(dir.tdir_tag, 1, a);
                            }
                            else
                                ok = SetField(dir.tdir_tag, v);

                            break;
                        }

                        v32 = extractData(dir);
                        if (fip.PassCount)
                        {
                            int[] a = new int[1];
                            a[0] = (int)v32;
                            ok = SetField(dir.tdir_tag, 1, a);
                        }
                        else
                            ok = SetField(dir.tdir_tag, v32);

                        break;

                    case TiffType.LONG:
                    case TiffType.SLONG:
                        v32 = extractData(dir);
                        if (fip.PassCount)
                        {
                            int[] a = new int[1];
                            a[0] = (int)v32;
                            ok = SetField(dir.tdir_tag, 1, a);
                        }
                        else
                            ok = SetField(dir.tdir_tag, v32);

                        break;

                    case TiffType.RATIONAL:
                    case TiffType.SRATIONAL:
                    case TiffType.FLOAT:
                        float f = (dir.tdir_type == TiffType.FLOAT ? fetchFloat(dir): fetchRational(dir));
                        if (fip.PassCount)
                        {
                            float[] a = new float[1];
                            a[0] = f;
                            ok = SetField(dir.tdir_tag, 1, a);
                        }
                        else
                            ok = SetField(dir.tdir_tag, f);

                        break;

                    case TiffType.DOUBLE:
                        double[] ds = new double[1];
                        ok = fetchDoubleArray(dir, ds);
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, 1, ds);
                            else
                                ok = SetField(dir.tdir_tag, ds[0]);
                        }
                        break;

                    case TiffType.ASCII:
                    case TiffType.UNDEFINED:
                        // bit of a cheat...
                        string c;
                        ok = fetchString(dir, out c) != 0;
                        if (ok)
                        {
                            if (fip.PassCount)
                                ok = SetField(dir.tdir_tag, 1, c);
                            else
                                ok = SetField(dir.tdir_tag, c);
                        }
                        break;
                }
            }

            return ok;
        }

        /// <summary>
        /// Fetches samples/pixel short values for the specified tag and verify
        /// that all values are the same.
        /// </summary>
        private bool fetchPerSampleShorts(TiffDirEntry dir, out short pl)
        {
            pl = 0;
            short samples = m_dir.td_samplesperpixel;
            bool status = false;

            if (checkDirCount(dir, samples))
            {
                short[] v = new short [dir.tdir_count];
                if (fetchShortArray(dir, v))
                {
                    int check_count = dir.tdir_count;
                    if (samples < check_count)
                        check_count = samples;

                    bool failed = false;
                    for (ushort i = 1; i < check_count; i++)
                    {
                        if (v[i] != v[0])
                        {
                            ErrorExt(this, m_clientdata, m_name,
                                "Cannot handle different per-sample values for field \"{0}\"",
                                FieldWithTag(dir.tdir_tag).Name);
                            failed = true;
                            break;
                        }
                    }

                    if (!failed)
                    {
                        pl = v[0];
                        status = true;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Fetches samples/pixel long values for the specified tag and verify
        /// that all values are the same.
        /// </summary>
        private bool fetchPerSampleLongs(TiffDirEntry dir, out int pl)
        {
            pl = 0;
            short samples = m_dir.td_samplesperpixel;
            bool status = false;

            if (checkDirCount(dir, samples))
            {
                int[] v = new int [dir.tdir_count];
                if (fetchLongArray(dir, v))
                {
                    int check_count = dir.tdir_count;
                    if (samples < check_count)
                        check_count = samples;

                    bool failed = false;
                    for (ushort i = 1; i < check_count; i++)
                    {
                        if (v[i] != v[0])
                        {
                            ErrorExt(this, m_clientdata, m_name,
                                "Cannot handle different per-sample values for field \"{0}\"",
                                FieldWithTag(dir.tdir_tag).Name);
                            failed = true;
                            break;
                        }
                    }

                    if (!failed)
                    {
                        pl = (int)v[0];
                        status = true;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Fetches samples/pixel ANY values for the specified tag and verify
        /// that all values are the same.
        /// </summary>
        private bool fetchPerSampleAnys(TiffDirEntry dir, out double pl)
        {
            pl = 0;
            short samples = m_dir.td_samplesperpixel;
            bool status = false;

            if (checkDirCount(dir, samples))
            {
                double[] v = new double [dir.tdir_count];
                if (fetchAnyArray(dir, v))
                {
                    int check_count = dir.tdir_count;
                    if (samples < check_count)
                        check_count = samples;

                    bool failed = false;
                    for (ushort i = 1; i < check_count; i++)
                    {
                        if (v[i] != v[0])
                        {
                            ErrorExt(this, m_clientdata, m_name,
                                "Cannot handle different per-sample values for field \"{0}\"",
                                FieldWithTag(dir.tdir_tag).Name);
                            failed = true;
                            break;
                        }
                    }

                    if (!failed)
                    {
                        pl = v[0];
                        status = true;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Fetches a set of offsets or lengths.
        /// </summary>
        /// <remarks>While this routine says "strips", in fact it's also used
        /// for tiles.</remarks>
        private bool fetchStripThing(TiffDirEntry dir, int nstrips, ref int[] lpp)
        {
            checkDirCount(dir, nstrips);

            // Allocate space for strip information.
            if (lpp == null)
                lpp = new int [nstrips];
            else
                Array.Clear(lpp, 0, lpp.Length);

            bool status = false;
            if (dir.tdir_type == TiffType.SHORT)
            {
                // Handle short -> int expansion.
                short[] dp = new short[dir.tdir_count];
                status = fetchShortArray(dir, dp);
                if (status)
                {
                    for (int i = 0; i < nstrips && i < dir.tdir_count; i++)
                        lpp[i] = dp[i];
                }
            }
            else if (nstrips != dir.tdir_count)
            {
                // Special case to correct length
                int[] dp = new int[dir.tdir_count];
                status = fetchLongArray(dir, dp);
                if (status)
                {
                    for (int i = 0; i < nstrips && i < dir.tdir_count; i++)
                        lpp[i] = dp[i];
                }
            }
            else
            {
                status = fetchLongArray(dir, lpp);
            }

            return status;
        }

        private bool fetchStripThing(TiffDirEntry dir, int nstrips, ref uint[] lpp)
        {
            int[] temp = null;
            if (lpp != null)
                temp = new int[lpp.Length];

            bool res = fetchStripThing(dir, nstrips, ref temp);
            if (res)
            {
                if (lpp == null)
                    lpp = new uint[temp.Length];

                Buffer.BlockCopy(temp, 0, lpp, 0, temp.Length * sizeof(uint));
            }

            return res;
        }

        /// <summary>
        /// Fetches and sets the RefBlackWhite tag.
        /// </summary>
        private bool fetchRefBlackWhite(TiffDirEntry dir)
        {
            if (dir.tdir_type == TiffType.RATIONAL)
                return fetchNormalTag(dir);
            
            // Handle LONG's for backward compatibility.
            int[] cp = new int [dir.tdir_count];
            bool ok = fetchLongArray(dir, cp);
            if (ok)
            {
                float[] fp = new float [dir.tdir_count];
                for (int i = 0; i < dir.tdir_count; i++)
                    fp[i] = (float)cp[i];

                ok = SetField(dir.tdir_tag, fp);
            }

            return ok;
        }

        /// <summary>
        /// Replace a single strip (tile) of uncompressed data with multiple
        /// strips (tiles), each approximately 8Kbytes.
        /// </summary>
        /// <remarks>This is useful for dealing with large images or for
        /// dealing with machines with a limited amount of memory.</remarks>
        private void chopUpSingleUncompressedStrip()
        {
            uint bytecount = m_dir.td_stripbytecount[0];
            uint offset = m_dir.td_stripoffset[0];

            // Make the rows hold at least one scanline, but fill specified
            // amount of data if possible.
            int rowbytes = VTileSize(1);
            uint stripbytes;
            int rowsperstrip;
            if (rowbytes > STRIP_SIZE_DEFAULT)
            {
                stripbytes = (uint)rowbytes;
                rowsperstrip = 1;
            }
            else if (rowbytes > 0)
            {
                rowsperstrip = STRIP_SIZE_DEFAULT / rowbytes;
                stripbytes = (uint)(rowbytes * rowsperstrip);
            }
            else
            {
                return;
            }

            // never increase the number of strips in an image
            if (rowsperstrip >= m_dir.td_rowsperstrip)
                return;
            
            uint nstrips = howMany(bytecount, stripbytes);
            if (nstrips == 0)
            {
                // something is wonky, do nothing.
                return;
            }

            uint[] newcounts = new uint [nstrips];
            uint[] newoffsets = new uint [nstrips];

            // Fill the strip information arrays with new bytecounts and offsets
            // that reflect the broken-up format.
            for (int strip = 0; strip < nstrips; strip++)
            {
                if (stripbytes > bytecount)
                    stripbytes = bytecount;

                newcounts[strip] = stripbytes;
                newoffsets[strip] = offset;
                offset += stripbytes;
                bytecount -= stripbytes;
            }

            // Replace old single strip info with multi-strip info.
            m_dir.td_nstrips = (int)nstrips;
            m_dir.td_stripsperimage = (int)nstrips;
            SetField(TiffTag.ROWSPERSTRIP, rowsperstrip);

            m_dir.td_stripbytecount = newcounts;
            m_dir.td_stripoffset = newoffsets;
            m_dir.td_stripbytecountsorted = true;
        }

        internal static int roundUp(int x, int y)
        {
            return (howMany(x, y) * y);
        }

        internal static int howMany(int x, int y)
        {
            long res = (((long)x + ((long)y - 1)) / (long)y);
            if (res > int.MaxValue)
                return 0;

            return (int)res;
        }

        internal static uint howMany(uint x, uint y)
        {
            long res = (((long)x + ((long)y - 1)) / (long)y);
            if (res > uint.MaxValue)
                return 0;

            return (uint)res;
        }
    }
}
