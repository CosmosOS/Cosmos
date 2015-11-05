using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuNodes.HAL.FileSystem.Crypto
{
    public class Crc32 : HashAlgorithm
    {
        public const uint DefaultPolynomial = 3988292384U;
        public const uint DefaultSeed = 4294967295U;
        private uint hash;
        private uint seed;
        private uint[] table;
        private static uint[] defaultTable;

        public Crc32()
        {
            this.table = Crc32.InitializeTable(3988292384U);
            this.seed = uint.MaxValue;
            this.hash = this.seed;
        }

        public Crc32(uint polynomial, uint seed)
        {
            this.table = Crc32.InitializeTable(polynomial);
            this.seed = seed;
            this.hash = seed;
        }

        public override void Calculate(byte[] data, ref uint val)
        {
            val = Crc32.Compute(data);
        }

        private void HashCore(byte[] buffer, int start, int length)
        {
            this.hash = Crc32.CalculateHash(this.table, this.hash, buffer, start, length);
        }

        private byte[] HashFinal()
        {
            byte[] numArray = this.UInt32ToBigEndianBytes(~this.hash);
            this.HashValue = numArray;
            return numArray;
        }

        public static uint Compute(byte[] buffer)
        {
            return ~Crc32.CalculateHash(Crc32.InitializeTable(3988292384U), uint.MaxValue, buffer, 0, buffer.Length);
        }

        public static uint Compute(uint seed, byte[] buffer)
        {
            return ~Crc32.CalculateHash(Crc32.InitializeTable(3988292384U), seed, buffer, 0, buffer.Length);
        }

        public static uint Compute(uint polynomial, uint seed, byte[] buffer)
        {
            return ~Crc32.CalculateHash(Crc32.InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
        }

        private static uint[] InitializeTable(uint polynomial)
        {
            if ((int)polynomial == -306674912 && Crc32.defaultTable != null)
                return Crc32.defaultTable;
            uint[] numArray = new uint[256];
            for (int index1 = 0; index1 < 256; ++index1)
            {
                uint num = (uint)index1;
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    if (((int)num & 1) == 1)
                        num = num >> 1 ^ polynomial;
                    else
                        num >>= 1;
                }
                numArray[index1] = num;
            }
            if ((int)polynomial == -306674912)
                Crc32.defaultTable = numArray;
            return numArray;
        }

        private static uint CalculateHash(uint[] table, uint seed, byte[] buffer, int start, int size)
        {
            uint num = seed;
            for (int index = start; index < size; ++index)
                num = num >> 8 ^ table[(int) ((uint)buffer[index] ^ num & (uint)byte.MaxValue)];
            return num;
        }

        private byte[] UInt32ToBigEndianBytes(uint x)
        {
            return new byte[4]
            {
        (byte) (x >> 24 & (uint) byte.MaxValue),
        (byte) (x >> 16 & (uint) byte.MaxValue),
        (byte) (x >> 8 & (uint) byte.MaxValue),
        (byte) (x & (uint) byte.MaxValue)
            };
        }
    }
}
