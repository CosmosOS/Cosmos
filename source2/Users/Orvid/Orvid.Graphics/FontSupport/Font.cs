using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport
{
    public abstract class Font
    {
        public abstract String Name { get; }
        public abstract Image GetCharacter(UInt64 charNumber, FontFlag flags);
    }

    [Flags]
    public enum FontFlag
    {
        Normal = 1,
        Bold = 2,
        Italic = 3,
    }
}
