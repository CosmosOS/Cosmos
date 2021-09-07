using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics.Fonts
{
    /// <summary>
    /// Base class for fonts.
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Get font pure data.
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Get font height.
        /// </summary>
        public byte Height { get; }

        /// <summary>
        /// Get font Width.
        /// </summary>
        public byte Width { get; }

        /// <summary>
        /// Used to draw font.
        /// </summary>
        /// <param name="byteToConvert">byteToConvert</param>
        /// <param name="bitToReturn">bitToReturn</param>
        public bool ConvertByteToBitAddres(byte byteToConvert, int bitToReturn)
        {
            int mask = 1 << (bitToReturn - 1);
            return (byteToConvert & mask) != 0;
        }

        public Font(byte aWidth, byte aHeight, byte[] aData)
        {
            Width = aWidth;
            Height = aHeight;
            Data = aData;
        }
    }
}
