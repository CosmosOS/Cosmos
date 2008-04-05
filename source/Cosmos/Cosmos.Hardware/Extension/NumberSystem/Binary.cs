using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware.Extension.NumberSystem
{
    public static class Binary
    {
        #region Extension methods

        public static string ToBinary(this byte n)
        {
            return ConvertToBinary((UInt32)n);
        }

        public static string ToBinary(this byte n, int width)
        {
            return ConvertToBinary((UInt32)n).PadLeft(width, '0');
        }

        public static string ToBinary(this byte n, int width, bool prefix, bool suffix)
        {
            string bin = "";
            if (prefix)
                bin += GetPrefix();

            bin += ConvertToBinary((UInt32)n).PadLeft(width, '0');

            if (suffix)
                bin = bin + GetSuffix();

            return bin;
        }

        public static string ToBinary(this int n)
        {
            return ConvertToBinary((UInt32)n);
        }

        public static string ToBinary(this int n, int width)
        {
            return ConvertToBinary((UInt32)n).PadLeft(width, '0');
        }

        #endregion

        #region Prefix/Suffix

        private static string GetPrefix()
        {
            return "0b";
        }

        private static string GetSuffix()
        {
            return "b";
        }

        #endregion

        #region Conversion

        public static string ConvertToBinary(UInt32 d)
        {
            if (d == 0)
                return "0";

            string bin = string.Empty;

            while (d > 0)
            {
                if (d.IsOdd())
                {
                    bin = "1" + bin;
                    d = d - 1;
                }
                else
                {
                    bin = "0" + bin;
                }

                d = d / 2;
            }

            return bin;
        }

        private static bool IsOdd(this UInt32 n)
        {
            if ((n % 2) > 0)
                return true;
            else
                return false;
        }

        #endregion
    }
}
