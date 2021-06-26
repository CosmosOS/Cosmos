using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics.Fonts
{
    /// <summary>
    /// Base class for fonts
    /// </summary>
    public abstract class Font
    {
        /// <summary>
        /// Get font width.
        /// </summary>
        public byte Width { get; }

        /// <summary>
        /// Get font height.
        /// </summary>
        public byte Height { get; }

        /// <summary>
        /// Get horizontal font spacing
        /// </summary>
        public byte HorizontalSpacing { get; private set; }

        /// <summary>
        /// Get vertical font spacing
        /// </summary>
        public byte VerticalSpacing { get; private set; }

        /// <summary>
        /// Get raw binary font data
        /// </summary>
        public byte[] Data { get; }

        /// <summary>
        /// Create new font with default spacing
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="aData"></param>
        public Font(byte aWidth, byte aHeight, byte[] aData)
        {
            Width = aWidth;
            Height = aHeight;
            HorizontalSpacing = 0;
            VerticalSpacing = 0;
            Data = aData;
        }

        /// <summary>
        /// Create new font with specified horizontal and vertical spacing
        /// </summary>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        /// <param name="aSpacingH"></param>
        /// <param name="aSpacingV"></param>
        /// <param name="aData"></param>
        public Font(byte aWidth, byte aHeight, byte aSpacingH, byte aSpacingV, byte[] aData)
        {
            Width = aWidth;
            Height = aHeight;
            HorizontalSpacing = aSpacingH;
            VerticalSpacing = aSpacingV;
            Data = aData;
        }

        /// <summary>
        /// Conversion algorithm used for drawing font characters
        /// </summary>
        /// <param name="byteToConvert">byteToConvert</param>
        /// <param name="bitToReturn">bitToReturn</param>
        public bool ConvertByteToBitAddress(byte byteToConvert, int bitToReturn)
        {
            int mask = 1 << (bitToReturn - 1);
            return (byteToConvert & mask) != 0;
        }
    }
}
