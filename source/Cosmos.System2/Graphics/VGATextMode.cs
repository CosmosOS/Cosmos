using System;
using Cosmos.HAL;
using static Cosmos.HAL.VGADriver;

namespace Cosmos.System.Graphics
{
    public class VGAScreen
    {

        private static readonly VGADriver _Screen = new VGADriver();

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

        public static void SetTextMode(TextSize aSize)
        {
            _Screen.SetTextMode(aSize);
        }

        public static int PixelHeight = _Screen.PixelHeight;

        public static int PixelWidth = _Screen.PixelWidth;

        public static int Colors = _Screen.Colors;

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
