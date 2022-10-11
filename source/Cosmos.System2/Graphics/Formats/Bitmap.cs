//#define COSMOSDEBUG
using System;
using System.IO;
using System.Security;
using Cosmos.System.Graphics.Extensions;

namespace Cosmos.System.Graphics.Formats
{
    /// <summary>
    /// Bitmap class, used to represent image of the type of Bitmap. See also: <seealso cref="Image"/>.
    /// </summary>
    public class Bitmap : Canvas
    {
        /// <summary>
        /// Create a bitmap from a byte array representing the pixels.
        /// </summary>
        /// <param name="Width">Width of the bitmap.</param>
        /// <param name="Height">Height of the bitmap.</param>
        /// <param name="pixelData">Byte array which includes the values for each pixel.</param>
        /// <param name="colorDepth">Format of pixel data.</param>
        /// <exception cref="NotImplementedException">Thrwon if color depth is not 32.</exception>
        /// <exception cref="OverflowException">Thrown if bitmap size is bigger than Int32.MaxValue.</exception>
        /// <exception cref="ArgumentException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        public Bitmap(uint aWidth, uint aHeight, byte[] aPixelData, ColorDepth aColorDepth) : base(aWidth, aHeight, aColorDepth)
        {
            if (aColorDepth != ColorDepth.ColorDepth32 && aColorDepth != ColorDepth.ColorDepth24)
            {
                Global.mDebugger.Send("Only color depths 24 and 32 are supported!");
                throw new NotImplementedException("Only color depths 24 and 32 are supported!");
            }
            
            switch (aColorDepth)
            {
                case ColorDepth.ColorDepth32:
                    for (uint I = 0; I < Buffer.Length; I++)
                    {
                        Buffer[I] = BitConverter.ToUInt32(new byte[]
                        {
                            aPixelData[I * 4],
                            aPixelData[(I * 4) + 1],
                            aPixelData[(I * 4) + 2],
                            aPixelData[(I * 4) + 3]
                        });
                    }
                    break;
                case ColorDepth.ColorDepth24:
                    for (uint I = 0; I < Buffer.Length; I++)
                    {
                        Buffer[I] = BitConverter.ToUInt32(new byte[]
                        {
                            255, // Alpha chanel set to 255
                            aPixelData[I * 3],
                            aPixelData[(I * 3) + 1],
                            aPixelData[(I * 3) + 2]
                        });
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Create new instance of the <see cref="Bitmap"/> class, with a specified path to a BMP file.
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error (contact support).</item>
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
        public Bitmap(string aPath) : this(aPath, ColorOrder.BGR)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="Bitmap"/> class, with a specified path to a BMP file.
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
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error (contact support).</item>
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
        public Bitmap(string aPath, ColorOrder aColorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            using var fs = new FileStream(aPath, FileMode.Open);
            CreateBitmap(fs, aColorOrder);
        }
        
        /// <summary>
        /// Create new instance of the <see cref="Bitmap"/> class, with a specified image data byte array. 
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Bitmap(byte[] aImageData) : this(aImageData, ColorOrder.BGR) //Call the image constructor with wrong values
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="Bitmap"/> class, with a specified image data byte array. 
        /// </summary>
        /// <param name="imageData">byte array.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentNullException">Thrown if imageData is null / memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown if header is not from a BMP.</item>
        /// <item>Info header size has the wrong value.</item>
        /// <item>Number of planes is not 1.</item>
        /// <item>Total Image Size is smaller than pure image size.</item>
        /// </list>
        /// </exception>
        /// <exception cref="NotImplementedException">Thrown if pixelsize is other then 32 / 24 or the file compressed.</exception>
        public Bitmap(byte[] aImageData, ColorOrder aColorOrder = ColorOrder.BGR) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            using var ms = new MemoryStream(aImageData);
            CreateBitmap(ms, aColorOrder);
        }


        // For more information about the format: https://docs.microsoft.com/en-us/previous-versions/ms969901(v=msdn.10)?redirectedfrom=MSDN
        /// <summary>
        /// Create bitmap from stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="colorOrder">Order of colors in each pixel.</param>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">
        /// <list type="bullet">
        /// <item>Thrown on fatal error (contact support).</item>
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
        private void CreateBitmap(Stream aStream, ColorOrder aColorOrder)
        {
            #region BMP Header

            byte[] _int = new byte[4];
            byte[] _short = new byte[2];
            //Assume that we are using the BMP (Windows) V3 header format

            //reading magic number to identify if BMP file (BM as string - 42 4D as Hex) - bytes 0 -> 2
            aStream.Read(_short, 0, 2);
            if ("42-4D" != BitConverter.ToString(_short))
            {
                throw new Exception("Header is not from a BMP");
            }

            //read size of BMP file - byte 2 -> 6
            aStream.Read(_int, 0, 4);
            uint fileSize = BitConverter.ToUInt32(_int, 0);

            aStream.Position = 10;
            //read header - bytes 10 -> 14 is the offset of the bitmap image data
            aStream.Read(_int, 0, 4);
            uint pixelTableOffset = BitConverter.ToUInt32(_int, 0);

            //now reading size of BITMAPINFOHEADER should be 40 - bytes 14 -> 18
            aStream.Read(_int, 0, 4);
            uint infoHeaderSize = BitConverter.ToUInt32(_int, 0);
            if (infoHeaderSize != 40 && infoHeaderSize != 56 && infoHeaderSize != 124) // 124 - is BITMAPV5INFOHEADER, 56 - is BITMAPV3INFOHEADER, where we ignore the additional values see https://web.archive.org/web/20150127132443/https://forums.adobe.com/message/3272950
            {
                throw new Exception("Info header size has the wrong value!");
            }
            //now reading width of image in pixels - bytes 18 -> 22
            aStream.Read(_int, 0, 4);
            uint imageWidth = BitConverter.ToUInt32(_int, 0);

            //now reading height of image in pixels - byte 22 -> 26
            aStream.Read(_int, 0, 4);
            uint imageHeight = BitConverter.ToUInt32(_int, 0);

            //now reading number of planes should be 1 - byte 26 -> 28
            aStream.Read(_short, 0, 2);
            ushort planes = BitConverter.ToUInt16(_short, 0);
            if (planes != 1)
            {
                throw new Exception("Number of planes is not 1! Can not read file!");
            }

            //now reading size of bits per pixel (1, 4, 8, 24, 32) - bytes 28 - 30
            aStream.Read(_short, 0, 2);
            ushort pixelSize = BitConverter.ToUInt16(_short, 0);
            //TODO: Be able to handle other pixel sizes
            if (!(pixelSize == 32 || pixelSize == 24))
            {
                throw new NotImplementedException("Can only handle 32bit or 24bit bitmaps!");
            }
            //now reading compression type - bytes 30 -> 34
            aStream.Read(_int, 0, 4);
            uint compression = BitConverter.ToUInt32(_int, 0);
            //TODO: Be able to handle compressed files
            if (compression != 0 && compression != 3) //3 is BI_BITFIELDS again ignore for now is for Adobe Images
            {
                //Global.mDebugger.Send("Can only handle uncompressed files!");
                throw new NotImplementedException("Can only handle uncompressed files!");
            }
            //now reading total image data size(including padding) - bytes 34 -> 38
            aStream.Read(_int, 0, 4);
            uint totalImageSize = BitConverter.ToUInt32(_int, 0);
            if (totalImageSize == 0)
            {
                totalImageSize = (uint)((((imageWidth * pixelSize) + 31) & ~31) >> 3) * imageHeight; // Look at the link above for the explanation
                Global.mDebugger.SendInternal("Calcualted image size: " + totalImageSize);
            }

            #endregion BMP Header

            //Set the bitmap to have the correct values
            Mode = new(imageWidth, imageHeight, (ColorDepth)pixelSize);
            Global.mDebugger.SendInternal("Width: " + Mode.Width);
            Global.mDebugger.SendInternal("Height: " + Mode.Height);
            Global.mDebugger.SendInternal("Depth: " + pixelSize);

            #region Pixel Table

            //Calculate padding
            int paddingPerRow;
            int pureImageSize = (int)(imageWidth * imageHeight * pixelSize / 8);
            if (totalImageSize != 0)
            {
                int remainder = (int)totalImageSize - pureImageSize;
                if (remainder < 0)
                {
                    throw new Exception("Total Image Size is smaller than pure image size");
                }
                paddingPerRow = remainder / (int)imageHeight;
                pureImageSize = (int)totalImageSize;
            }
            else
            {
                //total image size is 0 if it is not compressed
                paddingPerRow = 0;
            }
            //Read data
            stream.Position = (int)pixelTableOffset;
            int position = 0;
            byte[] pixelData = new byte[pureImageSize];
            stream.Read(pixelData, 0, pureImageSize);
            byte[] pixel = new byte[4]; //All must have the same size

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
                    rawData[x + (imageHeight - (y + 1)) * imageWidth] = BitConverter.ToInt32(pixel, 0); //This bits should be A, R, G, B but order is switched
                }
                position += paddingPerRow;
            }

            #endregion Pixel Table
        }

        /// <summary>
        /// Save image as bmp file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ObjectDisposedException">Thrown on fatal error (contact support).</exception>
        public void Save(string aPath)
        {
            using FileStream fs = File.Open(aPath, FileMode.Create);
            Save(fs, ImageFormat.Bitmap);
        }

        /// <summary>
        /// Save image to stream.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="imageFormat">Image format.</param>
        /// <exception cref="ArgumentNullException">Thrown on memory error.</exception>
        /// <exception cref="RankException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArrayTypeMismatchException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="InvalidCastException">Thrown on fatal error (contact support).</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on memory error.</exception>
        /// <exception cref="ArgumentException">Thrown on memory error.</exception>
        /// <exception cref="OverflowException">Thrown on memory error.</exception>
        /// <exception cref="IOException">Thrown on IO error.</exception>
        /// <exception cref="NotSupportedException">Thrown if the stream does not support writing.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the stream is closed.</exception>
        public void Save(Stream aStream, ImageFormat aImageFormat)
        {
            //Calculate padding
            int padding = 4 - (((int)Mode.Width * (int)Mode.ColorDepth) % 32) / 8;
            if (padding == 4)
            {
                padding = 0;
            }
            byte[] file = new byte[54 /*header*/ + Mode.Width * Mode.Height * (uint)Mode.ColorDepth / 8 + padding * Mode.Height];
            //Writes all bytes at the end into the stream, rather than a few every time

            int position = 0;
            //Set signature
            byte[] data = BitConverter.GetBytes(0x4D42);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            //Write apporiximate file size
            data = BitConverter.GetBytes(54 /*header*/ + Mode.Width * Mode.Height * (uint)Mode.ColorDepth / 8 /*assume that it is full bytes */);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Leave bytes 6 -> 10 empty
            data = new byte[] { 0, 0, 0, 0 };
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Offset to start of image data
            uint offset = 54;
            data = BitConverter.GetBytes(offset);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Write size of bitmapinfoheader
            data = BitConverter.GetBytes(40);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Width in pixels
            data = BitConverter.GetBytes(Mode.Width);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Height in pixels
            data = BitConverter.GetBytes(Mode.Height);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Number of planes(1)
            data = BitConverter.GetBytes(1);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            //Bits per pixel
            data = BitConverter.GetBytes((int)Mode.ColorDepth);
            Array.Copy(data, 0, file, position, 2);
            position += 2;

            //Compression type
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Size of image data in bytes
            data = BitConverter.GetBytes(Mode.Width * Mode.Height * (uint)Mode.ColorDepth / 8);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Horizontal resolution in meters (is not accurate)
            data = BitConverter.GetBytes(Mode.Width / 40);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Vertical resolution in meters (is not accurate)
            data = BitConverter.GetBytes(Mode.Height / 40);
            Array.Copy(data, 0, file, position, 0);
            position += 4;

            //Number of colors in image /zero
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 0);
            position += 4;

            //number of important colors in image / zero
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 4);

            //Finished header

            //Copy image data
            position = (int)offset;
            int byteNum = (int)Mode.ColorDepth / 8;
            byte[] imageData = new byte[Mode.Width * Mode.Height * byteNum + padding * Mode.Height];
            int imageDataPoint = 0;
            int cOffset = 4 - (int)Mode.ColorDepth / 8;
            for (int y = 0; y < Mode.Height; y++)
            {
                for (int x = 0; x < Mode.Width; x++)
                {
                    data = BitConverter.GetBytes(Buffer[x + (Mode.Height - (y + 1)) * Mode.Width]);
                    for (int i = 0; i < byteNum; i++)
                    {
                        imageData[imageDataPoint++] = data[i + cOffset];
                    }
                }
                imageDataPoint += padding;
            }
            Array.Copy(imageData, 0, file, position, imageData.Length);
            aStream.Write(file, 0, file.Length);
        }
    }
}
