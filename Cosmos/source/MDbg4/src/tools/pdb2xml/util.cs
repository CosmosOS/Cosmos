using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

// For symbol store
using System.Diagnostics.SymbolStore;
using Microsoft.Samples.Debugging.CorSymbolStore;

namespace Pdb2Xml
{
    // Random utility methods.
    static class Util
    {
        // Format a token to a string. Tokens are in hex.
        internal static string AsToken(int i)
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "0x{0:x}", i);
        }

        // If I have a string of a hex token and I want a SymbolToken, here's how to do it
        internal static SymbolToken AsSymToken(string token)
        {
            return new SymbolToken(Util.ToInt32(token, 16));
        }

        internal static int ToInt32(string input)
        {
            return Util.ToInt32(input, 10);
        }

        internal static int ToInt32(string input, int numberBase)
        {
            return Convert.ToInt32(input, numberBase);
        }

        internal static string ToHexString(byte[] input)
        {
            StringBuilder sb = new StringBuilder(input.Length);
            foreach (byte b in input)
            {
                sb.AppendFormat("{0:X2}", b);
            }
            return sb.ToString();
        }

        internal static byte[] ToByteArray(string input)
        {
            // Convert from a hex string
            if (input.Length % 2 != 0)
                throw new FormatException("Hex string should have even number of characters");

            byte[] retval = new byte[input.Length / 2];
            for (int i = 0; i < retval.Length; i++)
            {
                string byteInHex = input.Substring(i * 2, 2);
                retval[i] = Convert.ToByte(byteInHex, 16);
            }
            return retval;
        }

        internal static string CultureInvariantToString(int input)
        {
            return input.ToString(CultureInfo.InvariantCulture);
        }

        internal static void Error(string message)
        {
            Console.WriteLine("Error: {0}", message);
            Debug.Assert(false, message);
        }
    }
}
