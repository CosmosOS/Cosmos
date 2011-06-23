using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orvid.Graphics.Shapes
{
    public abstract class Shape
    {
        public Int32 X { get; set; }
        public Int32 Y { get; set; }
        public bool Modified { get; internal set; }
        public abstract void Draw();
    }
}
