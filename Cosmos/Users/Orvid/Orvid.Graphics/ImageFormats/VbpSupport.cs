using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Orvid.Graphics.ImageFormats
{
    public class VbpImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            VbpFile.WriteBitmap(i, dest);
        }

        public override Image Load(Stream s)
        {
            return VbpFile.ReadBitmap(s);
        }


        // Please note, everything below this
        // point was originally from a plugin 
        // for Paint.Net, the plugin is available here:
        // http://forums.getpaint.net/index.php?/topic/14711-vantage-preview-images-vbp/
        //
        //
        // The source has been modified for use in this library.
        // 
        // This disclaimer was last
        // modified on August 9, 2011.


        #region VbpFile
        private static class VbpFile
        {
            private static Dictionary<uint, uint> _colorTable5Bit = new Dictionary<uint, uint>();
            private static Dictionary<uint, uint> _colorTable8Bit = new Dictionary<uint, uint>();

            static VbpFile()
            {
                for (uint i = 0; i <= 0xff; i++)
                {
                    _colorTable8Bit[i] = (uint)(((double)i) / 8.2);
                    if (i > 0)
                    {
                        if (_colorTable8Bit[i - 1] != _colorTable8Bit[i])
                        {
                            _colorTable5Bit[_colorTable8Bit[i]] = i;
                        }
                    }
                    else
                    {
                        _colorTable5Bit[0] = 0;
                    }
                }
            }

            private static int GetShift(uint mask)
            {
                int num = 0;
                uint num2 = mask;
                if ((num2 & 1) == 0)
                {
                    do
                    {
                        num++;
                        num2 = num2 >> 1;
                    }
                    while ((num2 & 1) == 0);
                }
                return num;
            }

            public static Image ReadBitmap(Stream input)
            {
                Image i = new Image(160, 120);
                VbpFileHeader fileHeader = ReadFileHeader(input);
                VbpInformationHeader infoHeader = ReadInfoHeader(input);
                if (infoHeader.Compression == IH_Compression.BI_BITFIELDS)
                {
                    i = ReadBitmapData(input, fileHeader, infoHeader);
                }
                return i;
            }

            static Image ReadBitmapData(Stream input, VbpFileHeader fileHeader, VbpInformationHeader infoHeader)
            {
                input.Position = fileHeader.OffBits;
                int count = (infoHeader.Width * infoHeader.Height) * 2;
                byte[] buffer = new byte[count];
                input.Read(buffer, 0, count);
                Image bitmap = new Image(infoHeader.Width, infoHeader.Height);
                int shift = GetShift(infoHeader.ColorMask.R);
                int num3 = GetShift(infoHeader.ColorMask.G);
                int num4 = GetShift(infoHeader.ColorMask.B);
                int num5 = 0;
                for (int i = 0; i < infoHeader.Height; i++)
                {
                    for (int j = 0; j < infoHeader.Width; j++)
                    {
                        ushort num8 = BitConverter.ToUInt16(buffer, num5++ * 2);
                        ushort red = (ushort)_colorTable5Bit[(num8 & infoHeader.ColorMask.R) >> shift];
                        ushort green = (ushort)_colorTable5Bit[(num8 & infoHeader.ColorMask.G) >> num3];
                        ushort blue = (ushort)_colorTable5Bit[(num8 & infoHeader.ColorMask.B) >> num4];
                        Pixel color = new Pixel((byte)red, (byte)green, (byte)blue, 255);
                        bitmap.SetPixel((uint)j, (uint)i, color);
                    }
                }
                return bitmap;
            }

            static VbpFileHeader ReadFileHeader(Stream input)
            {
                input.Position = 0L;
                byte[] buffer = new byte[14];
                input.Read(buffer, 0, 14);
                VbpFileHeader header = new VbpFileHeader
                {
                    Type = new char[] { (char)buffer[0], (char)buffer[1] }
                };
                if ((header.Type[0] != 'B') || (header.Type[1] != 'M'))
                {
                    throw new Exception("Wrong Filetype!");
                }
                header.Size = BitConverter.ToUInt32(buffer, 2);
                header.Reserved = BitConverter.ToUInt32(buffer, 6);
                header.OffBits = BitConverter.ToUInt32(buffer, 10);
                return header;
            }

            static VbpInformationHeader ReadInfoHeader(Stream input)
            {
                input.Position = 14L;
                byte[] buffer = new byte[40];
                input.Read(buffer, 0, 40);
                VbpInformationHeader header = new VbpInformationHeader
                {
                    Size = BitConverter.ToUInt32(buffer, 0),
                    Width = BitConverter.ToInt32(buffer, 4),
                    Height = BitConverter.ToInt32(buffer, 8),
                    Planes = BitConverter.ToUInt16(buffer, 12),
                    BitCount = BitConverter.ToUInt16(buffer, 14)
                };
                uint num = BitConverter.ToUInt32(buffer, 0x10);
                header.Compression = (IH_Compression)num;
                header.SizeImage = BitConverter.ToUInt32(buffer, 20);
                header.XPelsPerMeter = BitConverter.ToInt32(buffer, 0x18);
                header.YPelsPerMeter = BitConverter.ToInt32(buffer, 0x1c);
                header.ClrUsed = BitConverter.ToUInt32(buffer, 0x20);
                header.ClrImportant = BitConverter.ToUInt32(buffer, 0x24);
                if (header.Compression != IH_Compression.BI_BITFIELDS)
                {
                    throw new Exception("Wrong Filetype!");
                }
                input.Position = 0x36L;
                buffer = new byte[0x10];
                input.Read(buffer, 0, 0x10);
                header.ColorMask.R = BitConverter.ToUInt32(buffer, 0);
                header.ColorMask.G = BitConverter.ToUInt32(buffer, 4);
                header.ColorMask.B = BitConverter.ToUInt32(buffer, 8);
                header.ColorMask.Mask = BitConverter.ToUInt32(buffer, 12);
                return header;
            }

            public static void WriteBitmap(Image input, Stream output)
            {
                WriteFileHeader(output, input);
                WriteInfoHeader(output, input);
                WriteBitmapData(output, input);
            }

            static void WriteBitmapData(Stream output, Image input)
            {
                int num = 10;
                int num2 = 5;
                int num3 = 0;
                for (int i = 0; i < input.Height; i++)
                {
                    for (int j = 0; j < input.Width; j++)
                    {
                        Pixel pixel = input.GetPixel((uint)j, (uint)i);
                        ushort num6 = (ushort)(_colorTable8Bit[pixel.B] << (num3 & 0x1f));
                        ushort num7 = (ushort)(_colorTable8Bit[pixel.G] << (num2 & 0x1f));
                        ushort num8 = (ushort)(_colorTable8Bit[pixel.R] << (num & 0x1f));
                        ushort num9 = (ushort)(((0x8000 | num8) | num7) | num6);
                        output.Write(BitConverter.GetBytes(num9), 0, 2);
                    }
                }
            }

            private static void WriteBytes(char[] values, int count, Stream output)
            {
                for (int i = 0; i < count; i++)
                {
                    output.WriteByte((byte)values[i]);
                }
            }

            static void WriteFileHeader(Stream output, Image image)
            {
                WriteBytes(new char[] { 'B', 'M' }, 2, output);
                uint num = (uint)(((image.Width * image.Height) * 2) + 70);
                output.Write(BitConverter.GetBytes(num), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0), 0, 4);
                output.Write(BitConverter.GetBytes((uint)70), 0, 4);
            }

            private static void WriteInfoHeader(Stream output, Image input)
            {
                output.Write(BitConverter.GetBytes((uint)0x38), 0, 4);
                output.Write(BitConverter.GetBytes(input.Width), 0, 4);
                output.Write(BitConverter.GetBytes(input.Height), 0, 4);
                output.Write(BitConverter.GetBytes((ushort)1), 0, 2);
                output.Write(BitConverter.GetBytes((ushort)0x10), 0, 2);
                output.Write(BitConverter.GetBytes((uint)3), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0x62), 0, 4);
                output.Write(BitConverter.GetBytes(0x2e20), 0, 4);
                output.Write(BitConverter.GetBytes(0x2e20), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0x7c00), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0x3e0), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0x1f), 0, 4);
                output.Write(BitConverter.GetBytes((uint)0x8000), 0, 4);
            }

            public struct IH_ColorMask
            {
                public uint R;
                public uint G;
                public uint B;
                public uint Mask;
            }

            public enum IH_Compression
            {
                BI_RGB,
                BI_RLE8,
                BI_RLE4,
                BI_BITFIELDS
            }

            public struct VbpFileHeader
            {
                public char[] Type;
                public uint Size;
                public uint Reserved;
                public uint OffBits;
            }

            public struct VbpInformationHeader
            {
                public uint Size;
                public int Width;
                public int Height;
                public ushort Planes;
                public ushort BitCount;
                public VbpFile.IH_Compression Compression;
                public uint SizeImage;
                public int XPelsPerMeter;
                public int YPelsPerMeter;
                public uint ClrUsed;
                public uint ClrImportant;
                public VbpFile.IH_ColorMask ColorMask;
            }
        }
        #endregion

    }
}
