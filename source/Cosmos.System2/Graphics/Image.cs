using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Image file format
    /// <list type="">
    /// <item>BMP  - Bitmap</item>
    /// <item>PNG  - Portable Network Graphics</item>
    /// <item>JPEG - Joint Photographics</item>
    /// <item>GIF  - Graphics Interchange Format</item>
    /// </list>
    /// </summary>
    public enum ImageFormat
    {
        /// <summary>
        /// Bitmap
        /// </summary>
        BMP,

        /// <summary>
        /// Portable Network Graphics
        /// </summary>
        PNG,

        /// <summary>
        /// Joint Photographics
        /// </summary>
        JPEG,

        /// <summary>
        /// Graphics Interchange Format
        /// </summary>
        GIF,
    }

    /// <summary>
    /// Base class for images
    /// </summary>
    public abstract class Image
    {
        /// <summary>
        /// Get image width
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// Get image height
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// Get image color depth
        /// </summary>
        public ColorDepth Depth { get; protected set; }

        /// <summary>
        /// Get image's raw pixel data
        /// </summary>
        public uint[] RawData { get; protected set; }

        /// <summary>
        /// Create a new image with specified width, height, and depth
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="depth"></param>
        protected Image(int width, int height, ColorDepth depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }

        /// <summary>
        /// Resize image and return resized raw data
        /// </summary>
        /// <param name="image"></param>
        /// <param name="newWidth"></param>
        /// <param name="newHeight"></param>
        /// <returns></returns>
        public static uint[] ResizeRaw(Image image, int newWidth, int newHeight)
        {
            int w1 = image.Width;
            int h1 = image.Height;
            uint[] temp = new uint[newWidth * newHeight];
            int x_ratio = (int)((w1 << 16) / newWidth) + 1;
            int y_ratio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * newWidth) + j] = image.RawData[(y2 * w1) + x2];
                }
            }
            return temp;
        }
    }
}
