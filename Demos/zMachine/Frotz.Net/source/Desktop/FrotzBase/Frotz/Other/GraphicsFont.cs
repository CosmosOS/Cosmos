#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Frotz.Other
{
    public static class GraphicsFont
    {
        static List<System.Drawing.Bitmap> files = new List<System.Drawing.Bitmap>();

        static GraphicsFont()
        {
        }

        public static String getLines(int id)
        {
            switch (id)
            {
                case 32: return "0000000000000000";
                case 33: return "000004067F060400";
                case 34: return "000010307F301000";
                case 35: return "8040201008040201";
                case 36: return "0102040810204080";
                case 37: return "0000000000000000";
                case 38: return "00000000FF000000";
                case 39: return "000000FF00000000";
                case 40: return "1010101010101010";
                case 41: return "0808080808080808";
                case 42: return "101010FF00000000";
                case 43: return "00000000FF101010";
                case 44: return "10101010F0101010";
                case 45: return "080808080F080808";
                case 46: return "08080808F8000000";
                case 47: return "000000F808080808";
                case 48: return "0000001F10101010";
                case 49: return "101010101F000000";
                case 50: return "08080808F8040201";
                case 51: return "010204F808080808";
                case 52: return "8040201F10101010";
                case 53: return "101010101F204080";
                case 54: return "FFFFFFFFFFFFFFFF";
                case 55: return "FFFFFFFFFF000000";
                case 56: return "000000FFFFFFFFFF";
                case 57: return "1F1F1F1F1F1F1F1F";
                case 58: return "F8F8F8F8F8F8F8F8";
                default:
                    return "FF818181818181FF";
            }
        }
    }
}
#endif