using System;
using System.Collections.Generic;
using System.Text;

namespace FrodeTest.Extension.NumberSystem
{
    public static class Hex
    {

        #region C# 3.0 Extension Methods
        public static string AsHex(this int n)
        {
            return ConvertToHex((UInt32)n);
        }

        #endregion 

        #region Converters

        private static string ConvertToHex(UInt32 num)
        {
            string xHex = string.Empty;

            while (num >= 16)
            {
                xHex = (SingleDigitToHex((byte)(num & 0xf))) + xHex;
                num = num / 16;
            }

            xHex = SingleDigitToHex((byte)(num & 0xf)) + xHex;

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
                    return 'A';
                case 11:
                    return 'B';
                case 12:
                    return 'C';
                case 13:
                    return 'D';
                case 14:
                    return 'E';
                case 15:
                    return 'F';
            }
            return ' ';

        }

        #endregion
    }
}
