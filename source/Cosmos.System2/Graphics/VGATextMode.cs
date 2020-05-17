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

        public static void SetPixel(uint aX, uint aY, uint aColor)
        {
            _Screen.SetPixel(aX, aY, aColor);
        }

        public static void Clear(int aColor)
        {
            _Screen.DrawFilledRectangle(0,0, _Screen.PixelWidth, _Screen.PixelHeight, (uint)aColor);
        }

        public static void SetPaletteEntry(int aIndex, byte aR, byte aG, byte aB)
        {
            _Screen.SetPaletteEntry(aIndex, aR, aG, aB);
        }

        public static uint GetPixel(uint aX, uint aY)
        {
            return _Screen.GetPixel(aX, aY);
        }

        public static void SetTextMode(TextSize aSize)
        {
            switch (aSize)
            {
                case TextSize.Size40x25:
                    _Screen.SetTextMode(TextSize.Size40x25);
                    break;
                case TextSize.Size40x50:
                    _Screen.SetTextMode(TextSize.Size40x50);
                    break;
                case TextSize.Size80x25:
                    _Screen.SetTextMode(TextSize.Size80x25);
                    break;
                case TextSize.Size80x50:
                    _Screen.SetTextMode(TextSize.Size80x50);
                    break;
                case TextSize.Size90x30:
                    _Screen.SetTextMode(TextSize.Size90x30);
                    break;
                case TextSize.Size90x60:
                    _Screen.SetTextMode(TextSize.Size90x60);
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }
        }

        public static int PixelHeight = _Screen.PixelHeight;

        public static int PixelWidth = _Screen.PixelWidth;

        public static int Colors = _Screen.Colors;
    }
}
