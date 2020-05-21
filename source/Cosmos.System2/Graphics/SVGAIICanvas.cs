//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;

using Cosmos.Debug.Kernel;
using Cosmos.HAL.Drivers.PCI.Video;

namespace Cosmos.System.Graphics
{
    public class SVGAIICanvas : Canvas
    {
        public override void Disable()
        {
            _CanvasSVGADriver.Disable();
        }

        internal Debugger mSVGAIIDebugger = new Debugger("System", "SVGAIIScreen");
        
        private static readonly Mode _DefaultMode = new Mode(1024, 768, ColorDepth.ColorDepth32);

        private Mode _Mode;

        private readonly VMWareSVGAII _CanvasSVGADriver;

        public SVGAIICanvas()
            : this(_DefaultMode)
        {
        }

        public SVGAIICanvas(Mode aMode)
        {
            mSVGAIIDebugger.SendInternal($"Called ctor with mode {aMode}");
            ThrowIfModeIsNotValid(aMode);

            _CanvasSVGADriver = new VMWareSVGAII();
            Mode = aMode;
        }

        public override Mode Mode
        {
            get => _Mode;
            set
            {
                _Mode = value;
                mSVGAIIDebugger.SendInternal($"Called Mode set property with mode {_Mode}");
                SetGraphicsMode(_Mode);
            }
        }

        public override Mode DefaultGraphicMode => _DefaultMode;

        public override void DrawPoint(Pen aPen, int aX, int aY)
        {
            Color xColor = aPen.Color;

            mSVGAIIDebugger.SendInternal($"Drawing point to x:{aX}, y:{aY} with {xColor.Name} Color");
            _CanvasSVGADriver.SetPixel((uint)aX, (uint)aY, (uint)xColor.ToArgb());
            mSVGAIIDebugger.SendInternal($"Done drawing point");
            /* No need to refresh all the screen to make the point appear on Screen! */
            //xSVGAIIDriver.Update((uint)x, (uint)y, (uint)mode.Columns, (uint)mode.Rows);
            _CanvasSVGADriver.Update((uint)aX, (uint)aY, 1, 1);
        }

        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            throw new NotImplementedException();
            //xSVGAIIDriver.
        }
        public override void DrawPoint(Pen aPen, float aX, float aY)
        {
            //xSVGAIIDriver.
            throw new NotImplementedException();
        }

        public override void DrawFilledRectangle(Pen aPen, int aX_start, int aY_start, int aWidth, int aHeight)
        {
            _CanvasSVGADriver.Fill((uint)aX_start, (uint)aY_start, (uint)aWidth, (uint)aHeight, (uint)aPen.Color.ToArgb());
        }

        //public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>
        public override List<Mode> AvailableModes { get; } = new List<Mode>
        {
            /*  VmWare maybe supports 16 bit resolutions but CGS not yet (we should need to do RGB32->RGB16 conversion) */
#if false
            /* 16-bit Depth Resolutions*/

            /* SD Resolutions */
            new Mode(320, 200, ColorDepth.ColorDepth16),
            new Mode(320, 240, ColorDepth.ColorDepth16),
            new Mode(640, 480, ColorDepth.ColorDepth16),
            new Mode(720, 480, ColorDepth.ColorDepth16),
            new Mode(800, 600, ColorDepth.ColorDepth16),
            new Mode(1024, 768, ColorDepth.ColorDepth16),
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
#endif
            /* 32-bit Depth Resolutions*/
            /* SD Resolutions */
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
        
        private void SetGraphicsMode(Mode aMode)
        {
            ThrowIfModeIsNotValid(aMode);

            var xWidth = (uint)aMode.Columns;
            var xHeight = (uint)aMode.Rows;
            var xColorDepth = (uint)aMode.ColorDepth;

            _CanvasSVGADriver.SetMode(xWidth, xHeight, xColorDepth);
        }

        public override void Clear(Color aColor)
        {
            _CanvasSVGADriver.Fill(0, 0, (uint)Mode.Columns, (uint)Mode.Rows, (uint)aColor.ToArgb());
        }

        public Color GetPixel(int aX, int aY)
        {
            var xColorARGB = _CanvasSVGADriver.GetPixel((uint)aX, (uint)aY);
            return Color.FromArgb((int)xColorARGB);
        }

        public void SetCursor(bool aVisible, int aX, int aY)
        {
            _CanvasSVGADriver.SetCursor(aVisible, (uint)aX, (uint)aY);
        }

        public void CreateCursor()
        {
            _CanvasSVGADriver.DefineCursor();
        }

        public void CopyPixel(int aX, int aY, int aNewX, int aNewY, int aWidth = 1, int aHeight = 1)
        {
            _CanvasSVGADriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, (uint)aWidth, (uint)aHeight);
        }

        public void MovePixel(int aX, int aY, int aNewX, int aNewY)
        {
            _CanvasSVGADriver.Copy((uint)aX, (uint)aY, (uint)aNewX, (uint)aNewY, 1, 1);
            _CanvasSVGADriver.SetPixel((uint)aX, (uint)aY, 0);
        }

        public override Color GetPointColor(int aX, int aY)
        {
            return Color.FromArgb((int)_CanvasSVGADriver.GetPixel((uint)aX, (uint)aY));
        }
    }
}
