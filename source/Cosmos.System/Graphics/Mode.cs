using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    /*
     * This class represents a video mode in term of its number of columns, rows and color_depth
     */
    //public struct Mode : IEquatable<Mode>, IComparable<Mode>
    //[StructLayout(LayoutKind.Explicit, Size = 12)]
    public class Mode
    {
        //[FieldOffset(0)]
        int columns;
        //[FieldOffset(4)]
        int rows;
        //[FieldOffset(8)]
        ColorDepth color_depth;

        public bool Equals(Mode other)
        {
            if ((object)other == null)
                return false;

            if (this.columns == other.columns && this.rows == other.rows && this.color_depth == other.color_depth)
                return true;

            return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Mode))
            {
                return false;
            }

            return Equals((Mode)obj);
        }

        /* If you ovveride Equals you should ovveride GetHashCode too! */
        public override int GetHashCode()
        {
            // overflow is acceptable in this case
            unchecked
            {
                int hash = columns.GetHashCode();
                hash = (hash * 17) + rows.GetHashCode();
                hash = (hash * 31) + (int)color_depth.GetHashCode();
                return hash;
            }
        }

        public int CompareTo(Mode other)
        {
            // color_depth has no effect on the orderiring
            if (this.columns < other.columns && this.rows < other.rows)
                return -1;

            if (this.columns > other.columns && this.rows > other.rows)
                return 1;

            // They are effectively Equals
            return 0;
        }

        public static bool operator ==(Mode mode_a, Mode mode_b)
        {
            if ((object)mode_a == (object)mode_b)
                return true;

            return mode_a.Equals(mode_b);
        }

        public static bool operator !=(Mode mode_a, Mode mode_b)
        {
            return !(mode_a == mode_b);
        }

        public static bool operator >(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            return (result > 0) ? true : false;
        }

        public static bool operator <(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            return (result < 0) ? true : false;
        }

        public static bool operator >=(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            if (result == 0 || result > 0)
                return true;
            return false;
        }

        public static bool operator <=(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            if (result == 0 || result < 0)
                return true;

            return false;
        }

        /* Constuctor of our struct */
        public Mode(int columns, int rows, ColorDepth color_depth)
        {
            this.columns = columns;
            this.rows = rows;
            this.color_depth = color_depth;
        }

        public int Columns
        {
            get
            {
                return columns;
            }
        }

        public int Rows
        {
            get
            {
                return rows;
            }
        }

        public ColorDepth ColorDepth
        {
            get
            {
                return color_depth;
            }
        }

        public override String ToString()
        {
            return $"{columns}x{rows}@{(int)color_depth}";
        }
    }
}
