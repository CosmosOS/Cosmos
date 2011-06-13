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
 * Core Directory Tag Support.
 */

using System.Globalization;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        /// <summary>
        /// NB:   THIS ARRAY IS ASSUMED TO BE SORTED BY TAG.
        ///       If a tag can have both LONG and SHORT types then the LONG must
        ///       be placed before the SHORT for writing to work properly.
        ///       
        /// NOTE: The second field (field_readcount) and third field
        ///       (field_writecount) sometimes use the values
        ///       TiffFieldInfo.Variable (-1), TiffFieldInfo.Variable2 (-3)
        ///       and TiffFieldInfo.Spp (-2). These values should be used but
        ///       would throw off the formatting of the code, so please
        ///       interpret the -1, -2 and -3  values accordingly.
        /// </summary>
        static TiffFieldInfo[] tiffFieldInfo = 
        {
            new TiffFieldInfo(TiffTag.SUBFILETYPE, 1, 1, TiffType.LONG, FieldBit.SubFileType, true, false, "SubfileType"), 
            /* XXX SHORT for compatibility w/ old versions of the library */
            new TiffFieldInfo(TiffTag.SUBFILETYPE, 1, 1, TiffType.SHORT, FieldBit.SubFileType, true, false, "SubfileType"), 
            new TiffFieldInfo(TiffTag.OSUBFILETYPE, 1, 1, TiffType.SHORT, FieldBit.SubFileType, true, false, "OldSubfileType"), 
            new TiffFieldInfo(TiffTag.IMAGEWIDTH, 1, 1, TiffType.LONG, FieldBit.ImageDimensions, false, false, "ImageWidth"), 
            new TiffFieldInfo(TiffTag.IMAGEWIDTH, 1, 1, TiffType.SHORT, FieldBit.ImageDimensions, false, false, "ImageWidth"), 
            new TiffFieldInfo(TiffTag.IMAGELENGTH, 1, 1, TiffType.LONG, FieldBit.ImageDimensions, true, false, "ImageLength"), 
            new TiffFieldInfo(TiffTag.IMAGELENGTH, 1, 1, TiffType.SHORT, FieldBit.ImageDimensions, true, false, "ImageLength"), 
            new TiffFieldInfo(TiffTag.BITSPERSAMPLE, -1, -1, TiffType.SHORT, FieldBit.BitsPerSample, false, false, "BitsPerSample"), 
            /* XXX LONG for compatibility with some broken TIFF writers */
            new TiffFieldInfo(TiffTag.BITSPERSAMPLE, -1, -1, TiffType.LONG, FieldBit.BitsPerSample, false, false, "BitsPerSample"), 
            new TiffFieldInfo(TiffTag.COMPRESSION, -1, 1, TiffType.SHORT, FieldBit.Compression, false, false, "Compression"), 
            /* XXX LONG for compatibility with some broken TIFF writers */
            new TiffFieldInfo(TiffTag.COMPRESSION, -1, 1, TiffType.LONG, FieldBit.Compression, false, false, "Compression"), 
            new TiffFieldInfo(TiffTag.PHOTOMETRIC, 1, 1, TiffType.SHORT, FieldBit.Photometric, false, false, "PhotometricInterpretation"), 
            /* XXX LONG for compatibility with some broken TIFF writers */
            new TiffFieldInfo(TiffTag.PHOTOMETRIC, 1, 1, TiffType.LONG, FieldBit.Photometric, false, false, "PhotometricInterpretation"), 
            new TiffFieldInfo(TiffTag.THRESHHOLDING, 1, 1, TiffType.SHORT, FieldBit.Thresholding, true, false, "Threshholding"), 
            new TiffFieldInfo(TiffTag.CELLWIDTH, 1, 1, TiffType.SHORT, FieldBit.Ignore, true, false, "CellWidth"), 
            new TiffFieldInfo(TiffTag.CELLLENGTH, 1, 1, TiffType.SHORT, FieldBit.Ignore, true, false, "CellLength"), 
            new TiffFieldInfo(TiffTag.FILLORDER, 1, 1, TiffType.SHORT, FieldBit.FillOrder, false, false, "FillOrder"), 
            new TiffFieldInfo(TiffTag.DOCUMENTNAME, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "DocumentName"), 
            new TiffFieldInfo(TiffTag.IMAGEDESCRIPTION, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "ImageDescription"), 
            new TiffFieldInfo(TiffTag.MAKE, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "Make"), 
            new TiffFieldInfo(TiffTag.MODEL, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "Model"), 
            new TiffFieldInfo(TiffTag.STRIPOFFSETS, -1, -1, TiffType.LONG, FieldBit.StripOffsets, false, false, "StripOffsets"), 
            new TiffFieldInfo(TiffTag.STRIPOFFSETS, -1, -1, TiffType.SHORT, FieldBit.StripOffsets, false, false, "StripOffsets"), 
            new TiffFieldInfo(TiffTag.ORIENTATION, 1, 1, TiffType.SHORT, FieldBit.Orientation, false, false, "Orientation"), 
            new TiffFieldInfo(TiffTag.SAMPLESPERPIXEL, 1, 1, TiffType.SHORT, FieldBit.SamplesPerPixel, false, false, "SamplesPerPixel"), 
            new TiffFieldInfo(TiffTag.ROWSPERSTRIP, 1, 1, TiffType.LONG, FieldBit.RowsPerStrip, false, false, "RowsPerStrip"), 
            new TiffFieldInfo(TiffTag.ROWSPERSTRIP, 1, 1, TiffType.SHORT, FieldBit.RowsPerStrip, false, false, "RowsPerStrip"), 
            new TiffFieldInfo(TiffTag.STRIPBYTECOUNTS, -1, -1, TiffType.LONG, FieldBit.StripByteCounts, false, false, "StripByteCounts"), 
            new TiffFieldInfo(TiffTag.STRIPBYTECOUNTS, -1, -1, TiffType.SHORT, FieldBit.StripByteCounts, false, false, "StripByteCounts"), 
            new TiffFieldInfo(TiffTag.MINSAMPLEVALUE, -2, -1, TiffType.SHORT, FieldBit.MinSampleValue, true, false, "MinSampleValue"), 
            new TiffFieldInfo(TiffTag.MAXSAMPLEVALUE, -2, -1, TiffType.SHORT, FieldBit.MaxSampleValue, true, false, "MaxSampleValue"), 
            new TiffFieldInfo(TiffTag.XRESOLUTION, 1, 1, TiffType.RATIONAL, FieldBit.Resolution, true, false, "XResolution"), 
            new TiffFieldInfo(TiffTag.YRESOLUTION, 1, 1, TiffType.RATIONAL, FieldBit.Resolution, true, false, "YResolution"), 
            new TiffFieldInfo(TiffTag.PLANARCONFIG, 1, 1, TiffType.SHORT, FieldBit.PlanarConfig, false, false, "PlanarConfiguration"), 
            new TiffFieldInfo(TiffTag.PAGENAME, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "PageName"), 
            new TiffFieldInfo(TiffTag.XPOSITION, 1, 1, TiffType.RATIONAL, FieldBit.Position, true, false, "XPosition"), 
            new TiffFieldInfo(TiffTag.YPOSITION, 1, 1, TiffType.RATIONAL, FieldBit.Position, true, false, "YPosition"), 
            new TiffFieldInfo(TiffTag.FREEOFFSETS, -1, -1, TiffType.LONG, FieldBit.Ignore, false, false, "FreeOffsets"), 
            new TiffFieldInfo(TiffTag.FREEBYTECOUNTS, -1, -1, TiffType.LONG, FieldBit.Ignore, false, false, "FreeByteCounts"), 
            new TiffFieldInfo(TiffTag.GRAYRESPONSEUNIT, 1, 1, TiffType.SHORT, FieldBit.Ignore, true, false, "GrayResponseUnit"), 
            new TiffFieldInfo(TiffTag.GRAYRESPONSECURVE, -1, -1, TiffType.SHORT, FieldBit.Ignore, true, false, "GrayResponseCurve"), 
            new TiffFieldInfo(TiffTag.RESOLUTIONUNIT, 1, 1, TiffType.SHORT, FieldBit.ResolutionUnit, true, false, "ResolutionUnit"), 
            new TiffFieldInfo(TiffTag.PAGENUMBER, 2, 2, TiffType.SHORT, FieldBit.PageNumber, true, false, "PageNumber"), 
            new TiffFieldInfo(TiffTag.COLORRESPONSEUNIT, 1, 1, TiffType.SHORT, FieldBit.Ignore, true, false, "ColorResponseUnit"), 
            new TiffFieldInfo(TiffTag.TRANSFERFUNCTION, -1, -1, TiffType.SHORT, FieldBit.TransferFunction, true, false, "TransferFunction"), 
            new TiffFieldInfo(TiffTag.SOFTWARE, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "Software"), 
            new TiffFieldInfo(TiffTag.DATETIME, 20, 20, TiffType.ASCII, FieldBit.Custom, true, false, "DateTime"), 
            new TiffFieldInfo(TiffTag.ARTIST, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "Artist"), 
            new TiffFieldInfo(TiffTag.HOSTCOMPUTER, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "HostComputer"), 
            new TiffFieldInfo(TiffTag.WHITEPOINT, 2, 2, TiffType.RATIONAL, FieldBit.Custom, true, false, "WhitePoint"), 
            new TiffFieldInfo(TiffTag.PRIMARYCHROMATICITIES, 6, 6, TiffType.RATIONAL, FieldBit.Custom, true, false, "PrimaryChromaticities"), 
            new TiffFieldInfo(TiffTag.COLORMAP, -1, -1, TiffType.SHORT, FieldBit.ColorMap, true, false, "ColorMap"), 
            new TiffFieldInfo(TiffTag.HALFTONEHINTS, 2, 2, TiffType.SHORT, FieldBit.HalftoneHints, true, false, "HalftoneHints"), 
            new TiffFieldInfo(TiffTag.TILEWIDTH, 1, 1, TiffType.LONG, FieldBit.TileDimensions, false, false, "TileWidth"), 
            new TiffFieldInfo(TiffTag.TILEWIDTH, 1, 1, TiffType.SHORT, FieldBit.TileDimensions, false, false, "TileWidth"), 
            new TiffFieldInfo(TiffTag.TILELENGTH, 1, 1, TiffType.LONG, FieldBit.TileDimensions, false, false, "TileLength"), 
            new TiffFieldInfo(TiffTag.TILELENGTH, 1, 1, TiffType.SHORT, FieldBit.TileDimensions, false, false, "TileLength"), 
            new TiffFieldInfo(TiffTag.TILEOFFSETS, -1, 1, TiffType.LONG, FieldBit.StripOffsets, false, false, "TileOffsets"), 
            new TiffFieldInfo(TiffTag.TILEBYTECOUNTS, -1, 1, TiffType.LONG, FieldBit.StripByteCounts, false, false, "TileByteCounts"), 
            new TiffFieldInfo(TiffTag.TILEBYTECOUNTS, -1, 1, TiffType.SHORT, FieldBit.StripByteCounts, false, false, "TileByteCounts"), 
            new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.IFD, FieldBit.SubIFD, true, true, "SubIFD"), 
            new TiffFieldInfo(TiffTag.SUBIFD, -1, -1, TiffType.LONG, FieldBit.SubIFD, true, true, "SubIFD"), 
            new TiffFieldInfo(TiffTag.INKSET, 1, 1, TiffType.SHORT, FieldBit.Custom, false, false, "InkSet"), 
            new TiffFieldInfo(TiffTag.INKNAMES, -1, -1, TiffType.ASCII, FieldBit.InkNames, true, true, "InkNames"), 
            new TiffFieldInfo(TiffTag.NUMBEROFINKS, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "NumberOfInks"), 
            new TiffFieldInfo(TiffTag.DOTRANGE, 2, 2, TiffType.SHORT, FieldBit.Custom, false, false, "DotRange"), 
            new TiffFieldInfo(TiffTag.DOTRANGE, 2, 2, TiffType.BYTE, FieldBit.Custom, false, false, "DotRange"), 
            new TiffFieldInfo(TiffTag.TARGETPRINTER, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "TargetPrinter"), 
            new TiffFieldInfo(TiffTag.EXTRASAMPLES, -1, -1, TiffType.SHORT, FieldBit.ExtraSamples, false, true, "ExtraSamples"), 
            /* XXX for bogus Adobe Photoshop v2.5 files */
            new TiffFieldInfo(TiffTag.EXTRASAMPLES, -1, -1, TiffType.BYTE, FieldBit.ExtraSamples, false, true, "ExtraSamples"), 
            new TiffFieldInfo(TiffTag.SAMPLEFORMAT, -1, -1, TiffType.SHORT, FieldBit.SampleFormat, false, false, "SampleFormat"), 
            new TiffFieldInfo(TiffTag.SMINSAMPLEVALUE, -2, -1, TiffType.ANY, FieldBit.SMinSampleValue, true, false, "SMinSampleValue"), 
            new TiffFieldInfo(TiffTag.SMAXSAMPLEVALUE, -2, -1, TiffType.ANY, FieldBit.SMaxSampleValue, true, false, "SMaxSampleValue"), 
            new TiffFieldInfo(TiffTag.CLIPPATH, -1, -3, TiffType.BYTE, FieldBit.Custom, false, true, "ClipPath"), 
            new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SLONG, FieldBit.Custom, false, false, "XClipPathUnits"), 
            new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SSHORT, FieldBit.Custom, false, false, "XClipPathUnits"), 
            new TiffFieldInfo(TiffTag.XCLIPPATHUNITS, 1, 1, TiffType.SBYTE, FieldBit.Custom, false, false, "XClipPathUnits"), 
            new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SLONG, FieldBit.Custom, false, false, "YClipPathUnits"), 
            new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SSHORT, FieldBit.Custom, false, false, "YClipPathUnits"), 
            new TiffFieldInfo(TiffTag.YCLIPPATHUNITS, 1, 1, TiffType.SBYTE, FieldBit.Custom, false, false, "YClipPathUnits"), 
            new TiffFieldInfo(TiffTag.YCBCRCOEFFICIENTS, 3, 3, TiffType.RATIONAL, FieldBit.Custom, false, false, "YCbCrCoefficients"), 
            new TiffFieldInfo(TiffTag.YCBCRSUBSAMPLING, 2, 2, TiffType.SHORT, FieldBit.YCbCrSubsampling, false, false, "YCbCrSubsampling"), 
            new TiffFieldInfo(TiffTag.YCBCRPOSITIONING, 1, 1, TiffType.SHORT, FieldBit.YCbCrPositioning, false, false, "YCbCrPositioning"), 
            new TiffFieldInfo(TiffTag.REFERENCEBLACKWHITE, 6, 6, TiffType.RATIONAL, FieldBit.RefBlackWhite, true, false, "ReferenceBlackWhite"), 
            /* XXX temporarily accept LONG for backwards compatibility */
            new TiffFieldInfo(TiffTag.REFERENCEBLACKWHITE, 6, 6, TiffType.LONG, FieldBit.RefBlackWhite, true, false, "ReferenceBlackWhite"), 
            new TiffFieldInfo(TiffTag.XMLPACKET, -3, -3, TiffType.BYTE, FieldBit.Custom, false, true, "XMLPacket"), 
            /* begin SGI tags */
            new TiffFieldInfo(TiffTag.MATTEING, 1, 1, TiffType.SHORT, FieldBit.ExtraSamples, false, false, "Matteing"), 
            new TiffFieldInfo(TiffTag.DATATYPE, -2, -1, TiffType.SHORT, FieldBit.SampleFormat, false, false, "DataType"), 
            new TiffFieldInfo(TiffTag.IMAGEDEPTH, 1, 1, TiffType.LONG, FieldBit.ImageDepth, false, false, "ImageDepth"), 
            new TiffFieldInfo(TiffTag.IMAGEDEPTH, 1, 1, TiffType.SHORT, FieldBit.ImageDepth, false, false, "ImageDepth"), 
            new TiffFieldInfo(TiffTag.TILEDEPTH, 1, 1, TiffType.LONG, FieldBit.TileDepth, false, false, "TileDepth"), 
            new TiffFieldInfo(TiffTag.TILEDEPTH, 1, 1, TiffType.SHORT, FieldBit.TileDepth, false, false, "TileDepth"), 
            /* end SGI tags */
            /* begin Pixar tags */
            new TiffFieldInfo(TiffTag.PIXAR_IMAGEFULLWIDTH, 1, 1, TiffType.LONG, FieldBit.Custom, true, false, "ImageFullWidth"), 
            new TiffFieldInfo(TiffTag.PIXAR_IMAGEFULLLENGTH, 1, 1, TiffType.LONG, FieldBit.Custom, true, false, "ImageFullLength"), 
            new TiffFieldInfo(TiffTag.PIXAR_TEXTUREFORMAT, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "TextureFormat"), 
            new TiffFieldInfo(TiffTag.PIXAR_WRAPMODES, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "TextureWrapModes"), 
            new TiffFieldInfo(TiffTag.PIXAR_FOVCOT, 1, 1, TiffType.FLOAT, FieldBit.Custom, true, false, "FieldOfViewCotangent"), 
            new TiffFieldInfo(TiffTag.PIXAR_MATRIX_WORLDTOSCREEN, 16, 16, TiffType.FLOAT, FieldBit.Custom, true, false, "MatrixWorldToScreen"), 
            new TiffFieldInfo(TiffTag.PIXAR_MATRIX_WORLDTOCAMERA, 16, 16, TiffType.FLOAT, FieldBit.Custom, true, false, "MatrixWorldToCamera"), 
            new TiffFieldInfo(TiffTag.COPYRIGHT, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "Copyright"), 
            /* end Pixar tags */
            new TiffFieldInfo(TiffTag.RICHTIFFIPTC, -3, -3, TiffType.LONG, FieldBit.Custom, false, true, "RichTIFFIPTC"), 
            new TiffFieldInfo(TiffTag.PHOTOSHOP, -3, -3, TiffType.BYTE, FieldBit.Custom, false, true, "Photoshop"), 
            new TiffFieldInfo(TiffTag.EXIFIFD, 1, 1, TiffType.LONG, FieldBit.Custom, false, false, "EXIFIFDOffset"), 
            new TiffFieldInfo(TiffTag.ICCPROFILE, -3, -3, TiffType.UNDEFINED, FieldBit.Custom, false, true, "ICC Profile"), 
            new TiffFieldInfo(TiffTag.GPSIFD, 1, 1, TiffType.LONG, FieldBit.Custom, false, false, "GPSIFDOffset"), 
            new TiffFieldInfo(TiffTag.STONITS, 1, 1, TiffType.DOUBLE, FieldBit.Custom, false, false, "StoNits"), 
            new TiffFieldInfo(TiffTag.INTEROPERABILITYIFD, 1, 1, TiffType.LONG, FieldBit.Custom, false, false, "InteroperabilityIFDOffset"), 
            /* begin DNG tags */
            new TiffFieldInfo(TiffTag.DNGVERSION, 4, 4, TiffType.BYTE, FieldBit.Custom, false, false, "DNGVersion"), 
            new TiffFieldInfo(TiffTag.DNGBACKWARDVERSION, 4, 4, TiffType.BYTE, FieldBit.Custom, false, false, "DNGBackwardVersion"), 
            new TiffFieldInfo(TiffTag.UNIQUECAMERAMODEL, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "UniqueCameraModel"), 
            new TiffFieldInfo(TiffTag.LOCALIZEDCAMERAMODEL, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "LocalizedCameraModel"), 
            new TiffFieldInfo(TiffTag.LOCALIZEDCAMERAMODEL, -1, -1, TiffType.BYTE, FieldBit.Custom, true, true, "LocalizedCameraModel"), 
            new TiffFieldInfo(TiffTag.CFAPLANECOLOR, -1, -1, TiffType.BYTE, FieldBit.Custom, false, true, "CFAPlaneColor"), 
            new TiffFieldInfo(TiffTag.CFALAYOUT, 1, 1, TiffType.SHORT, FieldBit.Custom, false, false, "CFALayout"), 
            new TiffFieldInfo(TiffTag.LINEARIZATIONTABLE, -1, -1, TiffType.SHORT, FieldBit.Custom, false, true, "LinearizationTable"), 
            new TiffFieldInfo(TiffTag.BLACKLEVELREPEATDIM, 2, 2, TiffType.SHORT, FieldBit.Custom, false, false, "BlackLevelRepeatDim"), 
            new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.LONG, FieldBit.Custom, false, true, "BlackLevel"), 
            new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.SHORT, FieldBit.Custom, false, true, "BlackLevel"), 
            new TiffFieldInfo(TiffTag.BLACKLEVEL, -1, -1, TiffType.RATIONAL, FieldBit.Custom, false, true, "BlackLevel"), 
            new TiffFieldInfo(TiffTag.BLACKLEVELDELTAH, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "BlackLevelDeltaH"), 
            new TiffFieldInfo(TiffTag.BLACKLEVELDELTAV, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "BlackLevelDeltaV"), 
            new TiffFieldInfo(TiffTag.WHITELEVEL, -2, -2, TiffType.LONG, FieldBit.Custom, false, false, "WhiteLevel"), 
            new TiffFieldInfo(TiffTag.WHITELEVEL, -2, -2, TiffType.SHORT, FieldBit.Custom, false, false, "WhiteLevel"), 
            new TiffFieldInfo(TiffTag.DEFAULTSCALE, 2, 2, TiffType.RATIONAL, FieldBit.Custom, false, false, "DefaultScale"), 
            new TiffFieldInfo(TiffTag.BESTQUALITYSCALE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "BestQualityScale"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.LONG, FieldBit.Custom, false, false, "DefaultCropOrigin"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.SHORT, FieldBit.Custom, false, false, "DefaultCropOrigin"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPORIGIN, 2, 2, TiffType.RATIONAL, FieldBit.Custom, false, false, "DefaultCropOrigin"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.LONG, FieldBit.Custom, false, false, "DefaultCropSize"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.SHORT, FieldBit.Custom, false, false, "DefaultCropSize"), 
            new TiffFieldInfo(TiffTag.DEFAULTCROPSIZE, 2, 2, TiffType.RATIONAL, FieldBit.Custom, false, false, "DefaultCropSize"), 
            new TiffFieldInfo(TiffTag.COLORMATRIX1, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "ColorMatrix1"), 
            new TiffFieldInfo(TiffTag.COLORMATRIX2, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "ColorMatrix2"), 
            new TiffFieldInfo(TiffTag.CAMERACALIBRATION1, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "CameraCalibration1"), 
            new TiffFieldInfo(TiffTag.CAMERACALIBRATION2, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "CameraCalibration2"), 
            new TiffFieldInfo(TiffTag.REDUCTIONMATRIX1, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "ReductionMatrix1"), 
            new TiffFieldInfo(TiffTag.REDUCTIONMATRIX2, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "ReductionMatrix2"), 
            new TiffFieldInfo(TiffTag.ANALOGBALANCE, -1, -1, TiffType.RATIONAL, FieldBit.Custom, false, true, "AnalogBalance"), 
            new TiffFieldInfo(TiffTag.ASSHOTNEUTRAL, -1, -1, TiffType.SHORT, FieldBit.Custom, false, true, "AsShotNeutral"), 
            new TiffFieldInfo(TiffTag.ASSHOTNEUTRAL, -1, -1, TiffType.RATIONAL, FieldBit.Custom, false, true, "AsShotNeutral"), 
            new TiffFieldInfo(TiffTag.ASSHOTWHITEXY, 2, 2, TiffType.RATIONAL, FieldBit.Custom, false, false, "AsShotWhiteXY"), 
            new TiffFieldInfo(TiffTag.BASELINEEXPOSURE, 1, 1, TiffType.SRATIONAL, FieldBit.Custom, false, false, "BaselineExposure"), 
            new TiffFieldInfo(TiffTag.BASELINENOISE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "BaselineNoise"), 
            new TiffFieldInfo(TiffTag.BASELINESHARPNESS, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "BaselineSharpness"), 
            new TiffFieldInfo(TiffTag.BAYERGREENSPLIT, 1, 1, TiffType.LONG, FieldBit.Custom, false, false, "BayerGreenSplit"), 
            new TiffFieldInfo(TiffTag.LINEARRESPONSELIMIT, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "LinearResponseLimit"), 
            new TiffFieldInfo(TiffTag.CAMERASERIALNUMBER, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "CameraSerialNumber"), 
            new TiffFieldInfo(TiffTag.LENSINFO, 4, 4, TiffType.RATIONAL, FieldBit.Custom, false, false, "LensInfo"), 
            new TiffFieldInfo(TiffTag.CHROMABLURRADIUS, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "ChromaBlurRadius"), 
            new TiffFieldInfo(TiffTag.ANTIALIASSTRENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "AntiAliasStrength"), 
            new TiffFieldInfo(TiffTag.SHADOWSCALE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, false, false, "ShadowScale"), 
            new TiffFieldInfo(TiffTag.DNGPRIVATEDATA, -1, -1, TiffType.BYTE, FieldBit.Custom, false, true, "DNGPrivateData"), 
            new TiffFieldInfo(TiffTag.MAKERNOTESAFETY, 1, 1, TiffType.SHORT, FieldBit.Custom, false, false, "MakerNoteSafety"), 
            new TiffFieldInfo(TiffTag.CALIBRATIONILLUMINANT1, 1, 1, TiffType.SHORT, FieldBit.Custom, false, false, "CalibrationIlluminant1"), 
            new TiffFieldInfo(TiffTag.CALIBRATIONILLUMINANT2, 1, 1, TiffType.SHORT, FieldBit.Custom, false, false, "CalibrationIlluminant2"), 
            new TiffFieldInfo(TiffTag.RAWDATAUNIQUEID, 16, 16, TiffType.BYTE, FieldBit.Custom, false, false, "RawDataUniqueID"), 
            new TiffFieldInfo(TiffTag.ORIGINALRAWFILENAME, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "OriginalRawFileName"), 
            new TiffFieldInfo(TiffTag.ORIGINALRAWFILENAME, -1, -1, TiffType.BYTE, FieldBit.Custom, true, true, "OriginalRawFileName"), 
            new TiffFieldInfo(TiffTag.ORIGINALRAWFILEDATA, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, false, true, "OriginalRawFileData"), 
            new TiffFieldInfo(TiffTag.ACTIVEAREA, 4, 4, TiffType.LONG, FieldBit.Custom, false, false, "ActiveArea"), 
            new TiffFieldInfo(TiffTag.ACTIVEAREA, 4, 4, TiffType.SHORT, FieldBit.Custom, false, false, "ActiveArea"), 
            new TiffFieldInfo(TiffTag.MASKEDAREAS, -1, -1, TiffType.LONG, FieldBit.Custom, false, true, "MaskedAreas"), 
            new TiffFieldInfo(TiffTag.ASSHOTICCPROFILE, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, false, true, "AsShotICCProfile"), 
            new TiffFieldInfo(TiffTag.ASSHOTPREPROFILEMATRIX, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "AsShotPreProfileMatrix"), 
            new TiffFieldInfo(TiffTag.CURRENTICCPROFILE, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, false, true, "CurrentICCProfile"), 
            new TiffFieldInfo(TiffTag.CURRENTPREPROFILEMATRIX, -1, -1, TiffType.SRATIONAL, FieldBit.Custom, false, true, "CurrentPreProfileMatrix"),
            /* end DNG tags */
        };

        static TiffFieldInfo[] exifFieldInfo = 
        {
            new TiffFieldInfo(TiffTag.EXIF_EXPOSURETIME, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "ExposureTime"), 
            new TiffFieldInfo(TiffTag.EXIF_FNUMBER, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "FNumber"), 
            new TiffFieldInfo(TiffTag.EXIF_EXPOSUREPROGRAM, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "ExposureProgram"), 
            new TiffFieldInfo(TiffTag.EXIF_SPECTRALSENSITIVITY, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "SpectralSensitivity"), 
            new TiffFieldInfo(TiffTag.EXIF_ISOSPEEDRATINGS, -1, -1, TiffType.SHORT, FieldBit.Custom, true, true, "ISOSpeedRatings"), 
            new TiffFieldInfo(TiffTag.EXIF_OECF, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "OptoelectricConversionFactor"), 
            new TiffFieldInfo(TiffTag.EXIF_EXIFVERSION, 4, 4, TiffType.UNDEFINED, FieldBit.Custom, true, false, "ExifVersion"), 
            new TiffFieldInfo(TiffTag.EXIF_DATETIMEORIGINAL, 20, 20, TiffType.ASCII, FieldBit.Custom, true, false, "DateTimeOriginal"), 
            new TiffFieldInfo(TiffTag.EXIF_DATETIMEDIGITIZED, 20, 20, TiffType.ASCII, FieldBit.Custom, true, false, "DateTimeDigitized"), 
            new TiffFieldInfo(TiffTag.EXIF_COMPONENTSCONFIGURATION, 4, 4, TiffType.UNDEFINED, FieldBit.Custom, true, false, "ComponentsConfiguration"), 
            new TiffFieldInfo(TiffTag.EXIF_COMPRESSEDBITSPERPIXEL, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "CompressedBitsPerPixel"), 
            new TiffFieldInfo(TiffTag.EXIF_SHUTTERSPEEDVALUE, 1, 1, TiffType.SRATIONAL, FieldBit.Custom, true, false, "ShutterSpeedValue"), 
            new TiffFieldInfo(TiffTag.EXIF_APERTUREVALUE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "ApertureValue"), 
            new TiffFieldInfo(TiffTag.EXIF_BRIGHTNESSVALUE, 1, 1, TiffType.SRATIONAL, FieldBit.Custom, true, false, "BrightnessValue"), 
            new TiffFieldInfo(TiffTag.EXIF_EXPOSUREBIASVALUE, 1, 1, TiffType.SRATIONAL, FieldBit.Custom, true, false, "ExposureBiasValue"), 
            new TiffFieldInfo(TiffTag.EXIF_MAXAPERTUREVALUE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "MaxApertureValue"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBJECTDISTANCE, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "SubjectDistance"), 
            new TiffFieldInfo(TiffTag.EXIF_METERINGMODE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "MeteringMode"), 
            new TiffFieldInfo(TiffTag.EXIF_LIGHTSOURCE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "LightSource"), 
            new TiffFieldInfo(TiffTag.EXIF_FLASH, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "Flash"), 
            new TiffFieldInfo(TiffTag.EXIF_FOCALLENGTH, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "FocalLength"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBJECTAREA, -1, -1, TiffType.SHORT, FieldBit.Custom, true, true, "SubjectArea"), 
            new TiffFieldInfo(TiffTag.EXIF_MAKERNOTE, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "MakerNote"), 
            new TiffFieldInfo(TiffTag.EXIF_USERCOMMENT, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "UserComment"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBSECTIME, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "SubSecTime"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBSECTIMEORIGINAL, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "SubSecTimeOriginal"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBSECTIMEDIGITIZED, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "SubSecTimeDigitized"), 
            new TiffFieldInfo(TiffTag.EXIF_FLASHPIXVERSION, 4, 4, TiffType.UNDEFINED, FieldBit.Custom, true, false, "FlashpixVersion"), 
            new TiffFieldInfo(TiffTag.EXIF_COLORSPACE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "ColorSpace"),
            new TiffFieldInfo(TiffTag.EXIF_PIXELXDIMENSION, 1, 1, TiffType.LONG, FieldBit.Custom, true, false, "PixelXDimension"), 
            new TiffFieldInfo(TiffTag.EXIF_PIXELXDIMENSION, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "PixelXDimension"), 
            new TiffFieldInfo(TiffTag.EXIF_PIXELYDIMENSION, 1, 1, TiffType.LONG, FieldBit.Custom, true, false, "PixelYDimension"), 
            new TiffFieldInfo(TiffTag.EXIF_PIXELYDIMENSION, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "PixelYDimension"), 
            new TiffFieldInfo(TiffTag.EXIF_RELATEDSOUNDFILE, 13, 13, TiffType.ASCII, FieldBit.Custom, true, false, "RelatedSoundFile"), 
            new TiffFieldInfo(TiffTag.EXIF_FLASHENERGY, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "FlashEnergy"), 
            new TiffFieldInfo(TiffTag.EXIF_SPATIALFREQUENCYRESPONSE, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "SpatialFrequencyResponse"), 
            new TiffFieldInfo(TiffTag.EXIF_FOCALPLANEXRESOLUTION, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "FocalPlaneXResolution"), 
            new TiffFieldInfo(TiffTag.EXIF_FOCALPLANEYRESOLUTION, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "FocalPlaneYResolution"), 
            new TiffFieldInfo(TiffTag.EXIF_FOCALPLANERESOLUTIONUNIT, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "FocalPlaneResolutionUnit"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBJECTLOCATION, 2, 2, TiffType.SHORT, FieldBit.Custom, true, false, "SubjectLocation"), 
            new TiffFieldInfo(TiffTag.EXIF_EXPOSUREINDEX, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "ExposureIndex"), 
            new TiffFieldInfo(TiffTag.EXIF_SENSINGMETHOD, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "SensingMethod"), 
            new TiffFieldInfo(TiffTag.EXIF_FILESOURCE, 1, 1, TiffType.UNDEFINED, FieldBit.Custom, true, false, "FileSource"), 
            new TiffFieldInfo(TiffTag.EXIF_SCENETYPE, 1, 1, TiffType.UNDEFINED, FieldBit.Custom, true, false, "SceneType"), 
            new TiffFieldInfo(TiffTag.EXIF_CFAPATTERN, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "CFAPattern"), 
            new TiffFieldInfo(TiffTag.EXIF_CUSTOMRENDERED, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "CustomRendered"), 
            new TiffFieldInfo(TiffTag.EXIF_EXPOSUREMODE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "ExposureMode"), 
            new TiffFieldInfo(TiffTag.EXIF_WHITEBALANCE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "WhiteBalance"), 
            new TiffFieldInfo(TiffTag.EXIF_DIGITALZOOMRATIO, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "DigitalZoomRatio"), 
            new TiffFieldInfo(TiffTag.EXIF_FOCALLENGTHIN35MMFILM, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "FocalLengthIn35mmFilm"), 
            new TiffFieldInfo(TiffTag.EXIF_SCENECAPTURETYPE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "SceneCaptureType"), 
            new TiffFieldInfo(TiffTag.EXIF_GAINCONTROL, 1, 1, TiffType.RATIONAL, FieldBit.Custom, true, false, "GainControl"), 
            new TiffFieldInfo(TiffTag.EXIF_CONTRAST, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "Contrast"), 
            new TiffFieldInfo(TiffTag.EXIF_SATURATION, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "Saturation"), 
            new TiffFieldInfo(TiffTag.EXIF_SHARPNESS, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "Sharpness"), 
            new TiffFieldInfo(TiffTag.EXIF_DEVICESETTINGDESCRIPTION, -1, -1, TiffType.UNDEFINED, FieldBit.Custom, true, true, "DeviceSettingDescription"), 
            new TiffFieldInfo(TiffTag.EXIF_SUBJECTDISTANCERANGE, 1, 1, TiffType.SHORT, FieldBit.Custom, true, false, "SubjectDistanceRange"), 
            new TiffFieldInfo(TiffTag.EXIF_IMAGEUNIQUEID, 33, 33, TiffType.ASCII, FieldBit.Custom, true, false, "ImageUniqueID")
        };

        private static TiffFieldInfo[] getFieldInfo(out int size)
        {
            size = tiffFieldInfo.Length;
            return tiffFieldInfo;
        }

        private static TiffFieldInfo[] getExifFieldInfo(out int size)
        {
            size = exifFieldInfo.Length;
            return exifFieldInfo;
        }

        private void setupFieldInfo(TiffFieldInfo[] info, int n)
        {
            m_nfields = 0;
            MergeFieldInfo(info, n);
        }

        /*
        * Return nearest TiffDataType to the sample type of an image.
        */
        private TiffType sampleToTagType()
        {
            int bps = howMany8(m_dir.td_bitspersample);

            switch (m_dir.td_sampleformat)
            {
                case SampleFormat.IEEEFP:
                    return (bps == 4 ? TiffType.FLOAT : TiffType.DOUBLE);
                case SampleFormat.INT:
                    return (bps <= 1 ? TiffType.SBYTE : bps <= 2 ? TiffType.SSHORT : TiffType.SLONG);
                case SampleFormat.UINT:
                    return (bps <= 1 ? TiffType.BYTE : bps <= 2 ? TiffType.SHORT : TiffType.LONG);
                case SampleFormat.VOID:
                    return TiffType.UNDEFINED;
            }
            
            return TiffType.UNDEFINED;
        }

        private static TiffFieldInfo createAnonFieldInfo(TiffTag tag, TiffType field_type)
        {
            TiffFieldInfo fld = new TiffFieldInfo(tag, TiffFieldInfo.Variable2,
                TiffFieldInfo.Variable2, field_type, FieldBit.Custom, true, true, null);

            // note that this name is a special sign to Close() and
            // setupFieldInfo() to free the field
            fld.Name = string.Format(CultureInfo.InvariantCulture, "Tag {0}", tag);
            return fld;
        }
        
        /*
        * Return size of TiffDataType in bytes.
        *
        * XXX: We need a separate function to determine the space needed
        * to store the value. For TiffType.RATIONAL values DataWidth()
        * returns 8, but we use 4-byte float to represent rationals.
        */
        internal static int dataSize(TiffType type)
        {
            switch (type)
            {
                case TiffType.BYTE:
                case TiffType.SBYTE:
                case TiffType.ASCII:
                case TiffType.UNDEFINED:
                    return 1;

                case TiffType.SHORT:
                case TiffType.SSHORT:
                    return 2;

                case TiffType.LONG:
                case TiffType.SLONG:
                case TiffType.FLOAT:
                case TiffType.IFD:
                case TiffType.RATIONAL:
                case TiffType.SRATIONAL:
                    return 4;

                case TiffType.DOUBLE:
                    return 8;

                default:
                    return 0;
            }
        }
    }
}
