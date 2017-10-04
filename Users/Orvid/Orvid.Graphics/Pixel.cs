using System;

namespace Orvid.Graphics
{
    /// <summary>
    /// This class describes a single pixel.
    /// </summary>
    public struct Pixel
    {
        /// <summary>
        /// The byte that describes the amount of Red in the pixel.
        /// </summary>
        public byte R;
        /// <summary>
        /// The byte that describes the amount of Green in the pixel.
        /// </summary>
        public byte G;
        /// <summary>
        /// The byte that describes the amount of Blue in the pixel.
        /// </summary>
        public byte B;
        /// <summary>
        /// The byte that describes the transparency of the pixel.
        /// </summary>
        public byte A;
        /// <summary>
        /// This tells if the pixel is empty, and should be ignored.
        /// </summary>
        public bool Empty
        {
            get
            {
                return (A + R + B + G == 0);
            }
        }

        public uint ToUInt()
        {
            return unchecked((uint)((R << 24) | (G << 16) | (B << 8) | (A)));
        }

        //public Pixel()
        //    : this(true)
        //{
        //}

        public Pixel(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Pixel(bool empty)
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
            this.A = 0;
        }

        public static implicit operator System.Drawing.Color(Pixel a)
        {
            return System.Drawing.Color.FromArgb(a.A, a.R, a.G, a.B);
        }

        public static implicit operator Pixel(System.Drawing.Color a)
        {
            return new Pixel(a.R, a.G, a.B, a.A);
        }

        public static bool operator !=(Pixel a, Pixel b)
        {
            //if (!(a is Pixel) || !(b is Pixel))
            //{
            //    if (!(a is Pixel) && !(b is Pixel))
            //        return false;
            //    return true;
            //}
            //else
            //{
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return true;
            return false;
            //}
        }

        public static bool operator !=(Pixel a, int b)
        {
            return (a.A != b && a.B != b && a.G != b && a.R != b);
        }

        public static bool operator ==(Pixel a, int b)
        {
            return (a.A == b && a.B == b && a.G == b && a.R == b);
        }

        public static bool operator ==(Pixel a, Pixel b)
        {

            //if (!(a is Pixel) || !(b is Pixel))
            //{
            //    if (!(a is Pixel) && !(b is Pixel))
            //        return true;
            //    return false;
            //}
            //else
            //{
            if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                return false;
            return true;
            //}
        }

        public override bool Equals(object obj)
        {
            return (this == (Pixel)obj);
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }
}
