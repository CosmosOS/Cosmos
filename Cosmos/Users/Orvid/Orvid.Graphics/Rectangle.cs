using System;

namespace Orvid.Graphics
{
	public class Rectangle
        {
            private int x, y, width, height;
            
			public int Height
			{
				get { return height; }
				set { height = value; }
			}
            
			public int Width 
			{
				get { return width; }
				set { width = value; }
			}
            
			public int X 
			{
				get { return x; }
				set { x = value; }
			}
            
			public int Y 
			{
				get { return y; }
				set { y = value; }
			}

            public Rectangle() { }
            public Rectangle(Rectangle rect)
            {
                SetBounds(rect.X, rect.Y, rect.Width, rect.Height);
            }

            public void SetBounds(int x, int y, int w, int h)
            {
                this.x = x;
                this.y = y;
                this.width = w;
                this.height = h;
            }
        }
}
