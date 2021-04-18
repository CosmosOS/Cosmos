using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    public enum ImageFormat
    {
        BMP,
        PNG,
        JPEG,
    }

    public abstract class Image
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public ColorDepth Depth { get; protected set; }
        public uint[] RawData { get; protected set; }

        // constructor
        public Image(int width, int height, ColorDepth depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }
}
