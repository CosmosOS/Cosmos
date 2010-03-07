using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public static class HexExtension {
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
            Console.Write("Pad size: ");
            WriteNumber((uint)width, 32);
            Console.WriteLine("");
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

        public static string ToHex(this UInt16 n) {
            return ConvertToHex((UInt32)n);
        }

        public static string ToHex(this UInt16 n, int width) {
            return ConvertToHex((UInt32)n).PadLeft(width, '0');
        }


        public static string ToHex(this uint aValue) {
            return ConvertToHex(aValue);
        }

        public static string ToHex(this uint aValue, int aWidth) {
            return ConvertToHex(aValue).PadLeft(aWidth, '0');
        }

        public static string ToHex(this ulong aValue)
        {
            return ConvertToHex(aValue);
        }

        public static string ToHex(this ulong aValue, int aWidth)
        {
            return ConvertToHex(aValue).PadLeft(aWidth, '0');
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

            while (num != 0)
            {
                //Note; char is converted to string because Cosmos crashes when adding char and string. Frode, 7.june.
                xHex = SingleDigitToHex((byte)(num & 0xf)) + xHex;
                num = num >> 4;
            }

            return xHex;
        }

        private static string ConvertToHex(UInt64 num)
        {
            string xHex = string.Empty;

            while (num != 0)
            {
                //Note; char is converted to string because Cosmos crashes when adding char and string. Frode, 7.june.
                xHex = SingleDigitToHex((byte)(num & 0xf)) + xHex;
                num = num >> 4;
            }

            return xHex;
        }

        public static string SingleDigitToHex(byte d)
        {
            switch (d)
            {
                case 0:
                    return "0";
                case 1:    
                    return "1";
                case 2:
                    return "2";
                case 3:
                    return "3";
                case 4:
                    return "4";
                case 5:
                    return "5";
                case 6:
                    return "6";
                case 7:
                    return "7";
                case 8:
                    return "8";
                case 9:
                    return "9";
                case 10:
                    return "a";
                case 11:
                    return "b";
                case 12:
                    return "c";
                case 13:
                    return "d";
                case 14:
                    return "e";
                case 15:
                    return "f";
            }
            return " ";

        }

        #endregion

        public static void WriteNumber(uint aValue,
                                byte aBitCount)
        {
            uint xValue = aValue;
            byte xCurrentBits = aBitCount;
            Console.Write("0x");
            while (xCurrentBits >= 4)
            {
                xCurrentBits -= 4;
                byte xCurrentDigit = (byte)((xValue >> xCurrentBits) & 0xF);
                string xDigitString = null;
                switch (xCurrentDigit)
                {
                    case 0:
                        xDigitString = "0";
                        goto default;
                    case 1:
                        xDigitString = "1";
                        goto default;
                    case 2:
                        xDigitString = "2";
                        goto default;
                    case 3:
                        xDigitString = "3";
                        goto default;
                    case 4:
                        xDigitString = "4";
                        goto default;
                    case 5:
                        xDigitString = "5";
                        goto default;
                    case 6:
                        xDigitString = "6";
                        goto default;
                    case 7:
                        xDigitString = "7";
                        goto default;
                    case 8:
                        xDigitString = "8";
                        goto default;
                    case 9:
                        xDigitString = "9";
                        goto default;
                    case 10:
                        xDigitString = "A";
                        goto default;
                    case 11:
                        xDigitString = "B";
                        goto default;
                    case 12:
                        xDigitString = "C";
                        goto default;
                    case 13:
                        xDigitString = "D";
                        goto default;
                    case 14:
                        xDigitString = "E";
                        goto default;
                    case 15:
                        xDigitString = "F";
                        goto default;
                    default:
                        Console.Write(xDigitString);
                        break;
                }
            }
        }

    }
}
