//#define COSMOSDEBUG

namespace Cosmos.System.Graphics
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }
    }

    public struct Color
    {
        byte _r, _g, _b, _a;

        public Color(byte a, byte r, byte g, byte b)
        {
            this._r = r;
            this._g = g;
            this._b = b;
            this._a = a;
        }
        public Color(byte r, byte g, byte b)
        {
            this._r = r;
            this._g = g;
            this._b = b;
            this._a = 255;
        }
        public byte R
        {
            get { return this._r; }
        }
        public byte G
        {
            get { return this._g; }
        }
        public byte B
        {
            get { return this._b; }
        }
        public byte A
        {
            get { return this._a; }
        }
        public byte alpha
        {
            get { return this._a; }
        }
        public int ToArgb()
        {
            return unchecked((int)(
                (_a << 24) |
                (_b << 16) |
                (_g << 8) |
                (_r)));

        }
        public static Color FromArgb(int value)
        {
            return new Color(
                 (byte)((value >> 24) & 0xFF),
                 (byte)((value) & 0xFF),
                 (byte)((value >> 8) & 0xFF),
                 (byte)((value >> 16) & 0xFF));
        }
        public byte red { get { return this._r; } }
        public byte green { get { return this._g; } }
        public byte blue { get { return this._b; } }


        public static Color FromArgb(int a, Color c)
        {
            return new Color((byte)a, c.R, c.G, c.B);
        }
        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(int r, int g, int b)
        {
            return new Color(255, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(float a, float r, float g, float b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                Color c = (Color)obj;
                return c._a == this._a &&
                    c._b == this._b &&
                    c._r == this._r &&
                    c._g == this._g;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static readonly Color Empty = new Color(0, 0, 0, 0);
        public static readonly Color Transparent = new Color(0, 255, 255, 255);
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(255, 0, 0, 0);
        public static readonly Color Blue = new Color(255, 0, 0, 255);
        public static readonly Color Aqua = new Color(255, 0, 255, 255);
        public static readonly Color Red = new Color(255, 255, 0, 0);
        public static readonly Color Yellow = new Color(255, 255, 255, 0);
        public static readonly Color LightGray = new Color(0xFF, 0xD3, 0xD3, 0xD3);
        public static readonly Color Gray = new Color(0xFF, 0x80, 0x80, 0x80);
        public static readonly Color Green = new Color(0xFF, 0x00, 0x80, 0x00);
        public static readonly Color OrangeRed = new Color(0xFF, 0xFF, 0x45, 0x00);//0xFF FF 45 00
        public static readonly Color DeepPink = new Color(0xFF, 0xFF, 0x14, 0x93);
        public static readonly Color Magenta = new Color(0xFF, 0xFF, 0, 0xFF);


        public static bool operator ==(Color c1, Color c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) ==
                   (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public static bool operator !=(Color c1, Color c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) !=
                  (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public int ToARGB()
        {
            return ((this._a << 24) | (this._r << 16) | (this._g << 8) | this._b);
        }
        public uint ToABGR()
        {
            return (uint)((this._a << 24) | (this._b << 16) | (this._g << 8) | this._r);
        }

        public byte Red0To255
        {
            get { return _r; }
        }
        public byte Green0To255
        {
            get { return _g; }
        }
        public byte Blue0To255
        {
            get { return _b; }
        }
        public byte Alpha0To255
        {
            get { return _a; }
        }




        public Color CreateGradient(Color another, float colorDistanceRatio)
        {
            //int ik = AggBasics.uround(colorDistanceRatio * BASE_SCALE); 
            //byte r = (byte)((int)(Red0To255) + ((((int)(another.Red0To255) - Red0To255) * ik) >> BASE_SHIFT));
            //byte g = (byte)((int)(Green0To255) + ((((int)(another.Green0To255) - Green0To255) * ik) >> BASE_SHIFT));
            //byte b = (byte)((int)(Blue0To255) + ((((int)(another.Blue0To255) - Blue0To255) * ik) >> BASE_SHIFT));
            //byte a = (byte)((int)(Alpha0To255) + ((((int)(another.Alpha0To255) - Alpha0To255) * ik) >> BASE_SHIFT));

            //from this color to another c color
            //colorDistance ratio [0-1]
            //new_color = old_color + diff

            byte r = (byte)(Red0To255 + (another.Red0To255 - this.Red0To255) * colorDistanceRatio);
            byte g = (byte)(Green0To255 + (another.Green0To255 - this.Green0To255) * colorDistanceRatio);
            byte b = (byte)(Blue0To255 + (another.Blue0To255 - this.Blue0To255) * colorDistanceRatio);
            byte a = (byte)(Alpha0To255 + (another.Alpha0To255 - this.Alpha0To255) * colorDistanceRatio);
            return new Color(a, r, g, b);
        }

        public static Color operator +(Color A, Color B)
        {
            byte r = (byte)((A._r + B._r) > 255 ? 255 : (A._r + B._r));
            byte g = (byte)((A._g + B._g) > 255 ? 255 : (A._g + B._g));
            byte b = (byte)((A._b + B._b) > 255 ? 255 : (A._b + B._b));
            byte a = (byte)((A._a + B._a) > 255 ? 255 : (A._a + B._a));
            return new Color(a, r, g, b);
        }

        public static Color operator -(Color A, Color B)
        {
            byte red = (byte)((A._r - B._r) < 0 ? 0 : (A._r - B._r));
            byte green = (byte)((A._g - B._g) < 0 ? 0 : (A._g - B._g));
            byte blue = (byte)((A._b - B._b) < 0 ? 0 : (A._b - B._b));
            byte alpha = (byte)((A._a - B._a) < 0 ? 0 : (A._a - B._a));
            return new Color(alpha, red, green, blue);
        }

        /// <summary>
        /// rgb= original rgb
        /// alpha= (byte)((color.alpha * (cover) + 255) >> 8);
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public Color NewFromChangeCoverage(int cover)
        {
            return new Color(
                (byte)((_a * cover + 255) >> 8),
                _r, _g, _b);
        }
        /// <summary>
        /// new color from changing the alpha value
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Color NewFromChangeAlpha(byte alpha)
        {
            return new Color(
                 alpha,
                _r, _g, _b);
        }
        //public void AddColor(ColorRGBA c, int cover)
        //{
        //    int cr, cg, cb, ca;
        //    if (cover == COVER_MASK)
        //    {
        //        if (c.Alpha0To255 == BASE_MASK)
        //        {
        //            this = c;
        //        }
        //        else
        //        {
        //            cr = Red0To255 + c.Red0To255; Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //            cg = Green0To255 + c.Green0To255; Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //            cb = Blue0To255 + c.Blue0To255; Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //            ca = Alpha0To255 + c.Alpha0To255; Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //        }
        //    }
        //    else
        //    {
        //        cr = Red0To255 + ((c.Red0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cg = Green0To255 + ((c.Green0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cb = Blue0To255 + ((c.Blue0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        ca = Alpha0To255 + ((c.Alpha0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //        Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //        Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //        Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //    }
        //}

        //public void ApplyGammaDir(GammaLookUpTable gamma)
        //{
        //    Red0To255 = gamma.dir((byte)Red0To255);
        //    Green0To255 = gamma.dir((byte)Green0To255);
        //    Blue0To255 = gamma.dir((byte)Blue0To255);
        //}

        //-------------------------------------------------------------rgb8_packed
        static public Color CreatRGB8Packed(int v)
        {
            //argb
            return new Color(255, (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), ((byte)(v & 0xFF)));
        }


#if DEBUG
        public override string ToString()
        {
            return "r:" + this._r + ",g:" + this._g + ",b:" + this._b + ",a:" + this._a;
        }
#endif
    }
}
