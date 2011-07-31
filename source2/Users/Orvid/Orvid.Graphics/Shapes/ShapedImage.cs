using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.Shapes
{
    public class ShapedImage : Image
    {
        public bool Modified { get; internal set; }
        internal List<Shape> Shapes = new List<Shape>();

        public ShapedImage(int width, int height) : base(width, height)
        {
        }

        public Image Render()
        {
            if (Modified)
            {

            }
            return this;
        }
    }
}
