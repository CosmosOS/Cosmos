using System;

namespace Orvid.Graphics
{
    /// <summary>
    /// This is a 2d vector. aka. A point on a 2d plane.
    /// </summary>
    public struct Vec2
    {
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
        /// <param name="x">The X position.</param>
        /// <param name="y">The Y position.</param>
        public Vec2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vec2 operator -(Vec2 v)
        {
            Vec2 vec = new Vec2();
            vec.X = -v.X;
            vec.Y = -v.Y;
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            Vec2 v;
            v.X = a.X - b.X;
            v.Y = a.Y - b.Y;
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            Vec2 v;
            v.X = a.X + b.X;
            v.Y = a.Y + b.Y;
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vec2 operator /(Vec2 a, Vec2 b)
        {
            Vec2 v;
            v.X = a.X / b.X;
            v.Y = a.Y / b.Y;
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Vec2 operator /(Vec2 a, int b)
        {
            Vec2 v;
            v.X = (Int32)(a.X / b);
            v.Y = (Int32)(a.Y / b);
            return v;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Vec2 operator /(Vec2 v, float s)
        {
            Vec2 vec;
            float num = 1f / s;
            vec.X = (Int32)(v.X * num);
            vec.Y = (Int32)(v.Y * num);
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vec2 operator /(float s, Vec2 v)
        {
            Vec2 vec;
            vec.X = (Int32)(s / v.X);
            vec.Y = (Int32)(s / v.Y);
            return vec;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Vec2 a, Vec2 b)
        {
            if (a.X != b.X || a.Y != b.Y)
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return (this == (Vec2)obj);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
