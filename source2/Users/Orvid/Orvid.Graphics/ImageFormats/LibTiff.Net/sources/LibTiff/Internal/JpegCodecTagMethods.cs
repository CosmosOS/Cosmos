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
using System.Diagnostics;
using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class JpegCodecTagMethods : TiffTagMethods
    {
        public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
        {
            JpegCodec sp = tif.m_currentCodec as JpegCodec;
            Debug.Assert(sp != null);
            
            switch (tag)
            {
                case TiffTag.JPEGTABLES:
                    int v32 = ap[0].ToInt();
                    if (v32 == 0)
                    {
                        // XXX
                        return false;
                    }

                    sp.m_jpegtables = new byte [v32];
                    Buffer.BlockCopy(ap[1].ToByteArray(), 0, sp.m_jpegtables, 0, v32);
                    sp.m_jpegtables_length = v32;
                    tif.setFieldBit(JpegCodec.FIELD_JPEGTABLES);
                    break;

                case TiffTag.JPEGQUALITY:
                    sp.m_jpegquality = ap[0].ToInt();
                    return true; // pseudo tag

                case TiffTag.JPEGCOLORMODE:
                    sp.m_jpegcolormode = (JpegColorMode)ap[0].ToShort();
                    sp.JPEGResetUpsampled();
                    return true; // pseudo tag

                case TiffTag.PHOTOMETRIC:
                    bool ret_value = base.SetField(tif, tag, ap);
                    sp.JPEGResetUpsampled();
                    return ret_value;

                case TiffTag.JPEGTABLESMODE:
                    sp.m_jpegtablesmode = (JpegTablesMode)ap[0].ToShort();
                    return true; // pseudo tag
                
                case TiffTag.YCBCRSUBSAMPLING:
                    // mark the fact that we have a real ycbcrsubsampling!
                    sp.m_ycbcrsampling_fetched = true;
                    // should we be recomputing upsampling info here?
                    return base.SetField(tif, tag, ap);
                
                case TiffTag.FAXRECVPARAMS:
                    sp.m_recvparams = ap[0].ToInt();
                    break;
                
                case TiffTag.FAXSUBADDRESS:
                    Tiff.setString(out sp.m_subaddress, ap[0].ToString());
                    break;
                
                case TiffTag.FAXRECVTIME:
                    sp.m_recvtime = ap[0].ToInt();
                    break;
                
                case TiffTag.FAXDCS:
                    Tiff.setString(out sp.m_faxdcs, ap[0].ToString());
                    break;
                
                default:
                    return base.SetField(tif, tag, ap);
            }

            TiffFieldInfo fip = tif.FieldWithTag(tag);
            if (fip != null)
                tif.setFieldBit(fip.Bit);
            else
                return false;

            tif.m_flags |= TiffFlags.DIRTYDIRECT;
            return true;
        }

        public override FieldValue[] GetField(Tiff tif, TiffTag tag)
        {
            JpegCodec sp = tif.m_currentCodec as JpegCodec;
            Debug.Assert(sp != null);

            FieldValue[] result = null;

            switch (tag)
            {
                case TiffTag.JPEGTABLES:
                    result = new FieldValue[2];
                    result[0].Set(sp.m_jpegtables_length);
                    result[1].Set(sp.m_jpegtables);
                    break;

                case TiffTag.JPEGQUALITY:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_jpegquality);
                    break;

                case TiffTag.JPEGCOLORMODE:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_jpegcolormode);
                    break;

                case TiffTag.JPEGTABLESMODE:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_jpegtablesmode);
                    break;

                case TiffTag.YCBCRSUBSAMPLING:
                    JPEGFixupTestSubsampling(tif);
                    return base.GetField(tif, tag);

                case TiffTag.FAXRECVPARAMS:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_recvparams);
                    break;

                case TiffTag.FAXSUBADDRESS:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_subaddress);
                    break;

                case TiffTag.FAXRECVTIME:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_recvtime);
                    break;

                case TiffTag.FAXDCS:
                    result = new FieldValue[1];
                    result[0].Set(sp.m_faxdcs);
                    break;

                default:
                    return base.GetField(tif, tag);
            }

            return result;
        }

        public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
        {
            JpegCodec sp = tif.m_currentCodec as JpegCodec;
            Debug.Assert(sp != null);
            
            if (tif.fieldSet(JpegCodec.FIELD_JPEGTABLES))
                Tiff.fprintf(fd, "  JPEG Tables: ({0} bytes)\n", sp.m_jpegtables_length);
            
            if (tif.fieldSet(JpegCodec.FIELD_RECVPARAMS))
                Tiff.fprintf(fd, "  Fax Receive Parameters: {0,8:x}\n", sp.m_recvparams);
            
            if (tif.fieldSet(JpegCodec.FIELD_SUBADDRESS))
                Tiff.fprintf(fd, "  Fax SubAddress: {0}\n", sp.m_subaddress);
            
            if (tif.fieldSet(JpegCodec.FIELD_RECVTIME))
                Tiff.fprintf(fd, "  Fax Receive Time: {0} secs\n", sp.m_recvtime);
            
            if (tif.fieldSet(JpegCodec.FIELD_FAXDCS))
                Tiff.fprintf(fd, "  Fax DCS: {0}\n", sp.m_faxdcs);
        }

        /*
        * Some JPEG-in-TIFF produces do not emit the YCBCRSUBSAMPLING values in
        * the TIFF tags, but still use non-default (2,2) values within the jpeg
        * data stream itself.  In order for TIFF applications to work properly
        * - for instance to get the strip buffer size right - it is imperative
        * that the subsampling be available before we start reading the image
        * data normally.  This function will attempt to load the first strip in
        * order to get the sampling values from the jpeg data stream.  Various
        * hacks are various places are done to ensure this function gets called
        * before the td_ycbcrsubsampling values are used from the directory structure,
        * including calling TIFFGetField() for the YCBCRSUBSAMPLING field from 
        * TIFFStripSize(), and the printing code in tif_print.c. 
        *
        * Note that JPEGPreDeocode() will produce a fairly loud warning when the
        * discovered sampling does not match the default sampling (2,2) or whatever
        * was actually in the tiff tags. 
        *
        * Problems:
        *  o This code will cause one whole strip/tile of compressed data to be
        *    loaded just to get the tags right, even if the imagery is never read.
        *    It would be more efficient to just load a bit of the header, and
        *    initialize things from that. 
        *
        * See the bug in bugzilla for details:
        *
        * http://bugzilla.remotesensing.org/show_bug.cgi?id=168
        *
        * Frank Warmerdam, July 2002
        */
        private static void JPEGFixupTestSubsampling(Tiff tif)
        {
            if (Tiff.CHECK_JPEG_YCBCR_SUBSAMPLING)
            {
                JpegCodec sp = tif.m_currentCodec as JpegCodec;
                Debug.Assert(sp != null);

                sp.InitializeLibJPEG(false, false);

                /*
                * Some JPEG-in-TIFF files don't provide the ycbcrsampling tags, 
                * and use a sampling schema other than the default 2,2.  To handle
                * this we actually have to scan the header of a strip or tile of
                * jpeg data to get the sampling.  
                */
                if (!sp.m_common.IsDecompressor || sp.m_ycbcrsampling_fetched ||
                    tif.m_dir.td_photometric != Photometric.YCBCR)
                {
                    return;
                }

                sp.m_ycbcrsampling_fetched = true;
                if (tif.IsTiled())
                {
                    if (!tif.fillTile(0))
                        return;
                }
                else
                {
                    if (!tif.fillStrip(0))
                        return;
                }

                tif.SetField(TiffTag.YCBCRSUBSAMPLING, sp.m_h_sampling, sp.m_v_sampling);

                // We want to clear the loaded strip so the application has time
                // to set JPEGCOLORMODE or other behavior modifiers. This essentially
                // undoes the JPEGPreDecode triggers by FileStrip().
                tif.m_curstrip = -1;
            }
        }
    }
}
