using System;
using System.Collections.Generic;
using System.IO;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// The base class for adding support for different font formats.
    /// </summary>
    public abstract class IFontProvider<F> where F : Font
    {
        /// <summary>
        /// The name of the FontProvider.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Returns true if the given Font is a format
        /// that this FontProvider supports.
        /// </summary>
        /// <param name="f">Font to check.</param>
        /// <returns>True if supported.</returns>
        public abstract bool SupportsFormat(Font f);
        /// <summary>
        /// All fonts this FontProvider can provide.
        /// </summary>
        public abstract List<Font> Fonts { get; }
        /// <summary>
        /// Gets the TextRenderer that should be used to render this font.
        /// </summary>
        /// <returns>The TextRenderer.</returns>
        public abstract ITextRenderer GetTextRenderer(Font f);
        /// <summary>
        /// Gets the FontMetrics for the specified font.
        /// </summary>
        /// <param name="f">The Font to get the metrics for.</param>
        /// <returns>The FontMetrics.</returns>
        public abstract FontMetrics GetFontMetrics(Font f);
        /// <summary>
        /// Gets the specified font in a format supported by this provider.
        /// </summary>
        /// <param name="fontIn">Font to convert.</param>
        /// <param name="fontOut">
        /// The converted Font, null if unable to convert.
        /// </param>
        /// <returns>True if conversion was successful.</returns>
        public abstract bool GetCompatibleFont(Font fontIn, out Font fontOut);
        /// <summary>
        /// Loads a Font from the specified Stream.
        /// </summary>
        /// <param name="s">The Stream to load from.</param>
        /// <returns>The loaded font.</returns>
        public abstract Font LoadFont(Stream s);
    }
}
