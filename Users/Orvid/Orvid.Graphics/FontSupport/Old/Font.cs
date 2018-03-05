using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport.Old
{
    public abstract class Font
    {
        public abstract String Name { get; }
        public abstract Image GetCharacter(Int32 charNumber, FontFlag flags);
    }

    /// <summary>
    /// There can be up to 8 different flags.
    /// </summary>
    [Flags]
    public enum FontFlag
    {
        Normal = 0,
        Bold = 1,
        Italic = 2,
        Underline = 4,
		Strikeout = 8,
    }
}
