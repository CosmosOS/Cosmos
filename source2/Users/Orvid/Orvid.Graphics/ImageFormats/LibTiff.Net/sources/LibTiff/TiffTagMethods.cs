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

using BitMiracle.LibTiff.Classic.Internal;

namespace BitMiracle.LibTiff.Classic
{
    /// <summary>
    /// Tiff tag methods.
    /// </summary>
#if EXPOSE_LIBTIFF
    public
#endif
    class TiffTagMethods
    {
        //
        // These are used in the backwards compatibility code...
        // 

        /// <summary>
        /// untyped data
        /// </summary>
        private const short DATATYPE_VOID = 0;

        /// <summary>
        /// signed integer data
        /// </summary>
        private const short DATATYPE_INT = 1;

        /// <summary>
        /// unsigned integer data
        /// </summary>
        private const short DATATYPE_UINT = 2;

        /// <summary>
        /// IEEE floating point data
        /// </summary>
        private const short DATATYPE_IEEEFP = 3;

        /// <summary>
        /// Sets the value(s) of a tag in a TIFF file/stream open for writing.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class.</param>
        /// <param name="tag">The tag.</param>
        /// <param name="value">The tag value(s).</param>
        /// <returns>
        /// <c>true</c> if tag value(s) were set successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <seealso cref="Tiff.SetField"/>
        public virtual bool SetField(Tiff tif, TiffTag tag, FieldValue[] value)
        {
            const string module = "vsetfield";

            TiffDirectory td = tif.m_dir;
            bool status = true;
            int v32 = 0;
            int v = 0;

            bool end = false;
            bool badvalue = false;
            bool badvalue32 = false;

            switch (tag)
            {
                case TiffTag.SUBFILETYPE:
                    td.td_subfiletype = (FileType)value[0].ToByte();
                    break;
                case TiffTag.IMAGEWIDTH:
                    td.td_imagewidth = value[0].ToInt();
                    break;
                case TiffTag.IMAGELENGTH:
                    td.td_imagelength = value[0].ToInt();
                    break;
                case TiffTag.BITSPERSAMPLE:
                    td.td_bitspersample = value[0].ToShort();
                    // If the data require post-decoding processing to byte-swap samples, set it
                    // up here. Note that since tags are required to be ordered, compression code
                    // can override this behavior in the setup method if it wants to roll the post
                    // decoding work in with its normal work.
                    if ((tif.m_flags & TiffFlags.SWAB) == TiffFlags.SWAB)
                    {
                        if (td.td_bitspersample == 16)
                            tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab16Bit;
                        else if (td.td_bitspersample == 24)
                            tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab24Bit;
                        else if (td.td_bitspersample == 32)
                            tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab32Bit;
                        else if (td.td_bitspersample == 64)
                            tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab64Bit;
                        else if (td.td_bitspersample == 128)
                        {
                            // two 64's
                            tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab64Bit;
                        }
                    }
                    break;
                case TiffTag.COMPRESSION:
                    v = value[0].ToInt() & 0xffff;
                    Compression comp = (Compression)v;
                    // If we're changing the compression scheme, then notify the previous module
                    // so that it can cleanup any state it's setup.
                    if (tif.fieldSet(FieldBit.Compression))
                    {
                        if (td.td_compression == comp)
                            break;

                        tif.m_currentCodec.Cleanup();
                        tif.m_flags &= ~TiffFlags.CODERSETUP;
                    }
                    // Setup new compression scheme.
                    status = tif.setCompressionScheme(comp);
                    if (status)
                        td.td_compression = comp;
                    else
                        status = false;
                    break;

                case TiffTag.PHOTOMETRIC:
                    td.td_photometric = (Photometric)value[0].ToInt();
                    break;
                case TiffTag.THRESHHOLDING:
                    td.td_threshholding = (Threshold)value[0].ToByte();
                    break;
                case TiffTag.FILLORDER:
                    v = value[0].ToInt();
                    FillOrder fo = (FillOrder)v;
                    if (fo != FillOrder.LSB2MSB && fo != FillOrder.MSB2LSB)
                    {
                        badvalue = true;
                        break;
                    }

                    td.td_fillorder = fo;
                    break;
                case TiffTag.ORIENTATION:
                    v = value[0].ToInt();
                    Orientation or = (Orientation)v;
                    if (or < Orientation.TOPLEFT || Orientation.LEFTBOT < or)
                    {
                        badvalue = true;
                        break;
                    }
                    else
                        td.td_orientation = or;
                    break;
                case TiffTag.SAMPLESPERPIXEL:
                    // XXX should cross check - e.g. if pallette, then 1
                    v = value[0].ToInt();
                    if (v == 0)
                    {
                        badvalue = true;
                        break;
                    }

                    td.td_samplesperpixel = (short)v;
                    break;
                case TiffTag.ROWSPERSTRIP:
                    v32 = value[0].ToInt();
                    if (v32 == 0)
                    {
                        badvalue32 = true;
                        break;
                    }

                    td.td_rowsperstrip = v32;
                    if (!tif.fieldSet(FieldBit.TileDimensions))
                    {
                        td.td_tilelength = v32;
                        td.td_tilewidth = td.td_imagewidth;
                    }
                    break;
                case TiffTag.MINSAMPLEVALUE:
                    td.td_minsamplevalue = value[0].ToShort();
                    break;
                case TiffTag.MAXSAMPLEVALUE:
                    td.td_maxsamplevalue = value[0].ToShort();
                    break;
                case TiffTag.SMINSAMPLEVALUE:
                    td.td_sminsamplevalue = value[0].ToDouble();
                    break;
                case TiffTag.SMAXSAMPLEVALUE:
                    td.td_smaxsamplevalue = value[0].ToDouble();
                    break;
                case TiffTag.XRESOLUTION:
                    td.td_xresolution = value[0].ToFloat();
                    break;
                case TiffTag.YRESOLUTION:
                    td.td_yresolution = value[0].ToFloat();
                    break;
                case TiffTag.PLANARCONFIG:
                    v = value[0].ToInt();
                    PlanarConfig pc = (PlanarConfig)v;
                    if (pc != PlanarConfig.CONTIG && pc != PlanarConfig.SEPARATE)
                    {
                        badvalue = true;
                        break;
                    }
                    td.td_planarconfig = pc;
                    break;
                case TiffTag.XPOSITION:
                    td.td_xposition = value[0].ToFloat();
                    break;
                case TiffTag.YPOSITION:
                    td.td_yposition = value[0].ToFloat();
                    break;
                case TiffTag.RESOLUTIONUNIT:
                    v = value[0].ToInt();
                    ResUnit ru = (ResUnit)v;
                    if (ru < ResUnit.NONE || ResUnit.CENTIMETER < ru)
                    {
                        badvalue = true;
                        break;
                    }

                    td.td_resolutionunit = ru;
                    break;
                case TiffTag.PAGENUMBER:
                    td.td_pagenumber[0] = value[0].ToShort();
                    td.td_pagenumber[1] = value[1].ToShort();
                    break;
                case TiffTag.HALFTONEHINTS:
                    td.td_halftonehints[0] = value[0].ToShort();
                    td.td_halftonehints[1] = value[1].ToShort();
                    break;
                case TiffTag.COLORMAP:
                    v32 = 1 << td.td_bitspersample;
                    Tiff.setShortArray(out td.td_colormap[0], value[0].ToShortArray(), v32);
                    Tiff.setShortArray(out td.td_colormap[1], value[1].ToShortArray(), v32);
                    Tiff.setShortArray(out td.td_colormap[2], value[2].ToShortArray(), v32);
                    break;
                case TiffTag.EXTRASAMPLES:
                    if (!setExtraSamples(td, ref v, value))
                    {
                        badvalue = true;
                        break;
                    }

                    break;
                case TiffTag.MATTEING:
                    if (value[0].ToShort() != 0)
                        td.td_extrasamples = 1;
                    else
                        td.td_extrasamples = 0;

                    if (td.td_extrasamples != 0)
                    {
                        td.td_sampleinfo = new ExtraSample[1];
                        td.td_sampleinfo[0] = ExtraSample.ASSOCALPHA;
                    }
                    break;
                case TiffTag.TILEWIDTH:
                    v32 = value[0].ToInt();
                    if ((v32 % 16) != 0)
                    {
                        if (tif.m_mode != Tiff.O_RDONLY)
                        {
                            badvalue32 = true;
                            break;
                        }

                        Tiff.WarningExt(tif, tif.m_clientdata, tif.m_name,
                            "Nonstandard tile width {0}, convert file", v32);
                    }
                    td.td_tilewidth = v32;
                    tif.m_flags |= TiffFlags.ISTILED;
                    break;
                case TiffTag.TILELENGTH:
                    v32 = value[0].ToInt();
                    if ((v32 % 16) != 0)
                    {
                        if (tif.m_mode != Tiff.O_RDONLY)
                        {
                            badvalue32 = true;
                            break;
                        }

                        Tiff.WarningExt(tif, tif.m_clientdata, tif.m_name,
                            "Nonstandard tile length {0}, convert file", v32);
                    }
                    td.td_tilelength = v32;
                    tif.m_flags |= TiffFlags.ISTILED;
                    break;
                case TiffTag.TILEDEPTH:
                    v32 = value[0].ToInt();
                    if (v32 == 0)
                    {
                        badvalue32 = true;
                        break;
                    }

                    td.td_tiledepth = v32;
                    break;
                case TiffTag.DATATYPE:
                    v = value[0].ToInt();
                    SampleFormat sf = SampleFormat.VOID;
                    switch (v)
                    {
                        case DATATYPE_VOID:
                            sf = SampleFormat.VOID;
                            break;
                        case DATATYPE_INT:
                            sf = SampleFormat.INT;
                            break;
                        case DATATYPE_UINT:
                            sf = SampleFormat.UINT;
                            break;
                        case DATATYPE_IEEEFP:
                            sf = SampleFormat.IEEEFP;
                            break;
                        default:
                            badvalue = true;
                            break;
                    }

                    if (!badvalue)
                        td.td_sampleformat = sf;

                    break;
                case TiffTag.SAMPLEFORMAT:
                    v = value[0].ToInt();
                    sf = (SampleFormat)v;
                    if (sf < SampleFormat.UINT || SampleFormat.COMPLEXIEEEFP < sf)
                    {
                        badvalue = true;
                        break;
                    }

                    td.td_sampleformat = sf;

                    // Try to fix up the SWAB function for complex data.
                    if (td.td_sampleformat == SampleFormat.COMPLEXINT &&
                        td.td_bitspersample == 32 && tif.m_postDecodeMethod == Tiff.PostDecodeMethodType.pdmSwab32Bit)
                    {
                        tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab16Bit;
                    }
                    else if ((td.td_sampleformat == SampleFormat.COMPLEXINT ||
                        td.td_sampleformat == SampleFormat.COMPLEXIEEEFP) &&
                        td.td_bitspersample == 64 && tif.m_postDecodeMethod == Tiff.PostDecodeMethodType.pdmSwab64Bit)
                    {
                        tif.m_postDecodeMethod = Tiff.PostDecodeMethodType.pdmSwab32Bit;
                    }
                    break;
                case TiffTag.IMAGEDEPTH:
                    td.td_imagedepth = value[0].ToInt();
                    break;
                case TiffTag.SUBIFD:
                    if ((tif.m_flags & TiffFlags.INSUBIFD) != TiffFlags.INSUBIFD)
                    {
                        td.td_nsubifd = value[0].ToShort();
                        Tiff.setLongArray(out td.td_subifd, value[1].ToIntArray(), td.td_nsubifd);
                    }
                    else
                    {
                        Tiff.ErrorExt(tif, tif.m_clientdata, module,
                            "{0}: Sorry, cannot nest SubIFDs", tif.m_name);
                        status = false;
                    }
                    break;
                case TiffTag.YCBCRPOSITIONING:
                    td.td_ycbcrpositioning = (YCbCrPosition)value[0].ToByte();
                    break;
                case TiffTag.YCBCRSUBSAMPLING:
                    td.td_ycbcrsubsampling[0] = value[0].ToShort();
                    td.td_ycbcrsubsampling[1] = value[1].ToShort();
                    break;
                case TiffTag.TRANSFERFUNCTION:
                    v = ((td.td_samplesperpixel - td.td_extrasamples) > 1 ? 3 : 1);
                    for (int i = 0; i < v; i++)
                    {
                        Tiff.setShortArray(out td.td_transferfunction[i], value[0].ToShortArray(), 1 << td.td_bitspersample);
                    }
                    break;
                case TiffTag.REFERENCEBLACKWHITE:
                    // XXX should check for null range
                    Tiff.setFloatArray(out td.td_refblackwhite, value[0].ToFloatArray(), 6);
                    break;
                case TiffTag.INKNAMES:
                    v = value[0].ToInt();
                    string s = value[1].ToString();
                    v = checkInkNamesString(tif, v, s);
                    status = v > 0;
                    if (v > 0)
                    {
                        setNString(out td.td_inknames, s, v);
                        td.td_inknameslen = v;
                    }
                    break;
                default:
                    // This can happen if multiple images are open with
                    // different codecs which have private tags. The global tag
                    // information table may then have tags that are valid for
                    // one file but not the other. If the client tries to set a
                    // tag that is not valid for the image's codec then we'll
                    // arrive here. This happens, for example, when tiffcp is
                    // used to convert between compression schemes and
                    // codec-specific tags are blindly copied.
                    TiffFieldInfo fip = tif.FindFieldInfo(tag, TiffType.ANY);
                    if (fip == null || fip.Bit != FieldBit.Custom)
                    {
                        Tiff.ErrorExt(tif, tif.m_clientdata, module,
                            "{0}: Invalid {1}tag \"{2}\" (not supported by codec)",
                            tif.m_name, Tiff.isPseudoTag(tag) ? "pseudo-" : "",
                            fip != null ? fip.Name : "Unknown");
                        status = false;
                        break;
                    }

                    // Find the existing entry for this custom value.
                    int tvIndex = -1;
                    for (int iCustom = 0; iCustom < td.td_customValueCount; iCustom++)
                    {
                        if (td.td_customValues[iCustom].info.Tag == tag)
                        {
                            td.td_customValues[iCustom].value = null;
                            break;
                        }
                    }

                    // Grow the custom list if the entry was not found.
                    if (tvIndex == -1)
                    {
                        td.td_customValueCount++;
                        TiffTagValue[] new_customValues = Tiff.Realloc(
                            td.td_customValues, td.td_customValueCount - 1, td.td_customValueCount);
                        td.td_customValues = new_customValues;

                        tvIndex = td.td_customValueCount - 1;
                        td.td_customValues[tvIndex].info = fip;
                        td.td_customValues[tvIndex].value = null;
                        td.td_customValues[tvIndex].count = 0;
                    }

                    // Set custom value ... save a copy of the custom tag value.
                    int tv_size = Tiff.dataSize(fip.Type);
                    if (tv_size == 0)
                    {
                        status = false;
                        Tiff.ErrorExt(tif, tif.m_clientdata, module,
                            "{0}: Bad field type {1} for \"{2}\"",
                            tif.m_name, fip.Type, fip.Name);
                        end = true;
                        break;
                    }

                    int paramIndex = 0;
                    if (fip.PassCount)
                    {
                        if (fip.WriteCount == TiffFieldInfo.Variable2)
                            td.td_customValues[tvIndex].count = value[paramIndex++].ToInt();
                        else
                            td.td_customValues[tvIndex].count = value[paramIndex++].ToInt();
                    }
                    else if (fip.WriteCount == TiffFieldInfo.Variable ||
                        fip.WriteCount == TiffFieldInfo.Variable2)
                    {
                        td.td_customValues[tvIndex].count = 1;
                    }
                    else if (fip.WriteCount == TiffFieldInfo.Spp)
                    {
                        td.td_customValues[tvIndex].count = td.td_samplesperpixel;
                    }
                    else
                    {
                        td.td_customValues[tvIndex].count = fip.WriteCount;
                    }

                    if (fip.Type == TiffType.ASCII)
                    {
                        string ascii;
                        Tiff.setString(out ascii, value[paramIndex++].ToString());
                        td.td_customValues[tvIndex].value = Tiff.Latin1Encoding.GetBytes(ascii);
                    }
                    else
                    {
                        td.td_customValues[tvIndex].value = new byte[tv_size * td.td_customValues[tvIndex].count];
                        if ((fip.PassCount ||
                            fip.WriteCount == TiffFieldInfo.Variable ||
                            fip.WriteCount == TiffFieldInfo.Variable2 ||
                            fip.WriteCount == TiffFieldInfo.Spp ||
                            td.td_customValues[tvIndex].count > 1) &&
                            fip.Tag != TiffTag.PAGENUMBER &&
                            fip.Tag != TiffTag.HALFTONEHINTS &&
                            fip.Tag != TiffTag.YCBCRSUBSAMPLING &&
                            fip.Tag != TiffTag.DOTRANGE)
                        {
                            byte[] apBytes = value[paramIndex++].GetBytes();
                            //Buffer.BlockCopy(apBytes, 0, td.td_customValues[tvIndex].value, 0, apBytes.Length);
                            Buffer.BlockCopy(apBytes, 0, td.td_customValues[tvIndex].value, 0, td.td_customValues[tvIndex].value.Length);
                        }
                        else
                        {
                            // XXX: The following loop required to handle
                            // PAGENUMBER, HALFTONEHINTS,
                            // YCBCRSUBSAMPLING and DOTRANGE tags.
                            // These tags are actually arrays and should be
                            // passed as arrays to SetField() function, but
                            // actually passed as a list of separate values.
                            // This behavior must be changed in the future!

                            // Upd: This loop also processes some EXIF tags with
                            // UNDEFINED type (like EXIF_FILESOURCE or EXIF_SCENETYPE)
                            // In this case input value is string-based, so
                            // in TiffType.UNDEFINED case we use FieldValue.GetBytes()[0]
                            // construction instead of direct call of FieldValue.ToByte() method.
                            byte[] val = td.td_customValues[tvIndex].value;
                            int valPos = 0;
                            for (int i = 0; i < td.td_customValues[tvIndex].count; i++, valPos += tv_size)
                            {
                                switch (fip.Type)
                                {
                                    case TiffType.BYTE:
                                    case TiffType.UNDEFINED:
                                        val[valPos] = value[paramIndex + i].GetBytes()[0];
                                        break;
                                    case TiffType.SBYTE:
                                        val[valPos] = value[paramIndex + i].ToByte();
                                        break;
                                    case TiffType.SHORT:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToShort()), 0, val, valPos, tv_size);
                                        break;
                                    case TiffType.SSHORT:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToShort()), 0, val, valPos, tv_size);
                                        break;
                                    case TiffType.LONG:
                                    case TiffType.IFD:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToInt()), 0, val, valPos, tv_size);
                                        break;
                                    case TiffType.SLONG:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToInt()), 0, val, valPos, tv_size);
                                        break;
                                    case TiffType.RATIONAL:
                                    case TiffType.SRATIONAL:
                                    case TiffType.FLOAT:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToFloat()), 0, val, valPos, tv_size);
                                        break;
                                    case TiffType.DOUBLE:
                                        Buffer.BlockCopy(BitConverter.GetBytes(value[paramIndex + i].ToDouble()), 0, val, valPos, tv_size);
                                        break;
                                    default:
                                        Array.Clear(val, valPos, tv_size);
                                        status = false;
                                        break;
                                }
                            }
                        }
                    }
                    break;
            }

            if (!end && !badvalue && !badvalue32)
            {
                if (status)
                {
                    tif.setFieldBit(tif.FieldWithTag(tag).Bit);
                    tif.m_flags |= TiffFlags.DIRTYDIRECT;
                }
            }

            if (badvalue)
            {
                Tiff.ErrorExt(tif, tif.m_clientdata, module,
                    "{0}: Bad value {1} for \"{2}\" tag",
                    tif.m_name, v, tif.FieldWithTag(tag).Name);
                return false;
            }

            if (badvalue32)
            {
                Tiff.ErrorExt(tif, tif.m_clientdata, module,
                    "{0}: Bad value {1} for \"{2}\" tag",
                    tif.m_name, v32, tif.FieldWithTag(tag).Name);
                return false;
            }

            return status;
        }

        /// <summary>
        /// Gets the value(s) of a tag in an open TIFF file.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class.</param>
        /// <param name="tag">The tag.</param>
        /// <returns>The value(s) of a tag in an open TIFF file/stream as array of
        /// <see cref="FieldValue"/> objects or <c>null</c> if there is no such tag set.</returns>
        /// <seealso cref="Tiff.GetField"/>
        public virtual FieldValue[] GetField(Tiff tif, TiffTag tag)
        {
            TiffDirectory td = tif.m_dir;
            FieldValue[] result = null;

            switch (tag)
            {
                case TiffTag.SUBFILETYPE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_subfiletype);
                    break;
                case TiffTag.IMAGEWIDTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_imagewidth);
                    break;
                case TiffTag.IMAGELENGTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_imagelength);
                    break;
                case TiffTag.BITSPERSAMPLE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_bitspersample);
                    break;
                case TiffTag.COMPRESSION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_compression);
                    break;
                case TiffTag.PHOTOMETRIC:
                    result = new FieldValue[1];
                    result[0].Set(td.td_photometric);
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
                case TiffTag.SMINSAMPLEVALUE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_sminsamplevalue);
                    break;
                case TiffTag.SMAXSAMPLEVALUE:
                    result = new FieldValue[1];
                    result[0].Set(td.td_smaxsamplevalue);
                    break;
                case TiffTag.XRESOLUTION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_xresolution);
                    break;
                case TiffTag.YRESOLUTION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_yresolution);
                    break;
                case TiffTag.PLANARCONFIG:
                    result = new FieldValue[1];
                    result[0].Set(td.td_planarconfig);
                    break;
                case TiffTag.XPOSITION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_xposition);
                    break;
                case TiffTag.YPOSITION:
                    result = new FieldValue[1];
                    result[0].Set(td.td_yposition);
                    break;
                case TiffTag.RESOLUTIONUNIT:
                    result = new FieldValue[1];
                    result[0].Set(td.td_resolutionunit);
                    break;
                case TiffTag.PAGENUMBER:
                    result = new FieldValue[2];
                    result[0].Set(td.td_pagenumber[0]);
                    result[1].Set(td.td_pagenumber[1]);
                    break;
                case TiffTag.HALFTONEHINTS:
                    result = new FieldValue[2];
                    result[0].Set(td.td_halftonehints[0]);
                    result[1].Set(td.td_halftonehints[1]);
                    break;
                case TiffTag.COLORMAP:
                    result = new FieldValue[3];
                    result[0].Set(td.td_colormap[0]);
                    result[1].Set(td.td_colormap[1]);
                    result[2].Set(td.td_colormap[2]);
                    break;
                case TiffTag.STRIPOFFSETS:
                case TiffTag.TILEOFFSETS:
                    result = new FieldValue[1];
                    result[0].Set(td.td_stripoffset);
                    break;
                case TiffTag.STRIPBYTECOUNTS:
                case TiffTag.TILEBYTECOUNTS:
                    result = new FieldValue[1];
                    result[0].Set(td.td_stripbytecount);
                    break;
                case TiffTag.MATTEING:
                    result = new FieldValue[1];
                    result[0].Set((td.td_extrasamples == 1 && td.td_sampleinfo[0] == ExtraSample.ASSOCALPHA));
                    break;
                case TiffTag.EXTRASAMPLES:
                    result = new FieldValue[2];
                    result[0].Set(td.td_extrasamples);
                    result[1].Set(td.td_sampleinfo);
                    break;
                case TiffTag.TILEWIDTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_tilewidth);
                    break;
                case TiffTag.TILELENGTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_tilelength);
                    break;
                case TiffTag.TILEDEPTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_tiledepth);
                    break;
                case TiffTag.DATATYPE:
                    switch (td.td_sampleformat)
                    {
                        case SampleFormat.UINT:
                            result = new FieldValue[1];
                            result[0].Set(DATATYPE_UINT);
                            break;
                        case SampleFormat.INT:
                            result = new FieldValue[1];
                            result[0].Set(DATATYPE_INT);
                            break;
                        case SampleFormat.IEEEFP:
                            result = new FieldValue[1];
                            result[0].Set(DATATYPE_IEEEFP);
                            break;
                        case SampleFormat.VOID:
                            result = new FieldValue[1];
                            result[0].Set(DATATYPE_VOID);
                            break;
                    }
                    break;
                case TiffTag.SAMPLEFORMAT:
                    result = new FieldValue[1];
                    result[0].Set(td.td_sampleformat);
                    break;
                case TiffTag.IMAGEDEPTH:
                    result = new FieldValue[1];
                    result[0].Set(td.td_imagedepth);
                    break;
                case TiffTag.SUBIFD:
                    result = new FieldValue[2];
                    result[0].Set(td.td_nsubifd);
                    result[1].Set(td.td_subifd);
                    break;
                case TiffTag.YCBCRPOSITIONING:
                    result = new FieldValue[1];
                    result[0].Set(td.td_ycbcrpositioning);
                    break;
                case TiffTag.YCBCRSUBSAMPLING:
                    result = new FieldValue[2];
                    result[0].Set(td.td_ycbcrsubsampling[0]);
                    result[1].Set(td.td_ycbcrsubsampling[1]);
                    break;
                case TiffTag.TRANSFERFUNCTION:
                    result = new FieldValue[3];
                    result[0].Set(td.td_transferfunction[0]);
                    if (td.td_samplesperpixel - td.td_extrasamples > 1)
                    {
                        result[1].Set(td.td_transferfunction[1]);
                        result[2].Set(td.td_transferfunction[2]);
                    }
                    break;
                case TiffTag.REFERENCEBLACKWHITE:
                    if (td.td_refblackwhite != null)
                    {
                        result = new FieldValue[1];
                        result[0].Set(td.td_refblackwhite);
                    }
                    break;
                case TiffTag.INKNAMES:
                    result = new FieldValue[1];
                    result[0].Set(td.td_inknames);
                    break;
                default:
                    // This can happen if multiple images are open with 
                    // different codecs which have private tags. The global tag
                    // information table may then have tags that are valid for
                    // one file but not the other. If the client tries to get a
                    // tag that is not valid for the image's codec then we'll
                    // arrive here.
                    TiffFieldInfo fip = tif.FindFieldInfo(tag, TiffType.ANY);
                    if (fip == null || fip.Bit != FieldBit.Custom)
                    {
                        Tiff.ErrorExt(tif, tif.m_clientdata, "_TIFFVGetField",
                            "{0}: Invalid {1}tag \"{2}\" (not supported by codec)",
                            tif.m_name, Tiff.isPseudoTag(tag) ? "pseudo-" : "",
                            fip != null ? fip.Name : "Unknown");
                        result = null;
                        break;
                    }

                    // Do we have a custom value?
                    result = null;
                    for (int i = 0; i < td.td_customValueCount; i++)
                    {
                        TiffTagValue tv = td.td_customValues[i];
                        if (tv.info.Tag != tag)
                            continue;

                        if (fip.PassCount)
                        {
                            result = new FieldValue[2];

                            if (fip.ReadCount == TiffFieldInfo.Variable2)
                            {
                                result[0].Set(tv.count);
                            }
                            else
                            {
                                // Assume TiffFieldInfo.Variable
                                result[0].Set(tv.count);
                            }
                            
                            result[1].Set(tv.value);
                        }
                        else
                        {
                            if ((fip.Type == TiffType.ASCII ||
                                fip.ReadCount == TiffFieldInfo.Variable ||
                                fip.ReadCount == TiffFieldInfo.Variable2 ||
                                fip.ReadCount == TiffFieldInfo.Spp || 
                                tv.count > 1) && fip.Tag != TiffTag.PAGENUMBER && 
                                fip.Tag != TiffTag.HALFTONEHINTS && 
                                fip.Tag != TiffTag.YCBCRSUBSAMPLING && 
                                fip.Tag != TiffTag.DOTRANGE)
                            {
                                result = new FieldValue[1];
                                byte[] value = tv.value;

                                if (fip.Type == TiffType.ASCII &&
                                    tv.value.Length > 0 &&
                                    tv.value[tv.value.Length - 1] == 0)
                                {
                                    // cut unwanted zero at the end
                                    value = new byte[Math.Max(tv.value.Length - 1, 0)];
                                    Buffer.BlockCopy(tv.value, 0, value, 0, value.Length);
                                }

                                result[0].Set(value);
                            }
                            else
                            {
                                result = new FieldValue[tv.count];
                                byte[] val = tv.value;
                                int valPos = 0;
                                for (int j = 0; j < tv.count; j++, valPos += Tiff.dataSize(tv.info.Type))
                                {
                                    switch (fip.Type)
                                    {
                                        case TiffType.BYTE:
                                        case TiffType.UNDEFINED:
                                        case TiffType.SBYTE:
                                            result[j].Set(val[valPos]);
                                            break;
                                        case TiffType.SHORT:
                                        case TiffType.SSHORT:
                                            result[j].Set(BitConverter.ToInt16(val, valPos));
                                            break;
                                        case TiffType.LONG:
                                        case TiffType.IFD:
                                        case TiffType.SLONG:
                                            result[j].Set(BitConverter.ToInt32(val, valPos));
                                            break;
                                        case TiffType.RATIONAL:
                                        case TiffType.SRATIONAL:
                                        case TiffType.FLOAT:
                                            result[j].Set(BitConverter.ToSingle(val, valPos));
                                            break;
                                        case TiffType.DOUBLE:
                                            result[j].Set(BitConverter.ToDouble(val, valPos));
                                            break;
                                        default:
                                            result = null;
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                    break;
            }

            return result;
        }

        /// <summary>
        /// Prints formatted description of the contents of the current directory to the
        /// specified stream using specified print (formatting) options.
        /// </summary>
        /// <param name="tif">An instance of the <see cref="Tiff"/> class.</param>
        /// <param name="stream">The stream to print to.</param>
        /// <param name="flags">The print (formatting) options.</param>
        public virtual void PrintDir(Tiff tif, Stream stream, TiffPrintFlags flags)
        {
        }

        /// <summary>
        /// Install extra samples information.
        /// </summary>
        private static bool setExtraSamples(TiffDirectory td, ref int v, FieldValue[] ap)
        {
            // XXX: Unassociated alpha data == 999 is a known Corel Draw bug, see below
            const short EXTRASAMPLE_COREL_UNASSALPHA = 999;

            v = ap[0].ToInt();
            if (v > td.td_samplesperpixel)
                return false;

            byte[] va = ap[1].ToByteArray();
            if (v > 0 && va == null)
            {
                // typically missing param
                return false;
            }

            for (int i = 0; i < v; i++)
            {
                if ((ExtraSample)va[i] > ExtraSample.UNASSALPHA)
                {
                    // XXX: Corel Draw is known to produce incorrect 
                    // ExtraSamples tags which must be patched here if we
                    // want to be able to open some of the damaged TIFF files: 
                    if (i < v - 1)
                    {
                        short s = BitConverter.ToInt16(va, i);
                        if (s == EXTRASAMPLE_COREL_UNASSALPHA)
                            va[i] = (byte)ExtraSample.UNASSALPHA;
                    }
                    else
                        return false;
                }
            }

            td.td_extrasamples = (short)v;
            td.td_sampleinfo = new ExtraSample[td.td_extrasamples];
            for (int i = 0; i < td.td_extrasamples; i++)
                td.td_sampleinfo[i] = (ExtraSample)va[i];

            return true;
        }

        private static int checkInkNamesString(Tiff tif, int slen, string s)
        {
            bool failed = false;
            short i = tif.m_dir.td_samplesperpixel;

            if (slen > 0)
            {
                int endPos = slen;
                int pos = 0;

                for (; i > 0; i--)
                {
                    for (; s[pos] != '\0'; pos++)
                    {
                        if (pos >= endPos)
                        {
                            failed = true;
                            break;
                        }
                    }

                    if (failed)
                        break;

                    pos++; // skip \0
                }

                if (!failed)
                    return pos;
            }

            Tiff.ErrorExt(tif, tif.m_clientdata, "TIFFSetField",
                "{0}: Invalid InkNames value; expecting {1} names, found {2}",
                tif.m_name, tif.m_dir.td_samplesperpixel, tif.m_dir.td_samplesperpixel - i);
            return 0;
        }

        private static void setNString(out string cpp, string cp, int n)
        {
            cpp = cp.Substring(0, n);
        }
    }
}
