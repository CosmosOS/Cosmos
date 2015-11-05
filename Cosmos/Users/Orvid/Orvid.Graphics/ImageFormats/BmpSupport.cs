using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class BmpImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            BmpInternals.BmpEncoder b = new BmpInternals.BmpEncoder();
            b.Encode(i, dest);
        }

        public override Image Load(Stream s)
        {
            BmpInternals.BmpDecoder b = new BmpInternals.BmpDecoder();
            return b.Decode(s);
        }

        // Please note, everything below this
        // point was originally from the ImageTools 
        // Library, available here:
        // http://imagetools.codeplex.com/
        //
        //
        // The source has been modified for use in this library.
        // 
        // This disclaimer was last
        // modified on August 9, 2011.

        #region Internals
        private static class BmpInternals
        {
            #region BmpDecoder
            public class BmpDecoder
            {
                /// <summary>
                /// The mask for the red part of the color for 16 bit rgb bitmaps.
                /// </summary>
                private const int Rgb16RMask = 0x00007C00;
                /// <summary>
                /// The mask for the green part of the color for 16 bit rgb bitmaps.
                /// </summary>
                private const int Rgb16GMask = 0x000003E0;
                /// <summary>
                /// The mask for the blue part of the color for 16 bit rgb bitmaps.
                /// </summary>
                private const int Rgb16BMask = 0x0000001F;

                private Stream _stream;
                private BmpFileHeader _fileHeader;
                private BmpInfoHeader _infoHeader;

                /// <summary>
                /// Decodes the image from the specified _stream and sets
                /// the data to image.
                /// </summary>
                /// <param name="image">The image, where the data should be set to.
                /// Cannot be null (Nothing in Visual Basic).</param>
                /// <param name="stream">The _stream, where the image should be
                /// decoded from. Cannot be null (Nothing in Visual Basic).</param>
                /// <exception cref="ArgumentNullException">
                /// 	<para><paramref name="image"/> is null (Nothing in Visual Basic).</para>
                /// 	<para>- or -</para>
                /// 	<para><paramref name="stream"/> is null (Nothing in Visual Basic).</para>
                /// </exception>
                public Image Decode(Stream stream)
                {
                    _stream = stream;

                    try
                    {
                        ReadFileHeader();
                        ReadInfoHeader();

                        int colorMapSize = -1;

                        if (_infoHeader.ClrUsed == 0)
                        {
                            if (_infoHeader.BitsPerPixel == 1 ||
                                _infoHeader.BitsPerPixel == 4 ||
                                _infoHeader.BitsPerPixel == 8)
                            {
                                colorMapSize = (int)Math.Pow(2, _infoHeader.BitsPerPixel) * 4;
                            }
                        }
                        else
                        {
                            colorMapSize = _infoHeader.ClrUsed * 4;
                        }

                        byte[] palette = null;
                        if (colorMapSize > 0)
                        {
                            palette = new byte[colorMapSize];

                            _stream.Read(palette, 0, colorMapSize);
                        }

                        byte[] imageData = new byte[_infoHeader.Width * _infoHeader.Height * 4];

                        switch (_infoHeader.Compression)
                        {
                            case BmpCompression.RGB:
                                if (_infoHeader.HeaderSize != 40)
                                {
                                    throw new Exception("Header Size value '" + _infoHeader.HeaderSize.ToString() + "' is not valid.");
                                }

                                if (_infoHeader.BitsPerPixel == 32)
                                {
                                    ReadRgb32(imageData, _infoHeader.Width, _infoHeader.Height);
                                }
                                else if (_infoHeader.BitsPerPixel == 24)
                                {
                                    ReadRgb24(imageData, _infoHeader.Width, _infoHeader.Height);
                                }
                                else if (_infoHeader.BitsPerPixel == 16)
                                {
                                    ReadRgb16(imageData, _infoHeader.Width, _infoHeader.Height);
                                }
                                else if (_infoHeader.BitsPerPixel <= 8)
                                {
                                    ReadRgbPalette(imageData, palette,
                                        _infoHeader.ImageSize,
                                        _infoHeader.Width,
                                        _infoHeader.Height,
                                        _infoHeader.BitsPerPixel);
                                }
                                break;
                            default:
                                throw new NotSupportedException("Does not support this kind of bitmap files.");
                        }

                        Image i = new Image(_infoHeader.Width, _infoHeader.Height);
                        int indx = 0;
                        byte r, g, b, a;
                        for (uint y = 0; y < i.Height; y++)
                        {
                            for (uint x = 0; x < i.Width; x++)
                            {
                                r = imageData[indx];
                                indx++;
                                g = imageData[indx];
                                indx++;
                                b = imageData[indx];
                                indx++;
                                a = imageData[indx];
                                indx++;
                                i.SetPixel(x, y, new Pixel(r, g, b, a));
                            }
                        }
                        imageData = null;
                        System.GC.Collect();
                        return i;
                    }
                    catch (IndexOutOfRangeException e)
                    {
                        throw new Exception("Bitmap does not have a valid format.", e);
                    }
                }

                private void ReadRgbPalette(byte[] imageData, byte[] colors, int size, int width, int height, int bits)
                {
                    // Pixels per byte (bits per pixel)
                    int ppb = 8 / bits;

                    int arrayWidth = (width + ppb - 1) / ppb;

                    // Bit mask
                    int mask = (0xFF >> (8 - bits));

                    byte[] data = new byte[size];
                    _stream.Read(data, 0, size);

                    // Rows are aligned on 4 byte boundaries
                    int alignment = arrayWidth % 4;
                    if (alignment != 0)
                    {
                        alignment = 4 - alignment;
                    }

                    int offset, row, rowOffset, colOffset, arrayOffset;

                    for (int y = 0; y < height; y++)
                    {
                        rowOffset = y * (arrayWidth + alignment);

                        for (int x = 0; x < arrayWidth; x++)
                        {
                            offset = rowOffset + x;

                            // Revert the y value, because bitmaps are saved from down to top
                            row = Invert(y, height);

                            colOffset = x * ppb;

                            for (int shift = 0; shift < ppb && (colOffset + shift) < width; shift++)
                            {
                                int colorIndex = ((data[offset]) >> (8 - bits - (shift * bits))) & mask;

                                arrayOffset = (row * width + (colOffset + shift)) * 4;
                                imageData[arrayOffset + 0] = colors[colorIndex * 4 + 2];
                                imageData[arrayOffset + 1] = colors[colorIndex * 4 + 1];
                                imageData[arrayOffset + 2] = colors[colorIndex * 4 + 0];

                                imageData[arrayOffset + 3] = (byte)255;

                            }
                        }
                    }
                }

                private void ReadRgb16(byte[] imageData, int width, int height)
                {
                    byte r, g, b;

                    int scaleR = 256 / 32;
                    int scaleG = 256 / 64;

                    int alignment = 0;
                    byte[] data = GetImageArray(width, height, 2, ref alignment);

                    int offset, row, rowOffset, arrayOffset;

                    for (int y = 0; y < height; y++)
                    {
                        rowOffset = y * (width * 2 + alignment);

                        // Revert the y value, because bitmaps are saved from down to top
                        row = Invert(y, height);

                        for (int x = 0; x < width; x++)
                        {
                            offset = rowOffset + x * 2;

                            short temp = BitConverter.ToInt16(data, offset);

                            r = (byte)(((temp & Rgb16RMask) >> 11) * scaleR);
                            g = (byte)(((temp & Rgb16GMask) >> 5) * scaleG);
                            b = (byte)(((temp & Rgb16BMask)) * scaleR);

                            arrayOffset = (row * width + x) * 4;
                            imageData[arrayOffset + 0] = r;
                            imageData[arrayOffset + 1] = g;
                            imageData[arrayOffset + 2] = b;

                            imageData[arrayOffset + 3] = (byte)255;
                        }
                    }
                }

                private void ReadRgb24(byte[] imageData, int width, int height)
                {
                    int alignment = 0;
                    byte[] data = GetImageArray(width, height, 3, ref alignment);

                    int offset, row, rowOffset, arrayOffset;

                    for (int y = 0; y < height; y++)
                    {
                        rowOffset = y * (width * 3 + alignment);

                        // Revert the y value, because bitmaps are saved from down to top
                        row = Invert(y, height);

                        for (int x = 0; x < width; x++)
                        {
                            offset = rowOffset + x * 3;

                            arrayOffset = (row * width + x) * 4;
                            imageData[arrayOffset + 0] = data[offset + 2];
                            imageData[arrayOffset + 1] = data[offset + 1];
                            imageData[arrayOffset + 2] = data[offset + 0];

                            imageData[arrayOffset + 3] = (byte)255;
                        }
                    }
                }

                private void ReadRgb32(byte[] imageData, int width, int height)
                {
                    int alignment = 0;
                    byte[] data = GetImageArray(width, height, 4, ref alignment);

                    int offset, row, rowOffset, arrayOffset;

                    for (int y = 0; y < height; y++)
                    {
                        rowOffset = y * (width * 4 + alignment);

                        // Revert the y value, because bitmaps are saved from down to top
                        row = Invert(y, height);

                        for (int x = 0; x < width; x++)
                        {
                            offset = rowOffset + x * 4;

                            arrayOffset = (row * width + x) * 4;
                            imageData[arrayOffset + 0] = data[offset + 2];
                            imageData[arrayOffset + 1] = data[offset + 1];
                            imageData[arrayOffset + 2] = data[offset + 0];

                            imageData[arrayOffset + 3] = (byte)255;
                        }
                    }
                }

                private static int Invert(int y, int height)
                {
                    int row = 0;

                    if (height > 0)
                    {
                        row = (height - y - 1);
                    }
                    else
                    {
                        row = y;
                    }

                    return row;
                }

                private byte[] GetImageArray(int width, int height, int bytes, ref int alignment)
                {
                    int dataWidth = width;

                    alignment = (width * bytes) % 4;
                    if (alignment != 0)
                    {
                        alignment = 4 - alignment;
                    }

                    int size = (dataWidth * bytes + alignment) * height;

                    byte[] data = new byte[size];
                    _stream.Read(data, 0, size);

                    return data;
                }

                private void ReadInfoHeader()
                {
                    byte[] data = new byte[BmpInfoHeader.Size];

                    _stream.Read(data, 0, BmpInfoHeader.Size);

                    _infoHeader = new BmpInfoHeader();
                    _infoHeader.HeaderSize = BitConverter.ToInt32(data, 0);
                    _infoHeader.Width = BitConverter.ToInt32(data, 4);
                    _infoHeader.Height = BitConverter.ToInt32(data, 8);
                    _infoHeader.Planes = BitConverter.ToInt16(data, 12);
                    _infoHeader.BitsPerPixel = BitConverter.ToInt16(data, 14);
                    _infoHeader.ImageSize = BitConverter.ToInt32(data, 20);
                    _infoHeader.XPelsPerMeter = BitConverter.ToInt32(data, 24);
                    _infoHeader.YPelsPerMeter = BitConverter.ToInt32(data, 28);
                    _infoHeader.ClrUsed = BitConverter.ToInt32(data, 32);
                    _infoHeader.ClrImportant = BitConverter.ToInt32(data, 36);
                    _infoHeader.Compression = (BmpCompression)BitConverter.ToInt32(data, 16);
                }

                private void ReadFileHeader()
                {
                    byte[] data = new byte[BmpFileHeader.Size];

                    _stream.Read(data, 0, BmpFileHeader.Size);

                    _fileHeader = new BmpFileHeader();
                    _fileHeader.Type = BitConverter.ToInt16(data, 0);
                    _fileHeader.FileSize = BitConverter.ToInt32(data, 2);
                    _fileHeader.Reserved = BitConverter.ToInt32(data, 6);
                    _fileHeader.Offset = BitConverter.ToInt32(data, 10);
                }

            }
            #endregion

            #region BmpEncoder
            public class BmpEncoder
            {
                public void Encode(Image image, Stream stream)
                {
                    int rowWidth = image.Width;

                    int amount = (image.Width * 3) % 4;
                    if (amount != 0)
                    {
                        rowWidth += 4 - amount;
                    }

                    BinaryWriter writer = new BinaryWriter(stream);

                    BmpFileHeader fileHeader = new BmpFileHeader();
                    fileHeader.Type = 19778;
                    fileHeader.Offset = 54;
                    fileHeader.FileSize = 54 + image.Height * rowWidth * 3;
                    Write(writer, fileHeader);

                    BmpInfoHeader infoHeader = new BmpInfoHeader();
                    infoHeader.HeaderSize = 40;
                    infoHeader.Height = image.Height;
                    infoHeader.Width = image.Width;
                    infoHeader.BitsPerPixel = 24;
                    infoHeader.Planes = 1;
                    infoHeader.Compression = BmpCompression.RGB;
                    infoHeader.ImageSize = image.Height * rowWidth * 3;
                    infoHeader.ClrUsed = 0;
                    infoHeader.ClrImportant = 0;
                    Write(writer, infoHeader);

                    WriteImage(writer, image);

                    writer.Flush();
                }

                private static void WriteImage(BinaryWriter writer, Image image)
                {
                    int amount = (image.Width * 3) % 4, offset = 0;
                    if (amount != 0)
                    {
                        amount = 4 - amount;
                    }

                    byte[] data = ConvertPixelArrayToByteArray(image.Data);

                    for (int y = image.Height - 1; y >= 0; y--)
                    {
                        for (int x = 0; x < image.Width; x++)
                        {
                            offset = (y * image.Width + x) * 4;

                            writer.Write(data[offset + 2]);
                            writer.Write(data[offset + 1]);
                            writer.Write(data[offset + 0]);
                        }

                        for (int i = 0; i < amount; i++)
                        {
                            writer.Write((byte)0);
                        }
                    }
                }

                private static byte[] ConvertPixelArrayToByteArray(Pixel[] a)
                {
                    byte[] b = new byte[a.Length * 4];

                    int indx = 0;
                    Pixel p;
                    for (uint i = 0; i < a.Length; i++)
                    {
                        p = a[i];
                        b[indx] = p.R;
                        indx++;
                        b[indx] = p.G;
                        indx++;
                        b[indx] = p.B;
                        indx++;
                        b[indx] = p.A;
                        indx++;
                    }

                    return b;
                }

                private static void Write(BinaryWriter writer, BmpFileHeader fileHeader)
                {
                    writer.Write(fileHeader.Type);
                    writer.Write(fileHeader.FileSize);
                    writer.Write(fileHeader.Reserved);
                    writer.Write(fileHeader.Offset);
                }

                private static void Write(BinaryWriter writer, BmpInfoHeader infoHeader)
                {
                    writer.Write(infoHeader.HeaderSize);
                    writer.Write(infoHeader.Width);
                    writer.Write(infoHeader.Height);
                    writer.Write(infoHeader.Planes);
                    writer.Write(infoHeader.BitsPerPixel);
                    writer.Write((int)infoHeader.Compression);
                    writer.Write(infoHeader.ImageSize);
                    writer.Write(infoHeader.XPelsPerMeter);
                    writer.Write(infoHeader.YPelsPerMeter);
                    writer.Write(infoHeader.ClrUsed);
                    writer.Write(infoHeader.ClrImportant);
                }
            }
            #endregion

            #region BitmapCompression
            private enum BmpCompression : int
            {
                /// <summary>
                /// Each image row has a multiple of four elements. If the 
                /// row has less elements, zeros will be added at the right side.
                /// The format depends on the number of bits, stored in the info header.
                /// If the number of bits are one, four or eight each pixel data is 
                /// a index to the palette. If the number of bits are sixteen, 
                /// twenty-four or thirtee-two each pixel contains a color.
                /// </summary>
                RGB = 0,
                /// <summary>
                /// Two bytes are one data record. If the first byte is not zero, the 
                /// next two half bytes will be repeated as much as the value of the first byte.
                /// If the first byte is zero, the record has different meanings, depending
                /// on the second byte. If the second byte is zero, it is the end of the row,
                /// if it is one, it is the end of the image.
                /// Not supported at the moment.
                /// </summary>
                RLE8 = 1,
                /// <summary>
                /// Two bytes are one data record. If the first byte is not zero, the 
                /// next byte will be repeated as much as the value of the first byte.
                /// If the first byte is zero, the record has different meanings, depending
                /// on the second byte. If the second byte is zero, it is the end of the row,
                /// if it is one, it is the end of the image.
                /// Not supported at the moment.
                /// </summary>
                RLE4 = 2,
                /// <summary>
                /// Each image row has a multiple of four elements. If the 
                /// row has less elements, zeros will be added at the right side.
                /// Not supported at the moment.
                /// </summary>
                BitFields = 3,
                /// <summary>
                /// The bitmap contains a JPG image. 
                /// Not supported at the moment.
                /// </summary>
                JPEG = 4,
                /// <summary>
                /// The bitmap contains a PNG image. 
                /// Not supported at the moment.
                /// </summary>
                PNG = 5
            }
            #endregion

            #region BmpFileHeader
            private class BmpFileHeader
            {
                /// <summary>
                /// Defines of the data structure in the bitmap file.
                /// </summary>
                public const int Size = 14;

                /// <summary>
                /// The magic number used to identify the bitmap file: 0x42 0x4D 
                /// (Hex code points for B and M)
                /// </summary>
                public short Type;
                /// <summary>
                /// The size of the bitmap file in bytes.
                /// </summary>
                public int FileSize;
                /// <summary>
                /// Reserved; actual value depends on the application 
                /// that creates the image.
                /// </summary>
                public int Reserved;
                /// <summary>
                /// The offset, i.e. starting address, of the byte where 
                /// the bitmap data can be found.
                /// </summary>
                public int Offset;
            }
            #endregion

            #region BmpInfoHeader
            private class BmpInfoHeader
            {
                /// <summary>
                /// Defines of the data structure in the bitmap file.
                /// </summary>
                public const int Size = 40;

                /// <summary>
                /// The size of this header (40 bytes)
                /// </summary>
                public int HeaderSize;
                /// <summary>
                /// The bitmap width in pixels (signed integer).
                /// </summary>
                public int Width;
                /// <summary>
                /// The bitmap height in pixels (signed integer).
                /// </summary>
                public int Height;
                /// <summary>
                /// The number of color planes being used. Must be set to 1.
                /// </summary>
                public short Planes;
                /// <summary>
                /// The number of bits per pixel, which is the color depth of the image. 
                /// Typical values are 1, 4, 8, 16, 24 and 32.
                /// </summary>
                public short BitsPerPixel;
                /// <summary>
                /// The compression method being used. 
                /// See the next table for a list of possible values.
                /// </summary>
                public BmpCompression Compression;
                /// <summary>
                /// The image size. This is the size of the raw bitmap data (see below), 
                /// and should not be confused with the file size.
                /// </summary>
                public int ImageSize;
                /// <summary>
                /// The horizontal resolution of the image. 
                /// (pixel per meter, signed integer)
                /// </summary>
                public int XPelsPerMeter;
                /// <summary>
                /// The vertical resolution of the image. 
                /// (pixel per meter, signed integer)
                /// </summary>
                public int YPelsPerMeter;
                /// <summary>
                /// The number of colors in the color palette, 
                /// or 0 to default to 2^n.
                /// </summary>
                public int ClrUsed;
                /// <summary>
                /// The number of important colors used, 
                /// or 0 when every color is important; generally ignored.
                /// </summary>
                public int ClrImportant;
            }
            #endregion

        }
        #endregion

    }
}
