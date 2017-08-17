using System;
using System.Collections.Generic;
using System.Text;
using ShaniSoft.Drawing;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class PnmFamilyImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
            Pnm.WritePnm(dest, (System.Drawing.Bitmap)i, PnmEncoding.BinaryEncoding, Pnm.DetectType((System.Drawing.Bitmap)i));
        }

        public override Image Load(Stream s)
        {
            return (Image)Pnm.ReadPnm(s);
        }
    }
}
