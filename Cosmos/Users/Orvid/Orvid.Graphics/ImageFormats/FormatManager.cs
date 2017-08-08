using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.ImageFormats
{
    public class FormatManager
    {
        internal List<ImageFormat> Formats = new List<ImageFormat>();
        public FormatManager()
        {
            Formats.Add(new BmpImage());
            Formats.Add(new OIFImage());
            Formats.Add(new PngImage());
            Formats.Add(new VbpImage());
            Formats.Add(new JpegImage());
            Formats.Add(new TiffImage());
            Formats.Add(new TgaImage());
            Formats.Add(new XpmImage());
            Formats.Add(new PnmFamilyImage());
            Formats.Add(new DdsImage());
            Formats.Add(new PcxImage());
        }

    }
}
