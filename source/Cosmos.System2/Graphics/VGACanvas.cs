using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using System.Drawing;
using static Cosmos.HAL.VGADriver;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// VGACanvas class. Used to control screen.
    /// </summary>
    public class VGACanvas : Canvas
    {
        /// <summary>
        /// Private boolean whether VGA graphics mode is enabled or not
        /// </summary>
        bool _Enabled;

        /// <summary>
        /// The HAL VGA driver
        /// </summary>
        private readonly VGADriver _VGADriver;

        /// <summary>
        /// VGA graphics mode Canvas constructor - see Canvas.cs
        /// </summary>
        /// <param name="aMode"></param>
        public VGACanvas(Mode aMode) : base()
        {
            Global.mDebugger.Send("Creating VGACanvas with mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(aMode), (VGADriver.ColorDepth)(int)aMode.ColorDepth);
            Mode = aMode;
            Enabled = true;
        }

        /// <summary>
        /// Creates a VGA graphics mode with the default mode
        /// </summary>
        public VGACanvas() : base()
        {
            Enabled = true;
            Mode = DefaultGraphicMode;
            Global.mDebugger.Send("Creating VGACanvas with standard mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(DefaultGraphicMode), (VGADriver.ColorDepth)(int)DefaultGraphicMode.ColorDepth);
        }

        /// <summary>
        /// Name of the backend
        /// </summary>
        public override string Name() => "VGACanvas";

        /// <summary>
        /// Gets or sets the VGA graphics mode
        /// </summary>
        public override Mode Mode { get; set; }

        /// <summary>
        /// Clears the screen of all pixels
        /// </summary>
        /// <param name="aColor"></param>
        public override void Clear(int aColor)
        {
            _VGADriver.DrawFilledRectangle(0, 0, _VGADriver.PixelWidth, _VGADriver.PixelHeight, (uint)aColor);
        }

        /// <summary>
        /// Clears the screen of all pixels
        /// </summary>
        /// <param name="aColor"></param>
        public override void Clear(Color aColor)
        {
            var paletteIndex = _VGADriver.GetClosestColorInPalette(aColor);
            _VGADriver.DrawFilledRectangle(0, 0, _VGADriver.PixelWidth, _VGADriver.PixelHeight, paletteIndex);
        }

        /// <summary>
        /// Disables VGA graphics mode, parent method returns to 80x25 text mode
        /// </summary>
        public override void Disable()
        {
            if (Enabled)
            {
                Enabled = false;
            }
        }

        /// <summary>
        /// Draws an array of colors, specifiying X and Y coords
        /// </summary>
        /// <param name="aColors"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws a circle
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aXCenter"></param>
        /// <param name="aYCenter"></param>
        /// <param name="aRadius"></param>
        public override void DrawCircle(Color aColor, int aXCenter, int aYCenter, int aRadius)
        {
            base.DrawCircle(aColor, aXCenter, aYCenter, aRadius);
        }

        /// <summary>
        /// Draws an ellipse
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aXCenter"></param>
        /// <param name="aYCenter"></param>
        /// <param name="aXRadius"></param>
        /// <param name="aYRadius"></param>
        public override void DrawEllipse(Color aColor, int aXCenter, int aYCenter, int aXRadius, int aYRadius)
        {
            base.DrawEllipse(aColor, aXCenter, aYCenter, aXRadius, aYRadius);
        }

        /// <summary>
        /// Draws a filled circle
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX0"></param>
        /// <param name="aY0"></param>
        /// <param name="aRadius"></param>
        public override void DrawFilledCircle(Color aColor, int aX0, int aY0, int aRadius)
        {
            base.DrawFilledCircle(aColor, aX0, aY0, aRadius);
        }

        /// <summary>
        /// Draws a filled ellipse
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public override void DrawFilledEllipse(Color aColor, int aX, int aY, int aHeight, int aWidth)
        {
            base.DrawFilledEllipse(aColor, aX, aY, aHeight, aWidth);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aXStart"></param>
        /// <param name="aYStart"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawFilledRectangle(Color aColor, int aXStart, int aYStart, int aWidth, int aHeight)
        {
            _VGADriver.DrawFilledRectangle(aXStart, aYStart, aWidth, aHeight, _VGADriver.GetClosestColorInPalette(aColor));
        }

        /// <summary>
        /// Draws a line (in the sand?)
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX1"></param>
        /// <param name="aY1"></param>
        /// <param name="aX2"></param>
        /// <param name="aY2"></param>
        public override void DrawLine(Color aColor, int aX1, int aY1, int aX2, int aY2)
        {
            base.DrawLine(aColor, aX1, aY1, aX2, aY2);
        }

        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public override void DrawPoint(Color aColor, int aX, int aY)
        {
            _VGADriver.SetPixel((uint)aX, (uint)aY, aColor);
        }

        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public void DrawPoint(uint aColor, int aX, int aY)
        {
            _VGADriver.SetPixel((uint)aX, (uint)aY, aColor);
        }

        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public override void DrawPoint(Color aColor, float aX, float aY)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws a polygon
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aPoints"></param>
        public override void DrawPolygon(Color aColor, params Point[] aPoints)
        {
            base.DrawPolygon(aColor, aPoints);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight)
        {
            base.DrawRectangle(aColor, aX, aY, aWidth, aHeight);
        }

        /// <summary>
        /// Draws a square
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aSize"></param>
        public override void DrawSquare(Color aColor, int aX, int aY, int aSize)
        {
            base.DrawSquare(aColor, aX, aY, aSize);
        }

        /// <summary>
        /// Draws a triangle
        /// </summary>
        /// <param name="aColor"></param>
        /// <param name="aV1x"></param>
        /// <param name="aV1y"></param>
        /// <param name="aV2x"></param>
        /// <param name="aV2y"></param>
        /// <param name="aV3x"></param>
        /// <param name="aV3y"></param>
        public override void DrawTriangle(Color aColor, int aV1x, int aV1y, int aV2x, int aV2y, int aV3x, int aV3y)
        {
            base.DrawTriangle(aColor, aV1x, aV1y, aV2x, aV2y, aV3x, aV3y);
        }

        /// <summary>
        /// List of available resolutions
        /// </summary>
        private static readonly List<Mode> _AvailableModes = new List<Mode>
        {
            new Mode(640, 480, ColorDepth.ColorDepth4),
            new Mode(720, 480, ColorDepth.ColorDepth4),
            new Mode(320, 200, ColorDepth.ColorDepth8)
        };

        public override List<Mode> AvailableModes
        {
            get
            {
                return _AvailableModes;
            }
        }

        /// <summary>
        /// Retrieves the RGB value of a specified pixel
        /// </summary>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <returns></returns>
        public override Color GetPointColor(int aX, int aY)
        {
            return Color.FromArgb((int)_VGADriver.GetPixel((uint)aX, (uint)aY));
        }

        /// <summary>
        /// The default graphics mode
        /// </summary>
        public override Mode DefaultGraphicMode => new Mode(640, 480, ColorDepth.ColorDepth4);

        /// <summary>
        /// Boolean value whether VGA is in graphics mode or not
        /// </summary>
        public bool Enabled { get => _Enabled; private set => _Enabled = value; }

        private ScreenSize ModeToScreenSize(Mode aMode)
        {
            if (aMode.Width == 320 && aMode.Height == 200)
            {
                return ScreenSize.Size320x200;
            }
            else if (aMode.Width == 640 && aMode.Height == 480)
            {
                return ScreenSize.Size640x480;
            }
            else if (aMode.Width == 720 && aMode.Height == 480)
            {
                return ScreenSize.Size720x480;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Display()
        {

        }
    }
}
