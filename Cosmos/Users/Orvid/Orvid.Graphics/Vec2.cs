// I dislike empty xml comments, so I disable that warning.
#pragma warning disable 1591

using System;

namespace Orvid.Graphics
{
    /// <summary>
    /// This is a 2d vector. aka. A point on a 2d plane.
    /// </summary>
    public class Vec2
    {
        public static Vec2 Zero = new Vec2(0, 0);

        /// <summary>
        /// The X position.
        /// </summary>
        public int X;
        /// <summary>
        /// The Y position.
        /// </summary>
        public int Y;

        /// <summary>
        /// The default constructor.
        /// </summary>
        public Vec2() { }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="width">The X position.</param>
        /// <param name="height">The Y position.</param>
        public Vec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public static Vec2 operator -(Vec2 v)
        {
            Vec2 vec = new Vec2();
            vec.X = -v.X;
            vec.Y = -v.Y;
            return vec;
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            Vec2 v = new Vec2();
            v.X = a.X - b.X;
            v.Y = a.Y - b.Y;
            return v;
        }

        public static Vec2 operator -(Vec2 a, int b)
        {
            Vec2 v = new Vec2();
            v.X = a.X - b;
            v.Y = a.Y - b;
            return v;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            Vec2 v = new Vec2();
            v.X = a.X + b.X;
            v.Y = a.Y + b.Y;
            return v;
        }

        public static Vec2 operator /(Vec2 a, Vec2 b)
        {
            Vec2 v = new Vec2();
            v.X = a.X / b.X;
            v.Y = a.Y / b.Y;
            return v;
        }

        public static Vec2 operator /(Vec2 a, int b)
        {
            Vec2 v = new Vec2();
            v.X = (Int32)(a.X / b);
            v.Y = (Int32)(a.Y / b);
            return v;
        }

        public static Vec2 operator /(Vec2 v, float s)
        {
            Vec2 vec = new Vec2();
            // Division of floats can be expensive,
            // so we multiply instead, as it's much cheaper.
            float num = 1f / s;
            vec.X = (Int32)(v.X * num);
            vec.Y = (Int32)(v.Y * num);
            return vec;
        }

        public static Vec2 operator /(float s, Vec2 v)
        {
            Vec2 vec = new Vec2();
            vec.X = (Int32)(s / v.X);
            vec.Y = (Int32)(s / v.Y);
            return vec;
        }

        public static bool operator !=(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return true;
            return false;
        }

        public static bool operator ==(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return false;
            return true;
        }

        public Vec2 Double()
        {
            return new Vec2(this.X * 2, this.Y * 2);
        }

        public override bool Equals(object obj)
        {
            return (this == (Vec2)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
// And restore the warning.
#pragma warning restore 1591