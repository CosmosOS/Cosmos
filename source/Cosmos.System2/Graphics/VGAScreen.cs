using static Cosmos.HAL.Drivers.Video.VGADriver;
using Cosmos.HAL.Drivers.Video;
using System;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Controls VGA text-mode.
    /// </summary>
    public class VGAScreen
    {
        static readonly VGADriver screen = new();

        /// <summary>
        /// Sets the currently used graphics mode.
        /// </summary>
        /// <param name="screenSize">The size of the screen.</param>
        /// <param name="colorDepth">The color depth of each pixel.</param>
        /// <exception cref="Exception">Thrown if screen size or color depth not supported.</exception>
        public static void SetGraphicsMode(ScreenSize screenSize, ColorDepth colorDepth)
        {
            var vgaColorDepth = colorDepth switch
            {
                ColorDepth.ColorDepth4 => VGADriver.ColorDepth.BitDepth4,
                ColorDepth.ColorDepth8 => VGADriver.ColorDepth.BitDepth8,
                ColorDepth.ColorDepth16 => VGADriver.ColorDepth.BitDepth16,
                ColorDepth.ColorDepth24 => throw new NotImplementedException(),
                ColorDepth.ColorDepth32 => throw new NotImplementedException(),
                _ => throw new NotImplementedException(),
            };

            screen.SetGraphicsMode(screenSize, vgaColorDepth);
        }

        /// <summary>
        /// Sets the currently used text mode.
        /// </summary>
        /// <param name="size">The text mode size.</param>
        public static void SetTextMode(TextSize size)
        {
            screen.SetTextMode(size);
        }

        /// <summary>
        /// The width of each pixel.
        /// </summary>
        public static int PixelWidth = screen.PixelWidth;

        /// <summary>
        /// The height of each pixel.
        /// </summary>
        public static int PixelHeight = screen.PixelHeight;

        /// <summary>
        /// The text-mode colors used by the display.
        /// </summary>
        public static int Colors = screen.Colors;

        /// <summary>
        /// Sets the used text-mode font.
        /// </summary>
        /// <param name="fontData">The data of the font.</param>
        /// <param name="fontHeight">The height of each character in the font.</param>
        /// <exception cref="Exception">Thrown when font height > 32.</exception>
        public static void SetFont(byte[] fontData, int fontHeight)
        {
            if(fontHeight > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(fontHeight));
            }

            screen.WriteFont(fontData, (byte)fontHeight);
        }
    }
}
