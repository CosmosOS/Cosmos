using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Extension.NumberSystem
{
    public static class Hex
    {

        #region C# 3.0 Extension Methods

        //TODO: Can add several more overloads for other numbertypes, with and withouth width argument.

        public static string ToHex(this byte n)
        {
            return ConvertToHex((UInt32)n);
        }

        /// <summary>
        /// When width is supplied the hex value will be left padded with zeroes.
        /// </summary>
        public static string ToHex(this byte n, int width)
        {
            return ConvertToHex((UInt32)n).PadLeft(width, '0');
        }

        public static string ToHex(this int n)
        {
            return ConvertToHex((UInt32)n);
        }

        public static string ToHex(this int n, int width)
        {
            return ConvertToHex((UInt32)n).PadLeft(width, '0');
        }

        public static string ToHex(this UInt16 n)
        {
            return ConvertToHex((UInt32)n);
        }

        public static string ToHex(this UInt16 n, int width)
        {
            return ConvertToHex((UInt32)n).PadLeft(width, '0');
        }

        #endregion 

        #region Prefix/Suffix

        private static string GetPrefix()
        {
            return "0x";
        }

        private static string GetSuffix()
        {
            return "h";
        }

        #endregion

        #region Converters

        private static string ConvertToHex(UInt32 num)
        {
            string xHex = string.Empty;

            while (num >= 16)
            {
                //Note; char is converted to string because Cosmos crashes when adding char and string. Frode, 7.june.
                xHex = SingleDigitToHex((byte)(num & 0xf)).ToString() + xHex;
                num = num / 16;
            }

            xHex = SingleDigitToHex((byte)(num & 0xf)).ToString() + xHex;

            return xHex;
        }

        private static char SingleDigitToHex(byte d)
        {
            switch (d)
            {
                case 0:
                    return '0';
                case 1:
                    return '1';
                case 2:
                    return '2';
                case 3:
                    return '3';
                case 4:
                    return '4';
                case 5:
                    return '5';
                case 6:
                    return '6';
                case 7:
                    return '7';
                case 8:
                    return '8';
                case 9:
                    return '9';
                case 10:
                    return 'a';
                case 11:
                    return 'b';
                case 12:
                    return 'c';
                case 13:
                    return 'd';
                case 14:
                    return 'e';
                case 15:
                    return 'f';
            }
            return ' ';

        }

        #endregion
    }
}
