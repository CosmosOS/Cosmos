using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    /*
     * This struct represents a video mode in term of its number of columns, rows and color_depth
     */
    public struct Mode
    {
        int columns;
        int rows;
        ColorDepth color_depth;

        /* Constuctor of our struct */
        public Mode(int columns, int rows, ColorDepth color_depth)
        {
            this.columns = columns;
            this.rows = rows;
            this.color_depth = color_depth;
        }

        public bool Equals(Mode other)
        {
            if (columns == other.columns && rows == other.rows && color_depth == other.color_depth)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj) => obj is Mode mode && Equals(mode);

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
            if (columns < other.columns && rows < other.rows)
            {
                return -1;
            }

            if (columns > other.columns && rows > other.rows)
            {
                return 1;
            }

            // They are effectively Equals
            return 0;
        }

        public static bool operator ==(Mode mode_a, Mode mode_b) => mode_a.Equals(mode_b);

        public static bool operator !=(Mode mode_a, Mode mode_b) => !(mode_a == mode_b);

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

            return (result == 0 || result > 0) ? true : false;
        }

        public static bool operator <=(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            if (result == 0 || result < 0)
            {
                return true;
            }

            return false;
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

        public override string ToString()
        {
            return $"{columns}x{rows}@{(int)color_depth}";
        }
    }
}
