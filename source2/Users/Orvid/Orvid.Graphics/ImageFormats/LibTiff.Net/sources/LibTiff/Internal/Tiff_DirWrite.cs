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
 * Directory Write Support Routines.
 */

using System;
using System.IO;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private uint insertData(TiffType type, int v)
        {
            int t = (int)type;
            if (m_header.tiff_magic == TIFF_BIGENDIAN)
                return (((uint)v & m_typemask[t]) << m_typeshift[t]);
            
            return ((uint)v & m_typemask[t]);
        }

        private static void resetFieldBit(int[] fields, short f)
        {
            fields[f / 32] &= ~BITn(f);
        }

        private static bool fieldSet(int[] fields, short f)
        {
            return ((fields[f / 32] & BITn(f)) != 0);
        }

        private bool writeRational(TiffType type, TiffTag tag, ref TiffDirEntry dir, float v)
        {
            dir.tdir_tag = tag;
            dir.tdir_type = type;
            dir.tdir_count = 1;

            float[] a = new float[1];
            a[0] = v;
            if (!writeRationalArray(ref dir, a))
                return false;

            return true;
        }

        private bool writeRationalPair(TiffDirEntry[] entries, int dirOffset, TiffType type, TiffTag tag1, float v1, TiffTag tag2, float v2)
        {
            if (!writeRational(type, tag1, ref entries[dirOffset], v1))
                return false;

            if (!writeRational(type, tag2, ref entries[dirOffset + 1], v2))
                return false;

            return true;
        }

        /// <summary>
        /// Writes the contents of the current directory to the specified file.
        /// </summary>
        /// <remarks>This routine doesn't handle overwriting a directory with
        /// auxiliary storage that's been changed.</remarks>
        private bool writeDirectory(bool done)
        {
            if (m_mode == O_RDONLY)
                return true;

            // Clear write state so that subsequent images with different
            // characteristics get the right buffers setup for them.
            if (done)
            {
                if ((m_flags & TiffFlags.POSTENCODE) == TiffFlags.POSTENCODE)
                {
                    m_flags &= ~TiffFlags.POSTENCODE;
                    if (!m_currentCodec.PostEncode())
                    {
                        ErrorExt(this, m_clientdata, m_name, "Error post-encoding before directory write");
                        return false;
                    }
                }

                // shutdown encoder
                m_currentCodec.Close();
                
                // Flush any data that might have been written by the
                // compression close+cleanup routines.
                if (m_rawcc > 0 && (m_flags & TiffFlags.BEENWRITING) == TiffFlags.BEENWRITING && !flushData1())
                {
                    ErrorExt(this, m_clientdata, m_name, "Error flushing data before directory write");
                    return false;
                }

                if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER && m_rawdata != null)
                {
                    m_rawdata = null;
                    m_rawcc = 0;
                    m_rawdatasize = 0;
                }

                m_flags &= ~(TiffFlags.BEENWRITING | TiffFlags.BUFFERSETUP);
            }

            // Size the directory so that we can calculate offsets for the data
            // items that aren't kept in-place in each field.
            int nfields = 0;
            for (int b = 0; b <= FieldBit.Last; b++)
            {
                if (fieldSet(b) && b != FieldBit.Custom)
                    nfields += (b < FieldBit.SubFileType ? 2 : 1);
            }

            nfields += m_dir.td_customValueCount;
            int dirsize = nfields * TiffDirEntry.SizeInBytes;
            TiffDirEntry[] data = new TiffDirEntry [nfields];
            for (int i = 0; i < nfields; i++)
                data[i] = new TiffDirEntry();

            // Directory hasn't been placed yet, put it at the end of the file
            // and link it into the existing directory structure.
            if (m_diroff == 0 && !linkDirectory())
                return false;

            m_dataoff = m_diroff + sizeof(short) + (uint)dirsize + sizeof(int);
            if ((m_dataoff & 1) != 0)
                m_dataoff++;

            seekFile(m_dataoff, SeekOrigin.Begin);
            m_curdir++;
            int dir = 0;

            // Setup external form of directory entries and write data items.
            int[] fields = new int[FieldBit.SetLongs];
            Buffer.BlockCopy(m_dir.td_fieldsset, 0, fields, 0, FieldBit.SetLongs * sizeof(int));

            // Write out ExtraSamples tag only if extra samples are present in the data.
            if (fieldSet(fields, FieldBit.ExtraSamples) && m_dir.td_extrasamples == 0)
            {
                resetFieldBit(fields, FieldBit.ExtraSamples);
                nfields--;
                dirsize -= TiffDirEntry.SizeInBytes;
            } // XXX

            for (int fi = 0, nfi = m_nfields; nfi > 0; nfi--, fi++)
            {
                TiffFieldInfo fip = m_fieldinfo[fi];

                // For custom fields, we test to see if the custom field is set
                // or not.  For normal fields, we just use the fieldSet test. 
                if (fip.Bit == FieldBit.Custom)
                {
                    bool is_set = false;
                    for (int ci = 0; ci < m_dir.td_customValueCount; ci++)
                        is_set |= (m_dir.td_customValues[ci].info == fip);

                    if (!is_set)
                        continue;
                }
                else if (!fieldSet(fields, fip.Bit))
                {
                    continue;
                }

                // Handle other fields.

                TiffTag tag = (TiffTag)FieldBit.Ignore;
                switch (fip.Bit)
                {
                    case FieldBit.StripOffsets:
                        // We use one field bit for both strip and tile 
                        // offsets, and so must be careful in selecting
                        // the appropriate field descriptor (so that tags
                        // are written in sorted order).
                        tag = IsTiled() ? TiffTag.TILEOFFSETS : TiffTag.STRIPOFFSETS;
                        if (tag != fip.Tag)
                            continue;

                        data[dir].tdir_tag = tag;
                        data[dir].tdir_type = TiffType.LONG;
                        data[dir].tdir_count = m_dir.td_nstrips;
                        if (!writeLongArray(ref data[dir], m_dir.td_stripoffset))
                            return false;

                        break;
                    case FieldBit.StripByteCounts:
                        // We use one field bit for both strip and tile byte
                        // counts, and so must be careful in selecting the
                        // appropriate field descriptor (so that tags are
                        // written in sorted order).
                        tag = IsTiled() ? TiffTag.TILEBYTECOUNTS: TiffTag.STRIPBYTECOUNTS;
                        if (tag != fip.Tag)
                            continue;

                        data[dir].tdir_tag = tag;
                        data[dir].tdir_type = TiffType.LONG;
                        data[dir].tdir_count = m_dir.td_nstrips;
                        if (!writeLongArray(ref data[dir], m_dir.td_stripbytecount))
                            return false;

                        break;
                    case FieldBit.RowsPerStrip:
                        setupShortLong(TiffTag.ROWSPERSTRIP, ref data[dir], m_dir.td_rowsperstrip);
                        break;
                    case FieldBit.ColorMap:
                        if (!writeShortTable(TiffTag.COLORMAP, ref data[dir], 3, m_dir.td_colormap))
                            return false;

                        break;
                    case FieldBit.ImageDimensions:
                        setupShortLong(TiffTag.IMAGEWIDTH, ref data[dir++], m_dir.td_imagewidth);
                        setupShortLong(TiffTag.IMAGELENGTH, ref data[dir], m_dir.td_imagelength);
                        break;
                    case FieldBit.TileDimensions:
                        setupShortLong(TiffTag.TILEWIDTH, ref data[dir++], m_dir.td_tilewidth);
                        setupShortLong(TiffTag.TILELENGTH, ref data[dir], m_dir.td_tilelength);
                        break;
                    case FieldBit.Compression:
                        setupShort(TiffTag.COMPRESSION, ref data[dir], (short)m_dir.td_compression);
                        break;
                    case FieldBit.Photometric:
                        setupShort(TiffTag.PHOTOMETRIC, ref data[dir], (short)m_dir.td_photometric);
                        break;
                    case FieldBit.Position:
                        if (!writeRationalPair(data, dir, TiffType.RATIONAL, TiffTag.XPOSITION, m_dir.td_xposition, TiffTag.YPOSITION, m_dir.td_yposition))
                            return false;

                        dir++;
                        break;
                    case FieldBit.Resolution:
                        if (!writeRationalPair(data, dir, TiffType.RATIONAL, TiffTag.XRESOLUTION, m_dir.td_xresolution, TiffTag.YRESOLUTION, m_dir.td_yresolution))
                            return false;

                        dir++;
                        break;
                    case FieldBit.BitsPerSample:
                    case FieldBit.MinSampleValue:
                    case FieldBit.MaxSampleValue:
                    case FieldBit.SampleFormat:
                        if (!writePerSampleShorts(fip.Tag, ref data[dir]))
                            return false;

                        break;
                    case FieldBit.SMinSampleValue:
                    case FieldBit.SMaxSampleValue:
                        if (!writePerSampleAnys(sampleToTagType(), fip.Tag, ref data[dir]))
                            return false;

                        break;
                    case FieldBit.PageNumber:
                    case FieldBit.HalftoneHints:
                    case FieldBit.YCbCrSubsampling:
                        if (!setupShortPair(fip.Tag, ref data[dir]))
                            return false;

                        break;
                    case FieldBit.InkNames:
                        if (!writeInkNames(ref data[dir]))
                            return false;

                        break;
                    case FieldBit.TransferFunction:
                        if (!writeTransferFunction(ref data[dir]))
                            return false;

                        break;
                    case FieldBit.SubIFD:
                        // XXX: Always write this field using LONG type
                        // for backward compatibility.
                        data[dir].tdir_tag = fip.Tag;
                        data[dir].tdir_type = TiffType.LONG;
                        data[dir].tdir_count = m_dir.td_nsubifd;
                        if (!writeLongArray(ref data[dir], m_dir.td_subifd))
                            return false;

                        // Total hack: if this directory includes a SubIFD
                        // tag then force the next <n> directories to be
                        // written as "sub directories" of this one.  This
                        // is used to write things like thumbnails and
                        // image masks that one wants to keep out of the
                        // normal directory linkage access mechanism.
                        if (data[dir].tdir_count > 0)
                        {
                            m_flags |= TiffFlags.INSUBIFD;
                            m_nsubifd = (short)data[dir].tdir_count;
                            if (data[dir].tdir_count > 1)
                            {
                                m_subifdoff = data[dir].tdir_offset;
                            }
                            else
                            {
                                m_subifdoff = m_diroff + sizeof(short) +
                                    (uint)dir * TiffDirEntry.SizeInBytes +
                                    sizeof(short) * 2 + sizeof(int);
                            }
                        }
                        break;
                    default:
                        // XXX: Should be fixed and removed.
                        if (fip.Tag == TiffTag.DOTRANGE)
                        {
                            if (!setupShortPair(fip.Tag, ref data[dir]))
                                return false;
                        }
                        else if (!writeNormalTag(ref data[dir], fip))
                            return false;

                        break;
                }

                dir++;

                if (fip.Bit != FieldBit.Custom)
                    resetFieldBit(fields, fip.Bit);
            }

            // Write directory.

            short dircount = (short)nfields;
            uint diroff = m_nextdiroff;
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
            {
                // The file's byte order is opposite to the native machine
                // architecture. We overwrite the directory information with
                // impunity because it'll be released below after we write it to
                // the file. Note that all the other tag construction routines
                // assume that we do this byte-swapping; i.e. they only
                // byte-swap indirect data.
                for (dir = 0; dircount != 0; dir++, dircount--)
                {
                    short temp = (short)data[dir].tdir_tag;
                    SwabShort(ref temp);
                    data[dir].tdir_tag = (TiffTag)(ushort)temp;

                    temp = (short)data[dir].tdir_type;
                    SwabShort(ref temp);
                    data[dir].tdir_type = (TiffType)temp;

                    SwabLong(ref data[dir].tdir_count);
                    SwabUInt(ref data[dir].tdir_offset);
                }

                dircount = (short)nfields;
                SwabShort(ref dircount);
                SwabUInt(ref diroff);
            }

            seekFile(m_diroff, SeekOrigin.Begin);
            if (!writeShortOK(dircount))
            {
                ErrorExt(this, m_clientdata, m_name, "Error writing directory count");
                return false;
            }
            
            if (!writeDirEntryOK(data, dirsize / TiffDirEntry.SizeInBytes))
            {
                ErrorExt(this, m_clientdata, m_name, "Error writing directory contents");
                return false;
            }
            
            if (!writeIntOK((int)diroff))
            {
                ErrorExt(this, m_clientdata, m_name, "Error writing directory link");
                return false;
            }
            
            if (done)
            {
                FreeDirectory();
                m_flags &= ~TiffFlags.DIRTYDIRECT;
                m_currentCodec.Cleanup();

                // Reset directory-related state for subsequent directories.
                CreateDirectory();
            }

            return true;
        }

        /// <summary>
        /// Writes tags that are not special cased.
        /// </summary>
        private bool writeNormalTag(ref TiffDirEntry dir, TiffFieldInfo fip)
        {
            short wc = fip.WriteCount;
            dir.tdir_tag = fip.Tag;
            dir.tdir_type = fip.Type;
            dir.tdir_count = wc;

            switch (fip.Type)
            {
                case TiffType.SHORT:
                case TiffType.SSHORT:
                    if (fip.PassCount)
                    {
                        short[] wp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            wp = result[1].ToShortArray();

                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            wp = result[1].ToShortArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeShortArray(ref dir, wp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            short sv = result[0].ToShort();
                            dir.tdir_offset = insertData(dir.tdir_type, sv);
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            short[] wp = result[0].ToShortArray();
                            if (!writeShortArray(ref dir, wp))
                                return false;
                        }
                    }
                    break;
                case TiffType.LONG:
                case TiffType.SLONG:
                case TiffType.IFD:
                    if (fip.PassCount)
                    {
                        int[] lp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            lp = result[1].ToIntArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            lp = result[1].ToIntArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeLongArray(ref dir, lp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            // XXX handle LONG->SHORT conversion
                            FieldValue[] result = GetField(fip.Tag);
                            dir.tdir_offset = result[0].ToUInt();
                        }
                        else
                        {
                            int[] lp;
                            FieldValue[] result = GetField(fip.Tag);
                            lp = result[0].ToIntArray();
                            if (!writeLongArray(ref dir, lp))
                                return false;
                        }
                    }
                    break;
                case TiffType.RATIONAL:
                case TiffType.SRATIONAL:
                    if (fip.PassCount)
                    {
                        float[] fp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            fp = result[1].ToFloatArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            fp = result[1].ToFloatArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeRationalArray(ref dir, fp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            float[] fv = new float[1];
                            FieldValue[] result = GetField(fip.Tag);
                            fv[0] = result[0].ToFloat();
                            if (!writeRationalArray(ref dir, fv))
                                return false;
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            float[] fp = result[0].ToFloatArray();
                            if (!writeRationalArray(ref dir, fp))
                                return false;
                        }
                    }
                    break;
                case TiffType.FLOAT:
                    if (fip.PassCount)
                    {
                        float[] fp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            fp = result[1].ToFloatArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            fp = result[1].ToFloatArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeFloatArray(ref dir, fp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            float[] fv = new float[1];
                            FieldValue[] result = GetField(fip.Tag);
                            fv[0] = result[0].ToFloat();
                            if (!writeFloatArray(ref dir, fv))
                                return false;
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            float[] fp = result[0].ToFloatArray();
                            if (!writeFloatArray(ref dir, fp))
                                return false;
                        }
                    }
                    break;
                case TiffType.DOUBLE:
                    if (fip.PassCount)
                    {
                        double[] dp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            dp = result[1].ToDoubleArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            dp = result[1].ToDoubleArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeDoubleArray(ref dir, dp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            double[] dv = new double[1];
                            FieldValue[] result = GetField(fip.Tag);
                            dv[0] = result[0].ToDouble();
                            if (!writeDoubleArray(ref dir, dv))
                                return false;
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            double[] dp = result[0].ToDoubleArray();
                            if (!writeDoubleArray(ref dir, dp))
                                return false;
                        }
                    }
                    break;
                case TiffType.ASCII:
                    {
                        FieldValue[] result = GetField(fip.Tag);

                        string cp;
                        if (fip.PassCount)
                            cp = result[1].ToString();
                        else
                            cp = result[0].ToString();

                        // add zero ('\0') at the end of the byte array
                        byte[] stringBytes = Latin1Encoding.GetBytes(cp);
                        byte[] totalBytes = new byte[stringBytes.Length + 1];
                        Buffer.BlockCopy(stringBytes, 0, totalBytes, 0, stringBytes.Length);

                        dir.tdir_count = totalBytes.Length;
                        if (!writeByteArray(ref dir, totalBytes))
                            return false;
                    }
                    break;

                case TiffType.BYTE:
                case TiffType.SBYTE:
                    if (fip.PassCount)
                    {
                        byte[] cp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            cp = result[1].ToByteArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            // Assume TiffFieldInfo.Variable
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            cp = result[1].ToByteArray();
                            dir.tdir_count = wc;
                        }

                        if (!writeByteArray(ref dir, cp))
                            return false;
                    }
                    else
                    {
                        if (wc == 1)
                        {
                            byte[] cv = new byte[1];
                            FieldValue[] result = GetField(fip.Tag);
                            cv[0] = result[0].ToByte();
                            if (!writeByteArray(ref dir, cv))
                                return false;
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            byte[] cp = result[0].ToByteArray();
                            if (!writeByteArray(ref dir, cp))
                                return false;
                        }
                    }
                    break;

                case TiffType.UNDEFINED:
                    {
                        byte[] cp;
                        int wc2;
                        if (wc == TiffFieldInfo.Variable)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc = result[0].ToShort();
                            cp = result[1].ToByteArray();
                            dir.tdir_count = wc;
                        }
                        else if (wc == TiffFieldInfo.Variable2)
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            wc2 = result[0].ToInt();
                            cp = result[1].ToByteArray();
                            dir.tdir_count = wc2;
                        }
                        else
                        {
                            FieldValue[] result = GetField(fip.Tag);
                            cp = result[0].ToByteArray();
                        }

                        if (!writeByteArray(ref dir, cp))
                            return false;
                    }
                    break;

                case TiffType.NOTYPE:
                    break;
            }

            return true;
        }

        /// <summary>
        /// Setups a directory entry with either a SHORT or LONG type
        /// according to the value.
        /// </summary>
        private void setupShortLong(TiffTag tag, ref TiffDirEntry dir, int v)
        {
            dir.tdir_tag = tag;
            dir.tdir_count = 1;
            if (v > 0xffffL)
            {
                dir.tdir_type = TiffType.LONG;
                dir.tdir_offset = (uint)v;
            }
            else
            {
                dir.tdir_type = TiffType.SHORT;
                dir.tdir_offset = insertData(TiffType.SHORT, v);
            }
        }

        /// <summary>
        /// Setups a SHORT directory entry
        /// </summary>
        private void setupShort(TiffTag tag, ref TiffDirEntry dir, short v)
        {
            dir.tdir_tag = tag;
            dir.tdir_count = 1;
            dir.tdir_type = TiffType.SHORT;
            dir.tdir_offset = insertData(TiffType.SHORT, v);
        }

        /*
        * Setup a directory entry that references a
        * samples/pixel array of SHORT values and
        * (potentially) write the associated indirect
        * values.
        */
        private bool writePerSampleShorts(TiffTag tag, ref TiffDirEntry dir)
        {
            short[] w = new short [m_dir.td_samplesperpixel];

            FieldValue[] result = GetField(tag);
            short v = result[0].ToShort();

            for (short i = 0; i < m_dir.td_samplesperpixel; i++)
                w[i] = v;

            dir.tdir_tag = tag;
            dir.tdir_type = TiffType.SHORT;
            dir.tdir_count = m_dir.td_samplesperpixel;
            bool status = writeShortArray(ref dir, w);
            return status;
        }

        /*
        * Setup a directory entry that references a samples/pixel array of ``type''
        * values and (potentially) write the associated indirect values.  The source
        * data from GetField() for the specified tag must be returned as double.
        */
        private bool writePerSampleAnys(TiffType type, TiffTag tag, ref TiffDirEntry dir)
        {
            double[] w = new double [m_dir.td_samplesperpixel];

            FieldValue[] result = GetField(tag);
            double v = result[0].ToDouble();

            for (short i = 0; i < m_dir.td_samplesperpixel; i++)
                w[i] = v;
            
            bool status = writeAnyArray(type, tag, ref dir, m_dir.td_samplesperpixel, w);
            return status;
        }

        /*
        * Setup a pair of shorts that are returned by
        * value, rather than as a reference to an array.
        */
        private bool setupShortPair(TiffTag tag, ref TiffDirEntry dir)
        {
            short[] v = new short[2];
            FieldValue[] result = GetField(tag);
            v[0] = result[0].ToShort();
            v[1] = result[1].ToShort();

            dir.tdir_tag = tag;
            dir.tdir_type = TiffType.SHORT;
            dir.tdir_count = 2;
            return writeShortArray(ref dir, v);
        }

        /// <summary>
        /// Setup a directory entry for an NxM table of shorts, where M is
        /// known to be 2**bitspersample, and write the associated indirect data.
        /// </summary>
        private bool writeShortTable(TiffTag tag, ref TiffDirEntry dir, int n, short[][] table)
        {
            dir.tdir_tag = tag;
            dir.tdir_type = TiffType.SHORT;

            // XXX -- yech, fool writeData
            dir.tdir_count = 1 << m_dir.td_bitspersample;
            uint off = m_dataoff;
            for (int i = 0; i < n; i++)
            {
                if (!writeData(ref dir, table[i], dir.tdir_count))
                    return false;
            }

            dir.tdir_count *= n;
            dir.tdir_offset = off;
            return true;
        }

        /// <summary>
        /// Write/copy data associated with an ASCII or opaque tag value.
        /// </summary>
        private bool writeByteArray(ref TiffDirEntry dir, byte[] cp)
        {
            if (dir.tdir_count <= 4)
            {
                if (m_header.tiff_magic == TIFF_BIGENDIAN)
                {
                    dir.tdir_offset = (uint)(cp[0] << 24);
                    if (dir.tdir_count >= 2)
                        dir.tdir_offset |= (uint)(cp[1] << 16);

                    if (dir.tdir_count >= 3)
                        dir.tdir_offset |= (uint)(cp[2] << 8);

                    if (dir.tdir_count == 4)
                        dir.tdir_offset |= cp[3];
                }
                else
                {
                    dir.tdir_offset = cp[0];
                    if (dir.tdir_count >= 2)
                        dir.tdir_offset |= (uint)(cp[1] << 8);

                    if (dir.tdir_count >= 3)
                        dir.tdir_offset |= (uint)(cp[2] << 16);

                    if (dir.tdir_count == 4)
                        dir.tdir_offset |= (uint)(cp[3] << 24);
                }

                return true;
            }

            return writeData(ref dir, cp, dir.tdir_count);
        }

        /// <summary>
        /// Setup a directory entry of an array of SHORT or SSHORT and write
        /// the associated indirect values.
        /// </summary>
        private bool writeShortArray(ref TiffDirEntry dir, short[] v)
        {
            if (dir.tdir_count <= 2)
            {
                if (m_header.tiff_magic == TIFF_BIGENDIAN)
                {
                    dir.tdir_offset = (uint)(v[0] << 16);
                    if (dir.tdir_count == 2)
                        dir.tdir_offset |= (uint)(v[1] & 0xffff);
                }
                else
                {
                    dir.tdir_offset = (uint)(v[0] & 0xffff);
                    if (dir.tdir_count == 2)
                        dir.tdir_offset |= (uint)(v[1] << 16);
                }

                return true;
            }

            return writeData(ref dir, v, dir.tdir_count);
        }

        /// <summary>
        /// Setup a directory entry of an array of LONG or SLONG and write the
        /// associated indirect values.
        /// </summary>
        private bool writeLongArray(ref TiffDirEntry dir, int[] v)
        {
            if (dir.tdir_count == 1)
            {
                dir.tdir_offset = (uint)v[0];
                return true;
            }

            return writeData(ref dir, v, dir.tdir_count);
        }

        private bool writeLongArray(ref TiffDirEntry dir, uint[] v)
        {
            int[] temp = new int[v.Length];
            Buffer.BlockCopy(v, 0, temp, 0, v.Length * sizeof(uint));
            return writeLongArray(ref dir, temp);
        }

        /// <summary>
        /// Setup a directory entry of an array of RATIONAL or SRATIONAL and
        /// write the associated indirect values.
        /// </summary>
        private bool writeRationalArray(ref TiffDirEntry dir, float[] v)
        {
            int[] t = new int [2 * dir.tdir_count];
            for (int i = 0; i < dir.tdir_count; i++)
            {
                int sign = 1;
                float fv = v[i];
                if (fv < 0)
                {
                    if (dir.tdir_type == TiffType.RATIONAL)
                    {
                        WarningExt(this, m_clientdata, m_name,
                            "\"{0}\": Information lost writing value ({1:G}) as (unsigned) RATIONAL",
                            FieldWithTag(dir.tdir_tag).Name, fv);
                        fv = 0;
                    }
                    else
                    {
                        fv = -fv;
                        sign = -1;
                    }
                }
            
                int den = 1;
                if (fv > 0)
                {
                    while (fv < (1L << (31 - 3)) && den < (1L << (31 - 3)))
                    {
                        fv *= 1 << 3;
                        den *= 1 << 3;
                    }
                }
                
                t[2 * i + 0] = (int)(sign * (fv + 0.5));
                t[2 * i + 1] = den;
            }

            return writeData(ref dir, t, 2 * dir.tdir_count);
        }

        private bool writeFloatArray(ref TiffDirEntry dir, float[] v)
        {
            if (dir.tdir_count == 1)
            {
                dir.tdir_offset = BitConverter.ToUInt32(BitConverter.GetBytes(v[0]), 0);
                return true;
            }

            return writeData(ref dir, v, dir.tdir_count);
        }

        private bool writeDoubleArray(ref TiffDirEntry dir, double[] v)
        {
            return writeData(ref dir, v, dir.tdir_count);
        }

        /// <summary>
        /// Writes an array of "type" values for a specified tag (i.e. this is
        /// a tag which is allowed to have different types, e.g. SMaxSampleType).
        /// Internally the data values are represented as double since a double
        /// can hold any of the TIFF tag types (yes, this should really be an abstract
        /// type tany_t for portability).  The data is converted into the specified
        /// type in a temporary buffer and then handed off to the appropriate array
        /// writer.
        /// </summary>
        private bool writeAnyArray(TiffType type, TiffTag tag, ref TiffDirEntry dir, int n, double[] v)
        {
            dir.tdir_tag = tag;
            dir.tdir_type = type;
            dir.tdir_count = n;

            bool failed = false;
            switch (type)
            {
                case TiffType.BYTE:
                case TiffType.SBYTE:
                    {
                        byte[] bp = new byte [n];
                        for (int i = 0; i < n; i++)
                            bp[i] = (byte)v[i];

                        if (!writeByteArray(ref dir, bp))
                            failed = true;
                    }
                    break;
                case TiffType.SHORT:
                case TiffType.SSHORT:
                    {
                        short[] bp = new short [n];
                        for (int i = 0; i < n; i++)
                            bp[i] = (short)v[i];
                        
                        if (!writeShortArray(ref dir, bp))
                            failed = true;
                    }
                    break;
                case TiffType.LONG:
                case TiffType.SLONG:
                    {
                        int[] bp = new int [n];
                        for (int i = 0; i < n; i++)
                            bp[i] = (int)v[i];
                        
                        if (!writeLongArray(ref dir, bp))
                            failed = true;
                    }
                    break;
                case TiffType.FLOAT:
                    {
                        float[] bp = new float [n];
                        for (int i = 0; i < n; i++)
                            bp[i] = (float)v[i];
                        
                        if (!writeFloatArray(ref dir, bp))
                            failed = true;
                    }
                    break;
                case TiffType.DOUBLE:
                    if (!writeDoubleArray(ref dir, v))
                        failed = true;
                        
                    break;

                default:
                    // NOTYPE
                    // ASCII
                    // UNDEFINED
                    // RATIONAL
                    // SRATIONAL
                    failed = true;
                    break;
            }

            return !failed;
        }

        private bool writeTransferFunction(ref TiffDirEntry dir)
        {
            // Check if the table can be written as a single column, or if it
            // must be written as 3 columns. Note that we write a 3-column tag
            // if there are 2 samples/pixel and a single column of data
            // won't suffice--hmm.
            int u = m_dir.td_samplesperpixel - m_dir.td_extrasamples;
            int ncols = 1;
            bool reCheck = false;
            int n = 1 << m_dir.td_bitspersample;

            if (u < 0 || u > 2)
            {
                if (Compare(m_dir.td_transferfunction[0], m_dir.td_transferfunction[2], n) != 0)
                    ncols = 3;
                else
                    reCheck = true;
            }

            if (u == 2 || reCheck)
            {
                if (Compare(m_dir.td_transferfunction[0], m_dir.td_transferfunction[1], n) != 0)
                    ncols = 3;
            }
            
            return writeShortTable(TiffTag.TRANSFERFUNCTION, ref dir, ncols, m_dir.td_transferfunction);
        }

        private bool writeInkNames(ref TiffDirEntry dir)
        {
            dir.tdir_tag = TiffTag.INKNAMES;
            dir.tdir_type = TiffType.ASCII;
            byte[] bytes = Latin1Encoding.GetBytes(m_dir.td_inknames);
            dir.tdir_count = bytes.Length;
            return writeByteArray(ref dir, bytes);
        }

        /// <summary>
        /// Writes a contiguous directory item.
        /// </summary>
        private bool writeData(ref TiffDirEntry dir, byte[] buffer, int count)
        {
            dir.tdir_offset = m_dataoff;
            count = (int)dir.tdir_count * DataWidth(dir.tdir_type);
            if (seekOK(dir.tdir_offset) && writeOK(buffer, 0, count))
            {
                m_dataoff += (uint)((count + 1) & ~1);
                return true;
            }

            ErrorExt(this, m_clientdata, m_name,
                "Error writing data for field \"{0}\"",
                FieldWithTag(dir.tdir_tag).Name);
            return false;
        }

        private bool writeData(ref TiffDirEntry dir, short[] buffer, int count)
        {
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabArrayOfShort(buffer, count);

            int byteCount = count * sizeof(short);
            byte[] bytes = new byte [byteCount];
            ShortsToByteArray(buffer, 0, count, bytes, 0);
            return writeData(ref dir, bytes, byteCount);
        }

        private bool writeData(ref TiffDirEntry dir, int[] cp, int cc)
        {
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabArrayOfLong(cp, cc);

            int byteCount = cc * sizeof(int);
            byte[] bytes = new byte [byteCount];
            IntsToByteArray(cp, 0, cc, bytes, 0);
            bool res = writeData(ref dir, bytes, byteCount);
            return res;
        }

        private bool writeData(ref TiffDirEntry dir, float[] cp, int cc)
        {
            int[] ints = new int[cc];
            for (int i = 0; i < cc; i++)
            {
                byte[] result = BitConverter.GetBytes(cp[i]);
                ints[i] = BitConverter.ToInt32(result, 0);
            }

            return writeData(ref dir, ints, cc);
        }

        private bool writeData(ref TiffDirEntry dir, double[] buffer, int count)
        {
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabArrayOfDouble(buffer, count);

            byte[] bytes = new byte[count * sizeof(double)];
            Buffer.BlockCopy(buffer, 0, bytes, 0, bytes.Length);

            return writeData(ref dir, bytes, count * sizeof(double));
        }

        /// <summary>
        /// Link the current directory into the directory chain for the file.
        /// </summary>
        private bool linkDirectory()
        {
            const string module = "linkDirectory";

            m_diroff = (uint)((seekFile(0, SeekOrigin.End) + 1) & ~1);
            uint diroff = m_diroff;
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabUInt(ref diroff);

            // Handle SubIFDs

            if ((m_flags & TiffFlags.INSUBIFD) == TiffFlags.INSUBIFD)
            {
                seekFile(m_subifdoff, SeekOrigin.Begin);
                if (!writeIntOK((int)diroff))
                {
                    ErrorExt(this, m_clientdata, module,
                        "{0}: Error writing SubIFD directory link", m_name);
                    return false;
                }

                // Advance to the next SubIFD or, if this is the last one
                // configured, revert back to the normal directory linkage.
                --m_nsubifd;

                if (m_nsubifd != 0)
                    m_subifdoff += sizeof(int);
                else
                    m_flags &= ~TiffFlags.INSUBIFD;
                
                return true;
            }

            if (m_header.tiff_diroff == 0)
            {
                // First directory, overwrite offset in header.

                m_header.tiff_diroff = m_diroff;
                seekFile(TiffHeader.TIFF_MAGIC_SIZE + TiffHeader.TIFF_VERSION_SIZE, SeekOrigin.Begin);
                if (!writeIntOK((int)diroff))
                {
                    ErrorExt(this, m_clientdata, m_name, "Error writing TIFF header");
                    return false;
                }

                return true;
            }

            // Not the first directory, search to the last and append.

            uint nextdir = m_header.tiff_diroff;
            do
            {
                short dircount;
                if (!seekOK(nextdir) || !readShortOK(out dircount))
                {
                    ErrorExt(this, m_clientdata, module, "Error fetching directory count");
                    return false;
                }
                
                if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    SwabShort(ref dircount);

                seekFile(dircount * TiffDirEntry.SizeInBytes, SeekOrigin.Current);
                if (!readUIntOK(out nextdir))
                {
                    ErrorExt(this, m_clientdata, module, "Error fetching directory link");
                    return false;
                }

                if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    SwabUInt(ref nextdir);
            }
            while (nextdir != 0);

            // get current offset
            long off = seekFile(0, SeekOrigin.Current);
            seekFile(off - sizeof(int), SeekOrigin.Begin);

            if (!writeIntOK((int)diroff))
            {
                ErrorExt(this, m_clientdata, module, "Error writing directory link");
                return false;
            }

            return true;
        }
    }
}
