using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using au.id.micolous.libs.DDSReader;

namespace Orvid.Graphics.ImageFormats
{
    public class DdsImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            //byte[] b = DDS.Encode((System.Drawing.Bitmap)i, "dxt5");
            //dest.Write(b, 0, b.Length);
            //b = null;
        }

        public override Image Load(Stream s)
        {
            DDSImage im = new DDSImage(s);
            return (Image)im.BitmapImage;
        }
    }
}
