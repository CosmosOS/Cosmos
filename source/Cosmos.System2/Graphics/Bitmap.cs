using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

namespace Cosmos.System.Graphics
{
    public enum ColorOrder
    {
        RGB,
        BGR,
    }

    public class Bitmap : Image
    {
        // new constructor
        public Bitmap(int width, int height, ColorDepth depth) : base(width, height, depth)
        {
            RawData = new uint[width * height];
        }

        // constructor with raw data
        public Bitmap(int width, int height, uint[] data, ColorDepth depth) : base(width, height, depth)
        {
            if (depth != ColorDepth.ColorDepth24 && depth != ColorDepth.ColorDepth32)
            {
                Global.mDebugger.Send("Only 24-bit and 32-bit bitmaps are supported");
                throw new NotImplementedException("Only 24-bit and 32-bit bitmaps are supported");
            }
            SetData(width, height, data);
        }

        // constructor with color data
        public Bitmap(int width, int height, Color[] data, ColorDepth depth) : base(width, height, depth)
        {
            if (depth != ColorDepth.ColorDepth24 && depth != ColorDepth.ColorDepth32)
            {
                Global.mDebugger.Send("Only 24-bit and 32-bit bitmaps are supported");
                throw new NotImplementedException("Only 24-bit and 32-bit bitmaps are supported");
            }
            SetData(width, height, data);
        }

        // load bitmap fromm file
        public void Load(Stream stream, ColorOrder order) { using (stream) { CreateBitmap(stream, order); } }

        public void Load(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open))
            {
                CreateBitmap(fs, ColorOrder.BGR);
            }
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.OpenOrCreate)) { Save(fs, ImageFormat.BMP); }
        }

        public void Save(Stream stream, ImageFormat imageFormat)
        {
            if (imageFormat == ImageFormat.BMP)
            {
                //Calculate padding
                int padding = 4 - (((int)Width * (int)Depth) % 32) / 8;
                if (padding == 4)
                {
                    padding = 0;
                }
                Byte[] file = new Byte[54 /*header*/ + Width * Height * (uint)Depth / 8 + padding * Height];
                //Writes all bytes at the end into the stream, rather than a few every time

                int position = 0;
                //Set signature
                byte[] data = BitConverter.GetBytes(0x4D42);
                Array.Copy(data, 0, file, position, 2);
                position += 2;

                //Write apporiximate file size
                data = BitConverter.GetBytes(54 /*header*/ + Width * Height * (uint)Depth / 8 /*assume that it is full bytes */);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Leave bytes 6 -> 10 empty
                data = new Byte[] { 0, 0, 0, 0 };
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
                data = BitConverter.GetBytes(Width);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Height in pixels
                data = BitConverter.GetBytes(Height);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Number of planes(1)
                data = BitConverter.GetBytes(1);
                Array.Copy(data, 0, file, position, 2);
                position += 2;

                //Bits per pixel
                data = BitConverter.GetBytes((int)Depth);
                Array.Copy(data, 0, file, position, 2);
                position += 2;

                //Compression type
                data = BitConverter.GetBytes(0);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Size of image data in bytes
                data = BitConverter.GetBytes(Width * Height * (uint)Depth / 8);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Horizontal resolution in meters (is not accurate)
                data = BitConverter.GetBytes(Width / 40);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Vertical resolution in meters (is not accurate)
                data = BitConverter.GetBytes(Height / 40);
                Array.Copy(data, 0, file, position, 0);
                position += 4;

                //Number of colors in image /zero
                data = BitConverter.GetBytes(0);
                Array.Copy(data, 0, file, position, 0);
                position += 4;

                //number of important colors in image / zero
                data = BitConverter.GetBytes(0);
                Array.Copy(data, 0, file, position, 4);
                position += 4;

                //Finished header

                //Copy image data
                position = (int)offset;
                int byteNum = (int)Depth / 8;
                byte[] imageData = new byte[Width * Height * byteNum + padding * Height];
                int imageDataPoint = 0;
                int cOffset = 4 - (int)Depth / 8;
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

        // create bitmap from stream
        private void CreateBitmap(Stream stream, ColorOrder colorOrder)
        {
            #region BMP Header

            byte[] _int = new byte[4];
            byte[] _short = new byte[2];
            //Assume that we are using the BMP (Windows) V3 header format

            //reading magic number to identify if BMP file (BM as string - 42 4D as Hex) - bytes 0 -> 2
            stream.Read(_short, 0, 2);
            if ("42-4D" != BitConverter.ToString(_short))
            {
                throw new Exception("Header is not from a BMP");
            }

            //read size of BMP file - byte 2 -> 6
            stream.Read(_int, 0, 4);
            uint fileSize = BitConverter.ToUInt32(_int, 0);

            stream.Position = 10;
            //read header - bytes 10 -> 14 is the offset of the bitmap image data
            stream.Read(_int, 0, 4);
            uint pixelTableOffset = BitConverter.ToUInt32(_int, 0);

            //now reading size of BITMAPINFOHEADER should be 40 - bytes 14 -> 18
            stream.Read(_int, 0, 4);
            uint infoHeaderSize = BitConverter.ToUInt32(_int, 0);
            if (infoHeaderSize != 40 && infoHeaderSize != 56 && infoHeaderSize != 124) // 124 - is BITMAPV5INFOHEADER, 56 - is BITMAPV3INFOHEADER, where we ignore the additional values see https://web.archive.org/web/20150127132443/https://forums.adobe.com/message/3272950
            {
                throw new Exception("Info header size has the wrong value!");
            }
            //now reading width of image in pixels - bytes 18 -> 22
            stream.Read(_int, 0, 4);
            uint imageWidth = BitConverter.ToUInt32(_int, 0);

            //now reading height of image in pixels - byte 22 -> 26
            stream.Read(_int, 0, 4);
            uint imageHeight = BitConverter.ToUInt32(_int, 0);

            //now reading number of planes should be 1 - byte 26 -> 28
            stream.Read(_short, 0, 2);
            ushort planes = BitConverter.ToUInt16(_short, 0);
            if (planes != 1)
            {
                throw new Exception("Number of planes is not 1! Can not read file!");
            }

            //now reading size of bits per pixel (1, 4, 8, 24, 32) - bytes 28 - 30
            stream.Read(_short, 0, 2);
            ushort pixelSize = BitConverter.ToUInt16(_short, 0);
            //TODO: Be able to handle other pixel sizes
            if (!(pixelSize == 32 || pixelSize == 24))
            {
                throw new NotImplementedException("Can only handle 32bit or 24bit bitmaps!");
            }
            //now reading compression type - bytes 30 -> 34
            stream.Read(_int, 0, 4);
            uint compression = BitConverter.ToUInt32(_int, 0);
            //TODO: Be able to handle compressed files
            if (compression != 0 && compression != 3) //3 is BI_BITFIELDS again ignore for now is for Adobe Images
            {
                //Global.mDebugger.Send("Can only handle uncompressed files!");
                throw new NotImplementedException("Can only handle uncompressed files!");
            }
            //now reading total image data size(including padding) - bytes 34 -> 38
            stream.Read(_int, 0, 4);
            uint totalImageSize = BitConverter.ToUInt32(_int, 0);
            if (totalImageSize == 0)
            {
                totalImageSize = (uint)((((imageWidth * pixelSize) + 31) & ~31) >> 3) * imageHeight; // Look at the link above for the explanation
                Global.mDebugger.SendInternal("Calcualted image size: " + totalImageSize);
            }

            #endregion BMP Header

            //Set the bitmap to have the correct values
            Width = (int)imageWidth;
            Height = (int)imageHeight;
            Depth = (ColorDepth)pixelSize;
            Global.mDebugger.SendInternal("Width: " + Width);
            Global.mDebugger.SendInternal("Height: " + Height);
            Global.mDebugger.SendInternal("Depth: " + pixelSize);

            RawData = new uint[Width * Height];

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
                        if (colorOrder == ColorOrder.BGR)
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
                    RawData[x + (imageHeight - (y + 1)) * imageWidth] = BitConverter.ToUInt32(pixel, 0); //This bits should be A, R, G, B but order is switched
                }
                position += paddingPerRow;
            }

            #endregion Pixel Table
        }

        // set data from array of unsigned integers
        public void SetData(int width, int height, uint[] data)
        {
            RawData = new uint[width * height];
            for (int i = 0; i < width * height; i++) { RawData[i] = data[i]; }
        }

        // set data from array of colors
        public void SetData(int width, int height, Color[] data)
        {
            RawData = new uint[width * height];
            for (int i = 0; i < width * height; i++) { RawData[i] = (uint)data[i].ToArgb(); }
        }

        // set pixel
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return; }
            RawData[x + (y * Width)] = (uint)color.ToArgb();
        }

        // get pixel
        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) { return Color.Black; }
            return Color.FromArgb((int)RawData[x + (y * Width)]);
        }
    }
}
