using System;
using Cosmos.Common.Extensions;

namespace Cosmos.Common
{
    /// <summary>
    /// Helper class for working with numbers.
    /// </summary>
    public static class NumberHelper
    {
        /// <summary>
        /// Write number to console.
        /// </summary>
        /// <param name="aValue">A value to print.</param>
        /// <param name="aZeroFill">A value indicating whether strarting zeros should be present.</param>
        public static void WriteNumber(uint aValue, bool aZeroFill)
        {
            WriteNumber(aValue, 32, aZeroFill);
        }

        /// <summary>
        /// Write number to console.
        /// </summary>
        /// <param name="aValue">A value to print.</param>
        /// <param name="aBits">Count of bits to display.</param>
        public static void WriteNumber(uint aValue, int aBits)
        {
            WriteNumber(aValue, aBits, true);
        }

        /// <summary>
        /// Write number to console.
        /// </summary>
        /// <param name="aValue">A value to print.</param>
        /// <param name="aBits">Count of bits to display.</param>
        /// <param name="aZeroFill">A value indicating whether strarting zeros should be present.</param>
        public static void WriteNumber(uint aValue, int aBits, bool aZeroFill)
        {
            if (aZeroFill)
            {
                Console.WriteLine("0x" + aValue.ToHex(aBits / 4));
            }
            else
            {
                Console.WriteLine("0x" + aValue.ToHex(aBits / 4).TrimStart('0'));
            }
        }
    }
}
