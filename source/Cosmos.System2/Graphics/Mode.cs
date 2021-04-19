using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    public struct Mode
    {
        public int Width;
        public int Height;
        public ColorDepth Depth;

        // constructor
        public Mode(int width, int height, ColorDepth depth)
        {
            Width = width;
            Height = height;
            Depth = depth;
        }
    }
}
