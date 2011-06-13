
namespace Orvid.Graphics
{
    /// <summary>
    /// The class that defines a bounding box.
    /// </summary>
    public class BoundingBox
    {
        private int right;
        private int left;
        private int top;
        private int bottom;
        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="ileft">The farthest left side of the bounding box.</param>
        /// <param name="iright">The farthest right side of the bounding box.</param>
        /// <param name="itop">The farthest up side of the bounding box.</param>
        /// <param name="ibottom">The farthest down side of the bounding box.</param>
        public BoundingBox(int ileft, int iright, int itop, int ibottom)
        {
            this.left = ileft;
            this.right = iright;
            this.top = itop;
            this.bottom = ibottom;
        }
        /// <summary>
        /// Returns true if the given point is inside the bounding box.
        /// </summary>
        /// <param name="p">The point to check.</param>
        /// <returns></returns>
        public bool IsInBounds(Vec2 p)
        {
            return ((p.X < right && p.X > left) && (p.Y < top && p.Y > bottom));
        }
    }
}
