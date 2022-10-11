using System.Drawing;

namespace Cosmos.System.Graphics.Extensions
{
    public static class ColorEx
    {
        /// <summary>
        /// Mix the current color with another based on transparency.
        /// 255 alpha = 100% of new color.
        /// 0 alpha = 0% of new color.
        /// </summary>
        /// <param name="aColor">New color to add on top of the existing color.</param>
        /// <returns>Mixed color.</returns>
        public static Color AlphaBlend(Color aOldColor, Color aColor)
        {
            if (aColor.A == 255)
            {
                return aColor;
            }
            if (aColor.A == 0)
            {
                return aOldColor;
            }

            return Color.FromArgb(
                (byte)((aOldColor.A * (255 - aColor.A) / 255) + aColor.A),
                (byte)((aOldColor.R * (255 - aColor.A) / 255) + aColor.R),
                (byte)((aOldColor.G * (255 - aColor.A) / 255) + aColor.G),
                (byte)((aOldColor.B * (255 - aColor.A) / 255) + aColor.B));
        }
    }
}
