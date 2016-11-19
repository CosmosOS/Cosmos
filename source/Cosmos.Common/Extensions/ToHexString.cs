namespace Cosmos.Common.Extensions
{
    public static class ToHexString
    {
        //TODO: Can add several more overloads for other numbertypes, with and without width argument.

        public static string ToHex(this byte n)
        {
            return ConvertToHex((uint)n, 2);
        }

        public static string ToHex(this byte n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        public static string ToHex(this int n)
        {
            return ConvertToHex((uint)n, 4);
        }

        public static string ToHex(this int n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        public static string ToHex(this ushort n)
        {
            return ConvertToHex((uint)n, 4);
        }

        public static string ToHex(this ushort n, int aWidth)
        {
            return ConvertToHex((uint)n, aWidth);
        }

        public static string ToHex(this uint aValue)
        {
            return ConvertToHex(aValue, 8);
        }

        public static string ToHex(this uint aValue, int aWidth)
        {
            return ConvertToHex(aValue, aWidth);
        }

        public static string ToHex(this ulong aValue)
        {
            return ConvertToHex(aValue).PadLeft(16, '0');
        }

        public static string ToHex(this ulong aValue, int aWidth)
        {
            return ConvertToHex(aValue).PadLeft(aWidth, '0');
        }

        private static string GetPrefix()
        {
            return "0x";
        }

        private static string GetSuffix()
        {
            return "h";
        }

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

        private static string ConvertToHex(uint aValue, int aWidth)
        {
            return ConvertToHex(aValue).PadLeft(aWidth, '0');
        }


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
