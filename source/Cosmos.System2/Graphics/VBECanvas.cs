//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;

using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VBECanvas class. Used to control screen, by VBE (VESA BIOS Extensions) standard. See also: <seealso cref="Canvas"/>.
    /// </summary>
    public class VBECanvas : Canvas
    {
        /// <summary>
        /// Default video mode. 1024x768x32.
        /// </summary>
        private static readonly Mode _DefaultMode = new Mode(1024, 768, ColorDepth.ColorDepth32);

        /// <summary>
        /// Driver for Setting vbe modes and ploting/getting pixels
        /// </summary>
        private readonly VBEDriver _VBEDriver;

        /// <summary>
        /// Video mode.
        /// </summary>
        private Mode _Mode;

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if default mode (1024x768x32) is not suppoted.</exception>
        public VBECanvas() : this(_DefaultMode)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <param name="mode">VBE mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        public VBECanvas(Mode aMode)
        {
            Global.mDebugger.SendInternal($"Creating new VBEScreen() with mode {aMode.Width}x{aMode.Height}x{(uint)aMode.ColorDepth}");

            if (Core.VBE.IsAvailable())
            {
                Core.VBE.ModeInfo ModeInfo = Core.VBE.getModeInfo();
                aMode = new Mode(ModeInfo.width, ModeInfo.height, (ColorDepth)ModeInfo.bpp);
                Global.mDebugger.SendInternal($"Detected VBE VESA with {aMode.Width}x{aMode.Height}x{(uint)aMode.ColorDepth}");
            }

            ThrowIfModeIsNotValid(aMode);

            _VBEDriver = new VBEDriver((ushort)aMode.Width, (ushort)aMode.Height, (ushort)aMode.ColorDepth);
            Mode = aMode;
        }

        /// <summary>
        /// Disables VBE Graphics mode, parent method returns to VGA text mode (80x25)
        /// </summary>
        public override void Disable()
        {
            _VBEDriver.DisableDisplay();
        }

        /// <summary>
        /// Name of the backend
        /// </summary>
        public override string Name() => "VBECanvas";

        /// <summary>
        /// Get and set video mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown if mode is not suppoted.</exception>
        public override Mode Mode
        {
            get => _Mode;
            set
            {
                _Mode = value;
                SetMode(_Mode);
            }
        }

        #region Display

        ///// <summary>
        ///// All the available screen modes VBE supports, I would like to query the hardware and obtain from it the list but I have
        ///// not yet find how to do it! For now I hardcode the most used VESA modes, VBE seems to support until HDTV resolution
        ///// without problems that is well... excellent :-)
        ///// </summary>
        //public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>

        /// <summary>
        /// Available VBE supported video modes.
        /// <para>
        /// Low res:
        /// <list type="bullet">
        /// <item>320x240x32.</item>
        /// <item>640x480x32.</item>
        /// <item>800x600x32.</item>
        /// <item>1024x768x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HD:
        /// <list type="bullet">
        /// <item>1280x720x32.</item>
        /// <item>1280x1024x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDR:
        /// <list type="bullet">
        /// <item>1366x768x32.</item>
        /// <item>1680x1050x32.</item>
        /// </list>
        /// </para>
        /// <para>
        /// HDTV:
        /// <list type="bullet">
        /// <item>1920x1080x32.</item>
        /// <item>1920x1200x32.</item>
        /// </list>
        /// </para>
        /// </summary>
        public override List<Mode> AvailableModes { get; } = new List<Mode>
        {
            new Mode(320, 240, ColorDepth.ColorDepth32),
            new Mode(640, 480, ColorDepth.ColorDepth32),
            new Mode(800, 600, ColorDepth.ColorDepth32),
            new Mode(1024, 768, ColorDepth.ColorDepth32),
            /* The so called HD-Ready resolution */
            new Mode(1280, 720, ColorDepth.ColorDepth32),
            new Mode(1280, 768, ColorDepth.ColorDepth32),
            new Mode(1280, 1024, ColorDepth.ColorDepth32),
            /* A lot of HD-Ready screen uses this instead of 1280x720 */
            new Mode(1366, 768, ColorDepth.ColorDepth32),
            new Mode(1680, 1050, ColorDepth.ColorDepth32),
            /* HDTV resolution */
            new Mode(1920, 1080, ColorDepth.ColorDepth32),
            /* HDTV resolution (16:10 AR) */
            new Mode(1920, 1200, ColorDepth.ColorDepth32),
        };

        /// <summary>
        /// Override Canvas default graphics mode.
        /// </summary>
        public override Mode DefaultGraphicMode => _DefaultMode;

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        private void SetMode(Mode aMode)
        {
            ThrowIfModeIsNotValid(aMode);

            ushort xres = (ushort)Mode.Width;
            ushort yres = (ushort)Mode.Height;
            ushort bpp = (ushort)Mode.ColorDepth;

            //set the screen
            _VBEDriver.VBESet(xres, yres, bpp);
        }
        #endregion

        #region Drawing

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void Clear(int aColor)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {aColor}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            switch (_Mode.ColorDepth)
            {
                case ColorDepth.ColorDepth4:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth8:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth16:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth24:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth32:
                    _VBEDriver.ClearVRAM((uint)aColor);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void Clear(Color aColor)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {aColor}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            switch (_Mode.ColorDepth)
            {
                case ColorDepth.ColorDepth4:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth8:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth16:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth24:
                    throw new NotImplementedException();
                case ColorDepth.ColorDepth32:
                    _VBEDriver.ClearVRAM((uint)aColor.ToArgb());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /*
         * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
         * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
         * to be drawn slowing down all.
         */
        /// <summary>
        /// Draw point to the screen.
        /// </summary>
        /// <param name="aColor">Color to draw the point with.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported (currently only 32 is supported).</exception>
        public override void DrawPoint(Color aColor, int aX, int aY)
        {
            uint offset;

            /*
             * For now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise.
             *
             * How to support other ColorDepth? The offset calculation should be the same (and so could be done out of the switch)
             * ColorDepth.ColorDepth16 and ColorDepth.ColorDepth8 need a conversion from color (an ARGB32 color) to the RGB16 and RGB8
             * how to do this conversion faster maybe using pre-computed tables? What happens if the color cannot be converted? We will throw?
             */
            switch (Mode.ColorDepth)
            {
                case ColorDepth.ColorDepth32:

                    offset = (uint)GetPointOffset(aX, aY);

                    if (aColor.A < 255)
                    {
                        if (aColor.A == 0)
                        {
                            return;
                        }

                        aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
                    }

                    _VBEDriver.SetVRAM(offset, aColor.B);
                    _VBEDriver.SetVRAM(offset + 1, aColor.G);
                    _VBEDriver.SetVRAM(offset + 2, aColor.R);
                    _VBEDriver.SetVRAM(offset + 3, aColor.A);

                    break;
                case ColorDepth.ColorDepth24:

                    offset = (uint)GetPointOffset(aX, aY);

                    _VBEDriver.SetVRAM(offset, aColor.B);
                    _VBEDriver.SetVRAM(offset + 1, aColor.G);
                    _VBEDriver.SetVRAM(offset + 2, aColor.R);

                    break;
                default:
                    string errorMsg = "DrawPoint() with ColorDepth " + (int)Mode.ColorDepth + " not yet supported";
                    throw new NotImplementedException(errorMsg);
            }
        }

        /// <summary>
        /// Draw point to the screen.
        /// Not implemented.
        /// </summary>
        /// <param name="aColor">Color to draw the point with.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always (only int coordinats supported).</exception>
        public override void DrawPoint(Color aColor, float aX, float aY)
        {
            throw new NotImplementedException();
        }

        /* This is just temp */
        /// <summary>
        /// Draw array of colors.
        /// </summary>
        /// <param name="aColors">Colors array.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <param name="aWidth">Width.</param>
        /// <param name="aHeight">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid, or width is less than 0.</exception>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported.</exception>
        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            ThrowIfCoordNotValid(aX, aY);

            ThrowIfCoordNotValid(aX + aWidth, aY + aHeight);

            for (int i = 0; i < aX; i++)
            {

                for (int ii = 0; ii < aY; ii++)
                {

                    DrawPoint(aColors[i + ii * aWidth], i, ii);

                }
            }
        }

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="aColor">Color to draw with.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <param name="aWidth">Width.</param>
        /// <param name="aHeight">Height.</param>
        public override void DrawFilledRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight)
        {
            //ClearVRAM clears one uint at a time. So we clear pixelwise not byte wise. That's why we divide by 32 and not 8.
            aWidth = Math.Min(aWidth, Mode.Width - aX) * (int)Mode.ColorDepth / 32;
            var color = aColor.ToArgb();

            for (int i = aY; i < aY + aHeight; i++)
            {
                _VBEDriver.ClearVRAM(GetPointOffset(aX, i), aWidth, color);
            }
        }

        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="aImage">Image.</param>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        public override void DrawImage(Image aImage, int aX, int aY)
        {
            var xBitmap = aImage.rawData;
            var xWidth = (int)aImage.Width;
            var xHeight = (int)aImage.Height;

            int xOffset = GetPointOffset(aX, aY);
            int xScreenWidthInPixel = Mode.Width;

            for (int i = 0; i < xHeight; i++)
            {
                _VBEDriver.CopyVRAM(i * xScreenWidthInPixel + xOffset, xBitmap, i * xWidth, xWidth);
            }
        }

        #endregion

        /// <summary>
        /// Display screen
        /// </summary>
        public override void Display()
        {
            _VBEDriver.Swap();
        }

        #region Reading

        /// <summary>
        /// Get point color.
        /// </summary>
        /// <param name="aX">X coordinate.</param>
        /// <param name="aY">Y coordinate.</param>
        /// <returns>Color value.</returns>
        public override Color GetPointColor(int aX, int aY)
        {
            uint offset = (uint)GetPointOffset(aX, aY);

            return Color.FromArgb((int)_VBEDriver.GetVRAM(offset));
        }

        #endregion

    }
}
