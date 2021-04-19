using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.HAL.Drivers;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class VBECanvas : Canvas
    {
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

        // default mode
        public override Mode DefaultGraphicMode { get; } = new Mode(640, 480, ColorDepth.ColorDepth32);

        // current mode
        private Mode mode;
        public override Mode Mode { get { return mode; } set
            {
                mode = value;
                if (!CheckIfModeIsValid(value)) { throw new Exception("Unsupported video mode " + value.Width.ToString() + "x" + value.Height.ToString() + " at " + value.Depth.ToString()); }
                driver.VBESet((ushort)value.Width, (ushort)value.Height, (ushort)value.Depth);
            } }

        // driver
        private VBEDriver driver;

        // constructor
        public VBECanvas()
        {
            this.Mode = DefaultGraphicMode;
            driver = new VBEDriver((ushort)Mode.Width, (ushort)Mode.Height, (ushort)Mode.Depth);
        }

        // constructor with mode
        public VBECanvas(Mode aMode)
        {
            this.mode = aMode;
            driver = new VBEDriver((ushort)aMode.Width, (ushort)aMode.Height, (ushort)aMode.Depth);
        }

        // clear
        public override void Clear(Color color)
        {
            driver.ClearVRAM((uint)color.ToArgb());
        }

        // draw point
        private uint tempCol = 0;
        public override void DrawPoint(int x, int y, Color color)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { return; }
            tempCol = (uint)color.ToArgb();
            driver.SetVRAM(GetPointOffset(x, y), color.B);
            driver.SetVRAM(GetPointOffset(x, y) + 1, color.G);
            driver.SetVRAM(GetPointOffset(x, y) + 2, color.R);
            driver.SetVRAM(GetPointOffset(x, y) + 3, color.A);
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
        public override Color GetPointColor(int x, int y) { return Color.FromArgb((int)driver.GetVRAM(GetPointOffset(x, y))); }

        // disable driver
        public override void Disable() { driver.DisableDisplay(); }

        // display
        public override void Display() { driver.Swap(); }

        // get point offset
        private uint GetPointOffset(int x, int y)
        {
            uint xBytePerPixel = 32 / 8;
            uint stride = 32 / 8;
            uint pitch = (uint)Mode.Width * xBytePerPixel;

            return (uint)((uint)x * stride) + ((uint)y * pitch);
        }

    }
}
