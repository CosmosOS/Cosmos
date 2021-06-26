using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers;

namespace Cosmos.System.Graphics
{
    public class VBECanvas : Canvas
    {
         // debugger
        internal Debugger debugger = new Debugger("System", "VBECanvas");

        // driver
        private VBEDriver driver;
        private Color pointCol;

        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>()
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
        /// Default graphics mode - 640x480x32
        /// </summary>
        public override Mode DefaultGraphicMode { get; } = new Mode(640, 480, ColorDepth.ColorDepth32);

        /// <summary>
        /// Get and set active video mode
        /// </summary>
        private Mode mode;
        public override Mode Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                if (SendDebug) { debugger.SendInternal($"Set mode property to {value}"); }
                SetMode(value);
            }
        }

        /// <summary>
        /// Create new VBE/VESA canvas with default mode
        /// </summary>
        public VBECanvas() { this.Mode = DefaultGraphicMode; }

        /// <summary>
        /// Create new VBE/VESA canvas with specified mode
        /// </summary>
        /// <param name="mode"></param>
        public VBECanvas(Mode mode) { this.Mode = mode; }

        // set graphics mode
        private void SetMode(Mode mode)
        {
            this.mode = mode;
            if (!CheckIfModeIsValid(mode)) { throw new Exception($"Unsupported mode {mode}"); }
            if (driver == null) { driver = new VBEDriver((ushort)mode.Width, (ushort)mode.Height, (ushort)mode.Depth); }
            else { driver.VBESet((ushort)mode.Width, (ushort)mode.Height, (ushort)mode.Depth); }
        }


        /// <summary>
        /// Clear the screen with specified color
        /// </summary>
        /// <param name="color"></param>
        public override void Clear(Color color) { driver.ClearVRAM((uint)color.ToArgb()); }

        /// <summary>
        /// Disable video driver
        /// </summary>
        public override void Disable() { driver.DisableDisplay(); }

        /// <summary>
        /// Render canvas onto screen
        /// </summary>
        public override void Display() { driver.Swap(); }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public override void DrawFilledRectangle(Color color, int x, int y, int width, int height)
        {
            for (int i = 0; i < width * height; i++) { DrawPoint(color, i % width, i / width); }
        }


        /// <summary>
        /// Draw point with color at specified position
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(Color color, int x, int y)
        {
            // return if
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { if (SendDebug) { debugger.SendInternal("Tried to draw pixel out of bounds at " + x.ToString() + ", " + y.ToString()); } return; }
            if (color.A == 0) { return; }
            // get offset
            uint offset = GetPointOffset(x, y);
            // get alpha
            pointCol = color;
            if (pointCol.A < 255) { pointCol = AlphaBlend(color, GetPointColor(x, y), pointCol.A); }
            // draw pixel
            driver.SetVRAM(offset, pointCol.B);
            driver.SetVRAM(offset + 1, pointCol.G);
            driver.SetVRAM(offset + 2, pointCol.R);
            driver.SetVRAM(offset + 3, pointCol.A);
        }

        /// <summary>
        /// Get color at specified position on screen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override Color GetPointColor(int x, int y)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { if (SendDebug) { debugger.SendInternal("Tried to draw pixel out of bounds at " + x.ToString() + ", " + y.ToString()); } return Color.Black; }
            uint offset = GetPointOffset(x, y);
            return Color.FromArgb((int)driver.GetVRAM(offset));
        }

        /// <summary>
        /// Convert screen position to offset
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public uint GetPointOffset(int x, int y)
        {
            if (SendDebug) { Global.mDebugger.SendInternal($"Computing offset for coordinates {x},{y}"); }
            int xBytePerPixel = (int)Mode.Depth / 8;
            int stride = (int)Mode.Depth / 8;
            int pitch = Mode.Width * xBytePerPixel;
            return (uint)((x * stride) + (y * pitch));
        }
    }
}
