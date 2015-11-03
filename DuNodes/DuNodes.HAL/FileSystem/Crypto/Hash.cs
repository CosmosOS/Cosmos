using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuNodes.HAL.FileSystem.Base;

namespace DuNodes.HAL.FileSystem.Crypto
{
    public class Hash
    {
        public static int GHash(string s)
        {
            int num1 = 23;
            int num2 = 0;
            foreach (char ch in s.ToCharArray())
                num1 = num1 ^ num2 ^ (int)ch;
            return num1 ^ s.Length;
        }

        public static string PasswordHASH(string str)
        {
            int length = 4;
            int[] numArray = new int[length];
            int index1 = 0;
            char[] chArray = str.ToCharArray();
            int num1 = Hash.GHash(str);
            int num2 = 1;
            int num3 = 2;
            foreach (byte num4 in str)
            {
                num3 = (int)num4 ^ num3;
                chArray[index1] = (char)num4;
                ++index1;
            }
            int num5 = num1 ^ num3;
            int index2 = 0;
            for (int index3 = 0; index3 < length; ++index3)
            {
                for (int index4 = 0; index4 < chArray.Length; ++index4)
                {
                    if (index2 > length - 1)
                        index2 = 0;
                    int num4 = (int)(byte)chArray[index4] ^ num3;
                    numArray[index2] = str.Length ^ index4;
                    if (num4 != 0)
                    {
                        numArray[index2] = numArray[index2] ^ num5;
                        numArray[index2] = numArray[index2] << 1;
                        numArray[index2] = numArray[index2] ^ num2;
                        numArray[index2] = numArray[index2] << 4;
                        numArray[index2] = numArray[index2] ^ num3;
                        numArray[index2] = numArray[index2] << 2;
                        numArray[index2] = numArray[index2] + num4;
                    }
                    ++index2;
                    ++num2;
                }
            }
            int index5 = 0;
            int num6 = 0;
            for (int index3 = 0; index3 < chArray.Length; ++index3)
            {
                if (index5 > length - 1)
                    index5 = 0;
                int num4 = (int)(byte)((int)chArray[index3] ^ num5 ^ num3);
                byte[] bytes = BitConverter.GetBytes(num4);
                bytes[0] = (byte)((uint)bytes[0] ^ (uint)(num4 & num6));
                bytes[1] = (byte)((uint)bytes[1] & (uint)(byte)num6);
                bytes[2] = (byte)((uint)bytes[2] | (uint)bytes[1] ^ (uint)num4);
                bytes[3] = (byte)((uint)bytes[3] ^ ((uint)bytes[1] ^ (uint)bytes[2]));
                numArray[index5] = num5 ^ BitConverter.ToInt32(bytes, 0);
                numArray[index5] = numArray[index5] ^ ((int)bytes[0] * (int)bytes[1] ^ (int)bytes[2]);
                for (int index4 = 0; index4 < bytes.Length; ++index4)
                {
                    if ((int)bytes[index4] == 0)
                        bytes[index4] = (byte)(num4 ^ num6);
                }
                ++index5;
                ++num6;
            }
            string str1 = "";
            for (int index3 = 0; index3 < length; ++index3)
            {
                foreach (byte num4 in BitConverter.GetBytes(numArray[index3]))
                    str1 += Conversions.ByteToHex((int)num4);
            }
            return str1;
        }

        public static int GHash(byte[] s)
        {
            int num1 = 23;
            int num2 = 0;
            foreach (byte num3 in s)
            {
                ++num2;
                num1 += num2 | (int)num3;
            }
            return num1 * s.Length;
        }

        public static int getCRC(byte[] s)
        {
            return Hash.GHash(s);
        }
    }
}
