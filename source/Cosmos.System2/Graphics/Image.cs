using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Image class.
    /// </summary>
    public abstract class Image
    {
        /// <summary>
        /// Get and set raw data (pixels array).
        /// </summary>
        public int[] rawData;

        /// <summary>
        /// Get and set image width.
        /// </summary>
        public uint Width { get; protected set; }

        /// <summary>
        /// Get and set image height.
        /// </summary>
        public uint Height { get; protected set; }

        /// <summary>
        /// Get and set image color depth.
        /// </summary>
        public ColorDepth Depth { get; protected set; }

        /// <summary>
        /// Create new instance of <see cref="Image"/> class.
        /// </summary>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="color">Color depth.</param>
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
        bmp
    } //Add more as more are supported
}
