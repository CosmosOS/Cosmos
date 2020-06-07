using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Common
{
    /// <summary>
    /// Helper class for converting byte array to string and vice versa.
    /// </summary>
    static public class ByteToString
    {
        /// <summary>
        /// Parse numeric (positive) string to byte array.
        /// </summary>
        /// <param name="str">A string to be converted to byte array.</param>
        /// <exception cref="Exception">thrown when the passed string length is 0, string length is not divisible by 3 or the string not numeric</exception>
        /// <returns>Byte array.</returns>
        public static byte[] StrToByteArray(string str)
        {
            if (str.Length == 0 || !StringHelper.IsNumeric(str) || str.Length % 3 != 0)
                throw new Exception("Invalid string value in StrToByteArray");

            byte val;
            byte[] byteArr = new byte[str.Length / 3];
            int i = 0;
            int j = 0;
            do
            {
                val = byte.Parse(str.Substring(i, 3));
                byteArr[j++] = val;
                i += 3;
            }
            while (i < str.Length);
            return byteArr;
        }

        /// <summary>
        /// Parse byte array to string.
        /// To be used on byte arrays created by StrToByteArray method.
        /// </summary>
        /// <param name="byteArr">A byte array to be converted to string.</param>
        /// <returns>String value.</returns>
        public static string ByteArrToString(byte[] byteArr)
        {
            byte val;
            string tempStr = "";
            for (int i = 0; i <= byteArr.GetUpperBound(0); i++)
            {
                val = byteArr[i];
                if (val < (byte)10)
                    tempStr += "00" + val.ToString();
                else if (val < (byte)100)
                    tempStr += "0" + val.ToString();
                else
                    tempStr += val.ToString();
            }
            return tempStr;
        }
    }
}
