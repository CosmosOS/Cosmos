using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport
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
        Normal = 0, // Main
        Bold = 1, // Main
        Italic = 2, // Main
        BoldItalic = 3,
        Underline = 4, // Main
        BoldUnderline = 5,
        ItalicUnderline = 6,
        BoldItalicUnderline = 7,
		Strikeout = 8, // Main
		StrikeoutBold = 9,
		StrikoutItalic = 10,
		StrikeoutBoldItalic = 11,
		StrikoutUnderline = 12,
		StrikoutBoldUnderline = 13,
		StrikoutItalicUnderline = 14,
		StrikoutBoldItalicUnderline = 15,
    }
}
