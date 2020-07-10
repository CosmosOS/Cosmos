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
        public abstract byte[] Data { get; }

        /// <summary>
        /// Get font height.
        /// </summary>
        public abstract byte Height { get; }

        /// <summary>
        /// Get font Width.
        /// </summary>
        public abstract byte Width { get; }

        /// <summary>
        /// Set font file.
        /// </summary>
        /// <param name="aFileData">Font file.</param>
        public abstract void SetFont(byte[] aFileData);

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
    }
}
