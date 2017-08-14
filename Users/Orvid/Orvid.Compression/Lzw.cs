using System;
using Orvid.Compression.Streams;
using System.IO;

namespace Orvid.Compression
{
    public static class Lzw
    {
        public static byte[] Decompress(byte[] data)
        {
            MemoryStream ot = new MemoryStream();
            LzwInputStream CompInStream = new LzwInputStream(new MemoryStream(data));
            int ch = CompInStream.ReadByte();
            while (ch != -1)
            {
                ot.WriteByte((byte)ch);
                ch = CompInStream.ReadByte();
            }
            return ot.GetBuffer();
        }

        //public static byte[] Compress(byte[] data, int blockSize)
        //{
        //    MemoryStream strm = new MemoryStream(data);
        //    MemoryStream ostrm = new MemoryStream();
        //    LzwOutputStream bzos = new LzwOutputStream(ostrm, blockSize);
        //    int ch = strm.ReadByte();
        //    while (ch != -1)
        //    {
        //        bzos.WriteByte((byte)ch);
        //        ch = strm.ReadByte();
        //    }
        //    return ostrm.GetBuffer();
        //}
    }
}
