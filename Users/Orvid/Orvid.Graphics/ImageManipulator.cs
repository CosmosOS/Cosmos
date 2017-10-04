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
            Hq2x,
            Hq3x,
            Hq4x,
            //Lq2x,
            //Lq3x,
            //Lq4x,
            //Scale2x,
            //Scale3x,
        }
        #endregion

        /// <summary>
        /// Resizes an image using the given algorithm.
        /// </summary>
        /// <param name="i">Input image.</param>
        /// <param name="outSize">Output size. (If Applicable)</param>
        /// <param name="method">Scaling method.</param>
        /// <returns>The resized image.</returns>
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

                case ScalingAlgorithm.Hq2x:
                    o = ResizeHq2x(i);
                    break;

                case ScalingAlgorithm.Hq3x:
                    o = ResizeHq3x(i);
                    break;

                case ScalingAlgorithm.Hq4x:
                    o = ResizeHq4x(i);
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

        #region Hqx

        #region 2x
        private static Image ResizeHq2x(Image i)
        {
            return (Image)Hqx.HqxSharp.Scale2((System.Drawing.Bitmap)i);
        }
        #endregion

        #region 3x
        private static Image ResizeHq3x(Image i)
        {
            return (Image)Hqx.HqxSharp.Scale3((System.Drawing.Bitmap)i);
        }
        #endregion

        #region 4x
        private static Image ResizeHq4x(Image i)
        {
            return (Image)Hqx.HqxSharp.Scale4((System.Drawing.Bitmap)i);
        }
        #endregion

        #endregion

        #endregion

        #endregion

        #region Greyscale
        public static Image ConvertToGreyscale(Image i)
        {
            Image i2 = new Image(i.Width, i.Height);

            uint Len = (uint)(i.Height * i.Width);
            byte b;
            Pixel p;
            for (uint t = 0; t < Len; t++)
            {
                p = i.Data[t];
                b = (byte)((0.2125 * p.R) + (0.7154 * p.G) + (0.0721 * p.B));
                i2.Data[t] = new Pixel(b, b, b, 255);
            }

            return i2;
        }
        #endregion

        #region Invert Colors
        public static Image InvertColors(Image i)
        {
            Image i2 = new Image(i.Width, i.Height);

            uint Len = (uint)(i.Height * i.Width);
            Pixel p;
            for (uint t = 0; t < Len; t++)
            {
                p = i.Data[t];
                i2.Data[t] = new Pixel((byte)(255 - p.R), (byte)(255 - p.G), (byte)(255 - p.B), 255);
            }

            return i2;
        }
        #endregion

        #region ReverseRGB
        public static Image ReverseRGB(Image i)
        {
            Image i2 = new Image(i.Width, i.Height);

            uint Len = (uint)(i.Height * i.Width);
            Pixel p;
            for (uint t = 0; t < Len; t++)
            {
                p = i.Data[t];
                i2.Data[t] = new Pixel(p.B, p.G, p.R, 255);
            }

            return i2;
        }
        #endregion

        #region AddNoise
        public enum NoiseGenerationMethod
        {
            Additive,
            SaltAndPepper,
        }
        public static Image AddNoise(Image i, BoundingBox bounds, int strength = 10, NoiseGenerationMethod method = NoiseGenerationMethod.Additive)
        {
            Image im = null;
            switch (method)
            {
                case NoiseGenerationMethod.Additive:
                    im = AddAdditiveNoise(i, bounds, strength);
                    break;

                //case NoiseGenerationMethod.SaltAndPepper:
                //    im = AddSaltAndPepperNoise(i, bounds);
                //    break;
            }
            return im;
        }

        private static Image AddAdditiveNoise(Image i, BoundingBox b, int strength)
        {
            Random r = new Random(0);
            uint startY = (uint)b.Top;
            uint stopY = (uint)(startY + b.Height);

            uint startX = (uint)b.Left;
            uint stopX = (uint)(startX + b.Width);

            Pixel p;
            for (uint y = startY; y < stopY; y++)
            {
                for (uint x = startX; x < stopX; x++)
                {
                    p = i.GetPixel(x, y);
                    i.SetPixel(x, y, new Pixel((byte)Math.Max(0, Math.Min(255, p.R + (r.NextDouble() * strength - strength))), (byte)Math.Max(0, Math.Min(255, p.G + (r.NextDouble() * strength - strength))), (byte)Math.Max(0, Math.Min(255, p.B + (r.NextDouble() * strength - strength))), 255));
                }
            }
            return i;
        }

        #endregion

    }
}
