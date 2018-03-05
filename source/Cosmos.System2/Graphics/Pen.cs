using System;

namespace Cosmos.System.Graphics
{
    /*
     * This represents a Pen the same thing a drawer uses to paint.
     * For now it is a very little abstraction it put together a color and the width of the Pen in future
     * it could be more having maybe a concept of Style?
     * It is a class because we want to permit to change the color of a Pen doing Pen.Color = <aColor> it could have been
     * a struct but struct should be immutable so...
     */
    public class Pen
    {
        Color color;
        int width;

        public Pen(Color color, int width = 1)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException($"width ({width}) cannot be less than 1");
           // Contract.Requires<ArgumentOutOfRangeException>(width >= 1, "thickness");

            this.color = color;
            this.width = width;
        }

        public Color Color
        {
            get
            {
                return color;
            }

            set
            {
                color = value;
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                width = value;
            }
        }

        public override String ToString()
        {
            return $"color: {color} width: {width}";
        }
    }
}
