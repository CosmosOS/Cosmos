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

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// TIFF tag definitions.
    /// </summary>
    /// <remarks>
    /// Joris Van Damme maintains
    /// <a href="http://www.awaresystems.be/imaging/tiff/tifftags.html" target="_blank">
    /// TIFF Tag Reference</a>, good source of tag information. It's an overview of known TIFF
    /// Tags with properties, short description, and other useful information.
    /// </remarks>
#if EXPOSE_LIBTIFF
    public
#endif
    enum TiffTag
    {
        /// <summary>
        /// Tag placeholder
        /// </summary>
        IGNORE = 0,
        
        /// <summary>
        /// Subfile data descriptor.
        /// For the list of possible values, see <see cref="FileType"/>.
        /// </summary>
        SUBFILETYPE = 254,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Kind of data in subfile. For the list of possible values, see <see cref="OFileType"/>.
        /// </summary>
        OSUBFILETYPE = 255,
        
        /// <summary>
        /// Image width in pixels.
        /// </summary>
        IMAGEWIDTH = 256,
        
        /// <summary>
        /// Image height in pixels.
        /// </summary>
        IMAGELENGTH = 257,
        
        /// <summary>
        /// Bits per channel (sample).
        /// </summary>
        BITSPERSAMPLE = 258,
        
        /// <summary>
        /// Data compression technique.
        /// For the list of possible values, see <see cref="Compression"/>.
        /// </summary>
        COMPRESSION = 259,
        
        /// <summary>
        /// Photometric interpretation.
        /// For the list of possible values, see <see cref="Photometric"/>.
        /// </summary>
        PHOTOMETRIC = 262,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Thresholding used on data. For the list of possible values, see <see cref="Threshold"/>.
        /// </summary>
        THRESHHOLDING = 263,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Dithering matrix width.
        /// </summary>
        CELLWIDTH = 264,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Dithering matrix height.
        /// </summary>
        CELLLENGTH = 265,
        
        /// <summary>
        /// Data order within a byte.
        /// For the list of possible values, see <see cref="FillOrder"/>.
        /// </summary>
        FILLORDER = 266,
        
        /// <summary>
        /// Name of document which holds for image.
        /// </summary>
        DOCUMENTNAME = 269,
        
        /// <summary>
        /// Information about image.
        /// </summary>
        IMAGEDESCRIPTION = 270,
        
        /// <summary>
        /// Scanner manufacturer name.
        /// </summary>
        MAKE = 271,
        
        /// <summary>
        /// Scanner model name/number.
        /// </summary>
        MODEL = 272,
        
        /// <summary>
        /// Offsets to data strips.
        /// </summary>
        STRIPOFFSETS = 273,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Image orientation. For the list of possible values, see <see cref="Orientation"/>.
        /// </summary>
        ORIENTATION = 274,
        
        /// <summary>
        /// Samples per pixel.
        /// </summary>
        SAMPLESPERPIXEL = 277,
        
        /// <summary>
        /// Rows per strip of data.
        /// </summary>
        ROWSPERSTRIP = 278,
        
        /// <summary>
        /// Bytes counts for strips.
        /// </summary>
        STRIPBYTECOUNTS = 279,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Minimum sample value.
        /// </summary>
        MINSAMPLEVALUE = 280,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Maximum sample value.
        /// </summary>
        MAXSAMPLEVALUE = 281,
        
        /// <summary>
        /// Pixels/resolution in x.
        /// </summary>
        XRESOLUTION = 282,
        
        /// <summary>
        /// Pixels/resolution in y.
        /// </summary>
        YRESOLUTION = 283,
        
        /// <summary>
        /// Storage organization.
        /// For the list of possible values, see <see cref="PlanarConfig"/>.
        /// </summary>
        PLANARCONFIG = 284,
        
        /// <summary>
        /// Page name image is from.
        /// </summary>
        PAGENAME = 285,
        
        /// <summary>
        /// X page offset of image lhs.
        /// </summary>
        XPOSITION = 286,
        
        /// <summary>
        /// Y page offset of image lhs.
        /// </summary>
        YPOSITION = 287,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Byte offset to free block.
        /// </summary>
        FREEOFFSETS = 288,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 5.0]<br/>
        /// Sizes of free blocks.
        /// </summary>
        FREEBYTECOUNTS = 289,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 6.0]<br/>
        /// Gray scale curve accuracy.
        /// For the list of possible values, see <see cref="GrayResponseUnit"/>.
        /// </summary>
        GRAYRESPONSEUNIT = 290,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 6.0]<br/>
        /// Gray scale response curve.
        /// </summary>
        GRAYRESPONSECURVE = 291,
        
        /// <summary>
        /// Options for CCITT Group 3 fax encoding. 32 flag bits.
        /// For the list of possible values, see <see cref="Group3Opt"/>.
        /// </summary>
        GROUP3OPTIONS = 292,
        
        /// <summary>
        /// TIFF 6.0 proper name alias for GROUP3OPTIONS.
        /// </summary>
        T4OPTIONS = 292,
        
        /// <summary>
        /// Options for CCITT Group 4 fax encoding. 32 flag bits.
        /// For the list of possible values, see <see cref="Group3Opt"/>.
        /// </summary>
        GROUP4OPTIONS = 293,
        
        /// <summary>
        /// TIFF 6.0 proper name alias for GROUP4OPTIONS.
        /// </summary>
        T6OPTIONS = 293,
        
        /// <summary>
        /// Units of resolutions.
        /// For the list of possible values, see <see cref="ResUnit"/>.
        /// </summary>
        RESOLUTIONUNIT = 296,
        
        /// <summary>
        /// Page numbers of multi-page.
        /// </summary>
        PAGENUMBER = 297,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 6.0]<br/>
        /// Color curve accuracy.
        /// For the list of possible values, see <see cref="ColorResponseUnit"/>.
        /// </summary>
        COLORRESPONSEUNIT = 300,
        
        /// <summary>
        /// Colorimetry info.
        /// </summary>
        TRANSFERFUNCTION = 301,
        
        /// <summary>
        /// Name &amp; release.
        /// </summary>
        SOFTWARE = 305,
        
        /// <summary>
        /// Creation date and time.
        /// </summary>
        DATETIME = 306,
        
        /// <summary>
        /// Creator of image.
        /// </summary>
        ARTIST = 315,
        
        /// <summary>
        /// Machine where created.
        /// </summary>
        HOSTCOMPUTER = 316,
        
        /// <summary>
        /// Prediction scheme w/ LZW.
        /// For the list of possible values, see <see cref="Predictor"/>.
        /// </summary>
        PREDICTOR = 317,
        
        /// <summary>
        /// Image white point.
        /// </summary>
        WHITEPOINT = 318,
        
        /// <summary>
        /// Primary chromaticities.
        /// </summary>
        PRIMARYCHROMATICITIES = 319,
        
        /// <summary>
        /// RGB map for pallette image.
        /// </summary>
        COLORMAP = 320,
        
        /// <summary>
        /// Highlight + shadow info.
        /// </summary>
        HALFTONEHINTS = 321,
        
        /// <summary>
        /// Tile width in pixels.
        /// </summary>
        TILEWIDTH = 322,
        
        /// <summary>
        /// Tile height in pixels.
        /// </summary>
        TILELENGTH = 323,
        
        /// <summary>
        /// Offsets to data tiles.
        /// </summary>
        TILEOFFSETS = 324,
        
        /// <summary>
        /// Byte counts for tiles.
        /// </summary>
        TILEBYTECOUNTS = 325,
        
        /// <summary>
        /// Lines with wrong pixel count.
        /// </summary>
        BADFAXLINES = 326,
        
        /// <summary>
        /// Regenerated line info.
        /// For the list of possible values, see <see cref="CleanFaxData"/>.
        /// </summary>
        CLEANFAXDATA = 327,
        
        /// <summary>
        /// Max consecutive bad lines.
        /// </summary>
        CONSECUTIVEBADFAXLINES = 328,
        
        /// <summary>
        /// Subimage descriptors.
        /// </summary>
        SUBIFD = 330,
        
        /// <summary>
        /// Inks in separated image.
        /// For the list of possible values, see <see cref="InkSet"/>.
        /// </summary>
        INKSET = 332,
        
        /// <summary>
        /// ASCII names of inks.
        /// </summary>
        INKNAMES = 333,
        
        /// <summary>
        /// Number of inks.
        /// </summary>
        NUMBEROFINKS = 334,
        
        /// <summary>
        /// 0% and 100% dot codes.
        /// </summary>
        DOTRANGE = 336,
        
        /// <summary>
        /// Separation target.
        /// </summary>
        TARGETPRINTER = 337,
        
        /// <summary>
        /// Information about extra samples.
        /// For the list of possible values, see <see cref="ExtraSample"/>.
        /// </summary>
        EXTRASAMPLES = 338,
        
        /// <summary>
        /// Data sample format.
        /// For the list of possible values, see <see cref="SampleFormat"/>.
        /// </summary>
        SAMPLEFORMAT = 339,
        
        /// <summary>
        /// Variable MinSampleValue.
        /// </summary>
        SMINSAMPLEVALUE = 340,
        
        /// <summary>
        /// Variable MaxSampleValue.
        /// </summary>
        SMAXSAMPLEVALUE = 341,
        
        /// <summary>
        /// ClipPath. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
        /// </summary>
        CLIPPATH = 343,
        
        /// <summary>
        /// XClipPathUnits. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
        /// </summary>
        XCLIPPATHUNITS = 344,
        
        /// <summary>
        /// YClipPathUnits. Introduced post TIFF rev 6.0 by Adobe TIFF technote 2.
        /// </summary>
        YCLIPPATHUNITS = 345,
        
        /// <summary>
        /// Indexed. Introduced post TIFF rev 6.0 by Adobe TIFF Technote 3.
        /// </summary>
        INDEXED = 346,
        
        /// <summary>
        /// JPEG table stream. Introduced post TIFF rev 6.0.
        /// </summary>
        JPEGTABLES = 347,
        
        /// <summary>
        /// OPI Proxy. Introduced post TIFF rev 6.0 by Adobe TIFF technote.
        /// </summary>
        OPIPROXY = 351,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// JPEG processing algorithm.
        /// For the list of possible values, see <see cref="JpegProc"/>.
        /// </summary>
        JPEGPROC = 512,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// Pointer to SOI marker.
        /// </summary>
        JPEGIFOFFSET = 513,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// JFIF stream length
        /// </summary>
        JPEGIFBYTECOUNT = 514,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// Restart interval length.
        /// </summary>
        JPEGRESTARTINTERVAL = 515,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// Lossless proc predictor.
        /// </summary>
        JPEGLOSSLESSPREDICTORS = 517,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// Lossless point transform.
        /// </summary>
        JPEGPOINTTRANSFORM = 518,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// Q matrice offsets.
        /// </summary>
        JPEGQTABLES = 519,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// DCT table offsets.
        /// </summary>
        JPEGDCTABLES = 520,
        
        /// <summary>
        /// [obsoleted by Technical Note #2 which specifies a revised JPEG-in-TIFF scheme]<br/>
        /// AC coefficient offsets.
        /// </summary>
        JPEGACTABLES = 521,
        
        /// <summary>
        /// RGB -> YCbCr transform.
        /// </summary>
        YCBCRCOEFFICIENTS = 529,
        
        /// <summary>
        /// YCbCr subsampling factors.
        /// </summary>
        YCBCRSUBSAMPLING = 530,
        
        /// <summary>
        /// Subsample positioning.
        /// For the list of possible values, see <see cref="YCbCrPosition"/>.
        /// </summary>
        YCBCRPOSITIONING = 531,
        
        /// <summary>
        /// Colorimetry info.
        /// </summary>
        REFERENCEBLACKWHITE = 532,
        
        /// <summary>
        /// XML packet. Introduced post TIFF rev 6.0 by Adobe XMP Specification, January 2004.
        /// </summary>
        XMLPACKET = 700,
        
        /// <summary>
        /// OPI ImageID. Introduced post TIFF rev 6.0 by Adobe TIFF technote.
        /// </summary>
        OPIIMAGEID = 32781,
        
        /// <summary>
        /// Image reference points. Private tag registered to Island Graphics.
        /// </summary>
        REFPTS = 32953,
        
        /// <summary>
        /// Region-xform tack point. Private tag registered to Island Graphics.
        /// </summary>
        REGIONTACKPOINT = 32954,
        
        /// <summary>
        /// Warp quadrilateral. Private tag registered to Island Graphics.
        /// </summary>
        REGIONWARPCORNERS = 32955,
        
        /// <summary>
        /// Affine transformation matrix. Private tag registered to Island Graphics.
        /// </summary>
        REGIONAFFINE = 32956,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 6.0]<br/>
        /// Use EXTRASAMPLE tag. Private tag registered to SGI.
        /// </summary>
        MATTEING = 32995,
        
        /// <summary>
        /// [obsoleted by TIFF rev. 6.0]<br/>
        /// Use SAMPLEFORMAT tag. Private tag registered to SGI.
        /// </summary>
        DATATYPE = 32996,
        
        /// <summary>
        /// Z depth of image. Private tag registered to SGI.
        /// </summary>
        IMAGEDEPTH = 32997,
        
        /// <summary>
        /// Z depth/data tile. Private tag registered to SGI.
        /// </summary>
        TILEDEPTH = 32998,
        
        /// <summary>
        /// Full image size in X. This tag is set when an image has been cropped out of a larger
        /// image. It reflect width of the original uncropped image. The XPOSITION tag can be used
        /// to determine the position of the smaller image in the larger one.
        /// Private tag registered to Pixar.
        /// </summary>
        PIXAR_IMAGEFULLWIDTH = 33300,
        
        /// <summary>
        /// Full image size in Y. This tag is set when an image has been cropped out of a larger
        /// image. It reflect height of the original uncropped image. The YPOSITION can be used
        /// to determine the position of the smaller image in the larger one.
        /// Private tag registered to Pixar.
        /// </summary>
        PIXAR_IMAGEFULLLENGTH = 33301,
        
        /// <summary>
        /// Texture map format. Used to identify special image modes and data used by Pixar's
        /// texture formats. Private tag registered to Pixar.
        /// </summary>
        PIXAR_TEXTUREFORMAT = 33302, /* t */
        
        /// <summary>
        /// S&amp;T wrap modes. Used to identify special image modes and data used by Pixar's
        /// texture formats. Private tag registered to Pixar.
        /// </summary>
        PIXAR_WRAPMODES = 33303,
        
        /// <summary>
        /// Cotan(fov) for env. maps. Used to identify special image modes and data used by
        /// Pixar's texture formats. Private tag registered to Pixar.
        /// </summary>
        PIXAR_FOVCOT = 33304,
        
        /// <summary>
        /// Used to identify special image modes and data used by Pixar's texture formats.
        /// Private tag registered to Pixar.
        /// </summary>
        PIXAR_MATRIX_WORLDTOSCREEN = 33305,
        
        /// <summary>
        /// Used to identify special image modes and data used by Pixar's texture formats.
        /// Private tag registered to Pixar.
        /// </summary>
        PIXAR_MATRIX_WORLDTOCAMERA = 33306,
        
        /// <summary>
        /// Device serial number. Private tag registered to Eastman Kodak.
        /// </summary>
        WRITERSERIALNUMBER = 33405,
        
        /// <summary>
        /// Copyright string. This tag is listed in the TIFF rev. 6.0 w/ unknown ownership.
        /// </summary>
        COPYRIGHT = 33432,
        
        /// <summary>
        /// IPTC TAG from RichTIFF specifications.
        /// </summary>
        RICHTIFFIPTC = 33723,
        
        /// <summary>
        /// Site name. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8SITE = 34016,
        
        /// <summary>
        /// Color seq. [RGB, CMYK, etc]. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8COLORSEQUENCE = 34017,
        
        /// <summary>
        /// DDES Header. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8HEADER = 34018,
        
        /// <summary>
        /// Raster scanline padding. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8RASTERPADDING = 34019,
        
        /// <summary>
        /// The number of bits in short run. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8BITSPERRUNLENGTH = 34020,
        
        /// <summary>
        /// The number of bits in long run. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8BITSPEREXTENDEDRUNLENGTH = 34021,
        
        /// <summary>
        /// LW colortable. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8COLORTABLE = 34022,
        
        /// <summary>
        /// BP/BL image color switch. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8IMAGECOLORINDICATOR = 34023,
        
        /// <summary>
        /// BP/BL bg color switch. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8BKGCOLORINDICATOR = 34024,
        
        /// <summary>
        /// BP/BL image color value. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8IMAGECOLORVALUE = 34025,
        
        /// <summary>
        /// BP/BL bg color value. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8BKGCOLORVALUE = 34026,
        
        /// <summary>
        /// MP pixel intensity value. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8PIXELINTENSITYRANGE = 34027,
        
        /// <summary>
        /// HC transparency switch. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8TRANSPARENCYINDICATOR = 34028,
        
        /// <summary>
        /// Color characterization table. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8COLORCHARACTERIZATION = 34029,
        
        /// <summary>
        /// HC usage indicator. Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8HCUSAGE = 34030,
        
        /// <summary>
        /// Trapping indicator (untrapped = 0, trapped = 1). Reserved for ANSI IT8 TIFF/IT.
        /// </summary>
        IT8TRAPINDICATOR = 34031,
        
        /// <summary>
        /// CMYK color equivalents.
        /// </summary>
        IT8CMYKEQUIVALENT = 34032,
        
        /// <summary>
        /// Sequence Frame Count. Private tag registered to Texas Instruments.
        /// </summary>
        FRAMECOUNT = 34232,
        
        /// <summary>
        /// Private tag registered to Adobe for PhotoShop.
        /// </summary>
        PHOTOSHOP = 34377,
        
        /// <summary>
        /// Pointer to EXIF private directory. This tag is documented in EXIF specification.
        /// </summary>
        EXIFIFD = 34665,
        
        /// <summary>
        /// ICC profile data. ?? Private tag registered to Adobe. ??
        /// </summary>
        ICCPROFILE = 34675,
        
        /// <summary>
        /// JBIG options. Private tag registered to Pixel Magic.
        /// </summary>
        JBIGOPTIONS = 34750,
        
        /// <summary>
        /// Pointer to GPS private directory. This tag is documented in EXIF specification.
        /// </summary>
        GPSIFD = 34853,
        
        /// <summary>
        /// Encoded Class 2 ses. params. Private tag registered to SGI.
        /// </summary>
        FAXRECVPARAMS = 34908,
        
        /// <summary>
        /// Received SubAddr string. Private tag registered to SGI.
        /// </summary>
        FAXSUBADDRESS = 34909,
        
        /// <summary>
        /// Receive time (secs). Private tag registered to SGI.
        /// </summary>
        FAXRECVTIME = 34910,
        
        /// <summary>
        /// Encoded fax ses. params, Table 2/T.30. Private tag registered to SGI.
        /// </summary>
        FAXDCS = 34911,
        
        /// <summary>
        /// Sample value to Nits. Private tag registered to SGI.
        /// </summary>
        STONITS = 37439,
        
        /// <summary>
        /// Private tag registered to FedEx.
        /// </summary>
        FEDEX_EDR = 34929,
        
        /// <summary>
        /// Pointer to Interoperability private directory.
        /// This tag is documented in EXIF specification.
        /// </summary>
        INTEROPERABILITYIFD = 40965,
        
        /// <summary>
        /// DNG version number. Introduced by Adobe DNG specification.
        /// </summary>
        DNGVERSION = 50706,
        
        /// <summary>
        /// DNG compatibility version. Introduced by Adobe DNG specification.
        /// </summary>
        DNGBACKWARDVERSION = 50707,
        
        /// <summary>
        /// Name for the camera model. Introduced by Adobe DNG specification.
        /// </summary>
        UNIQUECAMERAMODEL = 50708,
        
        /// <summary>
        /// Localized camera model name. Introduced by Adobe DNG specification.
        /// </summary>
        LOCALIZEDCAMERAMODEL = 50709,
        
        /// <summary>
        /// CFAPattern->LinearRaw space mapping. Introduced by Adobe DNG specification.
        /// </summary>
        CFAPLANECOLOR = 50710,
        
        /// <summary>
        /// Spatial layout of the CFA. Introduced by Adobe DNG specification.
        /// </summary>
        CFALAYOUT = 50711,
        
        /// <summary>
        /// Lookup table description. Introduced by Adobe DNG specification.
        /// </summary>
        LINEARIZATIONTABLE = 50712,
        
        /// <summary>
        /// Repeat pattern size for the BlackLevel tag. Introduced by Adobe DNG specification.
        /// </summary>
        BLACKLEVELREPEATDIM = 50713,
        
        /// <summary>
        /// Zero light encoding level. Introduced by Adobe DNG specification.
        /// </summary>
        BLACKLEVEL = 50714,
        
        /// <summary>
        /// Zero light encoding level differences (columns). Introduced by Adobe DNG specification.
        /// </summary>
        BLACKLEVELDELTAH = 50715,
        
        /// <summary>
        /// Zero light encoding level differences (rows). Introduced by Adobe DNG specification.
        /// </summary>
        BLACKLEVELDELTAV = 50716,
        
        /// <summary>
        /// Fully saturated encoding level. Introduced by Adobe DNG specification.
        /// </summary>
        WHITELEVEL = 50717,
        
        /// <summary>
        /// Default scale factors. Introduced by Adobe DNG specification.
        /// </summary>
        DEFAULTSCALE = 50718,
        
        /// <summary>
        /// Origin of the final image area. Introduced by Adobe DNG specification.
        /// </summary>
        DEFAULTCROPORIGIN = 50719,
        
        /// <summary>
        /// Size of the final image area. Introduced by Adobe DNG specification.
        /// </summary>
        DEFAULTCROPSIZE = 50720,
        
        /// <summary>
        /// XYZ->reference color space transformation matrix 1.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        COLORMATRIX1 = 50721,
        
        /// <summary>
        /// XYZ->reference color space transformation matrix 2.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        COLORMATRIX2 = 50722,
        
        /// <summary>
        /// Calibration matrix 1. Introduced by Adobe DNG specification.
        /// </summary>
        CAMERACALIBRATION1 = 50723,
        
        /// <summary>
        /// Calibration matrix 2. Introduced by Adobe DNG specification.
        /// </summary>
        CAMERACALIBRATION2 = 50724,
        
        /// <summary>
        /// Dimensionality reduction matrix 1. Introduced by Adobe DNG specification.
        /// </summary>
        REDUCTIONMATRIX1 = 50725,
        
        /// <summary>
        /// Dimensionality reduction matrix 2. Introduced by Adobe DNG specification.
        /// </summary>
        REDUCTIONMATRIX2 = 50726,
        
        /// <summary>
        /// Gain applied the stored raw values. Introduced by Adobe DNG specification.
        /// </summary>
        ANALOGBALANCE = 50727,
        
        /// <summary>
        /// Selected white balance in linear reference space.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        ASSHOTNEUTRAL = 50728,
        
        /// <summary>
        /// Selected white balance in x-y chromaticity coordinates.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        ASSHOTWHITEXY = 50729,
        
        /// <summary>
        /// How much to move the zero point. Introduced by Adobe DNG specification.
        /// </summary>
        BASELINEEXPOSURE = 50730,
        
        /// <summary>
        /// Relative noise level. Introduced by Adobe DNG specification.
        /// </summary>
        BASELINENOISE = 50731,
        
        /// <summary>
        /// Relative amount of sharpening. Introduced by Adobe DNG specification.
        /// </summary>
        BASELINESHARPNESS = 50732,
        
        /// <summary>
        /// How closely the values of the green pixels in the blue/green rows 
        /// track the values of the green pixels in the red/green rows.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        BAYERGREENSPLIT = 50733,
        
        /// <summary>
        /// Non-linear encoding range. Introduced by Adobe DNG specification.
        /// </summary>
        LINEARRESPONSELIMIT = 50734,
        
        /// <summary>
        /// Camera's serial number. Introduced by Adobe DNG specification.
        /// </summary>
        CAMERASERIALNUMBER = 50735,
        
        /// <summary>
        /// Information about the lens.
        /// </summary>
        LENSINFO = 50736,
        
        /// <summary>
        /// Chroma blur radius. Introduced by Adobe DNG specification.
        /// </summary>
        CHROMABLURRADIUS = 50737,
        
        /// <summary>
        /// Relative strength of the camera's anti-alias filter.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        ANTIALIASSTRENGTH = 50738,
        
        /// <summary>
        /// Used by Adobe Camera Raw. Introduced by Adobe DNG specification.
        /// </summary>
        SHADOWSCALE = 50739,
        
        /// <summary>
        /// Manufacturer's private data. Introduced by Adobe DNG specification.
        /// </summary>
        DNGPRIVATEDATA = 50740,
        
        /// <summary>
        /// Whether the EXIF MakerNote tag is safe to preserve along with the rest of the EXIF data.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        MAKERNOTESAFETY = 50741,
        
        /// <summary>
        /// Illuminant 1. Introduced by Adobe DNG specification.
        /// </summary>
        CALIBRATIONILLUMINANT1 = 50778,
        
        /// <summary>
        /// Illuminant 2. Introduced by Adobe DNG specification.
        /// </summary>
        CALIBRATIONILLUMINANT2 = 50779,
        
        /// <summary>
        /// Best quality multiplier. Introduced by Adobe DNG specification.
        /// </summary>
        BESTQUALITYSCALE = 50780,
        
        /// <summary>
        /// Unique identifier for the raw image data. Introduced by Adobe DNG specification.
        /// </summary>
        RAWDATAUNIQUEID = 50781,
        
        /// <summary>
        /// File name of the original raw file. Introduced by Adobe DNG specification.
        /// </summary>
        ORIGINALRAWFILENAME = 50827,
        
        /// <summary>
        /// Contents of the original raw file. Introduced by Adobe DNG specification.
        /// </summary>
        ORIGINALRAWFILEDATA = 50828,
        
        /// <summary>
        /// Active (non-masked) pixels of the sensor. Introduced by Adobe DNG specification.
        /// </summary>
        ACTIVEAREA = 50829,
        
        /// <summary>
        /// List of coordinates of fully masked pixels. Introduced by Adobe DNG specification.
        /// </summary>
        MASKEDAREAS = 50830,
        
        /// <summary>
        /// Used to map cameras's color space into ICC profile space.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        ASSHOTICCPROFILE = 50831,
        
        /// <summary>
        /// Used to map cameras's color space into ICC profile space.
        /// Introduced by Adobe DNG specification.
        /// </summary>
        ASSHOTPREPROFILEMATRIX = 50832,
        
        /// <summary>
        /// Introduced by Adobe DNG specification.
        /// </summary>
        CURRENTICCPROFILE = 50833,
        
        /// <summary>
        /// Introduced by Adobe DNG specification.
        /// </summary>
        CURRENTPREPROFILEMATRIX = 50834,
        
        /// <summary>
        /// Undefined tag used by Eastman Kodak, hue shift correction data.
        /// </summary>
        DCSHUESHIFTVALUES = 65535,

        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Group 3/4 format control.
        /// For the list of possible values, see <see cref="FaxMode"/>.
        /// </summary>
        FAXMODE = 65536,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Compression quality level. Quality level is on the IJG 0-100 scale. Default value is 75.
        /// </summary>
        JPEGQUALITY = 65537,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Auto RGB&lt;=&gt;YCbCr convert.
        /// For the list of possible values, see <see cref="JpegColorMode"/>.
        /// </summary>
        JPEGCOLORMODE = 65538,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// For the list of possible values, see <see cref="JpegTablesMode"/>.
        /// Default is <see cref="JpegTablesMode.QUANT"/> | <see cref="JpegTablesMode.HUFF"/>.
        /// </summary>
        JPEGTABLESMODE = 65539,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// G3/G4 fill function.
        /// </summary>
        FAXFILLFUNC = 65540,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// PixarLogCodec I/O data sz.
        /// </summary>
        PIXARLOGDATAFMT = 65549,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Imager mode &amp; filter.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>).
        /// </summary>
        DCSIMAGERTYPE = 65550,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Interpolation mode.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>).
        /// </summary>
        DCSINTERPMODE = 65551,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Color balance values.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>). 
        /// </summary>
        DCSBALANCEARRAY = 65552,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Color correction values.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>). 
        /// </summary>
        DCSCORRECTMATRIX = 65553,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Gamma value.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>). 
        /// </summary>
        DCSGAMMA = 65554,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Toe &amp; shoulder points.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>). 
        /// </summary>
        DCSTOESHOULDERPTS = 65555,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Calibration file description.
        /// </summary>
        DCSCALIBRATIONFD = 65556,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Compression quality level.
        /// Quality level is on the ZLIB 1-9 scale. Default value is -1.
        /// </summary>
        ZIPQUALITY = 65557,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// PixarLog uses same scale.
        /// </summary>
        PIXARLOGQUALITY = 65558,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// Area of image to acquire.
        /// Allocated to Oceana Matrix (<a href="mailto:dev@oceana.com">dev@oceana.com</a>).
        /// </summary>
        DCSCLIPRECTANGLE = 65559,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// SGILog user data format.
        /// </summary>
        SGILOGDATAFMT = 65560,
        
        /// <summary>
        /// [pseudo tag. not written to file]<br/>
        /// SGILog data encoding control.
        /// </summary>
        SGILOGENCODE = 65561,

        /// <summary>
        /// Exposure time.
        /// </summary>
        EXIF_EXPOSURETIME = 33434,
        
        /// <summary>
        /// F number.
        /// </summary>
        EXIF_FNUMBER = 33437,
        
        /// <summary>
        /// Exposure program.
        /// </summary>
        EXIF_EXPOSUREPROGRAM = 34850,
        
        /// <summary>
        /// Spectral sensitivity.
        /// </summary>
        EXIF_SPECTRALSENSITIVITY = 34852,
        
        /// <summary>
        /// ISO speed rating.
        /// </summary>
        EXIF_ISOSPEEDRATINGS = 34855,
        
        /// <summary>
        /// Optoelectric conversion factor.
        /// </summary>
        EXIF_OECF = 34856,
        
        /// <summary>
        /// Exif version.
        /// </summary>
        EXIF_EXIFVERSION = 36864,
        
        /// <summary>
        /// Date and time of original data generation.
        /// </summary>
        EXIF_DATETIMEORIGINAL = 36867,
        
        /// <summary>
        /// Date and time of digital data generation.
        /// </summary>
        EXIF_DATETIMEDIGITIZED = 36868,
        
        /// <summary>
        /// Meaning of each component.
        /// </summary>
        EXIF_COMPONENTSCONFIGURATION = 37121,
        
        /// <summary>
        /// Image compression mode.
        /// </summary>
        EXIF_COMPRESSEDBITSPERPIXEL = 37122,
        
        /// <summary>
        /// Shutter speed.
        /// </summary>
        EXIF_SHUTTERSPEEDVALUE = 37377,
        
        /// <summary>
        /// Aperture.
        /// </summary>
        EXIF_APERTUREVALUE = 37378,
        
        /// <summary>
        /// Brightness.
        /// </summary>
        EXIF_BRIGHTNESSVALUE = 37379,
        
        /// <summary>
        /// Exposure bias.
        /// </summary>
        EXIF_EXPOSUREBIASVALUE = 37380,
        
        /// <summary>
        /// Maximum lens aperture.
        /// </summary>
        EXIF_MAXAPERTUREVALUE = 37381,
        
        /// <summary>
        /// Subject distance.
        /// </summary>
        EXIF_SUBJECTDISTANCE = 37382,
        
        /// <summary>
        /// Metering mode.
        /// </summary>
        EXIF_METERINGMODE = 37383,
        
        /// <summary>
        /// Light source.
        /// </summary>
        EXIF_LIGHTSOURCE = 37384,
        
        /// <summary>
        /// Flash.
        /// </summary>
        EXIF_FLASH = 37385,
        
        /// <summary>
        /// Lens focal length.
        /// </summary>
        EXIF_FOCALLENGTH = 37386,
        
        /// <summary>
        /// Subject area.
        /// </summary>
        EXIF_SUBJECTAREA = 37396,
        
        /// <summary>
        /// Manufacturer notes.
        /// </summary>
        EXIF_MAKERNOTE = 37500,
        
        /// <summary>
        /// User comments.
        /// </summary>
        EXIF_USERCOMMENT = 37510,
        
        /// <summary>
        /// DateTime subseconds.
        /// </summary>
        EXIF_SUBSECTIME = 37520,
        
        /// <summary>
        /// DateTimeOriginal subseconds.
        /// </summary>
        EXIF_SUBSECTIMEORIGINAL = 37521,
        
        /// <summary>
        /// DateTimeDigitized subseconds.
        /// </summary>
        EXIF_SUBSECTIMEDIGITIZED = 37522,
        
        /// <summary>
        /// Supported Flashpix version.
        /// </summary>
        EXIF_FLASHPIXVERSION = 40960,
        
        /// <summary>
        /// Color space information.
        /// </summary>
        EXIF_COLORSPACE = 40961,
        
        /// <summary>
        /// Valid image width.
        /// </summary>
        EXIF_PIXELXDIMENSION = 40962,
        
        /// <summary>
        /// Valid image height.
        /// </summary>
        EXIF_PIXELYDIMENSION = 40963,
        
        /// <summary>
        /// Related audio file.
        /// </summary>
        EXIF_RELATEDSOUNDFILE = 40964,
        
        /// <summary>
        /// Flash energy.
        /// </summary>
        EXIF_FLASHENERGY = 41483,
        
        /// <summary>
        /// Spatial frequency response.
        /// </summary>
        EXIF_SPATIALFREQUENCYRESPONSE = 41484,
        
        /// <summary>
        /// Focal plane X resolution.
        /// </summary>
        EXIF_FOCALPLANEXRESOLUTION = 41486,
        
        /// <summary>
        /// Focal plane Y resolution.
        /// </summary>
        EXIF_FOCALPLANEYRESOLUTION = 41487,
        
        /// <summary>
        /// Focal plane resolution unit.
        /// </summary>
        EXIF_FOCALPLANERESOLUTIONUNIT = 41488,
        
        /// <summary>
        /// Subject location.
        /// </summary>
        EXIF_SUBJECTLOCATION = 41492,
        
        /// <summary>
        /// Exposure index.
        /// </summary>
        EXIF_EXPOSUREINDEX = 41493,
        
        /// <summary>
        /// Sensing method.
        /// </summary>
        EXIF_SENSINGMETHOD = 41495,
        
        /// <summary>
        /// File source.
        /// </summary>
        EXIF_FILESOURCE = 41728,
        
        /// <summary>
        /// Scene type.
        /// </summary>
        EXIF_SCENETYPE = 41729,
        
        /// <summary>
        /// CFA pattern.
        /// </summary>
        EXIF_CFAPATTERN = 41730,
        
        /// <summary>
        /// Custom image processing.
        /// </summary>
        EXIF_CUSTOMRENDERED = 41985,
        
        /// <summary>
        /// Exposure mode.
        /// </summary>
        EXIF_EXPOSUREMODE = 41986,
        
        /// <summary>
        /// White balance.
        /// </summary>
        EXIF_WHITEBALANCE = 41987,
        
        /// <summary>
        /// Digital zoom ratio.
        /// </summary>
        EXIF_DIGITALZOOMRATIO = 41988,
        
        /// <summary>
        /// Focal length in 35 mm film.
        /// </summary>
        EXIF_FOCALLENGTHIN35MMFILM = 41989,
        
        /// <summary>
        /// Scene capture type.
        /// </summary>
        EXIF_SCENECAPTURETYPE = 41990,
        
        /// <summary>
        /// Gain control.
        /// </summary>
        EXIF_GAINCONTROL = 41991,
        
        /// <summary>
        /// Contrast.
        /// </summary>
        EXIF_CONTRAST = 41992,
        
        /// <summary>
        /// Saturation.
        /// </summary>
        EXIF_SATURATION = 41993,
        
        /// <summary>
        /// Sharpness.
        /// </summary>
        EXIF_SHARPNESS = 41994,
        
        /// <summary>
        /// Device settings description.
        /// </summary>
        EXIF_DEVICESETTINGDESCRIPTION = 41995,
        
        /// <summary>
        /// Subject distance range.
        /// </summary>
        EXIF_SUBJECTDISTANCERANGE = 41996,
        
        /// <summary>
        /// Unique image ID.
        /// </summary>
        EXIF_IMAGEUNIQUEID = 42016,
    }
}
