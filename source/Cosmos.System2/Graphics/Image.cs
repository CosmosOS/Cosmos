using System;
using System.Collections.Generic;
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
    }

    /// <summary>
    /// Supported image formats.
    /// </summary>
    public enum ImageFormat
    {
        BMP
    }
}
