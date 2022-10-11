namespace Cosmos.System.Graphics
{
    /*
     * This struct represents a video mode in term of its number of columns, rows and color depth
     */

    /// <summary>
    /// Mode struct. Represents a video mode in term of its number of columns, rows and color depth.
    /// </summary>
    public struct Mode
    {
        public ColorDepth ColorDepth;
        public uint Size
        {
            get
            {
                return Width * Height;
            }
        }
        public uint Height;
        public uint Width;

        /* Constuctor of our struct */
        /// <summary>
        /// Create new instance of the <see cref="Mode"/> struct.
        /// </summary>
        /// <param name="columns">Number of columns.</param>
        /// <param name="rows">Number of rows.</param>
        /// <param name="color_depth">Color depth.</param>
        public Mode(uint aWidth, uint aHeight, ColorDepth aColorDepth)
        {
            ColorDepth = aColorDepth;
            Width = aWidth;
            Height = aHeight;
        }

        /// <summary>
        /// Throws an exception if the mode is not valid.
        /// </summary>
        public void ThrowIfNotValid()
        {
            if (Width < 0 || Height < 0 || (byte)ColorDepth > 4)
            {
                throw new("Mode not valid!");
            }
        }

        /// <summary>
        /// Check if modes equal.
        /// </summary>
        /// <param name="other">Other mode.</param>
        /// <returns>bool value.</returns>
        public bool Equals(Mode aOther)
        {
            return Width == aOther.Width && Height == aOther.Height && ColorDepth == aOther.ColorDepth;
        }

        /// <summary>
        /// Check if modes equal.
        /// </summary>
        /// <param name="obj">Object to compare to.</param>
        /// <returns>bool value.</returns>
        public override bool Equals(object aObject) => Equals((Mode)aObject);

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
                int hash = Width.GetHashCode();
                hash = (hash * 17) + Height.GetHashCode();
                hash = (hash * 31) + ColorDepth.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="other">Other mode to compare to.</param>
        /// <returns>-1 if this smaller, +1 if this bigger, 0 otherwise.</returns>
        public int CompareTo(Mode aOther)
        {
            // color_depth has no effect on the orderiring
            if (Width < aOther.Height && Width < aOther.Height)
            {
                return -1;
            }

            if (Width > aOther.Width && Height > aOther.Height)
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
        public static bool operator ==(Mode aMode_a, Mode aMode_b) => aMode_a.Equals(aMode_b);

        /// <summary>
        /// Check if modes are not equal.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator !=(Mode aMode_a, Mode aMode_b) => !(aMode_a == aMode_b);

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator >(Mode aMode_a, Mode aMode_b)
        {
            int result;

            result = aMode_a.CompareTo(aMode_b);

            return (result > 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator <(Mode aMode_a, Mode aMode_b)
        {
            int result;

            result = aMode_a.CompareTo(aMode_b);

            return (result < 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator >=(Mode aMode_a, Mode aMode_b)
        {
            int result;

            result = aMode_a.CompareTo(aMode_b);

            return (result == 0 || result > 0) ? true : false;
        }

        /// <summary>
        /// Compare modes.
        /// </summary>
        /// <param name="mode_a">lhs mode.</param>
        /// <param name="mode_b">rhs mode.</param>
        /// <returns>bool value.</returns>
        public static bool operator <=(Mode aMode_a, Mode aMode_b)
        {
            int result;

            result = aMode_a.CompareTo(aMode_b);

            if (result == 0 || result < 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return $"{Width}x{Height}@{(int)ColorDepth}";
        }
    }
}
