using System;
using System.Text;
using Indy.IL2CPU.Plugs;
using System.Drawing;

namespace Indy.IL2CPU.IL.CustomImplementations.System.Drawing {
    [Plug(TargetName = "System.Drawing.Color, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public static class ColorImpl {       
		public static string ToString(ref Color aThis) {

            return "System.Drawing.Color.ToString()";
            /*
            StringBuilder builder = new StringBuilder(0x20);
            builder.Append("System.Drawing.Color");
            builder.Append(" [");
            if ((c.state & c.StateNameValid) != 0)
            {
                builder.Append(this.Name);
            }
            else if ((this.state & StateKnownColorValid) != 0)
            {
                builder.Append(this.Name);
            }
            else if ((this.state & StateValueMask) != 0)
            {
                builder.Append("A=");
                builder.Append(this.A);
                builder.Append(", R=");
                builder.Append(this.R);
                builder.Append(", G=");
                builder.Append(this.G);
                builder.Append(", B=");
                builder.Append(this.B);
            }
            else
            {
                builder.Append("Empty");
            }
            builder.Append("]");
            return builder.ToString();
             //* */
        }


	}
}