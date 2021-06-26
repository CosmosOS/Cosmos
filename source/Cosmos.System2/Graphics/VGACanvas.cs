using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class VGACanvas : Canvas
    {
        // debugger
        internal Debugger debugger = new Debugger("System", "VGACanvas");

        // driver
        private VGADriver driver;

        // available modes
        public override List<Mode> AvailableModes { get; } = new List<Mode>()
        {
            new Mode(640, 480, ColorDepth.ColorDepth4),
            new Mode(720, 480, ColorDepth.ColorDepth4),
            new Mode(320, 200, ColorDepth.ColorDepth8),
        };

        /// <summary>
        /// Default graphics mode - 320x200x8
        /// </summary>
        public override Mode DefaultGraphicMode { get; } = new Mode(320, 200, ColorDepth.ColorDepth8);

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
        /// Create new VGA canvas with default mode
        /// </summary>
        public VGACanvas() { this.Mode = DefaultGraphicMode; }

        /// <summary>
        /// Create new VGA canvas with specified mode
        /// </summary>
        /// <param name="mode"></param>
        public VGACanvas(Mode mode) { this.Mode = mode; }

        // set graphics mode
        private void SetMode(Mode mode)
        {
            this.mode = mode;
            if (!CheckIfModeIsValid(mode)) { throw new Exception($"Unsupported mode {mode}"); }
            if (driver == null) { driver = new VGADriver(); }
            if (mode.Width == 720) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size720x480, VGADriver.ColorDepth.BitDepth4); }
            else if (mode.Width == 640) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size640x480, VGADriver.ColorDepth.BitDepth4); }
            else if (mode.Width == 320) { driver.SetGraphicsMode(VGADriver.ScreenSize.Size320x200, VGADriver.ColorDepth.BitDepth8); }
            else { throw new Exception($"Unsupported mode {mode}"); }
        }

        /// <summary>
        /// Clear the screen with specified color
        /// </summary>
        /// <param name="color"></param>
        public override void Clear(Color color) { for (int i = 0; i < Mode.Width * Mode.Height; i++) { driver.SetPixel((uint)(i % Mode.Width), (uint)(i / Mode.Width), color); } }

        /// <summary>
        /// Disable video driver
        /// </summary>
        public override void Disable() { driver.SetTextMode(VGADriver.TextSize.Size80x25); }

        /// <summary>
        /// Render canvas onto screen - not required for VGA mode
        /// </summary>
        public override void Display() { return; }

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
            if (x < 0 || x >= Mode.Width || y < 0 || y >= Mode.Height) { if (SendDebug) { debugger.SendInternal("Tried to draw pixel out of bounds at " + x.ToString() + ", " + y.ToString()); } return; }
            driver.SetPixel((uint)x, (uint)y, color);
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
