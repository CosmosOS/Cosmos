using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics.Fonts
{
    public abstract class Font
    {

        public abstract byte[] Data { get; }

        public abstract void SetFont(byte[] aFileData);

        public bool ConvertByteToBitAddres(byte byteToConvert, int bitToReturn)
        {
            int mask = 1 << (bitToReturn - 1);
            return (byteToConvert & mask) != 0;
        }
    }
}
