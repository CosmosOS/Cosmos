using System;

namespace Orvid.Graphics
{
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
