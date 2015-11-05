using System;
using System.IO;

namespace Orvid.Compression
{
    internal struct WiltRangeDecoder
    {
        public UInt32 Range;
        public UInt32 Code;
        public Stream Strm;
    }

    internal class Wilt
    {
        static void InitRangeDecoder(WiltRangeDecoder self, Stream fh)
        {
            self.Range = 0xffffffff;
            self.Code = 0;
            self.Strm = fh;

            for (int i = 0; i < 4; i++)
                self.Code = (UInt32)((self.Code << 8) | (uint)fh.ReadByte());
        }

        static void NormalizeRangeDecoder(WiltRangeDecoder self)
        {
            while (self.Range < 0x1000000)
            {
                self.Code = (UInt32)((self.Code << 8) | (uint)self.Strm.ReadByte());
                self.Range <<= 8;
            }
        }

        static uint ReadBitAndUpdateWeight(WiltRangeDecoder self, UInt16 weight, uint shift)
        {
            NormalizeRangeDecoder(self);

            uint threshold = (self.Range >> 12) * weight;

            if (self.Code < threshold)
            {
                self.Range = threshold;
                weight += (ushort)((0x1000 - weight) >> (int)shift);
                return 0;
            }
            else
            {
                self.Range -= threshold;
                self.Code -= threshold;
                weight -= (ushort)(weight >> (int)shift);
                return 1;
            }
        }

        static uint ReadUniversalCode(WiltRangeDecoder self, ushort[] weights1, uint shift1, ushort[] weights2, uint shift2)
        {
            int numbits = 0;
            while (ReadBitAndUpdateWeight(self, weights1[numbits], shift1) == 1)
            {
                numbits++;
            }
            if (numbits == 0)
            {
                return 0;
            }
            uint val = 1;
            for (int i = 0; i < numbits - 1; i++)
            {
                val = (val << 1) | ReadBitAndUpdateWeight(self, weights2[numbits - 1 - i], shift2);
            }

            return val;
        }

        static void CopyMemory(byte[] dest, int destindx, byte[] src, int srcindx, int length)
        {
            for (int i = 0; i < length; i++)
            {
                dest[destindx + i] = src[srcindx + i];
            }
        }

        void DecompressData(Stream fh, byte[] buf, uint size, int typeshift, int literalshift, uint lengthshift1, uint lengthshift2, uint offsetshift1, uint offsetshift2)
        {
            WiltRangeDecoder dec = new WiltRangeDecoder();
            InitRangeDecoder(dec, fh);

            ushort typeweight = 0x800;

            ushort[] lengthweights1 = new ushort[32];
            ushort[] lengthweights2 = new ushort[32];
            ushort[] offsetweights1 = new ushort[32];
            ushort[] offsetweights2 = new ushort[32];
            for (int i = 0; i < 32; i++)
            {
                lengthweights1[i] = lengthweights2[i] = offsetweights1[i] = offsetweights2[i] = 0x800;
            }

            ushort[] literalbitweights = new ushort[256];
            for (int i = 0; i < 256; i++)
            {
                literalbitweights[i] = 0x800;
            }

            int pos = 0;
            while (pos < size)
            {
                if (ReadBitAndUpdateWeight(dec, typeweight, (uint)typeshift) == 1)
                {
                    int length = (int)(ReadUniversalCode(dec, lengthweights1, lengthshift1, lengthweights2, lengthshift2) + 3);
                    int offs = (int)(ReadUniversalCode(dec, offsetweights1, offsetshift1, offsetweights2, offsetshift2) + 1);

                    CopyMemory(buf, pos, buf, pos - offs, length);
                    pos += length;
                }
                else
                {
                    int val = 1;
                    for (int i = 0; i < 8; i++)
                    {
                        int bit = (int)ReadBitAndUpdateWeight(dec, literalbitweights[val], (uint)literalshift);
                        val = (val << 1) | bit;
                    }
                    buf[pos++] = (byte)val;
                }
            }
        }

    }
}
