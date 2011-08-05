using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using BitMiracle.LibJpeg;

namespace Orvid.Graphics.ImageFormats
{
    public class JpgImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            JpegImage j = JpegImage.FromBitmap((System.Drawing.Bitmap)i);
            j.WriteJpeg(dest);
        }

        public override Image Load(Stream s)
        {
            JpegImage j = new JpegImage(s);
            return (Image)j.ToBitmap();
        }
    }
}
