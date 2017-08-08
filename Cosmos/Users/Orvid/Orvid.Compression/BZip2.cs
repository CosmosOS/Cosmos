using System.IO;
using Orvid.Compression.Streams;

namespace Orvid.Compression
{
    public static class BZip2
    {
        public static byte[] Decompress(byte[] data)
        {
            MemoryStream ot = new MemoryStream();
            BZip2InputStream CompInStream = new BZip2InputStream(new MemoryStream(data));
            int ch = CompInStream.ReadByte();
            while (ch != -1)
            {
                ot.WriteByte((byte)ch);
                ch = CompInStream.ReadByte();
            }
            return ot.GetBuffer();
        }

        public static byte[] Compress(byte[] data, int blockSize)
        {
            MemoryStream strm = new MemoryStream(data);
            MemoryStream ostrm = new MemoryStream();
            BZip2OutputStream bzos = new BZip2OutputStream(ostrm, blockSize);
            int ch = strm.ReadByte();
            while (ch != -1)
            {
                bzos.WriteByte((byte)ch);
                ch = strm.ReadByte();
            }
            return ostrm.GetBuffer();
        }
    }
}
