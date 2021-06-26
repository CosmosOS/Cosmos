using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    public struct Mode
    {
        /// <summary>
        /// Get mode width
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Get mode height
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Get mode color depth
        /// </summary>
        public ColorDepth Depth { get; private set; }

        /// <summary>
        /// Create a new video mode
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        /// <param name="depth"></param>
        public Mode(int w, int h, ColorDepth depth)
        {
            Width = w;
            Height = h;
            Depth = depth;
        }
    }
}
