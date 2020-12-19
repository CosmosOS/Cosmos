namespace Cosmos.Common.Extensions
{
    /// <summary>
    /// Contains various helper methods to convert ints to hex represented by string.
    /// Supported types:
    /// <list type="bullet">
    /// <item>byte.</item>
    /// <item>ushort.</item>
    /// <item>int.</item>
    /// <item>uint.</item>
    /// <item>ulong.</item>
    /// </list>
    /// </summary>
    public static class ToHexString
    {
        //TODO: Can add several more overloads for other numbertypes, with and without width argument.

        /// <summary>
        /// Convert byte to 2 characters long hexadecimal string, padded with '0's.
        /// </summary>
        /// <param name="n">A byte to be converted to hexadecimal string.</param>
        /// <returns>2 characters long string value, padded with '0's.</returns>
        public static string ToHex(this byte n)
        {
            return ConvertToHex((uint)n, 2);
        }

        /// <summary>
        /// Convert byte to hexadecimal string of a given length.
        /// </summary>
        /// <param name="n">A byte to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        public static string ToHex(this byte n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        /// <summary>
        /// Convert int to 4 characters long hexadecimal string, padded with '0's.
        /// </summary>
        /// <param name="n">A int to be converted to hexadecimal string.</param>
        /// <returns>4 characters long string value, padded with '0's.</returns>
        public static string ToHex(this int n)
        {
            return ConvertToHex((uint)n, 4); //TODO: this cast might throw OverflowException. Better catch it.
        }

        /// <summary>
        /// Convert int to hexadecimal string of a given length.
        /// </summary>
        /// <param name="n">A int to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        public static string ToHex(this int n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        /// <summary>
        /// Convert 16-bit unsigned int to 4 characters long hexadecimal string, padded with '0's.
        /// </summary>
        /// <param name="n">A 16-bit unsigned int to be converted to hexadecimal string.</param>
        /// <returns>4 characters long string value, padded with '0's.</returns>
        public static string ToHex(this ushort n)
        {
            return ConvertToHex((uint)n, 4);
        }

        /// <summary>
        /// Convert 16-bit unsigned int to hexadecimal string of a given length.
        /// </summary>
        /// <param name="n">A 16-bit unsigned int to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        public static string ToHex(this ushort n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        /// <summary>
        /// Convert 32-bit unsigned int to 8 characters long hexadecimal string, padded with '0's.
        /// </summary>
        /// <param name="aValue">A 32-bit unsigned int to be converted to hexadecimal string.</param>
        /// <returns>8 characters long string value, padded with '0's.</returns>
        public static string ToHex(this uint aValue)
        {
            return ConvertToHex(aValue, 8);
        }

        /// <summary>
        /// Convert 32-bit unsigned int to hexadecimal string of a given length.
        /// </summary>
        /// <param name="aValue">A 32-bit unsigned int to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        public static string ToHex(this uint aValue, int aWidth)
        {
            return ConvertToHex(aValue, aWidth);
        }

        /// <summary>
        /// Convert 64-bit unsigned int to 16 characters long hexadecimal string, optionally padded with '0's.
        /// </summary>
        /// <param name="aValue">A 64-bit unsigned int to be converted to hexadecimal string.</param>
        /// <param name="withPadding">Determines if a left padding should be applied</param>
        /// <returns>16 characters long string value, optionally padded with '0's.</returns>
        public static string ToHex(this ulong aValue, bool withPadding = true)
        {
            var hex = ConvertToHex(aValue);
            if (withPadding)
            {
                return hex.PadLeft(16, '0');
            }
            else
            {
                return hex;
            }
        }

        /// <summary>
        /// Convert 64-bit unsigned int to hexadecimal string of a given length.
        /// </summary>
        /// <param name="aValue">A 64-bit unsigned int to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        public static string ToHex(this ulong aValue, int aWidth)
        {
            return ConvertToHex(aValue).PadLeft(aWidth, '0');
        }

        /// <summary>
        /// Get hex prefix.
        /// </summary>
        /// <returns>Hex prefix, as string.</returns>
        private static string GetPrefix()
        {
            return "0x";
        }

        /// <summary>
        /// Get hex suffix.
        /// </summary>
        /// <returns>Hex suffix, as string.</returns>
        private static string GetSuffix()
        {
            return "h";
        }

        /// <summary>
        /// Convert 32-bit unsigned int to hexadecimal string.
        /// </summary>
        /// <param name="num">A 32-bit unsigned int to be converted to hexadecimal string.</param>
        /// <returns>String value.</returns>
        private static string ConvertToHex(uint num)
        {
            string xHex = string.Empty;

            if (num == 0)
            {
                xHex = "0";
            }
            else
            {
                while (num != 0)
                {
                    xHex = DigitToHexChar((byte)(num & 0xf)) + xHex;
                    num = num >> 4;
                }
            }

            return xHex;
        }

        /// <summary>
        /// Convert 32-bit unsigned int to hexadecimal string of a given length.
        /// </summary>
        /// <param name="aValue">A 32-bit unsigned int to be converted to hexadecimal string.</param>
        /// <param name="aWidth">The number of characters in the resulting string.</param>
        /// <returns>String value, right aligned and padded on the left with '0's.
        /// If aWidth is less then the length of the resulting string, the resulting
        /// string would not be trimmed.
        /// </returns>
        private static string ConvertToHex(uint aValue, int aWidth)
        {
            return ConvertToHex(aValue).PadLeft(aWidth, '0'); //TODO: PadLeft might throw ArgumentOutOfRangeException. Better catch it.
        }

        /// <summary>
        /// Convert 64-bit unsigned int to hexadecimal string.
        /// </summary>
        /// <param name="num">A 64-bit unsigned int to be converted to hexadecimal string.</param>
        /// <returns>String value.</returns>
        private static string ConvertToHex(ulong num)
        {
            string xHex = string.Empty;

            while (num != 0)
            {
                //Note; char is converted to string because Cosmos crashes when adding char and string. Frode, 7.june.
                xHex = DigitToHexChar((byte)(num & 0xf)) + xHex;
                num = num >> 4;
            }

            return xHex;
        }

        /// <summary>
        /// Convert byte to hexadecimal char.
        /// </summary>
        /// <param name="d">A byte to be converted to hexadecimal char.</param>
        /// <returns>Char value.</returns>
        public static char DigitToHexChar(byte d)
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
    }
}
