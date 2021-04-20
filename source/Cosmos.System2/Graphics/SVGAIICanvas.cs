using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class SVGAIICanvas : Canvas
    {
        // debugger
        internal Debugger debugger = new Debugger("System", "SVGAIICanvas");

        // driver
        private VMWareSVGAII driver;

        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>()
        {
            new Mode(320, 200, ColorDepth.ColorDepth32),
            new Mode(320, 240, ColorDepth.ColorDepth32),
            new Mode(512, 384, ColorDepth.ColorDepth32),
            new Mode(640, 480, ColorDepth.ColorDepth32),
            new Mode(720, 480, ColorDepth.ColorDepth32),
            new Mode(800, 600, ColorDepth.ColorDepth32),
            new Mode(1024, 768, ColorDepth.ColorDepth32),
            new Mode(1152, 768, ColorDepth.ColorDepth32),

            /* Old HD-Ready Resolutions */
            new Mode(1280, 720, ColorDepth.ColorDepth32),
            new Mode(1280, 768, ColorDepth.ColorDepth32),
            new Mode(1280, 800, ColorDepth.ColorDepth32),  // WXGA
            new Mode(1280, 1024, ColorDepth.ColorDepth32), // SXGA

            /* Better HD-Ready Resolutions */
            new Mode(1360, 768, ColorDepth.ColorDepth32),
            //new Mode(1366, 768, ColorDepth.ColorDepth32),  // Original Laptop Resolution - this one is for some reason broken in vmware
            new Mode(1440, 900, ColorDepth.ColorDepth32),  // WXGA+
            new Mode(1400, 1050, ColorDepth.ColorDepth32), // SXGA+
            new Mode(1600, 1200, ColorDepth.ColorDepth32), // UXGA
            new Mode(1680, 1050, ColorDepth.ColorDepth32), // WXGA++

            /* HDTV Resolutions */
            new Mode(1920, 1080, ColorDepth.ColorDepth32),
            new Mode(1920, 1200, ColorDepth.ColorDepth32), // WUXGA

            /* 2K Resolutions */
            new Mode(2048, 1536, ColorDepth.ColorDepth32), // QXGA
            new Mode(2560, 1080, ColorDepth.ColorDepth32), // UW-UXGA
            new Mode(2560, 1600, ColorDepth.ColorDepth32), // WQXGA
            new Mode(2560, 2048, ColorDepth.ColorDepth32), // QXGA+
            new Mode(3200, 2048, ColorDepth.ColorDepth32), // WQXGA+
            new Mode(3200, 2400, ColorDepth.ColorDepth32), // QUXGA
            new Mode(3840, 2400, ColorDepth.ColorDepth32), // WQUXGA
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
        /// Create new SVGA canvas with default mode
        /// </summary>
        public SVGAIICanvas() { driver = new VMWareSVGAII(); this.Mode = DefaultGraphicMode; }

        /// <summary>
        /// Create new SVGA canvas with specified mode
        /// </summary>
        /// <param name="mode"></param>
        public SVGAIICanvas(Mode mode) { driver = new VMWareSVGAII(); this.Mode = mode; }

        // set graphics mode
        private void SetMode(Mode mode)
        {
            this.mode = mode;
            if (!CheckIfModeIsValid(mode)) { throw new Exception($"Unsupported mode {mode}"); }
            if (driver == null) { driver = new VMWareSVGAII(); }
            driver.SetMode((uint)mode.Width, (uint)mode.Height, (uint)mode.Depth);

        }

        /// <summary>
        /// Clear the screen with specified color
        /// </summary>
        /// <param name="color"></param>
        public override void Clear(Color color) { driver.Clear((uint)color.ToArgb()); }

        /// <summary>
        /// Disable video driver
        /// </summary>
        public override void Disable() { driver.Disable(); }

        /// <summary>
        /// Render canvas onto screen
        /// </summary>
        public override void Display() { driver.Update(0, 0, (uint)Mode.Width, (uint)Mode.Height); }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public override void DrawFilledRectangle(Color color, int x, int y, int width, int height) { driver.Fill((uint)x, (uint)y, (uint)width, (uint)height, (uint)color.ToArgb()); }

        public override void DrawImage(Image image, int x, int y)
        {
            for (int i = 0; i < image.Width * image.Height; i++)
            {
                DrawPoint(image.RawData[i], x + (i % image.Width), y + (i / image.Width));
            }
        }

        /// <summary>
        /// Draw point with color at specified position
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(Color color, int x, int y)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { if (SendDebug) { debugger.SendInternal("Tried to draw pixel out of bounds at " + x.ToString() + ", " + y.ToString()); } return; }
            driver.SetPixel((uint)x, (uint)y, (uint)color.ToArgb());
        }

        /// <summary>
        /// Draw point with packed color at specified position
        /// </summary>
        /// <param name="packedColor"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void DrawPoint(uint packedColor, int x, int y)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { if (SendDebug) { debugger.SendInternal("Tried to draw pixel out of bounds at " + x.ToString() + ", " + y.ToString()); } return; }
            driver.SetPixel((uint)x, (uint)y, packedColor);
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
            return Color.FromArgb((int)driver.GetPixel((uint)x, (uint)y));
        }
    }
}
