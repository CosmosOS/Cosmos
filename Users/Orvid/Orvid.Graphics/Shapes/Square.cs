using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.Shapes
{
    public class Square : Shape
    {
        private Image i;
        private Pixel fillColor = new Pixel(255, 255, 255, 255); // Initialize white.
        public Pixel FillColor
        {
            get
            {
                return fillColor;
            }
            set
            {
                if (fillColor != value)
                {
                    Modified = true;
                    fillColor = value;
                }
            }
        }
        private Pixel borderColor = new Pixel(0, 0, 0, 255); // Initialize black.
        public Pixel BorderColor
        {
            get
            {
                return borderColor;
            }
            set
            {
                if (borderColor != value)
                {
                    Modified = true;
                    borderColor = value;
                }
            }
        }
        private int size;
        public Int32 Size
        {
            get
            {
                return size;
            }
            set
            {
                if (size != value)
                {
                    Modified = true;
                    size = value;
                    i = new Image(this.Size, this.Size);
                }
            }
        }

        public Square(int x, int y, ShapedImage parent, int size)
        {
            this.X = x;
            this.Y = y;
            this.Parent = parent;
            this.Parent.Shapes.Add(this);
            this.Size = size;
            this.i = new Image(this.Size, this.Size);
        }

        public override void Draw()
        {
            if (Modified)
            {
                i.Clear(this.FillColor);
                i.DrawLines(new Vec2[] { new Vec2(0, 0), new Vec2(Size, 0), new Vec2(Size, Size), new Vec2(0, Size), new Vec2(0, 0) }, BorderColor);
                Modified = false;
            }
            Parent.DrawImage(new Vec2(X, Y), i);
        }
    }
}
