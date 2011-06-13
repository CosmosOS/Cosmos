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

using System.Diagnostics;
using System.IO;

namespace BitMiracle.LibTiff.Classic.Internal
{
    class CCITTCodecTagMethods : TiffTagMethods
    {
        public override bool SetField(Tiff tif, TiffTag tag, FieldValue[] ap)
        {
            CCITTCodec sp = tif.m_currentCodec as CCITTCodec;
            Debug.Assert(sp != null);

            switch (tag)
            {
                case TiffTag.FAXMODE:
                    sp.m_mode = (FaxMode)ap[0].ToShort();
                    return true; /* NB: pseudo tag */
                case TiffTag.FAXFILLFUNC:
                    sp.fill = ap[0].Value as Tiff.FaxFillFunc;
                    return true; /* NB: pseudo tag */
                case TiffTag.GROUP3OPTIONS:
                    /* XXX: avoid reading options if compression mismatches. */
                    if (tif.m_dir.td_compression == Compression.CCITTFAX3)
                        sp.m_groupoptions = (Group3Opt)ap[0].ToShort();
                    break;
                case TiffTag.GROUP4OPTIONS:
                    /* XXX: avoid reading options if compression mismatches. */
                    if (tif.m_dir.td_compression == Compression.CCITTFAX4)
                        sp.m_groupoptions = (Group3Opt)ap[0].ToShort();
                    break;
                case TiffTag.BADFAXLINES:
                    sp.m_badfaxlines = ap[0].ToInt();
                    break;
                case TiffTag.CLEANFAXDATA:
                    sp.m_cleanfaxdata = (CleanFaxData)ap[0].ToByte();
                    break;
                case TiffTag.CONSECUTIVEBADFAXLINES:
                    sp.m_badfaxrun = ap[0].ToInt();
                    break;
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
            CCITTCodec sp = tif.m_currentCodec as CCITTCodec;
            Debug.Assert(sp != null);

            FieldValue[] result = new FieldValue[1];

            switch (tag)
            {
                case TiffTag.FAXMODE:
                    result[0].Set(sp.m_mode);
                    break;
                case TiffTag.FAXFILLFUNC:
                    result[0].Set(sp.fill);
                    break;
                case TiffTag.GROUP3OPTIONS:
                case TiffTag.GROUP4OPTIONS:
                    result[0].Set(sp.m_groupoptions);
                    break;
                case TiffTag.BADFAXLINES:
                    result[0].Set(sp.m_badfaxlines);
                    break;
                case TiffTag.CLEANFAXDATA:
                    result[0].Set(sp.m_cleanfaxdata);
                    break;
                case TiffTag.CONSECUTIVEBADFAXLINES:
                    result[0].Set(sp.m_badfaxrun);
                    break;
                case TiffTag.FAXRECVPARAMS:
                    result[0].Set(sp.m_recvparams);
                    break;
                case TiffTag.FAXSUBADDRESS:
                    result[0].Set(sp.m_subaddress);
                    break;
                case TiffTag.FAXRECVTIME:
                    result[0].Set(sp.m_recvtime);
                    break;
                case TiffTag.FAXDCS:
                    result[0].Set(sp.m_faxdcs);
                    break;
                default:
                    return base.GetField(tif, tag);
            }

            return result;
        }

        public override void PrintDir(Tiff tif, Stream fd, TiffPrintFlags flags)
        {
            CCITTCodec sp = tif.m_currentCodec as CCITTCodec;
            Debug.Assert(sp != null);

            if (tif.fieldSet(CCITTCodec.FIELD_OPTIONS))
            {
                string sep = " ";
                if (tif.m_dir.td_compression == Compression.CCITTFAX4)
                {
                    Tiff.fprintf(fd, "  Group 4 Options:");
                    if ((sp.m_groupoptions & Group3Opt.UNCOMPRESSED) != 0)
                        Tiff.fprintf(fd, "{0}uncompressed data", sep);
                }
                else
                {
                    Tiff.fprintf(fd, "  Group 3 Options:");
                    if ((sp.m_groupoptions & Group3Opt.ENCODING2D) != 0)
                    {
                        Tiff.fprintf(fd, "{0}2-d encoding", sep);
                        sep = "+";
                    }

                    if ((sp.m_groupoptions & Group3Opt.FILLBITS) != 0)
                    {
                        Tiff.fprintf(fd, "{0}EOL padding", sep);
                        sep = "+";
                    }

                    if ((sp.m_groupoptions & Group3Opt.UNCOMPRESSED) != 0)
                        Tiff.fprintf(fd, "{0}uncompressed data", sep);
                }

                Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", sp.m_groupoptions, sp.m_groupoptions);
            }

            if (tif.fieldSet(CCITTCodec.FIELD_CLEANFAXDATA))
            {
                Tiff.fprintf(fd, "  Fax Data:");
                
                switch (sp.m_cleanfaxdata)
                {
                    case CleanFaxData.CLEAN:
                        Tiff.fprintf(fd, " clean");
                        break;
                    case CleanFaxData.REGENERATED:
                        Tiff.fprintf(fd, " receiver regenerated");
                        break;
                    case CleanFaxData.UNCLEAN:
                        Tiff.fprintf(fd, " uncorrected errors");
                        break;
                }

                Tiff.fprintf(fd, " ({0} = 0x{1:x})\n", sp.m_cleanfaxdata, sp.m_cleanfaxdata);
            }

            if (tif.fieldSet(CCITTCodec.FIELD_BADFAXLINES))
                Tiff.fprintf(fd, "  Bad Fax Lines: {0}\n", sp.m_badfaxlines);
            
            if (tif.fieldSet(CCITTCodec.FIELD_BADFAXRUN))
                Tiff.fprintf(fd, "  Consecutive Bad Fax Lines: {0}\n", sp.m_badfaxrun);
            
            if (tif.fieldSet(CCITTCodec.FIELD_RECVPARAMS))
                Tiff.fprintf(fd, "  Fax Receive Parameters: {0,8:x}\n", sp.m_recvparams);
            
            if (tif.fieldSet(CCITTCodec.FIELD_SUBADDRESS))
                Tiff.fprintf(fd, "  Fax SubAddress: {0}\n", sp.m_subaddress);
            
            if (tif.fieldSet(CCITTCodec.FIELD_RECVTIME))
                Tiff.fprintf(fd, "  Fax Receive Time: {0} secs\n", sp.m_recvtime);
            
            if (tif.fieldSet(CCITTCodec.FIELD_FAXDCS))
                Tiff.fprintf(fd, "  Fax DCS: {0}\n", sp.m_faxdcs);
        }
    }
}
