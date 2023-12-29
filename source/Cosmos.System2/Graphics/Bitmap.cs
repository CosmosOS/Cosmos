//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.IO;
using System.Security;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Represents a bitmap image.
    /// </summary>
    public class Bitmap : Image
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="colorDepth">The color depth.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either the width or height is lower than 0.</exception>
        public Bitmap(uint width, uint height, ColorDepth colorDepth) : base(width, height, colorDepth)
        {
            if(width < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if(height < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            RawData = new int[width * height];
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Bitmap"/> class from a byte array
        /// representing the pixels.
        /// </summary>
        /// <param name="width">The width of the bitmap.</param>
        /// <param name="height">The height of the bitmap.</param>
        /// <param name="pixelData">A byte array which includes the values for each pixel.</param>
        /// <param name="colorDepth">The format of the pixel data.</param>
        /// <exception cref="NotImplementedException">Thrown if color depth is not 32.</exception>
        /// <exception cref="OverflowException">Thrown if bitmap size is bigger than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public Bitmap(uint width, uint height, byte[] pixelData, ColorDepth colorDepth) : base(width, height, colorDepth)
        {
            RawData = new int[width * height];
            if (colorDepth != ColorDepth.ColorDepth32 && colorDepth != ColorDepth.ColorDepth24)
            {
                Global.Debugger.Send("Only color depths 24 and 32 are supported!");
                throw new NotImplementedException("Only color depths 24 and 32 are supported!");
            }

            for (int i = 0; i < RawData.Length; i++)
            {
                if (colorDepth == ColorDepth.ColorDepth32)
                {
                    RawData[i] = BitConverter.ToInt32(new byte[] { pixelData[i * 4], pixelData[i * 4 + 1], pixelData[i * 4 + 2], pixelData[i * 4 + 3] }, 0);
                }
                else
                {
                    RawData[i] = BitConverter.ToInt32(new byte[] { 0, pixelData[i * 3], pixelData[i * 3 + 1], pixelData[i * 3 + 2] }, 0);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class, using the specified path to a BMP file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if path is invalid.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if path is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The path refers to non-file.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        /// <exception cref="SecurityException">Thrown if the caller does not have permissions to read / write the file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified path is invalid.</exception>
        /// <exception cref="PathTooLongException">Thrown if the specified path is exceed the system-defined max length.</exception>
        public Bitmap(string path) : this(path, ColorOrder.BGR)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class, with a specified path to a BMP file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentException">
        /// <list type="bullet">
        /// <item>Thrown if path is invalid.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <list type="bullet">
        /// <item>Thrown if path is null.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The path refers to non-file.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        /// <exception cref="SecurityException">Thrown if the caller does not have permissions to read / write the file.</exception>
        /// <exception cref="FileNotFoundException">Thrown if the file cannot be found.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified path is invalid.</exception>
        /// <exception cref="PathTooLongException">Thrown if the specified path is exceed the system-defined max length.</exception>
        public Bitmap(string path, ColorOrder colorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            using FileStream fs = new FileStream(path, FileMode.Open);
            CreateBitmap(fs, colorOrder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class, with the specified image data byte array.
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Bitmap(byte[] imageData) : this(imageData, ColorOrder.BGR) //Call the image constructor with wrong values
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class, with the specified image data byte array.
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Bitmap(byte[] imageData, ColorOrder colorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            using var ms = new MemoryStream(imageData);
            CreateBitmap(ms, colorOrder);
        }


        // For more information about the format: https://docs.microsoft.com/en-us/previous-versions/ms969901(v=msdn.10)?redirectedfrom=MSDN
        /// <summary>
        /// Creates a bitmap from the given I/O stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error.</item>
        /// <item>The stream does not support seeking.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1. Can not read file.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        private void CreateBitmap(Stream stream, ColorOrder colorOrder)
        {
            #region BMP Header

            byte[] buffer32 = new byte[4];
            byte[] buffer16 = new byte[2];

            // Assume that we are using the BMP (Windows) V3 header format
            // reading magic number to identify if BMP file (BM as string - 42 4D as Hex) - bytes 0 -> 2
            stream.Read(buffer16, 0, 2);
            if ("42-4D" != BitConverter.ToString(buffer16))
            {
                throw new Exception("Header is not from a BMP");
            }

            // read size of BMP file - byte 2 -> 6
            stream.Read(buffer32, 0, 4);

            stream.Position = 10;
            // read header - bytes 10 -> 14 is the offset of the bitmap image data
            stream.Read(buffer32, 0, 4);
            uint pixelTableOffset = BitConverter.ToUInt32(buffer32, 0);

            // now reading size of BITMAPINFOHEADER should be 40 - bytes 14 -> 18
            stream.Read(buffer32, 0, 4);
            uint infoHeaderSize = BitConverter.ToUInt32(buffer32, 0);
            if (infoHeaderSize is not 40 and not 56 and not 124) // 124 - is BITMAPV5INFOHEADER, 56 - is BITMAPV3INFOHEADER, where we ignore the additional values see https://web.archive.org/web/20150127132443/https://forums.adobe.com/message/3272950
            {
                throw new Exception("The information header size is incorrect.");
            }

            // Now reading width of image in pixels - bytes 18 -> 22
            stream.Read(buffer32, 0, 4);
            uint imageWidth = BitConverter.ToUInt32(buffer32, 0);

            // Now reading height of image in pixels - byte 22 -> 26
            stream.Read(buffer32, 0, 4);
            uint imageHeight = BitConverter.ToUInt32(buffer32, 0);

            // Now reading number of planes should be 1 - byte 26 -> 28
            stream.Read(buffer16, 0, 2);
            ushort planes = BitConverter.ToUInt16(buffer16, 0);
            if (planes != 1)
            {
                throw new Exception("The number of planes is not 1.");
            }

            // Now reading size of bits per pixel (1, 4, 8, 24, 32) - bytes 28 - 30
            stream.Read(buffer16, 0, 2);
            ushort pixelSize = BitConverter.ToUInt16(buffer16, 0);

            //TODO: Be able to handle other pixel sizes
            if (pixelSize is not (32 or 24))
            {
                throw new NotImplementedException("The current implementation can only handle 32-bit or 24-bit bitmaps.");
            }

            //now reading compression type - bytes 30 -> 34
            stream.Read(buffer32, 0, 4);
            uint compression = BitConverter.ToUInt32(buffer32, 0);

            // TODO: Be able to handle compressed files
            if (compression is not 0 and not 3) //3 is BI_BITFIELDS again ignore for now is for Adobe Images
            {
                //Global.mDebugger.Send("Can only handle uncompressed files!");
                throw new NotImplementedException("Bitmap compression is not supported.");
            }

            // Now reading total image data size(including padding) - bytes 34 -> 38
            stream.Read(buffer32, 0, 4);
            uint totalImageSize = BitConverter.ToUInt32(buffer32, 0);
            if (totalImageSize == 0)
            {
                totalImageSize = (uint)((((imageWidth * pixelSize) + 31) & ~31) >> 3) * imageHeight; // Look at the link above for the explanation
            }

            #endregion BMP Header

            //Set the bitmap to have the correct values
            Width = imageWidth;
            Height = imageHeight;
            Depth = (ColorDepth)pixelSize;

            RawData = new int[Width * Height];

            #region Pixel Table

            // Calculate padding
            int paddingPerRow;
            int pureImageSize = (int)(imageWidth * imageHeight * pixelSize / 8);
            if (totalImageSize != 0)
            {
                int remainder = (int)totalImageSize - pureImageSize;
                if (remainder < 0)
                {
                    throw new Exception("The total image size is smaller than the pure image size.");
                }

                paddingPerRow = remainder / (int)imageHeight;
                pureImageSize = (int)totalImageSize;
            }
            else
            {
                // total image size is 0 if it is not compressed
                paddingPerRow = 0;
            }

            // Read data
            stream.Position = (int)pixelTableOffset;
            int position = 0;
            byte[] pixelData = new byte[pureImageSize];
            stream.Read(pixelData, 0, pureImageSize);
            byte[] pixel = new byte[4]; // All must have the same size

            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    if (pixelSize == 32)
                    {
                        pixel[0] = pixelData[position++];
                        pixel[1] = pixelData[position++];
                        pixel[2] = pixelData[position++];
                        pixel[3] = pixelData[position++];
                    }
                    else
                    {
                        if(colorOrder == ColorOrder.BGR)
                        {
                            pixel[3] = pixelData[position++];
                            pixel[2] = pixelData[position++];
                            pixel[1] = pixelData[position++];
                            pixel[0] = 0;
                        }
                        else
                        {
                            pixel[0] = pixelData[position++];
                            pixel[1] = pixelData[position++];
                            pixel[2] = pixelData[position++];
                            pixel[3] = 0;
                        }
                    }

                    // fix color mix bug

                    long pixelPosition = x + (imageHeight - (y + 1)) * imageWidth;

                    Color color = Color.FromArgb(BitConverter.ToInt32(pixel, 0));
                    color = Color.FromArgb(255, color.G, color.R, color.A);

                    RawData[pixelPosition] = color.ToArgb(); //This bits should be A, R, G, B but order is switched
                    
                }
                position += paddingPerRow;
            }

            #endregion Pixel Table
        }

        /// <summary>
        /// Saves the given image as a BMP file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error.</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error.</exception>
        /// <exception cref="InvalidCastException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error.</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error.</exception>
        public void Save(string path)
        {
            using FileStream fs = File.Open(path, FileMode.Create);
            Save(fs, ImageFormat.BMP);
        }

        /// <summary>
        /// Saves the image to the given stream.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="imageFormat">The format to save the image with.</param>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error.</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error.</exception>
        /// <exception cref="InvalidCastException">Thrown on fatal error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown if the stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        public void Save(Stream stream, ImageFormat imageFormat = ImageFormat.BMP)
        {
            //Calculate padding
            int padding = 4 - ((int)Width * (int)Depth % 32 / 8);
            if (padding == 4)
            {
                padding = 0;
            }

            byte[] file = new byte[54 /*header*/ + (Width * Height * (uint)Depth / 8) + padding * Height];
            // Writes all bytes at the end into the stream, rather than a few every time

            int position = 0;
            // Set signature
            byte[] data = BitConverter.GetBytes(0x4D42);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            // Write apporiximate file size
            data = BitConverter.GetBytes(54 /*header*/ + (Width * Height * (uint)Depth / 8) /*assume that it is full bytes */);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Leave bytes 6 -> 10 empty
            data = new byte[] { 0, 0, 0, 0 };
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Offset to start of image data
            uint offset = 54;
            data = BitConverter.GetBytes(offset);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Write size of bitmapinfoheader
            data = BitConverter.GetBytes(40);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Width in pixels
            data = BitConverter.GetBytes(Width);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Height in pixels
            data = BitConverter.GetBytes(Height);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Number of planes(1)
            data = BitConverter.GetBytes(1);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            // Bits per pixel
            data = BitConverter.GetBytes((int)Depth);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            // Compression type
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Size of image data in bytes
            data = BitConverter.GetBytes(Width * Height * (uint)Depth / 8);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Horizontal resolution in meters (is not accurate)
            data = BitConverter.GetBytes(Width / 40);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            // Vertical resolution in meters (is not accurate)
            data = BitConverter.GetBytes(Height / 40);
            Array.Copy(data, 0, file, position, 0);
            position += 4;

            // Number of colors in image /zero
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 0);
            position += 4;

            // number of important colors in image / zero
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 4);

            // Finished header

            // Copy image data
            position = (int)offset;
            int byteNum = (int)Depth / 8;
            byte[] imageData = new byte[(Width * Height * byteNum) + padding * Height];
            int imageDataPoint = 0;
            int cOffset = 4 - ((int)Depth / 8);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    data = BitConverter.GetBytes(RawData[x + (Height - (y + 1)) * Width]);
                    for (int i = 0; i < byteNum; i++)
                    {
                        imageData[imageDataPoint++] = data[i + cOffset];
                    }
                }
                imageDataPoint += padding;
            }
            Array.Copy(imageData, 0, file, position, imageData.Length);
            stream.Write(file, 0, file.Length);
        }
    }
}
