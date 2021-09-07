using System;
using System.Collections.Generic;
using System.IO;
using Cosmos.HAL.BlockDevice;

namespace Cosmos.System.Tests
{
    class TestBlockDevice : BlockDevice
    {
        private byte[] mData;

        public TestBlockDevice()
        {
            LoadTestData();

            mBlockSize = 512;
            mBlockCount = (ulong) (mData.Length / (int) mBlockSize);
        }

        private void LoadTestData()
        {
            var xList = new List<byte>();

            using (var xReader = new StreamReader("../../../../Data/disk.txt"))
            {
                while (!xReader.EndOfStream)
                {
                    string xLine = xReader.ReadLine();
                    if (!string.IsNullOrWhiteSpace(xLine))
                    {
                        xLine = xLine.Replace(" ", "");
                        xList.AddRange(StringToByteArray(xLine));
                    }
                }
                xReader.Close();
            }

            for (int i = 0; i < 534610432 - xList.Count; i++)
            {
                xList.Add(0x00);
            }

            mData = xList.ToArray();
        }

        private static byte[] StringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return bytes;
        }

        public override void ReadBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            aData = NewBlockArray((uint) aBlockCount);
            Array.Copy(mData, (long) (aBlockNo * BlockSize), aData, 0, (long) (aBlockCount * BlockSize));
        }

        public override void WriteBlock(ulong aBlockNo, ulong aBlockCount, ref byte[] aData)
        {
            Array.Copy(aData, 0, mData, (long)(aBlockNo * BlockSize), (long)(aBlockCount * BlockSize));
        }
    }
}