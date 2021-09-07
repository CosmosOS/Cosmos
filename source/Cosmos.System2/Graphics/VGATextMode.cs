using System;
using Cosmos.HAL;
using static Cosmos.HAL.VGADriver;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VGAScreen class. Used to control VGA textmode.
    /// </summary>
    public class VGAScreen
    {
        private static readonly VGADriver _Screen = new VGADriver();

        /// <summary>
        /// Set graphics mode.
        /// </summary>
        /// <param name="screenSize">Screen size.</param>
        /// <param name="colorDepth">Color depth.</param>
        /// <exception cref="Exception">Thrown if screen size / color depth not supported.</exception>
        public static void SetGraphicsMode(ScreenSize aScreenSize, ColorDepth aColorDepth)
        {
            var vgaColorDepth = aColorDepth switch
            {
                ColorDepth.ColorDepth4 => VGADriver.ColorDepth.BitDepth4,
                ColorDepth.ColorDepth8 => VGADriver.ColorDepth.BitDepth8,
                ColorDepth.ColorDepth16 => VGADriver.ColorDepth.BitDepth16,
                ColorDepth.ColorDepth24 => throw new NotImplementedException(),
                ColorDepth.ColorDepth32 => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };
            _Screen.SetGraphicsMode(aScreenSize, vgaColorDepth);
        }

        /// <summary>
        /// Set text mode.
        /// </summary>
        /// <param name="Size">Text size.</param>
        public static void SetTextMode(TextSize aSize)
        {
            _Screen.SetTextMode(aSize);
        }

        /// <summary>
        /// Get Height
        /// </summary>
        public static int PixelHeight = _Screen.PixelHeight;

        /// <summary>
        /// Get Width
        /// </summary>
        public static int PixelWidth = _Screen.PixelWidth;

        /// <summary>
        /// Get Colors
        /// </summary>
        public static int Colors = _Screen.Colors;

        /// <summary>
        /// Set a textmode font.
        /// </summary>
        /// <param name="fontData">Font file.</param>
        /// /// <param name="fontHeight">Font Height.</param>
        /// <exception cref="Exception">Thrown when font height > 32.</exception>
        public static void SetFont(byte[] fontData, int fontHeight)
        {
            if(fontHeight > 32)
            {
                throw new ArgumentOutOfRangeException("fontHeight");
            }
            _Screen.WriteFont(fontData, (byte)fontHeight);
        }
    }
}
