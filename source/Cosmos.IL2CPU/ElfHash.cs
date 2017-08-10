using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Cosmos.IL2CPU
{
    public class Elf32 : HashAlgorithm
    {
        // TODO: Review this when we start using .NET Standard 2.0
        protected byte[] HashValue;

        private UInt32 hash;

        public Elf32()
        {
            Initialize();
        }

        public override void Initialize()
        {
            hash = 0;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt32ToBigEndianBytes(hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static UInt32 Compute(UInt32 polynomial, UInt32 seed, byte[] buffer)
        {
            return CalculateHash(seed, buffer, 0, buffer.Length);
        }

        private static UInt32 CalculateHash(UInt32 seed, byte[] buffer, int start, int size)
        {
            UInt32 hash = seed;

            for (int i = start; i < size; i++)
                unchecked
                {
                    hash = (hash << 4) + buffer[i];
                    UInt32 work = (hash & 0xf0000000);
                    if (work != 0)
                        hash ^= (work >> 24);
                    hash &= ~work;
                }
            return hash;
        }

        private byte[] UInt32ToBigEndianBytes(UInt32 x)
        {
            return new byte[] {
			(byte)((x >> 24) & 0xff),
			(byte)((x >> 16) & 0xff),
			(byte)((x >> 8) & 0xff),
			(byte)(x & 0xff)
		};
        }
    }

    public class Elf64 : HashAlgorithm
    {
        // TODO: Review this when we start using .NET Standard 2.0
        protected byte[] HashValue;

        private UInt64 hash;

        public Elf64()
        {
            Initialize();
        }

        public override void Initialize()
        {
            hash = 0;
        }

        protected override void HashCore(byte[] buffer, int start, int length)
        {
            hash = CalculateHash(hash, buffer, start, length);
        }

        protected override byte[] HashFinal()
        {
            byte[] hashBuffer = UInt64ToBigEndianBytes(hash);
            this.HashValue = hashBuffer;
            return hashBuffer;
        }

        public override int HashSize
        {
            get { return 32; }
        }

        public static UInt64 Compute(UInt64 polynomial, UInt64 seed, byte[] buffer)
        {
            return CalculateHash(seed, buffer, 0, buffer.Length);
        }

        //not sure if this is valid.
        private static UInt64 CalculateHash(UInt64 seed, byte[] buffer, int start, int size)
        {
            UInt64 hash = seed;

            for (int i = start; i < size; i++)
                unchecked
                {
                    hash = (hash << 4) + buffer[i];
                    UInt64 work = (hash & 0xf000000000000000L);
                    if (work != 0)
                        hash ^= (work >> 56);
                    hash &= ~work;
                }
            return hash;
        }

        private byte[] UInt64ToBigEndianBytes(UInt64 x)
        {
            return new byte[]
            {
            (byte)((x >> 56) & 0xff),
            (byte)((x >> 48) & 0xff),
            (byte)((x >> 40) & 0xff),
            (byte)((x >> 32) & 0xff),
            (byte)((x >> 24) & 0xff),
			(byte)((x >> 16) & 0xff),
			(byte)((x >> 8) & 0xff),
			(byte)(x & 0xff)
		};
        }
    }

}
