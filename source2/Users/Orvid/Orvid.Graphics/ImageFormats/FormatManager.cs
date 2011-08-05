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
            Formats.Add(new OIFImage());
            Formats.Add(new VbpImage());
        }

    }
}
