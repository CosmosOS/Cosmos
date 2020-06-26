using System;
using HALVGAScreen = Cosmos.HAL.VGAScreen;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VGAScreen class. Used to control screen.
    /// </summary>
    public class VGAScreen
    {
        /// <summary>
        /// Text size.
        /// </summary>
        public enum TextSize {
            /// <summary>
            /// 40x25
            /// </summary>
            Size40x25,
            /// <summary>
            /// 40x50
            /// </summary>
            Size40x50,
            /// <summary>
            /// 80x25
            /// </summary>
            Size80x25,
            /// <summary>
            /// 80x50
            /// </summary>
            Size80x50,
            /// <summary>
            /// 90x30
            /// </summary>
            Size90x30,
            /// <summary>
            /// 90x60
            /// </summary>
            Size90x60 };

        /// <summary>
        /// Screen size (resolution).
        /// </summary>
        public enum ScreenSize
        {
            /// <summary>
            /// 640x480 graphics mode  - 2 and 4 bit color depth available
            /// </summary>
            Size640x480,

            /// <summary>
            /// 720x480 graphics mode  - 16 bit color depth available
            /// </summary>
            Size720x480,

            /// <summary>
            /// 320x200 graphics mode  - 4 and 8 bit color depth available
            /// </summary>
            Size320x200
        };

        /// <summary>
        /// Color depth. bits per pixel.
        /// </summary>
        public enum ColorDepth
        {
            /// <summary>
            /// 2 bits per pixel.
            /// </summary>
            BitDepth2,
            /// <summary>
            /// 4 bits per pixel.
            /// </summary>
            BitDepth4,
            /// <summary>
            /// 8 bits per pixel.
            /// </summary>
            BitDepth8,
            /// <summary>
            /// 16 bits per pixel.
            /// </summary>
            BitDepth16
        };

        private static HALVGAScreen mScreen = new HALVGAScreen();

        /// <summary>
        /// Set graphics mode.
        /// </summary>
        /// <param name="screenSize">Screen size.</param>
        /// <param name="colorDepth">Color depth.</param>
        /// <exception cref="Exception">Thrown if screen size / color depth not supported.</exception>
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

        /// <summary>
        /// Set pixel color.
        /// </summary>
        /// <param name="X">x coordinat.</param>
        /// <param name="Y">y coordinat.</param>
        /// <param name="Color">Color to set.</param>
        public static void SetPixel(uint X, uint Y, uint Color)
        {
            mScreen.SetPixel(X, Y, Color);
        }

        /// <summary>
        /// Clear screen, and paint in in color.
        /// </summary>
        /// <param name="Color">Color to set the screen to.</param>
        public static void Clear(int Color)
        {
            mScreen.Clear(Color);
        }

        /// <summary>
        /// Test mode 320x200x8
        /// </summary>
        /// <exception cref="Exception">
        /// <list type="bullet">
        /// <item>Thrown when color depth not supported for the size.</item>
        /// <item>Unknown screen size.</item>
        /// <item>Memory error.</item>
        /// </list>
        /// </exception>
        public static void TestMode320x200x8()
        {
            mScreen.TestMode320x200x8();
        }

        /// <summary>
        /// Set palette.
        /// </summary>
        /// <param name="Index">Index.</param>
        /// <param name="Palette">Palette.</param>
        /// <exception cref="OverflowException">The array contains more than Int32.MaxValue elements.</exception>
        public static void SetPalette(int Index, byte[] Palette)
        {
            mScreen.SetPalette(Index, Palette);
        }

        /// <summary>
        /// Set palette entry.
        /// </summary>
        /// <param name="index">Index.</param>
        /// <param name="r">Red.</param>
        /// <param name="g">Green.</param>
        /// <param name="b">Blue.</param>
        public static void SetPaletteEntry(int Index, byte R, byte G, byte B)
        {
            mScreen.SetPaletteEntry(Index, R, G, B);
        }

        /// <summary>
        /// Get pixel.
        /// </summary>
        /// <param name="X">x coordinat.</param>
        /// <param name="Y">y coordinat.</param>
        /// <returns>uint value.</returns>
        public static uint GetPixel(uint X, uint Y)
        {
            return mScreen.GetPixel(X, Y);
        }

        /// <summary>
        /// Set text mode.
        /// </summary>
        /// <param name="Size">Text size.</param>
        /// <exception cref="Exception">Thrown when text size invalid / unable to determine memory segment.</exception>
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

        /// <summary>
        /// Get pixel height.
        /// </summary>
        public static int PixelHeight = mScreen.PixelHeight;

        /// <summary>
        /// Get pixel width.
        /// </summary>
        public static int PixelWidth = mScreen.PixelWidth;

        /// <summary>
        /// Get colors.
        /// </summary>
        public static int Colors = mScreen.Colors;
    }
}
