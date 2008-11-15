using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FrodeTest.Test
{
    class BitConverterTest
    {
        public static void RunTest()
        {
            Check.Text = "BitConverter.DoubleToInt64Bits";
            Check.Validate(BitConverter.DoubleToInt64Bits(123456.78) == 4683220298531686318);
            Check.Text = "BitConverter.GetBytes(char)";
            Check.Validate(BitConverter.GetBytes('a')[0] == 97);
            Check.Text = "BitConverter.ToBoolean";
            Check.Validate(BitConverter.ToBoolean(new byte[] { 1 }, 0) == true);
            Check.Validate(BitConverter.ToBoolean(new byte[] { 0 }, 0) == false);
            Check.Text = "BitConverter.ToString()";
            Check.Validate(BitConverter.ToString(new byte[] {1, 2, 3}).Equals("01-02-03"));
            Check.Text = "BitConverter.ToUInt32";
            Check.Validate(BitConverter.ToUInt32(new byte[] { 1, 1, 0, 0 }, 0) == 257);
            
        }
    }
}
