using System;
using HALVGAScreen = Cosmos.HAL.VGAScreen;

namespace Cosmos.System
{
    public class VGAScreen
    {
        public enum TextSize { Size40x25, Size40x50, Size80x25, Size80x50, Size90x30, Size90x60 };

        public enum ScreenSize
        {
            Size640x480,
            Size720x480,
            Size320x200
        };

        public enum ColorDepth
        {
            BitDepth2, BitDepth4, BitDepth8, BitDepth16
        };

        private static HALVGAScreen mScreen = new HALVGAScreen();

        public static void SetGraphicsMode(ScreenSize screenSize, ColorDepth colorDepth)
        {
            HALVGAScreen.ScreenSize ScrSize = HALVGAScreen.ScreenSize.Size320x200;
            HALVGAScreen.ColorDepth ClrDepth = HALVGAScreen.ColorDepth.BitDepth8;

            switch (screenSize)
            {
                case ScreenSize.Size320x200:
                    ScrSize = HALVGAScreen.ScreenSize.Size320x200;
                    break;
                case ScreenSize.Size640x480:
                    ScrSize = HALVGAScreen.ScreenSize.Size640x480;
                    break;
                case ScreenSize.Size720x480:
                    ScrSize = HALVGAScreen.ScreenSize.Size720x480;
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }

            switch (colorDepth)
            {
                case ColorDepth.BitDepth2:
                    ClrDepth = HALVGAScreen.ColorDepth.BitDepth2;
                    break;
                case ColorDepth.BitDepth4:
                    ClrDepth = HALVGAScreen.ColorDepth.BitDepth4;
                    break;
                case ColorDepth.BitDepth8:
                    ClrDepth = HALVGAScreen.ColorDepth.BitDepth8;
                    break;
                case ColorDepth.BitDepth16:
                    ClrDepth = HALVGAScreen.ColorDepth.BitDepth16;
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }

            mScreen.SetGraphicsMode(ScrSize, ClrDepth);
        }

        public static void SetPixel(uint X, uint Y, uint Color)
        {
            mScreen.SetPixel(X, Y, Color);
        }

        public static void Clear(int Color)
        {
            mScreen.Clear(Color);
        }

        public static void TestMode320x200x8()
        {
            mScreen.TestMode320x200x8();
        }

        public static void SetPalette(int Index, byte[] Palette)
        {
            mScreen.SetPalette(Index, Palette);
        }

        public static void SetPaletteEntry(int Index, byte R, byte G, byte B)
        {
            mScreen.SetPaletteEntry(Index, R, G, B);
        }

        public static uint GetPixel(uint X, uint Y)
        {
            return mScreen.GetPixel(X, Y);
        }

        public static void SetTextMode(TextSize Size)
        {
            switch (Size)
            {
                case TextSize.Size40x25:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size40x25);
                    break;
                case TextSize.Size40x50:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size40x50);
                    break;
                case TextSize.Size80x25:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size80x25);
                    break;
                case TextSize.Size80x50:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size80x50);
                    break;
                case TextSize.Size90x30:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size90x30);
                    break;
                case TextSize.Size90x60:
                    mScreen.SetTextMode(HALVGAScreen.TextSize.Size90x60);
                    break;
                default:
                    throw new Exception("This situation is not implemented!");
            }
        }

        public static int PixelHeight = mScreen.PixelHeight;

        public static int PixelWidth = mScreen.PixelWidth;

        public static int Colors = mScreen.Colors;
    }
}
