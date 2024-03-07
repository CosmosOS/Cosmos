using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Represents a raster image.
    /// </summary>
    public abstract class Image
    {
        /// <summary>
        /// The raw data of the image. This array holds all of the pixel
        /// values of the raster image.
        /// </summary>
        public int[] RawData;

        /// <summary>
        /// The width of the image.
        /// </summary>
        public uint Width { get; protected set; }

        /// <summary>
        /// The height of the image.
        /// </summary>
        public uint Height { get; protected set; }

        /// <summary>
        /// The color depth of each pixel of the image - i.e, the amount
        /// of bits per each pixel.
        /// </summary>
        public ColorDepth Depth { get; protected set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Image"/> class.
        /// </summary>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="color">The color depth of each pixel.</param>
        protected Image(uint width, uint height, ColorDepth color)
        {
            Width = width;
            Height = height;
            Depth = color;
        }

        /// <summary>
        /// Resizes the bitmap
        /// </summary>
        /// <param name="NewW">the new width of the bitmap</param>
        /// <param name="NewH">the new height of the bitmap</param>
        public void Resize(uint NewW, uint NewH)
        {

            this.RawData = ScaleImage(this, (int)NewW, (int)NewH);
            this.Width = NewW;
            this.Height = NewH;

        }

        /// <summary>
        /// sets a pixel of the image
        /// </summary>
        /// <param name="color">the color of the pixel</param>
        /// <param name="X">the x coordinate of the pixel</param>
        /// <param name="Y">the y coordinate of the pixel</param>
        public void SetPixel(Color color,int X,int Y)
        {

            RawData[X + (Y * Width)] = color.ToArgb();

        }

        static int[] ScaleImage(Image image, int newWidth, int newHeight)
        {
            int[] pixels = image.RawData;
            int w1 = (int)image.Width;
            int h1 = (int)image.Height;
            int[] temp = new int[newWidth * newHeight];
            int xRatio = (int)((w1 << 16) / newWidth) + 1;
            int yRatio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;

            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = (j * xRatio) >> 16;
                    y2 = (i * yRatio) >> 16;
                    temp[(i * newWidth) + j] = pixels[(y2 * w1) + x2];
                }
            }

            return temp;
        }

    }

    /// <summary>
    /// Supported image formats.
    /// </summary>
    public enum ImageFormat
    {
        BMP
    }
}
