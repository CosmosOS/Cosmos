//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;

using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VBEScreen class. Used to control screen, by VBE (VESA BIOS Extensions) standard. See also: <seealso cref="Canvas"/>.
    /// </summary>
    public class VBEScreen : Canvas
    {
        /// <summary>
        /// Default video mode. 1024x768x32.
        /// </summary>
        private static readonly Mode DefaultMode = new Mode(1024, 768, ColorDepth.ColorDepth32);

        /// <summary>
        /// Driver for Setting vbe modes and ploting/getting pixels
        /// </summary>
        private readonly VBEDriver VBEDriver;

        /// <summary>
        /// Video mode.
        /// </summary>
        private Mode _mode;

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if default mode (1024x768x32) is not suppoted.</exception>
        public VBEScreen()
            : this(DefaultMode)
        {
        }

        /// <summary>
        /// Create new instance of the <see cref="VBEScreen"/> class.
        /// </summary>
        /// <param name="mode">VBE mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        public VBEScreen(Mode mode)
        {
            Global.mDebugger.SendInternal($"Creating new VBEScreen() with mode {mode.Columns}x{mode.Rows}x{(uint)mode.ColorDepth}");

            ThrowIfModeIsNotValid(mode);

            VBEDriver = new VBEDriver((ushort)mode.Columns, (ushort)mode.Rows, (ushort)mode.ColorDepth);
            Mode = mode;
        }

        /// <summary>
        /// Get and set video mode.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">(set) Thrown if mode is not suppoted.</exception>
        public override Mode Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                SetMode(_mode);
            }
        }

        #region Display

        /*/// <summary>
        /// All the available screen modes VBE supports, I would like to query the hardware and obtain from it the list but I have
        /// not yet find how to do it! For now I hardcode the most used VESA modes, VBE seems to support until HDTV resolution
        /// without problems that is well... excellent :-)
        /// </summary>*/
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
        public override Mode DefaultGraphicMode => DefaultMode;

        /// <summary>
        /// Use this to setup the screen, this will disable the console.
        /// </summary>
        /// <param name="Mode">The desired Mode resolution</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        private void SetMode(Mode mode)
        {
            ThrowIfModeIsNotValid(mode);

            ushort xres = (ushort)Mode.Columns;
            ushort yres = (ushort)Mode.Rows;
            ushort bpp = (ushort)Mode.ColorDepth;

            //set the screen
           VBEDriver.VBESet(xres, yres, bpp);
        }
#endregion

#region Drawing

        /// <summary>
        /// Clear screen to specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        public override void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");
            //if (color == null)
            //   throw new ArgumentNullException(nameof(color));

            /*
             * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
             * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
             * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
             * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
             */
            VBEDriver.ClearVRAM((uint)color.ToArgb());
        }

        /*
         * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
         * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
         * to be drawn slowing down all.
         */
        /// <summary>
        /// Draw point to the screen.
        /// </summary>
        /// <param name="pen">Pen to draw the point with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported (currently only 32 is supported).</exception>
        public override void DrawPoint(Pen pen, int x, int y)
        {
            Color color = pen.Color;
            uint offset;
            uint ColorDepthInBytes = (uint)Mode.ColorDepth / 8;

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
                    offset = (uint)GetPointOffset(x, y);

                    Global.mDebugger.SendInternal($"Drawing Point of color {color} at offset {offset}");

                    VBEDriver.SetVRAM(offset, (uint)color.ToArgb());

                    Global.mDebugger.SendInternal("Point drawn");
                    break;
                case ColorDepth.ColorDepth24:

                    offset = (uint)GetPointOffset(x, y);
                    Global.mDebugger.SendInternal($"Drawing Point of color {color} at offset {offset}");
                    VBEDriver.SetVRAM(offset, (((uint)color.R * 1000 + color.G) * 1000 + color.B));

                    Global.mDebugger.SendInternal("Point drawn");
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
        /// <param name="pen">Pen to draw the point with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always (only int coordinats supported).</exception>
        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }

        /* This is just temp */
        /// <summary>
        /// Draw array of colors.
        /// </summary>
        /// <param name="colors">Colors array.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid, or width is less than 0.</exception>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported.</exception>
        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            ThrowIfCoordNotValid(x, y);
            
            ThrowIfCoordNotValid(x + width, y + height);

            for (int i = 0; i < x; i++)
            {

                for (int ii = 0; ii < y; ii++)
                {

                    DrawPoint(new Pen(colors[i + (ii * width)]), i, ii);

                }
            }
        }

        /// <summary>
        /// Get point offset.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>int value.</returns>
        private int GetPointOffset(int x, int y)
        {
            Global.mDebugger.SendInternal($"Computing offset for coordinates {x},{y}");
            int xBytePerPixel = (int)Mode.ColorDepth / 8;
            int stride = (int)Mode.ColorDepth / 8;
            int pitch = Mode.Columns * xBytePerPixel;

            return (x * stride) + (y * pitch);
        }

        /// <summary>
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="pen">Pen to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        public override void DrawFilledRectangle(Pen pen, int x, int y, int width, int height)
        {
            int xOffset = GetPointOffset(x, y);
            int xScreenWidthInPixel = Mode.Columns * ((int)Mode.ColorDepth / 8);

            for (int i = 0; i < height; i++)
            {
                VBEDriver.ClearVRAM((i * xScreenWidthInPixel) + xOffset, width, pen.Color.ToArgb());
            }
        }

        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="image">Image.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public override void DrawImage(Image image, int x, int y)
        {
            var xBitmap = image.rawData;
            var xWidht = (int)image.Width;
            var xHeight = (int)image.Height;

            int xOffset = GetPointOffset(x, y);
            int xScreenWidthInPixel = Mode.Columns * ((int)Mode.ColorDepth / 8);

            Global.mDebugger.SendInternal($"Drawing image of size {image.Width}x{image.Height} array size {image.rawData.Length}");
            for (int i = 0; i < xHeight; i++)
            {
                VBEDriver.CopyVRAM((i * xScreenWidthInPixel) + xOffset, xBitmap, (i * xWidht), xWidht);
            }
            Global.mDebugger.SendInternal("Done");
        }

        #endregion

        #region Reading

        /// <summary>
        /// Get point color.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Color value.</returns>
        public override Color GetPointColor(int x, int y)
        {
            uint pitch;
            uint stride;
            uint offset;
            uint ColorDepthInBytes = (uint)Mode.ColorDepth / 8;

            Global.mDebugger.SendInternal("Computing offset...");
            pitch = (uint)Mode.Columns * ColorDepthInBytes;
            stride = ColorDepthInBytes;
            //offset = ((uint)x * pitch) + ((uint)y * stride);
            offset = ((uint)x * stride) + ((uint)y * pitch);

            Global.mDebugger.SendInternal($"Getting color from point at offset {offset}");
            return Color.FromArgb(VBEDriver.GetVRAM(offset));
        }

#endregion

    }
}
