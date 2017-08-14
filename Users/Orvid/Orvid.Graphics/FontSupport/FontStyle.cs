using System;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// There can be up to 8 different flags.
    /// </summary>
    [Flags]
    public enum FontStyle
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Strikeout = 8,
    }
}
