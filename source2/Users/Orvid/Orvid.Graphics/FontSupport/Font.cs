using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// The base type for a font.
    /// </summary>
    public abstract class Font
    {
        private static Dictionary<string, Font> LoadedFonts = new Dictionary<string, Font>();

        public static Font LoadFont(string s)
        {
            if (LoadedFonts.ContainsKey(s))
            {
                return LoadedFonts[s];
            }
            return null;
        }

        /// <summary>
        /// The name of the font.
        /// </summary>
        public string Name;
        /// <summary>
        /// The style of the font.
        /// </summary>
        public FontStyle Style;
        /// <summary>
        /// The point-size of the Font rounded to an int.
        /// </summary>
        public int Size;

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="style">The style of the font.</param>
        /// <param name="size">The point-size of the font.</param>
        public Font(string name, FontStyle style, int size)
        {
            this.Name = name;
            this.Style = style;
            this.Size = size;
            LoadedFonts.Add(name, this);
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <param name="style">The style of the font.</param>
        /// <param name="size">The point-size of the font.</param>
        public Font(string name, FontStyle style, double size)
        {
            this.Name = name;
            this.Style = style;
            this.Size = (int)size;
        }
    }
}
