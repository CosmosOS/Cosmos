using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.System.Graphics
{
    public struct Rectangle
    {
        // properties
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // constructor
        public Rectangle(int x = 0, int y = 0, int w = 0, int h = 0)
        {
            this.X = x; this.Y = y;
            this.Width = w; this.Height = h;
        }

        // set
        public void Set(int x, int y, int w, int h) { this.X = x; this.Y = y; this.Width = w; this.Height = h; }

        // contains
        public bool Contains(Point pos) { return Contains(pos.X, pos.Y); }
        public bool Contains(int x, int y) { return x >= X && x < X + Width && y >= Y && y < Y + Height; }

        // intersects
        public bool Intersects(Rectangle bounds) { return Contains(bounds.X, bounds.Y); }
    }
}
