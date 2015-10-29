using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class TgaImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            dest.WriteByte(0); // Write No Image ID
            dest.WriteByte(0); // Write No Color-Map
            dest.WriteByte(2); // Write Image Type (Uncompressed- True Color)
            byte[] dat = new byte[5];
            dest.Write(dat, 0, 5); // Write Color-Map Spec

            dest.WriteByte(0);
            dest.WriteByte(0); // Write X-Origin
            dest.WriteByte(0);
            dest.WriteByte(0); // Write Y-Origin
            dat = BitConverter.GetBytes((UInt16)i.Width);
            dest.Write(dat, 0, 2); // Write the Width
            dat = BitConverter.GetBytes((UInt16)i.Height);
            dest.Write(dat, 0, 2); // Write the Height
            dest.WriteByte(32); // Write the Color-Depth

            byte ImageDescriptor = 0;
            ImageDescriptor |= 8; // Set 8-Bit Alpha data.
            ImageDescriptor |= (byte)(1 << 5); // Set Top-Left Image origin
            dest.WriteByte(ImageDescriptor); // Write Image Descriptor Byte

            UInt32 len = (uint)(i.Width * i.Height);
            Pixel p;
            for (int loc = 0; loc < len; loc++) // Write the Image Data.
            {
                p = i.Data[loc];
                dest.WriteByte(p.B);
                dest.WriteByte(p.G);
                dest.WriteByte(p.R);
                dest.WriteByte(p.A);
            }
        }

        public override Image Load(Stream s)
        {
            Paloma.TargaImage t = new Paloma.TargaImage(s);
            Image i = (Image)t.Image;
            t.Dispose();
            return i;
        }
    }
}
