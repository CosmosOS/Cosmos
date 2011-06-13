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
using System.Text;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private const int TIFF_VERSION = 42;
        private const int TIFF_BIGTIFF_VERSION = 43;

        private const short TIFF_BIGENDIAN = 0x4d4d;
        private const short TIFF_LITTLEENDIAN = 0x4949;
        private const short MDI_LITTLEENDIAN = 0x5045;

        // reference white
        private const float D50_X0 = 96.4250F;
        private const float D50_Y0 = 100.0F;
        private const float D50_Z0 = 82.4680F;

        internal const int STRIP_SIZE_DEFAULT = 8192;

        /// <summary>
        /// Support strip chopping (whether or not to convert single-strip 
        /// uncompressed images to mutiple strips of ~8Kb to reduce memory usage)
        /// </summary>
        internal const TiffFlags STRIPCHOP_DEFAULT = TiffFlags.STRIPCHOP;

        /// <summary>
        /// Treat extra sample as alpha (default enabled). The RGBA interface 
        /// will treat a fourth sample with no EXTRASAMPLE_ value as being 
        /// ASSOCALPHA. Many packages produce RGBA files but don't mark the 
        /// alpha properly.
        /// </summary>
        internal const bool DEFAULT_EXTRASAMPLE_AS_ALPHA = true;

        /// <summary>
        /// Pick up YCbCr subsampling info from the JPEG data stream to support 
        /// files lacking the tag (default enabled).
        /// </summary>
        internal const bool CHECK_JPEG_YCBCR_SUBSAMPLING = true;

#if !SILVERLIGHT
        internal static Encoding Latin1Encoding = Encoding.GetEncoding("Latin1");
#else
        // Encoding.GetEncoding("Latin1") is not supported in Silverlight. Will throw exceptions at runtime.
        internal static Enc28591 Latin1Encoding = new Enc28591();
#endif        

        internal enum PostDecodeMethodType
        {
            pdmNone,
            pdmSwab16Bit,
            pdmSwab24Bit,
            pdmSwab32Bit,
            pdmSwab64Bit
        };

        /// <summary>
        /// name of open file
        /// </summary>
        internal string m_name;

        /// <summary>
        /// open mode (O_*)
        /// </summary>
        internal int m_mode;
        internal TiffFlags m_flags;

        //
        // the first directory
        //

        /// <summary>
        /// file offset of current directory
        /// </summary>
        internal uint m_diroff;

        // directories to prevent IFD looping

        /// <summary>
        /// internal rep of current directory
        /// </summary>
        internal TiffDirectory m_dir;

        /// <summary>
        /// current scanline
        /// </summary>
        internal int m_row;

        /// <summary>
        /// current strip for read/write
        /// </summary>
        internal int m_curstrip;

        // tiling support

        /// <summary>
        /// current tile for read/write
        /// </summary>
        internal int m_curtile;

        /// <summary>
        /// # of bytes in a tile
        /// </summary>
        internal int m_tilesize;

        // compression scheme hooks
        internal TiffCodec m_currentCodec;

        // input/output buffering

        /// <summary>
        /// # of bytes in a scanline
        /// </summary>
        internal int m_scanlinesize;

        /// <summary>
        /// raw data buffer
        /// </summary>
        internal byte[] m_rawdata;

        /// <summary>
        /// # of bytes in raw data buffer
        /// </summary>
        internal int m_rawdatasize;

        /// <summary>
        /// current spot in raw buffer
        /// </summary>
        internal int m_rawcp;

        /// <summary>
        /// bytes unread from raw buffer
        /// </summary>
        internal int m_rawcc;

        /// <summary>
        /// callback parameter
        /// </summary>
        internal object m_clientdata;

        // post-decoding support

        /// <summary>
        /// post decoding method type
        /// </summary>
        internal PostDecodeMethodType m_postDecodeMethod;

        // tag support

        /// <summary>
        /// tag get/set/print routines
        /// </summary>
        internal TiffTagMethods m_tagmethods;

        private class codecList
        {
            public codecList next;
            public TiffCodec codec;
        };

        private class clientInfoLink
        {
            public clientInfoLink next;
            public object data;
            public string name;
        };

        // the first directory

        /// <summary>
        /// file offset of following directory
        /// </summary>
        private uint m_nextdiroff;

        /// <summary>
        /// list of offsets to already seen directories to prevent IFD looping
        /// </summary>
        private uint[] m_dirlist;

        /// <summary>
        /// number of entires in offset list
        /// </summary>
        private int m_dirlistsize;

        /// <summary>
        /// number of already seen directories
        /// </summary>
        private short m_dirnumber;

        /// <summary>
        /// file's header block
        /// </summary>
        private TiffHeader m_header;

        /// <summary>
        /// data type shift counts
        /// </summary>
        private int[] m_typeshift;

        /// <summary>
        /// data type masks
        /// </summary>
        private uint[] m_typemask;

        /// <summary>
        /// current directory (index)
        /// </summary>
        private short m_curdir;

        /// <summary>
        /// current offset for read/write
        /// </summary>
        private uint m_curoff;

        /// <summary>
        /// current offset for writing dir
        /// </summary>
        private uint m_dataoff;

        //
        // SubIFD support
        // 

        /// <summary>
        /// remaining subifds to write
        /// </summary>
        private short m_nsubifd;

        /// <summary>
        /// offset for patching SubIFD link
        /// </summary>
        private uint m_subifdoff;

        // tiling support

        /// <summary>
        /// current column (offset by row too)
        /// </summary>
        private int m_col;

        // compression scheme hooks

        private bool m_decodestatus;

        // tag support

        /// <summary>
        /// sorted table of registered tags
        /// </summary>
        private TiffFieldInfo[] m_fieldinfo;

        /// <summary>
        /// # entries in registered tag table
        /// </summary>
        private int m_nfields;

        /// <summary>
        /// cached pointer to already found tag
        /// </summary>
        private TiffFieldInfo m_foundfield;

        /// <summary>
        /// extra client information.
        /// </summary>
        private clientInfoLink m_clientinfo;

        private TiffCodec[] m_builtInCodecs;
        private codecList m_registeredCodecs;

        private TiffTagMethods m_defaultTagMethods;

        private static TiffErrorHandler m_errorHandler;
        private TiffErrorHandler m_defaultErrorHandler;

        private bool m_disposed;
        private Stream m_fileStream;
 
        /// <summary>
        /// Client Tag extension support (from Niles Ritter).
        /// </summary>
        private static TiffExtendProc m_extender;

        /// <summary>
        /// stream used for read|write|etc.
        /// </summary>
        private TiffStream m_stream;

        private Tiff()
        {
            m_clientdata = 0;
            m_postDecodeMethod = PostDecodeMethodType.pdmNone;

            setupBuiltInCodecs();

            m_defaultTagMethods = new TiffTagMethods();

            m_defaultErrorHandler = null;
            if (m_errorHandler == null)
            {
                // user did not setup custom handler.
                // install default
                m_defaultErrorHandler = new TiffErrorHandler();
                m_errorHandler = m_defaultErrorHandler;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!this.m_disposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    Close();

                    if (m_fileStream != null)
                        m_fileStream.Dispose();
                }

                // Call the appropriate methods to clean up
                // unmanaged resources here.

                // Note disposing has been done.
                m_disposed = true;
            }
        }

        /// <summary>
        /// Writes custom directory. See ticket #51.
        /// </summary>
        /// <param name="pdiroff">Output directory offset.</param>
        /// <returns><c>true</c> if succeeded; otherwise, <c>false</c></returns>
        private bool WriteCustomDirectory(out long pdiroff)
        {
            pdiroff = -1;

            if (m_mode == O_RDONLY)
                return true;

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
            TiffDirEntry[] data = new TiffDirEntry[nfields];

            // Put the directory at the end of the file.
            m_diroff = (uint)((seekFile(0, SeekOrigin.End) + 1) & ~1);
            m_dataoff = m_diroff + sizeof(short) + (uint)dirsize + sizeof(int);
            if ((m_dataoff & 1) != 0)
                m_dataoff++;

            seekFile(m_dataoff, SeekOrigin.Begin);

            // Setup external form of directory entries and write data items.
            int[] fields = new int[FieldBit.SetLongs];
            Buffer.BlockCopy(m_dir.td_fieldsset, 0, fields, 0, FieldBit.SetLongs * sizeof(int));

            for (int fi = 0, nfi = m_nfields; nfi > 0; nfi--, fi++)
            {
                TiffFieldInfo fip = m_fieldinfo[fi];

                // For custom fields, we test to see if the custom field
                // is set or not.  For normal fields, we just use the FieldSet test.
                if (fip.Bit == FieldBit.Custom)
                {
                    bool is_set = false;
                    for (int ci = 0; ci < m_dir.td_customValueCount; ci++)
                        is_set |= (m_dir.td_customValues[ci].info == fip);

                    if (!is_set)
                        continue;
                }
                else if (!fieldSet(fields, fip.Bit))
                    continue;

                if (fip.Bit != FieldBit.Custom)
                    resetFieldBit(fields, fip.Bit);
            }

            // Write directory.

            short dircount = (short)nfields;
            pdiroff = m_nextdiroff;
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
            {
                // The file's byte order is opposite to the native machine
                // architecture. We overwrite the directory information with
                // impunity because it'll be released below after we write it
                // to the file. Note that all the other tag construction
                // routines assume that we do this byte-swapping; i.e. they only
                // byte-swap indirect data.
                for (int i = 0; i < dircount; i++)
                {
                    TiffDirEntry dirEntry = data[i];

                    short temp = (short)dirEntry.tdir_tag;
                    SwabShort(ref temp);
                    dirEntry.tdir_tag = (TiffTag)(ushort)temp;

                    temp = (short)dirEntry.tdir_type;
                    SwabShort(ref temp);
                    dirEntry.tdir_type = (TiffType)temp;

                    SwabLong(ref dirEntry.tdir_count);
                    SwabUInt(ref dirEntry.tdir_offset);
                }

                dircount = (short)nfields;
                SwabShort(ref dircount);

                int tempOff = (int)pdiroff;
                SwabLong(ref tempOff);
                pdiroff = tempOff;
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

            if (!writeIntOK((int)pdiroff))
            {
                ErrorExt(this, m_clientdata, m_name, "Error writing directory link");
                return false;
            }

            return true;
        }

        internal static void SwabUInt(ref uint lp)
        {
            byte[] cp = new byte[4];
            cp[0] = (byte)lp;
            cp[1] = (byte)(lp >> 8);
            cp[2] = (byte)(lp >> 16);
            cp[3] = (byte)(lp >> 24);

            byte t = cp[3];
            cp[3] = cp[0];
            cp[0] = t;

            t = cp[2];
            cp[2] = cp[1];
            cp[1] = t;

            lp = (uint)(cp[0] & 0xFF);
            lp += (uint)((cp[1] & 0xFF) << 8);
            lp += (uint)((cp[2] & 0xFF) << 16);
            lp += (uint)(cp[3] << 24);
        }

        internal static uint[] Realloc(uint[] buffer, int elementCount, int newElementCount)
        {
            uint[] newBuffer = new uint[newElementCount];
            if (buffer != null)
            {
                int copyLength = Math.Min(elementCount, newElementCount);
                Buffer.BlockCopy(buffer, 0, newBuffer, 0, copyLength * sizeof(uint));
            }

            return newBuffer;
        }

        internal static TiffFieldInfo[] Realloc(TiffFieldInfo[] buffer, int elementCount, int newElementCount)
        {
            TiffFieldInfo[] newBuffer = new TiffFieldInfo [newElementCount];

            if (buffer != null)
            {
                int copyLength = Math.Min(elementCount, newElementCount);
                Array.Copy(buffer, newBuffer, copyLength);
            }

            return newBuffer;
        }

        internal static TiffTagValue[] Realloc(TiffTagValue[] buffer, int elementCount, int newElementCount)
        {
            TiffTagValue[] newBuffer = new TiffTagValue[newElementCount];

            if (buffer != null)
            {
                int copyLength = Math.Min(elementCount, newElementCount);
                Array.Copy(buffer, newBuffer, copyLength);
            }

            return newBuffer;
        }

        internal bool setCompressionScheme(Compression scheme)
        {
            TiffCodec c = FindCodec(scheme);
            if (c == null)
            {
                /*
                 * Don't treat an unknown compression scheme as an error.
                 * This permits applications to open files with data that
                 * the library does not have builtin support for, but which
                 * may still be meaningful.
                 */
                c = m_builtInCodecs[0];
            }

            m_decodestatus = c.CanDecode;
            m_flags &= ~(TiffFlags.NOBITREV | TiffFlags.NOREADRAW);

            m_currentCodec = c;
            return c.Init();
        }

        /// <summary>
        /// post decoding routine
        /// </summary>
        private void postDecode(byte[] buffer, int offset, int count)
        {
            switch (m_postDecodeMethod)
            {
                case PostDecodeMethodType.pdmSwab16Bit:
                    swab16BitData(buffer, offset, count);
                    break;
                case PostDecodeMethodType.pdmSwab24Bit:
                    swab24BitData(buffer, offset, count);
                    break;
                case PostDecodeMethodType.pdmSwab32Bit:
                    swab32BitData(buffer, offset, count);
                    break;
                case PostDecodeMethodType.pdmSwab64Bit:
                    swab64BitData(buffer, offset, count);
                    break;
            }
        }
    }
}
