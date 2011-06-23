using System;

namespace Orvid.Graphics
{
    public class Colors
    {
        public static Pixel Black = new Pixel(0, 0, 0, 0);
        public static Pixel White = new Pixel(255, 255, 255, 0);
    }

    /// <summary>
    /// This class describes a single pixel.
    /// </summary>
    public class Pixel
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
        public bool Empty = false;

        public Pixel() : this(true)
        {
        }

        public Pixel(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Pixel(bool empty)
        {
            if (empty)
                this.Empty = true;
        }

        public static bool operator !=(Pixel a, Pixel b)
        {
            if (!(a is Pixel) || !(b is Pixel))
            {
                if (!(a is Pixel) && !(b is Pixel))
                    return false;
                return true;
            }
            else
            {
                if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                    return true;
                return false;
            }
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

            if (!(a is Pixel) || !(b is Pixel))
            {
                if (!(a is Pixel) && !(b is Pixel))
                    return true;
                return false;
            }
            else
            {
                if (a.A != b.A || a.B != b.B || a.G != b.G || a.R != b.R)
                    return false;
                return true;
            }
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
