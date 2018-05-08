using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.HAL;

namespace Cosmos.System.Graphics
{
    class VGACanvas : Canvas
    {
        private VGADriver VGADriver;

        public VGACanvas(Mode mode) : base(mode)
        {

        }

        public VGACanvas() : base()
        {

        }

        public override Mode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Clear(Color color)
        {
            base.Clear(color);
        }

        public override void Disable()
        {
            throw new NotImplementedException();
        }

        public override void DrawArray(Color[] colors, Point point, int width, int height)
        {
            base.DrawArray(colors, point, width, height);
        }

        public override void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            throw new NotImplementedException();
        }

        public override void DrawCircle(Pen pen, int x_center, int y_center, int radius)
        {
            base.DrawCircle(pen, x_center, y_center, radius);
        }

        public override void DrawCircle(Pen pen, Point point, int radius)
        {
            base.DrawCircle(pen, point, radius);
        }

        public override void DrawEllipse(Pen pen, int x_center, int y_center, int x_radius, int y_radius)
        {
            base.DrawEllipse(pen, x_center, y_center, x_radius, y_radius);
        }

        public override void DrawEllipse(Pen pen, Point point, int x_radius, int y_radius)
        {
            base.DrawEllipse(pen, point, x_radius, y_radius);
        }

        public override void DrawFilledCircle(Pen pen, int x0, int y0, int radius)
        {
            base.DrawFilledCircle(pen, x0, y0, radius);
        }

        public override void DrawFilledCircle(Pen pen, Point point, int radius)
        {
            base.DrawFilledCircle(pen, point, radius);
        }

        public override void DrawFilledEllipse(Pen pen, Point point, int height, int width)
        {
            base.DrawFilledEllipse(pen, point, height, width);
        }

        public override void DrawFilledEllipse(Pen pen, int x, int y, int height, int width)
        {
            base.DrawFilledEllipse(pen, x, y, height, width);
        }

        public override void DrawFilledRectangle(Pen pen, Point point, int width, int height)
        {
            base.DrawFilledRectangle(pen, point, width, height);
        }

        public override void DrawFilledRectangle(Pen pen, int x_start, int y_start, int width, int height)
        {
            base.DrawFilledRectangle(pen, x_start, y_start, width, height);
        }

        public override void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            base.DrawLine(pen, x1, y1, x2, y2);
        }

        public override void DrawPoint(Pen pen, int x, int y)
        {
            throw new NotImplementedException();
        }

        public override void DrawPoint(Pen pen, float x, float y)
        {
            throw new NotImplementedException();
        }

        public override void DrawPolygon(Pen pen, params Point[] points)
        {
            base.DrawPolygon(pen, points);
        }

        public override void DrawRectangle(Pen pen, Point point, int width, int height)
        {
            base.DrawRectangle(pen, point, width, height);
        }

        public override void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            base.DrawRectangle(pen, x, y, width, height);
        }

        public override void DrawSquare(Pen pen, Point point, int size)
        {
            base.DrawSquare(pen, point, size);
        }

        public override void DrawSquare(Pen pen, int x, int y, int size)
        {
            base.DrawSquare(pen, x, y, size);
        }

        public override void DrawTriangle(Pen pen, Point point0, Point point1, Point point2)
        {
            base.DrawTriangle(pen, point0, point1, point2);
        }

        public override void DrawTriangle(Pen pen, int v1x, int v1y, int v2x, int v2y, int v3x, int v3y)
        {
            base.DrawTriangle(pen, v1x, v1y, v2x, v2y, v3x, v3y);
        }

        public override List<Mode> getAvailableModes()
        {
            throw new NotImplementedException();
        }

        public override Color GetPointColor(int x, int y)
        {
            throw new NotImplementedException();
        }

        protected override Mode getDefaultGraphicMode()
        {
            throw new NotImplementedException();
        }
    }
}
