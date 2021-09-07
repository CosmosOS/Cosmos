//#define COSMOSDEBUG

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Point class.
    /// </summary>
    public struct Point
    {
        /// <summary>
        /// Create new instance of <see cref="Point"/> class.
        /// </summary>
        /// <param name="x">x coordinate.</param>
        /// <param name="y">y coordinate.</param>
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Get and set x coordinate.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Get and set y coordinate.
        /// </summary>
        public int Y { get; set; }
    }
}
