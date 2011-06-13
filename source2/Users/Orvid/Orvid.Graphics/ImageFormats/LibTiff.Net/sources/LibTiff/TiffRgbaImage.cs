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
using System.Globalization;

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// RGBA-style image support. Provides methods for decoding images into RGBA (or other) format.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>TiffRgbaImage</b> provide a high-level interface through which TIFF images may be read
    /// into memory. Images may be strip- or tile-based and have a variety of different
    /// characteristics: bits/sample, samples/pixel, photometric, etc. The target raster format
    /// can be customized to a particular application's needs by installing custom methods that
    /// manipulate image data according to application requirements.
    /// </para><para>
    /// The default usage for this class: check if an image can be processed using
    /// <see cref="BitMiracle.LibTiff.Classic.Tiff.RGBAImageOK"/>, construct an instance of
    /// <b>TiffRgbaImage</b> using <see cref="Create"/> and then read and decode an image into a
    /// target raster using <see cref="GetRaster"/>. <see cref="GetRaster"/> can be called
    /// multiple times to decode an image using different state parameters. If multiple images
    /// are to be displayed and there is not enough space for each of the decoded rasters,
    /// multiple instances of <b>TiffRgbaImage</b> can be managed and then calls can be made to
    /// <see cref="GetRaster"/> as needed to display an image.</para>
    /// <para>
    /// To use the core support for reading and processing TIFF images, but write the resulting
    /// raster data in a different format one need only override the "put methods" used to store
    /// raster data. These methods are initially setup by <see cref="Create"/> to point to methods
    /// that pack raster data in the default ABGR pixel format. Two different methods are used
    /// according to the physical organization of the image data in the file: one for
    /// <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.CONTIG (packed samples),
    /// and another for <see cref="TiffTag.PLANARCONFIG"/> = <see cref="PlanarConfig"/>.SEPARATE
    /// (separated samples). Note that this mechanism can be used to transform the data before 
    /// storing it in the raster. For example one can convert data to colormap indices for display
    /// on a colormap display.</para><para>
    /// To setup custom "put" method please use <see cref="PutContig"/> property for contiguously
    /// packed samples and/or <see cref="PutSeparate"/> property for separated samples.</para>
    /// <para>
    /// The methods of <b>TiffRgbaImage</b> support the most commonly encountered flavors of TIFF.
    /// It is possible to extend this support by overriding the "get method" invoked by
    /// <see cref="GetRaster"/> to read TIFF image data. Details of doing this are a bit involved,
    /// it is best to make a copy of an existing get method and modify it to suit the needs of an
    /// application. To setup custom "get" method please use <see cref="Get"/> property.</para>
    /// </remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    class TiffRgbaImage
    {       
        internal const string photoTag = "PhotometricInterpretation";

        /// <summary>
        /// image handle
        /// </summary>
        private Tiff tif;

        /// <summary>
        /// stop on read error
        /// </summary>
        private bool stoponerr;

        /// <summary>
        /// data is packed/separate
        /// </summary>
        private bool isContig;

        /// <summary>
        /// type of alpha data present
        /// </summary>
        private ExtraSample alpha;

        /// <summary>
        /// image width
        /// </summary>
        private int width;

        /// <summary>
        /// image height
        /// </summary>
        private int height;

        /// <summary>
        /// image bits/sample
        /// </summary>
        private short bitspersample;

        /// <summary>
        /// image samples/pixel
        /// </summary>
        private short samplesperpixel;

        /// <summary>
        /// image orientation
        /// </summary>
        private Orientation orientation;

        /// <summary>
        /// requested orientation
        /// </summary>
        private Orientation req_orientation;

        /// <summary>
        /// image photometric interp
        /// </summary>
        private Photometric photometric;

        /// <summary>
        /// colormap pallete
        /// </summary>
        private short[] redcmap;

        private short[] greencmap;

        private short[] bluecmap;

        private GetDelegate get;
        private PutContigDelegate putContig;
        private PutSeparateDelegate putSeparate;

        /// <summary>
        /// sample mapping array
        /// </summary>
        private byte[] Map;

        /// <summary>
        /// black and white map
        /// </summary>
        private int[][] BWmap;

        /// <summary>
        /// palette image map
        /// </summary>
        private int[][] PALmap;

        /// <summary>
        /// YCbCr conversion state
        /// </summary>
        private TiffYCbCrToRGB ycbcr;

        /// <summary>
        /// CIE L*a*b conversion state
        /// </summary>
        private TiffCIELabToRGB cielab;

        private static TiffDisplay display_sRGB = new TiffDisplay(
            // XYZ -> luminance matrix
            new float[] { 3.2410F, -1.5374F, -0.4986F },
            new float[] { -0.9692F, 1.8760F, 0.0416F },
            new float[] { 0.0556F, -0.2040F, 1.0570F },
            100.0F, 100.0F, 100.0F,  // Light o/p for reference white
            255, 255, 255,  // Pixel values for ref. white
            1.0F, 1.0F, 1.0F,  // Residual light o/p for black pixel
            2.4F, 2.4F, 2.4F  // Gamma values for the three guns
        );

        private const int A1 = 0xff << 24;

        // Helper constants used in Orientation tag handling
        private const int FLIP_VERTICALLY = 0x01;
        private const int FLIP_HORIZONTALLY = 0x02;

        internal int row_offset;
        internal int col_offset;

        /// <summary>
        /// Delegate for "put" method (the method that is called to pack pixel data in the raster)
        /// used when converting contiguously packed samples.
        /// </summary>
        /// <param name="img">An instance of the <see cref="TiffRgbaImage"/> class.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="rasterOffset">The zero-based byte offset in <paramref name="raster"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="rasterShift">The value that should be added to
        /// <paramref name="rasterOffset"/> after each row processed.</param>
        /// <param name="x">The x-coordinate of the first pixel in block of pixels to be decoded.</param>
        /// <param name="y">The y-coordinate of the first pixel in block of pixels to be decoded.</param>
        /// <param name="width">The block width.</param>
        /// <param name="height">The block height.</param>
        /// <param name="buffer">The buffer with image data.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin reading image bytes.</param>
        /// <param name="bufferShift">The value that should be added to <paramref name="offset"/>
        /// after each row processed.</param>
        /// <remarks><para>
        /// The image reading and conversion methods invoke "put" methods to copy/image/whatever
        /// tiles of raw image data. A default set of methods is provided to convert/copy raw
        /// image data to 8-bit packed ABGR format rasters. Applications can supply alternate
        /// methods that unpack the data into a different format or, for example, unpack the data
        /// and draw the unpacked raster on the display.
        /// </para><para>
        /// To setup custom "put" method for contiguously packed samples please use
        /// <see cref="PutContig"/> property.</para>
        /// <para>
        /// The <paramref name="bufferShift"/> is usually 0. It is greater than 0 if width of strip
        /// being converted is greater than image width or part of the tile being converted is
        /// outside the image (may be true for tiles on the right and bottom edge of the image).
        /// In other words, <paramref name="bufferShift"/> is used to make up for any padding on
        /// the end of each line of the buffer with image data.
        /// </para><para>
        /// The <paramref name="rasterShift"/> is 0 if width of tile being converted is equal to
        /// image width and image data should not be flipped vertically. In other circumstances
        /// <paramref name="rasterShift"/> is used to make up for any padding on the end of each
        /// line of the raster and/or for flipping purposes.
        /// </para></remarks>
        public delegate void PutContigDelegate(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift);

        /// <summary>
        /// Delegate for "put" method (the method that is called to pack pixel data in the raster)
        /// used when converting separated samples.
        /// </summary>
        /// <param name="img">An instance of the <see cref="TiffRgbaImage"/> class.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="rasterOffset">The zero-based byte offset in <paramref name="raster"/> at
        /// which to begin storing decoded bytes.</param>
        /// <param name="rasterShift">The value that should be added to
        /// <paramref name="rasterOffset"/> after each row processed.</param>
        /// <param name="x">The x-coordinate of the first pixel in block of pixels to be decoded.</param>
        /// <param name="y">The y-coordinate of the first pixel in block of pixels to be decoded.</param>
        /// <param name="width">The block width.</param>
        /// <param name="height">The block height.</param>
        /// <param name="buffer">The buffer with image data.</param>
        /// <param name="offset1">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin reading image bytes that constitute first sample plane.</param>
        /// <param name="offset2">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin reading image bytes that constitute second sample plane.</param>
        /// <param name="offset3">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin reading image bytes that constitute third sample plane.</param>
        /// <param name="offset4">The zero-based byte offset in <paramref name="buffer"/> at
        /// which to begin reading image bytes that constitute fourth sample plane.</param>
        /// <param name="bufferShift">The value that should be added to <paramref name="offset1"/>,
        /// <paramref name="offset2"/>, <paramref name="offset3"/> and <paramref name="offset4"/>
        /// after each row processed.</param>
        /// <remarks><para>
        /// The image reading and conversion methods invoke "put" methods to copy/image/whatever
        /// tiles of raw image data. A default set of methods is provided to convert/copy raw
        /// image data to 8-bit packed ABGR format rasters. Applications can supply alternate
        /// methods that unpack the data into a different format or, for example, unpack the data
        /// and draw the unpacked raster on the display.
        /// </para><para>
        /// To setup custom "put" method for separated samples please use
        /// <see cref="PutSeparate"/> property.</para>
        /// <para>
        /// The <paramref name="bufferShift"/> is usually 0. It is greater than 0 if width of strip
        /// being converted is greater than image width or part of the tile being converted is
        /// outside the image (may be true for tiles on the right and bottom edge of the image).
        /// In other words, <paramref name="bufferShift"/> is used to make up for any padding on
        /// the end of each line of the buffer with image data.
        /// </para><para>
        /// The <paramref name="rasterShift"/> is 0 if width of tile being converted is equal to
        /// image width and image data should not be flipped vertically. In other circumstances
        /// <paramref name="rasterShift"/> is used to make up for any padding on the end of each
        /// line of the raster and/or for flipping purposes.
        /// </para></remarks>
        public delegate void PutSeparateDelegate(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift);

        /// <summary>
        /// Delegate for "get" method (the method that is called to produce RGBA raster).
        /// </summary>
        /// <param name="img">An instance of the <see cref="TiffRgbaImage"/> class.</param>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="raster"/> at which
        /// to begin storing decoded bytes.</param>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <returns><c>true</c> if the image was successfully read and decoded; otherwise,
        /// <c>false</c>.</returns>
        /// <remarks><para>
        /// A default set of methods is provided to read and convert/copy raw image data to 8-bit
        /// packed ABGR format rasters. Applications can supply alternate method for this.
        /// </para><para>
        /// To setup custom "get" method please use <see cref="Get"/> property.
        /// </para></remarks>
        public delegate bool GetDelegate(TiffRgbaImage img, int[] raster, int offset, int width, int height);

        private TiffRgbaImage()
        {
        }

        /// <summary>
        /// Creates new instance of the <see cref="TiffRgbaImage"/> class.
        /// </summary>
        /// <param name="tif">
        /// The instance of the <see cref="BitMiracle.LibTiff.Classic"/> class used to retrieve
        /// image data.
        /// </param>
        /// <param name="stopOnError">
        /// if set to <c>true</c> then an error will terminate the conversion; otherwise "get"
        /// methods will continue processing data until all the possible data in the image have
        /// been requested.
        /// </param>
        /// <param name="errorMsg">The error message (if any) gets placed here.</param>
        /// <returns>
        /// New instance of the <see cref="TiffRgbaImage"/> class if the image specified
        /// by <paramref name="tif"/> can be converted to RGBA format; otherwise, <c>null</c> is
        /// returned and <paramref name="errorMsg"/> contains the reason why it is being
        /// rejected.
        /// </returns>
        public static TiffRgbaImage Create(Tiff tif, bool stopOnError, out string errorMsg)
        {
            errorMsg = null;

            // Initialize to normal values
            TiffRgbaImage img = new TiffRgbaImage();
            img.row_offset = 0;
            img.col_offset = 0;
            img.redcmap = null;
            img.greencmap = null;
            img.bluecmap = null;
            img.req_orientation = Orientation.BOTLEFT; // It is the default
            img.tif = tif;
            img.stoponerr = stopOnError;

            FieldValue[] result = tif.GetFieldDefaulted(TiffTag.BITSPERSAMPLE);
            img.bitspersample = result[0].ToShort();
            switch (img.bitspersample)
            {
                case 1:
                case 2:
                case 4:
                case 8:
                case 16:
                    break;

                default:
                    errorMsg = string.Format(CultureInfo.InvariantCulture,
                        "Sorry, can not handle images with {0}-bit samples", img.bitspersample);
                    return null;
            }

            img.alpha = 0;
            result = tif.GetFieldDefaulted(TiffTag.SAMPLESPERPIXEL);
            img.samplesperpixel = result[0].ToShort();

            result = tif.GetFieldDefaulted(TiffTag.EXTRASAMPLES);
            short extrasamples = result[0].ToShort();
            byte[] sampleinfo = result[1].ToByteArray();

            if (extrasamples >= 1)
            {
                switch ((ExtraSample)sampleinfo[0])
                {
                    case ExtraSample.UNSPECIFIED:
                        if (img.samplesperpixel > 3)
                        {
                            // Workaround for some images without correct info about alpha channel
                            img.alpha = ExtraSample.ASSOCALPHA;
                        }
                        break;

                    case ExtraSample.ASSOCALPHA:
                        // data is pre-multiplied
                    case ExtraSample.UNASSALPHA:
                        // data is not pre-multiplied
                        img.alpha = (ExtraSample)sampleinfo[0];
                        break;
                }
            }

            if (Tiff.DEFAULT_EXTRASAMPLE_AS_ALPHA)
            {
                result = tif.GetField(TiffTag.PHOTOMETRIC);
                if (result == null)
                    img.photometric = Photometric.MINISWHITE;

                if (extrasamples == 0 && img.samplesperpixel == 4 && img.photometric == Photometric.RGB)
                {
                    img.alpha = ExtraSample.ASSOCALPHA;
                    extrasamples = 1;
                }
            }

            int colorchannels = img.samplesperpixel - extrasamples;

            result = tif.GetFieldDefaulted(TiffTag.COMPRESSION);
            Compression compress = (Compression)result[0].ToInt();

            result = tif.GetFieldDefaulted(TiffTag.PLANARCONFIG);
            PlanarConfig planarconfig = (PlanarConfig)result[0].ToShort();

            result = tif.GetField(TiffTag.PHOTOMETRIC);
            if (result == null)
            {
                switch (colorchannels)
                {
                    case 1:
                        if (img.isCCITTCompression())
                            img.photometric = Photometric.MINISWHITE;
                        else
                            img.photometric = Photometric.MINISBLACK;
                        break;

                    case 3:
                        img.photometric = Photometric.RGB;
                        break;

                    default:
                        errorMsg = string.Format(CultureInfo.InvariantCulture, "Missing needed {0} tag", photoTag);
                        return null;
                }
            }
            else
                img.photometric = (Photometric)result[0].ToInt();

            switch (img.photometric)
            {
                case Photometric.PALETTE:
                    result = tif.GetField(TiffTag.COLORMAP);
                    if (result == null)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture, "Missing required \"Colormap\" tag");
                        return null;
                    }

                    short[] red_orig = result[0].ToShortArray();
                    short[] green_orig = result[1].ToShortArray();
                    short[] blue_orig = result[2].ToShortArray();

                    // copy the colormaps so we can modify them
                    int n_color = (1 << img.bitspersample);
                    img.redcmap = new short[n_color];
                    img.greencmap = new short[n_color];
                    img.bluecmap = new short[n_color];

                    Buffer.BlockCopy(red_orig, 0, img.redcmap, 0, n_color * sizeof(short));
                    Buffer.BlockCopy(green_orig, 0, img.greencmap, 0, n_color * sizeof(short));
                    Buffer.BlockCopy(blue_orig, 0, img.bluecmap, 0, n_color * sizeof(short));

                    if (planarconfig == PlanarConfig.CONTIG &&
                        img.samplesperpixel != 1 && img.bitspersample < 8)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}",
                            photoTag, img.photometric, "Samples/pixel", img.samplesperpixel, img.bitspersample);
                        return null;
                    }
                    break;

                case Photometric.MINISWHITE:
                case Photometric.MINISBLACK:
                    if (planarconfig == PlanarConfig.CONTIG &&
                        img.samplesperpixel != 1 && img.bitspersample < 8)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle contiguous data with {0}={1}, and {2}={3} and Bits/Sample={4}",
                            photoTag, img.photometric, "Samples/pixel", img.samplesperpixel, img.bitspersample);
                        return null;
                    }
                    break;

                case Photometric.YCBCR:
                    // It would probably be nice to have a reality check here.
                    if (planarconfig == PlanarConfig.CONTIG)
                    {
                        // can rely on LibJpeg.Net to convert to RGB
                        // XXX should restore current state on exit
                        switch (compress)
                        {
                            case Compression.JPEG:
                                // TODO: when complete tests verify complete desubsampling and
                                // YCbCr handling, remove use of JPEGCOLORMODE in favor of native
                                // handling
                                tif.SetField(TiffTag.JPEGCOLORMODE, JpegColorMode.RGB);
                                img.photometric = Photometric.RGB;
                                break;

                            default:
                                // do nothing
                                break;
                        }
                    }

                    // TODO: if at all meaningful and useful, make more complete support check
                    // here, or better still, refactor to let supporting code decide whether there
                    // is support and what meaningfull error to return
                    break;

                case Photometric.RGB:
                    if (colorchannels < 3)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle RGB image with {0}={1}", "Color channels", colorchannels);
                        return null;
                    }
                    break;

                case Photometric.SEPARATED:
                    result = tif.GetFieldDefaulted(TiffTag.INKSET);
                    InkSet inkset = (InkSet)result[0].ToByte();

                    if (inkset != InkSet.CMYK)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle separated image with {0}={1}", "InkSet", inkset);
                        return null;
                    }

                    if (img.samplesperpixel < 4)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle separated image with {0}={1}", "Samples/pixel", img.samplesperpixel);
                        return null;
                    }
                    break;

                case Photometric.LOGL:
                    if (compress != Compression.SGILOG)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, LogL data must have {0}={1}", "Compression", Compression.SGILOG);
                        return null;
                    }

                    tif.SetField(TiffTag.SGILOGDATAFMT, 3); // 8-bit RGB monitor values. 
                    img.photometric = Photometric.MINISBLACK; // little white lie
                    img.bitspersample = 8;
                    break;

                case Photometric.LOGLUV:
                    if (compress != Compression.SGILOG && compress != Compression.SGILOG24)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, LogLuv data must have {0}={1} or {2}", "Compression", Compression.SGILOG, Compression.SGILOG24);
                        return null;
                    }

                    if (planarconfig != PlanarConfig.CONTIG)
                    {
                        errorMsg = string.Format(CultureInfo.InvariantCulture,
                            "Sorry, can not handle LogLuv images with {0}={1}", "Planarconfiguration", planarconfig);
                        return null;
                    }

                    tif.SetField(TiffTag.SGILOGDATAFMT, 3); // 8-bit RGB monitor values. 
                    img.photometric = Photometric.RGB; // little white lie
                    img.bitspersample = 8;
                    break;

                case Photometric.CIELAB:
                    break;

                default:
                    errorMsg = string.Format(CultureInfo.InvariantCulture,
                        "Sorry, can not handle image with {0}={1}", photoTag, img.photometric);
                    return null;
            }

            img.Map = null;
            img.BWmap = null;
            img.PALmap = null;
            img.ycbcr = null;
            img.cielab = null;

            result = tif.GetField(TiffTag.IMAGEWIDTH);
            img.width = result[0].ToInt();

            result = tif.GetField(TiffTag.IMAGELENGTH);
            img.height = result[0].ToInt();

            result = tif.GetFieldDefaulted(TiffTag.ORIENTATION);
            img.orientation = (Orientation)result[0].ToByte();

            img.isContig = !(planarconfig == PlanarConfig.SEPARATE && colorchannels > 1);
            if (img.isContig)
            {
                if (!img.pickContigCase())
                {
                    errorMsg = "Sorry, can not handle image";
                    return null;
                }
            }
            else
            {
                if (!img.pickSeparateCase())
                {
                    errorMsg = "Sorry, can not handle image";
                    return null;
                }
            }

            return img;
        }

        /// <summary>
        /// Gets a value indicating whether image data has contiguous (packed) or separated samples.
        /// </summary>
        /// <value><c>true</c> if this image data has contiguous (packed) samples; otherwise,
        /// <c>false</c>.</value>
        public bool IsContig
        {
            get
            {
                return isContig;
            }
        }

        /// <summary>
        /// Gets the type of alpha data present.
        /// </summary>
        /// <value>The type of alpha data present.</value>
        public ExtraSample Alpha
        {
            get
            {
                return alpha;
            }
        }

        /// <summary>
        /// Gets the image width.
        /// </summary>
        /// <value>The image width.</value>
        public int Width
        {
            get
            {
                return width;
            }
        }

        /// <summary>
        /// Gets the image height.
        /// </summary>
        /// <value>The image height.</value>
        public int Height
        {
            get
            {
                return height;
            }
        }

        /// <summary>
        /// Gets the image bits per sample count.
        /// </summary>
        /// <value>The image bits per sample count.</value>
        public short BitsPerSample
        {
            get
            {
                return bitspersample;
            }
        }

        /// <summary>
        /// Gets the image samples per pixel count.
        /// </summary>
        /// <value>The image samples per pixel count.</value>
        public short SamplesPerPixel
        {
            get
            {
                return samplesperpixel;
            }
        }

        /// <summary>
        /// Gets the image orientation.
        /// </summary>
        /// <value>The image orientation.</value>
        public Orientation Orientation
        {
            get
            {
                return orientation;
            }
        }

        /// <summary>
        /// Gets or sets the requested orientation.
        /// </summary>
        /// <value>The requested orientation.</value>
        /// <remarks>The <see cref="GetRaster"/> method uses this value when placing converted
        /// image data into raster buffer.</remarks>
        public Orientation ReqOrientation
        {
            get
            {
                return req_orientation;
            }
            set
            {
                req_orientation = value;
            }
        }

        /// <summary>
        /// Gets the photometric interpretation of the image data.
        /// </summary>
        /// <value>The photometric interpretation of the image data.</value>
        public Photometric Photometric
        {
            get
            {
                return photometric;
            }
        }

        /// <summary>
        /// Gets or sets the "get" method (the method that is called to produce RGBA raster).
        /// </summary>
        /// <value>The "get" method.</value>
        public GetDelegate Get
        {
            get
            {
                return get;
            }
            set
            {
                get = value;
            }
        }

        /// <summary>
        /// Gets or sets the "put" method (the method that is called to pack pixel data in the
        /// raster) used when converting contiguously packed samples.
        /// </summary>
        /// <value>The "put" method used when converting contiguously packed samples.</value>
        public PutContigDelegate PutContig
        {
            get
            {
                return putContig;
            }
            set
            {
                putContig = value;
            }
        }

        /// <summary>
        /// Gets or sets the "put" method (the method that is called to pack pixel data in the
        /// raster) used when converting separated samples.
        /// </summary>
        /// <value>The "put" method used when converting separated samples.</value>
        public PutSeparateDelegate PutSeparate
        {
            get
            {
                return putSeparate;
            }
            set
            {
                putSeparate = value;
            }
        }

        /// <summary>
        /// Reads the underlaying TIFF image and decodes it into RGBA format raster.
        /// </summary>
        /// <param name="raster">The raster (the buffer to place decoded image data to).</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="raster"/> at which
        /// to begin storing decoded bytes.</param>
        /// <param name="width">The raster width.</param>
        /// <param name="height">The raster height.</param>
        /// <returns><c>true</c> if the image was successfully read and decoded; otherwise,
        /// <c>false</c>.</returns>
        /// <remarks><para>
        /// <b>GetRaster</b> reads image into memory using current "get" (<see cref="Get"/>) method,
        /// storing the result in the user supplied RGBA <paramref name="raster"/> using one of
        /// the "put" (<see cref="PutContig"/> or <see cref="PutSeparate"/>) methods. The raster
        /// is assumed to be an array of <paramref name="width"/> times <paramref name="height"/>
        /// 32-bit entries, where <paramref name="width"/> must be less than or equal to the width
        /// of the image (<paramref name="height"/> may be any non-zero size). If the raster
        /// dimensions are smaller than the image, the image data is cropped to the raster bounds.
        /// If the raster height is greater than that of the image, then the image data placement
        /// depends on the value of <see cref="ReqOrientation"/> property. Note that the raster is
        /// assumed to be organized such that the pixel at location (x, y) is
        /// <paramref name="raster"/>[y * width + x]; with the raster origin specified by the
        /// value of <see cref="ReqOrientation"/> property.
        /// </para><para>
        /// Raster pixels are 8-bit packed red, green, blue, alpha samples. The 
        /// <see cref="Tiff.GetR"/>, <see cref="Tiff.GetG"/>, <see cref="Tiff.GetB"/>, and
        /// <see cref="Tiff.GetA"/> should be used to access individual samples. Images without
        /// Associated Alpha matting information have a constant Alpha of 1.0 (255).
        /// </para><para>
        /// <b>GetRaster</b> converts non-8-bit images by scaling sample values. Palette,
        /// grayscale, bilevel, CMYK, and YCbCr images are converted to RGB transparently.
        /// Raster pixels are returned uncorrected by any colorimetry information present in
        /// the directory.
        /// </para><para>
        /// Samples must be either 1, 2, 4, 8, or 16 bits. Colorimetric samples/pixel must be
        /// either 1, 3, or 4 (i.e. SamplesPerPixel minus ExtraSamples).
        /// </para><para>
        /// Palette image colormaps that appear to be incorrectly written as 8-bit values are
        /// automatically scaled to 16-bits.
        /// </para><para>
        /// All error messages are directed to the current error handler.
        /// </para></remarks>
        public bool GetRaster(int[] raster, int offset, int width, int height)
        {
            if (get == null)
            {
                Tiff.ErrorExt(tif, tif.m_clientdata, tif.FileName(), "No \"get\" method setup");
                return false;
            }

            return get(this, raster, offset, width, height);
        }

        private static int PACK(int r, int g, int b)
        {
            return (r | (g << 8) | (b << 16) | A1);
        }

        private static int PACK4(int r, int g, int b, int a)
        {
            return (r | (g << 8) | (b << 16) | (a << 24));
        }

        private static int W2B(short v)
        {
            return ((v >> 8) & 0xff);
        }

        private static int PACKW(short r, short g, short b)
        {
            return (W2B(r) | (W2B(g) << 8) | (W2B(b) << 16) | (int)A1);
        }

        private static int PACKW4(short r, short g, short b, short a)
        {
            return (W2B(r) | (W2B(g) << 8) | (W2B(b) << 16) | (W2B(a) << 24));
        }

        /// <summary>
        /// Palette images with &lt;= 8 bits/sample are handled with a table to avoid lots of shifts
        /// and masks. The table is setup so that put*cmaptile (below) can retrieve 8 / bitspersample
        /// pixel values simply by indexing into the table with one number.
        /// </summary>
        private void CMAP(int x, int i, ref int j)
        {
            PALmap[i][j++] = PACK(redcmap[x] & 0xff, greencmap[x] & 0xff, bluecmap[x] & 0xff);
        }

        /// <summary>
        /// Greyscale images with less than 8 bits/sample are handled with a table to avoid lots
        /// of shifts and masks. The table is setup so that put*bwtile (below) can retrieve
        /// 8 / bitspersample pixel values simply by indexing into the table with one number.
        /// </summary>
        private void GREY(int x, int i, ref int j)
        {
            int c = Map[x];
            BWmap[i][j++] = PACK(c, c, c);
        }

        /// <summary>
        /// Get an tile-organized image that has
        /// PlanarConfiguration contiguous if SamplesPerPixel > 1
        ///  or
        /// SamplesPerPixel == 1
        /// </summary>
        private static bool gtTileContig(TiffRgbaImage img, int[] raster, int offset, int width, int height)
        {
            byte[] buf = new byte[img.tif.TileSize()];

            FieldValue[] result = img.tif.GetField(TiffTag.TILEWIDTH);
            int tileWidth = result[0].ToInt();

            result = img.tif.GetField(TiffTag.TILELENGTH);
            int tileHeight = result[0].ToInt();

            int flip = img.setorientation();
            int y;
            int rasterShift;
            if ((flip & FLIP_VERTICALLY) != 0)
            {
                y = height - 1;
                rasterShift = -(tileWidth + width);
            }
            else
            {
                y = 0;
                rasterShift = -(tileWidth - width);
            }

            bool ret = true;
            for (int row = 0; row < height; )
            {
                int rowstoread = tileHeight - (row + img.row_offset) % tileHeight;
                int nrow = (row + rowstoread > height ? height - row : rowstoread);
                for (int col = 0; col < width; col += tileWidth)
                {
                    if (img.tif.ReadTile(buf, 0, col + img.col_offset, row + img.row_offset, 0, 0) < 0 && img.stoponerr)
                    {
                        ret = false;
                        break;
                    }

                    int pos = ((row + img.row_offset) % tileHeight) * img.tif.TileRowSize();

                    if (col + tileWidth > width)
                    {
                        // Tile is clipped horizontally. Calculate visible portion and
                        // skewing factors.
                        int npix = width - col;
                        int bufferShift = tileWidth - npix;

                        img.putContig(img, raster, offset + y * width + col, rasterShift + bufferShift,
                            col, y, npix, nrow, buf, pos, bufferShift);
                    }
                    else
                    {
                        img.putContig(img, raster, offset + y * width + col, rasterShift,
                            col, y, tileWidth, nrow, buf, pos, 0);
                    }
                }

                y += ((flip & FLIP_VERTICALLY) != 0 ? -nrow : nrow);
                row += nrow;
            }

            if ((flip & FLIP_HORIZONTALLY) != 0)
            {
                for (int line = 0; line < height; line++)
                {
                    int left = offset + line * width;
                    int right = left + width - 1;

                    while (left < right)
                    {
                        int temp = raster[left];
                        raster[left] = raster[right];
                        raster[right] = temp;
                        left++;
                        right--;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Get an tile-organized image that has
        /// SamplesPerPixel > 1
        /// PlanarConfiguration separated
        /// We assume that all such images are RGB.
        /// </summary>
        private static bool gtTileSeparate(TiffRgbaImage img, int[] raster, int offset, int width, int height)
        {
            int tilesize = img.tif.TileSize();
            byte[] buf = new byte[(img.alpha != 0 ? 4 : 3) * tilesize];

            int p0 = 0;
            int p1 = p0 + tilesize;
            int p2 = p1 + tilesize;
            int pa = (img.alpha != 0 ? (p2 + tilesize) : -1);

            FieldValue[] result = img.tif.GetField(TiffTag.TILEWIDTH);
            int tileWidth = result[0].ToInt();

            result = img.tif.GetField(TiffTag.TILELENGTH);
            int tileHeight = result[0].ToInt();

            int flip = img.setorientation();
            int y;
            int rasterShift;
            if ((flip & FLIP_VERTICALLY) != 0)
            {
                y = height - 1;
                rasterShift = -(tileWidth + width);
            }
            else
            {
                y = 0;
                rasterShift = -(tileWidth - width);
            }

            bool ret = true;
            for (int row = 0; row < height; )
            {
                int rowstoread = tileHeight - (row + img.row_offset) % tileHeight;
                int nrow = (row + rowstoread > height ? height - row : rowstoread);
                for (int col = 0; col < width; col += tileWidth)
                {
                    if (img.tif.ReadTile(buf, p0, col + img.col_offset, row + img.row_offset, 0, 0) < 0 && img.stoponerr)
                    {
                        ret = false;
                        break;
                    }

                    if (img.tif.ReadTile(buf, p1, col + img.col_offset, row + img.row_offset, 0, 1) < 0 && img.stoponerr)
                    {
                        ret = false;
                        break;
                    }

                    if (img.tif.ReadTile(buf, p2, col + img.col_offset, row + img.row_offset, 0, 2) < 0 && img.stoponerr)
                    {
                        ret = false;
                        break;
                    }

                    if (img.alpha != 0)
                    {
                        if (img.tif.ReadTile(buf, pa, col + img.col_offset, row + img.row_offset, 0, 3) < 0 && img.stoponerr)
                        {
                            ret = false;
                            break;
                        }
                    }

                    int pos = ((row + img.row_offset) % tileHeight) * img.tif.TileRowSize();

                    if (col + tileWidth > width)
                    {
                        // Tile is clipped horizontally.
                        // Calculate visible portion and skewing factors.
                        int npix = width - col;
                        int bufferShift = tileWidth - npix;

                        img.putSeparate(img, raster, offset + y * width + col, rasterShift + bufferShift,
                            col, y, npix, nrow,
                            buf, p0 + pos, p1 + pos, p2 + pos, img.alpha != 0 ? (pa + pos) : -1, bufferShift);
                    }
                    else
                    {
                        img.putSeparate(img, raster, offset + y * width + col, rasterShift,
                            col, y, tileWidth, nrow,
                            buf, p0 + pos, p1 + pos, p2 + pos, img.alpha != 0 ? (pa + pos) : -1, 0);
                    }
                }

                y += ((flip & FLIP_VERTICALLY) != 0 ? -nrow : nrow);
                row += nrow;
            }

            if ((flip & FLIP_HORIZONTALLY) != 0)
            {
                for (int line = 0; line < height; line++)
                {
                    int left = offset + line * width;
                    int right = left + width - 1;

                    while (left < right)
                    {
                        int temp = raster[left];
                        raster[left] = raster[right];
                        raster[right] = temp;
                        left++;
                        right--;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Get a strip-organized image that has 
        /// PlanarConfiguration contiguous if SamplesPerPixel > 1
        ///  or
        /// SamplesPerPixel == 1
        /// </summary>
        private static bool gtStripContig(TiffRgbaImage img, int[] raster, int offset, int width, int height)
        {
            byte[] buf = new byte[img.tif.StripSize()];

            int flip = img.setorientation();
            int y;
            int rasterShift;
            if ((flip & FLIP_VERTICALLY) != 0)
            {
                y = height - 1;
                rasterShift = -(width + width);
            }
            else
            {
                y = 0;
                rasterShift = -(width - width);
            }

            FieldValue[] result = img.tif.GetFieldDefaulted(TiffTag.ROWSPERSTRIP);
            int rowsperstrip = result[0].ToInt();
            if (rowsperstrip == -1)
            {
                // San Chen <bigsan.chen@gmail.com>
                // HACK: should be UInt32.MaxValue
                rowsperstrip = Int32.MaxValue;
            }

            result = img.tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
            short subsamplingver = result[1].ToShort();

            int scanline = img.tif.newScanlineSize();
            int bufferShift = (width < img.width ? img.width - width : 0);
            bool ret = true;

            for (int row = 0; row < height; )
            {
                int rowstoread = rowsperstrip - (row + img.row_offset) % rowsperstrip;
                int nrow = (row + rowstoread > height ? height - row : rowstoread);
                int nrowsub = nrow;
                if ((nrowsub % subsamplingver) != 0)
                    nrowsub += subsamplingver - nrowsub % subsamplingver;

                if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(row + img.row_offset, 0), buf, 0, ((row + img.row_offset) % rowsperstrip + nrowsub) * scanline) < 0 && img.stoponerr)
                {
                    ret = false;
                    break;
                }

                int pos = ((row + img.row_offset) % rowsperstrip) * scanline;
                img.putContig(img, raster, offset + y * width, rasterShift, 0, y, width, nrow, buf, pos, bufferShift);
                y += (flip & FLIP_VERTICALLY) != 0 ? -nrow : nrow;
                row += nrow;
            }

            if ((flip & FLIP_HORIZONTALLY) != 0)
            {
                for (int line = 0; line < height; line++)
                {
                    int left = offset + line * width;
                    int right = left + width - 1;

                    while (left < right)
                    {
                        int temp = raster[left];
                        raster[left] = raster[right];
                        raster[right] = temp;
                        left++;
                        right--;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Get a strip-organized image with
        ///  SamplesPerPixel > 1
        ///  PlanarConfiguration separated
        /// We assume that all such images are RGB.
        /// </summary>
        private static bool gtStripSeparate(TiffRgbaImage img, int[] raster, int offset, int width, int height)
        {
            int stripsize = img.tif.StripSize();
            byte[] buf = new byte[(img.alpha != 0 ? 4 : 3) * stripsize];

            int p0 = 0;
            int p1 = p0 + stripsize;
            int p2 = p1 + stripsize;
            int pa = p2 + stripsize;
            pa = (img.alpha != 0 ? (p2 + stripsize) : -1);

            int flip = img.setorientation();
            int y;
            int rasterShift;
            if ((flip & FLIP_VERTICALLY) != 0)
            {
                y = height - 1;
                rasterShift = -(width + width);
            }
            else
            {
                y = 0;
                rasterShift = -(width - width);
            }

            FieldValue[] result = img.tif.GetFieldDefaulted(TiffTag.ROWSPERSTRIP);
            int rowsperstrip = result[0].ToInt();

            int scanline = img.tif.ScanlineSize();
            int bufferShift = (width < img.width ? img.width - width : 0);
            bool ret = true;
            for (int row = 0; row < height; )
            {
                int rowstoread = rowsperstrip - (row + img.row_offset) % rowsperstrip;
                int nrow = (row + rowstoread > height ? height - row : rowstoread);
                int offset_row = row + img.row_offset;

                if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(offset_row, 0), buf, p0, ((row + img.row_offset) % rowsperstrip + nrow) * scanline) < 0 && img.stoponerr)
                {
                    ret = false;
                    break;
                }

                if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(offset_row, 1), buf, p1, ((row + img.row_offset) % rowsperstrip + nrow) * scanline) < 0 && img.stoponerr)
                {
                    ret = false;
                    break;
                }

                if (img.tif.ReadEncodedStrip(img.tif.ComputeStrip(offset_row, 2), buf, p2, ((row + img.row_offset) % rowsperstrip + nrow) * scanline) < 0 && img.stoponerr)
                {
                    ret = false;
                    break;
                }

                if (img.alpha != 0)
                {
                    if ((img.tif.ReadEncodedStrip(img.tif.ComputeStrip(offset_row, 3), buf, pa, ((row + img.row_offset) % rowsperstrip + nrow) * scanline) < 0 && img.stoponerr))
                    {
                        ret = false;
                        break;
                    }
                }

                int pos = ((row + img.row_offset) % rowsperstrip) * scanline;
                
                img.putSeparate(img, raster, offset + y * width, rasterShift,
                    0, y, width, nrow,
                    buf, p0 + pos, p1 + pos, p2 + pos, img.alpha != 0 ? (pa + pos) : -1, bufferShift);

                y += (flip & FLIP_VERTICALLY) != 0 ? -nrow : nrow;
                row += nrow;
            }

            if ((flip & FLIP_HORIZONTALLY) != 0)
            {
                for (int line = 0; line < height; line++)
                {
                    int left = offset + line * width;
                    int right = left + width - 1;

                    while (left < right)
                    {
                        int temp = raster[left];
                        raster[left] = raster[right];
                        raster[right] = temp;
                        left++;
                        right--;
                    }
                }
            }

            return ret;
        }

        private bool isCCITTCompression()
        {
            FieldValue[] result = tif.GetField(TiffTag.COMPRESSION);
            Compression compress = (Compression)result[0].ToInt();

            return (compress == Compression.CCITTFAX3 ||
                compress == Compression.CCITTFAX4 ||
                compress == Compression.CCITTRLE ||
                compress == Compression.CCITTRLEW);
        }

        private int setorientation()
        {
            switch (orientation)
            {
                case Orientation.TOPLEFT:
                case Orientation.LEFTTOP:
                    if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
                        return FLIP_HORIZONTALLY;
                    else if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
                        return FLIP_HORIZONTALLY | FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
                        return FLIP_VERTICALLY;

                    return 0;

                case Orientation.TOPRIGHT:
                case Orientation.RIGHTTOP:
                    if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
                        return FLIP_HORIZONTALLY;
                    else if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
                        return FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
                        return FLIP_HORIZONTALLY | FLIP_VERTICALLY;

                    return 0;

                case Orientation.BOTRIGHT:
                case Orientation.RIGHTBOT:
                    if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
                        return FLIP_HORIZONTALLY | FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
                        return FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.BOTLEFT || req_orientation == Orientation.LEFTBOT)
                        return FLIP_HORIZONTALLY;

                    return 0;

                case Orientation.BOTLEFT:
                case Orientation.LEFTBOT:
                    if (req_orientation == Orientation.TOPLEFT || req_orientation == Orientation.LEFTTOP)
                        return FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.TOPRIGHT || req_orientation == Orientation.RIGHTTOP)
                        return FLIP_HORIZONTALLY | FLIP_VERTICALLY;
                    else if (req_orientation == Orientation.BOTRIGHT || req_orientation == Orientation.RIGHTBOT)
                        return FLIP_HORIZONTALLY;

                    return 0;
            }

            return 0;
        }

        /// <summary>
        /// Select the appropriate conversion routine for packed data.
        /// </summary>
        private bool pickContigCase()
        {
            get = tif.IsTiled() ? new GetDelegate(gtTileContig) : new GetDelegate(gtStripContig);
            putContig = null;

            switch (photometric)
            {
                case Photometric.RGB:
                    switch (bitspersample)
                    {
                        case 8:
                            if (alpha == ExtraSample.ASSOCALPHA)
                                putContig = putRGBAAcontig8bittile;
                            else if (alpha == ExtraSample.UNASSALPHA)
                                putContig = putRGBUAcontig8bittile;
                            else
                                putContig = putRGBcontig8bittile;
                            break;

                        case 16:
                            if (alpha == ExtraSample.ASSOCALPHA)
                                putContig = putRGBAAcontig16bittile;
                            else if (alpha == ExtraSample.UNASSALPHA)
                                putContig = putRGBUAcontig16bittile;
                            else
                                putContig = putRGBcontig16bittile;
                            break;
                    }
                    break;

                case Photometric.SEPARATED:
                    if (buildMap())
                    {
                        if (bitspersample == 8)
                        {
                            if (Map == null)
                                putContig = putRGBcontig8bitCMYKtile;
                            else
                                putContig = putRGBcontig8bitCMYKMaptile;
                        }
                    }
                    break;

                case Photometric.PALETTE:
                    if (buildMap())
                    {
                        switch (bitspersample)
                        {
                            case 8:
                                putContig = put8bitcmaptile;
                                break;
                            case 4:
                                putContig = put4bitcmaptile;
                                break;
                            case 2:
                                putContig = put2bitcmaptile;
                                break;
                            case 1:
                                putContig = put1bitcmaptile;
                                break;
                        }
                    }
                    break;

                case Photometric.MINISWHITE:
                case Photometric.MINISBLACK:
                    if (buildMap())
                    {
                        switch (bitspersample)
                        {
                            case 16:
                                putContig = put16bitbwtile;
                                break;
                            case 8:
                                putContig = putgreytile;
                                break;
                            case 4:
                                putContig = put4bitbwtile;
                                break;
                            case 2:
                                putContig = put2bitbwtile;
                                break;
                            case 1:
                                putContig = put1bitbwtile;
                                break;
                        }
                    }
                    break;

                case Photometric.YCBCR:
                    if (bitspersample == 8)
                    {
                        if (initYCbCrConversion())
                        {
                            // The 6.0 spec says that subsampling must be one of 1, 2, or 4, and
                            // that vertical subsampling must always be <= horizontal subsampling;
                            // so there are only a few possibilities and we just enumerate the cases.
                            // Joris: added support for the [1, 2] case, nonetheless, to accommodate
                            // some OJPEG files
                            FieldValue[] result = tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
                            short SubsamplingHor = result[0].ToShort();
                            short SubsamplingVer = result[1].ToShort();

                            switch (((ushort)SubsamplingHor << 4) | (ushort)SubsamplingVer)
                            {
                                case 0x44:
                                    putContig = putcontig8bitYCbCr44tile;
                                    break;
                                case 0x42:
                                    putContig = putcontig8bitYCbCr42tile;
                                    break;
                                case 0x41:
                                    putContig = putcontig8bitYCbCr41tile;
                                    break;
                                case 0x22:
                                    putContig = putcontig8bitYCbCr22tile;
                                    break;
                                case 0x21:
                                    putContig = putcontig8bitYCbCr21tile;
                                    break;
                                case 0x12:
                                    putContig = putcontig8bitYCbCr12tile;
                                    break;
                                case 0x11:
                                    putContig = putcontig8bitYCbCr11tile;
                                    break;
                            }
                        }
                    }
                    break;

                case Photometric.CIELAB:
                    if (buildMap())
                    {
                        if (bitspersample == 8)
                            putContig = initCIELabConversion();
                    }
                    break;
            }

            return (putContig != null);
        }

        /// <summary>
        /// Select the appropriate conversion routine for unpacked data.
        /// NB: we assume that unpacked single channel data is directed to the "packed routines.
        /// </summary>
        private bool pickSeparateCase()
        {
            get = tif.IsTiled() ? new GetDelegate(gtTileSeparate) : new GetDelegate(gtStripSeparate);
            putSeparate = null;

            switch (photometric)
            {
                case Photometric.RGB:
                    switch (bitspersample)
                    {
                        case 8:
                            if (alpha == ExtraSample.ASSOCALPHA)
                                putSeparate = putRGBAAseparate8bittile;
                            else if (alpha == ExtraSample.UNASSALPHA)
                                putSeparate = putRGBUAseparate8bittile;
                            else
                                putSeparate = putRGBseparate8bittile;
                            break;

                        case 16:
                            if (alpha == ExtraSample.ASSOCALPHA)
                                putSeparate = putRGBAAseparate16bittile;
                            else if (alpha == ExtraSample.UNASSALPHA)
                                putSeparate = putRGBUAseparate16bittile;
                            else
                                putSeparate = putRGBseparate16bittile;
                            break;
                    }
                    break;

                case Photometric.YCBCR:
                    if ((bitspersample == 8) && (samplesperpixel == 3))
                    {
                        if (initYCbCrConversion())
                        {
                            FieldValue[] result = tif.GetFieldDefaulted(TiffTag.YCBCRSUBSAMPLING);
                            short hs = result[0].ToShort();
                            short vs = result[0].ToShort();

                            switch (((ushort)hs << 4) | (ushort)vs)
                            {
                                case 0x11:
                                    putSeparate = putseparate8bitYCbCr11tile;
                                    break;
                                // TODO: add other cases here
                            }
                        }
                    }
                    break;
            }

            return (putSeparate != null);
        }

        private bool initYCbCrConversion()
        {
            if (ycbcr == null)
                ycbcr = new TiffYCbCrToRGB();

            FieldValue[] result = tif.GetFieldDefaulted(TiffTag.YCBCRCOEFFICIENTS);
            float[] luma = result[0].ToFloatArray();

            result = tif.GetFieldDefaulted(TiffTag.REFERENCEBLACKWHITE);
            float[] refBlackWhite = result[0].ToFloatArray();

            ycbcr.Init(luma, refBlackWhite);
            return true;
        }

        private PutContigDelegate initCIELabConversion()
        {
            if (cielab == null)
                cielab = new TiffCIELabToRGB();

            FieldValue[] result = tif.GetFieldDefaulted(TiffTag.WHITEPOINT);
            float[] whitePoint = result[0].ToFloatArray();

            float[] refWhite = new float[3];
            refWhite[1] = 100.0F;
            refWhite[0] = whitePoint[0] / whitePoint[1] * refWhite[1];
            refWhite[2] = (1.0F - whitePoint[0] - whitePoint[1]) / whitePoint[1] * refWhite[1];
            cielab.Init(display_sRGB, refWhite);

            return putcontig8bitCIELab;
        }

        /// <summary>
        /// Construct any mapping table used by the associated put method.
        /// </summary>
        private bool buildMap()
        {
            switch (photometric)
            {
                case Photometric.RGB:
                case Photometric.YCBCR:
                case Photometric.SEPARATED:
                    if (bitspersample == 8)
                        break;
                    if (!setupMap())
                        return false;
                    break;

                case Photometric.MINISBLACK:
                case Photometric.MINISWHITE:
                    if (!setupMap())
                        return false;
                    break;

                case Photometric.PALETTE:
                    // Convert 16-bit colormap to 8-bit
                    // (unless it looks like an old-style 8-bit colormap).
                    if (checkcmap() == 16)
                        cvtcmap();
                    else
                        Tiff.WarningExt(tif, tif.m_clientdata, tif.FileName(), "Assuming 8-bit colormap");

                    // Use mapping table and colormap to construct unpacking
                    // tables for samples < 8 bits.
                    if (bitspersample <= 8 && !makecmap())
                        return false;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Construct a mapping table to convert from the range of the data samples to [0, 255] -
        /// for display. This process also handles inverting B&amp;W images when needed.
        /// </summary>
        private bool setupMap()
        {
            int range = (1 << bitspersample) - 1;

            // treat 16 bit the same as eight bit
            if (bitspersample == 16)
                range = 255;

            Map = new byte[range + 1];

            if (photometric == Photometric.MINISWHITE)
            {
                for (int x = 0; x <= range; x++)
                    Map[x] = (byte)(((range - x) * 255) / range);
            }
            else
            {
                for (int x = 0; x <= range; x++)
                    Map[x] = (byte)((x * 255) / range);
            }

            if (bitspersample <= 16 && (photometric == Photometric.MINISBLACK || photometric == Photometric.MINISWHITE))
            {
                // Use photometric mapping table to construct unpacking tables for samples <= 8 bits.
                if (!makebwmap())
                    return false;

                // no longer need Map
                Map = null;
            }

            return true;
        }

        private int checkcmap()
        {
            int r = 0;
            int g = 0;
            int b = 0;
            int n = 1 << bitspersample;
            while (n-- > 0)
            {
                if (redcmap[r] >= 256 || greencmap[g] >= 256 || bluecmap[b] >= 256)
                    return 16;

                r++;
                g++;
                b++;
            }

            return 8;
        }

        private void cvtcmap()
        {
            for (int i = (1 << bitspersample) - 1; i >= 0; i--)
            {
                redcmap[i] = (short)(redcmap[i] >> 8);
                greencmap[i] = (short)(greencmap[i] >> 8);
                bluecmap[i] = (short)(bluecmap[i] >> 8);
            }
        }

        private bool makecmap()
        {
            int nsamples = 8 / bitspersample;

            PALmap = new int[256][];
            for (int i = 0; i < 256; i++)
                PALmap[i] = new int[nsamples];

            for (int i = 0; i < 256; i++)
            {
                int j = 0;
                switch (bitspersample)
                {
                    case 1:
                        CMAP(i >> 7, i, ref j);
                        CMAP((i >> 6) & 1, i, ref j);
                        CMAP((i >> 5) & 1, i, ref j);
                        CMAP((i >> 4) & 1, i, ref j);
                        CMAP((i >> 3) & 1, i, ref j);
                        CMAP((i >> 2) & 1, i, ref j);
                        CMAP((i >> 1) & 1, i, ref j);
                        CMAP(i & 1, i, ref j);
                        break;
                    case 2:
                        CMAP(i >> 6, i, ref j);
                        CMAP((i >> 4) & 3, i, ref j);
                        CMAP((i >> 2) & 3, i, ref j);
                        CMAP(i & 3, i, ref j);
                        break;
                    case 4:
                        CMAP(i >> 4, i, ref j);
                        CMAP(i & 0xf, i, ref j);
                        break;
                    case 8:
                        CMAP(i, i, ref j);
                        break;
                }
            }

            return true;
        }

        private bool makebwmap()
        {
            int nsamples = 8 / bitspersample;
            if (nsamples == 0)
                nsamples = 1;

            BWmap = new int[256][];
            for (int i = 0; i < 256; i++)
                BWmap[i] = new int[nsamples];

            for (int i = 0; i < 256; i++)
            {
                int j = 0;
                switch (bitspersample)
                {
                    case 1:
                        GREY(i >> 7, i, ref j);
                        GREY((i >> 6) & 1, i, ref j);
                        GREY((i >> 5) & 1, i, ref j);
                        GREY((i >> 4) & 1, i, ref j);
                        GREY((i >> 3) & 1, i, ref j);
                        GREY((i >> 2) & 1, i, ref j);
                        GREY((i >> 1) & 1, i, ref j);
                        GREY(i & 1, i, ref j);
                        break;
                    case 2:
                        GREY(i >> 6, i, ref j);
                        GREY((i >> 4) & 3, i, ref j);
                        GREY((i >> 2) & 3, i, ref j);
                        GREY(i & 3, i, ref j);
                        break;
                    case 4:
                        GREY(i >> 4, i, ref j);
                        GREY(i & 0xf, i, ref j);
                        break;
                    case 8:
                    case 16:
                        GREY(i, i, ref j);
                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// YCbCr -> RGB conversion and packing routines.
        /// </summary>
        private void YCbCrtoRGB(out int dst, int Y, int Cb, int Cr)
        {
            int r, g, b;
            ycbcr.YCbCrtoRGB(Y, Cb, Cr, out r, out g, out b);
            dst = PACK(r, g, b);
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        // The following routines move decoded data returned from the TIFF library into rasters
        // filled with packed ABGR pixels
        //
        // The routines have been created according to the most important cases and optimized.
        // pickTileContigCase and pickTileSeparateCase analyze the parameters and select the
        // appropriate "put" routine to use.


        ///////////////////////////////////////////////////////////////////////////////////////////
        // Contiguous cases
        //

        /// <summary>
        /// 8-bit palette => colormap/RGB
        /// </summary>
        private static void put8bitcmaptile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] PALmap = img.PALmap;
            int samplesperpixel = img.samplesperpixel;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    raster[rasterOffset] = PALmap[buffer[offset]][0];
                    rasterOffset++;
                    offset += samplesperpixel;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 4-bit palette => colormap/RGB
        /// </summary>
        private static void put4bitcmaptile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] PALmap = img.PALmap;
            bufferShift /= 2;
            
            while (height-- > 0)
            {
                int[] bw = null;

                int _x;
                for (_x = width; _x >= 2; _x -= 2)
                {
                    bw = PALmap[buffer[offset]];
                    offset++;
                    for (int rc = 0; rc < 2; rc++)
                    {
                        raster[rasterOffset] = bw[rc];
                        rasterOffset++;
                    }
                }

                if (_x != 0)
                {
                    bw = PALmap[buffer[offset]];
                    offset++;

                    raster[rasterOffset] = bw[0];
                    rasterOffset++;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 2-bit palette => colormap/RGB
        /// </summary>
        private static void put2bitcmaptile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] PALmap = img.PALmap;
            bufferShift /= 4;
            
            while (height-- > 0)
            {
                int[] bw = null;

                int _x;
                for (_x = width; _x >= 4; _x -= 4)
                {
                    bw = PALmap[buffer[offset]];
                    offset++;
                    for (int rc = 0; rc < 4; rc++)
                    {
                        raster[rasterOffset] = bw[rc];
                        rasterOffset++;
                    }
                }

                if (_x > 0)
                {
                    bw = PALmap[buffer[offset]];
                    offset++;

                    if (_x <= 3 && _x > 0)
                    {
                        for (int i = 0; i < _x; i++)
                        {
                            raster[rasterOffset] = bw[i];
                            rasterOffset++;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 1-bit palette => colormap/RGB
        /// </summary>
        private static void put1bitcmaptile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] PALmap = img.PALmap;
            bufferShift /= 8;

            while (height-- > 0)
            {
                int[] bw;
                int bwPos = 0;

                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    bw = PALmap[buffer[offset++]];
                    bwPos = 0;

                    for (int i = 0; i < 8; i++)
                        raster[rasterOffset++] = bw[bwPos++];
                }

                if (_x > 0)
                {
                    bw = PALmap[buffer[offset++]];
                    bwPos = 0;

                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = 0; i < _x; i++)
                            raster[rasterOffset++] = bw[bwPos++];
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit greyscale => colormap/RGB
        /// </summary>
        private static void putgreytile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            int[][] BWmap = img.BWmap;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    raster[rasterOffset] = BWmap[buffer[offset]][0];
                    rasterOffset++;
                    offset += samplesperpixel;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 16-bit greyscale => colormap/RGB
        /// </summary>
        private static void put16bitbwtile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            int[][] BWmap = img.BWmap;

            while (height-- > 0)
            {
                short[] wp = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length - offset);
                int wpPos = 0;

                for (x = width; x-- > 0; )
                {
                    // use high order byte of 16bit value
                    raster[rasterOffset] = BWmap[(wp[wpPos] & 0xffff) >> 8][0];
                    rasterOffset++;
                    offset += 2 * samplesperpixel;
                    wpPos += samplesperpixel;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 1-bit bilevel => colormap/RGB
        /// </summary>
        private static void put1bitbwtile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] BWmap = img.BWmap;
            bufferShift /= 8;

            while (height-- > 0)
            {
                int[] bw = null;

                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;

                    for (int rc = 0; rc < 8; rc++)
                    {
                        raster[rasterOffset] = bw[rc];
                        rasterOffset++;
                    }
                }

                if (_x > 0)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;

                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = 0; i < _x; i++)
                        {
                            raster[rasterOffset] = bw[i];
                            rasterOffset++;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 2-bit greyscale => colormap/RGB
        /// </summary>
        private static void put2bitbwtile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] BWmap = img.BWmap;
            bufferShift /= 4;

            while (height-- > 0)
            {
                int[] bw = null;

                int _x;
                for (_x = width; _x >= 4; _x -= 4)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;
                    for (int rc = 0; rc < 4; rc++)
                    {
                        raster[rasterOffset] = bw[rc];
                        rasterOffset++;
                    }
                }

                if (_x > 0)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;

                    if (_x <= 3 && _x > 0)
                    {
                        for (int i = 0; i < _x; i++)
                        {
                            raster[rasterOffset] = bw[i];
                            rasterOffset++;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 4-bit greyscale => colormap/RGB
        /// </summary>
        private static void put4bitbwtile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int[][] BWmap = img.BWmap;
            bufferShift /= 2;
            
            while (height-- > 0)
            {
                int[] bw = null;

                int _x;
                for (_x = width; _x >= 2; _x -= 2)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;
                    for (int rc = 0; rc < 2; rc++)
                    {
                        raster[rasterOffset] = bw[rc];
                        rasterOffset++;
                    }
                }

                if (_x != 0)
                {
                    bw = BWmap[buffer[offset]];
                    offset++;

                    raster[rasterOffset] = bw[0];
                    rasterOffset++;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed samples, no Map => RGB
        /// </summary>
        private static void putRGBcontig8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;
            
            while (height-- > 0)
            {
                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    for (int rc = 0; rc < 8; rc++)
                    {
                        raster[rasterOffset] = PACK(buffer[offset], buffer[offset + 1], buffer[offset + 2]);
                        rasterOffset++;
                        offset += samplesperpixel;
                    }
                }

                if (_x > 0)
                {
                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = _x; i > 0; i--)
                        {
                            raster[rasterOffset] = PACK(buffer[offset], buffer[offset + 1], buffer[offset + 2]);
                            rasterOffset++;
                            offset += samplesperpixel;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed samples => RGBA w/ associated alpha (known to have Map == null)
        /// </summary>
        private static void putRGBAAcontig8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;

            while (height-- > 0)
            {
                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    for (int rc = 0; rc < 8; rc++)
                    {
                        raster[rasterOffset] = PACK4(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
                        rasterOffset++;
                        offset += samplesperpixel;
                    }
                }

                if (_x > 0)
                {
                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = _x; i > 0; i--)
                        {
                            raster[rasterOffset] = PACK4(buffer[offset], buffer[offset + 1], buffer[offset + 2], buffer[offset + 3]);
                            rasterOffset++;
                            offset += samplesperpixel;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed samples => RGBA w/ unassociated alpha (known to have Map == null)
        /// </summary>
        private static void putRGBUAcontig8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;
            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    int a = buffer[offset + 3];
                    int r = (buffer[offset] * a + 127) / 255;
                    int g = (buffer[offset + 1] * a + 127) / 255;
                    int b = (buffer[offset + 2] * a + 127) / 255;
                    raster[rasterOffset] = PACK4(r, g, b, a);
                    rasterOffset++;
                    offset += samplesperpixel;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 16-bit packed samples => RGB
        /// </summary>
        private static void putRGBcontig16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;

            short[] wp = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
            int wpPos = 0;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    raster[rasterOffset] = PACKW(wp[wpPos], wp[wpPos + 1], wp[wpPos + 2]);
                    rasterOffset++;
                    wpPos += samplesperpixel;
                }

                rasterOffset += rasterShift;
                wpPos += bufferShift;
            }
        }

        /// <summary>
        /// 16-bit packed samples => RGBA w/ associated alpha (known to have Map == null)
        /// </summary>
        private static void putRGBAAcontig16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;

            short[] wp = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
            int wpPos = 0;

            bufferShift *= samplesperpixel;
            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    raster[rasterOffset] = PACKW4(wp[wpPos], wp[wpPos + 1], wp[wpPos + 2], wp[wpPos + 3]);
                    rasterOffset++;
                    wpPos += samplesperpixel;
                }

                rasterOffset += rasterShift;
                wpPos += bufferShift;
            }
        }

        /// <summary>
        /// 16-bit packed samples => RGBA w/ unassociated alpha (known to have Map == null)
        /// </summary>
        private static void putRGBUAcontig16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;

            short[] wp = Tiff.ByteArrayToShorts(buffer, offset, buffer.Length);
            int wpPos = 0;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    int a = W2B(wp[wpPos + 3]);
                    int r = (W2B(wp[wpPos]) * a + 127) / 255;
                    int g = (W2B(wp[wpPos + 1]) * a + 127) / 255;
                    int b = (W2B(wp[wpPos + 2]) * a + 127) / 255;
                    raster[rasterOffset] = PACK4(r, g, b, a);
                    rasterOffset++;
                    wpPos += samplesperpixel;
                }

                rasterOffset += rasterShift;
                wpPos += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed CMYK samples w/o Map => RGB.
        /// NB: The conversion of CMYK->RGB is *very* crude.
        /// </summary>
        private static void putRGBcontig8bitCMYKtile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            bufferShift *= samplesperpixel;

            while (height-- > 0)
            {
                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    for (int rc = 0; rc < 8; rc++)
                    {
                        short k = (short)(255 - buffer[offset + 3]);
                        short r = (short)((k * (255 - buffer[offset])) / 255);
                        short g = (short)((k * (255 - buffer[offset + 1])) / 255);
                        short b = (short)((k * (255 - buffer[offset + 2])) / 255);
                        raster[rasterOffset] = PACK(r, g, b);
                        rasterOffset++;
                        offset += samplesperpixel;
                    }
                }

                if (_x > 0)
                {
                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = _x; i > 0; i--)
                        {
                            short k = (short)(255 - buffer[offset + 3]);
                            short r = (short)((k * (255 - buffer[offset])) / 255);
                            short g = (short)((k * (255 - buffer[offset + 1])) / 255);
                            short b = (short)((k * (255 - buffer[offset + 2])) / 255);
                            raster[rasterOffset] = PACK(r, g, b);
                            rasterOffset++;
                            offset += samplesperpixel;
                        }
                    }
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed CIE L*a*b 1976 samples => RGB
        /// </summary>
        private static void putcontig8bitCIELab(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            bufferShift *= 3;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    float X, Y, Z;
                    img.cielab.CIELabToXYZ(buffer[offset], (sbyte)buffer[offset + 1], (sbyte)buffer[offset + 2], out X, out Y, out Z);

                    int r, g, b;
                    img.cielab.XYZToRGB(X, Y, Z, out r, out g, out b);

                    raster[rasterOffset] = PACK(r, g, b);
                    rasterOffset++;
                    offset += 3;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 2,2 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr22tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            bufferShift = (bufferShift / 2) * 6;
            int rasterOffset2 = rasterOffset + width + rasterShift;

            while (height >= 2)
            {
                x = width;
                while (x >= 2)
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 2], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset2 + 1], buffer[offset + 3], Cb, Cr);
                    rasterOffset += 2;
                    rasterOffset2 += 2;
                    offset += 6;
                    x -= 2;
                }

                if (x == 1)
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 2], Cb, Cr);
                    rasterOffset++;
                    rasterOffset2++;
                    offset += 6;
                }

                rasterOffset += rasterShift * 2 + width;
                rasterOffset2 += rasterShift * 2 + width;
                offset += bufferShift;
                height -= 2;
            }

            if (height == 1)
            {
                x = width;
                while (x >= 2)
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                    rasterOffset += 2;
                    rasterOffset2 += 2;
                    offset += 6;
                    x -= 2;
                }

                if (x == 1)
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                }
            }
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 2,1 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr21tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            bufferShift = (bufferShift * 4) / 2;

            do
            {
                x = width >> 1;
                do
                {
                    int Cb = buffer[offset + 2];
                    int Cr = buffer[offset + 3];

                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);

                    rasterOffset += 2;
                    offset += 4;
                }
                while (--x != 0);

                if ((width & 1) != 0)
                {
                    int Cb = buffer[offset + 2];
                    int Cr = buffer[offset + 3];

                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);

                    rasterOffset += 1;
                    offset += 4;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
            while (--height != 0);
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 4,4 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr44tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int rasterOffset1 = rasterOffset + width + rasterShift;
            int rasterOffset2 = rasterOffset1 + width + rasterShift;
            int rasterOffset3 = rasterOffset2 + width + rasterShift;
            int incr = 3 * width + 4 * rasterShift;

            // adjust bufferShift
            bufferShift = (bufferShift * 18) / 4;
            if ((height & 3) == 0 && (width & 3) == 0)
            {
                for (; height >= 4; height -= 4)
                {
                    x = width >> 2;
                    do
                    {
                        int Cb = buffer[offset + 16];
                        int Cr = buffer[offset + 17];

                        img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset + 0], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset1 + 0], buffer[offset + 4], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset1 + 1], buffer[offset + 5], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset1 + 2], buffer[offset + 6], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset1 + 3], buffer[offset + 7], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 8], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 1], buffer[offset + 9], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 2], buffer[offset + 10], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 3], buffer[offset + 11], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset3 + 0], buffer[offset + 12], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset3 + 1], buffer[offset + 13], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset3 + 2], buffer[offset + 14], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset3 + 3], buffer[offset + 15], Cb, Cr);

                        rasterOffset += 4;
                        rasterOffset1 += 4;
                        rasterOffset2 += 4;
                        rasterOffset3 += 4;
                        offset += 18;
                    }
                    while (--x != 0);

                    rasterOffset += incr;
                    rasterOffset1 += incr;
                    rasterOffset2 += incr;
                    rasterOffset3 += incr;
                    offset += bufferShift;
                }
            }
            else
            {
                while (height > 0)
                {
                    for (x = width; x > 0; )
                    {
                        int Cb = buffer[offset + 16];
                        int Cr = buffer[offset + 17];

                        bool h_goOn = false;
                        bool x_goOn = false;

                        // order of if's is important
                        if (x < 1 || x > 3)
                        {
                            // order of if's is important
                            h_goOn = false;
                            if (height < 1 || height > 3)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset3 + 3], buffer[offset + 15], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 3 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 3], buffer[offset + 11], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 2 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset1 + 3], buffer[offset + 7], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 1 || h_goOn)
                                img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], Cb, Cr);

                            x_goOn = true;
                        }

                        if (x == 3 || x_goOn)
                        {
                            // order of if's is important
                            h_goOn = false;
                            if (height < 1 || height > 3)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset3 + 2], buffer[offset + 14], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 3 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 2], buffer[offset + 10], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 2 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset1 + 2], buffer[offset + 6], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 1 || h_goOn)
                                img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);

                            x_goOn = true;
                        }

                        if (x == 2 || x_goOn)
                        {
                            // order of if's is important
                            h_goOn = false;
                            if (height < 1 || height > 3)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset3 + 1], buffer[offset + 13], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 3 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 1], buffer[offset + 9], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 2 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset1 + 1], buffer[offset + 5], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 1 || h_goOn)
                                img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                        }

                        if (x == 1 || x_goOn)
                        {
                            // order of if's is important
                            h_goOn = false;
                            if (height < 1 || height > 3)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset3 + 0], buffer[offset + 12], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 3 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 8], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 2 || h_goOn)
                            {
                                img.YCbCrtoRGB(out raster[rasterOffset1 + 0], buffer[offset + 4], Cb, Cr);
                                h_goOn = true;
                            }

                            if (height == 1 || h_goOn)
                                img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                        }

                        if (x < 4)
                        {
                            rasterOffset += x;
                            rasterOffset1 += x;
                            rasterOffset2 += x;
                            rasterOffset3 += x;
                            x = 0;
                        }
                        else
                        {
                            rasterOffset += 4;
                            rasterOffset1 += 4;
                            rasterOffset2 += 4;
                            rasterOffset3 += 4;
                            x -= 4;
                        }

                        offset += 18;
                    }

                    if (height <= 4)
                        break;

                    height -= 4;
                    rasterOffset += incr;
                    rasterOffset1 += incr;
                    rasterOffset2 += incr;
                    rasterOffset3 += incr;
                    offset += bufferShift;
                }
            }
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 4,2 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr42tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int rasterOffset2 = rasterOffset + width + rasterShift;
            int incr = 2 * rasterShift + width;

            bufferShift = (bufferShift * 10) / 4;
            if ((height & 3) == 0 && (width & 1) == 0)
            {
                for (; height >= 2; height -= 2)
                {
                    x = width >> 2;
                    do
                    {
                        int Cb = buffer[offset + 8];
                        int Cr = buffer[offset + 9];

                        img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 4], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 1], buffer[offset + 5], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 2], buffer[offset + 6], Cb, Cr);
                        img.YCbCrtoRGB(out raster[rasterOffset2 + 3], buffer[offset + 7], Cb, Cr);

                        rasterOffset += 4;
                        rasterOffset2 += 4;
                        offset += 10;
                    }
                    while (--x != 0);

                    rasterOffset += incr;
                    rasterOffset2 += incr;
                    offset += bufferShift;
                }
            }
            else
            {
                while (height > 0)
                {
                    for (x = width; x > 0; )
                    {
                        int Cb = buffer[offset + 8];
                        int Cr = buffer[offset + 9];

                        bool x_goOn = false;
                        if (x < 1 || x > 3)
                        {
                            if (height != 1)
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 3], buffer[offset + 7], Cb, Cr);

                            img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], Cb, Cr);
                            x_goOn = true;
                        }

                        if (x == 3 || x_goOn)
                        {
                            if (height != 1)
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 2], buffer[offset + 6], Cb, Cr);

                            img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);
                            x_goOn = true;
                        }

                        if (x == 2 || x_goOn)
                        {
                            if (height != 1)
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 1], buffer[offset + 5], Cb, Cr);

                            img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                            x_goOn = true;
                        }

                        if (x == 1 || x_goOn)
                        {
                            if (height != 1)
                                img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 4], Cb, Cr);

                            img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                        }

                        if (x < 4)
                        {
                            rasterOffset += x;
                            rasterOffset2 += x;
                            x = 0;
                        }
                        else
                        {
                            rasterOffset += 4;
                            rasterOffset2 += 4;
                            x -= 4;
                        }

                        offset += 10;
                    }

                    if (height <= 2)
                        break;

                    height -= 2;
                    rasterOffset += incr;
                    rasterOffset2 += incr;
                    offset += bufferShift;
                }
            }
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 4,1 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr41tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            // XXX adjust bufferShift
            do
            {
                x = width >> 2;
                do
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];

                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset + 3], buffer[offset + 3], Cb, Cr);

                    rasterOffset += 4;
                    offset += 6;
                }
                while (--x != 0);

                if ((width & 3) != 0)
                {
                    int Cb = buffer[offset + 4];
                    int Cr = buffer[offset + 5];

                    int xx = width & 3;
                    if (xx == 3)
                        img.YCbCrtoRGB(out raster[rasterOffset + 2], buffer[offset + 2], Cb, Cr);

                    if (xx == 3 || xx == 2)
                        img.YCbCrtoRGB(out raster[rasterOffset + 1], buffer[offset + 1], Cb, Cr);

                    if (xx == 3 || xx == 2 || xx == 1)
                        img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);

                    rasterOffset += xx;
                    offset += 6;
                }

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
            while (--height != 0);
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ no subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr11tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            bufferShift *= 3;
            do
            {
                x = width; // was x = w >> 1; patched 2000/09/25 warmerda@home.com
                do
                {
                    int Cb = buffer[offset + 1];
                    int Cr = buffer[offset + 2];

                    img.YCbCrtoRGB(out raster[rasterOffset], buffer[offset + 0], Cb, Cr);
                    rasterOffset++;
                    offset += 3;
                }
                while (--x != 0);

                rasterOffset += rasterShift;
                offset += bufferShift;
            }
            while (--height != 0);
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ 1,2 subsampling => RGB
        /// </summary>
        private static void putcontig8bitYCbCr12tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            bufferShift = (bufferShift / 2) * 4;
            int rasterOffset2 = rasterOffset + width + rasterShift;

            while (height >= 2)
            {
                x = width;
                do
                {
                    int Cb = buffer[offset + 2];
                    int Cr = buffer[offset + 3];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    img.YCbCrtoRGB(out raster[rasterOffset2 + 0], buffer[offset + 1], Cb, Cr);
                    rasterOffset++;
                    rasterOffset2++;
                    offset += 4;
                } while (--x != 0);

                rasterOffset += rasterShift * 2 + width;
                rasterOffset2 += rasterShift * 2 + width;
                offset += bufferShift;
                height -= 2;
            }

            if (height == 1)
            {
                x = width;
                do
                {
                    int Cb = buffer[offset + 2];
                    int Cr = buffer[offset + 3];
                    img.YCbCrtoRGB(out raster[rasterOffset + 0], buffer[offset + 0], Cb, Cr);
                    rasterOffset++;
                    offset += 4;
                } while (--x != 0);
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////
        // Separated cases
        //

        /// <summary>
        /// 8-bit unpacked samples => RGB
        /// </summary>
        private static void putRGBseparate8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            while (height-- > 0)
            {
                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    for (int rc = 0; rc < 8; rc++)
                    {
                        raster[rasterOffset] = PACK(buffer[offset1], buffer[offset2], buffer[offset3]);
                        rasterOffset++;
                        offset1++;
                        offset2++;
                        offset3++;
                    }
                }

                if (_x > 0)
                {
                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = _x; i > 0; i--)
                        {
                            raster[rasterOffset] = PACK(buffer[offset1], buffer[offset2], buffer[offset3]);
                            rasterOffset++;
                            offset1++;
                            offset2++;
                            offset3++;
                        }
                    }
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 8-bit unpacked samples => RGBA w/ associated alpha
        /// </summary>
        private static void putRGBAAseparate8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            while (height-- > 0)
            {
                int _x;
                for (_x = width; _x >= 8; _x -= 8)
                {
                    for (int rc = 0; rc < 8; rc++)
                    {
                        raster[rasterOffset] = PACK4(buffer[offset1], buffer[offset2], buffer[offset3], buffer[offset4]);
                        rasterOffset++;
                        offset1++;
                        offset2++;
                        offset3++;
                        offset4++;
                    }
                }

                if (_x > 0)
                {
                    if (_x <= 7 && _x > 0)
                    {
                        for (int i = _x; i > 0; i--)
                        {
                            raster[rasterOffset] = PACK4(buffer[offset1], buffer[offset2], buffer[offset3], buffer[offset4]);
                            rasterOffset++;
                            offset1++;
                            offset2++;
                            offset3++;
                            offset4++;
                        }
                    }
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                offset4 += bufferShift;

                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 8-bit unpacked samples => RGBA w/ unassociated alpha
        /// </summary>
        private static void putRGBUAseparate8bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    int av = buffer[offset4];
                    int rv = (buffer[offset1] * av + 127) / 255;
                    int gv = (buffer[offset2] * av + 127) / 255;
                    int bv = (buffer[offset3] * av + 127) / 255;
                    raster[rasterOffset] = PACK4(rv, gv, bv, av);
                    rasterOffset++;
                    offset1++;
                    offset2++;
                    offset3++;
                    offset4++;
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                offset4 += bufferShift;

                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 16-bit unpacked samples => RGB
        /// </summary>
        private static void putRGBseparate16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            short[] wrgba = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);

            offset1 /= sizeof(short);
            offset2 /= sizeof(short);
            offset3 /= sizeof(short);

            while (height-- > 0)
            {
                for (x = 0; x < width; x++)
                {
                    raster[rasterOffset] = PACKW(wrgba[offset1], wrgba[offset2], wrgba[offset3]);
                    rasterOffset++;
                    offset1++;
                    offset2++;
                    offset3++;
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 16-bit unpacked samples => RGBA w/ associated alpha
        /// </summary>
        private static void putRGBAAseparate16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            short[] wrgba = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);

            offset1 /= sizeof(short);
            offset2 /= sizeof(short);
            offset3 /= sizeof(short);
            offset4 /= sizeof(short);

            while (height-- > 0)
            {
                for (x = 0; x < width; x++)
                {
                    raster[rasterOffset] = PACKW4(wrgba[offset1], wrgba[offset2], wrgba[offset3], wrgba[offset4]);
                    rasterOffset++;
                    offset1++;
                    offset2++;
                    offset3++;
                    offset4++;
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                offset4 += bufferShift;

                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 16-bit unpacked samples => RGBA w/ unassociated alpha
        /// </summary>
        private static void putRGBUAseparate16bittile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            short[] wrgba = Tiff.ByteArrayToShorts(buffer, 0, buffer.Length);

            offset1 /= sizeof(short);
            offset2 /= sizeof(short);
            offset3 /= sizeof(short);
            offset4 /= sizeof(short);

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    int a = W2B(wrgba[offset4]);
                    int r = (W2B(wrgba[offset1]) * a + 127) / 255;
                    int g = (W2B(wrgba[offset2]) * a + 127) / 255;
                    int b = (W2B(wrgba[offset3]) * a + 127) / 255;
                    raster[rasterOffset] = PACK4(r, g, b, a);
                    rasterOffset++;
                    offset1++;
                    offset2++;
                    offset3++;
                    offset4++;
                }

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                offset4 += bufferShift;

                rasterOffset += rasterShift;
            }
        }

        /// <summary>
        /// 8-bit packed YCbCr samples w/ no subsampling => RGB
        /// </summary>
        private static void putseparate8bitYCbCr11tile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height,
            byte[] buffer, int offset1, int offset2, int offset3, int offset4, int bufferShift)
        {
            while (height-- > 0)
            {
                x = width;
                do
                {
                    int r, g, b;
                    img.ycbcr.YCbCrtoRGB(buffer[offset1], buffer[offset2], buffer[offset3], out r, out g, out b);

                    raster[rasterOffset] = PACK(r, g, b);
                    rasterOffset++;
                    offset1++;
                    offset2++;
                    offset3++;
                } while (--x != 0);

                offset1 += bufferShift;
                offset2 += bufferShift;
                offset3 += bufferShift;
                rasterOffset += rasterShift;
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Untested methods
        //
        // This methods are untested (and, probably, should be deleted).
        //
        // pickContigCase implicitly *excludes* putRGBcontig8bitCMYKMaptile from possible "put"
        //                methods when it requires images to have Photometric.SEPARATED *and* 8-bit
        //                samples *and* a Map to use putRGBcontig8bitCMYKMaptile. The problem is:
        //                no Map is ever built for Photometric.SEPARATED *and* 8-bit samples.

        /// <summary>
        /// 8-bit packed CMYK samples w/Map => RGB
        /// NB: The conversion of CMYK->RGB is *very* crude.
        /// </summary>
        private static void putRGBcontig8bitCMYKMaptile(
            TiffRgbaImage img, int[] raster, int rasterOffset, int rasterShift,
            int x, int y, int width, int height, byte[] buffer, int offset, int bufferShift)
        {
            int samplesperpixel = img.samplesperpixel;
            byte[] Map = img.Map;
            bufferShift *= samplesperpixel;

            while (height-- > 0)
            {
                for (x = width; x-- > 0; )
                {
                    short k = (short)(255 - buffer[offset + 3]);
                    short r = (short)((k * (255 - buffer[offset])) / 255);
                    short g = (short)((k * (255 - buffer[offset + 1])) / 255);
                    short b = (short)((k * (255 - buffer[offset + 2])) / 255);
                    raster[rasterOffset] = PACK(Map[r], Map[g], Map[b]);
                    rasterOffset++;
                    offset += samplesperpixel;
                }

                offset += bufferShift;
                rasterOffset += rasterShift;
            }
        }
    }
}
