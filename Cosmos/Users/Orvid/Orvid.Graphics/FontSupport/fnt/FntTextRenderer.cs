using System;

namespace Orvid.Graphics.FontSupport.fnt
{
	internal class FntTextRenderer : ITextRenderer
    {
        private FntLoader ldr;

        public FntTextRenderer(FntLoader loadr)
        {
            this.ldr = loadr;
        }

        public override void Render(Image im, BoundingBox clip, AffineTransform tx, string text, Vec2 loc, Pixel color)
        {
            if (text == null || text.Length == 0)
                return;

            int offset = 0, 
                x = 0, y = 0,
                FontDepth = ldr.GetDepth(),
                x_min = int.MaxValue,
                y_min = int.MaxValue,
                x_max = int.MinValue,
                y_max = int.MinValue,
                charsCount = text.Length,
                fHeight, scan, fg_r, fg_g, 
                fg_b, bx, by, offsetLine,
                fPixel, px, py, bg_r, 
                bg_g, bg_b, r, g, b
                ;
            int[] fData;
            Vec2 src, dst;
            Rectangle glyph_box = new Rectangle();
            FntGlyph glyph;
            Image img;

            if ((ldr != null) && (charsCount > 0))
            {
                char[] chrs = text.ToCharArray();
                int mH = 0;
                int mY = 0;
                int mxY = 0;
                int tW = 0;
                int tH = 0;
                for (uint m = 0; m < text.Length; m++)
                {
                    glyph = ldr.GetGlyph(chrs[m]);
                    glyph_box = glyph.GetBounds();
                    if (glyph_box.Height > mH)
                    {
                        mH = glyph_box.Height + 2;
                    }
                    if (glyph_box.Y < mY)
                    {
                        mY = glyph_box.Y;
                    }
                    if (glyph_box.Y > mxY)
                    {
                        mxY = glyph_box.Y;
                    }
                    tW += glyph_box.Width + glyph_box.X + 2;
                }

                tH = mH + (mxY - mY);
                y = mH + -mY;
                img = new Image(tW, tH);


                float f_max = (1 << FontDepth) - 1;
                if (f_max == 0)
                    f_max = 1;

                glyph_box = new Rectangle();
                src = new Vec2();
                dst = new Vec2();

                //throw new Exception();

                for (int i = 0; i < charsCount; i++)
                {
                    glyph = ldr.GetGlyph(chrs[i]);
                    //throw new Exception();
                    if (glyph == null)
                    {
                        continue;
                    }
                    
                    glyph_box = glyph.GetBounds();

                    fHeight = glyph_box.Height;
                    fData = glyph.Data;
                    scan = fData.Length / fHeight;

                    //throw new Exception();
                    fg_r = color.R;
                    fg_g = color.G;
                    fg_b = color.B;

                    //throw new Exception();
                    //box location
                    bx = x + offset + glyph_box.X;
                    by = y - fHeight - glyph_box.Y;

                    throw new Exception();
                    for (int k = 0; k < fHeight; k++)
                    {
                        offsetLine = k * scan;
                        for (int j = 0; j < scan; j++)
                        {
                            fPixel = fData[offsetLine + j];
                            if (fPixel != 0)
                            {

                                //pixel location
                                px = bx + j;
                                py = by + k;

                                if (tx != null)
                                {
                                    //src.setLocation(px, py);
                                    //tx.transform(src, dst);
#warning TODO: Add support for this.
                                    //px = dst.X;
                                    //py = dst.Y;
                                }

                                //clip
                                if (clip == null || clip.Contains(px, py))
                                {
                                    //compute color
                                    Pixel bg_color = img.GetPixel((uint)px, (uint)py);

                                    bg_r = bg_color.R;
                                    bg_g = bg_color.G;
                                    bg_b = bg_color.B;

                                    //todo improve this pixel composition

                                    float alpha = fPixel / f_max;

                                    r = bg_r + ((int)((fg_r - bg_r) * alpha)) & 0xFF;
                                    g = bg_g + ((int)((fg_g - bg_g) * alpha)) & 0xFF;
                                    b = bg_b + ((int)((fg_b - bg_b) * alpha)) & 0xFF;

                                    Pixel nw = new Pixel((byte)r, (byte)g, (byte)b, 255);

                                    img.SetPixel((uint)px, (uint)py, nw);

                                    if (x_min > px)
                                        x_min = px;
                                    if (y_min > py)
                                        y_min = py;
                                    if (x_max < px)
                                        x_max = px;
                                    if (y_max < py)
                                        y_max = py;
                                }
                            }
                        }
                    }
                    offset += glyph.Width;
                }
                //img = ImageManipulator.Resize(img, new Vec2(img.Width * 8, img.Height * 8), ImageManipulator.ScalingAlgorithm.Bicubic);
                //img = ImageManipulator.Resize(img, Vec2.Zero, ImageManipulator.ScalingAlgorithm.Hq2x);
                //img = ImageManipulator.Resize(img, Vec2.Zero, ImageManipulator.ScalingAlgorithm.Hq4x);
                img.FloodFill(new Vec2(3,3), new Pixel(true), Colors.Green);
                im.DrawImage(loc, img);
            }
        }
    }
}
