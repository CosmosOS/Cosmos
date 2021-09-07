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
        /// Gets or sets the VGA graphics mode
        /// </summary>
        public override Mode Mode { get; set; }

        /// <summary>
        /// Clears the screen of all pixels
        /// </summary>
        /// <param name="aColor"></param>
        public override void Clear(Color aColor)
        {
            var paletteIndex = _VGADriver.GetClosestColorInPalette(aColor);
            _VGADriver.DrawFilledRectangle(0,0, _VGADriver.PixelWidth, _VGADriver.PixelHeight, paletteIndex);
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
        /// Draws an array of colors
        /// </summary>
        /// <param name="aColors"></param>
        /// <param name="aPoint"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawArray(Color[] aColors, Point aPoint, int aWidth, int aHeight)
        {
            base.DrawArray(aColors, aPoint, aWidth, aHeight);
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
        /// <param name="aPen"></param>
        /// <param name="aXCenter"></param>
        /// <param name="aYCenter"></param>
        /// <param name="aRadius"></param>
        public override void DrawCircle(Pen aPen, int aXCenter, int aYCenter, int aRadius)
        {
            base.DrawCircle(aPen, aXCenter, aYCenter, aRadius);
        }

        /// <summary>
        /// Draws a circle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aRadius"></param>
        public override void DrawCircle(Pen aPen, Point aPoint, int aRadius)
        {
            base.DrawCircle(aPen, aPoint, aRadius);
        }

        /// <summary>
        /// Draws an ellipse
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aXCenter"></param>
        /// <param name="aYCenter"></param>
        /// <param name="aXRadius"></param>
        /// <param name="aYRadius"></param>
        public override void DrawEllipse(Pen aPen, int aXCenter, int aYCenter, int aXRadius, int aYRadius)
        {
            base.DrawEllipse(aPen, aXCenter, aYCenter, aXRadius, aYRadius);
        }

        /// <summary>
        /// Draws an ellipse
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aXRadius"></param>
        /// <param name="aYRadius"></param>
        public override void DrawEllipse(Pen aPen, Point aPoint, int aXRadius, int aYRadius)
        {
            base.DrawEllipse(aPen, aPoint, aXRadius, aYRadius);
        }

        /// <summary>
        /// Draws a filled circle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX0"></param>
        /// <param name="aY0"></param>
        /// <param name="aRadius"></param>
        public override void DrawFilledCircle(Pen aPen, int aX0, int aY0, int aRadius)
        {
            base.DrawFilledCircle(aPen, aX0, aY0, aRadius);
        }

        /// <summary>
        /// Draws a filled circle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aRadius"></param>
        public override void DrawFilledCircle(Pen aPen, Point aPoint, int aRadius)
        {
            base.DrawFilledCircle(aPen, aPoint, aRadius);
        }

        /// <summary>
        /// Draws a filled ellipse
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public override void DrawFilledEllipse(Pen aPen, Point aPoint, int aHeight, int aWidth)
        {
            base.DrawFilledEllipse(aPen, aPoint, aHeight, aWidth);
        }

        /// <summary>
        /// Draws a filled ellipse
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aHeight"></param>
        /// <param name="aWidth"></param>
        public override void DrawFilledEllipse(Pen aPen, int aX, int aY, int aHeight, int aWidth)
        {
            base.DrawFilledEllipse(aPen, aX, aY, aHeight, aWidth);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawFilledRectangle(Pen aPen, Point aPoint, int aWidth, int aHeight)
        {
            DrawFilledRectangle(aPen, aPoint.X, aPoint.Y, aWidth, aHeight);
        }

        /// <summary>
        /// Draws a filled rectangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aXStart"></param>
        /// <param name="aYStart"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawFilledRectangle(Pen aPen, int aXStart, int aYStart, int aWidth, int aHeight)
        {
            _VGADriver.DrawFilledRectangle(aXStart, aYStart, aWidth, aHeight, _VGADriver.GetClosestColorInPalette(aPen.Color));
        }

        /// <summary>
        /// Draws a line (in the sand?)
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX1"></param>
        /// <param name="aY1"></param>
        /// <param name="aX2"></param>
        /// <param name="aY2"></param>
        public override void DrawLine(Pen aPen, int aX1, int aY1, int aX2, int aY2)
        {
            base.DrawLine(aPen, aX1, aY1, aX2, aY2);
        }

        /// <summary>
        /// Draws a point
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public override void DrawPoint(Pen aPen, int aX, int aY)
        {
            _VGADriver.SetPixel((uint)aX, (uint)aY, aPen.Color);
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
        /// <param name="aPen"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        public override void DrawPoint(Pen aPen, float aX, float aY)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Draws a polygon
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoints"></param>
        public override void DrawPolygon(Pen aPen, params Point[] aPoints)
        {
            base.DrawPolygon(aPen, aPoints);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawRectangle(Pen aPen, Point aPoint, int aWidth, int aHeight)
        {
            base.DrawRectangle(aPen, aPoint, aWidth, aHeight);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aWidth"></param>
        /// <param name="aHeight"></param>
        public override void DrawRectangle(Pen aPen, int aX, int aY, int aWidth, int aHeight)
        {
            base.DrawRectangle(aPen, aX, aY, aWidth, aHeight);
        }

        /// <summary>
        /// Draws a square
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint"></param>
        /// <param name="aSize"></param>
        public override void DrawSquare(Pen aPen, Point aPoint, int aSize)
        {
            base.DrawSquare(aPen, aPoint, aSize);
        }

        /// <summary>
        /// Draws a square
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aX"></param>
        /// <param name="aY"></param>
        /// <param name="aSize"></param>
        public override void DrawSquare(Pen aPen, int aX, int aY, int aSize)
        {
            base.DrawSquare(aPen, aX, aY, aSize);
        }

        /// <summary>
        /// Draws a triangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aPoint0"></param>
        /// <param name="aPoint1"></param>
        /// <param name="aPoint2"></param>
        public override void DrawTriangle(Pen aPen, Point aPoint0, Point aPoint1, Point aPoint2)
        {
            base.DrawTriangle(aPen, aPoint0, aPoint1, aPoint2);
        }

        /// <summary>
        /// Draws a triangle
        /// </summary>
        /// <param name="aPen"></param>
        /// <param name="aV1x"></param>
        /// <param name="aV1y"></param>
        /// <param name="aV2x"></param>
        /// <param name="aV2y"></param>
        /// <param name="aV3x"></param>
        /// <param name="aV3y"></param>
        public override void DrawTriangle(Pen aPen, int aV1x, int aV1y, int aV2x, int aV2y, int aV3x, int aV3y)
        {
            base.DrawTriangle(aPen, aV1x, aV1y, aV2x, aV2y, aV3x, aV3y);
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
            return Color.FromArgb((int)(_VGADriver.GetPixel((uint)aX, (uint)aY)));
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
            if (aMode.Columns == 320 && aMode.Rows == 200)
            {
                return ScreenSize.Size320x200;
            }
            else if (aMode.Columns == 640 && aMode.Rows == 480)
            {
                return ScreenSize.Size640x480;
            }
            else if (aMode.Columns == 720 && aMode.Rows == 480)
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
