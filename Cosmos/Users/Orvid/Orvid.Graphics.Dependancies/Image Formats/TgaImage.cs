// ==========================================================
// TargaImage
//
// Design and implementation by
// - David Polomis (paloma_sw@cox.net)
//
//
// This source code, along with any associated files, is licensed under
// The Code Project Open License (CPOL) 1.02
// A copy of this license can be found at
// http://www.codeproject.com/info/cpol10.aspx
//
// 
// COVERED CODE IS PROVIDED UNDER THIS LICENSE ON AN "AS IS" BASIS,
// WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
// INCLUDING, WITHOUT LIMITATION, WARRANTIES THAT THE COVERED CODE IS
// FREE OF DEFECTS, MERCHANTABLE, FIT FOR A PARTICULAR PURPOSE OR
// NON-INFRINGING. THE ENTIRE RISK AS TO THE QUALITY AND PERFORMANCE
// OF THE COVERED CODE IS WITH YOU. SHOULD ANY COVERED CODE PROVE
// DEFECTIVE IN ANY RESPECT, YOU (NOT THE INITIAL DEVELOPER OR ANY
// OTHER CONTRIBUTOR) ASSUME THE COST OF ANY NECESSARY SERVICING,
// REPAIR OR CORRECTION. THIS DISCLAIMER OF WARRANTY CONSTITUTES AN
// ESSENTIAL PART OF THIS LICENSE. NO USE OF ANY COVERED CODE IS
// AUTHORIZED HEREUNDER EXCEPT UNDER THIS DISCLAIMER.
//
// Use at your own risk!
//
// ==========================================================


using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Paloma
{
    #region TargaConstants
    internal static class TargaConstants
    {
        internal const int HeaderByteLength = 18;
        internal const int FooterByteLength = 26;
        internal const int FooterSignatureOffsetFromEnd = 18;
        internal const int FooterSignatureByteLength = 16;
        internal const int FooterReservedCharByteLength = 1;
        internal const int ExtensionAreaAuthorNameByteLength = 41;
        internal const int ExtensionAreaAuthorCommentsByteLength = 324;
        internal const int ExtensionAreaJobNameByteLength = 41;
        internal const int ExtensionAreaSoftwareIDByteLength = 41;
        internal const int ExtensionAreaSoftwareVersionLetterByteLength = 1;
        internal const int ExtensionAreaColorCorrectionTableValueLength = 256;
        internal const string TargaFooterASCIISignature = "TRUEVISION-XFILE";
    }
    #endregion

    #region TGAFormat
    internal enum TGAFormat
    {
        UNKNOWN = 0,
        ORIGINAL_TGA = 100,
        NEW_TGA = 200
    }
    #endregion

    #region ColorMapType
    internal enum ColorMapType : byte
    {
        NO_COLOR_MAP = 0,
        COLOR_MAP_INCLUDED = 1
    }
    #endregion

    #region ImageType
    internal enum ImageType : byte
    {
        NO_IMAGE_DATA = 0,
        UNCOMPRESSED_COLOR_MAPPED = 1,
        UNCOMPRESSED_TRUE_COLOR = 2,
        UNCOMPRESSED_BLACK_AND_WHITE = 3,
        RUN_LENGTH_ENCODED_COLOR_MAPPED = 9,
        RUN_LENGTH_ENCODED_TRUE_COLOR = 10,
        RUN_LENGTH_ENCODED_BLACK_AND_WHITE = 11
    }
    #endregion

    #region VerticalTransferOrder
    internal enum VerticalTransferOrder
    {
        UNKNOWN = -1,
        BOTTOM = 0,
        TOP = 1
    }
    #endregion

    #region HorizontalTransferOrder
    internal enum HorizontalTransferOrder
    {
        UNKNOWN = -1,
        RIGHT = 0,
        LEFT = 1
    }
    #endregion

    #region FirstPixelDestination
    internal enum FirstPixelDestination
    {
        UNKNOWN = 0,
        TOP_LEFT = 1,
        TOP_RIGHT = 2,
        BOTTOM_LEFT = 3,
        BOTTOM_RIGHT = 4
    }
    #endregion

    #region RLEPacketType
    internal enum RLEPacketType
    {
        RAW = 0,
        RUN_LENGTH = 1
    }
    #endregion

    #region TargaImage
    /// <summary>
    /// Reads and loads a Truevision TGA Format image file.
    /// </summary>
    public class TargaImage : IDisposable
    {
        private TargaHeader objTargaHeader = null;
        private TargaExtensionArea objTargaExtensionArea = null;
        private TargaFooter objTargaFooter = null;
        private Bitmap bmpTargaImage = null;
        private TGAFormat eTGAFormat = TGAFormat.UNKNOWN;
        private string strFileName = string.Empty;
        private int intStride = 0;
        private int intPadding = 0;
        private GCHandle ImageByteHandle;
        private System.Collections.Generic.List<System.Collections.Generic.List<byte>> rows = new System.Collections.Generic.List<System.Collections.Generic.List<byte>>();
        private System.Collections.Generic.List<byte> row = new System.Collections.Generic.List<byte>();
        private bool disposed = false;
        public TargaImage()
        {
            this.objTargaFooter = new TargaFooter();
            this.objTargaHeader = new TargaHeader();
            this.objTargaExtensionArea = new TargaExtensionArea();
            this.bmpTargaImage = null;
        }
        internal TargaHeader Header
        {
            get { return this.objTargaHeader; }
        }
        internal TGAFormat Format
        {
            get { return this.eTGAFormat; }
        }
        public Bitmap Image
        {
            get { return this.bmpTargaImage; }
        }
        internal string FileName
        {
            get { return this.strFileName; }
        }
        internal int Stride
        {
            get { return this.intStride; }
        }
        internal int Padding
        {
            get { return this.intPadding; }
        }
        ~TargaImage()
        {
            Dispose(false);
        }

        public TargaImage(Stream s)
            : this()
        {
            using (BinaryReader binReader = new BinaryReader(s))
            {
                this.LoadTGAFooterInfo(binReader);
                this.LoadTGAHeaderInfo(binReader);
                this.LoadTGAExtensionArea(binReader);
                this.LoadTGAImage(binReader);
            }
        }

        /// <summary>
        /// Loads the Targa Footer information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAFooterInfo(BinaryReader binReader)
        {
            binReader.BaseStream.Seek((TargaConstants.FooterSignatureOffsetFromEnd * -1), SeekOrigin.End);
            string Signature = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterSignatureByteLength)).TrimEnd('\0');
            if (string.Compare(Signature, TargaConstants.TargaFooterASCIISignature) == 0)
            {
                this.eTGAFormat = TGAFormat.NEW_TGA;
                binReader.BaseStream.Seek((TargaConstants.FooterByteLength * -1), SeekOrigin.End);
                int ExtOffset = binReader.ReadInt32();
                int DevDirOff = binReader.ReadInt32();
                binReader.ReadBytes(TargaConstants.FooterSignatureByteLength);
                string ResChar = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.FooterReservedCharByteLength)).TrimEnd('\0');
                this.objTargaFooter.SetExtensionAreaOffset(ExtOffset);
                this.objTargaFooter.SetDeveloperDirectoryOffset(DevDirOff);
                this.objTargaFooter.SetSignature(Signature);
                this.objTargaFooter.SetReservedCharacter(ResChar);
            }
            else
            {
                this.eTGAFormat = TGAFormat.ORIGINAL_TGA;
            }
        }


        /// <summary>
        /// Loads the Targa Header information from the file.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAHeaderInfo(BinaryReader binReader)
        {
            binReader.BaseStream.Seek(0, SeekOrigin.Begin);
            this.objTargaHeader.SetImageIDLength(binReader.ReadByte());
            this.objTargaHeader.SetColorMapType((ColorMapType)binReader.ReadByte());
            this.objTargaHeader.SetImageType((ImageType)binReader.ReadByte());
            this.objTargaHeader.SetColorMapFirstEntryIndex(binReader.ReadInt16());
            this.objTargaHeader.SetColorMapLength(binReader.ReadInt16());
            this.objTargaHeader.SetColorMapEntrySize(binReader.ReadByte());
            this.objTargaHeader.SetXOrigin(binReader.ReadInt16());
            this.objTargaHeader.SetYOrigin(binReader.ReadInt16());
            this.objTargaHeader.SetWidth(binReader.ReadInt16());
            this.objTargaHeader.SetHeight(binReader.ReadInt16());
            byte pixeldepth = binReader.ReadByte();
            switch (pixeldepth)
            {
                case 8:
                case 16:
                case 24:
                case 32:
                    this.objTargaHeader.SetPixelDepth(pixeldepth);
                    break;

                default:
                    this.ClearAll();
                    throw new Exception("Targa Image only supports 8, 16, 24, or 32 bit pixel depths.");
            }


            byte ImageDescriptor = binReader.ReadByte();
            this.objTargaHeader.SetAttributeBits((byte)Utilities.GetBits(ImageDescriptor, 0, 4));

            this.objTargaHeader.SetVerticalTransferOrder((VerticalTransferOrder)Utilities.GetBits(ImageDescriptor, 5, 1));
            this.objTargaHeader.SetHorizontalTransferOrder((HorizontalTransferOrder)Utilities.GetBits(ImageDescriptor, 4, 1));

            if (this.objTargaHeader.ImageIDLength > 0)
            {
                byte[] ImageIDValueBytes = binReader.ReadBytes(this.objTargaHeader.ImageIDLength);
                this.objTargaHeader.SetImageIDValue(System.Text.Encoding.ASCII.GetString(ImageIDValueBytes).TrimEnd('\0'));
            }
            if (this.objTargaHeader.ColorMapType == ColorMapType.COLOR_MAP_INCLUDED)
            {
                if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                {
                    if (this.objTargaHeader.ColorMapLength > 0)
                    {
                        for (int i = 0; i < this.objTargaHeader.ColorMapLength; i++)
                        {
                            int a = 0;
                            int r = 0;
                            int g = 0;
                            int b = 0;
                            switch (this.objTargaHeader.ColorMapEntrySize)
                            {
                                case 15:
                                    byte[] color15 = binReader.ReadBytes(2);
                                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color15[1], color15[0]));
                                    break;
                                case 16:
                                    byte[] color16 = binReader.ReadBytes(2);
                                    this.objTargaHeader.ColorMap.Add(Utilities.GetColorFrom2Bytes(color16[1], color16[0]));
                                    break;
                                case 24:
                                    b = Convert.ToInt32(binReader.ReadByte());
                                    g = Convert.ToInt32(binReader.ReadByte());
                                    r = Convert.ToInt32(binReader.ReadByte());
                                    this.objTargaHeader.ColorMap.Add(System.Drawing.Color.FromArgb(r, g, b));
                                    break;
                                case 32:
                                    a = Convert.ToInt32(binReader.ReadByte());
                                    b = Convert.ToInt32(binReader.ReadByte());
                                    g = Convert.ToInt32(binReader.ReadByte());
                                    r = Convert.ToInt32(binReader.ReadByte());
                                    this.objTargaHeader.ColorMap.Add(System.Drawing.Color.FromArgb(a, r, g, b));
                                    break;
                                default:
                                    this.ClearAll();
                                    throw new Exception("TargaImage only supports ColorMap Entry Sizes of 15, 16, 24 or 32 bits.");

                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Image Type requires a Color Map and Color Map Length is zero.");
                    }
                }
            }
            else
            {
                if (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_COLOR_MAPPED ||
                    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED)
                {
                    this.ClearAll();
                    throw new Exception("Image Type requires a Color Map and there was not a Color Map included in the file.");
                }
            }
        }


        /// <summary>
        /// Loads the Targa Extension Area from the file, if it exists.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAExtensionArea(BinaryReader binReader)
        {
            if (this.objTargaFooter.ExtensionAreaOffset > 0)
            {
                binReader.BaseStream.Seek(this.objTargaFooter.ExtensionAreaOffset, SeekOrigin.Begin);
                this.objTargaExtensionArea.SetExtensionSize((int)(binReader.ReadInt16()));
                this.objTargaExtensionArea.SetAuthorName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorNameByteLength)).TrimEnd('\0'));
                this.objTargaExtensionArea.SetAuthorComments(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaAuthorCommentsByteLength)).TrimEnd('\0'));
                Int16 iMonth = binReader.ReadInt16();
                Int16 iDay = binReader.ReadInt16();
                Int16 iYear = binReader.ReadInt16();
                Int16 iHour = binReader.ReadInt16();
                Int16 iMinute = binReader.ReadInt16();
                Int16 iSecond = binReader.ReadInt16();
                DateTime dtstamp;
                string strStamp = iMonth.ToString() + @"/" + iDay.ToString() + @"/" + iYear.ToString() + @" ";
                strStamp += iHour.ToString() + @":" + iMinute.ToString() + @":" + iSecond.ToString();
                if (DateTime.TryParse(strStamp, out dtstamp) == true)
                    this.objTargaExtensionArea.SetDateTimeStamp(dtstamp);
                this.objTargaExtensionArea.SetJobName(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaJobNameByteLength)).TrimEnd('\0'));
                iHour = binReader.ReadInt16();
                iMinute = binReader.ReadInt16();
                iSecond = binReader.ReadInt16();
                TimeSpan ts = new TimeSpan((int)iHour, (int)iMinute, (int)iSecond);
                this.objTargaExtensionArea.SetJobTime(ts);
                this.objTargaExtensionArea.SetSoftwareID(System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareIDByteLength)).TrimEnd('\0'));
                float iVersionNumber = (float)binReader.ReadInt16() / 100.0F;
                string strVersionLetter = System.Text.Encoding.ASCII.GetString(binReader.ReadBytes(TargaConstants.ExtensionAreaSoftwareVersionLetterByteLength)).TrimEnd('\0');
                this.objTargaExtensionArea.SetSoftwareID(iVersionNumber.ToString(@"F2") + strVersionLetter);
                int a = (int)binReader.ReadByte();
                int r = (int)binReader.ReadByte();
                int b = (int)binReader.ReadByte();
                int g = (int)binReader.ReadByte();
                this.objTargaExtensionArea.SetKeyColor(Color.FromArgb(a, r, g, b));
                this.objTargaExtensionArea.SetPixelAspectRatioNumerator((int)binReader.ReadInt16());
                this.objTargaExtensionArea.SetPixelAspectRatioDenominator((int)binReader.ReadInt16());
                this.objTargaExtensionArea.SetGammaNumerator((int)binReader.ReadInt16());
                this.objTargaExtensionArea.SetGammaDenominator((int)binReader.ReadInt16());
                this.objTargaExtensionArea.SetColorCorrectionOffset(binReader.ReadInt32());
                this.objTargaExtensionArea.SetPostageStampOffset(binReader.ReadInt32());
                this.objTargaExtensionArea.SetScanLineOffset(binReader.ReadInt32());
                this.objTargaExtensionArea.SetAttributesType((int)binReader.ReadByte());

                if (this.objTargaExtensionArea.ScanLineOffset > 0)
                {
                    binReader.BaseStream.Seek(this.objTargaExtensionArea.ScanLineOffset, SeekOrigin.Begin);
                    for (int i = 0; i < this.objTargaHeader.Height; i++)
                    {
                        this.objTargaExtensionArea.ScanLineTable.Add(binReader.ReadInt32());
                    }
                }
                if (this.objTargaExtensionArea.ColorCorrectionOffset > 0)
                {
                    binReader.BaseStream.Seek(this.objTargaExtensionArea.ColorCorrectionOffset, SeekOrigin.Begin);
                    for (int i = 0; i < TargaConstants.ExtensionAreaColorCorrectionTableValueLength; i++)
                    {
                        a = (int)binReader.ReadInt16();
                        r = (int)binReader.ReadInt16();
                        b = (int)binReader.ReadInt16();
                        g = (int)binReader.ReadInt16();
                        this.objTargaExtensionArea.ColorCorrectionTable.Add(Color.FromArgb(a, r, g, b));
                    }
                }
            }
        }

        /// <summary>
        /// Reads the image data bytes from the file. Handles Uncompressed and RLE Compressed image data. 
        /// Uses FirstPixelDestination to properly align the image.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        /// <returns>An array of bytes representing the image data in the proper alignment.</returns>
        private byte[] LoadImageBytes(BinaryReader binReader)
        {
            byte[] data = null;
            if (this.objTargaHeader.ImageDataOffset > 0)
            {
                byte[] padding = new byte[this.intPadding];
                MemoryStream msData = null;
                binReader.BaseStream.Seek(this.objTargaHeader.ImageDataOffset, SeekOrigin.Begin);
                int intImageRowByteSize = (int)this.objTargaHeader.Width * ((int)this.objTargaHeader.BytesPerPixel);
                int intImageByteSize = intImageRowByteSize * (int)this.objTargaHeader.Height;
                if (this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE ||
                   this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_COLOR_MAPPED ||
                   this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_TRUE_COLOR)
                {

                    #region COMPRESSED
                    byte bRLEPacket = 0;
                    int intRLEPacketType = -1;
                    int intRLEPixelCount = 0;
                    byte[] bRunLengthPixel = null;
                    int intImageBytesRead = 0;
                    int intImageRowBytesRead = 0;
                    while (intImageBytesRead < intImageByteSize)
                    {
                        bRLEPacket = binReader.ReadByte();
                        intRLEPacketType = Utilities.GetBits(bRLEPacket, 7, 1);
                        intRLEPixelCount = Utilities.GetBits(bRLEPacket, 0, 7) + 1;
                        if ((RLEPacketType)intRLEPacketType == RLEPacketType.RUN_LENGTH)
                        {
                            bRunLengthPixel = binReader.ReadBytes((int)this.objTargaHeader.BytesPerPixel);

                            for (int i = 0; i < intRLEPixelCount; i++)
                            {
                                foreach (byte b in bRunLengthPixel)
                                    row.Add(b);
                                intImageRowBytesRead += bRunLengthPixel.Length;
                                intImageBytesRead += bRunLengthPixel.Length;
                                if (intImageRowBytesRead == intImageRowByteSize)
                                {
                                    rows.Add(row);
                                    row = new System.Collections.Generic.List<byte>();
                                    intImageRowBytesRead = 0;
                                }
                            }
                        }
                        else if ((RLEPacketType)intRLEPacketType == RLEPacketType.RAW)
                        {
                            int intBytesToRead = intRLEPixelCount * (int)this.objTargaHeader.BytesPerPixel;
                            for (int i = 0; i < intBytesToRead; i++)
                            {
                                row.Add(binReader.ReadByte());
                                intImageBytesRead++;
                                intImageRowBytesRead++;
                                if (intImageRowBytesRead == intImageRowByteSize)
                                {
                                    rows.Add(row);
                                    row = new System.Collections.Generic.List<byte>();
                                    intImageRowBytesRead = 0;
                                }
                            }
                        }
                    }
                    #endregion

                }

                else
                {
                    #region NON-COMPRESSED
                    for (int i = 0; i < (int)this.objTargaHeader.Height; i++)
                    {
                        for (int j = 0; j < intImageRowByteSize; j++)
                        {
                            row.Add(binReader.ReadByte());
                        }
                        rows.Add(row);
                        row = new System.Collections.Generic.List<byte>();
                    }
                    #endregion
                }
                bool blnRowsReverse = false;
                bool blnEachRowReverse = false;
                switch (this.objTargaHeader.FirstPixelDestination)
                {
                    case FirstPixelDestination.TOP_LEFT:
                        blnRowsReverse = false;
                        blnEachRowReverse = true;
                        break;

                    case FirstPixelDestination.TOP_RIGHT:
                        blnRowsReverse = false;
                        blnEachRowReverse = false;
                        break;

                    case FirstPixelDestination.BOTTOM_LEFT:
                        blnRowsReverse = true;
                        blnEachRowReverse = true;
                        break;

                    case FirstPixelDestination.BOTTOM_RIGHT:
                    case FirstPixelDestination.UNKNOWN:
                        blnRowsReverse = true;
                        blnEachRowReverse = false;

                        break;
                }
                using (msData = new MemoryStream())
                {
                    if (blnRowsReverse == true)
                        rows.Reverse();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (blnEachRowReverse == true)
                            rows[i].Reverse();
                        byte[] brow = rows[i].ToArray();
                        msData.Write(brow, 0, brow.Length);
                        msData.Write(padding, 0, padding.Length);
                    }
                    data = msData.ToArray();
                }
            }
            else
            {
                throw new Exception(@"Error, No image data in file.");
            }
            return data;

        }

        /// <summary>
        /// Reads the image data bytes from the file and loads them into the Image Bitmap object.
        /// Also loads the color map, if any, into the Image Bitmap.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        private void LoadTGAImage(BinaryReader binReader)
        {
            this.intStride = (((int)this.objTargaHeader.Width * (int)this.objTargaHeader.PixelDepth + 31) & ~31) >> 3;
            this.intPadding = this.intStride - ((((int)this.objTargaHeader.Width * (int)this.objTargaHeader.PixelDepth) + 7) / 8);
            byte[] bimagedata = this.LoadImageBytes(binReader);
            this.ImageByteHandle = GCHandle.Alloc(bimagedata, GCHandleType.Pinned);
            if (this.bmpTargaImage != null)
            {
                this.bmpTargaImage.Dispose();
            }
            PixelFormat pf = this.GetPixelFormat();
            this.bmpTargaImage = new Bitmap((int)this.objTargaHeader.Width, (int)this.objTargaHeader.Height, this.intStride, pf, this.ImageByteHandle.AddrOfPinnedObject());
            this.LoadThumbnail(binReader, pf);
            if (this.objTargaHeader.ColorMap.Count > 0)
            {
                ColorPalette pal = this.bmpTargaImage.Palette;
                for (int i = 0; i < this.objTargaHeader.ColorMap.Count; i++)
                {
                    if (this.objTargaExtensionArea.AttributesType == 0 ||
                        this.objTargaExtensionArea.AttributesType == 1)
                        pal.Entries[i] = Color.FromArgb(255, this.objTargaHeader.ColorMap[i].R, this.objTargaHeader.ColorMap[i].G, this.objTargaHeader.ColorMap[i].B);
                    else
                        pal.Entries[i] = this.objTargaHeader.ColorMap[i];

                }
                this.bmpTargaImage.Palette = pal;
            }
            else
            {
                if (this.objTargaHeader.PixelDepth == 8 && (this.objTargaHeader.ImageType == ImageType.UNCOMPRESSED_BLACK_AND_WHITE ||
                    this.objTargaHeader.ImageType == ImageType.RUN_LENGTH_ENCODED_BLACK_AND_WHITE))
                {
                    ColorPalette pal = this.bmpTargaImage.Palette;
                    for (int i = 0; i < 256; i++)
                    {
                        pal.Entries[i] = Color.FromArgb(i, i, i);
                    }
                    this.bmpTargaImage.Palette = pal;
                }
            }
        }

        /// <summary>
        /// Gets the PixelFormat to be used by the Image based on the Targa file's attributes
        /// </summary>
        /// <returns></returns>
        private PixelFormat GetPixelFormat()
        {
            PixelFormat pfTargaPixelFormat = PixelFormat.Undefined;
            switch (this.objTargaHeader.PixelDepth)
            {
                case 8:
                    pfTargaPixelFormat = PixelFormat.Format8bppIndexed;
                    break;

                case 16:
                    if (this.Format == TGAFormat.NEW_TGA)
                    {
                        switch (this.objTargaExtensionArea.AttributesType)
                        {
                            case 0:
                            case 1:
                            case 2:
                                pfTargaPixelFormat = PixelFormat.Format16bppRgb555;
                                break;

                            case 3:
                                pfTargaPixelFormat = PixelFormat.Format16bppArgb1555;
                                break;
                        }
                    }
                    else
                    {
                        pfTargaPixelFormat = PixelFormat.Format16bppRgb555;
                    }
                    break;

                case 24:
                    pfTargaPixelFormat = PixelFormat.Format24bppRgb;
                    break;

                case 32:
                    if (this.Format == TGAFormat.NEW_TGA)
                    {
                        switch (this.objTargaExtensionArea.AttributesType)
                        {

                            case 1:
                            case 2:
                                pfTargaPixelFormat = PixelFormat.Format32bppRgb;
                                break;

                            case 0:
                            case 3:
                                pfTargaPixelFormat = PixelFormat.Format32bppArgb;
                                break;

                            case 4:
                                pfTargaPixelFormat = PixelFormat.Format32bppPArgb;
                                break;
                        }
                    }
                    else
                    {
                        pfTargaPixelFormat = PixelFormat.Format32bppRgb;
                        break;
                    }
                    break;
            }
            return pfTargaPixelFormat;
        }


        /// <summary>
        /// Loads the thumbnail of the loaded image file, if any.
        /// </summary>
        /// <param name="binReader">A BinaryReader that points the loaded file byte stream.</param>
        /// <param name="pfPixelFormat">A PixelFormat value indicating what pixel format to use when loading the thumbnail.</param>
        private void LoadThumbnail(BinaryReader binReader, PixelFormat pfPixelFormat)
        {
            if (this.objTargaExtensionArea.PostageStampOffset > 0)
            {
                binReader.BaseStream.Seek(this.objTargaExtensionArea.PostageStampOffset, SeekOrigin.Begin);
                int iWidth = (int)binReader.ReadByte();
                int iHeight = (int)binReader.ReadByte();
                int intImageRowByteSize = iWidth * ((int)this.objTargaHeader.PixelDepth / 8);
                int intImageByteSize = intImageRowByteSize * iHeight;
                for (int i = 0; i < intImageByteSize; i++)
                {
                    binReader.ReadByte();
                }
            }
        }

        /// <summary>
        /// Clears out all objects and resources.
        /// </summary>
        private void ClearAll()
        {
            if (this.bmpTargaImage != null)
            {
                this.bmpTargaImage.Dispose();
                this.bmpTargaImage = null;
            }
            if (this.ImageByteHandle.IsAllocated)
                this.ImageByteHandle.Free();

            this.objTargaHeader = new TargaHeader();
            this.objTargaExtensionArea = new TargaExtensionArea();
            this.objTargaFooter = new TargaFooter();
            this.eTGAFormat = TGAFormat.UNKNOWN;
            this.intStride = 0;
            this.intPadding = 0;
            this.rows.Clear();
            this.row.Clear();
            this.strFileName = string.Empty;

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);

        }
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.bmpTargaImage != null)
                    {
                        this.bmpTargaImage.Dispose();
                    }
                    if (this.ImageByteHandle != null)
                    {
                        if (this.ImageByteHandle.IsAllocated)
                        {
                            this.ImageByteHandle.Free();
                        }

                    }
                }
            }
            disposed = true;
        }
    }
    #endregion

    #region TargaHeader
    internal class TargaHeader
    {
        private byte bImageIDLength = 0;
        private ColorMapType eColorMapType = ColorMapType.NO_COLOR_MAP;
        private ImageType eImageType = ImageType.NO_IMAGE_DATA;
        private short sColorMapFirstEntryIndex = 0;
        private short sColorMapLength = 0;
        private byte bColorMapEntrySize = 0;
        private short sXOrigin = 0;
        private short sYOrigin = 0;
        private short sWidth = 0;
        private short sHeight = 0;
        private byte bPixelDepth = 0;
        private byte bImageDescriptor = 0;
        private VerticalTransferOrder eVerticalTransferOrder = VerticalTransferOrder.UNKNOWN;
        private HorizontalTransferOrder eHorizontalTransferOrder = HorizontalTransferOrder.UNKNOWN;
        private byte bAttributeBits = 0;
        private string strImageIDValue = string.Empty;
        private System.Collections.Generic.List<System.Drawing.Color> cColorMap = new List<System.Drawing.Color>();

        public byte ImageIDLength
        {
            get { return this.bImageIDLength; }
        }
        internal protected void SetImageIDLength(byte bImageIDLength)
        {
            this.bImageIDLength = bImageIDLength;
        }
        public ColorMapType ColorMapType
        {
            get { return this.eColorMapType; }
        }
        internal protected void SetColorMapType(ColorMapType eColorMapType)
        {
            this.eColorMapType = eColorMapType;
        }
        public ImageType ImageType
        {
            get { return this.eImageType; }
        }
        internal protected void SetImageType(ImageType eImageType)
        {
            this.eImageType = eImageType;
        }
        public short ColorMapFirstEntryIndex
        {
            get { return this.sColorMapFirstEntryIndex; }
        }
        internal protected void SetColorMapFirstEntryIndex(short sColorMapFirstEntryIndex)
        {
            this.sColorMapFirstEntryIndex = sColorMapFirstEntryIndex;
        }
        public short ColorMapLength
        {
            get { return this.sColorMapLength; }
        }
        internal protected void SetColorMapLength(short sColorMapLength)
        {
            this.sColorMapLength = sColorMapLength;
        }
        public byte ColorMapEntrySize
        {
            get { return this.bColorMapEntrySize; }
        }
        internal protected void SetColorMapEntrySize(byte bColorMapEntrySize)
        {
            this.bColorMapEntrySize = bColorMapEntrySize;
        }
        public short XOrigin
        {
            get { return this.sXOrigin; }
        }
        internal protected void SetXOrigin(short sXOrigin)
        {
            this.sXOrigin = sXOrigin;
        }
        public short YOrigin
        {
            get { return this.sYOrigin; }
        }
        internal protected void SetYOrigin(short sYOrigin)
        {
            this.sYOrigin = sYOrigin;
        }
        public short Width
        {
            get { return this.sWidth; }
        }
        internal protected void SetWidth(short sWidth)
        {
            this.sWidth = sWidth;
        }
        public short Height
        {
            get { return this.sHeight; }
        }
        internal protected void SetHeight(short sHeight)
        {
            this.sHeight = sHeight;
        }
        public byte PixelDepth
        {
            get { return this.bPixelDepth; }
        }
        internal protected void SetPixelDepth(byte bPixelDepth)
        {
            this.bPixelDepth = bPixelDepth;
        }
        internal protected byte ImageDescriptor
        {
            get { return this.bImageDescriptor; }
            set { this.bImageDescriptor = value; }
        }
        public FirstPixelDestination FirstPixelDestination
        {
            get
            {
                if (this.eVerticalTransferOrder == VerticalTransferOrder.UNKNOWN || this.eHorizontalTransferOrder == HorizontalTransferOrder.UNKNOWN)
                    return FirstPixelDestination.UNKNOWN;
                else if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT)
                    return FirstPixelDestination.BOTTOM_LEFT;
                else if (this.eVerticalTransferOrder == VerticalTransferOrder.BOTTOM && this.eHorizontalTransferOrder == HorizontalTransferOrder.RIGHT)
                    return FirstPixelDestination.BOTTOM_RIGHT;
                else if (this.eVerticalTransferOrder == VerticalTransferOrder.TOP && this.eHorizontalTransferOrder == HorizontalTransferOrder.LEFT)
                    return FirstPixelDestination.TOP_LEFT;
                else
                    return FirstPixelDestination.TOP_RIGHT;
            }
        }
        public VerticalTransferOrder VerticalTransferOrder
        {
            get { return this.eVerticalTransferOrder; }
        }
        internal protected void SetVerticalTransferOrder(VerticalTransferOrder eVerticalTransferOrder)
        {
            this.eVerticalTransferOrder = eVerticalTransferOrder;
        }
        public HorizontalTransferOrder HorizontalTransferOrder
        {
            get { return this.eHorizontalTransferOrder; }
        }
        internal protected void SetHorizontalTransferOrder(HorizontalTransferOrder eHorizontalTransferOrder)
        {
            this.eHorizontalTransferOrder = eHorizontalTransferOrder;
        }
        public byte AttributeBits
        {
            get { return this.bAttributeBits; }
        }
        internal protected void SetAttributeBits(byte bAttributeBits)
        {
            this.bAttributeBits = bAttributeBits;
        }
        public string ImageIDValue
        {
            get { return this.strImageIDValue; }
        }
        internal protected void SetImageIDValue(string strImageIDValue)
        {
            this.strImageIDValue = strImageIDValue;
        }
        public System.Collections.Generic.List<System.Drawing.Color> ColorMap
        {
            get { return this.cColorMap; }
        }
        public int ImageDataOffset
        {
            get
            {
                int intImageDataOffset = TargaConstants.HeaderByteLength;
                intImageDataOffset += this.bImageIDLength;
                int Bytes = 0;
                switch (this.bColorMapEntrySize)
                {
                    case 15:
                        Bytes = 2;
                        break;
                    case 16:
                        Bytes = 2;
                        break;
                    case 24:
                        Bytes = 3;
                        break;
                    case 32:
                        Bytes = 4;
                        break;
                }
                intImageDataOffset += ((int)this.sColorMapLength * (int)Bytes);
                return intImageDataOffset;
            }
        }
        public int BytesPerPixel
        {
            get
            {
                return (int)this.bPixelDepth / 8;
            }
        }
    }
    #endregion

    #region TargaFooter
    internal class TargaFooter
    {
        private int intExtensionAreaOffset = 0;
        private int intDeveloperDirectoryOffset = 0;
        private string strSignature = string.Empty;
        private string strReservedCharacter = string.Empty;
        public int ExtensionAreaOffset
        {
            get { return this.intExtensionAreaOffset; }
        }
        internal protected void SetExtensionAreaOffset(int intExtensionAreaOffset)
        {
            this.intExtensionAreaOffset = intExtensionAreaOffset;
        }
        public int DeveloperDirectoryOffset
        {
            get { return this.intDeveloperDirectoryOffset; }
        }
        internal protected void SetDeveloperDirectoryOffset(int intDeveloperDirectoryOffset)
        {
            this.intDeveloperDirectoryOffset = intDeveloperDirectoryOffset;
        }
        public string Signature
        {
            get { return this.strSignature; }
        }
        internal protected void SetSignature(string strSignature)
        {
            this.strSignature = strSignature;
        }
        public string ReservedCharacter
        {
            get { return this.strReservedCharacter; }
        }
        internal protected void SetReservedCharacter(string strReservedCharacter)
        {
            this.strReservedCharacter = strReservedCharacter;
        }
        public TargaFooter() { }
    }
    #endregion

    #region TargaExtensionArea
    internal class TargaExtensionArea
    {
        int intExtensionSize = 0;
        string strAuthorName = string.Empty;
        string strAuthorComments = string.Empty;
        DateTime dtDateTimeStamp = DateTime.Now;
        string strJobName = string.Empty;
        TimeSpan dtJobTime = TimeSpan.Zero;
        string strSoftwareID = string.Empty;
        string strSoftwareVersion = string.Empty;
        Color cKeyColor = Color.Empty;
        int intPixelAspectRatioNumerator = 0;
        int intPixelAspectRatioDenominator = 0;
        int intGammaNumerator = 0;
        int intGammaDenominator = 0;
        int intColorCorrectionOffset = 0;
        int intPostageStampOffset = 0;
        int intScanLineOffset = 0;
        int intAttributesType = 0;
        private System.Collections.Generic.List<int> intScanLineTable = new List<int>();
        private System.Collections.Generic.List<System.Drawing.Color> cColorCorrectionTable = new List<System.Drawing.Color>();
        public int ExtensionSize
        {
            get { return this.intExtensionSize; }
        }
        internal protected void SetExtensionSize(int intExtensionSize)
        {
            this.intExtensionSize = intExtensionSize;
        }
        public string AuthorName
        {
            get { return this.strAuthorName; }
        }
        internal protected void SetAuthorName(string strAuthorName)
        {
            this.strAuthorName = strAuthorName;
        }
        public string AuthorComments
        {
            get { return this.strAuthorComments; }
        }
        internal protected void SetAuthorComments(string strAuthorComments)
        {
            this.strAuthorComments = strAuthorComments;
        }
        public DateTime DateTimeStamp
        {
            get { return this.dtDateTimeStamp; }
        }
        internal protected void SetDateTimeStamp(DateTime dtDateTimeStamp)
        {
            this.dtDateTimeStamp = dtDateTimeStamp;
        }
        public string JobName
        {
            get { return this.strJobName; }
        }
        internal protected void SetJobName(string strJobName)
        {
            this.strJobName = strJobName;
        }
        public TimeSpan JobTime
        {
            get { return this.dtJobTime; }
        }
        internal protected void SetJobTime(TimeSpan dtJobTime)
        {
            this.dtJobTime = dtJobTime;
        }
        public string SoftwareID
        {
            get { return this.strSoftwareID; }
        }
        internal protected void SetSoftwareID(string strSoftwareID)
        {
            this.strSoftwareID = strSoftwareID;
        }
        public string SoftwareVersion
        {
            get { return this.strSoftwareVersion; }
        }
        internal protected void SetSoftwareVersion(string strSoftwareVersion)
        {
            this.strSoftwareVersion = strSoftwareVersion;
        }
        public Color KeyColor
        {
            get { return this.cKeyColor; }
        }
        internal protected void SetKeyColor(Color cKeyColor)
        {
            this.cKeyColor = cKeyColor;
        }
        public int PixelAspectRatioNumerator
        {
            get { return this.intPixelAspectRatioNumerator; }
        }
        internal protected void SetPixelAspectRatioNumerator(int intPixelAspectRatioNumerator)
        {
            this.intPixelAspectRatioNumerator = intPixelAspectRatioNumerator;
        }
        public int PixelAspectRatioDenominator
        {
            get { return this.intPixelAspectRatioDenominator; }
        }
        internal protected void SetPixelAspectRatioDenominator(int intPixelAspectRatioDenominator)
        {
            this.intPixelAspectRatioDenominator = intPixelAspectRatioDenominator;
        }
        public float PixelAspectRatio
        {
            get
            {
                if (this.intPixelAspectRatioDenominator > 0)
                {
                    return (float)this.intPixelAspectRatioNumerator / (float)this.intPixelAspectRatioDenominator;
                }
                else
                    return 0.0F;
            }
        }
        public int GammaNumerator
        {
            get { return this.intGammaNumerator; }
        }
        internal protected void SetGammaNumerator(int intGammaNumerator)
        {
            this.intGammaNumerator = intGammaNumerator;
        }
        public int GammaDenominator
        {
            get { return this.intGammaDenominator; }
        }
        internal protected void SetGammaDenominator(int intGammaDenominator)
        {
            this.intGammaDenominator = intGammaDenominator;
        }
        public float GammaRatio
        {
            get
            {
                if (this.intGammaDenominator > 0)
                {
                    float ratio = (float)this.intGammaNumerator / (float)this.intGammaDenominator;
                    return (float)Math.Round(ratio, 1);
                }
                else
                    return 1.0F;
            }
        }
        public int ColorCorrectionOffset
        {
            get { return this.intColorCorrectionOffset; }
        }
        internal protected void SetColorCorrectionOffset(int intColorCorrectionOffset)
        {
            this.intColorCorrectionOffset = intColorCorrectionOffset;
        }
        public int PostageStampOffset
        {
            get { return this.intPostageStampOffset; }
        }
        internal protected void SetPostageStampOffset(int intPostageStampOffset)
        {
            this.intPostageStampOffset = intPostageStampOffset;
        }
        public int ScanLineOffset
        {
            get { return this.intScanLineOffset; }
        }
        internal protected void SetScanLineOffset(int intScanLineOffset)
        {
            this.intScanLineOffset = intScanLineOffset;
        }
        public int AttributesType
        {
            get { return this.intAttributesType; }
        }
        internal protected void SetAttributesType(int intAttributesType)
        {
            this.intAttributesType = intAttributesType;
        }
        public System.Collections.Generic.List<int> ScanLineTable
        {
            get { return this.intScanLineTable; }
        }
        public System.Collections.Generic.List<System.Drawing.Color> ColorCorrectionTable
        {
            get { return this.cColorCorrectionTable; }
        }
    }
    #endregion

    #region Utilities
    internal static class Utilities
    {
        internal static int GetBits(byte b, int offset, int count)
        {
            return (b >> offset) & ((1 << count) - 1);
        }
        internal static Color GetColorFrom2Bytes(byte one, byte two)
        {
            int r1 = Utilities.GetBits(one, 2, 5);
            int r = r1 << 3;
            int bit = Utilities.GetBits(one, 0, 2);
            int g1 = bit << 6;
            bit = Utilities.GetBits(two, 5, 3);
            int g2 = bit << 3;
            int g = g1 + g2;
            int b1 = Utilities.GetBits(two, 0, 5);
            int b = b1 << 3;
            int a1 = Utilities.GetBits(one, 7, 1);
            int a = a1 * 255;
            return Color.FromArgb(a, r, g, b);
        }
        internal static string GetIntBinaryString(Int32 n)
        {
            char[] b = new char[32];
            int pos = 31;
            int i = 0;

            while (i < 32)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }
        internal static string GetInt16BinaryString(Int16 n)
        {
            char[] b = new char[16];
            int pos = 15;
            int i = 0;

            while (i < 16)
            {
                if ((n & (1 << i)) != 0)
                {
                    b[pos] = '1';
                }
                else
                {
                    b[pos] = '0';
                }
                pos--;
                i++;
            }
            return new string(b);
        }
    }
    #endregion
}
