using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Cosmos.System.Graphics.Extensions
{
    public static class ColorEx
    {

        public static Pen ToPen(this Color color, int width)
        {
            return new Pen(color, width);
        }

    }
}
