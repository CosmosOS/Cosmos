using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;
using System.Drawing;
using static Cosmos.HAL.VGADriver;

namespace Cosmos.System.Graphics
{
    public class VGACanvas : Canvas
    {
        bool _Enabled;
        private readonly VGADriver _VGADriver;

        public VGACanvas(Mode aMode) : base()
        {
            Global.mDebugger.Send("Creating VGACanvas with mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(aMode), (VGADriver.ColorDepth)(int)aMode.ColorDepth);
            Mode = aMode;
            Enabled = true;
        }

        public VGACanvas() : base()
        {
            Enabled = true;
            Mode = DefaultGraphicMode;
            Global.mDebugger.Send("Creating VGACanvas with standard mode");
            _VGADriver = new VGADriver();
            _VGADriver.SetGraphicsMode(ModeToScreenSize(DefaultGraphicMode), (VGADriver.ColorDepth)(int)DefaultGraphicMode.ColorDepth);
        }

        public override Mode Mode { get; set; }

        public override void Clear(Color aColor)
        {
            var paletteIndex = _VGADriver.GetClosestColorInPalette(aColor);
            _VGADriver.DrawFilledRectangle(0,0, _VGADriver.PixelWidth, _VGADriver.PixelHeight, paletteIndex);
        }

        public override void Disable()
        {
            if (Enabled)
            {
                VGAScreen.SetTextMode(TextSize.Size80x25);
                Enabled = false;
            }
        }

        public override void DrawArray(Color[] aColors, Point aPoint, int aWidth, int aHeight)
        {
            base.DrawArray(aColors, aPoint, aWidth, aHeight);
        }

        public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
        {
            throw new NotImplementedException();
        }

        public override void DrawCircle(Pen aPen, int aXCenter, int aYCenter, int aRadius)
        {
            base.DrawCircle(aPen, aXCenter, aYCenter, aRadius);
        }

        public override void DrawCircle(Pen aPen, Point aPoint, int aRadius)
        {
            base.DrawCircle(aPen, aPoint, aRadius);
        }

        public override void DrawEllipse(Pen aPen, int aXCenter, int aYCenter, int aXRadius, int aYRadius)
        {
            base.DrawEllipse(aPen, aXCenter, aYCenter, aXRadius, aYRadius);
        }

        public override void DrawEllipse(Pen aPen, Point aPoint, int aXRadius, int aYRadius)
        {
            base.DrawEllipse(aPen, aPoint, aXRadius, aYRadius);
        }

        public override void DrawFilledCircle(Pen aPen, int aX0, int aY0, int aRadius)
        {
            base.DrawFilledCircle(aPen, aX0, aY0, aRadius);
        }

        public override void DrawFilledCircle(Pen aPen, Point aPoint, int aRadius)
        {
            base.DrawFilledCircle(aPen, aPoint, aRadius);
        }

        public override void DrawFilledEllipse(Pen aPen, Point aPoint, int aHeight, int aWidth)
        {
            base.DrawFilledEllipse(aPen, aPoint, aHeight, aWidth);
        }

        public override void DrawFilledEllipse(Pen aPen, int aX, int aY, int aHeight, int aWidth)
        {
            base.DrawFilledEllipse(aPen, aX, aY, aHeight, aWidth);
        }

        public override void DrawFilledRectangle(Pen aPen, Point aPoint, int aWidth, int aHeight)
        {
            DrawFilledRectangle(aPen, aPoint.X, aPoint.Y, aWidth, aHeight);
        }

        public override void DrawFilledRectangle(Pen aPen, int aXStart, int aYStart, int aWidth, int aHeight)
        {
            _VGADriver.DrawFilledRectangle(aXStart, aYStart, aWidth, aHeight, _VGADriver.GetClosestColorInPalette(aPen.Color));
        }

        public override void DrawLine(Pen aPen, int aX1, int aY1, int aX2, int aY2)
        {
            base.DrawLine(aPen, aX1, aY1, aX2, aY2);
        }

        public override void DrawPoint(Pen aPen, int aX, int aY)
        {
            _VGADriver.SetPixel((uint)aX, (uint)aY, aPen.Color);
        }

        public void DrawPoint(uint aColor, int aX, int aY)
        {
            _VGADriver.SetPixel((uint)aX, (uint)aY, aColor);
        }

        public override void DrawPoint(Pen aPen, float aX, float aY)
        {
            throw new NotImplementedException();
        }

        public override void DrawPolygon(Pen aPen, params Point[] aPoints)
        {
            base.DrawPolygon(aPen, aPoints);
        }

        public override void DrawRectangle(Pen aPen, Point aPoint, int aWidth, int aHeight)
        {
            base.DrawRectangle(aPen, aPoint, aWidth, aHeight);
        }

        public override void DrawRectangle(Pen aPen, int aX, int aY, int aWidth, int aHeight)
        {
            base.DrawRectangle(aPen, aX, aY, aWidth, aHeight);
        }

        public override void DrawSquare(Pen aPen, Point aPoint, int aSize)
        {
            base.DrawSquare(aPen, aPoint, aSize);
        }

        public override void DrawSquare(Pen aPen, int aX, int aY, int aSize)
        {
            base.DrawSquare(aPen, aX, aY, aSize);
        }

        public override void DrawTriangle(Pen aPen, Point aPoint0, Point aPoint1, Point aPoint2)
        {
            base.DrawTriangle(aPen, aPoint0, aPoint1, aPoint2);
        }

        public override void DrawTriangle(Pen aPen, int aV1x, int aV1y, int aV2x, int aV2y, int aV3x, int aV3y)
        {
            base.DrawTriangle(aPen, aV1x, aV1y, aV2x, aV2y, aV3x, aV3y);
        }

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

        public override Color GetPointColor(int aX, int aY)
        {
            return Color.FromArgb((int)(_VGADriver.GetPixel((uint)aX, (uint)aY)));
        }

        public override Mode DefaultGraphicMode => new Mode(640, 480, ColorDepth.ColorDepth4);

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
    }
}
