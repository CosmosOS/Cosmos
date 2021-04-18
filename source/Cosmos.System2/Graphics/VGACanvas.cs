using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class VGACanvas : Canvas
    {
        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>()
        {
            new Mode(640, 480, ColorDepth.ColorDepth4),
            new Mode(720, 480, ColorDepth.ColorDepth4),
            new Mode(320, 200, ColorDepth.ColorDepth8),
        };

        // default mode
        public override Mode DefaultGraphicMode { get; } = new Mode(320, 200, ColorDepth.ColorDepth8);

        // current mode
        private Mode mode;
        public override Mode Mode { get { return mode; } set
            {
                mode = value;
                if (!CheckIfModeIsValid(value)) { throw new Exception("Unsupported video mode " + value.Width.ToString() + "x" + value.Height.ToString() + " at " + value.Depth.ToString()); }
                if (value.Width == 320) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size320x200, VGADriver.ColorDepth.BitDepth8); }
                else if (value.Width == 640) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size640x480, VGADriver.ColorDepth.BitDepth4); }
                else if (value.Width == 720) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size720x480, VGADriver.ColorDepth.BitDepth4); }
            } }

        // driver
        private VGADriver driver;

        // constructor
        public VGACanvas()
        {
            driver = new VGADriver();
            this.Mode = DefaultGraphicMode;
        }

        // constructor with mode
        public VGACanvas(Mode aMode)
        {
            driver = new VGADriver();
            this.Mode = aMode;
        }

        // clear
        public override void Clear(Color color)
        {
            for (int i = 0; i < Mode.Width * Mode.Height; i++) { driver.SetPixel((uint)(i % Mode.Width), (uint)(i / Mode.Width), color); }
        }

        // draw point
        public override void DrawPoint(int x, int y, Color color)
        {
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { return; }
            driver.SetPixel((uint)x, (uint)y, color);
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
        public override void Disable() { driver.SetTextMode(VGADriver.TextSize.Size80x25); }

        // display
        public override void Display() { }

    }
}
