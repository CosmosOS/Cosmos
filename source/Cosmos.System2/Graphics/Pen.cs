using System;
using System.Drawing;

namespace Cosmos.System.Graphics
{
    /*
     * This represents a Pen the same thing a drawer uses to paint.
     * For now it is a very little abstraction it put together a color and the width of the Pen in future
     * it could be more having maybe a concept of Style?
     * It is a class because we want to permit to change the color of a Pen doing Pen.Color = <aColor> it could have been
     * a struct but struct should be immutable so...
     */
    /// <summary>
    /// Pen class. Represents pen, which used to draw in color and width.
    /// </summary>
    public class Pen
    {
        Color color;
        int width;

        /// <summary>
        /// Create new instance of the <see cref="Pen"/> class.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <param name="width">Width.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if width is smaller than 0.</exception>
        public Pen(Color color, int width = 1)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException($"width ({width}) cannot be less than 1");
           // Contract.Requires<ArgumentOutOfRangeException>(width >= 1, "thickness");

            this.color = color;
            this.width = width;
        }

        /// <summary>
        /// Get and set pen color.
        /// </summary>
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

        /// <summary>
        /// Get and set pen width.
        /// </summary>
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

        /// <summary>
        /// To string.
        /// </summary>
        /// <returns>string value.</returns>
        public override string ToString()
        {
            return $"color: {color} width: {width}";
        }
    }
}
