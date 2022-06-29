using System;
using System.Runtime.InteropServices;

namespace Cosmos.System.Graphics
{
    /*
     * This struct represents a video mode in term of its number of columns, rows and color_depth
     */

    /// <summary>
    /// Mode struct. Represents a video mode in term of its number of columns, rows and color depth.
    /// </summary>
    public struct Mode
    {
        int columns;
        int rows;
        ColorDepth color_depth;

        /* Constuctor of our struct */
        /// <summary>
        /// Create new instance of the <see cref="Mode"/> struct.
        /// </summary>
        /// <param name="columns">Number of columns.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="color_depth">Color depth.</param>
        public Mode(int columns, int rows, ColorDepth color_depth)
        {
            this.columns = columns;
            this.rows = rows;
            this.color_depth = color_depth;
        }

        /// <summary>
        /// Check if modes equal.
        /// </summary>
        /// <param name="other">Other mode.</param>
        /// <returns>bool value.</returns>
        public bool Equals(Mode other)
        {
            if (columns == other.columns && rows == other.rows && color_depth == other.color_depth)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if modes equal.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>bool value.</returns>
        public override bool Equals(object obj) => obj is Mode mode && Equals(mode);

        /* If you ovveride Equals you should ovveride GetHashCode too! */
        /// <summary>
        /// Get hash code.
        /// </summary>
        /// <returns>int value.</returns>
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

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="other">Other mode to compare to.</param>
        /// <returns>-1 if this smaller, +1 if this bigger, 0 otherwise.</returns>
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

        /// <summary>
        /// Check if modes are equal.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator ==(Mode mode_a, Mode mode_b) => mode_a.Equals(mode_b);

        /// <summary>
        /// Check if modes are not equal.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator !=(Mode mode_a, Mode mode_b) => !(mode_a == mode_b);

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator >(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            return (result > 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator <(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            return (result < 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator >=(Mode mode_a, Mode mode_b)
        {
            int result;

            result = mode_a.CompareTo(mode_b);

            return (result == 0 || result > 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
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

        /// <summary>
        /// Get columns.
        /// </summary>
        public int Columns
        {
            get
            {
                return columns;
            }
        }

        /// <summary>
        /// Get rows.
        /// </summary>
        public int Rows
        {
            get
            {
                return rows;
            }
        }

        /// <summary>
        /// Get color depth
        /// </summary>
        public ColorDepth ColorDepth
        {
            get
            {
                return color_depth;
            }
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return $"{columns}x{rows}@{(int)color_depth}";
        }
    }
}
