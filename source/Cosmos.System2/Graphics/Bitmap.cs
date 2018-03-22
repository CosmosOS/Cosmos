using System;
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

        public Bitmap(string path) : this(new FileStream(path, FileMode.Open))
        {
        }

        public Bitmap(byte[] imageData) : this(new MemoryStream(imageData))
        {
        }

        public Bitmap(Stream stream) : base(0, 0, ColorDepth.ColorDepth32) //Call the image constructor with wrong values
        {
            using (stream)
            {
                #region BMP Header

                Byte[] _int = new byte[4];
                Byte[] _short = new byte[2];
                //Assume that we are using the BMP (Windows) header format
                //I am using http://www.fastgraph.com/help/bmp_header_format.html
                //and https://upload.wikimedia.org/wikipedia/commons/c/c4/BMPfileFormat.png?1519566101894as as reference
                stream.Position = 10;
                //read header - bytes 10 -> 14 is the offset of the bitmap image data
                stream.Read(_int, 0, 4);
                uint pixelTableOffset = (uint)BitConverter.ToInt32(_int, 0);

                //now reading size of BITMAPINFOHEADER should be 40 - bytes 14 -> 18
                stream.Read(_int, 0, 4);
                uint infoHeaderSize = (uint)BitConverter.ToInt32(_int, 0);
                if (infoHeaderSize != 40)
                {
                    throw new Exception("Info header size has the wrong value!");
                }
                //now reading width of image in pixels - bytes 18 -> 22
                stream.Read(_int, 0, 4);
                uint imageWidth = (uint)BitConverter.ToInt32(_int, 0);

                //now reading height of image in pixels - byte 22 -> 26
                stream.Read(_int, 0, 4);
                uint imageHeight = (uint)BitConverter.ToInt32(_int, 0);

                //now reading number of planes should be 1 - byte 26 -> 28
                stream.Read(_short, 0, 2);
                ushort planes = (ushort)BitConverter.ToInt16(_short, 0);
                if (planes != 1)
                    throw new Exception("Number of planes is not 1! Can not read file!");
                //now reading size of bits per pixel (1, 4, 8, 24, 32) - bytes 28 - 30
                stream.Read(_short, 0, 2);
                ushort pixelSize = (ushort)BitConverter.ToInt16(_short, 0);
                //TODO: Be able to handle other pixel sizes
                if (pixelSize != 32)
                {
                    throw new NotImplementedException("Can only handle 32bit pictures!");
                }
                //now reading compression type - bytes 30 -> 34
                stream.Read(_int, 0, 4);
                uint compression = (uint)BitConverter.ToInt32(_int, 0);
                //TODO: Be able to handle compressed files
                if (compression != 0)
                    throw new NotImplementedException("Can only handle uncompressed files!");
                //now reading total image data size(including padding) - bytes 34 -> 38
                stream.Read(_int, 0, 4);
                uint totalImageSize = (uint)BitConverter.ToInt32(_int, 0);
                //Somehow this is 0 for my test bmp

                #endregion BMP Header

                //Set the bitmap to have the correct values
                Width = imageWidth;
                Height = imageHeight;
                Depth = (ColorDepth)pixelSize;

                rawData = new int[Width * Height * pixelSize / 8];

                #region Pixel Table

                //Calculate padding
                int paddingPerRow = 0;
                int pureImageSize = (int)(imageWidth * imageHeight * pixelSize / 8);
                if (totalImageSize != 0)
                {
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
                stream.Position = (int)pixelTableOffset;
                int position = 0;
                Byte[] pixelData = new byte[pureImageSize];
                stream.Read(pixelData, 0, pureImageSize);
                Byte[] pixel = new byte[pixelSize / 8]; //Pixel size is in byte

                for (int x = 0; x < imageWidth; x++)
                {
                    for (int y = 0; y < imageHeight; y++)
                    {
                        pixel[0] = pixelData[position++];
                        pixel[1] = pixelData[position++];
                        pixel[2] = pixelData[position++];
                        pixel[3] = pixelData[position++];
                        rawData[x + (imageHeight - (y + 1)) * imageWidth] = BitConverter.ToInt32(pixel, 0);
                    }
                    position += paddingPerRow;
                }

                #endregion Pixel Table
            }
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
            //What should we do when a stream has already been used ie. the position is not 0?
            Byte[] file = new Byte[54 /*header*/ + Width * Height * (uint)Depth / 8 /*assume that it is full bytes */];
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

            //Offset to start of iamge data
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

            //Number of palnes(1)
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

            //number of important colors in iamge / zero
            data = BitConverter.GetBytes(0);
            Array.Copy(data, 0, file, position, 4);
            position += 4;

            //Finished header

            //Copy image data
            position = (int)offset;
            Byte[] imageData = new Byte[Width * Height * (int)Depth / 8];
            int imageDataPoint = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    data = BitConverter.GetBytes(rawData[x + (Height - (y + 1)) * Width]);
                    Array.Copy(data, 0, imageData, imageDataPoint, data.Length);
                    imageDataPoint += data.Length;
                }
            }
            Array.Copy(imageData, 0, file, position, imageData.Length);
            stream.Write(file, 0, file.Length);
        }
    }
}
