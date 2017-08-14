using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BitMiracle.LibJpeg;

namespace Orvid.Graphics.ImageFormats
{
    public class JpegImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            BitMiracle.LibJpeg.JpegImage j = BitMiracle.LibJpeg.JpegImage.FromBitmap((System.Drawing.Bitmap)i);
            CompressionParameters c = new CompressionParameters();
            c.Quality = 100;
            c.SimpleProgressive = false;
            j.WriteJpeg(dest, c);
            j.Dispose();
            System.GC.Collect();
        }

        public override Image Load(Stream s)
        {
            BitMiracle.LibJpeg.JpegImage j = new BitMiracle.LibJpeg.JpegImage(s);
            Image i = (Image)j.ToBitmap();
            j.Dispose();
            System.GC.Collect();
            return i;
        }
    }
}
