using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
    public static class ImageManipulator
    {

        #region AntiAlias
        /// <summary>
        /// Run a basic Anti-Aliasing Algorithm on the specified image.
        /// </summary>
        /// <param name="o">The image to Anti-Alias.</param>
        /// <returns>The Anti-Aliased Image.</returns>
        public static Image AntiAlias(Image i)
        {
            Image o = new Image(i.Width, i.Height);

            for (uint y = 0; y < i.Height; y++)
            {
                for (uint x = 0; x < i.Width; x++)
                {
                    uint R = 0, G = 0, B = 0;
                    byte divBy = 0;
                    Pixel p = i.GetPixel(x, y);
                    R += p.R;
                    G += p.G;
                    B += p.B;
                    divBy++;

                    p = i.GetPixel(x - 1, y - 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x, y - 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x + 1, y - 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x - 1, y);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x + 1, y);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x - 1, y + 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x, y + 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x + 1, y + 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    o.SetPixel(x, y, new Pixel(((byte)(R / divBy)), ((byte)(G / divBy)), ((byte)(B / divBy)), 255));
                }
            }
            i.Dispose();
            return o;
        }
        #endregion

        #region Resize

        #region Enum
        public enum ScalingAlgorithm
        {
            NearestNeighbor, // 0.201676 Seconds
            Bilinear, // 1.079586 Seconds
            Bicubic, // 3.162943 Seconds
            //Bell,
            //Box,
            //Catmull_Rom,
            //Cosine,
            //Cubic_Convolution,
            //Cubic_Spline,
            //Hermite,
            //Lanczos3,
            //Lanczos8,
            //Mitchell,
            //Quadratic,
            //Quadratic_BSpline,
            //Triangle,
        }
        #endregion

        public static Image Resize(Image i, Vec2 outSize, ScalingAlgorithm method = ScalingAlgorithm.NearestNeighbor)
        {
            Image o;

            switch (method)
            {
                case ScalingAlgorithm.NearestNeighbor:
                    o = ResizeNearestNeighbor(i, outSize);
                    break;

                case ScalingAlgorithm.Bilinear:
                    o = ResizeBilinear(i, outSize);
                    break;

                case ScalingAlgorithm.Bicubic:
                    o = ResizeBicubic(i, outSize);
                    break;

                default:
                    o = ResizeNearestNeighbor(i, outSize);
                    break;
            }

            return o;
        }

        #region The Actual Scaling Algorithms


        #region Nearest Neighbor
        private static Image ResizeNearestNeighbor(Image i, Vec2 outSize)
        {
            Image o = new Image(outSize.X, outSize.Y);

            double x_ratio = i.Width / o.Width;
            double y_ratio = i.Height / o.Height;
            uint px, py;
            for (uint y = 0; y < o.Height; y++)
            {
                for (uint x = 0; x < o.Width; x++)
                {
                    px = (uint)Math.Floor(x * x_ratio);
                    py = (uint)Math.Floor(y * y_ratio);
                    o.SetPixel(x, y, i.GetPixel(px, py));
                }
            }

            return o;
        }
        #endregion

        #region Bi-Linear
        private static Image ResizeBilinear(Image i, Vec2 outSize)
        {
            Image o = new Image(outSize.X, outSize.Y);

            Pixel a, b, c, d;
            uint x, y;
            double x_ratio = ((double)(i.Width - 1)) / o.Width;
            double y_ratio = ((double)(i.Height - 1)) / o.Height;
            double x_diff, y_diff, red, green, blue, alpha;
            bool usea, useb, usec, used;
            for (uint ypos = 0; ypos < o.Height; ypos++)
            {
                for (uint xpos = 0; xpos < o.Width; xpos++)
                {
                    x = (uint)(x_ratio * xpos);
                    y = (uint)(y_ratio * ypos);
                    x_diff = (x_ratio * xpos) - x;
                    y_diff = (y_ratio * ypos) - y;

                    // Get the pixels, and set if they should be used.
                    a = i.GetPixel(x, y);
                    usea = (a.Empty ? false : true);
                    b = i.GetPixel(x + 1, y);
                    useb = (b.Empty ? false : true);
                    c = i.GetPixel(x, y + 1);
                    usec = (c.Empty ? false : true);
                    d = i.GetPixel(x + 1, y + 1);
                    used = (d.Empty ? false : true);

                    red = green = blue = alpha = 0;
                    
                    // Alpha
                    if (usea)
                        red += (a.R) * (1 - x_diff) * (1 - y_diff);
                    if (useb)
                        red += (b.R) * (x_diff) * (1 - y_diff);
                    if (usec)
                        red += (c.R) * (y_diff) * (1 - x_diff);
                    if (used)
                        red += (d.R) * (x_diff * y_diff);

                    // Green
                    if (usea)
                        green += (a.G) * (1 - x_diff) * (1 - y_diff);
                    if (useb)
                        green += (b.G) * (x_diff) * (1 - y_diff);
                    if (usec)
                        green += (c.G) * (y_diff) * (1 - x_diff);
                    if (used)
                        green += (d.G) * (x_diff * y_diff);

                    // Blue
                    if (usea)
                        blue += (a.B) * (1 - x_diff) * (1 - y_diff);
                    if (useb)
                        blue += (b.B) * (x_diff) * (1 - y_diff);
                    if (usec)
                        blue += (c.B) * (y_diff) * (1 - x_diff);
                    if (used)
                        blue += (d.B) * (x_diff * y_diff);
                    
                    // Alpha
                    if (usea)
                        alpha += (a.A) * (1 - x_diff) * (1 - y_diff);
                    if (useb)
                        alpha += (b.A) * (x_diff) * (1 - y_diff);
                    if (usec)
                        alpha += (c.A) * (y_diff) * (1 - x_diff);
                    if (used)
                        alpha += (d.A) * (x_diff * y_diff);


                    o.SetPixel(xpos, ypos, new Pixel((byte)red, (byte)green, (byte)blue, (byte)alpha));
                }
            }

            return o;
        }
        #endregion

        #region Bi-Cubic
        private static Image ResizeBicubic(Image i, Vec2 outSize)
        {
            Image o = new Image(outSize.X, outSize.Y);

            int height = i.Height,
                width = i.Width,
                newHeight = o.Height,
                newWidth = o.Width,
                ymax = height - 1,
                xmax = width - 1,
                ox1, oy1, ox2, oy2;
            double xFactor = (double)width / newWidth,
                yFactor = (double)height / newHeight,
                ox, oy, dx, dy, k1, k2, r, g, b, a,
                x8, a2, b2, c2, d2, xm1, xp1, xp2;
            for (int y = 0; y < newHeight; y++)
            {
                oy = (double)y * yFactor - 0.5f;
                oy1 = (int)oy;
                dy = oy - (double)oy1;

                for (int x = 0; x < newWidth; x++)
                {
                    ox = (double)x * xFactor - 0.5f;
                    ox1 = (int)ox;
                    dx = ox - (double)ox1;

                    r = g = b = a = 0;

                    for (int n = -1; n < 3; n++)
                    {
                        x8 = dy - (double)n;
                        if (x8 > 2.0)
                        {
                            k1 = 0.0;
                        }
                        else
                        {
                            xm1 = x8 - 1.0;
                            xp1 = x8 + 1.0;
                            xp2 = x8 + 2.0;
                            a2 = (xp2 <= 0.0) ? 0.0 : xp2 * xp2 * xp2;
                            b2 = (xp1 <= 0.0) ? 0.0 : xp1 * xp1 * xp1;
                            c2 = (x8 <= 0.0) ? 0.0 : x8 * x8 * x8;
                            d2 = (xm1 <= 0.0) ? 0.0 : xm1 * xm1 * xm1;
                            k1 = (0.16666666666666666667 * (a2 - (4.0 * b2) + (6.0 * c2) - (4.0 * d2)));
                        }


                        oy2 = oy1 + n;
                        if (oy2 < 0)
                            oy2 = 0;
                        if (oy2 > ymax)
                            oy2 = ymax;

                        for (int m = -1; m < 3; m++)
                        {
                            x8 = (double)m - dx;
                            if (x8 > 2.0)
                            {
                                k2 = 0.0;
                            }
                            else
                            {
                                xm1 = x8 - 1.0;
                                xp1 = x8 + 1.0;
                                xp2 = x8 + 2.0;
                                a2 = (xp2 <= 0.0) ? 0.0 : xp2 * xp2 * xp2;
                                b2 = (xp1 <= 0.0) ? 0.0 : xp1 * xp1 * xp1;
                                c2 = (x8 <= 0.0) ? 0.0 : x8 * x8 * x8;
                                d2 = (xm1 <= 0.0) ? 0.0 : xm1 * xm1 * xm1;
                                k2 = k1 * (0.16666666666666666667 * (a2 - (4.0 * b2) + (6.0 * c2) - (4.0 * d2)));
                            }

                            ox2 = ox1 + m;
                            if (ox2 < 0)
                                ox2 = 0;
                            if (ox2 > xmax)
                                ox2 = xmax;

                            Pixel srcPixel = i.GetPixel((uint)ox2, (uint)oy2);

                            r += k2 * srcPixel.R;
                            g += k2 * srcPixel.G;
                            b += k2 * srcPixel.B;
                            a += k2 * srcPixel.A;
                        }
                    }
                    o.SetPixel((uint)x, (uint)y, new Pixel((byte)r, (byte)g, (byte)b, (byte)a));
                }
            }

            return o;
        }
        #endregion


        #endregion

        #endregion

        #region HalveSize
        /// <summary>
        /// Halves the size of the given image.
        /// </summary>
        /// <param name="i">The image to Halve the size of.</param>
        /// <returns>The half-size image.</returns>
        public static Image HalveSize(Image i)
        {
            Image o = new Image(i.Width / 2, i.Height / 2);

            for (uint y = 0; y < i.Height; y = y + 2)
            {
                for (uint x = 0; x < i.Width; x = x + 2)
                {
                    uint R = 0, G = 0, B = 0;
                    byte divBy = 0;
                    Pixel p = i.GetPixel(x, y);
                    R += p.R;
                    G += p.G;
                    B += p.B;
                    divBy++;

                    p = i.GetPixel(x + 1, y);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x, y + 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    p = i.GetPixel(x + 1, y + 1);
                    if (!p.Empty)
                    {
                        R += p.R;
                        G += p.G;
                        B += p.B;
                        divBy++;
                    }

                    o.SetPixel(x / 2, y / 2, new Pixel(((byte)(R / divBy)), ((byte)(G / divBy)), ((byte)(B / divBy)), 255));
                }
            }
            i.Dispose();
            return o;
        }
        #endregion
    }
}
