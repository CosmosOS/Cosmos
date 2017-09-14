using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class SVGAIIScreen : Canvas
    {
        public VMWareSVGAII xSVGAIIDriver;
        internal Debug.Kernel.Debugger mSVGAIIDebugger = new Debug.Kernel.Debugger("System", "SVGAIIScreen");

        public SVGAIIScreen() : base()
        {
            mSVGAIIDebugger.SendInternal($"Creting new SVGAIIScreen with default mode {defaultGraphicMode}");

            xSVGAIIDriver = new VMWareSVGAII();
            xSVGAIIDriver.SetMode((uint)defaultGraphicMode.Columns, (uint)defaultGraphicMode.Rows, (uint)defaultGraphicMode.ColorDepth);
        }

        public SVGAIIScreen(Mode aMode) : base(aMode)
        {
            ThrowIfModeIsNotValid(aMode);

            xSVGAIIDriver = new VMWareSVGAII();
            xSVGAIIDriver.SetMode((uint)aMode.Columns, (uint)aMode.Rows, (uint)aMode.ColorDepth);
        }

        public override Mode Mode
        {
            get => mode;
            set
            {
                mode = value;
                SetGraphicsMode(mode);
            }
        }

        public override void DrawPoint(Pen pen, int x, int y)
        {
            Color xColor = pen.Color;

            mSVGAIIDebugger.SendInternal($"Drawing point to x:{x}, y:{y} with {xColor.Name} Color");
            xSVGAIIDriver.SetPixel((uint)x, (uint)y, (uint)xColor.ToArgb());
            mSVGAIIDebugger.SendInternal($"Done drawing point");
            xSVGAIIDriver.Update(0, 0, (uint)mode.Columns, (uint)mode.Rows);
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }

        public override void DrawFilledRectangle(Pen pen, int x_start, int y_start, int width, int height)
        {
            xSVGAIIDriver.Fill((uint)x_start, (uint)y_start, (uint)width, (uint)height, (uint)pen.Color.ToArgb());
        }

        public override List<Mode> getAvailableModes()
        {
            return new List<Mode>
            {
            /* 16-bit Depth Resolutions*/

                /* SD Resolutions */
                new Mode(320, 200, ColorDepth.ColorDepth16),
                new Mode(320, 240, ColorDepth.ColorDepth16),
                new Mode(640, 480, ColorDepth.ColorDepth16),
                new Mode(720, 480, ColorDepth.ColorDepth16),
                new Mode(800, 600, ColorDepth.ColorDepth16),
                new Mode(1152, 768, ColorDepth.ColorDepth16),

                /* Old HD-Ready Resolutions */
                new Mode(1280, 720, ColorDepth.ColorDepth16),
                new Mode(1280, 768, ColorDepth.ColorDepth16),
                new Mode(1280, 800, ColorDepth.ColorDepth16),  // WXGA
                new Mode(1280, 1024, ColorDepth.ColorDepth16), // SXGA

                /* Better HD-Ready Resolutions */
                new Mode(1360, 768, ColorDepth.ColorDepth16),
                new Mode(1366, 768, ColorDepth.ColorDepth16),  // Original Laptop Resolution
                new Mode(1440, 900, ColorDepth.ColorDepth16),  // WXGA+
                new Mode(1400, 1050, ColorDepth.ColorDepth16), // SXGA+
                new Mode(1600, 1200, ColorDepth.ColorDepth16), // UXGA
                new Mode(1680, 1050, ColorDepth.ColorDepth16), // WXGA++

                /* HDTV Resolutions */
                new Mode(1920, 1080, ColorDepth.ColorDepth16),
                new Mode(1920, 1200, ColorDepth.ColorDepth16), // WUXGA

                /* 2K Resolutions */
                new Mode(2048, 1536, ColorDepth.ColorDepth16), // QXGA
                new Mode(2560, 1080, ColorDepth.ColorDepth16), // UW-UXGA
                new Mode(2560, 1600, ColorDepth.ColorDepth16), // WQXGA
                new Mode(2560, 2048, ColorDepth.ColorDepth16), // QXGA+
                new Mode(3200, 2048, ColorDepth.ColorDepth16), // WQXGA+
                new Mode(3200, 2400, ColorDepth.ColorDepth16), // QUXGA
                new Mode(3840, 2400, ColorDepth.ColorDepth16), // WQUXGA
                
            /* 32-bit Depth Resolutions*/
                /* SD Resolutions */
                new Mode(320, 200, ColorDepth.ColorDepth32),
                new Mode(320, 240, ColorDepth.ColorDepth32),
                new Mode(640, 480, ColorDepth.ColorDepth32),
                new Mode(720, 480, ColorDepth.ColorDepth32),
                new Mode(800, 600, ColorDepth.ColorDepth32),
                new Mode(1152, 768, ColorDepth.ColorDepth32),

                /* Old HD-Ready Resolutions */
                new Mode(1280, 720, ColorDepth.ColorDepth32),
                new Mode(1280, 768, ColorDepth.ColorDepth32),
                new Mode(1280, 800, ColorDepth.ColorDepth32),  // WXGA
                new Mode(1280, 1024, ColorDepth.ColorDepth32), // SXGA

                /* Better HD-Ready Resolutions */
                new Mode(1360, 768, ColorDepth.ColorDepth32),
                new Mode(1366, 768, ColorDepth.ColorDepth32),  // Original Laptop Resolution
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
        }

        protected override Mode getDefaultGraphicMode() => new Mode(1024, 768, ColorDepth.ColorDepth16);

        private void SetGraphicsMode(Mode aMode)
        {
            ThrowIfModeIsNotValid(aMode);

            var xWidth = (uint)aMode.Columns;
            var xHeight = (uint)aMode.Rows;
            var xColorDepth = (uint)aMode.ColorDepth;

            xSVGAIIDriver.SetMode(xWidth, xHeight, xColorDepth);
        }

        public override void Clear(Color color)
        {
            xSVGAIIDriver.Fill(0, 0, (uint)mode.Columns, (uint)mode.Rows, (uint)color.ToArgb());
        }

        public Color GetPixel(int aX, int aY)
        {
            var xColorARGB = xSVGAIIDriver.GetPixel((uint)aX, (uint)aX);
            var xColor = Color.FromArgb((int)xColorARGB);
            return xColor;
        }

        public void SetCursor(bool aVisible, int aX, int aY)
        {
            xSVGAIIDriver.SetCursor(aVisible, (uint)aX, (uint)aY);
        }

        public void CreateCursor()
        {
            xSVGAIIDriver.DefineCursor();
        }

        public void CopyPixel(int aX, int aY, int aNewX, int aNewY, int aWidth = 1, int aHeight = 1)
        {
            xSVGAIIDriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, (uint)aWidth, (uint)aHeight);
        }

        public void MovePixel(int aX, int aY, int aNewX, int aNewY)
        {
            xSVGAIIDriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, 1, 1);
            xSVGAIIDriver.SetPixel((uint)aX, (uint)aY, 0);
        }

        public override Color GetPointColor(int x, int y)
        {
            return Color.FromArgb((int)xSVGAIIDriver.GetPixel((uint)x, (uint)y));
        }
    }
}
