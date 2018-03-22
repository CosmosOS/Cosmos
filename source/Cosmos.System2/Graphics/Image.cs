using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    public abstract class Image
    {
        public int[] rawData;
        public uint Width { get; protected set; }
        public uint Height { get; protected set; }
        public ColorDepth Depth { get; protected set; }

        protected Image(uint width, uint height, ColorDepth color)
        {
            Width = width;
            Height = height;
            Depth = color;
        }
    }

    public enum ImageFormat
    {
        bmp
    } //Add more as more are supported
}
