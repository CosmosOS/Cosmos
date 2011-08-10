using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SaveGameEditor;

namespace Orvid.Graphics.ImageFormats
{
    public class DdsImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            byte[] b = DDS.Encode((System.Drawing.Bitmap)i, "dxt5");
            dest.Write(b, 0, b.Length);
            b = null;
        }

        public override Image Load(Stream s)
        {
            byte[] b = new byte[s.Length];
            s.Read(b, 0, (int)s.Length);
            String str;
            Image i = (Image)(System.Drawing.Bitmap)DDS.Decode(b, out str);
            b = null;
            return i;
        }
    }
}
