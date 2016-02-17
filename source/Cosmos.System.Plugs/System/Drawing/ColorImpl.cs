using System.Drawing;
using Cosmos.IL2CPU.Plugs;

namespace Cosmos.System.Plugs.System.Drawing
{
    [Plug(Target = typeof (Color))]
    public static class ColorImpl
    {
        public static string ToString(ref Color aThis)
        {

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

        public static long get_Value(ref Color aThis)
        {
            return 0;
        }

    }
}