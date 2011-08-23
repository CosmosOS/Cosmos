
namespace Orvid.Graphics
{
    /// <summary>
    /// The class that defines a bounding box.
    /// </summary>
    public class BoundingBox
    {
        public int Right;
        public int Left;
        public int Top;
        public int Bottom;

        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        public int Height
        {
            get
            {
                return Bottom - Top;
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
        /// <returns></returns>
        public bool IsInBounds(Vec2 p)
        {
            //throw new Exception();
            return ((p.X < Right && p.X > Left) && (p.Y < Top && p.Y > Bottom));
        }
    }
}
