using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuNodes.HAL.FileSystem.Base;

namespace DuNodes.HAL.FileSystem.Crypto
{
    public abstract class HashAlgorithm
    {
        public byte[] HashValue;

        public string getString(byte[] data)
        {
            string str = "";
            foreach (byte num in this.Calculate(data))
                str += Conversions.ByteToHex((int)num);
            return str;
        }

        public byte[] Calculate(string str)
        {
            byte[] numArray = new byte[str.Length];
            int index = 0;
            foreach (byte num in str)
            {
                numArray[index] = num;
                ++index;
            }
            return numArray;
        }

        public virtual byte[] Calculate(byte[] data)
        {
            return (byte[])null;
        }

        public virtual void Calculate(byte[] data, ref uint val)
        {
        }
    }
}
