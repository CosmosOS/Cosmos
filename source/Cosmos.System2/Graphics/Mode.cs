using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    /*
     * This struct represents a video mode in term of its number of columns, rows and color_depth
     */

    /// <summary>
    /// Represents a video mode, definining its width (rows), height (columns) and color depth.
    /// </summary>
    public readonly struct Mode
    {
        /// <summary>
        /// The width, or rows, of the display mode.
        /// </summary>
        public uint Width { get; }

        /// <summary>
        /// The height, or columns, of the display mode.
        /// </summary>
        public uint Height { get; }

        /// <summary>
        /// The color depth of the display mode, i.e. the amount of bits per a single pixel.
        /// </summary>
        public ColorDepth ColorDepth { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mode"/> struct.
        /// </summary>
        /// <param name="columns">The number of columns.</param>
        /// <param name="rows">The number of rows.</param>
        /// <param name="colorDepth">The color depth, i.e. the amount of bits per a single pixel.</param>
        public Mode(uint columns, uint rows, ColorDepth colorDepth)
        {
            Width = columns;
            Height = rows;
            ColorDepth = colorDepth;
        }

        public bool Equals(Mode other)
        {
            return Width == other.Width && Height == other.Height && ColorDepth == other.ColorDepth;
        }

        public override bool Equals(object obj) => obj is Mode mode && Equals(mode);

        public override int GetHashCode()
        {
            // overflow is acceptable in this case
            unchecked
            {
                int hash = Width.GetHashCode();
                hash = hash * 17 + Height.GetHashCode();
                hash = hash * 31 + ColorDepth.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(Mode other)
        {
            // color_depth has no effect on the orderiring
            if (Width < other.Width && Height < other.Height)
            {
                return -1;
            }

            if (Width > other.Width && Height > other.Height)
            {
                return 1;
            }

            // They are effectively Equals
            return 0;
        }

        public static bool operator ==(Mode a, Mode b) => a.Equals(b);
        public static bool operator !=(Mode a, Mode b) => !(a == b);
        public static bool operator >(Mode a, Mode b) => a.CompareTo(b) > 0;
        public static bool operator <(Mode a, Mode b) => a.CompareTo(b) < 0;
        public static bool operator >=(Mode a, Mode b) => a.CompareTo(b) >= 0;
        public static bool operator <=(Mode a, Mode b) => a.CompareTo(b) <= 0;

        public override string ToString()
        {
            return $"{Width}x{Height}@{(int)ColorDepth}";
        }
    }
}
