using System;
using System.IO;

namespace Orvid.Graphics.FontSupport
{
    /// <summary>
    /// This class represents a font manager.
    /// It's a font manager's job to load, 
    /// draw, and keep track of fonts.
    /// </summary>
    public abstract class FontManager
    {
        /// <summary>
        /// Current font manager.
        /// </summary>
        private static FontManager instance;
        /// <summary>
        /// The current font manager.
        /// </summary>
        public static FontManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// The static constructor which sets-up the default font manager.
        /// </summary>
        static FontManager()
        {
            instance = new DefaultFontManager();
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public FontManager()
        {
            instance = this;
        }

        /// <summary>
        /// The name of the FontManager.
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// The known fonts.
        /// </summary>
        public abstract Font[] Fonts { get; }
        /// <summary>
        /// Get the FontMetrics for the specified font.
        /// </summary>
        /// <param name="font">Font to get the metrics for.</param>
        /// <returns>The FontMetrics for the specified font.</returns>
        public abstract FontMetrics GetFontMetrics(Font font);
        /// <summary>
        /// Draw the specified Text, using the specified Font, 
        /// in the specified Color, within the specified Bounds,
        /// at the specified Location, and on the specified image,
        /// making sure to take the transform into account.
        /// </summary>
        /// <param name="i">The Image to draw on.</param>
        /// <param name="clip">The BoundingBox to clip to.</param>
        /// <param name="trans">The Transform to apply.</param>
        /// <param name="s">The String to draw.</param>
        /// <param name="f">The Font to draw in.</param>
        /// <param name="Loc">The Location to draw at.</param>
        /// <param name="p">The Color to draw in.</param>
        public abstract void DrawText(Image i, BoundingBox clip, AffineTransform trans, String s, Font f, Vec2 Loc, Pixel p);
        /// <summary>
        /// Loads a font from the specified stream.
        /// </summary>
        /// <param name="format">The format of the font.</param>
        /// <param name="s">The Stream to load from.</param>
        /// <returns>The loaded font.</returns>
        public abstract Font LoadFont(int format, Stream s);
#warning TODO: Remove the need for the format parameter.
    }
}
