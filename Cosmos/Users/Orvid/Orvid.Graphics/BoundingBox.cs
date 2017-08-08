
namespace Orvid.Graphics
{
    /// <summary>
    /// The class that defines a bounding box.
    /// </summary>
    public class BoundingBox
    {
        /// <summary>
        /// The right side of the BoundingBox.
        /// </summary>
        public int Right;
        /// <summary>
        /// The left side of the BoundingBox.
        /// </summary>
        public int Left;
        /// <summary>
        /// The top of the BoundingBox.
        /// </summary>
        public int Top;
        /// <summary>
        /// The bottom of the BoundingBox.
        /// </summary>
        public int Bottom;

        /// <summary>
        /// Gets the Width of the BoundingBox.
        /// </summary>
        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        /// <summary>
        /// Gets the Height of the BoundingBox.
        /// </summary>
        public int Height
        {
            get
            {
                return Top - Bottom;
            }
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="ileft">The farthest left side of the bounding box.</param>
        /// <param name="iright">The farthest right side of the bounding box.</param>
        /// <param name="itop">The farthest up side of the bounding box.</param>
        /// <param name="ibottom">The farthest down side of the bounding box.</param>
        public BoundingBox(int ileft, int iright, int itop, int ibottom)
        {
            this.Left = ileft;
            this.Right = iright;
            this.Top = itop;
            this.Bottom = ibottom;
        }

        /// <summary>
        /// Returns true if the given point is inside the bounding box.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns>True if the specified point is inside the bounding box.</returns>
        public bool Contains(Vec2 p)
        {
            return IsInBounds(p);
        }

        /// <summary>
        /// Returns true if the given point is inside the bounding box.
        /// </summary>
        /// <param name="x">X location.</param>
        /// <param name="y">Y location.</param>
        /// <returns>True if the given point is inside the bounding box.</returns>
        public bool Contains(int x, int y)
        {
            return IsInBounds(new Vec2(x, y));
        }

        /// <summary>
        /// Returns true if the given point is inside the bounding box.
        /// </summary>
        /// <param name="x">X location.</param>
        /// <param name="y">Y location.</param>
        /// <returns>True if the given point is inside the bounding box.</returns>
        public bool IsInBounds(int x, int y)
        {
            return IsInBounds(new Vec2(x, y));
        }

        /// <summary>
        /// Returns true if the given point is inside the bounding box.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns>True if the specified point is inside the bounding box.</returns>
        public bool IsInBounds(Vec2 p)
        {
            //throw new Exception();
            return ((p.X < Right && p.X > Left) && (p.Y < Top && p.Y > Bottom));
        }

        /// <summary>
        /// Lowers all values in the BoundingBox by the specified Vec2.
        /// </summary>
        public static BoundingBox operator -(BoundingBox b, Vec2 v)
        {
            return new BoundingBox(b.Left - v.X, b.Right - v.X, b.Top - v.Y, b.Bottom - v.Y);
        }
    }
}
