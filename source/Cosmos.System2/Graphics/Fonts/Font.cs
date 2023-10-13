using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Cosmos.System.Graphics.Fonts
{
    /// <summary>
    /// Represents a bitmap font.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Gets the raw pixel data of the bitmap font.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// The height of a single character in pixels.
        /// </summary>
        public byte Height { get; }

        /// <summary>
        /// The width of a single character in pixels.
        /// </summary>
        public byte Width { get; }

        /// <summary>
        /// Converts a byte to its byte address.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ConvertByteToBitAddress(byte byteToConvert, int bitToReturn)
        {
            int mask = 1 << (8 - bitToReturn);
            return (byteToConvert & mask) != 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Font"/> class.
        /// </summary>
        /// <param name="width">The width of a single character in pixels</param>
        /// <param name="height">The height of a single character in pixels</param>
        /// <param name="data">The raw pixel data.</param>
        public Font(byte width, byte height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
}
