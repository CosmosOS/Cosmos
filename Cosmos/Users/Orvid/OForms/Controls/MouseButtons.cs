using System;

namespace OForms
{
    /// <summary>
    /// Represents the pressed mouse buttons.
    /// </summary>
    [Flags]
    public enum MouseButtons
    {
        None = 0,
        Left = 1,
        Right = 2,
        Middle = 4,
        XButton1 = 8,
        XButton2 = 16,
    }
}
