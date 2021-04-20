using System;
using Cosmos.HAL;
using static Cosmos.HAL.VGADriver;

namespace Cosmos.System.Graphics
{
    public class VGAScreen
    {
        // driver
        private static readonly VGADriver driver = new VGADriver();

        /// <summary>
        /// Real width of pixel
        /// </summary>
        public static int PixelWidth = driver.PixelWidth;

        /// <summary>
        /// Real height of pixel
        /// </summary>
        public static int PixelHeight = driver.PixelHeight;

        /// <summary>
        /// Color palette
        /// </summary>
        public static int Colors = driver.Colors;

        /// <summary>
        /// Set vga graphics mode
        /// </summary>
        /// <param name="size"></param>
        /// <param name="depth"></param>
        public static void SetGraphicsMode(ScreenSize size, ColorDepth depth)
        {
            if ((uint)depth > 8) { throw new Exception(((uint)depth).ToString() + "-bit color is not compatible with VGA"); }
            driver.SetGraphicsMode(size, (VGADriver.ColorDepth)depth);
        }

        /// <summary>
        /// Set vga text mode
        /// </summary>
        /// <param name="aSize"></param>
        public static void SetTextMode(TextSize size)
        {
            driver.SetTextMode(size);
        }

        /// <summary>
        /// Set vga font
        /// </summary>
        /// <param name="fontData"></param>
        /// <param name="fontHeight"></param>
        public static void SetFont(byte[] fontData, int fontHeight)
        {
            if (fontHeight > 32) { throw new ArgumentOutOfRangeException("Font height cannot be higher than 32"); }
            driver.WriteFont(fontData, (byte)fontHeight);
        }
    }
}
