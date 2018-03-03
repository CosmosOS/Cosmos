﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cosmos.System.Graphics
{
    public class Bitmap : Image
    {
        public Bitmap(uint Width, uint Height, ColorDepth colorDepth) : base(Width, Height, colorDepth)
        {
            rawData = new int[Width * Height];
        }

        public void Save(string path)
        {
            using (FileStream fs = File.Open(path, FileMode.Create))
            {
                Save(fs, ImageFormat.bmp);
            }
        }

        public void Save(Stream stream, ImageFormat imageFormat)
        {
            Byte[] file = new Byte[54 /*header*/ + width * height * (uint)depth / 8 /*assume that it is full bytes */];

            //Set signature
            byte[] data = BitConverter.GetBytes(0x4D42);
            stream.Write(data, 0, 2);
            //Write apporiximate file size
            data = BitConverter.GetBytes(54 /*header*/ + width * height * (uint)depth / 8 /*assume that it is full bytes */);
            stream.Write(data, 0, data.Length);
            //Leave bytes 6 -> 10 empty
            data = new Byte[] { 0, 0, 0, 0 };
            stream.Write(data, 0, 4);
            //Offset to start of iamge data
            uint offset = 54;
            data = BitConverter.GetBytes(offset);
            stream.Write(data, 0, 4);
            //Write size of bitmapinfoheader
            data = BitConverter.GetBytes(40);
            stream.Write(data, 0, 4);
            //Width in pixels
            data = BitConverter.GetBytes(width);
            stream.Write(data, 0, 4);
            //Height in pixels
            data = BitConverter.GetBytes(height);
            stream.Write(data, 0, 4);
            //Number of palnes(1)
            data = BitConverter.GetBytes(1);
            stream.Write(data, 0, 2);
            //Bits per pixel
            data = BitConverter.GetBytes((int)depth);
            stream.Write(data, 0, 2);
            //Compression type
            data = BitConverter.GetBytes(0);
            stream.Write(data, 0, 4);
            //Size of image data in bytes
            data = BitConverter.GetBytes(width * height * (uint)depth / 8);
            stream.Write(data, 0, 4);
            //Horizontal resolution in meters (is not accurate)
            data = BitConverter.GetBytes(width / 40);
            stream.Write(data, 0, 4);
            //Vertical resolution in meters (is not accurate)
            data = BitConverter.GetBytes(height / 40);
            stream.Write(data, 0, 4);
            //Number of colors in image /zero
            data = BitConverter.GetBytes(0);
            stream.Write(data, 0, 4);
            //number of important colors in iamge / zero
            data = BitConverter.GetBytes(0);
            stream.Write(data, 0, 4);
            //Finished header

            //Copy image data
            stream.Position = offset;
            Byte[] imageData = new Byte[width * height * (int)depth / 8];
            int imageDataPoint = 0;
            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    data = BitConverter.GetBytes(rawData[x + (height - (1 + y)) * width]);
                    for (int i = 0; i < data.Length; i++)
                    {
                        imageData[i + imageDataPoint] = data[i];
                    }
                    imageDataPoint += data.Length;
                }
            }
            stream.Write(imageData, 0, imageData.Length);

            stream.Flush();
        }

        public static Bitmap CreateBitmap(string path)
        {
            #region BMP Header

            Bitmap bitmap;
            Byte[] text = File.ReadAllBytes(path);
            Byte[] _int = new byte[4];
            Byte[] _short = new byte[2];
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                //Assume that we are using the BMP (Windows) header format
                //I am using http://www.fastgraph.com/help/bmp_header_format.html
                //and https://upload.wikimedia.org/wikipedia/commons/c/c4/BMPfileFormat.png?1519566101894as as reference
                fs.Position = 10;
                //read header - bytes 10 -> 14 is the offset of the bitmap image data
                fs.Read(_int, 0, 4);
                uint pixelTableOffset = (uint)BitConverter.ToInt32(_int, 0);

                //now reading size of BITMAPINFOHEADER should be 40 - bytes 14 -> 18
                fs.Read(_int, 0, 4);
                uint infoHeaderSize = (uint)BitConverter.ToInt32(_int, 0);
                if (infoHeaderSize != 40)
                {
                    throw new Exception("Info header size has the wrong value!");
                }

                //now reading width of image in pixels - bytes 18 -> 22
                fs.Read(_int, 0, 4);
                uint imageWidth = (uint)BitConverter.ToInt32(_int, 0);

                //now reading height of image in pixels - byte 22 -> 26
                fs.Read(_int, 0, 4);
                uint imageHeight = (uint)BitConverter.ToInt32(_int, 0);

                //now reading number of planes should be 1 - byte 26 -> 28
                fs.Read(_short, 0, 2);
                ushort planes = (ushort)BitConverter.ToInt16(_short, 0);
                if (planes != 1)
                    throw new Exception("Number of planes is not 1! Can not read file!");

                //now reading size of bits per pixel (1, 4, 8, 24, 32) - bytes 28 - 30
                fs.Read(_short, 0, 2);
                ushort pixelSize = (ushort)BitConverter.ToInt16(_short, 0);
                //TODO: Be able to handle other pixel sizes
                if (pixelSize != 32)
                {
                    throw new NotImplementedException("Can only handle 32bit pictures!");
                }

                //now reading compression type - bytes 30 -> 34
                fs.Read(_int, 0, 4);
                uint compression = (uint)BitConverter.ToInt32(_int, 0);
                //TODO: Be able to handle compressed files
                if (compression != 0)
                    throw new NotImplementedException("Can only handle uncompressed files!");

                //now reading total image data size(including padding) - bytes 34 -> 38
                fs.Read(_int, 0, 4);
                uint totalImageSize = (uint)BitConverter.ToInt32(_int, 0);
                //Somehow this is 0 for my test bmp

                #endregion BMP Header

                bitmap = new Bitmap(imageWidth, imageHeight, (ColorDepth)pixelSize);

                #region Pixel Table

                //Calculate padding
                int paddingPerRow = 0;
                if (totalImageSize != 0)
                {
                    int pureImageSize = (int)(imageWidth * imageHeight * pixelSize / 8);
                    int remainder = (int)totalImageSize - pureImageSize;
                    if (remainder < 0) throw new Exception("Total Image Size is smaller than pure image size");
                    paddingPerRow = remainder / (int)imageHeight;
                }
                else
                {
                    //total image size is 0 if it is not compressed
                    paddingPerRow = 0;
                }
                //Read data
                fs.Position = (int)pixelTableOffset;
                Byte[] _pixel = new byte[pixelSize / 8]; //Pixel size is in byte

                for (int x = 0; x < imageWidth; x++)
                {
                    for (int y = 0; y < imageHeight; y++)
                    {
                        fs.Read(_pixel, 0, pixelSize / 8);
                        bitmap.rawData[x + (imageHeight - (y + 1)) * imageWidth] = BitConverter.ToInt32(_pixel, 0);
                    }
                    fs.Position += paddingPerRow;
                }

                #endregion Pixel Table
            }
            return bitmap;
        }
    }
}
