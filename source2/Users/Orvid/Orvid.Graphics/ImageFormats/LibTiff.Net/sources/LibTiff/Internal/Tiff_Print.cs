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
 * Directory Printing Support
 */

using System.IO;

namespace BitMiracle.LibTiff.Classic
{
#if EXPOSE_LIBTIFF
    public
#endif
    partial class Tiff
    {
        private static readonly string[] photoNames = 
        {
            "min-is-white",                         // Photometric.MINISWHITE
            "min-is-black",                         // Photometric.MINISBLACK
            "RGB color",                            // Photometric.RGB
            "palette color (RGB from colormap)",    // Photometric.PALETTE
            "transparency mask",                    // Photometric.MASK
            "separated",                            // Photometric.SEPARATED
            "YCbCr",                                // Photometric.YCBCR
            "7 (0x7)",
            "CIE L*a*b*",                           // Photometric.CIELAB
        };

        private static readonly string[] orientNames = 
        {
            "0 (0x0)",
            "row 0 top, col 0 lhs",                 // Orientation.TOPLEFT
            "row 0 top, col 0 rhs",                 // Orientation.TOPRIGHT
            "row 0 bottom, col 0 rhs",              // Orientation.BOTRIGHT
            "row 0 bottom, col 0 lhs",              // Orientation.BOTLEFT
            "row 0 lhs, col 0 top",                 // Orientation.LEFTTOP
            "row 0 rhs, col 0 top",                 // Orientation.RIGHTTOP
            "row 0 rhs, col 0 bottom",              // Orientation.RIGHTBOT
            "row 0 lhs, col 0 bottom",              // Orientation.LEFTBOT
        };

        private static void printField(Stream fd, TiffFieldInfo fip, int value_count, object raw_data)
        {
            fprintf(fd, "  {0}: ", fip.Name);

            byte[] bytes = raw_data as byte[];
            sbyte[] sbytes = raw_data as sbyte[];
            short[] shorts = raw_data as short[];
            ushort[] ushorts = raw_data as ushort[];
            int[] ints = raw_data as int[];
            uint[] uints = raw_data as uint[];
            float[] floats = raw_data as float[];
            double[] doubles = raw_data as double[];
            string s = raw_data as string;

            for (int j = 0; j < value_count; j++)
            {
                if (fip.Type == TiffType.BYTE || fip.Type == TiffType.SBYTE)
                {
                    if (bytes != null)
                        fprintf(fd, "{0}", bytes[j]);
                    else if (sbytes != null)
                        fprintf(fd, "{0}", sbytes[j]);
                }
                else if (fip.Type == TiffType.UNDEFINED)
                {
                    if (bytes != null)
                        fprintf(fd, "0x{0:x}", bytes[j]);
                }
                else if (fip.Type == TiffType.SHORT || fip.Type == TiffType.SSHORT)
                {
                    if (shorts != null)
                        fprintf(fd, "{0}", shorts[j]);
                    else if (ushorts != null)
                        fprintf(fd, "{0}", ushorts[j]);
                }
                else if (fip.Type == TiffType.LONG || fip.Type == TiffType.SLONG)
                {
                    if (ints != null)
                        fprintf(fd, "{0}", ints[j]);
                    else if (uints != null)
                        fprintf(fd, "{0}", uints[j]);
                }
                else if (fip.Type == TiffType.RATIONAL ||
                    fip.Type == TiffType.SRATIONAL ||
                    fip.Type == TiffType.FLOAT)
                {
                    if (floats != null)
                        fprintf(fd, "{0}", floats[j]);
                }
                else if (fip.Type == TiffType.IFD)
                {
                    if (ints != null)
                        fprintf(fd, "0x{0:x}", ints[j]);
                    else if (uints != null)
                        fprintf(fd, "0x{0:x}", uints[j]);
                }
                else if (fip.Type == TiffType.ASCII)
                {
                    if (s != null)
                        fprintf(fd, "{0}", s);

                    break;
                }
                else if (fip.Type == TiffType.DOUBLE || fip.Type == TiffType.FLOAT)
                {
                    if (floats != null)
                        fprintf(fd, "{0}", floats[j]);
                    else if (doubles != null)
                        fprintf(fd, "{0}", doubles[j]);
                }
                else
                {
                    fprintf(fd, "<unsupported data type in printField>");
                    break;
                }

                if (j < value_count - 1)
                    fprintf(fd, ",");
            }

            fprintf(fd, "\r\n");
        }

        private bool prettyPrintField(Stream fd, TiffTag tag, int value_count, object raw_data)
        {
            FieldValue value = new FieldValue(raw_data);
            short[] sdata = value.ToShortArray();
            float[] fdata = value.ToFloatArray();
            double[] ddata = value.ToDoubleArray();

            switch (tag)
            {
                case TiffTag.INKSET:
                    if (sdata != null)
                    {
                        fprintf(fd, "  Ink Set: ");
                        switch ((InkSet)sdata[0])
                        {
                            case InkSet.CMYK:
                                fprintf(fd, "CMYK\n");
                                break;

                            default:
                                fprintf(fd, "{0} (0x{1:x})\n", sdata[0], sdata[0]);
                                break;
                        }
                        return true;
                    }
                    return false;

                case TiffTag.DOTRANGE:
                    if (sdata != null)
                    {
                        fprintf(fd, "  Dot Range: {0}-{1}\n", sdata[0], sdata[1]);
                        return true;
                    }
                    return false;

                case TiffTag.WHITEPOINT:
                    if (fdata != null)
                    {
                        fprintf(fd, "  White Point: {0:G}-{1:G}\n", fdata[0], fdata[1]);
                        return true;
                    }
                    return false;

                case TiffTag.REFERENCEBLACKWHITE:
                    if (fdata != null)
                    {
                        fprintf(fd, "  Reference Black/White:\n");
                        for (short i = 0; i < 3; i++)
                            fprintf(fd, "    {0,2:D}: {1,5:G} {2,5:G}\n", i, fdata[2 * i + 0], fdata[2 * i + 1]);
                        return true;
                    }
                    return false;

                case TiffTag.XMLPACKET:
                    string s = raw_data as string;
                    if (s != null)
                    {
                        fprintf(fd, "  XMLPacket (XMP Metadata):\n");
                        fprintf(fd, s.Substring(0, value_count));
                        fprintf(fd, "\n");
                        return true;
                    }
                    return false;

                case TiffTag.RICHTIFFIPTC:
                    // XXX: for some weird reason RichTIFFIPTC tag defined
                    // as array of LONG values.
                    fprintf(fd, "  RichTIFFIPTC Data: <present>, {0} bytes\n", value_count * 4);
                    return true;

                case TiffTag.PHOTOSHOP:
                    fprintf(fd, "  Photoshop Data: <present>, {0} bytes\n", value_count);
                    return true;

                case TiffTag.ICCPROFILE:
                    fprintf(fd, "  ICC Profile: <present>, {0} bytes\n", value_count);
                    return true;

                case TiffTag.STONITS:
                    if (ddata != null)
                    {
                        fprintf(fd, "  Sample to Nits conversion factor: {0:e4}\n", ddata[0]);
                        return true;
                    }
                    return false;
            }

            return false;
        }

        private static void printAscii(Stream fd, string cp)
        {
            for (int cpPos = 0; cp[cpPos] != '\0'; cpPos++)
            {
                if (!char.IsControl(cp[cpPos]))
                {
                    fprintf(fd, "{0}", cp[cpPos]);
                    continue;
                }

                string tp = "\tt\bb\rr\nn\vv";
                int tpPos = 0;
                for (; tp[tpPos] != 0; tpPos++)
                {
                    if (tp[tpPos++] == cp[cpPos])
                        break;
                }

                if (tp[tpPos] != 0)
                    fprintf(fd, "\\{0}", tp[tpPos]);
                else
                    fprintf(fd, "\\{0}", encodeOctalString((byte)(cp[cpPos] & 0xff)));
            }
        }
    }
}
