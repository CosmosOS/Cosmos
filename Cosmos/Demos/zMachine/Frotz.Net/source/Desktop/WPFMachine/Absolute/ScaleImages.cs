// TODO This can probably be remove
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Drawing;

namespace WPFMachine.Absolute
{
    internal static class ScaleImages
    {
        internal static byte[] Scale(byte[] img, int scale)
        {
            var ms = new MemoryStream(img);
            Image i = Image.FromStream(ms);
            Bitmap b = new Bitmap(i.Width * scale, i.Height * scale);

            Graphics g = Graphics.FromImage(b);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            g.DrawImage(i, new Rectangle(0, 0, b.Width, b.Height),
                new Rectangle(0, 0, i.Width, i.Height),
                GraphicsUnit.Pixel);

            ms = new MemoryStream();
            b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }
    }
}
