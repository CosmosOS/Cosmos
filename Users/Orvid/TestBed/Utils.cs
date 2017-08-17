using System;
using Forms = System.Windows.Forms;

namespace TestBed
{
    public static class Utils
    {

        /// <summary>
        /// Converts a System.Windows.Forms.MouseButtons to OForms.MouseButtons
        /// </summary>
        /// <param name="b">Object to convert.</param>
        /// <returns>Converted buttons.</returns>
        public static OForms.MouseButtons GetButtons(Forms.MouseButtons b)
        {
            OForms.MouseButtons buttons = OForms.MouseButtons.None;

            if (b.HasFlag(Forms.MouseButtons.Left))
            {
                buttons |= OForms.MouseButtons.Left;
            }
            else if (b.HasFlag(Forms.MouseButtons.Middle))
            {
                buttons |= OForms.MouseButtons.Middle;
            }
            else if (b.HasFlag(Forms.MouseButtons.Right))
            {
                buttons |= OForms.MouseButtons.Right;
            }
            else if (b.HasFlag(Forms.MouseButtons.XButton1))
            {
                buttons |= OForms.MouseButtons.XButton1;
            }
            else if (b.HasFlag(Forms.MouseButtons.XButton2))
            {
                buttons |= OForms.MouseButtons.XButton2;
            }

            return buttons;
        }
    }
}
