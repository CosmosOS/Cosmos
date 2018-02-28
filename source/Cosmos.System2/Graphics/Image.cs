using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    public abstract class Image
    {
        public int[] rawData;
        public readonly uint width;
        public readonly uint height;
        public readonly ColorDepth depth;

        protected Image(uint Width, uint Height, ColorDepth Color)
        {
            width = Width;
            height = Height;
            depth = Color;
        }
    }

    public enum ImageFormat
    {
        bmp
    } //Add more as more are supported
}
