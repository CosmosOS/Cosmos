using System;
using System.Globalization;
using System.Text;
using IL2CPU.API;

namespace Cosmos.IL2CPU.IL.CustomImplementations.System
{
    [Plug(Target = typeof(sbyte))]
    public static class Int8Impl
    {
        public static string ToString(ref sbyte aThis)
        {
            bool bNegative = false;

            sbyte aValue = aThis;

            if (aValue < 0)
            {
                bNegative = true;
                aValue *= -1;
            }

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, bNegative);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out sbyte result)
        {
            // Todo, consider how to implemente style and provider
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out sbyte result)
        {
            bool bCanParse  = false;
            result = 0;

            try
            {
                result    = Parse(s);
                bCanParse   = true;
            }
            catch(Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static sbyte Parse(string s)
        {
            Int64 result    = GeneralIntegerImplByUInt64.ParseSignedInteger(s);

            if (result > sbyte.MaxValue || result < sbyte.MinValue)
            {
                throw new OverflowException();
            }

            return (sbyte)result;
        }
    }

    [Plug(Target = typeof(byte))]
    public static class UInt8Impl
    {
        public static string ToString(ref byte aThis)
        {
            byte aValue = aThis;

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, false);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out byte result)
        {
            // Todo, consider how to implemente style and provider
            // Exponent, "1E0", "1e3"...
            // Decimal point "1.0", "3.5"
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out byte result)
        {
            bool bCanParse = false;
            result = 0;

            try
            {
                result = Parse(s);
                bCanParse = true;
            }
            catch (Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static byte Parse(string s)
        {
            UInt64 result = GeneralIntegerImplByUInt64.ParseUnsignedInteger(s);

            if (result > byte.MaxValue)
            {
                throw new OverflowException();
            }

            return (byte)result;
        }
    }

    [Plug(Target = typeof(short))]
    public static class Int16Impl
    {
        public static string ToString(ref short aThis)
        {
            bool bNegative = false;

            short aValue = aThis;

            if (aValue < 0)
            {
                bNegative = true;
                aValue *= -1;
            }

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, bNegative);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            // Todo, consider how to implemente style and provider
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out short result)
        {
            bool bCanParse = false;
            result = 0;

            try
            {
                result = Parse(s);
                bCanParse = true;
            }
            catch (Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static short Parse(string s)
        {
            Int64 result = GeneralIntegerImplByUInt64.ParseSignedInteger(s);

            if (result > short.MaxValue || result < short.MinValue)
            {
                throw new OverflowException();
            }

            return (short)result;
        }
    }

    [Plug(Target = typeof(ushort))]
    public static class UInt16Impl
    {
        public static string ToString(ref ushort aThis)
        {
            ushort aValue = aThis;

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, false);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out ushort result)
        {
            // Todo, consider how to implemente style and provider
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out ushort result)
        {
            bool bCanParse = false;
            result = 0;

            try
            {
                result = Parse(s);
                bCanParse = true;
            }
            catch (Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static ushort Parse(string s)
        {
            UInt64 result = GeneralIntegerImplByUInt64.ParseUnsignedInteger(s);

            if (result > ushort.MaxValue)
            {
                throw new OverflowException();
            }

            return (ushort)result;
        }
    }

    [Plug(Target = typeof(int))]
    public static class Int32Impl
    {
        public static string ToString(ref int aThis)
        {
            bool bNegative = false;

            int aValue = aThis;

            if (aValue < 0)
            {
                bNegative = true;
                aValue *= -1;
            }

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, bNegative);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out short result)
        {
            // Todo, consider how to implemente style and provider
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out int result)
        {
            bool bCanParse = false;
            result = 0;

            try
            {
                result = Parse(s);
                bCanParse = true;
            }
            catch (Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static int Parse(string s)
        {
            Int64 result = GeneralIntegerImplByUInt64.ParseSignedInteger(s);

            if (result > int.MaxValue || result < int.MinValue)
            {
                throw new OverflowException();
            }

            return (int)result;
        }
    }

    [Plug(Target = typeof(uint))]
    public static class UInt32Impl
    {
        public static string ToString(ref uint aThis)
        {
            uint aValue = aThis;

            return GeneralIntegerImplByUInt64.ToString((UInt64)aValue, false);
        }

        public static bool TryParse(string s, NumberStyles style, IFormatProvider provider, out uint result)
        {
            // Todo, consider how to implemente style and provider
            throw new NotImplementedException();

            //return TryParse(s, out result);
        }

        public static bool TryParse(string s, out uint result)
        {
            bool bCanParse = false;
            result = 0;

            try
            {
                result = Parse(s);
                bCanParse = true;
            }
            catch (Exception)
            {
                // Something wrong
            }

            return bCanParse;
        }

        public static uint Parse(string s)
        {
            UInt64 result = GeneralIntegerImplByUInt64.ParseUnsignedInteger(s);

            if (result > uint.MaxValue)
            {
                throw new OverflowException();
            }

            return (uint)result;
        }
    }

    internal static class GeneralIntegerImplByUInt64
    {
        internal static Int64 ParseSignedInteger(string s)
        {
            bool bNegative;
            Int64 result = (Int64)ParseInteger(s, out bNegative);

            if (bNegative)
                result *= -1;

            return result;
        }

        internal static UInt64 ParseUnsignedInteger(string s)
        {
            bool bNegative;
            UInt64 result = ParseInteger(s, out bNegative);

            if (bNegative && result != 0)
            {
                throw new OverflowException();
            }

            return result;
        }

        /// <summary>
        /// ParseInteger
        /// Sole algorithm implementation of "integer Parse(string)"
        /// </summary>
        /// <param name="s"></param>
        /// <param name="bNegative"></param>
        /// <returns></returns>
        private static UInt64 ParseInteger(string s, out bool bNegative)
        {
            UInt64 result = 0;

            int nParseStartPos = 0;
            bNegative = false;

            if (s.Length >= 1)
            {
                if (s[0] == '+')
                {
                    nParseStartPos = 1;
                }
                else if (s[0] == '-')
                {
                    nParseStartPos = 1;
                    bNegative = true;
                }
            }

            for (int i = nParseStartPos; i < s.Length; i++)
            {
                sbyte ind = (sbyte)(s[i] - '0');
                if (ind < 0 || ind > 9)
                {
                    throw new FormatException("Digit " + s[i] + " not found");
                }

                result = (result * 10) + (UInt64)ind;
            }

            return result;
        }

        /// <summary>
        /// ToString
        /// Sole algorithm implementation of "string ToString(integer)"
        /// </summary>
        /// <param name="aValue"></param>
        /// <param name="bIsNegative"></param>
        /// <returns></returns>
        internal static string ToString(UInt64 aValue, bool bIsNegative)
        {
            char[] xResultChars = new char[21]; // 64 bit UInteger convert to string, with sign symble, max length is 21.
            int xCurrentPos = xResultChars.Length - 1;
            while (aValue > 0)
            {
                byte xPos = (byte)(aValue % 10);
                aValue /= 10;
                xResultChars[xCurrentPos] = (char)('0' + xPos);
                xCurrentPos -= 1;
            }

            if (bIsNegative)
            {
                xResultChars[xCurrentPos] = '-';
                xCurrentPos -= 1;
            }

            return new String(xResultChars, xCurrentPos + 1, xResultChars.Length - xCurrentPos - 1);
        }
    }
}
