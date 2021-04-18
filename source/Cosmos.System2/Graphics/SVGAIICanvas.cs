using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class SVGAIICanvas : Canvas
    {
        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>()
        {
            new Mode(320, 200, ColorDepth.ColorDepth32),
            new Mode(320, 240, ColorDepth.ColorDepth32),
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

        // default mode
        public override Mode DefaultGraphicMode { get; } = new Mode(640, 480, ColorDepth.ColorDepth32);

        // current mode
        private Mode mode;
        public override Mode Mode { get { return mode; } set
            {
                mode = value;
                if (!CheckIfModeIsValid(value)) { throw new Exception("Unsupported video mode " + value.Width.ToString() + "x" + value.Height.ToString() + " at " + value.Depth.ToString()); }
                driver.SetMode((uint)value.Width, (uint)value.Height, (uint)value.Depth);
            } }

        // driver
        private VMWareSVGAII driver;

        // constructor
        public SVGAIICanvas()
        {
            driver = new VMWareSVGAII();
            this.Mode = DefaultGraphicMode;
        }

        // constructor with mode
        public SVGAIICanvas(Mode aMode)
        {
            driver = new VMWareSVGAII();
            this.Mode = aMode;
        }

        // clear
        public override void Clear(Color color) { driver.Clear((uint)color.ToArgb()); }

        // draw point
        public override void DrawPoint(int x, int y, Color color)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { return; }
            driver.SetPixel((uint)x, (uint)y, (uint)color.ToArgb());
        }

        // draw filled rectangle
        private int dx, dy;
        public override void DrawFilledRectangle(int x, int y, int width, int height, Color color)
        {
            for (int i = 0; i < width * height; i++)
            {
                dx = x + (i % width);
                dy = y + (i / width);
                DrawPoint(dx, dy, color);
            }
        }

        // get point color
        public override Color GetPointColor(int x, int y) { return Color.FromArgb((int)driver.GetPixel((uint)x, (uint)y)); }

        // disable driver
        public override void Disable() { driver.Disable(); }

        // display
        public override void Display() { driver.Update(0, 0, (uint)Mode.Width, (uint)Mode.Height); }

    }
}
