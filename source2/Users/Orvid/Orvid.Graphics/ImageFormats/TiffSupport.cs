using System;
using System.Collections.Generic;
using System.Text;
using BitMiracle.LibTiff;
using System.IO;

namespace Orvid.Graphics.ImageFormats
{
    public class TiffImage : ImageFormat
    {
        public override void Save(Image i, Stream dest)
        {
#warning TODO: Implement
            throw new NotImplementedException();
        }

        public override Image Load(Stream s)
        {
            Tiff tif = Tiff.Open(s);
            FieldValue[] value = tif.GetField(TiffTag.ImageWidth);
            int width = value[0].ToInt();
            value = tif.GetField(TiffTag.ImageLength);
            int height = value[0].ToInt();
            int[] raster = new int[height * width];
            tif.ReadRGBAImage(width, height, raster);
            Image i = new Image(width, height);
            int rgba, rasterOffset;
            byte r, g, b, a;
            for (uint y = 0; y < i.Height; y++)
            {
                rasterOffset = (int)(y * i.Width);
                for (uint x = 0; x < i.Width; x++)
                {
                    rgba = raster[rasterOffset++];
                    r = (byte)(rgba & 0xff);
                    g = (byte)((rgba >> 8) & 0xff);
                    b = (byte)((rgba >> 16) & 0xff);
                    a = (byte)((rgba >> 24) & 0xff);
                    i.SetPixel(x, y, new Pixel(r, g, b, a));
                }
            }
            // Need to flip the image.
            Image i2 = new Image(width, height);
            uint y2 = 0;
            for (uint y = (uint)(i.Height - 1); y >= 0 && y < i.Height; y--)
            {
                for (uint x = 0; x < i.Width; x++)
                {
                    i2.SetPixel(x, y2, i.GetPixel(x, y));
                }
                y2++;
            }
            i.Dispose();
            return i2;
        }
    }
}
