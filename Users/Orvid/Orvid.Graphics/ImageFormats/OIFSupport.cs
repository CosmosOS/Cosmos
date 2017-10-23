using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class OIFImage : ImageFormat
    {

        public override void Save(Image i, System.IO.Stream dest)
        {
            MemoryStream m = new MemoryStream();
            m.WriteByte(0);
#warning Change this next byte to 255 if you update Image with UInt64 for width and height.
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteByte(0); // Write the 8 empty bytes at the start of the file.
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteByte(0);
            m.WriteByte(0);

            byte[] dat = BitConverter.GetBytes(i.Height); // Write the height.
            m.Write(dat, 0, dat.Length);
            dat = BitConverter.GetBytes(i.Width); // Write the width.
            m.Write(dat, 0, dat.Length);

            // Now to write the actual data.
            Pixel p;
            for (uint x = 0; x < i.Width; x++)
            {
                for (uint y = 0; y < i.Height; y++)
                {
                    p = i.GetPixel(x, y);
                    m.WriteByte(p.R);
                    m.WriteByte(p.G);
                    m.WriteByte(p.B);
                    m.WriteByte(p.A);
                }
            }
            dat = Orvid.Compression.LZMA.Compress(m.GetBuffer());
            dest.WriteByte(255);
            dest.Write(dat, 0, dat.Length);
        }

        private UInt32 ReadUInt32(byte[] data)
        {
            UInt32 r = 0;

            r += data[3];
            r <<= 8;
            r += data[2];
            r <<= 8;
            r += data[1];
            r <<= 8;
            r += data[0];

            return r;
        }

        public override Image Load(System.IO.Stream s)
        {

            if (s.ReadByte() == 255)
            {
                byte[] buf = new byte[s.Length - 1];
                s.Read(buf, 0, buf.Length);
                s = new MemoryStream(Orvid.Compression.LZMA.Decompress(buf));
            }
            else
            {
                s.Position = 0;
            }
            byte[] tmp = new byte[8];
            s.Read(tmp, 0, 8); // skip the 8 empty bytes at the start of the file.
            tmp = new byte[4];
            s.Read(tmp, 0, 4);
            uint Height = ReadUInt32(tmp); // Read the Height.
            s.Read(tmp, 0, 4);
            uint Width = ReadUInt32(tmp); // Read the Width.
            Image i = new Image((int)Width, (int)Height);
            byte r, g, b, a;
            for (uint x = 0; x < Width; x++)
            {
                for (uint y = 0; y < Height; y++)
                {
                    r = (byte)s.ReadByte();
                    g = (byte)s.ReadByte();
                    b = (byte)s.ReadByte();
                    a = (byte)s.ReadByte();
                    i.SetPixel(x, y, new Pixel(r, g, b, a));
                }
            }
            return i;
        }
    }
}
