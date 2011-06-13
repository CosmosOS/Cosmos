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
 * Tile-oriented Read Support
 * Contributed by Nancy Cam (Silicon Graphics).
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Tag Image File Format (TIFF)
    /// </summary>
    /// <remarks>
    /// Based on Rev 6.0 from
    /// <see href="http://partners.adobe.com/asn/developer/PDFS/TN/TIFF6.pdf" target="_blank"/>
    /// </remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff : IDisposable
    {
        /// <summary>
        /// Delegate for LibTiff.Net extender method
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class.</param>
        /// <remarks>
        /// <para>Extender method is usually used for registering custom tags.</para>
        /// <para>To setup extender method that will be called upon creation of
        /// each instance of <see cref="Tiff"/> object please use <see cref="SetTagExtender"/>
        /// method.</para>
        /// </remarks>
        public delegate void TiffExtendProc(Tiff tif);

        /// <summary>
        /// Delegate for a method used to image decoded spans.        
        /// </summary>
        /// <param name="buffer">The buffer to place decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="runs">The array of black and white run lengths (white then black).</param>
        /// <param name="thisRunOffset">The zero-based offset in <paramref name="runs"/> array at
        /// which current row's run begins.</param>
        /// <param name="nextRunOffset">The zero-based offset in <paramref name="runs"/> array at
        /// which next row's run begins.</param>
        /// <param name="width">The width in pixels of the row.</param>
        /// <remarks><para>
        /// To override the default method used to image decoded spans please set
        /// <see cref="TiffTag.FAXFILLFUNC"/> tag with an instance of this delegate.</para>
        /// <para>
        /// Fill methods can assume the <paramref name="runs"/> array has room for at least
        /// <paramref name="width"/> runs and can overwrite data in the <paramref name="runs"/>
        /// array as needed (e.g. to append zero runs to bring the count up to a nice multiple).
        /// </para></remarks>
        public delegate void FaxFillFunc(
            byte[] buffer, int offset, int[] runs, int thisRunOffset, int nextRunOffset, int width);
        
        /// <summary>
        /// Gets the library version string.
        /// </summary>
        /// <returns>The library version string.</returns>
        public static string GetVersion()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "LibTiff.Net, Version {0}\nCopyright (C) 2008-2011, Bit Miracle.", AssemblyVersion);
        }

        /// <summary>
        /// Gets the version of the library's assembly.
        /// </summary>
        /// <value>The version of the library's assembly.</value>
        public static string AssemblyVersion
        {
            get
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string versionString = version.Major.ToString(CultureInfo.InvariantCulture) +
                    "." + version.Minor.ToString(CultureInfo.InvariantCulture);

                versionString += "." + version.Build.ToString(CultureInfo.InvariantCulture);
                versionString += "." + version.Revision.ToString(CultureInfo.InvariantCulture);

                return versionString;
            }
        }

        /// <summary>
        /// Gets the R component from ABGR value returned by 
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage">ReadRGBAImage</see>.
        /// </summary>
        /// <param name="abgr">The ABGR value.</param>
        /// <returns>The R component from ABGR value.</returns>
        public static int GetR(int abgr)
        {
            return (abgr & 0xff);
        }

        /// <summary>
        /// Gets the G component from ABGR value returned by 
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage">ReadRGBAImage</see>.
        /// </summary>
        /// <param name="abgr">The ABGR value.</param>
        /// <returns>The G component from ABGR value.</returns>
        public static int GetG(int abgr)
        {
            return ((abgr >> 8) & 0xff);
        }

        /// <summary>
        /// Gets the B component from ABGR value returned by 
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage">ReadRGBAImage</see>.
        /// </summary>
        /// <param name="abgr">The ABGR value.</param>
        /// <returns>The B component from ABGR value.</returns>
        public static int GetB(int abgr)
        {
            return ((abgr >> 16) & 0xff);
        }

        /// <summary>
        /// Gets the A component from ABGR value returned by 
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage">ReadRGBAImage</see>.
        /// </summary>
        /// <param name="abgr">The ABGR value.</param>
        /// <returns>The A component from ABGR value.</returns>
        public static int GetA(int abgr)
        {
            return ((abgr >> 24) & 0xff);
        }

        /// <summary>
        /// Retrieves the codec registered for the specified compression scheme.
        /// </summary>
        /// <param name="scheme">The compression scheme.</param>
        /// <returns>The codec registered for the specified compression scheme or <c>null</c>
        /// if there is no codec registered for the given scheme.</returns>
        /// <remarks>
        /// <para>
        /// LibTiff.Net supports a variety of compression schemes implemented by software codecs.
        /// Each codec adheres to a modular interface that provides for the decoding and encoding
        /// of image data; as well as some other methods for initialization, setup, cleanup, and
        /// the control of default strip and tile sizes. Codecs are identified by the associated
        /// value of the <see cref="TiffTag"/>.COMPRESSION tag.
        /// </para>
        /// <para>
        /// Other compression schemes may be registered. Registered schemes can also override the
        /// built-in versions provided by the library.
        /// </para>
        /// </remarks>
        public TiffCodec FindCodec(Compression scheme)
        {
            for (codecList list = m_registeredCodecs; list != null; list = list.next)
            {
                if (list.codec.m_scheme == scheme)
                    return list.codec;
            }

            for (int i = 0; m_builtInCodecs[i] != null; i++)
            {
                TiffCodec codec = m_builtInCodecs[i];
                if (codec.m_scheme == scheme)
                    return codec;
            }

            return null;
        }

        /// <summary>
        /// Adds specified codec to a list of registered codec.
        /// </summary>
        /// <param name="codec">The codec to register.</param>
        /// <remarks>
        /// This method can be used to augment or override the set of codecs available to an
        /// application. If the <paramref name="codec"/> is for a scheme that already has a
        /// registered codec then it is overridden and any images with data encoded with this
        /// compression scheme will be decoded using the supplied codec.
        /// </remarks>
        public void RegisterCodec(TiffCodec codec)
        {
            if (codec == null)
                throw new ArgumentNullException("codec");

            codecList list = new codecList();
            list.codec = codec;
            list.next = m_registeredCodecs;
            m_registeredCodecs = list;
        }

        /// <summary>
        /// Removes specified codec from a list of registered codecs.
        /// </summary>
        /// <param name="codec">The codec to remove from a list of registered codecs.</param>
        public void UnRegisterCodec(TiffCodec codec)
        {
            if (m_registeredCodecs == null)
                return;

            codecList temp;
            if (m_registeredCodecs.codec == codec)
            {
                temp = m_registeredCodecs.next;
                m_registeredCodecs = temp;
                return;
            }

            for (codecList list = m_registeredCodecs; list != null; list = list.next)
            {
                if (list.next != null)
                {
                    if (list.next.codec == codec)
                    {
                        temp = list.next.next;
                        list.next = temp;
                        return;
                    }
                }
            }

            ErrorExt(this, 0, "UnRegisterCodec",
                "Cannot remove compression scheme {0}; not registered", codec.m_name);
        }

        /// <summary>
        /// Checks whether library has working codec for the specific compression scheme.
        /// </summary>
        /// <param name="scheme">The scheme to check.</param>
        /// <returns>
        /// <c>true</c> if the codec is configured and working; otherwise, <c>false</c>.
        /// </returns>
        public bool IsCodecConfigured(Compression scheme)
        {
            TiffCodec codec = FindCodec(scheme);

            if (codec == null)
                return false;

            if (codec.CanEncode != false || codec.CanDecode != false)
                return true;

            return false;
        }

        /// <summary>
        /// Retrieves an array of configured codecs, both built-in and registered by user.
        /// </summary>
        /// <returns>An array of configured codecs.</returns>
        public TiffCodec[] GetConfiguredCodecs()
        {
            int totalCodecs = 0;
            for (int i = 0; m_builtInCodecs[i] != null; i++)
            {
                if (m_builtInCodecs[i] != null && IsCodecConfigured(m_builtInCodecs[i].m_scheme))
                    totalCodecs++;
            }

            for (codecList cd = m_registeredCodecs; cd != null; cd = cd.next)
                totalCodecs++;

            TiffCodec[] codecs = new TiffCodec[totalCodecs];

            int codecPos = 0;
            for (codecList cd = m_registeredCodecs; cd != null; cd = cd.next)
                codecs[codecPos++] = cd.codec;

            for (int i = 0; m_builtInCodecs[i] != null; i++)
            {
                if (m_builtInCodecs[i] != null && IsCodecConfigured(m_builtInCodecs[i].m_scheme))
                    codecs[codecPos++] = m_builtInCodecs[i];
            }

            return codecs;
        }

        /// <summary>
        /// Allocates new byte array of specified size and copies data from the existing to
        /// the new array.
        /// </summary>
        /// <param name="array">The existing array.</param>
        /// <param name="size">The number of elements in new array.</param>
        /// <returns>
        /// The new byte array of specified size with data from the existing array.
        /// </returns>
        /// <overloads>Allocates new array of specified size and copies data from the existing to
        /// the new array.</overloads>
        public static byte[] Realloc(byte[] array, int size)
        {
            byte[] newArray = new byte[size];
            if (array != null)
            {
                int copyLength = Math.Min(array.Length, size);
                Buffer.BlockCopy(array, 0, newArray, 0, copyLength);
            }

            return newArray;
        }

        /// <summary>
        /// Allocates new integer array of specified size and copies data from the existing to
        /// the new array.
        /// </summary>
        /// <param name="array">The existing array.</param>
        /// <param name="size">The number of elements in new array.</param>
        /// <returns>
        /// The new integer array of specified size with data from the existing array.
        /// </returns>
        /// <remarks>Size of the array is in elements, not bytes.</remarks>
        public static int[] Realloc(int[] array, int size)
        {
            int[] newArray = new int[size];
            if (array != null)
            {
                int copyLength = Math.Min(array.Length, size);
                Buffer.BlockCopy(array, 0, newArray, 0, copyLength * sizeof(int));
            }

            return newArray;
        }

        /// <summary>
        /// Compares specified number of elements in two arrays.
        /// </summary>
        /// <param name="first">The first array to compare.</param>
        /// <param name="second">The second array to compare.</param>
        /// <param name="elementCount">The number of elements to compare.</param>
        /// <returns>
        /// The difference between compared elements or 0 if all elements are equal.
        /// </returns>
        public static int Compare(short[] first, short[] second, int elementCount)
        {
            for (int i = 0; i < elementCount; i++)
            {
                if (first[i] != second[i])
                    return first[i] - second[i];
            }

            return 0;
        }

        /// <summary>
        /// Initializes new instance of <see cref="Tiff"/> class and opens a TIFF file for
        /// reading or writing.
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        /// <param name="mode">The open mode. Specifies if the file is to be opened for
        /// reading ("r"), writing ("w"), or appending ("a") and, optionally, whether to override
        /// certain default aspects of library operation (see remarks).</param>
        /// <returns>The new instance of <see cref="Tiff"/> class if specified file is
        /// successfully opened; otherwise, <c>null</c>.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="Open"/> opens a TIFF file whose name is <paramref name="fileName"/>. When
        /// a file is opened for appending, existing data will not be touched; instead new data
        /// will be written as additional subfiles. If an existing file is opened for writing,
        /// all previous data is overwritten.
        /// </para>
        /// <para>
        /// If a file is opened for reading, the first TIFF directory in the file is automatically
        /// read (see <see cref="SetDirectory"/> for reading directories other than the first). If
        /// a file is opened for writing or appending, a default directory is automatically
        /// created for writing subsequent data. This directory has all the default values
        /// specified in TIFF Revision 6.0: BitsPerSample = 1, ThreshHolding = Threshold.BILEVEL
        /// (bilevel art scan), FillOrder = MSB2LSB (most significant bit of each data byte is
        /// filled first), Orientation = TOPLEFT (the 0th row represents the visual top of the
        /// image, and the 0th column represents the visual left hand side), SamplesPerPixel = 1,
        /// RowsPerStrip = infinity, ResolutionUnit = INCH, and Compression = NONE. To alter
        /// these values, or to define values for additional fields, <see cref="SetField"/> must
        /// be used.
        /// </para>
        /// <para>
        /// The <paramref name="mode"/> parameter can include the following flags in addition to
        /// the "r", "w", and "a" flags. Note however that option flags must follow the
        /// read-write-append specification.
        /// </para>
        /// <list type="table"><listheader>
        /// <term>Flag</term><description>Description</description></listheader>
        /// <item><term>l</term>
        /// <description>When creating a new file force information be written with Little-Endian
        /// byte order (but see below).</description></item>
        /// <item><term>b</term>
        /// <description>When creating a new file force information be written with Big-Endian
        /// byte order (but see below).</description></item>
        /// <item><term>L</term>
        /// <description>Force image data that is read or written to be treated with bits filled
        /// from Least Significant Bit (LSB) to Most Significant Bit (MSB). Note that this is the
        /// opposite to the way the library has worked from its inception.</description></item>
        /// <item><term>B</term>
        /// <description>Force image data that is read or written to be treated with bits filled
        /// from Most Significant Bit (MSB) to Least Significant Bit (LSB); this is the
        /// default.</description></item>
        /// <item><term>H</term>
        /// <description>Force image data that is read or written to be treated with bits filled
        /// in the same order as the native CPU.</description></item>
        /// <item><term>C</term>
        /// <description>Enable the use of "strip chopping" when reading images that are comprised
        /// of a single strip or tile of uncompressed data. Strip chopping is a mechanism by which
        /// the library will automatically convert the single-strip image to multiple strips, each
        /// of which has about 8 Kilobytes of data. This facility can be useful in reducing the
        /// amount of memory used to read an image because the library normally reads each strip
        /// in its entirety. Strip chopping does however alter the apparent contents of the image
        /// because when an image is divided into multiple strips it looks as though the
        /// underlying file contains multiple separate strips. The default behaviour is to enable 
        /// strip chopping.</description></item>
        /// <item><term>c</term>
        /// <description>Disable the use of strip chopping when reading images.</description></item>
        /// <item><term>h</term>
        /// <description>Read TIFF header only, do not load the first image directory. That could
        /// be useful in case of the broken first directory. We can open the file and proceed to
        /// the other directories.</description></item></list>
        /// <para>
        /// By default the library will create new files with the native byte-order of the CPU on
        /// which the application is run. This ensures optimal performance and is portable to any
        /// application that conforms to the TIFF specification. To force the library to use a
        /// specific byte-order when creating a new file the "b" and "l" option flags may be
        /// included in the <paramref name="mode"/> parameter; for example, "wb" or "wl".</para>
        /// <para>The use of the "l" and "b" flags is strongly discouraged. These flags are
        /// provided solely because numerous vendors do not correctly support TIFF; they only
        /// support one of the two byte orders. It is strongly recommended that you not use this
        /// feature except to deal with busted apps that write invalid TIFF.</para>
        /// <para>The "L", "B", and "H" flags are intended for applications that can optimize
        /// operations on data by using a particular bit order.  By default the library returns
        /// data in MSB2LSB bit order. Returning data in the bit order of the native CPU makes the
        /// most sense but also requires applications to check the value of the
        /// <see cref="TiffTag.FILLORDER"/> tag; something they probably do not do right now.</para>
        /// <para>The "c" option permits applications that only want to look at the tags, for
        /// example, to get the unadulterated TIFF tag information.</para>
        /// </remarks>
        public static Tiff Open(string fileName, string mode)
        {
            const string module = "Open";

            FileMode fileMode;
            FileAccess fileAccess;
            getMode(mode, module, out fileMode, out fileAccess);

            FileStream stream = null;
            try
            {
                if (fileAccess == FileAccess.Read)
                    stream = File.Open(fileName, fileMode, fileAccess, FileShare.Read);
                else
                    stream = File.Open(fileName, fileMode, fileAccess);
            }
            catch (Exception e)
            {
                Error(module, "Failed to open '{0}'. {1}", fileName, e.Message);
                return null;
            }

            Tiff tif = ClientOpen(fileName, mode, stream, new TiffStream());
            if (tif == null)
                stream.Dispose();
            else
                tif.m_fileStream = stream;

            return tif;
        }

        /// <summary>
        /// Initializes new instance of <see cref="Tiff"/> class and opens a stream with TIFF data
        /// for reading or writing.
        /// </summary>
        /// <param name="name">The name for the new instance of <see cref="Tiff"/> class.</param>
        /// <param name="mode">The open mode. Specifies if the file is to be opened for
        /// reading ("r"), writing ("w"), or appending ("a") and, optionally, whether to override
        /// certain default aspects of library operation (see remarks for <see cref="Open"/>
        /// method for the list of the mode flags).</param>
        /// <param name="clientData">Some client data. This data is passed as parameter to every
        /// method of the <see cref="TiffStream"/> object specified by the
        /// <paramref name="stream"/> parameter.</param>
        /// <param name="stream">An instance of the <see cref="TiffStream"/> class to use for
        /// reading, writing and seeking of TIFF data.</param>
        /// <returns>The new instance of <see cref="Tiff"/> class if stream is successfully
        /// opened; otherwise, <c>null</c>.</returns>
        /// <remarks>
        /// <para>
        /// This method can be used to read TIFF data from sources other than file. When custom
        /// stream class derived from <see cref="TiffStream"/> is used it is possible to read (or
        /// write) TIFF data that reside in memory, database, etc.
        /// </para>
        /// <para>Please note, that <paramref name="name"/> is an arbitrary string used as
        /// ID for the created <see cref="Tiff"/>. It's not required to be a file name or anything
        /// meaningful at all.</para>
        /// <para>
        /// Please read remarks for <see cref="Open"/> method for the list of option flags that
        /// can be specified in <paramref name="mode"/> parameter.
        /// </para>
        /// </remarks>
        public static Tiff ClientOpen(string name, string mode, object clientData, TiffStream stream)
        {
            const string module = "ClientOpen";

            if (mode == null || mode.Length == 0)
            {
                ErrorExt(null, clientData, module, "{0}: mode string should contain at least one char", name);
                return null;
            }

            FileMode fileMode;
            FileAccess fileAccess;
            int m = getMode(mode, module, out fileMode, out fileAccess);

            Tiff tif = new Tiff();
            tif.m_name = name;

            tif.m_mode = m & ~(O_CREAT | O_TRUNC);
            tif.m_curdir = -1; // non-existent directory
            tif.m_curoff = 0;
            tif.m_curstrip = -1; // invalid strip
            tif.m_row = -1; // read/write pre-increment
            tif.m_clientdata = clientData;

            if (stream == null)
            {
                ErrorExt(tif, clientData, module, "TiffStream is null pointer.");
                return null;
            }

            tif.m_stream = stream;

            // setup default state
            tif.m_currentCodec = tif.m_builtInCodecs[0];

            // Default is to return data MSB2LSB and enable the use of
            // strip chopping when a file is opened read-only.
            tif.m_flags = TiffFlags.MSB2LSB;

            if (m == O_RDONLY || m == O_RDWR)
                tif.m_flags |= STRIPCHOP_DEFAULT;

            // Process library-specific flags in the open mode string.
            // See remarks for Open method for the list of supported flags.
            int modelength = mode.Length;
            for (int i = 0; i < modelength; i++)
            {
                switch (mode[i])
                {
                    case 'b':
                        if ((m & O_CREAT) != 0)
                            tif.m_flags |= TiffFlags.SWAB;
                        break;
                    case 'l':
                        break;
                    case 'B':
                        tif.m_flags = (tif.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.MSB2LSB;
                        break;
                    case 'L':
                        tif.m_flags = (tif.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.LSB2MSB;
                        break;
                    case 'H':
                        tif.m_flags = (tif.m_flags & ~TiffFlags.FILLORDER) | TiffFlags.LSB2MSB;
                        break;
                    case 'C':
                        if (m == O_RDONLY)
                            tif.m_flags |= TiffFlags.STRIPCHOP;
                        break;
                    case 'c':
                        if (m == O_RDONLY)
                            tif.m_flags &= ~TiffFlags.STRIPCHOP;
                        break;
                    case 'h':
                        tif.m_flags |= TiffFlags.HEADERONLY;
                        break;
                }
            }

            // Read in TIFF header.

            if ((tif.m_mode & O_TRUNC) != 0 || !tif.readHeaderOk(ref tif.m_header))
            {
                if (tif.m_mode == O_RDONLY)
                {
                    ErrorExt(tif, tif.m_clientdata, name, "Cannot read TIFF header");
                    return null;
                }

                // Setup header and write.

                if ((tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    tif.m_header.tiff_magic = TIFF_BIGENDIAN;
                else
                    tif.m_header.tiff_magic = TIFF_LITTLEENDIAN;

                tif.m_header.tiff_version = TIFF_VERSION;
                if ((tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    SwabShort(ref tif.m_header.tiff_version);

                tif.m_header.tiff_diroff = 0; // filled in later

                tif.seekFile(0, SeekOrigin.Begin);

                if (!tif.writeHeaderOK(tif.m_header))
                {
                    ErrorExt(tif, tif.m_clientdata, name, "Error writing TIFF header");
                    tif.m_mode = O_RDONLY;
                    return null;
                }

                // Setup the byte order handling.
                tif.initOrder(tif.m_header.tiff_magic);

                // Setup default directory.
                tif.setupDefaultDirectory();
                tif.m_diroff = 0;
                tif.m_dirlist = null;
                tif.m_dirlistsize = 0;
                tif.m_dirnumber = 0;
                return tif;
            }

            // Setup the byte order handling.
            if (tif.m_header.tiff_magic != TIFF_BIGENDIAN &&
                tif.m_header.tiff_magic != TIFF_LITTLEENDIAN &&
                tif.m_header.tiff_magic != MDI_LITTLEENDIAN)
            {
                ErrorExt(tif, tif.m_clientdata, name,
                    "Not a TIFF or MDI file, bad magic number {0} (0x{1:x})",
                    tif.m_header.tiff_magic, tif.m_header.tiff_magic);
                tif.m_mode = O_RDONLY;
                return null;
            }

            tif.initOrder(tif.m_header.tiff_magic);

            // Swap header if required.
            if ((tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
            {
                SwabShort(ref tif.m_header.tiff_version);
                SwabUInt(ref tif.m_header.tiff_diroff);
            }

            // Now check version (if needed, it's been byte-swapped).
            // Note that this isn't actually a version number, it's a
            // magic number that doesn't change (stupid).
            if (tif.m_header.tiff_version == TIFF_BIGTIFF_VERSION)
            {
                ErrorExt(tif, tif.m_clientdata, name,
                    "This is a BigTIFF file. This format not supported\nby this version of LibTiff.Net.");
                tif.m_mode = O_RDONLY;
                return null;
            }

            if (tif.m_header.tiff_version != TIFF_VERSION)
            {
                ErrorExt(tif, tif.m_clientdata, name,
                    "Not a TIFF file, bad version number {0} (0x{1:x})",
                    tif.m_header.tiff_version, tif.m_header.tiff_version);
                tif.m_mode = O_RDONLY;
                return null;
            }

            tif.m_flags |= TiffFlags.MYBUFFER;
            tif.m_rawcp = 0;
            tif.m_rawdata = null;
            tif.m_rawdatasize = 0;

            // Sometimes we do not want to read the first directory (for example,
            // it may be broken) and want to proceed to other directories. I this
            // case we use the HEADERONLY flag to open file and return
            // immediately after reading TIFF header.
            if ((tif.m_flags & TiffFlags.HEADERONLY) == TiffFlags.HEADERONLY)
                return tif;

            // Setup initial directory.
            switch (mode[0])
            {
                case 'r':
                    tif.m_nextdiroff = tif.m_header.tiff_diroff;

                    if (tif.ReadDirectory())
                    {
                        tif.m_rawcc = -1;
                        tif.m_flags |= TiffFlags.BUFFERSETUP;
                        return tif;
                    }
                    break;
                case 'a':
                    // New directories are automatically append to the end of
                    // the directory chain when they are written out (see WriteDirectory).
                    tif.setupDefaultDirectory();
                    return tif;
            }

            tif.m_mode = O_RDONLY;
            return null;
        }

        /// <summary>
        /// Closes a previously opened TIFF file.
        /// </summary>
        /// <remarks>
        /// This method closes a file or stream that was previously opened with <see cref="Open"/>
        /// or <see cref="ClientOpen"/>. Any buffered data are flushed to the file/stream,
        /// including the contents of the current directory (if modified); and all resources
        /// are reclaimed.
        /// </remarks>
        public void Close()
        {
            Flush();

            m_stream.Close(m_clientdata);

            if (m_fileStream != null)
                m_fileStream.Close();
        }

        /// <summary>
        /// Frees and releases all resources allocated by this <see cref="Tiff"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the number of elements in the custom tag list.
        /// </summary>
        /// <returns>The number of elements in the custom tag list.</returns>
        public int GetTagListCount()
        {
            return m_dir.td_customValueCount;
        }

        /// <summary>
        /// Retrieves the custom tag with specified index.
        /// </summary>
        /// <param name="index">The zero-based index of a custom tag to retrieve.</param>
        /// <returns>The custom tag with specified index.</returns>
        public int GetTagListEntry(int index)
        {
            if (index < 0 || index >= m_dir.td_customValueCount)
                return -1;
            else
                return (int)m_dir.td_customValues[index].info.Tag;
        }

        /// <summary>
        /// Merges given field information to existing one.
        /// </summary>
        /// <param name="info">The array of <see cref="TiffFieldInfo"/> objects.</param>
        /// <param name="count">The number of items to use from the <paramref name="info"/> array.</param>
        public void MergeFieldInfo(TiffFieldInfo[] info, int count)
        {
            m_foundfield = null;

            if (m_nfields > 0)
                m_fieldinfo = Realloc(m_fieldinfo, m_nfields, m_nfields + count);
            else
                m_fieldinfo = new TiffFieldInfo[count];

            for (int i = 0; i < count; i++)
            {
                TiffFieldInfo fip = FindFieldInfo(info[i].Tag, info[i].Type);

                // only add definitions that aren't already present
                if (fip == null)
                {
                    m_fieldinfo[m_nfields] = info[i];
                    m_nfields++;
                }
            }

            // Sort the field info by tag number
            IComparer myComparer = new TagCompare();
            Array.Sort(m_fieldinfo, 0, m_nfields, myComparer);
        }

        /// <summary>
        /// Retrieves field information for the specified tag.
        /// </summary>
        /// <param name="tag">The tag to retrieve field information for.</param>
        /// <param name="type">The tiff data type to use us additional filter.</param>
        /// <returns>The field information for specified tag with specified type or <c>null</c> if
        /// the field information wasn't found.</returns>
        public TiffFieldInfo FindFieldInfo(TiffTag tag, TiffType type)
        {
            if (m_foundfield != null && m_foundfield.Tag == tag &&
                (type == TiffType.ANY || type == m_foundfield.Type))
            {
                return m_foundfield;
            }

            // If we are invoked with no field information, then just return.
            if (m_fieldinfo == null)
                return null;

            m_foundfield = null;

            foreach (TiffFieldInfo info in m_fieldinfo)
            {
                if (info != null && info.Tag == tag && (type == TiffType.ANY || type == info.Type))
                {
                    m_foundfield = info;
                    break;
                }
            }

            return m_foundfield;
        }

        /// <summary>
        /// Retrieves field information for the tag with specified name.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve field information for.</param>
        /// <param name="type">The tiff data type to use us additional filter.</param>
        /// <returns>The field information for specified tag with specified type or <c>null</c> if
        /// the field information wasn't found.</returns>
        public TiffFieldInfo FindFieldInfoByName(string name, TiffType type)
        {
            if (m_foundfield != null && m_foundfield.Name == name &&
                (type == TiffType.ANY || type == m_foundfield.Type))
            {
                return m_foundfield;
            }

            // If we are invoked with no field information, then just return.
            if (m_fieldinfo == null)
                return null;

            m_foundfield = null;

            foreach (TiffFieldInfo info in m_fieldinfo)
            {
                if (info != null && info.Name == name &&
                    (type == TiffType.ANY || type == info.Type))
                {
                    m_foundfield = info;
                    break;
                }
            }

            return m_foundfield;
        }

        /// <summary>
        /// Retrieves field information for the specified tag.
        /// </summary>
        /// <param name="tag">The tag to retrieve field information for.</param>
        /// <returns>The field information for specified tag or <c>null</c> if
        /// the field information wasn't found.</returns>
        public TiffFieldInfo FieldWithTag(TiffTag tag)
        {
            TiffFieldInfo fip = FindFieldInfo(tag, TiffType.ANY);
            if (fip != null)
                return fip;

            ErrorExt(this, m_clientdata, "FieldWithTag", "Internal error, unknown tag 0x{0:x}", tag);
            Debug.Assert(false);
            return null;
        }

        /// <summary>
        /// Retrieves field information for the tag with specified name.
        /// </summary>
        /// <param name="name">The name of the tag to retrieve field information for.</param>
        /// <returns>The field information for specified tag or <c>null</c> if
        /// the field information wasn't found.</returns>
        public TiffFieldInfo FieldWithName(string name)
        {
            TiffFieldInfo fip = FindFieldInfoByName(name, TiffType.ANY);
            if (fip != null)
                return fip;

            ErrorExt(this, m_clientdata, "FieldWithName", "Internal error, unknown tag {0}", name);
            Debug.Assert(false);
            return null;
        }

        /// <summary>
        /// Gets the currently used tag methods.
        /// </summary>
        /// <returns>The currently used tag methods.</returns>
        public TiffTagMethods GetTagMethods()
        {
            return m_tagmethods;
        }

        /// <summary>
        /// Sets the new tag methods to use.
        /// </summary>
        /// <param name="methods">Tag methods.</param>
        /// <returns>The previously used tag methods.</returns>
        public TiffTagMethods SetTagMethods(TiffTagMethods methods)
        {
            TiffTagMethods prevTagMethods = m_tagmethods;

            if (methods != null)
                m_tagmethods = methods;

            return prevTagMethods;
        }

        /// <summary>
        /// Gets the extra information with specified name associated with this <see cref="Tiff"/>.
        /// </summary>
        /// <param name="name">Name of the extra information to retrieve.</param>
        /// <returns>The extra information with specified name associated with
        /// this <see cref="Tiff"/> or <c>null</c> if extra information with specified
        /// name was not found.</returns>
        public object GetClientInfo(string name)
        {
            // should get copy
            clientInfoLink link = m_clientinfo;

            while (link != null && link.name != name)
                link = link.next;

            if (link != null)
                return link.data;

            return null;
        }

        /// <summary>
        /// Associates extra information with this <see cref="Tiff"/>.
        /// </summary>
        /// <param name="data">The information to associate with this <see cref="Tiff"/>.</param>
        /// <param name="name">The name (label) of the information.</param>
        /// <remarks>If there is already an extra information with the name specified by
        /// <paramref name="name"/> it will be replaced by the information specified by
        /// <paramref name="data"/>.</remarks>
        public void SetClientInfo(object data, string name)
        {
            clientInfoLink link = m_clientinfo;

            // Do we have an existing link with this name? If so, just set it.
            while (link != null && link.name != name)
                link = link.next;

            if (link != null)
            {
                link.data = data;
                return;
            }

            // Create a new link.
            link = new clientInfoLink();
            link.next = m_clientinfo;
            link.name = name;
            link.data = data;

            m_clientinfo = link;
        }

        /// <summary>
        /// Flushes pending writes to an open TIFF file.
        /// </summary>
        /// <returns><c>true</c> if succeeded; otherwise, <c>false</c></returns>
        /// <remarks><see cref="Flush"/> causes any pending writes for the specified file
        /// (including writes for the current directory) to be done. In normal operation this call
        /// is never needed − the library automatically does any flushing required.
        /// </remarks>
        /// <seealso cref="FlushData"/>
        public bool Flush()
        {
            if (m_mode != O_RDONLY)
            {
                if (!FlushData())
                    return false;

                if ((m_flags & TiffFlags.DIRTYDIRECT) == TiffFlags.DIRTYDIRECT && !WriteDirectory())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Flushes any pending image data for the specified file to be written out.
        /// </summary>
        /// <returns><c>true</c> if succeeded; otherwise, <c>false</c></returns>
        /// <remarks><see cref="FlushData"/> flushes any pending image data for the specified file
        /// to be written out; directory-related data are not flushed. In normal operation this
        /// call is never needed − the library automatically does any flushing required.
        /// </remarks>
        /// <seealso cref="Flush"/>
        public bool FlushData()
        {
            if ((m_flags & TiffFlags.BEENWRITING) != TiffFlags.BEENWRITING)
                return false;

            if ((m_flags & TiffFlags.POSTENCODE) == TiffFlags.POSTENCODE)
            {
                m_flags &= ~TiffFlags.POSTENCODE;
                if (!m_currentCodec.PostEncode())
                    return false;
            }

            return flushData1();
        }

        /// <summary>
        /// Gets the value(s) of a tag in an open TIFF file.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The value(s) of a tag in an open TIFF file as array of
        /// <see cref="FieldValue"/> objects or <c>null</c> if there is no such tag set.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="GetField"/> returns the value(s) of a tag or pseudo-tag associated with the
        /// current directory of the opened TIFF file. The tag is identified by
        /// <paramref name="tag"/>. The type and number of values returned is dependent on the
        /// tag being requested. You may want to consult
        /// <a href = "54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> to become familiar with exact data types and calling
        /// conventions required for each tag supported by the library.
        /// </para>
        /// <para>
        /// A pseudo-tag is a parameter that is used to control the operation of the library but
        /// whose value is not read or written to the underlying file.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetFieldDefaulted"/>
        public FieldValue[] GetField(TiffTag tag)
        {
            TiffFieldInfo fip = FindFieldInfo(tag, TiffType.ANY);
            if (fip != null && (isPseudoTag(tag) || fieldSet(fip.Bit)))
                return m_tagmethods.GetField(this, tag);

            return null;
        }

        /// <summary>
        /// Gets the value(s) of a tag in an open TIFF file or default value(s) of a tag if a tag
        /// is not defined in the current directory and it has a default value(s).
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>
        /// The value(s) of a tag in an open TIFF file as array of
        /// <see cref="FieldValue"/> objects or <c>null</c> if there is no such tag set and
        /// tag has no default value.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <see cref="GetFieldDefaulted"/> returns the value(s) of a tag or pseudo-tag associated
        /// with the current directory of the opened TIFF file or default value(s) of a tag if a
        /// tag is not defined in the current directory and it has a default value(s). The tag is
        /// identified by <paramref name="tag"/>. The type and number of values returned is
        /// dependent on the tag being requested. You may want to consult
        /// <a href="54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> to become familiar with exact data types and calling
        /// conventions required for each tag supported by the library.
        /// </para>
        /// <para>
        /// A pseudo-tag is a parameter that is used to control the operation of the library but
        /// whose value is not read or written to the underlying file.
        /// </para>
        /// </remarks>
        /// <seealso cref="GetField"/>
        public FieldValue[] GetFieldDefaulted(TiffTag tag)
        {
            TiffDirectory td = m_dir;

            FieldValue[] result = GetField(tag);
            if (result != null)
                return result;

            switch (tag)
            {
                case TiffTag.SUBFILETYPE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_subfiletype);
                    break;
                case TiffTag.BITSPERSAMPLE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_bitspersample);
                    break;
                case TiffTag.THRESHHOLDING:
                    result = new FieldValue[1];
                    result[0].Set(td.td_threshholding);
                    break;
                case TiffTag.FILLORDER:
                    result = new FieldValue[1];
                    result[0].Set(td.td_fillorder);
                    break;
                case TiffTag.ORIENTATION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_orientation);
                    break;
                case TiffTag.SAMPLESPERPIXEL:
                    result = new FieldValue[1];
                    result[0].Set(td.td_samplesperpixel);
                    break;
                case TiffTag.ROWSPERSTRIP:
                    result = new FieldValue[1];
                    result[0].Set(td.td_rowsperstrip);
                    break;
                case TiffTag.MINSAMPLEVALUE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_minsamplevalue);
                    break;
                case TiffTag.MAXSAMPLEVALUE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_maxsamplevalue);
                    break;
                case TiffTag.PLANARCONFIG:
                    result = new FieldValue[1];
                    result[0].Set(td.td_planarconfig);
                    break;
                case TiffTag.RESOLUTIONUNIT:
                    result = new FieldValue[1];
                    result[0].Set(td.td_resolutionunit);
                    break;
                case TiffTag.PREDICTOR:
                    CodecWithPredictor sp = m_currentCodec as CodecWithPredictor;
                    if (sp != null)
                    {
                        result = new FieldValue[1];
                        result[0].Set(sp.GetPredictorValue());
                    }
                    break;
                case TiffTag.DOTRANGE:
                    result = new FieldValue[2];
                    result[0].Set(0);
                    result[1].Set((1 << td.td_bitspersample) - 1);
                    break;
                case TiffTag.INKSET:
                    result = new FieldValue[1];
                    result[0].Set(InkSet.CMYK);
                    break;
                case TiffTag.NUMBEROFINKS:
                    result = new FieldValue[1];
                    result[0].Set(4);
                    break;
                case TiffTag.EXTRASAMPLES:
                    result = new FieldValue[2];
                    result[0].Set(td.td_extrasamples);
                    result[1].Set(td.td_sampleinfo);
                    break;
                case TiffTag.MATTEING:
                    result = new FieldValue[1];
                    result[0].Set((td.td_extrasamples == 1 && td.td_sampleinfo[0] == ExtraSample.ASSOCALPHA));
                    break;
                case TiffTag.TILEDEPTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_tiledepth);
                    break;
                case TiffTag.DATATYPE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_sampleformat - 1);
                    break;
                case TiffTag.SAMPLEFORMAT:
                    result = new FieldValue[1];
                    result[0].Set(td.td_sampleformat);
                    break;
                case TiffTag.IMAGEDEPTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_imagedepth);
                    break;
                case TiffTag.YCBCRCOEFFICIENTS:
                    {
                        // defaults are from CCIR Recommendation 601-1
                        float[] ycbcrcoeffs = new float[3];
                        ycbcrcoeffs[0] = 0.299f;
                        ycbcrcoeffs[1] = 0.587f;
                        ycbcrcoeffs[2] = 0.114f;

                        result = new FieldValue[1];
                        result[0].Set(ycbcrcoeffs);
                        break;
                    }
                case TiffTag.YCBCRSUBSAMPLING:
                    result = new FieldValue[2];
                    result[0].Set(td.td_ycbcrsubsampling[0]);
                    result[1].Set(td.td_ycbcrsubsampling[1]);
                    break;
                case TiffTag.YCBCRPOSITIONING:
                    result = new FieldValue[1];
                    result[0].Set(td.td_ycbcrpositioning);
                    break;
                case TiffTag.WHITEPOINT:
                    {
                        // TIFF 6.0 specification tells that it is no default value for the
                        // WhitePoint, but AdobePhotoshop TIFF Technical Note tells that it
                        // should be CIE D50.
                        float[] whitepoint = new float[2];
                        whitepoint[0] = D50_X0 / (D50_X0 + D50_Y0 + D50_Z0);
                        whitepoint[1] = D50_Y0 / (D50_X0 + D50_Y0 + D50_Z0);

                        result = new FieldValue[1];
                        result[0].Set(whitepoint);
                        break;
                    }
                case TiffTag.TRANSFERFUNCTION:
                    if (td.td_transferfunction[0] == null && !defaultTransferFunction(td))
                    {
                        ErrorExt(this, m_clientdata, m_name, "No space for \"TransferFunction\" tag");
                        return null;
                    }

                    result = new FieldValue[3];
                    result[0].Set(td.td_transferfunction[0]);
                    if (td.td_samplesperpixel - td.td_extrasamples > 1)
                    {
                        result[1].Set(td.td_transferfunction[1]);
                        result[2].Set(td.td_transferfunction[2]);
                    }
                    break;
                case TiffTag.REFERENCEBLACKWHITE:
                    if (td.td_refblackwhite == null)
                        defaultRefBlackWhite(td);

                    result = new FieldValue[1];
                    result[0].Set(td.td_refblackwhite);
                    break;
            }

            return result;
        }

        /// <summary>
        /// Reads the contents of the next TIFF directory in an open TIFF file/stream and makes
        /// it the current directory.
        /// </summary>
        /// <returns><c>true</c> if directory was successfully read; otherwise, <c>false</c> if an
        /// error was encountered, or if there are no more directories to be read.</returns>
        /// <remarks><para>Directories are read sequentially.</para>
        /// <para>Applications only need to call <see cref="ReadDirectory"/> to read multiple
        /// subfiles in a single TIFF file/stream - the first directory in a file/stream is
        /// automatically read when <see cref="Open"/> or <see cref="ClientOpen"/> is called.
        /// </para><para>
        /// The images that have a single uncompressed strip or tile of data are automatically
        /// treated as if they were made up of multiple strips or tiles of approximately 8
        /// kilobytes each. This operation is done only in-memory; it does not alter the contents
        /// of the file/stream. However, the construction of the "chopped strips" is visible to
        /// the application through the number of strips returned by <see cref="NumberOfStrips"/>
        /// or the number of tiles returned by <see cref="NumberOfTiles"/>.</para>
        /// </remarks>
        public bool ReadDirectory()
        {
            const string module = "ReadDirectory";

            m_diroff = m_nextdiroff;
            if (m_diroff == 0)
            {
                // no more directories
                return false;
            }

            // Check whether we have the last offset or bad offset (IFD looping).
            if (!checkDirOffset(m_nextdiroff))
                return false;

            // Cleanup any previous compression state.
            m_currentCodec.Cleanup();
            m_curdir++;
            TiffDirEntry[] dir;
            short dircount = fetchDirectory(m_nextdiroff, out dir, out m_nextdiroff);
            if (dircount == 0)
            {
                ErrorExt(this, m_clientdata, module, "{0}: Failed to read directory at offset {1}", m_name, m_nextdiroff);
                return false;
            }

            // reset before new dir
            m_flags &= ~TiffFlags.BEENWRITING;

            // Setup default value and then make a pass over the fields to check type and tag
            // information, and to extract info required to size data structures. A second pass is
            // made afterwards to read in everthing not taken in the first pass.

            // free any old stuff and reinit
            FreeDirectory();
            setupDefaultDirectory();

            // Electronic Arts writes gray-scale TIFF files without a PlanarConfiguration
            // directory entry. Thus we setup a default value here, even though the TIFF spec says
            // there is no default value.
            SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);

            // Sigh, we must make a separate pass through the directory for the following reason:
            // 
            // We must process the Compression tag in the first pass in order to merge in
            // codec-private tag definitions (otherwise we may get complaints about unknown tags).
            // However, the Compression tag may be dependent on the SamplesPerPixel tag value
            // because older TIFF specs permited Compression to be written as a
            // SamplesPerPixel-count tag entry. Thus if we don't first figure out the correct
            // SamplesPerPixel tag value then we may end up ignoring the Compression tag value
            // because it has an incorrect count value (if the true value of SamplesPerPixel is not 1).
            //
            // It sure would have been nice if Aldus had really thought this stuff through carefully.

            for (int i = 0; i < dircount; i++)
            {
                TiffDirEntry dp = dir[i];
                if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                {
                    short temp = (short)dp.tdir_tag;
                    SwabShort(ref temp);
                    dp.tdir_tag = (TiffTag)(ushort)temp;

                    temp = (short)dp.tdir_type;
                    SwabShort(ref temp);
                    dp.tdir_type = (TiffType)temp;

                    SwabLong(ref dp.tdir_count);
                    SwabUInt(ref dp.tdir_offset);
                }

                if (dp.tdir_tag == TiffTag.SAMPLESPERPIXEL)
                {
                    if (!fetchNormalTag(dir[i]))
                        return false;

                    dp.tdir_tag = TiffTag.IGNORE;
                }
            }

            // First real pass over the directory.
            int fix = 0;
            bool diroutoforderwarning = false;
            bool haveunknowntags = false;
            for (int i = 0; i < dircount; i++)
            {
                if (dir[i].tdir_tag == TiffTag.IGNORE)
                    continue;

                if (fix >= m_nfields)
                    fix = 0;

                // Silicon Beach (at least) writes unordered directory tags (violating the spec).
                // Handle it here, but be obnoxious (maybe they'll fix it?).
                if (dir[i].tdir_tag < m_fieldinfo[fix].Tag)
                {
                    if (!diroutoforderwarning)
                    {
                        WarningExt(this, m_clientdata, module,
                            "{0}: invalid TIFF directory; tags are not sorted in ascending order", m_name);
                        diroutoforderwarning = true;
                    }

                    fix = 0; // O(n^2)
                }

                while (fix < m_nfields && m_fieldinfo[fix].Tag < dir[i].tdir_tag)
                    fix++;

                if (fix >= m_nfields || m_fieldinfo[fix].Tag != dir[i].tdir_tag)
                {
                    // Unknown tag ... we'll deal with it below
                    haveunknowntags = true;
                    continue;
                }

                // null out old tags that we ignore.
                if (m_fieldinfo[fix].Bit == FieldBit.Ignore)
                {
                    dir[i].tdir_tag = TiffTag.IGNORE;
                    continue;
                }

                // Check data type.
                TiffFieldInfo fip = m_fieldinfo[fix];
                while (dir[i].tdir_type != fip.Type && fix < m_nfields)
                {
                    if (fip.Type == TiffType.ANY)
                    {
                        // wildcard
                        break;
                    }

                    fip = m_fieldinfo[++fix];
                    if (fix >= m_nfields || fip.Tag != dir[i].tdir_tag)
                    {
                        WarningExt(this, m_clientdata, module,
                            "{0}: wrong data type {1} for \"{2}\"; tag ignored",
                            m_name, dir[i].tdir_type, m_fieldinfo[fix - 1].Name);

                        dir[i].tdir_tag = TiffTag.IGNORE;
                        continue;
                    }
                }

                // Check count if known in advance.
                if (fip.ReadCount != TiffFieldInfo.Variable &&
                    fip.ReadCount != TiffFieldInfo.Variable2)
                {
                    int expected = fip.ReadCount;
                    if (fip.ReadCount == TiffFieldInfo.Spp)
                        expected = m_dir.td_samplesperpixel;

                    if (!checkDirCount(dir[i], expected))
                    {
                        dir[i].tdir_tag = TiffTag.IGNORE;
                        continue;
                    }
                }

                switch (dir[i].tdir_tag)
                {
                    case TiffTag.COMPRESSION:
                        // The 5.0 spec says the Compression tag has one value,
                        // while earlier specs say it has one value per sample.
                        // Because of this, we accept the tag if one value is supplied.
                        if (dir[i].tdir_count == 1)
                        {
                            int v = extractData(dir[i]);
                            if (!SetField(dir[i].tdir_tag, v))
                                return false;

                            break;
                            // XXX: workaround for broken TIFFs
                        }
                        else if (dir[i].tdir_type == TiffType.LONG)
                        {
                            int v;
                            if (!fetchPerSampleLongs(dir[i], out v) || !SetField(dir[i].tdir_tag, v))
                                return false;
                        }
                        else
                        {
                            short iv;
                            if (!fetchPerSampleShorts(dir[i], out iv) || !SetField(dir[i].tdir_tag, iv))
                                return false;
                        }
                        dir[i].tdir_tag = TiffTag.IGNORE;
                        break;
                    case TiffTag.STRIPOFFSETS:
                    case TiffTag.STRIPBYTECOUNTS:
                    case TiffTag.TILEOFFSETS:
                    case TiffTag.TILEBYTECOUNTS:
                        setFieldBit(fip.Bit);
                        break;
                    case TiffTag.IMAGEWIDTH:
                    case TiffTag.IMAGELENGTH:
                    case TiffTag.IMAGEDEPTH:
                    case TiffTag.TILELENGTH:
                    case TiffTag.TILEWIDTH:
                    case TiffTag.TILEDEPTH:
                    case TiffTag.PLANARCONFIG:
                    case TiffTag.ROWSPERSTRIP:
                    case TiffTag.EXTRASAMPLES:
                        if (!fetchNormalTag(dir[i]))
                            return false;
                        dir[i].tdir_tag = TiffTag.IGNORE;
                        break;
                }
            }

            // If we saw any unknown tags, make an extra pass over the directory to deal with
            // them. This must be done separately because the tags could have become known when we
            // registered a codec after finding the Compression tag. In a correctly-sorted
            // directory there's no problem because Compression will come before any codec-private
            // tags, but if the sorting is wrong that might not hold.
            if (haveunknowntags)
            {
                fix = 0;
                for (int i = 0; i < dircount; i++)
                {
                    if (dir[i].tdir_tag == TiffTag.IGNORE)
                        continue;

                    if (fix >= m_nfields || dir[i].tdir_tag < m_fieldinfo[fix].Tag)
                    {
                        // O(n^2)
                        fix = 0;
                    }

                    while (fix < m_nfields && m_fieldinfo[fix].Tag < dir[i].tdir_tag)
                        fix++;

                    if (fix >= m_nfields || m_fieldinfo[fix].Tag != dir[i].tdir_tag)
                    {
                        Tiff.WarningExt(this, m_clientdata, module,
                            "{0}: unknown field with tag {1} (0x{2:x}) encountered",
                            m_name, (ushort)dir[i].tdir_tag, (ushort)dir[i].tdir_tag);

                        TiffFieldInfo[] arr = new TiffFieldInfo[1];
                        arr[0] = createAnonFieldInfo(dir[i].tdir_tag, dir[i].tdir_type);
                        MergeFieldInfo(arr, 1);

                        fix = 0;
                        while (fix < m_nfields && m_fieldinfo[fix].Tag < dir[i].tdir_tag)
                            fix++;
                    }

                    // Check data type.
                    TiffFieldInfo fip = m_fieldinfo[fix];
                    while (dir[i].tdir_type != fip.Type && fix < m_nfields)
                    {
                        if (fip.Type == TiffType.ANY)
                        {
                            // wildcard
                            break;
                        }

                        fip = m_fieldinfo[++fix];
                        if (fix >= m_nfields || fip.Tag != dir[i].tdir_tag)
                        {
                            Tiff.WarningExt(this, m_clientdata, module,
                                "{0}: wrong data type {1} for \"{2}\"; tag ignored",
                                m_name, dir[i].tdir_type, m_fieldinfo[fix - 1].Name);

                            dir[i].tdir_tag = TiffTag.IGNORE;
                            break;
                        }
                    }
                }
            }

            // XXX: OJPEG hack.
            // If a) compression is OJPEG, b) planarconfig tag says it's separate, c) strip
            // offsets/bytecounts tag are both present and d) both contain exactly one value, then
            // we consistently find that the buggy implementation of the buggy compression scheme
            // matches contig planarconfig best. So we 'fix-up' the tag here
            if ((m_dir.td_compression == Compression.OJPEG) && (m_dir.td_planarconfig == PlanarConfig.SEPARATE))
            {
                int dpIndex = readDirectoryFind(dir, dircount, TiffTag.STRIPOFFSETS);
                if (dpIndex != -1 && dir[dpIndex].tdir_count == 1)
                {
                    dpIndex = readDirectoryFind(dir, dircount, TiffTag.STRIPBYTECOUNTS);
                    if (dpIndex != -1 && dir[dpIndex].tdir_count == 1)
                    {
                        m_dir.td_planarconfig = PlanarConfig.CONTIG;
                        WarningExt(this, m_clientdata, "ReadDirectory",
                            "Planarconfig tag value assumed incorrect, assuming data is contig instead of chunky");
                    }
                }
            }

            // Allocate directory structure and setup defaults.
            if (!fieldSet(FieldBit.ImageDimensions))
            {
                missingRequired("ImageLength");
                return false;
            }

            // Setup appropriate structures (by strip or by tile)
            if (!fieldSet(FieldBit.TileDimensions))
            {
                m_dir.td_nstrips = NumberOfStrips();
                m_dir.td_tilewidth = m_dir.td_imagewidth;
                m_dir.td_tilelength = m_dir.td_rowsperstrip;
                m_dir.td_tiledepth = m_dir.td_imagedepth;
                m_flags &= ~TiffFlags.ISTILED;
            }
            else
            {
                m_dir.td_nstrips = NumberOfTiles();
                m_flags |= TiffFlags.ISTILED;
            }

            if (m_dir.td_nstrips == 0)
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: cannot handle zero number of {1}", m_name, IsTiled() ? "tiles" : "strips");
                return false;
            }

            m_dir.td_stripsperimage = m_dir.td_nstrips;
            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                m_dir.td_stripsperimage /= m_dir.td_samplesperpixel;

            if (!fieldSet(FieldBit.StripOffsets))
            {
                if ((m_dir.td_compression == Compression.OJPEG) && !IsTiled() && (m_dir.td_nstrips == 1))
                {
                    // XXX: OJPEG hack.
                    // If a) compression is OJPEG, b) it's not a tiled TIFF, and c) the number of
                    // strips is 1, then we tolerate the absence of stripoffsets tag, because,
                    // presumably, all required data is in the JpegInterchangeFormat stream.
                    setFieldBit(FieldBit.StripOffsets);
                }
                else
                {
                    missingRequired(IsTiled() ? "TileOffsets" : "StripOffsets");
                    return false;
                }
            }

            // Second pass: extract other information.
            for (int i = 0; i < dircount; i++)
            {
                if (dir[i].tdir_tag == TiffTag.IGNORE)
                    continue;

                switch (dir[i].tdir_tag)
                {
                    case TiffTag.MINSAMPLEVALUE:
                    case TiffTag.MAXSAMPLEVALUE:
                    case TiffTag.BITSPERSAMPLE:
                    case TiffTag.DATATYPE:
                    case TiffTag.SAMPLEFORMAT:
                        // The 5.0 spec says the Compression tag has one value, while earlier
                        // specs say it has one value per sample. Because of this, we accept the
                        // tag if one value is supplied.
                        //
                        // The MinSampleValue, MaxSampleValue, BitsPerSample DataType and
                        // SampleFormat tags are supposed to be written as one value/sample, but
                        // some vendors incorrectly write one value only - so we accept that as
                        // well (yech). Other vendors write correct value for NumberOfSamples, but
                        // incorrect one for BitsPerSample and friends, and we will read this too.
                        if (dir[i].tdir_count == 1)
                        {
                            int v = extractData(dir[i]);
                            if (!SetField(dir[i].tdir_tag, v))
                                return false;
                            // XXX: workaround for broken TIFFs
                        }
                        else if (dir[i].tdir_tag == TiffTag.BITSPERSAMPLE && dir[i].tdir_type == TiffType.LONG)
                        {
                            int v;
                            if (!fetchPerSampleLongs(dir[i], out v) || !SetField(dir[i].tdir_tag, v))
                                return false;
                        }
                        else
                        {
                            short iv;
                            if (!fetchPerSampleShorts(dir[i], out iv) || !SetField(dir[i].tdir_tag, iv))
                                return false;
                        }
                        break;
                    case TiffTag.SMINSAMPLEVALUE:
                    case TiffTag.SMAXSAMPLEVALUE:
                        double dv;
                        if (!fetchPerSampleAnys(dir[i], out dv) || !SetField(dir[i].tdir_tag, dv))
                            return false;
                        break;
                    case TiffTag.STRIPOFFSETS:
                    case TiffTag.TILEOFFSETS:
                        if (!fetchStripThing(dir[i], m_dir.td_nstrips, ref m_dir.td_stripoffset))
                            return false;
                        break;
                    case TiffTag.STRIPBYTECOUNTS:
                    case TiffTag.TILEBYTECOUNTS:
                        if (!fetchStripThing(dir[i], m_dir.td_nstrips, ref m_dir.td_stripbytecount))
                            return false;
                        break;
                    case TiffTag.COLORMAP:
                    case TiffTag.TRANSFERFUNCTION:
                        {
                            // TransferFunction can have either 1x or 3x data values;
                            // Colormap can have only 3x items.
                            int v = 1 << m_dir.td_bitspersample;
                            if (dir[i].tdir_tag == TiffTag.COLORMAP || dir[i].tdir_count != v)
                            {
                                if (!checkDirCount(dir[i], 3 * v))
                                    break;
                            }

                            byte[] cp = new byte[dir[i].tdir_count * sizeof(short)];
                            if (fetchData(dir[i], cp) != 0)
                            {
                                int c = 1 << m_dir.td_bitspersample;
                                if (dir[i].tdir_count == c)
                                {
                                    // This deals with there being only one array to apply to all samples.
                                    short[] u = ByteArrayToShorts(cp, 0, dir[i].tdir_count * sizeof(short));
                                    SetField(dir[i].tdir_tag, u, u, u);
                                }
                                else
                                {
                                    v *= sizeof(short);
                                    short[] u0 = ByteArrayToShorts(cp, 0, v);
                                    short[] u1 = ByteArrayToShorts(cp, v, v);
                                    short[] u2 = ByteArrayToShorts(cp, 2 * v, v);
                                    SetField(dir[i].tdir_tag, u0, u1, u2);
                                }
                            }
                            break;
                        }
                    case TiffTag.PAGENUMBER:
                    case TiffTag.HALFTONEHINTS:
                    case TiffTag.YCBCRSUBSAMPLING:
                    case TiffTag.DOTRANGE:
                        fetchShortPair(dir[i]);
                        break;
                    case TiffTag.REFERENCEBLACKWHITE:
                        fetchRefBlackWhite(dir[i]);
                        break;
                    // BEGIN REV 4.0 COMPATIBILITY
                    case TiffTag.OSUBFILETYPE:
                        FileType ft = 0;
                        switch ((OFileType)extractData(dir[i]))
                        {
                            case OFileType.REDUCEDIMAGE:
                                ft = FileType.REDUCEDIMAGE;
                                break;
                            case OFileType.PAGE:
                                ft = FileType.PAGE;
                                break;
                        }

                        if (ft != 0)
                            SetField(TiffTag.SUBFILETYPE, ft);

                        break;
                    // END REV 4.0 COMPATIBILITY
                    default:
                        fetchNormalTag(dir[i]);
                        break;
                }
            }

            // OJPEG hack:
            // - If a) compression is OJPEG, and b) photometric tag is missing, then we
            // consistently find that photometric should be YCbCr
            // - If a) compression is OJPEG, and b) photometric tag says it's RGB, then we
            // consistently find that the buggy implementation of the buggy compression scheme
            // matches photometric YCbCr instead.
            // - If a) compression is OJPEG, and b) bitspersample tag is missing, then we
            // consistently find bitspersample should be 8.
            // - If a) compression is OJPEG, b) samplesperpixel tag is missing, and c) photometric
            // is RGB or YCbCr, then we consistently find samplesperpixel should be 3
            // - If a) compression is OJPEG, b) samplesperpixel tag is missing, and c) photometric
            // is MINISWHITE or MINISBLACK, then we consistently find samplesperpixel should be 3
            if (m_dir.td_compression == Compression.OJPEG)
            {
                if (!fieldSet(FieldBit.Photometric))
                {
                    WarningExt(this, m_clientdata,
                        "ReadDirectory", "Photometric tag is missing, assuming data is YCbCr");

                    if (!SetField(TiffTag.PHOTOMETRIC, Photometric.YCBCR))
                        return false;
                }
                else if (m_dir.td_photometric == Photometric.RGB)
                {
                    m_dir.td_photometric = Photometric.YCBCR;
                    WarningExt(this, m_clientdata, "ReadDirectory",
                        "Photometric tag value assumed incorrect, assuming data is YCbCr instead of RGB");
                }

                if (!fieldSet(FieldBit.BitsPerSample))
                {
                    WarningExt(this, m_clientdata, "ReadDirectory",
                        "BitsPerSample tag is missing, assuming 8 bits per sample");

                    if (!SetField(TiffTag.BITSPERSAMPLE, 8))
                        return false;
                }

                if (!fieldSet(FieldBit.SamplesPerPixel))
                {
                    if ((m_dir.td_photometric == Photometric.RGB) ||
                        (m_dir.td_photometric == Photometric.YCBCR))
                    {
                        WarningExt(this, m_clientdata, "ReadDirectory",
                            "SamplesPerPixel tag is missing, assuming correct SamplesPerPixel value is 3");
                        
                        if (!SetField(TiffTag.SAMPLESPERPIXEL, 3))
                            return false;
                    }
                    else if ((m_dir.td_photometric == Photometric.MINISWHITE) ||
                        (m_dir.td_photometric == Photometric.MINISBLACK))
                    {
                        WarningExt(this, m_clientdata, "ReadDirectory",
                            "SamplesPerPixel tag is missing, assuming correct SamplesPerPixel value is 1");
                        if (!SetField(TiffTag.SAMPLESPERPIXEL, 1))
                            return false;
                    }
                }
            }

            // Verify Palette image has a Colormap.
            if (m_dir.td_photometric == Photometric.PALETTE && !fieldSet(FieldBit.ColorMap))
            {
                missingRequired("Colormap");
                return false;
            }

            // OJPEG hack:
            // We do no further messing with strip/tile offsets/bytecounts in OJPEG TIFFs
            if (m_dir.td_compression != Compression.OJPEG)
            {
                // Attempt to deal with a missing StripByteCounts tag.
                if (!fieldSet(FieldBit.StripByteCounts))
                {
                    // Some manufacturers violate the spec by not giving the size of the strips.
                    // In this case, assume there is one uncompressed strip of data.
                    if ((m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_nstrips > 1) ||
                        (m_dir.td_planarconfig == PlanarConfig.SEPARATE && m_dir.td_nstrips != m_dir.td_samplesperpixel))
                    {
                        missingRequired("StripByteCounts");
                        return false;
                    }

                    WarningExt(this, m_clientdata, module,
                        "{0}: TIFF directory is missing required \"{1}\" field, calculating from imagelength",
                        m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);

                    if (!estimateStripByteCounts(dir, dircount))
                        return false;
                }
                else if (m_dir.td_nstrips == 1 && m_dir.td_stripoffset[0] != 0 && byteCountLooksBad(m_dir))
                {
                    // XXX: Plexus (and others) sometimes give a value of zero for a tag when
                    // they don't know what the correct value is! Try and handle the simple case
                    // of estimating the size of a one strip image.
                    WarningExt(this, m_clientdata, module,
                        "{0}: Bogus \"{1}\" field, ignoring and calculating from imagelength",
                        m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);

                    if (!estimateStripByteCounts(dir, dircount))
                        return false;
                }
                else if (m_dir.td_planarconfig == PlanarConfig.CONTIG && m_dir.td_nstrips > 2 &&
                    m_dir.td_compression == Compression.NONE &&
                    m_dir.td_stripbytecount[0] != m_dir.td_stripbytecount[1])
                {
                    // XXX: Some vendors fill StripByteCount array with absolutely wrong values
                    // (it can be equal to StripOffset array, for example). Catch this case here.
                    WarningExt(this, m_clientdata, module,
                        "{0}: Wrong \"{1}\" field, ignoring and calculating from imagelength",
                        m_name, FieldWithTag(TiffTag.STRIPBYTECOUNTS).Name);

                    if (!estimateStripByteCounts(dir, dircount))
                        return false;
                }
            }

            dir = null;

            if (!fieldSet(FieldBit.MaxSampleValue))
                m_dir.td_maxsamplevalue = (short)((1 << m_dir.td_bitspersample) - 1);

            // Setup default compression scheme.

            // XXX: We can optimize checking for the strip bounds using the sorted bytecounts
            // array. See also comments for appendToStrip() function.
            if (m_dir.td_nstrips > 1)
            {
                m_dir.td_stripbytecountsorted = true;
                for (int strip = 1; strip < m_dir.td_nstrips; strip++)
                {
                    if (m_dir.td_stripoffset[strip - 1] > m_dir.td_stripoffset[strip])
                    {
                        m_dir.td_stripbytecountsorted = false;
                        break;
                    }
                }
            }

            if (!fieldSet(FieldBit.Compression))
                SetField(TiffTag.COMPRESSION, Compression.NONE);

            // Some manufacturers make life difficult by writing large amounts of uncompressed
            // data as a single strip. This is contrary to the recommendations of the spec. The
            // following makes an attempt at breaking such images into strips closer to the
            // recommended 8k bytes. A side effect, however, is that the RowsPerStrip tag value
            // may be changed.
            if (m_dir.td_nstrips == 1 && m_dir.td_compression == Compression.NONE &&
                (m_flags & TiffFlags.STRIPCHOP) == TiffFlags.STRIPCHOP &&
                (m_flags & TiffFlags.ISTILED) != TiffFlags.ISTILED)
            {
                chopUpSingleUncompressedStrip();
            }

            // Reinitialize i/o since we are starting on a new directory.
            m_row = -1;
            m_curstrip = -1;
            m_col = -1;
            m_curtile = -1;
            m_tilesize = -1;

            m_scanlinesize = ScanlineSize();
            if (m_scanlinesize == 0)
            {
                ErrorExt(this, m_clientdata, module, "{0}: cannot handle zero scanline size", m_name);
                return false;
            }

            if (IsTiled())
            {
                m_tilesize = TileSize();
                if (m_tilesize == 0)
                {
                    ErrorExt(this, m_clientdata, module, "{0}: cannot handle zero tile size", m_name);
                    return false;
                }
            }
            else
            {
                if (StripSize() == 0)
                {
                    ErrorExt(this, m_clientdata, module, "{0}: cannot handle zero strip size", m_name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads a custom directory from the arbitrary offset within file/stream.
        /// </summary>
        /// <param name="offset">The directory offset.</param>
        /// <param name="info">The array of <see cref="TiffFieldInfo"/> objects to merge to
        /// existing field information.</param>
        /// <param name="count">The number of items to use from
        /// the <paramref name="info"/> array.</param>
        /// <returns><c>true</c> if a custom directory was read successfully;
        /// otherwise, <c>false</c></returns>
        public bool ReadCustomDirectory(long offset, TiffFieldInfo[] info, int count)
        {
            const string module = "ReadCustomDirectory";

            setupFieldInfo(info, count);

            uint dummyNextDirOff;
            TiffDirEntry[] dir;
            short dircount = fetchDirectory((uint)offset, out dir, out dummyNextDirOff);
            if (dircount == 0)
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Failed to read custom directory at offset {1}", m_name, offset);
                return false;
            }

            FreeDirectory();
            m_dir = new TiffDirectory();

            int fix = 0;
            for (short i = 0; i < dircount; i++)
            {
                if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                {
                    short temp = (short)dir[i].tdir_tag;
                    SwabShort(ref temp);
                    dir[i].tdir_tag = (TiffTag)(ushort)temp;

                    temp = (short)dir[i].tdir_type;
                    SwabShort(ref temp);
                    dir[i].tdir_type = (TiffType)temp;

                    SwabLong(ref dir[i].tdir_count);
                    SwabUInt(ref dir[i].tdir_offset);
                }

                if (fix >= m_nfields || dir[i].tdir_tag == TiffTag.IGNORE)
                    continue;

                while (fix < m_nfields && m_fieldinfo[fix].Tag < dir[i].tdir_tag)
                    fix++;

                if (fix >= m_nfields || m_fieldinfo[fix].Tag != dir[i].tdir_tag)
                {
                    WarningExt(this, m_clientdata, module,
                        "{0}: unknown field with tag {1} (0x{2:x}) encountered",
                        m_name, (ushort)dir[i].tdir_tag, (ushort)dir[i].tdir_tag);

                    TiffFieldInfo[] arr = new TiffFieldInfo[1];
                    arr[0] = createAnonFieldInfo(dir[i].tdir_tag, dir[i].tdir_type);
                    MergeFieldInfo(arr, 1);

                    fix = 0;
                    while (fix < m_nfields && m_fieldinfo[fix].Tag < dir[i].tdir_tag)
                        fix++;
                }

                // null out old tags that we ignore.
                if (m_fieldinfo[fix].Bit == FieldBit.Ignore)
                {
                    dir[i].tdir_tag = TiffTag.IGNORE;
                    continue;
                }

                // Check data type.
                TiffFieldInfo fip = m_fieldinfo[fix];
                while (dir[i].tdir_type != fip.Type && fix < m_nfields)
                {
                    if (fip.Type == TiffType.ANY)
                    {
                        // wildcard
                        break;
                    }

                    fip = m_fieldinfo[++fix];
                    if (fix >= m_nfields || fip.Tag != dir[i].tdir_tag)
                    {
                        WarningExt(this, m_clientdata, module,
                            "{0}: wrong data type {1} for \"{2}\"; tag ignored",
                            m_name, dir[i].tdir_type, m_fieldinfo[fix - 1].Name);

                        dir[i].tdir_tag = TiffTag.IGNORE;
                        continue;
                    }
                }

                // Check count if known in advance.
                if (fip.ReadCount != TiffFieldInfo.Variable &&
                    fip.ReadCount != TiffFieldInfo.Variable2)
                {
                    int expected = fip.ReadCount;
                    if (fip.ReadCount == TiffFieldInfo.Spp)
                        expected = m_dir.td_samplesperpixel;

                    if (!checkDirCount(dir[i], expected))
                    {
                        dir[i].tdir_tag = TiffTag.IGNORE;
                        continue;
                    }
                }

                // EXIF tags which need to be specifically processed.
                switch (dir[i].tdir_tag)
                {
                    case TiffTag.EXIF_SUBJECTDISTANCE:
                        fetchSubjectDistance(dir[i]);
                        break;
                    default:
                        fetchNormalTag(dir[i]);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Reads an EXIF directory from the given offset within file/stream.
        /// </summary>
        /// <param name="offset">The directory offset.</param>
        /// <returns><c>true</c> if an EXIF directory was read successfully; 
        /// otherwise, <c>false</c></returns>
        public bool ReadEXIFDirectory(long offset)
        {
            int exifFieldInfoCount;
            TiffFieldInfo[] exifFieldInfo = getExifFieldInfo(out exifFieldInfoCount);
            return ReadCustomDirectory(offset, exifFieldInfo, exifFieldInfoCount);
        }

        /// <summary>
        /// Calculates the size in bytes of a row of data as it would be returned in a call to
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadScanline"/>, or as it would be
        /// expected in a call to <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/>.
        /// </summary>
        /// <returns>The size in bytes of a row of data.</returns>
        /// <remarks><b>ScanlineSize</b> calculates size for one sample plane only. Please use
        /// <see cref="RasterScanlineSize"/> if you want to get size in bytes of a complete
        /// decoded and packed raster scanline.</remarks>
        /// <seealso cref="RasterScanlineSize"/>
        public int ScanlineSize()
        {
            int scanline;
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
            {
                if (m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
                {
                    FieldValue[] result = GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
                    short ycbcrsubsampling0 = result[0].ToShort();

                    if (ycbcrsubsampling0 == 0)
                    {
                        ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
                        return 0;
                    }

                    scanline = roundUp(m_dir.td_imagewidth, ycbcrsubsampling0);
                    scanline = howMany8(multiply(scanline, m_dir.td_bitspersample, "ScanlineSize"));
                    return summarize(scanline, multiply(2, scanline / ycbcrsubsampling0, "VStripSize"), "VStripSize");
                }
                else
                {
                    scanline = multiply(m_dir.td_imagewidth, m_dir.td_samplesperpixel, "ScanlineSize");
                }
            }
            else
            {
                scanline = m_dir.td_imagewidth;
            }

            return howMany8(multiply(scanline, m_dir.td_bitspersample, "ScanlineSize"));
        }

        /// <summary>
        /// Calculates the size in bytes of a complete decoded and packed raster scanline.
        /// </summary>
        /// <returns>The size in bytes of a complete decoded and packed raster scanline.</returns>
        /// <remarks>The value returned by <b>RasterScanlineSize</b> may be different from the
        /// value returned by <see cref="ScanlineSize"/> if data is stored as separate
        /// planes (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// </remarks>
        public int RasterScanlineSize()
        {
            int scanline = multiply(m_dir.td_bitspersample, m_dir.td_imagewidth, "RasterScanlineSize");
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
            {
                scanline = multiply(scanline, m_dir.td_samplesperpixel, "RasterScanlineSize");
                return howMany8(scanline);
            }

            return multiply(howMany8(scanline), m_dir.td_samplesperpixel, "RasterScanlineSize");
        }

        /// <summary>
        /// Computes the number of rows for a reasonable-sized strip according to the current
        /// settings of the <see cref="TiffTag.IMAGEWIDTH"/>, <see cref="TiffTag.BITSPERSAMPLE"/>
        /// and <see cref="TiffTag.SAMPLESPERPIXEL"/> tags and any compression-specific requirements.
        /// </summary>
        /// <param name="estimate">The esimated value (may be zero).</param>
        /// <returns>The number of rows for a reasonable-sized strip according to the current
        /// tag settings and compression-specific requirements.</returns>
        /// <remarks>If the <paramref name="estimate"/> parameter is non-zero, then it is taken
        /// as an estimate of the desired strip size and adjusted according to any
        /// compression-specific requirements. The value returned by <b>DefaultStripSize</b> is
        /// typically used to define the <see cref="TiffTag.ROWSPERSTRIP"/> tag. If there is no
        /// any unusual requirements <b>DefaultStripSize</b> tries to create strips that have
        /// approximately 8 kilobytes of uncompressed data.</remarks>
        public int DefaultStripSize(int estimate)
        {
            return m_currentCodec.DefStripSize(estimate);
        }

        /// <summary>
        /// Computes the number of bytes in a row-aligned strip.
        /// </summary>
        /// <returns>The number of bytes in a row-aligned strip</returns>
        /// <remarks>
        /// <para>
        /// <b>StripSize</b> returns the equivalent size for a strip of data as it would be
        /// returned in a call to <see cref="ReadEncodedStrip"/> or as it would be expected in a
        /// call to <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteEncodedStrip"/>.
        /// </para><para>
        /// If the value of the field corresponding to <see cref="TiffTag.ROWSPERSTRIP"/> is
        /// larger than the recorded <see cref="TiffTag.IMAGELENGTH"/>, then the strip size is
        /// truncated to reflect the actual space required to hold the strip.</para>
        /// </remarks>
        public int StripSize()
        {
            int rps = m_dir.td_rowsperstrip;
            if (rps > m_dir.td_imagelength)
                rps = m_dir.td_imagelength;

            return VStripSize(rps);
        }

        /// <summary>
        /// Computes the number of bytes in a row-aligned strip with specified number of rows.
        /// </summary>
        /// <param name="rowCount">The number of rows in a strip.</param>
        /// <returns>
        /// The number of bytes in a row-aligned strip with specified number of rows.</returns>
        public int VStripSize(int rowCount)
        {
            if (rowCount == -1)
                rowCount = m_dir.td_imagelength;

            if (m_dir.td_planarconfig == PlanarConfig.CONTIG &&
                m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
            {
                // Packed YCbCr data contain one Cb+Cr for every
                // HorizontalSampling * VerticalSampling Y values.
                // Must also roundup width and height when calculating since images that are not
                // a multiple of the horizontal/vertical subsampling area include YCbCr data for
                // the extended image.
                FieldValue[] result = GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
                short ycbcrsubsampling0 = result[0].ToShort();
                short ycbcrsubsampling1 = result[1].ToShort();

                int samplingarea = ycbcrsubsampling0 * ycbcrsubsampling1;
                if (samplingarea == 0)
                {
                    ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
                    return 0;
                }

                int w = roundUp(m_dir.td_imagewidth, ycbcrsubsampling0);
                int scanline = howMany8(multiply(w, m_dir.td_bitspersample, "VStripSize"));
                rowCount = roundUp(rowCount, ycbcrsubsampling1);
                // NB: don't need howMany here 'cuz everything is rounded
                scanline = multiply(rowCount, scanline, "VStripSize");
                return summarize(scanline, multiply(2, scanline / samplingarea, "VStripSize"), "VStripSize");
            }

            return multiply(rowCount, ScanlineSize(), "VStripSize");
        }

        /// <summary>
        /// Computes the number of bytes in a raw (i.e. not decoded) strip.
        /// </summary>
        /// <param name="strip">The zero-based index of a strip.</param>
        /// <returns>The number of bytes in a raw strip.</returns>
        public long RawStripSize(int strip)
        {
            long bytecount = m_dir.td_stripbytecount[strip];
            if (bytecount <= 0)
            {
                ErrorExt(this, m_clientdata, m_name,
                    "{0}: Invalid strip byte count, strip {1}", bytecount, strip);
                bytecount = -1;
            }

            return bytecount;
        }

        /// <summary>
        /// Computes which strip contains the specified coordinates (row, plane).
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="plane">The sample plane.</param>
        /// <returns>The number of the strip that contains the specified coordinates.</returns>
        /// <remarks>
        /// A valid strip number is always returned; out-of-range coordinate values are clamped to
        /// the bounds of the image. The <paramref name="row"/> parameter is always used in
        /// calculating a strip. The <paramref name="plane"/> parameter is used only if data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// </remarks>
        public int ComputeStrip(int row, short plane)
        {
            int strip = 0;
            if (m_dir.td_rowsperstrip != -1)
                strip = row / m_dir.td_rowsperstrip;

            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
            {
                if (plane >= m_dir.td_samplesperpixel)
                {
                    ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel);
                    return 0;
                }

                strip += plane * m_dir.td_stripsperimage;
            }

            return strip;
        }

        /// <summary>
        /// Retrives the number of strips in the image.
        /// </summary>
        /// <returns>The number of strips in the image.</returns>
        public int NumberOfStrips()
        {
            int nstrips = (m_dir.td_rowsperstrip == -1 ? 1 : howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip));
            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                nstrips = multiply(nstrips, m_dir.td_samplesperpixel, "NumberOfStrips");

            return nstrips;
        }

        /// <summary>
        /// Computes the pixel width and height of a reasonable-sized tile suitable for setting
        /// up the <see cref="TiffTag.TILEWIDTH"/> and <see cref="TiffTag.TILELENGTH"/> tags.
        /// </summary>
        /// <param name="width">The proposed tile width upon the call / tile width to use
        /// after the call.</param>
        /// <param name="height">The proposed tile height upon the call / tile height to use
        /// after the call.</param>
        /// <remarks>If the <paramref name="width"/> and <paramref name="height"/> values passed
        /// in are non-zero, then they are adjusted to reflect any compression-specific
        /// requirements. The returned width and height are constrained to be a multiple of
        /// 16 pixels to conform with the TIFF specification.</remarks>
        public void DefaultTileSize(ref int width, ref int height)
        {
            m_currentCodec.DefTileSize(ref width, ref height);
        }

        /// <summary>
        /// Compute the number of bytes in a row-aligned tile.
        /// </summary>
        /// <returns>The number of bytes in a row-aligned tile.</returns>
        /// <remarks><b>TileSize</b> returns the equivalent size for a tile of data as it would be
        /// returned in a call to <see cref="ReadTile"/> or as it would be expected in a
        /// call to <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteTile"/>.
        /// </remarks>
        public int TileSize()
        {
            return VTileSize(m_dir.td_tilelength);
        }

        /// <summary>
        /// Computes the number of bytes in a row-aligned tile with specified number of rows.
        /// </summary>
        /// <param name="rowCount">The number of rows in a tile.</param>
        /// <returns>
        /// The number of bytes in a row-aligned tile with specified number of rows.</returns>
        public int VTileSize(int rowCount)
        {
            if (m_dir.td_tilelength == 0 || m_dir.td_tilewidth == 0 || m_dir.td_tiledepth == 0)
                return 0;

            int tilesize;
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG &&
                m_dir.td_photometric == Photometric.YCBCR && !IsUpSampled())
            {
                // Packed YCbCr data contain one Cb+Cr for every
                // HorizontalSampling * VerticalSampling Y values.
                // Must also roundup width and height when calculating since images that are not a
                // multiple of the horizontal/vertical subsampling area include YCbCr data for
                // the extended image.
                int w = roundUp(m_dir.td_tilewidth, m_dir.td_ycbcrsubsampling[0]);
                int rowsize = howMany8(multiply(w, m_dir.td_bitspersample, "VTileSize"));
                int samplingarea = m_dir.td_ycbcrsubsampling[0] * m_dir.td_ycbcrsubsampling[1];
                if (samplingarea == 0)
                {
                    ErrorExt(this, m_clientdata, m_name, "Invalid YCbCr subsampling");
                    return 0;
                }

                rowCount = roundUp(rowCount, m_dir.td_ycbcrsubsampling[1]);
                // NB: don't need howMany here 'cuz everything is rounded
                tilesize = multiply(rowCount, rowsize, "VTileSize");
                tilesize = summarize(tilesize, multiply(2, tilesize / samplingarea, "VTileSize"), "VTileSize");
            }
            else
            {
                tilesize = multiply(rowCount, TileRowSize(), "VTileSize");
            }

            return multiply(tilesize, m_dir.td_tiledepth, "VTileSize");
        }

        /// <summary>
        /// Computes the number of bytes in a raw (i.e. not decoded) tile.
        /// </summary>
        /// <param name="tile">The zero-based index of a tile.</param>
        /// <returns>The number of bytes in a raw tile.</returns>
        public long RawTileSize(int tile)
        {
            // yes, one method for raw tile and strip sizes
            return RawStripSize(tile);
        }

        /// <summary>
        /// Compute the number of bytes in each row of a tile.
        /// </summary>
        /// <returns>The number of bytes in each row of a tile.</returns>
        public int TileRowSize()
        {
            if (m_dir.td_tilelength == 0 || m_dir.td_tilewidth == 0)
                return 0;

            int rowsize = multiply(m_dir.td_bitspersample, m_dir.td_tilewidth, "TileRowSize");
            if (m_dir.td_planarconfig == PlanarConfig.CONTIG)
                rowsize = multiply(rowsize, m_dir.td_samplesperpixel, "TileRowSize");

            return howMany8(rowsize);
        }

        /// <summary>
        /// Computes which tile contains the specified coordinates (x, y, z, plane).
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        /// <param name="plane">The sample plane.</param>
        /// <returns>The number of the tile that contains the specified coordinates.</returns>
        /// <remarks>
        /// A valid tile number is always returned; out-of-range coordinate values are
        /// clamped to the bounds of the image. The <paramref name="x"/> and <paramref name="y"/>
        /// parameters are always used in calculating a tile. The <paramref name="z"/> parameter
        /// is used if the image is deeper than 1 slice (<see cref="TiffTag.IMAGEDEPTH"/> &gt; 1).
        /// The <paramref name="plane"/> parameter is used only if data are organized in separate
        /// planes (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// </remarks>
        public int ComputeTile(int x, int y, int z, short plane)
        {
            if (m_dir.td_imagedepth == 1)
                z = 0;

            int dx = m_dir.td_tilewidth;
            if (dx == -1)
                dx = m_dir.td_imagewidth;

            int dy = m_dir.td_tilelength;
            if (dy == -1)
                dy = m_dir.td_imagelength;

            int dz = m_dir.td_tiledepth;
            if (dz == -1)
                dz = m_dir.td_imagedepth;

            int tile = 1;
            if (dx != 0 && dy != 0 && dz != 0)
            {
                int xpt = howMany(m_dir.td_imagewidth, dx);
                int ypt = howMany(m_dir.td_imagelength, dy);
                int zpt = howMany(m_dir.td_imagedepth, dz);

                if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                    tile = (xpt * ypt * zpt) * plane + (xpt * ypt) * (z / dz) + xpt * (y / dy) + x / dx;
                else
                    tile = (xpt * ypt) * (z / dz) + xpt * (y / dy) + x / dx;
            }

            return tile;
        }

        /// <summary>
        /// Checks whether the specified (x, y, z, plane) coordinates are within the bounds of
        /// the image.
        /// </summary>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        /// <param name="z">The z-coordinate.</param>
        /// <param name="plane">The sample plane.</param>
        /// <returns><c>true</c> if the specified coordinates are within the bounds of the image;
        /// otherwise, <c>false</c>.</returns>
        /// <remarks>The <paramref name="x"/> parameter is checked against the value of the
        /// <see cref="TiffTag.IMAGEWIDTH"/> tag. The <paramref name="y"/> parameter is checked
        /// against the value of the <see cref="TiffTag.IMAGELENGTH"/> tag. The <paramref name="z"/>
        /// parameter is checked against the value of the <see cref="TiffTag.IMAGEDEPTH"/> tag
        /// (if defined). The <paramref name="plane"/> parameter is checked against the value of
        /// the <see cref="TiffTag.SAMPLESPERPIXEL"/> tag if the data are organized in separate
        /// planes.</remarks>
        public bool CheckTile(int x, int y, int z, short plane)
        {
            if (x >= m_dir.td_imagewidth)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Col out of range, max {1}", x, m_dir.td_imagewidth - 1);
                return false;
            }

            if (y >= m_dir.td_imagelength)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Row out of range, max {1}", y, m_dir.td_imagelength - 1);
                return false;
            }

            if (z >= m_dir.td_imagedepth)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Depth out of range, max {1}", z, m_dir.td_imagedepth - 1);
                return false;
            }

            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE && plane >= m_dir.td_samplesperpixel)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel - 1);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrives the number of tiles in the image.
        /// </summary>
        /// <returns>The number of tiles in the image.</returns>
        public int NumberOfTiles()
        {
            int dx = m_dir.td_tilewidth;
            if (dx == -1)
                dx = m_dir.td_imagewidth;

            int dy = m_dir.td_tilelength;
            if (dy == -1)
                dy = m_dir.td_imagelength;

            int dz = m_dir.td_tiledepth;
            if (dz == -1)
                dz = m_dir.td_imagedepth;

            int ntiles;
            if (dx == 0 || dy == 0 || dz == 0)
            {
                ntiles = 0;
            }
            else
            {
                ntiles = multiply(
                    multiply(howMany(m_dir.td_imagewidth, dx), howMany(m_dir.td_imagelength, dy), "NumberOfTiles"),
                    howMany(m_dir.td_imagedepth, dz), "NumberOfTiles");
            }
            
            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                ntiles = multiply(ntiles, m_dir.td_samplesperpixel, "NumberOfTiles");

            return ntiles;
        }

        /// <summary>
        /// Returns the custom client data associated with this <see cref="Tiff"/>.
        /// </summary>
        /// <returns>The custom client data associated with this <see cref="Tiff"/>.</returns>
        public object Clientdata()
        {
            return m_clientdata;
        }

        /// <summary>
        /// Asscociates a custom data with this <see cref="Tiff"/>.
        /// </summary>
        /// <param name="data">The data to associate.</param>
        /// <returns>The previously associated data.</returns>
        public object SetClientdata(object data)
        {
            object prev = m_clientdata;
            m_clientdata = data;
            return prev;
        }

        /// <summary>
        /// Gets the mode with which the underlying file or stream was opened.
        /// </summary>
        /// <returns>The mode with which the underlying file or stream was opened.</returns>
        public int GetMode()
        {
            return m_mode;
        }

        /// <summary>
        /// Sets the new mode for the underlying file or stream.
        /// </summary>
        /// <param name="mode">The new mode for the underlying file or stream.</param>
        /// <returns>The previous mode with which the underlying file or stream was opened.</returns>
        public int SetMode(int mode)
        {
            int prevMode = m_mode;
            m_mode = mode;
            return prevMode;
        }

        /// <summary>
        /// Gets the value indicating whether the image data of this <see cref="Tiff"/> has a
        /// tiled organization.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the image data of this <see cref="Tiff"/> has a tiled organization or
        /// <c>false</c> if the image data of this <see cref="Tiff"/> is organized in strips.
        /// </returns>
        public bool IsTiled()
        {
            return ((m_flags & TiffFlags.ISTILED) == TiffFlags.ISTILED);
        }

        /// <summary>
        /// Gets the value indicating whether the image data was in a different byte-order than
        /// the host computer.
        /// </summary>
        /// <returns><c>true</c> if the image data was in a different byte-order than the host
        /// computer or <c>false</c> if the TIFF file/stream and local host byte-orders are the
        /// same.</returns>
        /// <remarks><para>
        /// Note that <see cref="ReadTile"/>, <see cref="ReadEncodedTile"/>,
        /// <see cref="ReadEncodedStrip"/> and
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadScanline"/> methods already
        /// normally perform byte swapping to local host order if needed.
        /// </para><para>
        /// Also note that <see cref="ReadRawTile"/> and <see cref="ReadRawStrip"/> do not
        /// perform byte swapping to local host order.
        /// </para></remarks>
        public bool IsByteSwapped()
        {
            return ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB);
        }

        /// <summary>
        /// Gets the value indicating whether the image data returned through the read interface
        /// methods is being up-sampled.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the data is returned up-sampled; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>The value returned by this method can be useful to applications that want to
        /// calculate I/O buffer sizes to reflect this usage (though the usual strip and tile size
        /// routines already do this).</remarks>
        public bool IsUpSampled()
        {
            return ((m_flags & TiffFlags.UPSAMPLED) == TiffFlags.UPSAMPLED);
        }

        /// <summary>
        /// Gets the value indicating whether the image data is being returned in MSB-to-LSB
        /// bit order.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the data is being returned in MSB-to-LSB bit order (i.e with bit 0 as
        /// the most significant bit); otherwise, <c>false</c>.
        /// </returns>
        public bool IsMSB2LSB()
        {
            return isFillOrder(FillOrder.MSB2LSB);
        }

        /// <summary>
        /// Gets the value indicating whether given image data was written in big-endian order.
        /// </summary>
        /// <returns>
        /// <c>true</c> if given image data was written in big-endian order; otherwise, <c>false</c>.
        /// </returns>
        public bool IsBigEndian()
        {
            return (m_header.tiff_magic == TIFF_BIGENDIAN);
        }

        /// <summary>
        /// Gets the tiff stream.
        /// </summary>
        /// <returns>The tiff stream.</returns>
        public TiffStream GetStream()
        {
            return m_stream;
        }

        /// <summary>
        /// Gets the current row that is being read or written.
        /// </summary>
        /// <returns>The current row that is being read or written.</returns>
        /// <remarks>The current row is updated each time a read or write is done.</remarks>
        public int CurrentRow()
        {
            return m_row;
        }

        /// <summary>
        /// Gets the zero-based index of the current directory.
        /// </summary>
        /// <returns>The zero-based index of the current directory.</returns>
        /// <remarks>The zero-based index returned by this method is suitable for use with
        /// the <see cref="SetDirectory"/> method.
        /// </remarks>
        public short CurrentDirectory()
        {
            return m_curdir;
        }

        /// <summary>
        /// Gets the number of directories in a file.
        /// </summary>
        /// <returns>The number of directories in a file.</returns>
        public short NumberOfDirectories()
        {
            uint nextdir = m_header.tiff_diroff;
            short n = 0;
            long dummyOff;
            while (nextdir != 0 && advanceDirectory(ref nextdir, out dummyOff))
                n++;

            return n;
        }

        /// <summary>
        /// Retrieves the file/stream offset of the current directory.
        /// </summary>
        /// <returns>The file/stream offset of the current directory.</returns>
        public long CurrentDirOffset()
        {
            return m_diroff;
        }

        /// <summary>
        /// Gets the current strip that is being read or written.
        /// </summary>
        /// <returns>The current strip that is being read or written.</returns>
        /// <remarks>The current strip is updated each time a read or write is done.</remarks>
        public int CurrentStrip()
        {
            return m_curstrip;
        }

        /// <summary>
        /// Gets the current tile that is being read or written.
        /// </summary>
        /// <returns>The current tile that is being read or written.</returns>
        /// <remarks>The current tile is updated each time a read or write is done.</remarks>
        public int CurrentTile()
        {
            return m_curtile;
        }

        /// <summary>
        /// Sets up the data buffer used to read raw (encoded) data from a file.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        /// <param name="size">The buffer size.</param>
        /// <remarks>
        /// <para>
        /// This method is provided for client-control of the I/O buffers used by the library.
        /// Applications need never use this method; it's provided only for "intelligent clients"
        /// that wish to optimize memory usage and/or eliminate potential copy operations that can
        /// occur when working with images that have data stored without compression.
        /// </para>
        /// <para>
        /// If the <paramref name="buffer"/> is <c>null</c>, then a buffer of appropriate size is
        /// allocated by the library. Otherwise, the caller must guarantee that the buffer is
        /// large enough to hold any individual strip of raw data.
        /// </para>
        /// </remarks>
        public void ReadBufferSetup(byte[] buffer, int size)
        {
            Debug.Assert((m_flags & TiffFlags.NOREADRAW) != TiffFlags.NOREADRAW);
            m_rawdata = null;

            if (buffer != null)
            {
                m_rawdatasize = size;
                m_rawdata = buffer;
                m_flags &= ~TiffFlags.MYBUFFER;
            }
            else
            {
                m_rawdatasize = roundUp(size, 1024);
                if (m_rawdatasize > 0)
                {
                    m_rawdata = new byte[m_rawdatasize];
                }
                else
                {
                    Tiff.ErrorExt(this, m_clientdata,
                        "ReadBufferSetup", "{0}: No space for data buffer at scanline {1}", m_name, m_row);
                    m_rawdatasize = 0;
                }

                m_flags |= TiffFlags.MYBUFFER;
            }
        }

        /// <summary>
        /// Sets up the data buffer used to write raw (encoded) data to a file.
        /// </summary>
        /// <param name="buffer">The data buffer.</param>
        /// <param name="size">The buffer size.</param>
        /// <remarks>
        /// <para>
        /// This method is provided for client-control of the I/O buffers used by the library.
        /// Applications need never use this method; it's provided only for "intelligent clients"
        /// that wish to optimize memory usage and/or eliminate potential copy operations that can
        /// occur when working with images that have data stored without compression.
        /// </para>
        /// <para>
        /// If the <paramref name="size"/> is -1 then the buffer size is selected to hold a
        /// complete tile or strip, or at least 8 kilobytes, whichever is greater. If the
        /// <paramref name="buffer"/> is <c>null</c>, then a buffer of appropriate size is
        /// allocated by the library.
        /// </para>
        /// </remarks>
        public void WriteBufferSetup(byte[] buffer, int size)
        {
            if (m_rawdata != null)
            {
                if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER)
                    m_flags &= ~TiffFlags.MYBUFFER;

                m_rawdata = null;
            }

            if (size == -1)
            {
                size = (IsTiled() ? m_tilesize : StripSize());

                // Make raw data buffer at least 8K
                if (size < 8 * 1024)
                    size = 8 * 1024;

                // force allocation
                buffer = null;
            }

            if (buffer == null)
            {
                buffer = new byte[size];
                m_flags |= TiffFlags.MYBUFFER;
            }
            else
            {
                m_flags &= ~TiffFlags.MYBUFFER;
            }

            m_rawdata = buffer;
            m_rawdatasize = size;
            m_rawcc = 0;
            m_rawcp = 0;
            m_flags |= TiffFlags.BUFFERSETUP;
        }

        /// <summary>
        /// Setups the strips.
        /// </summary>
        /// <returns><c>true</c> if setup successfully; otherwise, <c>false</c></returns>
        public bool SetupStrips()
        {
            if (IsTiled())
                m_dir.td_stripsperimage = isUnspecified(FieldBit.TileDimensions) ? m_dir.td_samplesperpixel : NumberOfTiles();
            else
                m_dir.td_stripsperimage = isUnspecified(FieldBit.RowsPerStrip) ? m_dir.td_samplesperpixel : NumberOfStrips();

            m_dir.td_nstrips = m_dir.td_stripsperimage;

            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                m_dir.td_stripsperimage /= m_dir.td_samplesperpixel;

            m_dir.td_stripoffset = new uint[m_dir.td_nstrips];
            m_dir.td_stripbytecount = new uint[m_dir.td_nstrips];

            setFieldBit(FieldBit.StripOffsets);
            setFieldBit(FieldBit.StripByteCounts);
            return true;
        }

        /// <summary>
        /// Verifies that file/stream is writable and that the directory information is
        /// setup properly.
        /// </summary>
        /// <param name="tiles">If set to <c>true</c> then ability to write tiles will be verified;
        /// otherwise, ability to write strips will be verified.</param>
        /// <param name="method">The name of the calling method.</param>
        /// <returns><c>true</c> if file/stream is writeable and the directory information is
        /// setup properly; otherwise, <c>false</c></returns>
        public bool WriteCheck(bool tiles, string method)
        {
            if (m_mode == O_RDONLY)
            {
                ErrorExt(this, m_clientdata, method, "{0}: File not open for writing", m_name);
                return false;
            }

            if (tiles ^ IsTiled())
            {
                ErrorExt(this, m_clientdata, m_name,
                    tiles ? "Can not write tiles to a stripped image" : "Can not write scanlines to a tiled image");

                return false;
            }

            // On the first write verify all the required information has been setup and
            // initialize any data structures that had to wait until directory information was set.
            // Note that a lot of our work is assumed to remain valid because we disallow any of
            // the important parameters from changing after we start writing (i.e. once
            // BEENWRITING is set, SetField will only allow the image's length to be changed).
            if (!fieldSet(FieldBit.ImageDimensions))
            {
                ErrorExt(this, m_clientdata, method, "{0}: Must set \"ImageWidth\" before writing data", m_name);
                return false;
            }

            if (m_dir.td_samplesperpixel == 1)
            {
                // PlanarConfiguration is irrelevant in case of single band images and need not
                // be included. We will set it anyway, because this field is used in other parts
                // of library even in the single band case.
                if (!fieldSet(FieldBit.PlanarConfig))
                    m_dir.td_planarconfig = PlanarConfig.CONTIG;
            }
            else
            {
                if (!fieldSet(FieldBit.PlanarConfig))
                {
                    ErrorExt(this, m_clientdata, method,
                        "{0}: Must set \"PlanarConfiguration\" before writing data", m_name);

                    return false;
                }
            }

            if (m_dir.td_stripoffset == null && !SetupStrips())
            {
                m_dir.td_nstrips = 0;
                ErrorExt(this, m_clientdata, method,
                    "{0}: No space for {1} arrays", m_name, IsTiled() ? "tile" : "strip");

                return false;
            }

            m_tilesize = IsTiled() ? TileSize() : -1;
            m_scanlinesize = ScanlineSize();
            m_flags |= TiffFlags.BEENWRITING;
            return true;
        }

        /// <summary>
        /// Releases storage associated with current directory.
        /// </summary>
        public void FreeDirectory()
        {
            if (m_dir != null)
            {
                clearFieldBit(FieldBit.YCbCrSubsampling);
                clearFieldBit(FieldBit.YCbCrPositioning);

                m_dir = null;
            }
        }

        /// <summary>
        /// Creates a new directory within file/stream.
        /// </summary>
        /// <remarks>The newly created directory will not exist on the file/stream till
        /// <see cref="WriteDirectory"/>, <see cref="CheckpointDirectory"/>, <see cref="Flush"/>
        /// or <see cref="Close"/> is called.</remarks>
        public void CreateDirectory()
        {
            // Should we automatically call WriteDirectory()
            // if the current one is dirty?

            setupDefaultDirectory();
            m_diroff = 0;
            m_nextdiroff = 0;
            m_curoff = 0;
            m_row = -1;
            m_curstrip = -1;
        }

        /// <summary>
        /// Returns an indication of whether the current directory is the last directory
        /// in the file.
        /// </summary>
        /// <returns><c>true</c> if current directory is the last directory in the file;
        /// otherwise, <c>false</c>.</returns>
        public bool LastDirectory()
        {
            return (m_nextdiroff == 0);
        }

        /// <summary>
        /// Sets the directory with specified number as the current directory.
        /// </summary>
        /// <param name="number">The zero-based number of the directory to set as the
        /// current directory.</param>
        /// <returns><c>true</c> if the specified directory was set as current successfully;
        /// otherwise, <c>false</c></returns>
        /// <remarks><b>SetDirectory</b> changes the current directory and reads its contents with
        /// <see cref="ReadDirectory"/>.</remarks>
        public bool SetDirectory(short number)
        {
            uint nextdir = m_header.tiff_diroff;
            short n;
            for (n = number; n > 0 && nextdir != 0; n--)
            {
                long dummyOff;
                if (!advanceDirectory(ref nextdir, out dummyOff))
                    return false;
            }

            m_nextdiroff = nextdir;

            // Set curdir to the actual directory index. The -1 is because
            // ReadDirectory will increment m_curdir after successfully reading
            // the directory.
            m_curdir = (short)(number - n - 1);

            // Reset m_dirnumber counter and start new list of seen directories.
            // We need this to prevent IFD loops.
            m_dirnumber = 0;
            return ReadDirectory();
        }

        /// <summary>
        /// Sets the directory at specified file/stream offset as the current directory.
        /// </summary>
        /// <param name="offset">The offset from the beginnig of the file/stream to the directory
        /// to set as the current directory.</param>
        /// <returns><c>true</c> if the directory at specified file offset was set as current
        /// successfully; otherwise, <c>false</c></returns>
        /// <remarks><b>SetSubDirectory</b> acts like <see cref="SetDirectory"/>, except the
        /// directory is specified as a file offset instead of an index; this is required for
        /// accessing subdirectories linked through a SubIFD tag (e.g. thumbnail images).</remarks>        
        public bool SetSubDirectory(long offset)
        {
            m_nextdiroff = (uint)offset;

            // Reset m_dirnumber counter and start new list of seen directories.
            // We need this to prevent IFD loops.
            m_dirnumber = 0;
            return ReadDirectory();
        }

        /// <summary>
        /// Unlinks the specified directory from the directory chain.
        /// </summary>
        /// <param name="number">The zero-based number of the directory to unlink.</param>
        /// <returns><c>true</c> if directory was unlinked successfully; otherwise, <c>false</c>.</returns>
        /// <remarks><b>UnlinkDirectory</b> does not removes directory bytes from the file/stream.
        /// It only makes them unused.</remarks>
        public bool UnlinkDirectory(short number)
        {
            const string module = "UnlinkDirectory";

            if (m_mode == O_RDONLY)
            {
                ErrorExt(this, m_clientdata, module, "Can not unlink directory in read-only file");
                return false;
            }

            // Go to the directory before the one we want
            // to unlink and nab the offset of the link
            // field we'll need to patch.
            uint nextdir = m_header.tiff_diroff;
            long off = sizeof(short) + sizeof(short);
            for (int n = number - 1; n > 0; n--)
            {
                if (nextdir == 0)
                {
                    ErrorExt(this, m_clientdata, module,
                        "Directory {0} does not exist", number);
                    return false;
                }

                if (!advanceDirectory(ref nextdir, out off))
                    return false;
            }

            // Advance to the directory to be unlinked and fetch the offset of the directory
            // that follows.
            long dummyOff;
            if (!advanceDirectory(ref nextdir, out dummyOff))
                return false;

            // Go back and patch the link field of the preceding directory to point to the
            // offset of the directory that follows.
            seekFile(off, SeekOrigin.Begin);
            if ((m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                SwabUInt(ref nextdir);

            if (!writeIntOK((int)nextdir))
            {
                ErrorExt(this, m_clientdata, module, "Error writing directory link");
                return false;
            }

            // Leave directory state setup safely. We don't have facilities for doing inserting
            // and removing directories, so it's safest to just invalidate everything. This means
            // that the caller can only append to the directory chain.
            m_currentCodec.Cleanup();
            if ((m_flags & TiffFlags.MYBUFFER) == TiffFlags.MYBUFFER && m_rawdata != null)
            {
                m_rawdata = null;
                m_rawcc = 0;
            }

            m_flags &= ~(TiffFlags.BEENWRITING | TiffFlags.BUFFERSETUP | TiffFlags.POSTENCODE);
            FreeDirectory();
            setupDefaultDirectory();
            m_diroff = 0; // force link on next write
            m_nextdiroff = 0; // next write must be at end
            m_curoff = 0;
            m_row = -1;
            m_curstrip = -1;
            return true;
        }

        /// <summary>
        /// Sets the value(s) of a tag in a TIFF file/stream open for writing.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The tag value(s).</param>
        /// <returns><c>true</c> if tag value(s) were set successfully; otherwise, <c>false</c>.</returns>
        /// <remarks><para>
        /// <b>SetField</b> sets the value of a tag or pseudo-tag in the current directory
        /// associated with the open TIFF file/stream. To set the value of a field the file/stream
        /// must have been previously opened for writing with <see cref="Open"/> or
        /// <see cref="ClientOpen"/>; pseudo-tags can be set whether the file was opened for
        /// reading or writing. The tag is identified by <paramref name="tag"/>.
        /// The type and number of values in <paramref name="value"/> is dependent on the tag
        /// being set. You may want to consult
        /// <a href = "54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> to become familiar with exact data types and calling
        /// conventions required for each tag supported by the library.
        /// </para><para>
        /// A pseudo-tag is a parameter that is used to control the operation of the library but
        /// whose value is not read or written to the underlying file.
        /// </para><para>
        /// The field will be written to the file when/if the directory structure is updated.
        /// </para></remarks>
        public bool SetField(TiffTag tag, params object[] value)
        {
            if (okToChangeTag(tag))
                return m_tagmethods.SetField(this, tag, FieldValue.FromParams(value));

            return false;
        }

        /// <summary>
        /// Writes the contents of the current directory to the file and setup to create a new
        /// subfile (page) in the same file.
        /// </summary>
        /// <returns><c>true</c> if the current directory was written successfully;
        /// otherwise, <c>false</c></returns>
        /// <remarks>Applications only need to call <b>WriteDirectory</b> when writing multiple
        /// subfiles (pages) to a single TIFF file. <b>WriteDirectory</b> is automatically called
        /// by <see cref="Close"/> and <see cref="Flush"/> to write a modified directory if the
        /// file is open for writing.</remarks>
        public bool WriteDirectory()
        {
            return writeDirectory(true);
        }

        /// <summary>
        /// Writes the current state of the TIFF directory into the file to make what is currently
        /// in the file/stream readable.
        /// </summary>
        /// <returns><c>true</c> if the current directory was rewritten successfully;
        /// otherwise, <c>false</c></returns>
        /// <remarks>Unlike <see cref="WriteDirectory"/>, <b>CheckpointDirectory</b> does not free
        /// up the directory data structures in memory, so they can be updated (as strips/tiles
        /// are written) and written again. Reading such a partial file you will at worst get a
        /// TIFF read error for the first strip/tile encountered that is incomplete, but you will
        /// at least get all the valid data in the file before that. When the file is complete,
        /// just use <see cref="WriteDirectory"/> as usual to finish it off cleanly.</remarks>
        public bool CheckpointDirectory()
        {
            // Setup the strips arrays, if they haven't already been.
            if (m_dir.td_stripoffset == null)
                SetupStrips();

            bool rc = writeDirectory(false);
            SetWriteOffset(seekFile(0, SeekOrigin.End));
            return rc;
        }

        /// <summary>
        /// Rewrites the contents of the current directory to the file and setup to create a new
        /// subfile (page) in the same file.
        /// </summary>        
        /// <returns><c>true</c> if the current directory was rewritten successfully;
        /// otherwise, <c>false</c></returns>
        /// <remarks>The <b>RewriteDirectory</b> operates similarly to <see cref="WriteDirectory"/>,
        /// but can be called with directories previously read or written that already have an
        /// established location in the file. It will rewrite the directory, but instead of place
        /// it at it's old location (as <see cref="WriteDirectory"/> would) it will place them at
        /// the end of the file, correcting the pointer from the preceeding directory or file
        /// header to point to it's new location. This is particularly important in cases where
        /// the size of the directory and pointed to data has grown, so it won’t fit in the space
        /// available at the old location. Note that this will result in the loss of the 
        /// previously used directory space.</remarks>
        public bool RewriteDirectory()
        {
            const string module = "RewriteDirectory";

            // We don't need to do anything special if it hasn't been written.
            if (m_diroff == 0)
                return WriteDirectory();

            // Find and zero the pointer to this directory, so that linkDirectory will cause it to
            // be added after this directories current pre-link.

            // Is it the first directory in the file?
            if (m_header.tiff_diroff == m_diroff)
            {
                m_header.tiff_diroff = 0;
                m_diroff = 0;

                seekFile(TiffHeader.TIFF_MAGIC_SIZE + TiffHeader.TIFF_VERSION_SIZE, SeekOrigin.Begin);
                if (!writeIntOK((int)m_header.tiff_diroff))
                {
                    ErrorExt(this, m_clientdata, m_name, "Error updating TIFF header");
                    return false;
                }
            }
            else
            {
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
                while (nextdir != m_diroff && nextdir != 0);

                // get current offset
                long off = seekFile(0, SeekOrigin.Current);
                seekFile(off - sizeof(int), SeekOrigin.Begin);
                m_diroff = 0;

                if (!writeIntOK((int)m_diroff))
                {
                    ErrorExt(this, m_clientdata, module, "Error writing directory link");
                    return false;
                }
            }

            // Now use WriteDirectory() normally.
            return WriteDirectory();
        }

        /// <summary>
        /// Prints formatted description of the contents of the current directory to the
        /// specified stream.
        /// </summary>
        /// <overloads>
        /// Prints formatted description of the contents of the current directory to the
        /// specified stream possibly using specified print options.
        /// </overloads>
        /// <param name="stream">The stream.</param>
        public void PrintDirectory(Stream stream)
        {
            PrintDirectory(stream, TiffPrintFlags.NONE);
        }

        /// <summary>
        /// Prints formatted description of the contents of the current directory to the
        /// specified stream using specified print (formatting) options.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="flags">The print (formatting) options.</param>
        public void PrintDirectory(Stream stream, TiffPrintFlags flags)
        {
            const string EndOfLine = "\r\n";

            fprintf(stream, "TIFF Directory at offset 0x{0:x} ({1})" + EndOfLine, m_diroff, m_diroff);

            if (fieldSet(FieldBit.SubFileType))
            {
                fprintf(stream, "  Subfile Type:");
                string sep = " ";
                if ((m_dir.td_subfiletype & FileType.REDUCEDIMAGE) != 0)
                {
                    fprintf(stream, "{0}reduced-resolution image", sep);
                    sep = "/";
                }

                if ((m_dir.td_subfiletype & FileType.PAGE) != 0)
                {
                    fprintf(stream, "{0}multi-page document", sep);
                    sep = "/";
                }

                if ((m_dir.td_subfiletype & FileType.MASK) != 0)
                    fprintf(stream, "{0}transparency mask", sep);

                fprintf(stream, " ({0} = 0x{1:x})" + EndOfLine, m_dir.td_subfiletype, m_dir.td_subfiletype);
            }

            if (fieldSet(FieldBit.ImageDimensions))
            {
                fprintf(stream, "  Image Width: {0} Image Length: {1}", m_dir.td_imagewidth, m_dir.td_imagelength);
                if (fieldSet(FieldBit.ImageDepth))
                    fprintf(stream, " Image Depth: {0}", m_dir.td_imagedepth);
                fprintf(stream, EndOfLine);
            }

            if (fieldSet(FieldBit.TileDimensions))
            {
                fprintf(stream, "  Tile Width: {0} Tile Length: {1}", m_dir.td_tilewidth, m_dir.td_tilelength);
                if (fieldSet(FieldBit.TileDepth))
                    fprintf(stream, " Tile Depth: {0}", m_dir.td_tiledepth);
                fprintf(stream, EndOfLine);
            }

            if (fieldSet(FieldBit.Resolution))
            {
                fprintf(stream, "  Resolution: {0:G}, {1:G}", m_dir.td_xresolution, m_dir.td_yresolution);
                if (fieldSet(FieldBit.ResolutionUnit))
                {
                    switch (m_dir.td_resolutionunit)
                    {
                        case ResUnit.NONE:
                            fprintf(stream, " (unitless)");
                            break;
                        case ResUnit.INCH:
                            fprintf(stream, " pixels/inch");
                            break;
                        case ResUnit.CENTIMETER:
                            fprintf(stream, " pixels/cm");
                            break;
                        default:
                            fprintf(stream, " (unit {0} = 0x{1:x})", m_dir.td_resolutionunit, m_dir.td_resolutionunit);
                            break;
                    }
                }
                fprintf(stream, EndOfLine);
            }

            if (fieldSet(FieldBit.Position))
                fprintf(stream, "  Position: {0:G}, {1:G}" + EndOfLine, m_dir.td_xposition, m_dir.td_yposition);

            if (fieldSet(FieldBit.BitsPerSample))
                fprintf(stream, "  Bits/Sample: {0}" + EndOfLine, m_dir.td_bitspersample);

            if (fieldSet(FieldBit.SampleFormat))
            {
                fprintf(stream, "  Sample Format: ");
                switch (m_dir.td_sampleformat)
                {
                    case SampleFormat.VOID:
                        fprintf(stream, "void" + EndOfLine);
                        break;
                    case SampleFormat.INT:
                        fprintf(stream, "signed integer" + EndOfLine);
                        break;
                    case SampleFormat.UINT:
                        fprintf(stream, "unsigned integer" + EndOfLine);
                        break;
                    case SampleFormat.IEEEFP:
                        fprintf(stream, "IEEE floating point" + EndOfLine);
                        break;
                    case SampleFormat.COMPLEXINT:
                        fprintf(stream, "complex signed integer" + EndOfLine);
                        break;
                    case SampleFormat.COMPLEXIEEEFP:
                        fprintf(stream, "complex IEEE floating point" + EndOfLine);
                        break;
                    default:
                        fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_sampleformat, m_dir.td_sampleformat);
                        break;
                }
            }

            if (fieldSet(FieldBit.Compression))
            {
                TiffCodec c = FindCodec(m_dir.td_compression);
                fprintf(stream, "  Compression Scheme: ");
                if (c != null)
                    fprintf(stream, "{0}" + EndOfLine, c.m_name);
                else
                    fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_compression, m_dir.td_compression);
            }

            if (fieldSet(FieldBit.Photometric))
            {
                fprintf(stream, "  Photometric Interpretation: ");
                if ((int)m_dir.td_photometric < photoNames.Length)
                    fprintf(stream, "{0}" + EndOfLine, photoNames[(int)m_dir.td_photometric]);
                else
                {
                    switch (m_dir.td_photometric)
                    {
                        case Photometric.LOGL:
                            fprintf(stream, "CIE Log2(L)" + EndOfLine);
                            break;
                        case Photometric.LOGLUV:
                            fprintf(stream, "CIE Log2(L) (u',v')" + EndOfLine);
                            break;
                        default:
                            fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_photometric, m_dir.td_photometric);
                            break;
                    }
                }
            }

            if (fieldSet(FieldBit.ExtraSamples) && m_dir.td_extrasamples != 0)
            {
                fprintf(stream, "  Extra Samples: {0}<", m_dir.td_extrasamples);
                string sep = "";
                for (short i = 0; i < m_dir.td_extrasamples; i++)
                {
                    switch (m_dir.td_sampleinfo[i])
                    {
                        case ExtraSample.UNSPECIFIED:
                            fprintf(stream, "{0}unspecified", sep);
                            break;
                        case ExtraSample.ASSOCALPHA:
                            fprintf(stream, "{0}assoc-alpha", sep);
                            break;
                        case ExtraSample.UNASSALPHA:
                            fprintf(stream, "{0}unassoc-alpha", sep);
                            break;
                        default:
                            fprintf(stream, "{0}{1} (0x{2:x})", sep, m_dir.td_sampleinfo[i], m_dir.td_sampleinfo[i]);
                            break;
                    }
                    sep = ", ";
                }
                fprintf(stream, ">" + EndOfLine);
            }

            if (fieldSet(FieldBit.InkNames))
            {
                fprintf(stream, "  Ink Names: ");

                string[] names = m_dir.td_inknames.Split(new char[] { '\0' });
                for (int i = 0; i < names.Length; i++)
                {
                    printAscii(stream, names[i]);
                    fprintf(stream, ", ");
                }

                fprintf(stream, EndOfLine);
            }

            if (fieldSet(FieldBit.Thresholding))
            {
                fprintf(stream, "  Thresholding: ");
                switch (m_dir.td_threshholding)
                {
                    case Threshold.BILEVEL:
                        fprintf(stream, "bilevel art scan" + EndOfLine);
                        break;
                    case Threshold.HALFTONE:
                        fprintf(stream, "halftone or dithered scan" + EndOfLine);
                        break;
                    case Threshold.ERRORDIFFUSE:
                        fprintf(stream, "error diffused" + EndOfLine);
                        break;
                    default:
                        fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_threshholding, m_dir.td_threshholding);
                        break;
                }
            }

            if (fieldSet(FieldBit.FillOrder))
            {
                fprintf(stream, "  FillOrder: ");
                switch (m_dir.td_fillorder)
                {
                    case FillOrder.MSB2LSB:
                        fprintf(stream, "msb-to-lsb" + EndOfLine);
                        break;
                    case FillOrder.LSB2MSB:
                        fprintf(stream, "lsb-to-msb" + EndOfLine);
                        break;
                    default:
                        fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_fillorder, m_dir.td_fillorder);
                        break;
                }
            }

            if (fieldSet(FieldBit.YCbCrSubsampling))
            {
                // For hacky reasons (see JpegCodecTagMethods.JPEGFixupTestSubsampling method),
                // we need to fetch this rather than trust what is in our structures.
                FieldValue[] result = GetField(TiffTag.YCBCRSUBSAMPLING);
                short subsampling0 = result[0].ToShort();
                short subsampling1 = result[1].ToShort();
                fprintf(stream, "  YCbCr Subsampling: {0}, {1}" + EndOfLine, subsampling0, subsampling1);
            }

            if (fieldSet(FieldBit.YCbCrPositioning))
            {
                fprintf(stream, "  YCbCr Positioning: ");
                switch (m_dir.td_ycbcrpositioning)
                {
                    case YCbCrPosition.CENTERED:
                        fprintf(stream, "centered" + EndOfLine);
                        break;
                    case YCbCrPosition.COSITED:
                        fprintf(stream, "cosited" + EndOfLine);
                        break;
                    default:
                        fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_ycbcrpositioning, m_dir.td_ycbcrpositioning);
                        break;
                }
            }

            if (fieldSet(FieldBit.HalftoneHints))
                fprintf(stream, "  Halftone Hints: light {0} dark {1}" + EndOfLine, m_dir.td_halftonehints[0], m_dir.td_halftonehints[1]);

            if (fieldSet(FieldBit.Orientation))
            {
                fprintf(stream, "  Orientation: ");
                if ((int)m_dir.td_orientation < orientNames.Length)
                    fprintf(stream, "{0}" + EndOfLine, orientNames[(int)m_dir.td_orientation]);
                else
                    fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_orientation, m_dir.td_orientation);
            }

            if (fieldSet(FieldBit.SamplesPerPixel))
                fprintf(stream, "  Samples/Pixel: {0}" + EndOfLine, m_dir.td_samplesperpixel);

            if (fieldSet(FieldBit.RowsPerStrip))
            {
                fprintf(stream, "  Rows/Strip: ");
                if (m_dir.td_rowsperstrip == -1)
                    fprintf(stream, "(infinite)" + EndOfLine);
                else
                    fprintf(stream, "{0}" + EndOfLine, m_dir.td_rowsperstrip);
            }

            if (fieldSet(FieldBit.MinSampleValue))
                fprintf(stream, "  Min Sample Value: {0}" + EndOfLine, m_dir.td_minsamplevalue);

            if (fieldSet(FieldBit.MaxSampleValue))
                fprintf(stream, "  Max Sample Value: {0}" + EndOfLine, m_dir.td_maxsamplevalue);

            if (fieldSet(FieldBit.SMinSampleValue))
                fprintf(stream, "  SMin Sample Value: {0:G}" + EndOfLine, m_dir.td_sminsamplevalue);

            if (fieldSet(FieldBit.SMaxSampleValue))
                fprintf(stream, "  SMax Sample Value: {0:G}" + EndOfLine, m_dir.td_smaxsamplevalue);

            if (fieldSet(FieldBit.PlanarConfig))
            {
                fprintf(stream, "  Planar Configuration: ");
                switch (m_dir.td_planarconfig)
                {
                    case PlanarConfig.CONTIG:
                        fprintf(stream, "single image plane" + EndOfLine);
                        break;
                    case PlanarConfig.SEPARATE:
                        fprintf(stream, "separate image planes" + EndOfLine);
                        break;
                    default:
                        fprintf(stream, "{0} (0x{1:x})" + EndOfLine, m_dir.td_planarconfig, m_dir.td_planarconfig);
                        break;
                }
            }

            if (fieldSet(FieldBit.PageNumber))
                fprintf(stream, "  Page Number: {0}-{1}" + EndOfLine, m_dir.td_pagenumber[0], m_dir.td_pagenumber[1]);

            if (fieldSet(FieldBit.ColorMap))
            {
                fprintf(stream, "  Color Map: ");
                if ((flags & TiffPrintFlags.COLORMAP) != 0)
                {
                    fprintf(stream, "" + EndOfLine);
                    int n = 1 << m_dir.td_bitspersample;
                    for (int l = 0; l < n; l++)
                        fprintf(stream, "   {0,5}: {1,5} {2,5} {3,5}" + EndOfLine, l, m_dir.td_colormap[0][l], m_dir.td_colormap[1][l], m_dir.td_colormap[2][l]);
                }
                else
                    fprintf(stream, "(present)" + EndOfLine);
            }

            if (fieldSet(FieldBit.TransferFunction))
            {
                fprintf(stream, "  Transfer Function: ");
                if ((flags & TiffPrintFlags.CURVES) != 0)
                {
                    fprintf(stream, "" + EndOfLine);
                    int n = 1 << m_dir.td_bitspersample;
                    for (int l = 0; l < n; l++)
                    {
                        fprintf(stream, "    {0,2}: {0,5}", l, m_dir.td_transferfunction[0][l]);
                        for (short i = 1; i < m_dir.td_samplesperpixel; i++)
                            fprintf(stream, " {0,5}", m_dir.td_transferfunction[i][l]);
                        fprintf(stream, "" + EndOfLine);
                    }
                }
                else
                    fprintf(stream, "(present)" + EndOfLine);
            }

            if (fieldSet(FieldBit.SubIFD) && m_dir.td_subifd != null)
            {
                fprintf(stream, "  SubIFD Offsets:");
                for (short i = 0; i < m_dir.td_nsubifd; i++)
                    fprintf(stream, " {0,5}", m_dir.td_subifd[i]);
                fprintf(stream, "" + EndOfLine);
            }

            // Custom tag support.

            int count = GetTagListCount();
            for (int i = 0; i < count; i++)
            {
                TiffTag tag = (TiffTag)GetTagListEntry(i);
                TiffFieldInfo fip = FieldWithTag(tag);
                if (fip == null)
                    continue;

                byte[] raw_data = null;
                int value_count;
                if (fip.PassCount)
                {
                    FieldValue[] result = GetField(tag);
                    if (result == null)
                        continue;

                    value_count = result[0].ToInt();
                    raw_data = result[1].ToByteArray();
                }
                else
                {
                    if (fip.ReadCount == TiffFieldInfo.Variable ||
                        fip.ReadCount == TiffFieldInfo.Variable2)
                    {
                        value_count = 1;
                    }
                    else if (fip.ReadCount == TiffFieldInfo.Spp)
                    {
                        value_count = m_dir.td_samplesperpixel;
                    }
                    else
                    {
                        value_count = fip.ReadCount;
                    }

                    if ((fip.Type == TiffType.ASCII ||
                        fip.ReadCount == TiffFieldInfo.Variable ||
                        fip.ReadCount == TiffFieldInfo.Variable2 ||
                        fip.ReadCount == TiffFieldInfo.Spp ||
                        value_count > 1) &&
                        fip.Tag != TiffTag.PAGENUMBER &&
                        fip.Tag != TiffTag.HALFTONEHINTS &&
                        fip.Tag != TiffTag.YCBCRSUBSAMPLING &&
                        fip.Tag != TiffTag.DOTRANGE)
                    {
                        FieldValue[] result = GetField(tag);
                        if (result == null)
                            continue;

                        raw_data = result[0].ToByteArray();
                    }
                    else if (fip.Tag != TiffTag.PAGENUMBER &&
                        fip.Tag != TiffTag.HALFTONEHINTS &&
                        fip.Tag != TiffTag.YCBCRSUBSAMPLING &&
                        fip.Tag != TiffTag.DOTRANGE)
                    {
                        raw_data = new byte[dataSize(fip.Type) * value_count];

                        FieldValue[] result = GetField(tag);
                        if (result == null)
                            continue;

                        raw_data = result[0].ToByteArray();
                    }
                    else
                    {
                        // XXX: Should be fixed and removed, see the notes
                        // related to PAGENUMBER, HALFTONEHINTS,
                        // YCBCRSUBSAMPLING and DOTRANGE tags
                        raw_data = new byte[dataSize(fip.Type) * value_count];

                        FieldValue[] result = GetField(tag);
                        if (result == null)
                            continue;

                        byte[] first = result[0].ToByteArray();
                        byte[] second = result[1].ToByteArray();

                        Buffer.BlockCopy(first, 0, raw_data, 0, first.Length);
                        Buffer.BlockCopy(second, 0, raw_data, dataSize(fip.Type), second.Length);
                    }
                }

                // Catch the tags which needs to be specially handled and
                // pretty print them. If tag not handled in prettyPrintField()
                // fall down and print it as any other tag.
                if (prettyPrintField(stream, tag, value_count, raw_data))
                    continue;
                else
                    printField(stream, fip, value_count, raw_data);
            }

            m_tagmethods.PrintDir(this, stream, flags);

            if ((flags & TiffPrintFlags.STRIPS) != 0 && fieldSet(FieldBit.StripOffsets))
            {
                fprintf(stream, "  {0} {1}:" + EndOfLine, m_dir.td_nstrips, IsTiled() ? "Tiles" : "Strips");
                for (int s = 0; s < m_dir.td_nstrips; s++)
                    fprintf(stream, "    {0,3}: [{0,8}, {0,8}]" + EndOfLine, s, m_dir.td_stripoffset[s], m_dir.td_stripbytecount[s]);
            }
        }

        /// <summary>
        /// Reads and decodes a scanline of data from an open TIFF file/stream.
        /// </summary>
        /// <overloads>
        /// Reads and decodes a scanline of data from an open TIFF file/stream.
        /// </overloads>
        /// <param name="buffer">The buffer to place read and decoded image data to.</param>
        /// <param name="row">The zero-based index of scanline (row) to read.</param>
        /// <returns>
        /// <c>true</c> if image data were read and decoded successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>ReadScanline</b> reads the data for the specified <paramref name="row"/> into the
        /// user supplied data buffer <paramref name="buffer"/>. The data are returned
        /// decompressed and, in the native byte- and bit-ordering, but are otherwise packed
        /// (see further below). The <paramref name="buffer"/> must be large enough to hold an
        /// entire scanline of data. Applications should call the <see cref="ScanlineSize"/>
        /// to find out the size (in bytes) of a scanline buffer. Applications should use
        /// <see cref="ReadScanline(byte[], int, short)"/> or
        /// <see cref="ReadScanline(byte[], int, int, short)"/> and specify correct sample plane if
        /// image data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// </para>
        /// <para>
        /// The library attempts to hide bit- and byte-ordering differences between the image and
        /// the native machine by converting data to the native machine order. Bit reversal is
        /// done if the value of <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.
        /// </para>
        /// </remarks>
        public bool ReadScanline(byte[] buffer, int row)
        {
            return ReadScanline(buffer, 0, row, 0);
        }

        /// <summary>
        /// Reads and decodes a scanline of data from an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer to place read and decoded image data to.</param>
        /// <param name="row">The zero-based index of scanline (row) to read.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>
        /// 	<c>true</c> if image data were read and decoded successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>ReadScanline</b> reads the data for the specified <paramref name="row"/> and
        /// specified sample plane <paramref name="plane"/> into the user supplied data buffer
        /// <paramref name="buffer"/>. The data are returned decompressed and, in the native
        /// byte- and bit-ordering, but are otherwise packed (see further below). The
        /// <paramref name="buffer"/> must be large enough to hold an entire scanline of data.
        /// Applications should call the <see cref="ScanlineSize"/> to find out the size (in
        /// bytes) of a scanline buffer. Applications may use
        /// <see cref="ReadScanline(byte[], int)"/> or specify 0 for <paramref name="plane"/>
        /// parameter if image data is contiguous (i.e not organized in separate planes, 
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG).
        /// </para>
        /// <para>
        /// The library attempts to hide bit- and byte-ordering differences between the image and
        /// the native machine by converting data to the native machine order. Bit reversal is
        /// done if the value of <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.
        /// </para>
        /// </remarks>
        public bool ReadScanline(byte[] buffer, int row, short plane)
        {
            return ReadScanline(buffer, 0, row, plane);
        }

        /// <summary>
        /// Reads and decodes a scanline of data from an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer to place read and decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin storing read and decoded bytes.</param>
        /// <param name="row">The zero-based index of scanline (row) to read.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>
        /// 	<c>true</c> if image data were read and decoded successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>ReadScanline</b> reads the data for the specified <paramref name="row"/> and
        /// specified sample plane <paramref name="plane"/> into the user supplied data buffer
        /// <paramref name="buffer"/>. The data are returned decompressed and, in the native
        /// byte- and bit-ordering, but are otherwise packed (see further below). The
        /// <paramref name="buffer"/> must be large enough to hold an entire scanline of data.
        /// Applications should call the <see cref="ScanlineSize"/> to find out the size (in
        /// bytes) of a scanline buffer. Applications may use
        /// <see cref="ReadScanline(byte[], int)"/> or specify 0 for <paramref name="plane"/>
        /// parameter if image data is contiguous (i.e not organized in separate planes,
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG).
        /// </para>
        /// <para>
        /// The library attempts to hide bit- and byte-ordering differences between the image and
        /// the native machine by converting data to the native machine order. Bit reversal is
        /// done if the value of <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.
        /// </para>
        /// </remarks>
        public bool ReadScanline(byte[] buffer, int offset, int row, short plane)
        {
            if (!checkRead(false))
                return false;

            bool e = seek(row, plane);
            if (e)
            {
                // Decompress desired row into user buffer.
                e = m_currentCodec.DecodeRow(buffer, offset, m_scanlinesize, plane);

                // we are now poised at the beginning of the next row
                m_row = row + 1;

                if (e)
                    postDecode(buffer, offset, m_scanlinesize);
            }

            return e;
        }

        /// <summary>
        /// Encodes and writes a scanline of data to an open TIFF file/stream.
        /// </summary>
        /// <overloads>Encodes and writes a scanline of data to an open TIFF file/stream.</overloads>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="row">The zero-based index of scanline (row) to place encoded data at.</param>
        /// <returns>
        /// 	<c>true</c> if image data were encoded and written successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>WriteScanline</b> encodes and writes to a file at the specified
        /// <paramref name="row"/>. Applications should use
        /// <see cref="WriteScanline(byte[], int, short)"/> or
        /// <see cref="WriteScanline(byte[], int, int, short)"/> and specify correct sample plane
        /// parameter if image data in a file/stream is organized in separate planes (i.e
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// </para><para>
        /// The data are assumed to be uncompressed and in the native bit- and byte-order of the
        /// host machine. The data written to the file is compressed according to the compression
        /// scheme of the current TIFF directory (see further below). If the current scanline is
        /// past the end of the current subfile, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the scanline (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8. The library attempts to hide bit-ordering differences between the image and
        /// the native machine by converting data from the native machine order.
        /// </para><para>
        /// Once data are written to a file/stream for the current directory, the values of
        /// certain tags may not be altered; see
        /// <a href="54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> for more information.
        /// </para><para>
        /// It is not possible to write scanlines to a file/stream that uses a tiled organization.
        /// The <see cref="IsTiled"/> can be used to determine if the file/stream is organized as
        /// tiles or strips.
        /// </para></remarks>
        public bool WriteScanline(byte[] buffer, int row)
        {
            return WriteScanline(buffer, 0, row, 0);
        }

        /// <summary>
        /// Encodes and writes a scanline of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="row">The zero-based index of scanline (row) to place encoded data at.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>
        /// 	<c>true</c> if image data were encoded and written successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>WriteScanline</b> encodes and writes to a file at the specified
        /// <paramref name="row"/> and specified sample plane <paramref name="plane"/>.
        /// Applications may use <see cref="WriteScanline(byte[], int)"/> or specify 0 for
        /// <paramref name="plane"/> parameter if image data in a file/stream is contiguous (i.e
        /// not organized in separate planes,
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG).
        /// </para><para>
        /// The data are assumed to be uncompressed and in the native bit- and byte-order of the
        /// host machine. The data written to the file is compressed according to the compression
        /// scheme of the current TIFF directory (see further below). If the current scanline is
        /// past the end of the current subfile, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the scanline (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8. The library attempts to hide bit-ordering differences between the image and
        /// the native machine by converting data from the native machine order.
        /// </para><para>
        /// Once data are written to a file/stream for the current directory, the values of
        /// certain tags may not be altered; see
        /// <a href="54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> for more information.
        /// </para><para>
        /// It is not possible to write scanlines to a file/stream that uses a tiled organization.
        /// The <see cref="IsTiled"/> can be used to determine if the file/stream is organized as
        /// tiles or strips.
        /// </para></remarks>
        public bool WriteScanline(byte[] buffer, int row, short plane)
        {
            return WriteScanline(buffer, 0, row, plane);
        }

        /// <summary>
        /// Encodes and writes a scanline of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes.</param>
        /// <param name="row">The zero-based index of scanline (row) to place encoded data at.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>
        /// 	<c>true</c> if image data were encoded and written successfully; otherwise, <c>false</c>
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>WriteScanline</b> encodes and writes to a file at the specified
        /// <paramref name="row"/> and specified sample plane <paramref name="plane"/>.
        /// Applications may use <see cref="WriteScanline(byte[], int)"/> or specify 0 for
        /// <paramref name="plane"/> parameter if image data in a file/stream is contiguous (i.e
        /// not organized in separate planes,
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG).
        /// </para><para>
        /// The data are assumed to be uncompressed and in the native bit- and byte-order of the
        /// host machine. The data written to the file is compressed according to the compression
        /// scheme of the current TIFF directory (see further below). If the current scanline is
        /// past the end of the current subfile, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the scanline (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8. The library attempts to hide bit-ordering differences between the image and
        /// the native machine by converting data from the native machine order.
        /// </para><para>
        /// Once data are written to a file/stream for the current directory, the values of
        /// certain tags may not be altered; see 
        /// <a href = "54cbd23d-dc55-44b9-921f-3a06efc2f6ce.htm">"Well-known tags and their
        /// value(s) data types"</a> for more information.
        /// </para><para>
        /// It is not possible to write scanlines to a file/stream that uses a tiled organization.
        /// The <see cref="IsTiled"/> can be used to determine if the file/stream is organized as
        /// tiles or strips.
        /// </para></remarks>
        public bool WriteScanline(byte[] buffer, int offset, int row, short plane)
        {
            const string module = "WriteScanline";

            if (!writeCheckStrips(module))
                return false;

            // Handle delayed allocation of data buffer. This permits it to be
            // sized more intelligently (using directory information).
            bufferCheck();

            // Extend image length if needed (but only for PlanarConfig.CONTIG).
            bool imagegrew = false;
            if (row >= m_dir.td_imagelength)
            {
                // extend image
                if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                {
                    ErrorExt(this, m_clientdata, m_name, "Can not change \"ImageLength\" when using separate planes");
                    return false;
                }

                m_dir.td_imagelength = row + 1;
                imagegrew = true;
            }

            // Calculate strip and check for crossings.
            int strip;
            if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
            {
                if (plane >= m_dir.td_samplesperpixel)
                {
                    ErrorExt(this, m_clientdata, m_name,
                        "{0}: Sample out of range, max {1}", plane, m_dir.td_samplesperpixel);
                    return false;
                }

                if (m_dir.td_rowsperstrip != -1)
                    strip = plane * m_dir.td_stripsperimage + row / m_dir.td_rowsperstrip;
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

            // Check strip array to make sure there's space. We don't support
            // dynamically growing files that have data organized in separate
            // bitplanes because it's too painful.  In that case we require that
            // the imagelength be set properly before the first write (so that
            // the strips array will be fully allocated above).
            if (strip >= m_dir.td_nstrips && !growStrips(1))
                return false;

            if (strip != m_curstrip)
            {
                // Changing strips - flush any data present.
                if (!FlushData())
                    return false;

                m_curstrip = (int)strip;

                // Watch out for a growing image. The value of strips/image 
                // will initially be 1 (since it can't be deduced until the
                // imagelength is known).
                if (strip >= m_dir.td_stripsperimage && imagegrew)
                    m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);

                m_row = (strip % m_dir.td_stripsperimage) * m_dir.td_rowsperstrip;
                if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
                {
                    if (!m_currentCodec.SetupEncode())
                        return false;

                    m_flags |= TiffFlags.CODERSETUP;
                }

                m_rawcc = 0;
                m_rawcp = 0;

                if (m_dir.td_stripbytecount[strip] > 0)
                {
                    // if we are writing over existing tiles, zero length
                    m_dir.td_stripbytecount[strip] = 0;

                    // this forces appendToStrip() to do a seek
                    m_curoff = 0;
                }

                if (!m_currentCodec.PreEncode(plane))
                    return false;

                m_flags |= TiffFlags.POSTENCODE;
            }

            // Ensure the write is either sequential or at the beginning of a
            // strip (or that we can randomly access the data - i.e. no encoding).
            if (row != m_row)
            {
                if (row < m_row)
                {
                    // Moving backwards within the same strip:
                    // backup to the start and then decode forward (below).
                    m_row = (strip % m_dir.td_stripsperimage) * m_dir.td_rowsperstrip;
                    m_rawcp = 0;
                }

                // Seek forward to the desired row.
                if (!m_currentCodec.Seek(row - m_row))
                    return false;

                m_row = row;
            }

            // swab if needed - note that source buffer will be altered
            postDecode(buffer, offset, m_scanlinesize);

            bool status = m_currentCodec.EncodeRow(buffer, offset, m_scanlinesize, plane);

            // we are now poised at the beginning of the next row
            m_row = row + 1;
            return status;
        }

        /// <summary>
        /// Reads the image and decodes it into RGBA format raster.
        /// </summary>
        /// <overloads>
        /// Reads the image and decodes it into RGBA format raster.
        /// </overloads>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <returns><c>true</c> if the image was successfully read and converted; otherwise,
        /// <c>false</c> is returned if an error was encountered.</returns>
        /// <remarks><para>
        /// <b>ReadRGBAImage</b> reads a strip- or tile-based image into memory, storing the
        /// result in the user supplied RGBA <paramref name="raster"/>. The raster is assumed to
        /// be an array of <paramref name="width"/> times <paramref name="height"/> 32-bit entries,
        /// where <paramref name="width"/> must be less than or equal to the width of the image
        /// (<paramref name="height"/> may be any non-zero size). If the raster dimensions are
        /// smaller than the image, the image data is cropped to the raster bounds. If the raster
        /// height is greater than that of the image, then the image data are placed in the lower
        /// part of the raster. Note that the raster is assumed to be organized such that the
        /// pixel at location (x, y) is <paramref name="raster"/>[y * width + x]; with the raster
        /// origin in the lower-left hand corner. Please use
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/> if you
        /// want to specify another raster origin.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// <b>ReadRGBAImage</b> converts non-8-bit images by scaling sample values. Palette,
        /// grayscale, bilevel, CMYK, and YCbCr images are converted to RGB transparently. Raster
        /// pixels are returned uncorrected by any colorimetry information present in the directory.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBAImage</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>
        /// <seealso cref="ReadRGBAStrip"/>
        /// <seealso cref="ReadRGBATile"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBAImage(int width, int height, int[] raster)
        {
            return ReadRGBAImage(width, height, raster, false);
        }

        /// <summary>
        /// Reads the image and decodes it into RGBA format raster.
        /// </summary>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="stopOnError">if set to <c>true</c> then an error will terminate the
        /// operation; otherwise method will continue processing data until all the possible data
        /// in the image have been requested.</param>
        /// <returns>
        /// <c>true</c> if the image was successfully read and converted; otherwise, <c>false</c>
        /// is returned if an error was encountered and stopOnError is <c>false</c>.
        /// </returns>
        /// <remarks><para>
        /// <b>ReadRGBAImage</b> reads a strip- or tile-based image into memory, storing the
        /// result in the user supplied RGBA <paramref name="raster"/>. The raster is assumed to
        /// be an array of <paramref name="width"/> times <paramref name="height"/> 32-bit entries,
        /// where <paramref name="width"/> must be less than or equal to the width of the image
        /// (<paramref name="height"/> may be any non-zero size). If the raster dimensions are
        /// smaller than the image, the image data is cropped to the raster bounds. If the raster
        /// height is greater than that of the image, then the image data are placed in the lower
        /// part of the raster. Note that the raster is assumed to be organized such that the
        /// pixel at location (x, y) is <paramref name="raster"/>[y * width + x]; with the raster
        /// origin in the lower-left hand corner. Please use
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/> if you
        /// want to specify another raster origin.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// <b>ReadRGBAImage</b> converts non-8-bit images by scaling sample values. Palette,
        /// grayscale, bilevel, CMYK, and YCbCr images are converted to RGB transparently. Raster
        /// pixels are returned uncorrected by any colorimetry information present in the directory.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBAImage</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>
        /// <seealso cref="ReadRGBAStrip"/>
        /// <seealso cref="ReadRGBATile"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBAImage(int width, int height, int[] raster, bool stopOnError)
        {
            return ReadRGBAImageOriented(width, height, raster, Orientation.BOTLEFT, stopOnError);
        }

        /// <summary>
        /// Reads the image and decodes it into RGBA format raster using specified raster origin.
        /// </summary>
        /// <overloads>
        /// Reads the image and decodes it into RGBA format raster using specified raster origin.
        /// </overloads>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="orientation">The raster origin position.</param>
        /// <returns>
        /// <c>true</c> if the image was successfully read and converted; otherwise, <c>false</c>
        /// is returned if an error was encountered.
        /// </returns>
        /// <remarks><para>
        /// <b>ReadRGBAImageOriented</b> reads a strip- or tile-based image into memory, storing the
        /// result in the user supplied RGBA <paramref name="raster"/>. The raster is assumed to
        /// be an array of <paramref name="width"/> times <paramref name="height"/> 32-bit entries,
        /// where <paramref name="width"/> must be less than or equal to the width of the image
        /// (<paramref name="height"/> may be any non-zero size). If the raster dimensions are
        /// smaller than the image, the image data is cropped to the raster bounds. If the raster
        /// height is greater than that of the image, then the image data placement depends on
        /// <paramref name="orientation"/>. Note that the raster is assumed to be organized such
        /// that the pixel at location (x, y) is <paramref name="raster"/>[y * width + x]; with
        /// the raster origin specified by <paramref name="orientation"/> parameter.
        /// </para><para>
        /// When <b>ReadRGBAImageOriented</b> is used with <see cref="Orientation"/>.BOTLEFT for
        /// the <paramref name="orientation"/> the produced result is the same as retuned by
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// <b>ReadRGBAImageOriented</b> converts non-8-bit images by scaling sample values.
        /// Palette, grayscale, bilevel, CMYK, and YCbCr images are converted to RGB transparently.
        /// Raster pixels are returned uncorrected by any colorimetry information present in
        /// the directory.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBAImageOriented</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>
        /// <seealso cref="ReadRGBAStrip"/>
        /// <seealso cref="ReadRGBATile"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBAImageOriented(int width, int height, int[] raster, Orientation orientation)
        {
            return ReadRGBAImageOriented(width, height, raster, orientation, false);
        }

        /// <summary>
        /// Reads the image and decodes it into RGBA format raster using specified raster origin.
        /// </summary>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="orientation">The raster origin position.</param>
        /// <param name="stopOnError">if set to <c>true</c> then an error will terminate the
        /// operation; otherwise method will continue processing data until all the possible data
        /// in the image have been requested.</param>
        /// <returns>
        /// <c>true</c> if the image was successfully read and converted; otherwise, <c>false</c>
        /// is returned if an error was encountered and stopOnError is <c>false</c>.
        /// </returns>
        /// <remarks><para>
        /// <b>ReadRGBAImageOriented</b> reads a strip- or tile-based image into memory, storing the
        /// result in the user supplied RGBA <paramref name="raster"/>. The raster is assumed to
        /// be an array of <paramref name="width"/> times <paramref name="height"/> 32-bit entries,
        /// where <paramref name="width"/> must be less than or equal to the width of the image
        /// (<paramref name="height"/> may be any non-zero size). If the raster dimensions are
        /// smaller than the image, the image data is cropped to the raster bounds. If the raster
        /// height is greater than that of the image, then the image data placement depends on
        /// <paramref name="orientation"/>. Note that the raster is assumed to be organized such
        /// that the pixel at location (x, y) is <paramref name="raster"/>[y * width + x]; with
        /// the raster origin specified by <paramref name="orientation"/> parameter.
        /// </para><para>
        /// When <b>ReadRGBAImageOriented</b> is used with <see cref="Orientation"/>.BOTLEFT for
        /// the <paramref name="orientation"/> the produced result is the same as retuned by
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// <b>ReadRGBAImageOriented</b> converts non-8-bit images by scaling sample values.
        /// Palette, grayscale, bilevel, CMYK, and YCbCr images are converted to RGB transparently.
        /// Raster pixels are returned uncorrected by any colorimetry information present in
        /// the directory.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBAImageOriented</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>
        /// <seealso cref="ReadRGBAStrip"/>
        /// <seealso cref="ReadRGBATile"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBAImageOriented(int width, int height, int[] raster, Orientation orientation, bool stopOnError)
        {
            bool ok = false;
            string emsg;
            if (RGBAImageOK(out emsg))
            {
                TiffRgbaImage img = TiffRgbaImage.Create(this, stopOnError, out emsg);
                if (img != null)
                {
                    img.ReqOrientation = orientation;
                    // XXX verify rwidth and rheight against image width and height
                    ok = img.GetRaster(raster, (height - img.Height) * width, width, img.Height);
                }
            }
            else
            {
                ErrorExt(this, m_clientdata, FileName(), "{0}", emsg);
                ok = false;
            }

            return ok;
        }

        /// <summary>
        /// Reads a whole strip of a strip-based image, decodes it and converts it to RGBA format.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="raster">The RGBA raster.</param>
        /// <returns><c>true</c> if the strip was successfully read and converted; otherwise,
        /// <c>false</c></returns>
        /// <remarks>
        /// <para>
        /// <b>ReadRGBAStrip</b> reads a single strip of a strip-based image into memory, storing
        /// the result in the user supplied RGBA <paramref name="raster"/>. If specified strip is
        /// the last strip, then it will only contain the portion of the strip that is actually
        /// within the image space. The raster is assumed to be an array of width times
        /// rowsperstrip 32-bit entries, where width is the width of the image
        /// (<see cref="TiffTag.IMAGEWIDTH"/>) and rowsperstrip is the maximum lines in a strip
        /// (<see cref="TiffTag.ROWSPERSTRIP"/>).
        /// </para><para>
        /// The <paramref name="row"/> value should be the row of the first row in the strip
        /// (strip * rowsperstrip, zero based).
        /// </para><para>
        /// Note that the raster is assumed to be organized such that the pixel at location (x, y)
        /// is <paramref name="raster"/>[y * width + x]; with the raster origin in the lower-left
        /// hand corner of the strip. That is bottom to top organization. When reading a partial
        /// last strip in the file the last line of the image will begin at the beginning of
        /// the buffer.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// See <see cref="TiffRgbaImage"/> for more details on how various image types are
        /// converted to RGBA values.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBAStrip</b>'s main advantage over the similar
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/> function is that for
        /// large images a single buffer capable of holding the whole image doesn't need to be
        /// allocated, only enough for one strip. The <see cref="ReadRGBATile"/> function does a
        /// similar operation for tiled images.
        /// </para><para>
        /// <b>ReadRGBAStrip</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>
        /// <seealso cref="ReadRGBATile"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBAStrip(int row, int[] raster)
        {
            if (IsTiled())
            {
                ErrorExt(this, m_clientdata, FileName(), "Can't use ReadRGBAStrip() with tiled file.");
                return false;
            }

            FieldValue[] result = GetFieldDefaulted(TiffTag.ROWSPERSTRIP);
            int rowsperstrip = result[0].ToInt();
            if ((row % rowsperstrip) != 0)
            {
                ErrorExt(this, m_clientdata, FileName(), "Row passed to ReadRGBAStrip() must be first in a strip.");
                return false;
            }

            bool ok;
            string emsg;
            if (RGBAImageOK(out emsg))
            {
                TiffRgbaImage img = TiffRgbaImage.Create(this, false, out emsg);
                if (img != null)
                {
                    img.row_offset = row;
                    img.col_offset = 0;

                    int rows_to_read = rowsperstrip;
                    if (row + rowsperstrip > img.Height)
                        rows_to_read = img.Height - row;

                    ok = img.GetRaster(raster, 0, img.Width, rows_to_read);
                    return ok;
                }

                return true;
            }

            ErrorExt(this, m_clientdata, FileName(), "{0}", emsg);
            return false;
        }


        /// <summary>
        /// Reads a whole tile of a tile-based image, decodes it and converts it to RGBA format.
        /// </summary>
        /// <param name="col">The column.</param>
        /// <param name="row">The row.</param>
        /// <param name="raster">The RGBA raster.</param>
        /// <returns><c>true</c> if the strip was successfully read and converted; otherwise,
        /// <c>false</c></returns>
        /// <remarks>
        /// <para><b>ReadRGBATile</b> reads a single tile of a tile-based image into memory,
        /// storing the result in the user supplied RGBA <paramref name="raster"/>. The raster is
        /// assumed to be an array of width times length 32-bit entries, where width is the width
        /// of the tile (<see cref="TiffTag.TILEWIDTH"/>) and length is the height of a tile
        /// (<see cref="TiffTag.TILELENGTH"/>).
        /// </para><para>
        /// The <paramref name="col"/> and <paramref name="row"/> values are the offsets from the
        /// top left corner of the image to the top left corner of the tile to be read. They must
        /// be an exact multiple of the tile width and length.
        /// </para><para>
        /// Note that the raster is assumed to be organized such that the pixel at location (x, y)
        /// is <paramref name="raster"/>[y * width + x]; with the raster origin in the lower-left
        /// hand corner of the tile. That is bottom to top organization. Edge tiles which partly
        /// fall off the image will be filled out with appropriate zeroed areas.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// See <see cref="TiffRgbaImage"/> for more details on how various image types are
        /// converted to RGBA values.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// <b>ReadRGBATile</b>'s main advantage over the similar
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/> function is that for
        /// large images a single buffer capable of holding the whole image doesn't need to be
        /// allocated, only enough for one tile. The <see cref="ReadRGBAStrip"/> function does a
        /// similar operation for stripped images.
        /// </para><para>
        /// <b>ReadRGBATile</b> is just a wrapper around the more general
        /// <see cref="TiffRgbaImage"/> facilities.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>
        /// <seealso cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>
        /// <seealso cref="ReadRGBAStrip"/>
        /// <seealso cref="RGBAImageOK"/>
        public bool ReadRGBATile(int col, int row, int[] raster)
        {
            // Verify that our request is legal - on a tile file, and on a tile boundary.
            if (!IsTiled())
            {
                ErrorExt(this, m_clientdata, FileName(), "Can't use ReadRGBATile() with stripped file.");
                return false;
            }

            FieldValue[] result = GetFieldDefaulted(TiffTag.TILEWIDTH);
            int tile_xsize = result[0].ToInt();
            result = GetFieldDefaulted(TiffTag.TILELENGTH);
            int tile_ysize = result[0].ToInt();

            if ((col % tile_xsize) != 0 || (row % tile_ysize) != 0)
            {
                ErrorExt(this, m_clientdata, FileName(), "Row/col passed to ReadRGBATile() must be topleft corner of a tile.");
                return false;
            }

            // Setup the RGBA reader.
            string emsg;
            TiffRgbaImage img = TiffRgbaImage.Create(this, false, out emsg);
            if (!RGBAImageOK(out emsg) || img == null)
            {
                ErrorExt(this, m_clientdata, FileName(), "{0}", emsg);
                return false;
            }

            // The TiffRgbaImage.Get() function doesn't allow us to get off the edge of the
            // image, even to fill an otherwise valid tile. So we figure out how much we can read,
            // and fix up the tile buffer to a full tile configuration afterwards.
            int read_ysize;
            if (row + tile_ysize > img.Height)
                read_ysize = img.Height - row;
            else
                read_ysize = tile_ysize;

            int read_xsize;
            if (col + tile_xsize > img.Width)
                read_xsize = img.Width - col;
            else
                read_xsize = tile_xsize;

            // Read the chunk of imagery.
            img.row_offset = row;
            img.col_offset = col;

            bool ok = img.GetRaster(raster, 0, read_xsize, read_ysize);

            // If our read was incomplete we will need to fix up the tile by shifting the data
            // around as if a full tile of data is being returned. This is all the more
            // complicated because the image is organized in bottom to top format. 
            if (read_xsize == tile_xsize && read_ysize == tile_ysize)
                return ok;

            for (int i_row = 0; i_row < read_ysize; i_row++)
            {
                Buffer.BlockCopy(raster, (read_ysize - i_row - 1) * read_xsize * sizeof(int),
                    raster, (tile_ysize - i_row - 1) * tile_xsize * sizeof(int), read_xsize * sizeof(int));

                Array.Clear(raster, (tile_ysize - i_row - 1) * tile_xsize + read_xsize, tile_xsize - read_xsize);
            }

            for (int i_row = read_ysize; i_row < tile_ysize; i_row++)
                Array.Clear(raster, (tile_ysize - i_row - 1) * tile_xsize, tile_xsize);

            return ok;
        }

        /// <summary>
        /// Check the image to see if it can be converted to RGBA format.
        /// </summary>
        /// <param name="errorMsg">The error message (if any) gets placed here.</param>
        /// <returns><c>true</c> if the image can be converted to RGBA format; otherwise,
        /// <c>false</c> is returned and <paramref name="errorMsg"/> contains the reason why it
        /// is being rejected.</returns>
        /// <remarks><para>
        /// To convert the image to RGBA format please use
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImage"/>,
        /// <see cref="O:BitMiracle.LibTiff.Classic.Tiff.ReadRGBAImageOriented"/>,
        /// <see cref="ReadRGBAStrip"/> or <see cref="ReadRGBATile"/>
        /// </para><para>
        /// Convertible images should follow this rules: samples must be either 1, 2, 4, 8, or
        /// 16 bits; colorimetric samples/pixel must be either 1, 3, or 4 (i.e. SamplesPerPixel
        /// minus ExtraSamples).</para>
        /// </remarks>
        public bool RGBAImageOK(out string errorMsg)
        {
            errorMsg = null;

            if (!m_decodestatus)
            {
                errorMsg = "Sorry, requested compression method is not configured";
                return false;
            }

            switch (m_dir.td_bitspersample)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                case 16:
                    break;
                default:
                    errorMsg = string.Format(CultureInfo.InvariantCulture,
                        "Sorry, can not handle images with {0}-bit samples", m_dir.td_bitspersample);
                    return false;
            }

            int colorchannels = m_dir.td_samplesperpixel - m_dir.td_extrasamples;
            Photometric photometric = Photometric.RGB;
            FieldValue[] result = GetField(TiffTag.PHOTOMETRIC);
            if (result == null)
            {
                switch (colorchannels)
                {
                    case 1:
                        photometric = Photometric.MINISBLACK;
                        break;
                    case 3:
                        photometric = Photometric.RGB;
                        break;
                    default:
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Missing needed {0} tag", TiffRgbaImage.photoTag);
                        return false;
                }
            }
            else
            {
                // San Chen <bigsan.chen@gmail.com>
                photometric = (Photometric)result[0].Value;
            }

            switch (photometric)
            {
                case Photometric.MINISWHITE:
                case Photometric.MINISBLACK:
                case Photometric.PALETTE:
                    if (m_dir.td_planarconfig == PlanarConfig.CONTIG &&
                        m_dir.td_samplesperpixel != 1 && m_dir.td_bitspersample < 8)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}",
                            TiffRgbaImage.photoTag, photometric, "Samples/pixel", m_dir.td_samplesperpixel,
                            m_dir.td_bitspersample);

                        return false;
                    }
                    // We should likely validate that any extra samples are either to be ignored,
                    // or are alpha, and if alpha we should try to use them. But for now we won't
                    // bother with this. 
                    break;
                case Photometric.YCBCR:
                    // TODO: if at all meaningful and useful, make more complete support check
                    // here, or better still, refactor to let supporting code decide whether there
                    // is support and what meaningfull error to return
                    break;
                case Photometric.RGB:
                    if (colorchannels < 3)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle RGB image with {0}={1}",
                            "Color channels", colorchannels);

                        return false;
                    }
                    break;
                case Photometric.SEPARATED:
                    result = GetFieldDefaulted(TiffTag.INKSET);
                    InkSet inkset = (InkSet)result[0].ToByte();
                    if (inkset != InkSet.CMYK)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle separated image with {0}={1}", "InkSet", inkset);
                        return false;
                    }
                    if (m_dir.td_samplesperpixel < 4)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle separated image with {0}={1}",
                            "Samples/pixel", m_dir.td_samplesperpixel);
                        return false;
                    }
                    break;
                case Photometric.LOGL:
                    if (m_dir.td_compression != Compression.SGILOG)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, LogL data must have {0}={1}",
                            "Compression", Compression.SGILOG);
                        return false;
                    }
                    break;
                case Photometric.LOGLUV:
                    if (m_dir.td_compression != Compression.SGILOG &&
                        m_dir.td_compression != Compression.SGILOG24)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, LogLuv data must have {0}={1} or {2}",
                            "Compression", Compression.SGILOG, Compression.SGILOG24);
                        return false;
                    }

                    if (m_dir.td_planarconfig != PlanarConfig.CONTIG)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle LogLuv images with {0}={1}",
                            "Planarconfiguration", m_dir.td_planarconfig);
                        return false;
                    }
                    break;
                case Photometric.CIELAB:
                    break;
                default:
                    errorMsg = string.Format(CultureInfo.InvariantCulture,
                        "Sorry, can not handle image with {0}={1}",
                        TiffRgbaImage.photoTag, photometric);
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the name of the file or ID string for this <see cref="Tiff"/>.
        /// </summary>
        /// <returns>The name of the file or ID string for this <see cref="Tiff"/>.</returns>
        /// <remarks>If this <see cref="Tiff"/> was created using <see cref="Open"/> method then
        /// value of fileName parameter of <see cref="Open"/> method is returned. If this
        /// <see cref="Tiff"/> was created using <see cref="ClientOpen"/> then value of
        /// name parameter of <see cref="ClientOpen"/> method is returned.</remarks>
        public string FileName()
        {
            return m_name;
        }

        /// <summary>
        /// Sets the new ID string for this <see cref="Tiff"/>.
        /// </summary>
        /// <param name="name">The ID string for this <see cref="Tiff"/>.</param>
        /// <returns>The previous file name or ID string for this <see cref="Tiff"/>.</returns>
        /// <remarks>Please note, that <paramref name="name"/> is an arbitrary string used as
        /// ID for this <see cref="Tiff"/>. It's not required to be a file name or anything
        /// meaningful at all.</remarks>
        public string SetFileName(string name)
        {
            string old_name = m_name;
            m_name = name;
            return old_name;
        }

        /// <summary>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class. Can be <c>null</c>.</param>
        /// <param name="method">The method where an error is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which an error is detected.
        /// </para>
        /// <para>Applications that desire to capture control in the event of an error should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        /// <overloads>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/>.
        /// </overloads>
        public static void Error(Tiff tif, string method, string format, params object[] args)
        {
            if (m_errorHandler == null)
                return;

            m_errorHandler.ErrorHandler(tif, method, format, args);
            m_errorHandler.ErrorHandlerExt(tif, null, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="method">The method where an error is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which an error is detected.
        /// </para>
        /// <para>Applications that desire to capture control in the event of an error should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        public static void Error(string method, string format, params object[] args)
        {
            Error(null, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class. Can be <c>null</c>.</param>
        /// <param name="clientData">The client data to be passed to error handler.</param>
        /// <param name="method">The method where an error is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which an error is detected.
        /// </para>
        /// <para>
        /// The <paramref name="clientData"/> parameter can be anything you want. It will be passed
        /// unchanged to the error handler. Default error handler does not use it. Only custom
        /// error handlers may make use of it.
        /// </para>
        /// <para>Applications that desire to capture control in the event of an error should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        /// <overloads>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/> and passes client data to the error handler.
        /// </overloads>
        public static void ErrorExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            if (m_errorHandler == null)
                return;

            m_errorHandler.ErrorHandler(tif, method, format, args);
            m_errorHandler.ErrorHandlerExt(tif, clientData, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide error handling methods to (normally) write an error message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="clientData">The client data to be passed to error handler.</param>
        /// <param name="method">The method where an error is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which an error is detected.
        /// </para>
        /// <para>
        /// The <paramref name="clientData"/> parameter can be anything you want. It will be passed
        /// unchanged to the error handler. Default error handler does not use it. Only custom
        /// error handlers may make use of it.
        /// </para>
        /// <para>Applications that desire to capture control in the event of an error should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        public static void ErrorExt(object clientData, string method, string format, params object[] args)
        {
            ErrorExt(null, clientData, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class. Can be <c>null</c>.</param>
        /// <param name="method">The method in which a warning is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter,
        /// if not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which a warning is detected.
        /// </para>
        /// <para>Applications that desire to capture control in the event of a warning should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        /// <overloads>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/>.
        /// </overloads>
        public static void Warning(Tiff tif, string method, string format, params object[] args)
        {
            if (m_errorHandler == null)
                return;

            m_errorHandler.WarningHandler(tif, method, format, args);
            m_errorHandler.WarningHandlerExt(tif, null, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/>.
        /// </summary>
        /// <param name="method">The method in which a warning is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks><para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter,
        /// if not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which a warning is detected.
        /// </para>
        /// <para>Applications that desire to capture control in the event of a warning should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        public static void Warning(string method, string format, params object[] args)
        {
            Warning(null, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/> and passes client data to the warning handler.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class. Can be <c>null</c>.</param>
        /// <param name="clientData">The client data to be passed to warning handler.</param>
        /// <param name="method">The method in which a warning is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks>
        /// <para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which a warning is detected.
        /// </para>
        /// <para>
        /// The <paramref name="clientData"/> parameter can be anything you want. It will be passed
        /// unchanged to the warning handler. Default warning handler does not use it. Only custom
        /// warning handlers may make use of it.
        /// </para>
        /// <para>Applications that desire to capture control in the event of a warning should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        /// <overloads>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/> and passes client data to the warning handler.
        /// </overloads>
        public static void WarningExt(Tiff tif, object clientData, string method, string format, params object[] args)
        {
            if (m_errorHandler == null)
                return;

            m_errorHandler.WarningHandler(tif, method, format, args);
            m_errorHandler.WarningHandlerExt(tif, clientData, method, format, args);
        }

        /// <summary>
        /// Invokes the library-wide warning handling methods to (normally) write a warning message
        /// to the <see cref="Console.Error"/> and passes client data to the warning handler.
        /// </summary>
        /// <param name="clientData">The client data to be passed to warning handler.</param>
        /// <param name="method">The method in which a warning is detected.</param>
        /// <param name="format">A composite format string (see Remarks).</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        /// <remarks><para>
        /// The <paramref name="format"/> is a composite format string that uses the same format as
        /// <see cref="O:System.String.Format"/> method. The <paramref name="method"/> parameter, if
        /// not <c>null</c>, is printed before the message; it typically is used to identify the
        /// method in which a warning is detected.
        /// </para><para>
        /// The <paramref name="clientData"/> parameter can be anything you want. It will be passed
        /// unchanged to the warning handler. Default warning handler does not use it. Only custom
        /// warning handlers may make use of it.
        /// </para>
        /// <para>Applications that desire to capture control in the event of a warning should use
        /// <see cref="SetErrorHandler"/> to override the default error and warning handler.
        /// </para>
        /// </remarks>
        public static void WarningExt(object clientData, string method, string format, params object[] args)
        {
            WarningExt(null, clientData, method, format, args);
        }

        /// <summary>
        /// Sets an instance of the <see cref="TiffErrorHandler"/> class as custom library-wide
        /// error and warning handler.
        /// </summary>
        /// <param name="errorHandler">An instance of the <see cref="TiffErrorHandler"/> class
        /// to set as custom library-wide error and warning handler.</param>
        /// <returns>
        /// Previous error handler or <c>null</c> if there was no error handler set.
        /// </returns>
        public static TiffErrorHandler SetErrorHandler(TiffErrorHandler errorHandler)
        {
            TiffErrorHandler prev = m_errorHandler;
            m_errorHandler = errorHandler;
            return prev;
        }

        /// <summary>
        /// Sets the tag extender method.
        /// </summary>
        /// <param name="extender">The tag extender method.</param>
        /// <returns>Previous tag extender method.</returns>
        /// <remarks>
        /// Extender method is called upon creation of each instance of <see cref="Tiff"/> object.
        /// </remarks>
        public static TiffExtendProc SetTagExtender(TiffExtendProc extender)
        {
            TiffExtendProc prev = m_extender;
            m_extender = extender;
            return prev;
        }

        /// <summary>
        /// Reads and decodes a tile of data from an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer to place read and decoded image data to.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin storing read and decoded bytes.</param>
        /// <param name="x">The x-coordinate of the pixel within a tile to be read and decoded.</param>
        /// <param name="y">The y-coordinate of the pixel within a tile to be read and decoded.</param>
        /// <param name="z">The z-coordinate of the pixel within a tile to be read and decoded.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>The number of bytes in the decoded tile or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// The tile to read and decode is selected by the (x, y, z, plane) coordinates (i.e.
        /// <b>ReadTile</b> returns the data for the tile containing the specified coordinates.
        /// The data placed in <paramref name="buffer"/> are returned decompressed and, typically,
        /// in the native byte- and bit-ordering, but are otherwise packed (see further below).
        /// The buffer must be large enough to hold an entire tile of data. Applications should
        /// call the <see cref="TileSize"/> to find out the size (in bytes) of a tile buffer.
        /// The <paramref name="x"/> and <paramref name="y"/> parameters are always used by
        /// <b>ReadTile</b>. The <paramref name="z"/> parameter is used if the image is deeper
        /// than 1 slice (a value of <see cref="TiffTag.IMAGEDEPTH"/> &gt; 1). In other cases the
        /// value of <paramref name="z"/> is ignored. The <paramref name="plane"/> parameter is
        /// used only if data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE). In other
        /// cases the value of <paramref name="plane"/> is ignored.
        /// </para><para>
        /// The library attempts to hide bit- and byte-ordering differences between the image and
        /// the native machine by converting data to the native machine order. Bit reversal is
        /// done if the value of <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.
        /// </para></remarks>
        public int ReadTile(byte[] buffer, int offset, int x, int y, int z, short plane)
        {
            if (!checkRead(true) || !CheckTile(x, y, z, plane))
                return -1;

            return ReadEncodedTile(ComputeTile(x, y, z, plane), buffer, offset, -1);
        }

        /// <summary>
        /// Reads a tile of data from an open TIFF file/stream, decompresses it and places
        /// specified amount of decompressed bytes into the user supplied buffer.
        /// </summary>
        /// <param name="tile">The zero-based index of the tile to read.</param>
        /// <param name="buffer">The buffer to place decompressed tile bytes to.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing
        /// decompressed tile bytes.</param>
        /// <param name="count">The maximum number of decompressed tile bytes to be stored
        /// to buffer.</param>
        /// <returns>The actual number of bytes of data that were placed in buffer or -1 if an
        /// error was encountered.</returns>
        /// <remarks>
        /// <para>
        /// The value of <paramref name="tile"/> is a "raw tile number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.</para>
        /// <para>To read a full tile of data the data buffer should typically be at least as
        /// large as the number returned by <see cref="TileSize"/>. If the -1 passed in
        /// <paramref name="count"/> parameter, the whole tile will be read. You should be sure
        /// you have enough space allocated for the buffer.</para>
        /// <para>The library attempts to hide bit- and byte-ordering differences between the
        /// image and the native machine by converting data to the native machine order. Bit
        /// reversal is done if the <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.</para>
        /// </remarks>
        public int ReadEncodedTile(int tile, byte[] buffer, int offset, int count)
        {
            if (!checkRead(true))
                return -1;

            if (tile >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Tile out of range, max {1}", tile, m_dir.td_nstrips);
                return -1;
            }

            if (count == -1)
                count = m_tilesize;
            else if (count > m_tilesize)
                count = m_tilesize;

            if (fillTile(tile) && m_currentCodec.DecodeTile(buffer, offset, count, (short)(tile / m_dir.td_stripsperimage)))
            {
                postDecode(buffer, offset, count);
                return count;
            }

            return -1;
        }

        /// <summary>
        /// Reads the undecoded contents of a tile of data from an open TIFF file/stream and places
        /// specified amount of read bytes into the user supplied buffer.
        /// </summary>
        /// <param name="tile">The zero-based index of the tile to read.</param>
        /// <param name="buffer">The buffer to place read tile bytes to.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing
        /// read tile bytes.</param>
        /// <param name="count">The maximum number of read tile bytes to be stored to buffer.</param>
        /// <returns>The actual number of bytes of data that were placed in buffer or -1 if an
        /// error was encountered.</returns>
        /// <remarks>
        /// <para>
        /// The value of <paramref name="tile"/> is a "raw tile number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.</para>
        /// <para>To read a full tile of data the data buffer should typically be at least as
        /// large as the number returned by <see cref="RawTileSize"/>. If the -1 passed in
        /// <paramref name="count"/> parameter, the whole tile will be read. You should be sure
        /// you have enough space allocated for the buffer.</para></remarks>
        public int ReadRawTile(int tile, byte[] buffer, int offset, int count)
        {
            const string module = "ReadRawTile";

            if (!checkRead(true))
                return -1;

            if (tile >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Tile out of range, max {1}", tile, m_dir.td_nstrips);
                return -1;
            }

            if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
            {
                ErrorExt(m_clientdata, m_name, "Compression scheme does not support access to raw uncompressed data");
                return -1;
            }

            uint bytecount = m_dir.td_stripbytecount[tile];
            if (count != -1 && (uint)count < bytecount)
                bytecount = (uint)count;

            return readRawTile1(tile, buffer, offset, (int)bytecount, module);
        }

        /// <summary>
        /// Encodes and writes a tile of data to an open TIFF file/stream.
        /// </summary>
        /// <overloads>Encodes and writes a tile of data to an open TIFF file/stream.</overloads>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="x">The x-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="y">The y-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="z">The z-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>
        /// The number of encoded and written bytes or <c>-1</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The tile to place encoded data is selected by the (x, y, z, plane) coordinates (i.e.
        /// <b>WriteTile</b> writes data to the tile containing the specified coordinates.
        /// <b>WriteTile</b> (potentially) encodes the data <paramref name="buffer"/> and writes
        /// it to open file/stream. The buffer must contain an entire tile of data. Applications
        /// should call the <see cref="TileSize"/> to find out the size (in bytes) of a tile buffer.
        /// The <paramref name="x"/> and <paramref name="y"/> parameters are always used by
        /// <b>WriteTile</b>. The <paramref name="z"/> parameter is used if the image is deeper
        /// than 1 slice (a value of <see cref="TiffTag.IMAGEDEPTH"/> &gt; 1). In other cases the
        /// value of <paramref name="z"/> is ignored. The <paramref name="plane"/> parameter is
        /// used only if data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE). In other
        /// cases the value of <paramref name="plane"/> is ignored.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para></remarks>
        public int WriteTile(byte[] buffer, int x, int y, int z, short plane)
        {
            return WriteTile(buffer, 0, x, y, z, plane);
        }

        /// <summary>
        /// Encodes and writes a tile of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes to be encoded and written.</param>
        /// <param name="x">The x-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="y">The y-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="z">The z-coordinate of the pixel within a tile to be encoded and written.</param>
        /// <param name="plane">The zero-based index of the sample plane.</param>
        /// <returns>The number of encoded and written bytes or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// The tile to place encoded data is selected by the (x, y, z, plane) coordinates (i.e.
        /// <b>WriteTile</b> writes data to the tile containing the specified coordinates.
        /// <b>WriteTile</b> (potentially) encodes the data <paramref name="buffer"/> and writes
        /// it to open file/stream. The buffer must contain an entire tile of data. Applications
        /// should call the <see cref="TileSize"/> to find out the size (in bytes) of a tile buffer.
        /// The <paramref name="x"/> and <paramref name="y"/> parameters are always used by
        /// <b>WriteTile</b>. The <paramref name="z"/> parameter is used if the image is deeper
        /// than 1 slice (a value of <see cref="TiffTag.IMAGEDEPTH"/> &gt; 1). In other cases the
        /// value of <paramref name="z"/> is ignored. The <paramref name="plane"/> parameter is
        /// used only if data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE). In other
        /// cases the value of <paramref name="plane"/> is ignored.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para></remarks>
        public int WriteTile(byte[] buffer, int offset, int x, int y, int z, short plane)
        {
            if (!CheckTile(x, y, z, plane))
                return -1;

            // NB: A tile size of -1 is used instead of m_tilesize knowing that WriteEncodedTile
            //     will clamp this to the tile size. This is done because the tile size may not be
            //     defined until after the output buffer is setup in WriteBufferSetup.
            return WriteEncodedTile(ComputeTile(x, y, z, plane), buffer, offset, -1);
        }

        /// <summary>
        /// Reads a strip of data from an open TIFF file/stream, decompresses it and places
        /// specified amount of decompressed bytes into the user supplied buffer.
        /// </summary>
        /// <param name="strip">The zero-based index of the strip to read.</param>
        /// <param name="buffer">The buffer to place decompressed strip bytes to.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing
        /// decompressed strip bytes.</param>
        /// <param name="count">The maximum number of decompressed strip bytes to be stored
        /// to buffer.</param>
        /// <returns>The actual number of bytes of data that were placed in buffer or -1 if an
        /// error was encountered.</returns>
        /// <remarks>
        /// <para>
        /// The value of <paramref name="strip"/> is a "raw strip number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to a
        /// strip index.</para>
        /// <para>To read a full strip of data the data buffer should typically be at least
        /// as large as the number returned by <see cref="StripSize"/>. If the -1 passed in
        /// <paramref name="count"/> parameter, the whole strip will be read. You should be sure
        /// you have enough space allocated for the buffer.</para>
        /// <para>The library attempts to hide bit- and byte-ordering differences between the
        /// image and the native machine by converting data to the native machine order. Bit
        /// reversal is done if the <see cref="TiffTag.FILLORDER"/> tag is opposite to the native
        /// machine bit order. 16- and 32-bit samples are automatically byte-swapped if the file
        /// was written with a byte order opposite to the native machine byte order.</para>
        /// </remarks>
        public int ReadEncodedStrip(int strip, byte[] buffer, int offset, int count)
        {
            if (!checkRead(false))
                return -1;

            if (strip >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Strip out of range, max {1}", strip, m_dir.td_nstrips);
                return -1;
            }

            // Calculate the strip size according to the number of rows in the strip (check for
            // truncated last strip on any of the separations).
            int strips_per_sep;
            if (m_dir.td_rowsperstrip >= m_dir.td_imagelength)
                strips_per_sep = 1;
            else
                strips_per_sep = (m_dir.td_imagelength + m_dir.td_rowsperstrip - 1) / m_dir.td_rowsperstrip;

            int sep_strip = strip % strips_per_sep;

            int nrows = m_dir.td_imagelength % m_dir.td_rowsperstrip;
            if (sep_strip != strips_per_sep - 1 || nrows == 0)
                nrows = m_dir.td_rowsperstrip;

            int stripsize = VStripSize(nrows);
            if (count == -1)
                count = stripsize;
            else if (count > stripsize)
                count = stripsize;

            if (fillStrip(strip) && m_currentCodec.DecodeStrip(buffer, offset, count, (short)(strip / m_dir.td_stripsperimage)))
            {
                postDecode(buffer, offset, count);
                return count;
            }

            return -1;
        }

        /// <summary>
        /// Reads the undecoded contents of a strip of data from an open TIFF file/stream and
        /// places specified amount of read bytes into the user supplied buffer.
        /// </summary>
        /// <param name="strip">The zero-based index of the strip to read.</param>
        /// <param name="buffer">The buffer to place read bytes to.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing
        /// read bytes.</param>
        /// <param name="count">The maximum number of read bytes to be stored to buffer.</param>
        /// <returns>The actual number of bytes of data that were placed in buffer or -1 if an
        /// error was encountered.</returns>
        /// <remarks>
        /// <para>
        /// The value of <paramref name="strip"/> is a "raw strip number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to a
        /// strip index.</para>
        /// <para>To read a full strip of data the data buffer should typically be at least
        /// as large as the number returned by <see cref="RawStripSize"/>. If the -1 passed in
        /// <paramref name="count"/> parameter, the whole strip will be read. You should be sure
        /// you have enough space allocated for the buffer.</para></remarks>
        public int ReadRawStrip(int strip, byte[] buffer, int offset, int count)
        {
            const string module = "ReadRawStrip";

            if (!checkRead(false))
                return -1;

            if (strip >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Strip out of range, max {1}", strip, m_dir.td_nstrips);
                return -1;
            }

            if ((m_flags & TiffFlags.NOREADRAW) == TiffFlags.NOREADRAW)
            {
                ErrorExt(this, m_clientdata, m_name, "Compression scheme does not support access to raw uncompressed data");
                return -1;
            }

            uint bytecount = m_dir.td_stripbytecount[strip];
            if (bytecount <= 0)
            {
                ErrorExt(this, m_clientdata, m_name, "{0}: Invalid strip byte count, strip {1}", bytecount, strip);
                return -1;
            }

            if (count != -1 && (uint)count < bytecount)
                bytecount = (uint)count;

            return readRawStrip1(strip, buffer, offset, (int)bytecount, module);
        }

        /// <summary>
        /// Encodes and writes a strip of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="strip">The zero-based index of the strip to write.</param>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="count">The maximum number of strip bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>
        /// The number of encoded and written bytes or <c>-1</c> if an error occurred.
        /// </returns>
        /// <overloads>Encodes and writes a strip of data to an open TIFF file/stream.</overloads>
        /// <remarks>
        /// <para>
        /// <b>WriteEncodedStrip</b> encodes <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> and append the result to the specified strip; replacing any
        /// previously written data. Note that the value of <paramref name="strip"/> is a "raw
        /// strip number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to
        /// a strip index.
        /// </para><para>
        /// If there is no space for the strip, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the strip (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8.
        /// </para></remarks>
        public int WriteEncodedStrip(int strip, byte[] buffer, int count)
        {
            return WriteEncodedStrip(strip, buffer, 0, count);
        }

        /// <summary>
        /// Encodes and writes a strip of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="strip">The zero-based index of the strip to write.</param>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes to be encoded and written.</param>
        /// <param name="count">The maximum number of strip bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>The number of encoded and written bytes or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// <b>WriteEncodedStrip</b> encodes <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> and append the result to the specified strip; replacing any
        /// previously written data. Note that the value of <paramref name="strip"/> is a "raw
        /// strip number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to
        /// a strip index.
        /// </para><para>
        /// If there is no space for the strip, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the strip (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8.
        /// </para></remarks>
        public int WriteEncodedStrip(int strip, byte[] buffer, int offset, int count)
        {
            const string module = "WriteEncodedStrip";

            if (!writeCheckStrips(module))
                return -1;

            // Check strip array to make sure there's space. We don't support dynamically growing
            // files that have data organized in separate bitplanes because it's too painful.
            // In that case we require that the imagelength be set properly before the first write
            // (so that the strips array will be fully allocated above).
            if (strip >= m_dir.td_nstrips)
            {
                if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                {
                    ErrorExt(this, m_clientdata, m_name, "Can not grow image by strips when using separate planes");
                    return -1;
                }

                if (!growStrips(1))
                    return -1;

                m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);
            }

            // Handle delayed allocation of data buffer. This permits it to be sized according to
            // the directory info.
            bufferCheck();

            m_curstrip = strip;
            m_row = (strip % m_dir.td_stripsperimage) * m_dir.td_rowsperstrip;
            if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
            {
                if (!m_currentCodec.SetupEncode())
                    return -1;

                m_flags |= TiffFlags.CODERSETUP;
            }

            m_rawcc = 0;
            m_rawcp = 0;

            if (m_dir.td_stripbytecount[strip] > 0)
            {
                // this forces appendToStrip() to do a seek
                m_curoff = 0;
            }

            m_flags &= ~TiffFlags.POSTENCODE;
            short sample = (short)(strip / m_dir.td_stripsperimage);
            if (!m_currentCodec.PreEncode(sample))
                return -1;

            // swab if needed - note that source buffer will be altered
            postDecode(buffer, offset, count);

            if (!m_currentCodec.EncodeStrip(buffer, offset, count, sample))
                return 0;

            if (!m_currentCodec.PostEncode())
                return -1;

            if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
                ReverseBits(m_rawdata, m_rawcc);

            if (m_rawcc > 0 && !appendToStrip(strip, m_rawdata, 0, m_rawcc))
                return -1;

            m_rawcc = 0;
            m_rawcp = 0;
            return count;
        }

        /// <summary>
        /// Writes a strip of raw data to an open TIFF file/stream.
        /// </summary>
        /// <overloads>Writes a strip of raw data to an open TIFF file/stream.</overloads>
        /// <param name="strip">The zero-based index of the strip to write.</param>
        /// <param name="buffer">The buffer with raw image data to be written.</param>
        /// <param name="count">The maximum number of strip bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>
        /// The number of written bytes or <c>-1</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>WriteRawStrip</b> appends <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> to the specified strip; replacing any
        /// previously written data. Note that the value of <paramref name="strip"/> is a "raw
        /// strip number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to
        /// a strip index.
        /// </para><para>
        /// If there is no space for the strip, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the strip (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para></remarks>
        public int WriteRawStrip(int strip, byte[] buffer, int count)
        {
            return WriteRawStrip(strip, buffer, 0, count);
        }

        /// <summary>
        /// Writes a strip of raw data to an open TIFF file/stream.
        /// </summary>
        /// <param name="strip">The zero-based index of the strip to write.</param>
        /// <param name="buffer">The buffer with raw image data to be written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes to be written.</param>
        /// <param name="count">The maximum number of strip bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>The number of written bytes or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// <b>WriteRawStrip</b> appends <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> to the specified strip; replacing any
        /// previously written data. Note that the value of <paramref name="strip"/> is a "raw
        /// strip number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeStrip"/> automatically does this when converting an (row, plane) to
        /// a strip index.
        /// </para><para>
        /// If there is no space for the strip, the value of <see cref="TiffTag.IMAGELENGTH"/>
        /// tag is automatically increased to include the strip (except for
        /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE, where the
        /// <see cref="TiffTag.IMAGELENGTH"/> tag cannot be changed once the first data are
        /// written). If the <see cref="TiffTag.IMAGELENGTH"/> is increased, the values of
        /// <see cref="TiffTag.STRIPOFFSETS"/> and <see cref="TiffTag.STRIPBYTECOUNTS"/> tags are
        /// similarly enlarged to reflect data written past the previous end of image.
        /// </para></remarks>
        public int WriteRawStrip(int strip, byte[] buffer, int offset, int count)
        {
            const string module = "WriteRawStrip";

            if (!writeCheckStrips(module))
                return -1;

            // Check strip array to make sure there's space. We don't support dynamically growing
            // files that have data organized in separate bitplanes because it's too painful.
            // In that case we require that the imagelength be set properly before the first write
            // (so that the strips array will be fully allocated above).
            if (strip >= m_dir.td_nstrips)
            {
                if (m_dir.td_planarconfig == PlanarConfig.SEPARATE)
                {
                    ErrorExt(this, m_clientdata, m_name, "Can not grow image by strips when using separate planes");
                    return -1;
                }

                // Watch out for a growing image. The value of strips/image will initially be 1
                // (since it can't be deduced until the imagelength is known).
                if (strip >= m_dir.td_stripsperimage)
                    m_dir.td_stripsperimage = howMany(m_dir.td_imagelength, m_dir.td_rowsperstrip);

                if (!growStrips(1))
                    return -1;
            }

            m_curstrip = strip;
            m_row = (strip % m_dir.td_stripsperimage) * m_dir.td_rowsperstrip;
            return (appendToStrip(strip, buffer, offset, count) ? count : -1);
        }

        /// <summary>
        /// Encodes and writes a tile of data to an open TIFF file/stream.
        /// </summary>
        /// <overloads>Encodes and writes a tile of data to an open TIFF file/stream.</overloads>
        /// <param name="tile">The zero-based index of the tile to write.</param>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="count">The maximum number of tile bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>
        /// The number of encoded and written bytes or <c>-1</c> if an error occurred.
        /// </returns>
        /// <remarks><para>
        /// <b>WriteEncodedTile</b> encodes <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> and append the result to the end of the specified tile. Note
        /// that the value of <paramref name="tile"/> is a "raw tile number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.
        /// </para><para>
        /// There must be space for the data. The function clamps individual writes to a tile to
        /// the tile size, but does not (and can not) check that multiple writes to the same tile
        /// were performed.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteEncodedTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8.
        /// </para></remarks>
        public int WriteEncodedTile(int tile, byte[] buffer, int count)
        {
            return WriteEncodedTile(tile, buffer, 0, count);
        }

        /// <summary>
        /// Encodes and writes a tile of data to an open TIFF file/stream.
        /// </summary>
        /// <param name="tile">The zero-based index of the tile to write.</param>
        /// <param name="buffer">The buffer with image data to be encoded and written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes to be encoded and written.</param>
        /// <param name="count">The maximum number of tile bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>The number of encoded and written bytes or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// <b>WriteEncodedTile</b> encodes <paramref name="count"/> bytes of raw data from
        /// <paramref name="buffer"/> and append the result to the end of the specified tile. Note
        /// that the value of <paramref name="tile"/> is a "raw tile number". That is, the caller
        /// must take into account whether or not the data are organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.
        /// </para><para>
        /// There must be space for the data. The function clamps individual writes to a tile to
        /// the tile size, but does not (and can not) check that multiple writes to the same tile
        /// were performed.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteEncodedTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para><para>
        /// The library writes encoded data using the native machine byte order. Correctly
        /// implemented TIFF readers are expected to do any necessary byte-swapping to correctly
        /// process image data with value of <see cref="TiffTag.BITSPERSAMPLE"/> tag greater
        /// than 8.
        /// </para></remarks>
        public int WriteEncodedTile(int tile, byte[] buffer, int offset, int count)
        {
            const string module = "WriteEncodedTile";

            if (!writeCheckTiles(module))
                return -1;

            if (tile >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, module, "{0}: Tile {1} out of range, max {2}", m_name, tile, m_dir.td_nstrips);
                return -1;
            }

            // Handle delayed allocation of data buffer. This permits it to be sized more
            // intelligently (using directory information).
            bufferCheck();

            m_curtile = tile;

            m_rawcc = 0;
            m_rawcp = 0;

            if (m_dir.td_stripbytecount[tile] > 0)
            {
                // this forces appendToStrip() to do a seek
                m_curoff = 0;
            }

            // Compute tiles per row & per column to compute current row and column
            m_row = (tile % howMany(m_dir.td_imagelength, m_dir.td_tilelength)) * m_dir.td_tilelength;
            m_col = (tile % howMany(m_dir.td_imagewidth, m_dir.td_tilewidth)) * m_dir.td_tilewidth;

            if ((m_flags & TiffFlags.CODERSETUP) != TiffFlags.CODERSETUP)
            {
                if (!m_currentCodec.SetupEncode())
                    return -1;

                m_flags |= TiffFlags.CODERSETUP;
            }

            m_flags &= ~TiffFlags.POSTENCODE;
            short sample = (short)(tile / m_dir.td_stripsperimage);
            if (!m_currentCodec.PreEncode(sample))
                return -1;

            // Clamp write amount to the tile size. This is mostly done so that callers can pass
            // in some large number (e.g. -1) and have the tile size used instead.
            if (count < 1 || count > m_tilesize)
                count = m_tilesize;

            // swab if needed - note that source buffer will be altered
            postDecode(buffer, offset, count);

            if (!m_currentCodec.EncodeTile(buffer, offset, count, sample))
                return 0;

            if (!m_currentCodec.PostEncode())
                return -1;

            if (!isFillOrder(m_dir.td_fillorder) && (m_flags & TiffFlags.NOBITREV) != TiffFlags.NOBITREV)
                ReverseBits(m_rawdata, m_rawcc);

            if (m_rawcc > 0 && !appendToStrip(tile, m_rawdata, 0, m_rawcc))
                return -1;

            m_rawcc = 0;
            m_rawcp = 0;
            return count;
        }

        /// <summary>
        /// Writes a tile of raw data to an open TIFF file/stream.
        /// </summary>
        /// <overloads>Writes a tile of raw data to an open TIFF file/stream.</overloads>
        /// <param name="tile">The zero-based index of the tile to write.</param>
        /// <param name="buffer">The buffer with raw image data to be written.</param>
        /// <param name="count">The maximum number of tile bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>
        /// The number of written bytes or <c>-1</c> if an error occurred.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>WriteRawTile</b> appends <paramref name="count"/> bytes of raw data to the end of
        /// the specified tile. Note that the value of <paramref name="tile"/> is a "raw tile
        /// number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.
        /// </para><para>
        /// There must be space for the data. The function clamps individual writes to a tile to
        /// the tile size, but does not (and can not) check that multiple writes to the same tile
        /// were performed.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteRawTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para></remarks>
        public int WriteRawTile(int tile, byte[] buffer, int count)
        {
            return WriteRawTile(tile, buffer, 0, count);
        }

        /// <summary>
        /// Writes a tile of raw data to an open TIFF file/stream.
        /// </summary>
        /// <param name="tile">The zero-based index of the tile to write.</param>
        /// <param name="buffer">The buffer with raw image data to be written.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at which
        /// to begin reading bytes to be written.</param>
        /// <param name="count">The maximum number of tile bytes to be read from
        /// <paramref name="buffer"/>.</param>
        /// <returns>The number of written bytes or <c>-1</c> if an error occurred.</returns>
        /// <remarks>
        /// <para>
        /// <b>WriteRawTile</b> appends <paramref name="count"/> bytes of raw data to the end of
        /// the specified tile. Note that the value of <paramref name="tile"/> is a "raw tile
        /// number". That is, the caller must take into account whether or not the data are
        /// organized in separate planes
        /// (<see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE).
        /// <see cref="ComputeTile"/> automatically does this when converting an (x, y, z, plane)
        /// coordinate quadruple to a tile number.
        /// </para><para>
        /// There must be space for the data. The function clamps individual writes to a tile to
        /// the tile size, but does not (and can not) check that multiple writes to the same tile
        /// were performed.
        /// </para><para>
        /// A correct value for the <see cref="TiffTag.IMAGELENGTH"/> tag must be setup before
        /// writing; <b>WriteRawTile</b> does not support automatically growing the image on
        /// each write (as <see cref="O:BitMiracle.LibTiff.Classic.Tiff.WriteScanline"/> does).
        /// </para></remarks>
        public int WriteRawTile(int tile, byte[] buffer, int offset, int count)
        {
            const string module = "WriteRawTile";

            if (!writeCheckTiles(module))
                return -1;

            if (tile >= m_dir.td_nstrips)
            {
                ErrorExt(this, m_clientdata, module,
                    "{0}: Tile {1} out of range, max {2}", m_name, tile, m_dir.td_nstrips);
                return -1;
            }

            return (appendToStrip(tile, buffer, offset, count) ? count : -1);
        }

        /// <summary>
        /// Sets the current write offset.
        /// </summary>
        /// <param name="offset">The write offset.</param>
        /// <remarks>This should only be used to set the offset to a known previous location
        /// (very carefully), or to 0 so that the next write gets appended to the end of the file.
        /// </remarks>
        public void SetWriteOffset(long offset)
        {
            m_curoff = (uint)offset;
        }

        /// <summary>
        /// Gets the number of bytes occupied by the item of given type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The number of bytes occupied by the <paramref name="type"/> or 0 if unknown
        /// data type is supplied.</returns>
        public static int DataWidth(TiffType type)
        {
            switch (type)
            {
                case TiffType.NOTYPE:
                case TiffType.BYTE:
                case TiffType.ASCII:
                case TiffType.SBYTE:
                case TiffType.UNDEFINED:
                    return 1;

                case TiffType.SHORT:
                case TiffType.SSHORT:
                    return 2;

                case TiffType.LONG:
                case TiffType.SLONG:
                case TiffType.FLOAT:
                case TiffType.IFD:
                    return 4;

                case TiffType.RATIONAL:
                case TiffType.SRATIONAL:
                case TiffType.DOUBLE:
                    return 8;

                default:
                    // will return 0 for unknown types
                    return 0;
            }
        }

        /// <summary>
        /// Swaps the bytes in a single 16-bit item.
        /// </summary>
        /// <param name="value">The value to swap bytes in.</param>
        public static void SwabShort(ref short value)
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)value;
            bytes[1] = (byte)(value >> 8);

            byte temp = bytes[1];
            bytes[1] = bytes[0];
            bytes[0] = temp;

            value = (short)(bytes[0] & 0xFF);
            value += (short)((bytes[1] & 0xFF) << 8);
        }

        /// <summary>
        /// Swaps the bytes in a single 32-bit item.
        /// </summary>
        /// <param name="value">The value to swap bytes in.</param>
        public static void SwabLong(ref int value)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)value;
            bytes[1] = (byte)(value >> 8);
            bytes[2] = (byte)(value >> 16);
            bytes[3] = (byte)(value >> 24);

            byte temp = bytes[3];
            bytes[3] = bytes[0];
            bytes[0] = temp;

            temp = bytes[2];
            bytes[2] = bytes[1];
            bytes[1] = temp;

            value = bytes[0] & 0xFF;
            value += (bytes[1] & 0xFF) << 8;
            value += (bytes[2] & 0xFF) << 16;
            value += bytes[3] << 24;
        }

        /// <summary>
        /// Swaps the bytes in a single double-precision floating-point number.
        /// </summary>
        /// <param name="value">The value to swap bytes in.</param>
        public static void SwabDouble(ref double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            int[] ints = new int[2];
            ints[0] = BitConverter.ToInt32(bytes, 0);
            ints[0] = BitConverter.ToInt32(bytes, sizeof(int));

            SwabArrayOfLong(ints, 2);

            int temp = ints[0];
            ints[0] = ints[1];
            ints[1] = temp;

            Buffer.BlockCopy(BitConverter.GetBytes(ints[0]), 0, bytes, 0, sizeof(int));
            Buffer.BlockCopy(BitConverter.GetBytes(ints[1]), 0, bytes, sizeof(int), sizeof(int));
            value = BitConverter.ToDouble(bytes, 0);
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of 16-bit items.
        /// </summary>
        /// <overloads>
        /// Swaps the bytes in specified number of values in the array of 16-bit items.
        /// </overloads>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfShort(short[] array, int count)
        {
            SwabArrayOfShort(array, 0, count);
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of 16-bit items starting at
        /// specified offset.
        /// </summary>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="offset">The zero-based offset in <paramref name="array"/> at
        /// which to begin swapping bytes.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfShort(short[] array, int offset, int count)
        {
            byte[] bytes = new byte[2];
            for (int i = 0; i < count; i++, offset++)
            {
                bytes[0] = (byte)array[offset];
                bytes[1] = (byte)(array[offset] >> 8);

                byte temp = bytes[1];
                bytes[1] = bytes[0];
                bytes[0] = temp;

                array[offset] = (short)(bytes[0] & 0xFF);
                array[offset] += (short)((bytes[1] & 0xFF) << 8);
            }
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of triples (24-bit items).
        /// </summary>
        /// <overloads>
        /// Swaps the bytes in specified number of values in the array of triples (24-bit items).
        /// </overloads>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfTriples(byte[] array, int count)
        {
            SwabArrayOfTriples(array, 0, count);
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of triples (24-bit items)
        /// starting at specified offset.
        /// </summary>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="offset">The zero-based offset in <paramref name="array"/> at
        /// which to begin swapping bytes.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfTriples(byte[] array, int offset, int count)
        {
            // XXX unroll loop some
            while (count-- > 0)
            {
                byte t = array[offset + 2];
                array[offset + 2] = array[offset];
                array[offset] = t;
                offset += 3;
            }
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of 32-bit items.
        /// </summary>
        /// <overloads>
        /// Swaps the bytes in specified number of values in the array of 32-bit items.
        /// </overloads>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfLong(int[] array, int count)
        {
            SwabArrayOfLong(array, 0, count);
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of 32-bit items
        /// starting at specified offset.
        /// </summary>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="offset">The zero-based offset in <paramref name="array"/> at
        /// which to begin swapping bytes.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfLong(int[] array, int offset, int count)
        {
            byte[] bytes = new byte[4];

            for (int i = 0; i < count; i++, offset++)
            {
                bytes[0] = (byte)array[offset];
                bytes[1] = (byte)(array[offset] >> 8);
                bytes[2] = (byte)(array[offset] >> 16);
                bytes[3] = (byte)(array[offset] >> 24);

                byte temp = bytes[3];
                bytes[3] = bytes[0];
                bytes[0] = temp;

                temp = bytes[2];
                bytes[2] = bytes[1];
                bytes[1] = temp;

                array[offset] = bytes[0] & 0xFF;
                array[offset] += (bytes[1] & 0xFF) << 8;
                array[offset] += (bytes[2] & 0xFF) << 16;
                array[offset] += bytes[3] << 24;
            }
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of double-precision
        /// floating-point numbers.
        /// </summary>
        /// <overloads>
        /// Swaps the bytes in specified number of values in the array of double-precision
        /// floating-point numbers.
        /// </overloads>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfDouble(double[] array, int count)
        {
            SwabArrayOfDouble(array, 0, count);
        }

        /// <summary>
        /// Swaps the bytes in specified number of values in the array of double-precision
        /// floating-point numbers starting at specified offset.
        /// </summary>
        /// <param name="array">The array to swap bytes in.</param>
        /// <param name="offset">The zero-based offset in <paramref name="array"/> at
        /// which to begin swapping bytes.</param>
        /// <param name="count">The number of items to swap bytes in.</param>
        public static void SwabArrayOfDouble(double[] array, int offset, int count)
        {
            int[] ints = new int[count * sizeof(int) / sizeof(double)];
            Buffer.BlockCopy(array, offset * sizeof(double), ints, 0, ints.Length * sizeof(int));

            SwabArrayOfLong(ints, ints.Length);

            int pos = 0;
            while (count-- > 0)
            {
                int temp = ints[pos];
                ints[pos] = ints[pos + 1];
                ints[pos + 1] = temp;
                pos += 2;
            }

            Buffer.BlockCopy(ints, 0, array, offset * sizeof(double), ints.Length * sizeof(int));
        }

        /// <summary>
        /// Replaces specified number of bytes in <paramref name="buffer"/> with the
        /// equivalent bit-reversed bytes.
        /// </summary>
        /// <overloads>
        /// Replaces specified number of bytes in <paramref name="buffer"/> with the
        /// equivalent bit-reversed bytes.
        /// </overloads>
        /// <param name="buffer">The buffer to replace bytes in.</param>
        /// <param name="count">The number of bytes to process.</param>
        /// <remarks>
        /// This operation is performed with a lookup table, which can be retrieved using the
        /// <see cref="GetBitRevTable"/> method.
        /// </remarks>
        public static void ReverseBits(byte[] buffer, int count)
        {
            ReverseBits(buffer, 0, count);
        }

        /// <summary>
        /// Replaces specified number of bytes in <paramref name="buffer"/> with the
        /// equivalent bit-reversed bytes starting at specified offset.
        /// </summary>
        /// <param name="buffer">The buffer to replace bytes in.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at
        /// which to begin processing bytes.</param>
        /// <param name="count">The number of bytes to process.</param>
        /// <remarks>
        /// This operation is performed with a lookup table, which can be retrieved using the
        /// <see cref="GetBitRevTable"/> method.
        /// </remarks>
        public static void ReverseBits(byte[] buffer, int offset, int count)
        {
            for (; count > 8; count -= 8)
            {
                buffer[offset + 0] = TIFFBitRevTable[buffer[offset + 0]];
                buffer[offset + 1] = TIFFBitRevTable[buffer[offset + 1]];
                buffer[offset + 2] = TIFFBitRevTable[buffer[offset + 2]];
                buffer[offset + 3] = TIFFBitRevTable[buffer[offset + 3]];
                buffer[offset + 4] = TIFFBitRevTable[buffer[offset + 4]];
                buffer[offset + 5] = TIFFBitRevTable[buffer[offset + 5]];
                buffer[offset + 6] = TIFFBitRevTable[buffer[offset + 6]];
                buffer[offset + 7] = TIFFBitRevTable[buffer[offset + 7]];
                offset += 8;
            }

            while (count-- > 0)
            {
                buffer[offset] = TIFFBitRevTable[buffer[offset]];
                offset++;
            }
        }

        /// <summary>
        /// Retrieves a bit reversal table.
        /// </summary>
        /// <param name="reversed">if set to <c>true</c> then bit reversal table will be
        /// retrieved; otherwise, the table that do not reverse bit values will be retrieved.</param>
        /// <returns>The bit reversal table.</returns>
        /// <remarks>If <paramref name="reversed"/> is <c>false</c> then the table that do not
        /// reverse bit values will be retrieved. It is a lookup table that can be used as an
        /// identity function; i.e. NoBitRevTable[n] == n.</remarks>
        public static byte[] GetBitRevTable(bool reversed)
        {
            return (reversed ? TIFFBitRevTable : TIFFNoBitRevTable);
        }

        /// <summary>
        /// Converts a byte buffer into array of 32-bit values.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at
        /// which to begin converting bytes.</param>
        /// <param name="count">The number of bytes to convert.</param>
        /// <returns>The array of 32-bit values.</returns>
        public static int[] ByteArrayToInts(byte[] buffer, int offset, int count)
        {
            int intCount = count / sizeof(int);
            int[] integers = new int[intCount];
            Buffer.BlockCopy(buffer, offset, integers, 0, intCount * sizeof(int));            
            return integers;
        }

        /// <summary>
        /// Converts array of 32-bit values into array of bytes.
        /// </summary>
        /// <param name="source">The array of 32-bit values.</param>
        /// <param name="srcOffset">The zero-based offset in <paramref name="source"/> at
        /// which to begin converting bytes.</param>
        /// <param name="srcCount">The number of 32-bit values to convert.</param>
        /// <param name="bytes">The byte array to store converted values at.</param>
        /// <param name="offset">The zero-based offset in <paramref name="bytes"/> at
        /// which to begin storing converted values.</param>
        public static void IntsToByteArray(int[] source, int srcOffset, int srcCount, byte[] bytes, int offset)
        {
            Buffer.BlockCopy(source, srcOffset * sizeof(int), bytes, offset, srcCount * sizeof(int));
        }

        /// <summary>
        /// Converts a byte buffer into array of 16-bit values.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="offset">The zero-based offset in <paramref name="buffer"/> at
        /// which to begin converting bytes.</param>
        /// <param name="count">The number of bytes to convert.</param>
        /// <returns>The array of 16-bit values.</returns>
        public static short[] ByteArrayToShorts(byte[] buffer, int offset, int count)
        {
            int shortCount = count / sizeof(short);
            short[] shorts = new short[shortCount];
            Buffer.BlockCopy(buffer, offset, shorts, 0, shortCount * sizeof(short));
            return shorts;
        }

        /// <summary>
        /// Converts array of 16-bit values into array of bytes.
        /// </summary>
        /// <param name="source">The array of 16-bit values.</param>
        /// <param name="srcOffset">The zero-based offset in <paramref name="source"/> at
        /// which to begin converting bytes.</param>
        /// <param name="srcCount">The number of 16-bit values to convert.</param>
        /// <param name="bytes">The byte array to store converted values at.</param>
        /// <param name="offset">The zero-based offset in <paramref name="bytes"/> at
        /// which to begin storing converted values.</param>
        public static void ShortsToByteArray(short[] source, int srcOffset, int srcCount, byte[] bytes, int offset)
        {
            Buffer.BlockCopy(source, srcOffset * sizeof(short), bytes, offset, srcCount * sizeof(short));
        }
    }
}
