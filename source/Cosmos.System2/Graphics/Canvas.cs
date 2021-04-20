//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Base class for canvas drawing
    /// </summary>
    public abstract class Canvas
    {
        /// <summary>
        /// List of supported video modes
        /// </summary>
        public abstract List<Mode> AvailableModes { get; }

        /// <summary>
        /// Default video mode
        /// </summary>
        public abstract Mode DefaultGraphicMode { get; }

        /// <summary>
        /// Active video mode
        /// </summary>
        public abstract Mode Mode { get; set; }

        /// <summary>
        /// Toggle the use of debugging - potentially faster when disabled
        /// </summary>
        public bool SendDebug = true;

        // temporary variables
        private int arrayX, arrayY;
        private int lineX1, lineX2, lineY1, lineY2;
        private int strX, strY;

        /// <summary>
        /// Display canvas onto canvas
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Disable canvas
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Check if video mode is supported
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        protected bool CheckIfModeIsValid(Mode mode)
        {
            Global.mDebugger.SendInternal($"CheckIfModeIsValid");

            foreach (var elem in AvailableModes)
            {
                Global.mDebugger.SendInternal($"elem is {elem} mode is {mode}");
                if (mode.Width == elem.Width && mode.Height == elem.Height && mode.Depth == elem.Depth)
                {
                    Global.mDebugger.SendInternal($"Mode {mode} found");
                    return true; // All OK mode does exists in availableModes
                }
            }

            Global.mDebugger.SendInternal($"Mode {mode} found");
            return false;
        }

        /// <summary>
        /// Throw error if video mode is not supported
        /// </summary>
        /// <param name="mode"></param>
        protected void ThrowIfModeIsNotValid(Mode mode)
        {
            if (CheckIfModeIsValid(mode))
            {
                return;
            }

            Global.mDebugger.SendInternal($"Mode {mode} is not found! Raising exception...");

            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by this Driver");
        }

        /// <summary>
        /// Check if coordinate is within canvas space
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        protected bool IsCoordValid(int x, int y) { return x >= 0 && x < Mode.Width && y >= 0 && y < Mode.Height; }

        /// <summary>
        /// Clear the canvas black
        /// </summary>
        public void Clear() { Clear(Color.Black); }

        /// <summary>
        /// Clear the canvas using pen
        /// </summary>
        /// <param name="pen"></param>
        public void Clear(Pen pen) { Clear(pen.Color); }

        /// <summary>
        /// Clear the canvas using color
        /// </summary>
        /// <param name="color"></param>
        public virtual void Clear(Color color)
        {
            if (SendDebug) { Global.mDebugger.SendInternal($"Clearing the canvas with color {color}"); }

            for (int i = 0; i < Mode.Width * Mode.Height; i++) { DrawPoint(color, i % Mode.Width, i / Mode.Width); }
        }

        /// <summary>
        /// Draw point with color to position on canvas
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public abstract void DrawPoint(Color color, int x, int y);

        /// <summary>
        /// Draw point with color to position on canvas
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPoint(Color color, float x, float y) { DrawPoint(color, (int)x, (int)y); }

        /// <summary>
        /// Draw point with color to position on canvas
        /// </summary>
        /// <param name="color"></param>
        /// <param name="point"></param>
        public void DrawPoint(Color color, Point point) { DrawPoint(color, point.X, point.Y); }

        /// <summary>
        /// Draw point with pen to position on canvas
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPoint(Pen pen, float x, float y)
        {
            if (pen.Width == 1) { DrawPoint(pen.Color, (int)x, (int)y); }
            else { DrawCircle(pen.Color, x - (pen.Width / 2), y - (pen.Width / 2), pen.Width); }
        }

        /// <summary>
        /// Draw point with pen to position on canvas
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPoint(Pen pen, int x, int y) { DrawPoint(pen, x, y); }

        /// <summary>
        /// Draw point with pen to position on canvas
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="point"></param>
        public void DrawPoint(Pen pen, Point point) { DrawPoint(pen, point.X, point.Y); }

        /// <summary>
        /// Draw point with packed color to position on canvas
        /// </summary>
        /// <param name="packedColor"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void DrawPoint(uint packedColor, int x, int y) { DrawPoint(Color.FromArgb((int)packedColor), x, y); }

        /// <summary>
        /// Draw point with packed color to position on canvas
        /// </summary>
        /// <param name="packedColor"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawPoint(uint packedColor, float x, float y) { DrawPoint(packedColor, (int)x, (int)y); }

        /// <summary>
        /// Draw point with packed color to position on canvas
        /// </summary>
        /// <param name="packedColor"></param>
        /// <param name="pos"></param>
        public void DrawPoint(uint packedColor, Point pos) { DrawPoint(packedColor, pos.X, pos.Y); }

        /// <summary>
        /// Draw an array of colors to position on canvas - width and height corresponds to array size
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawArray(Color[] colors, Point pos, int width, int height) { DrawArray(colors, pos.X, pos.Y, width, height); }

        /// <summary>
        /// Draw an array of colors to position on canvas - width and height corresponds to array size
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawArray(Color[] colors, Point pos, Point size) { DrawArray(colors, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw an array of colors to position on canvas - width and height corresponds to array size
        /// </summary>
        /// <param name="colors"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            for (int i = 0; i < width * height; i++)
            {
                arrayX = x + (i % width);
                arrayY = y + (i / width);
                DrawPoint(colors[i], arrayX, arrayY);
            }
        }

        /// <summary>
        /// Draw a horizontal line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="len"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawHorizontalLine(Color color, int len, int x, int y) { for (int i = 0; i < len; i++) { DrawPoint(color, x + i, y); } }

        /// <summary>
        /// Draw a horizontal line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="len"></param>
        /// <param name="pos"></param>
        public void DrawHorizontalLine(Color color, int len, Point pos) { DrawHorizontalLine(color, len, pos.X, pos.Y); }

        /// <summary>
        /// Draw a horizontal line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="len"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawHorizontalLine(Pen pen, int len, int x, int y) { DrawHorizontalLine(pen.Color, len, x, y); }

        /// <summary>
        /// Draw a horizontal line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="len"></param>
        /// <param name="pos"></param>
        public void DrawHorizontalLine(Pen pen, int len, Point pos) { DrawHorizontalLine(pen.Color, len, pos.X, pos.Y); }

        /// <summary>
        /// Draw a vertical line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="len"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawVerticalLine(Color color, int len, int x, int y) { for (int i = 0; i < len; i++) { DrawPoint(color, x, y + i); } }

        /// <summary>
        /// Draw a vertical line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="len"></param>
        /// <param name="pos"></param>
        public void DrawVerticalLine(Color color, int len, Point pos) { DrawVerticalLine(color, len, pos.X, pos.Y); }

        /// <summary>
        /// Draw a vertical line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="len"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawVerticalLine(Pen pen, int len, int x, int y) { DrawVerticalLine(pen.Color, len, x, y); }

        /// <summary>
        /// Draw a vertical line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="len"></param>
        /// <param name="pos"></param>
        public void DrawVerticalLine(Pen pen, int len, Point pos) { DrawVerticalLine(pen.Color, len, pos.X, pos.Y); }

        /// <summary>
        /// Draw a diagonal line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawDiagonalLine(Color color, int x1, int y1, int x2, int y2)
        {
            int i, sdx, sdy, dxabs, dyabs, x, y, px, py;

            dxabs = Math.Abs(x1);
            dyabs = Math.Abs(y1);
            sdx = Math.Sign(x1);
            sdy = Math.Sign(y1);
            x = dyabs >> 1;
            y = dxabs >> 1;
            px = x2;
            py = y2;

            if (dxabs >= dyabs) /* the line is more horizontal than vertical */
            {
                for (i = 0; i < dxabs; i++)
                {
                    y += dyabs;
                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }
                    px += sdx;
                    DrawPoint(color, px, py);
                }
            }
            else /* the line is more vertical than horizontal */
            {
                for (i = 0; i < dyabs; i++)
                {
                    x += dxabs;
                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }
                    py += sdy;
                    DrawPoint(color, px, py);
                }
            }
        }

        /// <summary>
        /// Draw a diagonal line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawDiagonalLine(Color color, float x1, float y1, float x2, float y2) { DrawDiagonalLine(color, (int)x1, (int)y1, (int)x2, (int)y2); }

        /// <summary>
        /// Draw a diagonal line with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void DrawDiagonalLine(Color color, Point p1, Point p2) { DrawDiagonalLine(color, p1.X, p1.Y, p2.X, p2.Y); }

        /// <summary>
        /// Draw a diagonal line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawDiagonalLine(Pen pen, int x1, int y1, int x2, int y2) { DrawDiagonalLine(pen.Color, x1, y1, x2, y2); }

        /// <summary>
        /// Draw a diagonal line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawDiagonalLine(Pen pen, float x1, float y1, float x2, float y2) { DrawDiagonalLine(pen.Color, (int)x1, (int)y1, (int)x2, (int)y2); }

        /// <summary>
        /// Draw a diagonal line with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void DrawDiagonalLine(Pen pen, Point p1, Point p2) { DrawDiagonalLine(pen.Color, p1.X, p1.Y, p2.X, p2.Y); }

        /// <summary>
        /// Draw point from point to point with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Color color, int x1, int y1, int x2, int y2)
        {
            // trim the given line to fit inside the canvas boundries
            TrimLine(ref x1, ref y1, ref x2, ref y2);

            int dx, dy;

            dx = x2 - x1;      /* the horizontal distance of the line */
            dy = y2 - y1;      /* the vertical distance of the line */

            if (dy == 0) { DrawHorizontalLine(color, dx, x1, y1); return; }

            if (dx == 0) { DrawVerticalLine(color, dy, x1, y1); return; }

            /* the line is neither horizontal neither vertical, is diagonal then! */
            DrawDiagonalLine(color, dx, dy, x1, y1);
        }

        /// <summary>
        /// Draw point from point to point with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Color color, float x1, float y1, float x2, float y2) { DrawLine(color, (int)x1, (int)y1, (int)x2, (int)y2); }

        /// <summary>
        /// Draw point from point to point with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void DrawLine(Color color, Point p1, Point p2) { DrawLine(color, p1.X, p1.Y, p2.X, p2.Y); }

        /// <summary>
        /// Draw point from point to point with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Pen pen, int x1, int y1, int x2, int y2) { DrawLine(pen.Color, x1, y1, x2, y2); }

        /// <summary>
        /// Draw point from point to point with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2) { DrawLine(pen.Color, (int)x1, (int)y1, (int)x2, (int)y2); }

        /// <summary>
        /// Draw point from point to point with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public void DrawLine(Pen pen, Point p1, Point p2) { DrawLine(pen.Color, p1.X, p1.Y, p2.X, p2.Y); }

        /// <summary>
        /// Draw circle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Color color, int centerX, int centerY, int radius)
        {
            if (!IsCoordValid(centerX + radius, centerY)) { return; }
            if (!IsCoordValid(centerX - radius, centerY)) { return; }
            if (!IsCoordValid(centerX, centerY + radius)) { return; }
            if (!IsCoordValid(centerX, centerY - radius)) { return; }

            int x = radius, y = 0, e = 0;
            while (x >= y)
            {
                DrawPoint(color, centerX + x, centerY + y);
                DrawPoint(color, centerX + y, centerY + x);
                DrawPoint(color, centerX - y, centerY + x);
                DrawPoint(color, centerX - x, centerY + y);
                DrawPoint(color, centerX - x, centerY - y);
                DrawPoint(color, centerX - y, centerY - x);
                DrawPoint(color, centerX + y, centerY - x);
                DrawPoint(color, centerX + x, centerY - y);

                y++;
                if (e <= 0) { e += 2 * y + 1; }
                if (e > 0) { x--; e -= 2 * x + 1; }
            }
        }

        /// <summary>
        /// Draw circle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Color color, Point center, int radius) { DrawCircle(color, center.X, center.Y, radius); }

        /// <summary>
        /// Draw circle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Color color, float centerX, float centerY, float radius) { DrawCircle(color, (int)centerX, (int)centerY, radius); }

        /// <summary>
        /// Draw circle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Color color, Point center, float radius) { DrawCircle(color, center.X, center.Y, (int)radius); }

        /// <summary>
        /// Draw circle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Pen pen, int centerX, int centerY, int radius) { DrawCircle(pen.Color, centerX, centerY, radius); }

        /// <summary>
        /// Draw circle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Pen pen, Point center, int radius) { DrawCircle(pen.Color, center.X, center.Y, radius); }

        /// <summary>
        /// Draw circle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Pen pen, float centerX, float centerY, float radius) { DrawCircle(pen.Color, (int)centerX, (int)centerY, radius); }

        /// <summary>
        /// Draw circle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawCircle(Pen pen, Point center, float radius) { DrawCircle(pen.Color, center.X, center.Y, (int)radius); }

        /// <summary>
        /// Draw filled circle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Color color, int centerX, int centerY, int radius)
        {
            int x = radius;
            int y = 0;
            int xChange = 1 - (radius << 1);
            int yChange = 0;
            int radiusError = 0;

            while (x >= y)
            {
                for (int i = centerX - x; i <= centerX + x; i++)
                {

                    DrawPoint(color, i, centerY + y);
                    DrawPoint(color, i, centerY - y);
                }
                for (int i = centerX - y; i <= centerX + y; i++)
                {
                    DrawPoint(color, i, centerY + x);
                    DrawPoint(color, i, centerY - x);
                }

                y++;
                radiusError += yChange;
                yChange += 2;
                if (((radiusError << 1) + xChange) > 0)
                {
                    x--;
                    radiusError += xChange;
                    xChange += 2;
                }
            }
        }

        /// <summary>
        /// Draw filled circle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Color color, Point center, int radius) { DrawFilledCircle(color, center.X, center.Y, radius); }

        /// <summary>
        /// Draw filled circle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Color color, float centerX, float centerY, float radius) { DrawFilledCircle(color, (int)centerX, (int)centerY, radius); }

        /// <summary>
        /// Draw filled circle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Color color, Point center, float radius) { DrawFilledCircle(color, center.X, center.Y, (int)radius); }

        /// <summary>
        /// Draw filled circle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Pen pen, int centerX, int centerY, int radius) { DrawFilledCircle(pen.Color, centerX, centerY, radius); }

        /// <summary>
        /// Draw filled circle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Pen pen, Point center, int radius) { DrawFilledCircle(pen.Color, center.X, center.Y, radius); }

        /// <summary>
        /// Draw filled circle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Pen pen, float centerX, float centerY, float radius) { DrawFilledCircle(pen.Color, (int)centerX, (int)centerY, radius); }

        /// <summary>
        /// Draw filled circle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawFilledCircle(Pen pen, Point center, float radius) { DrawFilledCircle(pen.Color, center.X, center.Y, (int)radius); }

        /// <summary>
        /// Draw ellipse outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Color color, int centerX, int centerY, int radiusX, int radiusY)
        {
            if (!IsCoordValid(centerX + radiusX, centerY)) { return; }
            if (!IsCoordValid(centerX - radiusX, centerY)) { return; }
            if (!IsCoordValid(centerX, centerY + radiusY)) { return; }
            if (!IsCoordValid(centerX, centerY - radiusY)) { return; }

            int a = 2 * radiusX;
            int b = 2 * radiusY;
            int b1 = b & 1;
            int x1 = 4 * (1 - a) * b * b;
            int dy = 4 * (b1 + 1) * a * a;
            int err = x1 + dy + b1 * a * a;
            int e2;
            int y = 0;
            int x = radiusX;
            a *= 8 * a;
            b1 = 8 * b * b;

            while (x >= 0)
            {
                DrawPoint(color, centerX + x, centerY + y);
                DrawPoint(color, centerX - x, centerY + y);
                DrawPoint(color, centerX - x, centerY - y);
                DrawPoint(color, centerX + x, centerY - y);
                e2 = 2 * err;
                if (e2 <= dy) { y++; err += dy += a; }
                if (e2 >= x1 || 2 * err > dy) { x--; err += x1 += b1; }
            }
        }

        /// <summary>
        /// Draw ellipse outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Color color, float centerX, float centerY, float radiusX, float radiusY) { DrawEllipse(color, (int)centerX, (int)centerY, (int)radiusX, (int)radiusY); }

        /// <summary>
        /// Draw ellipse outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Color color, Point center, float radiusX, float radiusY) { DrawEllipse(color, center.X, center.Y, (int)radiusX, (int)radiusY); }

        /// <summary>
        /// Draw ellipse outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawEllipse(Color color, Point center, Point radius) { DrawEllipse(color, center.X, center.Y, radius.X, radius.Y); }

        /// <summary>
        /// Draw ellipse outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Pen pen, int centerX, int centerY, int radiusX, int radiusY) { DrawEllipse(pen.Color, centerX, centerY, radiusX, radiusY); }

        /// <summary>
        /// Draw ellipse outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Pen pen, float centerX, float centerY, float radiusX, float radiusY) { DrawEllipse(pen.Color, (int)centerX, (int)centerY, (int)radiusX, (int)radiusY); }

        /// <summary>
        /// Draw ellipse outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        public void DrawEllipse(Pen pen, Point center, float radiusX, float radiusY) { DrawEllipse(pen.Color, center.X, center.Y, (int)radiusX, (int)radiusY); }

        /// <summary>
        /// Draw ellipse outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        public void DrawEllipse(Pen pen, Point center, Point radius) { DrawEllipse(pen.Color, center.X, center.Y, radius.X, radius.Y); }

        /// <summary>
        /// Draw filled ellipse with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Color color, int x, int y, int width, int height)
        {
            for (int yy = -height; yy <= height; yy++)
            {
                for (int xx = -width; xx <= width; xx++)
                {
                    if (xx * xx * height * height + yy * yy * width * width <= height * height * width * width)
                    {
                        DrawPoint(color, x + xx, y + yy);
                    }
                }
            }
        }

        /// <summary>
        /// Draw filled ellipse with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Color color, float x, float y, float width, float height) { DrawFilledEllipse(color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled ellipse with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Color color, Point pos, float width, float height) { DrawFilledEllipse(color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled ellipse with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawFilledEllipse(Color color, Point pos, Point size) { DrawFilledEllipse(color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw filled ellipse with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Pen pen, int x, int y, int width, int height) { DrawFilledEllipse(pen.Color, x, y, width, height); }

        /// <summary>
        /// Draw filled ellipse with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Pen pen, float x, float y, float width, float height) { DrawFilledEllipse(pen.Color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled ellipse with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledEllipse(Pen pen, Point pos, float width, float height) { DrawFilledEllipse(pen.Color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled ellipse with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawFilledEllipse(Pen pen, Point pos, Point size) { DrawFilledEllipse(pen.Color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw polygon with specified points and color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="points"></param>
        public void DrawPolygon(Color color, params Point[] points)
        {
            // if debugging enabled throw exception, otherwise return
            if (points.Length < 3) { if (SendDebug) { throw new ArgumentException("A polygon requires more than 3 points."); } else { return; } }

            for (int i = 0; i < points.Length - 1; i++) { DrawLine(color, points[i], points[i + 1]); }
            DrawLine(color, points[0], points[points.Length - 1]);
        }

        /// <summary>
        /// Draw polygon with specified points and pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="points"></param>
        public void DrawPolygon(Pen pen, params Point[] points)
        {
            // if debugging enabled throw exception, otherwise return
            if (points.Length < 3) { if (SendDebug) { throw new ArgumentException("A polygon requires more than 3 points."); } else { return; } }

            for (int i = 0; i < points.Length - 1; i++) { DrawLine(pen.Color, points[i], points[i + 1]); }
            DrawLine(pen.Color, points[0], points[points.Length - 1]);
        }

        /// <summary>
        /// Draw square outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void DrawSquare(Color color, int x, int y, int size) { DrawRectangle(color, x, y, size, size); }

        /// <summary>
        /// Draw square outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void DrawSquare(Color color, float x, float y, float size) { DrawRectangle(color, x, y, size, size); }

        /// <summary>
        /// Draw square outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawSquare(Color color, Point pos, float size) { DrawRectangle(color, pos, size, size); }

        /// <summary>
        /// Draw square outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void DrawSquare(Pen pen, int x, int y, int size) { DrawSquare(pen.Color, x, y, size); }

        /// <summary>
        /// Draw square outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        public void DrawSquare(Pen pen, float x, float y, float size) { DrawSquare(pen.Color, x, y, size); }

        /// <summary>
        /// Draw square outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawSquare(Pen pen, Point pos, float size) { DrawSquare(pen.Color, pos, size); }

        /// <summary>
        /// Draw rectangle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Color color, int x, int y, int width, int height)
        {
            DrawHorizontalLine(color, width, x, y);
            DrawHorizontalLine(color, width, x, y + height - 1);
            DrawVerticalLine(color, height - 2, x, y + 1);
            DrawVerticalLine(color, height - 2, x + width - 1, y + 1);
        }

        /// <summary>
        /// Draw rectangle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Color color, float x, float y, float width, float height) { DrawRectangle(color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw rectangle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Color color, Point pos, float width, float height) { DrawRectangle(color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw rectangle outline with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawRectangle(Color color, Point pos, Point size) { DrawRectangle(color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw rectangle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Pen pen, int x, int y, int width, int height) { DrawRectangle(pen.Color, x, y, width, height); }

        /// <summary>
        /// Draw rectangle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Pen pen, float x, float y, float width, float height) { DrawRectangle(pen.Color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw rectangle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawRectangle(Pen pen, Point pos, float width, float height) { DrawRectangle(pen.Color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw rectangle outline with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawRectangle(Pen pen, Point pos, Point size) { DrawRectangle(pen.Color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void DrawFilledRectangle(Color color, int x, int y, int width, int height)
        {
            for (int i = 0; i < width * height; i++)
            { DrawPoint(color, x + (i % width), y + (i / width)); }
        }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledRectangle(Color color, float x, float y, float width, float height) { DrawFilledRectangle(color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledRectangle(Color color, Point pos, float width, float height) { DrawFilledRectangle(color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled rectangle with color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawFilledRectangle(Color color, Point pos, Point size) { DrawFilledRectangle(color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw filled rectangle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledRectangle(Pen pen, int x, int y, int width, int height) { DrawFilledRectangle(pen.Color, x, y, width, height); }

        /// <summary>
        /// Draw filled rectangle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledRectangle(Pen pen, float x, float y, float width, float height) { DrawFilledRectangle(pen.Color, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled rectangle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawFilledRectangle(Pen pen, Point pos, float width, float height) { DrawFilledRectangle(pen.Color, pos.X, pos.Y, (int)width, (int)height); }

        /// <summary>
        /// Draw filled rectangle with pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawFilledRectangle(Pen pen, Point pos, Point size) { DrawFilledRectangle(pen.Color, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw triangle with specified points and color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        public void DrawTriangle(Color color, int x1, int y1, int x2, int y2, int x3, int y3)
        {
            DrawLine(color, x1, y1, x2, y2);
            DrawLine(color, x1, y1, x3, y3);
            DrawLine(color, x2, y2, x3, y3);
        }

        /// <summary>
        /// Draw triangle with specified points and color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        public void DrawTriangle(Color color, float x1, float y1, float x2, float y2, float x3, float y3) { DrawTriangle(color, (int)x1, (int)y1, (int)x2, (int)y2, (int)x3, (int)y3); }

        /// <summary>
        /// Draw triangle with specified points and color
        /// </summary>
        /// <param name="color"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public void DrawTriangle(Color color, Point p1, Point p2, Point p3) { DrawTriangle(color, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y); }

        /// <summary>
        /// Draw triangle with specified points and pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        public void DrawTriangle(Pen pen, int x1, int y1, int x2, int y2, int x3, int y3) { DrawTriangle(pen.Color, x1, x2, y1, y2, x3, y3); }

        /// <summary>
        /// Draw triangle with specified points and pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        public void DrawTriangle(Pen pen, float x1, float y1, float x2, float y2, float x3, float y3) { DrawTriangle(pen.Color, (int)x1, (int)y1, (int)x2, (int)y2, (int)x3, (int)y3); }

        /// <summary>
        /// Draw triangle with specified points and pen
        /// </summary>
        /// <param name="pen"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        public void DrawTriangle(Pen pen, Point p1, Point p2, Point p3) { DrawTriangle(pen.Color, p1.X, p1.Y, p2.X, p2.Y, p3.X, p3.Y); }

        /// <summary>
        /// Draw image and specified position
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public virtual void DrawImage(Image image, int x, int y)
        {
            for (int i = 0; i < image.Width * image.Height; i++)
            {
                DrawPoint(image.RawData[i], x + (i % image.Width), y + (i / image.Width));
            }
        }

        /// <summary>
        /// Draw image and specified position
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawImage(Image image, float x, float y) { DrawImage(image, (int)x, (int)y); }

        /// <summary>
        /// Draw image and specified position
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pos"></param>
        public void DrawImage(Image image, Point pos) { DrawImage(image, pos.X, pos.Y); }

        /// <summary>
        /// Draw image and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawImage(Image image, int x, int y, int width, int height)
        {
            uint[] raw = Image.ResizeRaw(image, width, height);
            for (int i = 0; i < width * height; i++)
            {
                DrawPoint(raw[i], x + (i % width), y + (i / width));
            }
        }

        /// <summary>
        /// Draw image and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void DrawImage(Image image, float x, float y, float width, float height) { DrawImage(image, (int)x, (int)y, (int)width, (int)height); }

        /// <summary>
        /// Draw image and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        public void DrawImage(Image image, Point pos, Point size) { DrawImage(image, pos.X, pos.Y, size.X, size.Y); }

        /// <summary>
        /// Draw image with alpha and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawImageAlpha(Image image, int x, int y)
        {
            for (int i = 0; i < image.Width * image.Height; i++)
            {
                DrawPoint(image.RawData[i], x + (i % image.Width), y + (i / image.Width));
            }
        }

        /// <summary>
        /// Draw image with alpha and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawImageAlpha(Image image, float x, float y) { DrawImageAlpha(image, (int)x, (int)y); }

        /// <summary>
        /// Draw image with alpha and specified position and size
        /// </summary>
        /// <param name="image"></param>
        /// <param name="pos"></param>
        public void DrawImageAlpha(Image image, Point pos) { DrawImageAlpha(image, pos.X, pos.Y); }

        /// <summary>
        /// Draw string at position with specified font and color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawString(string text, Font font, Color color, int x, int y)
        {
            strX = x; strY = y;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { strX = x; strY += font.Height + font.VerticalSpacing; }
                else
                {
                    DrawChar(text[i], font, color, strX, strY);
                    strX += font.Width + font.HorizontalSpacing;
                }
            }
        }

        /// <summary>
        /// Draw string at position with specified font and color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawString(string text, Font font, Color color, float x, float y) { DrawString(text, font, color, (int)x, (int)y); }

        /// <summary>
        /// Draw string at position with specified font and color
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        public void DrawString(string text, Font font, Color color, Point pos) { DrawString(text, font, color, pos.X, pos.Y); }

        /// <summary>
        /// Draw string at position with specified font and pen
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawString(string text, Font font, Pen pen, int x, int y) { DrawString(text, font, pen.Color, x, y); }

        //Draw string at position with specified font and pen
        public void DrawString(string text, Font font, Pen pen, float x, float y) { DrawString(text, font, pen.Color, (int)x, (int)y); }

        /// <summary>
        /// Draw string at position with specified font and pen
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        public void DrawString(string text, Font font, Pen pen, Point pos) { DrawString(text, font, pen.Color, pos.X, pos.Y); }

        /// <summary>
        /// Draw character at position with specified font and color
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawChar(char c, Font font, Color color, int x, int y)
        {
            int p = font.Height * (byte)c;
            for (int cy = 0; cy < font.Height; cy++)
            {
                for (byte cx = 0; cx < font.Width; cx++)
                {
                    if (font.ConvertByteToBitAddress(font.Data[p + cy], cx + 1))
                    {
                        DrawPoint(color, (x) + (font.Width - cx), (y) + cy);
                    }
                }
            }
        }

        /// <summary>
        /// Draw character at position with specified font and color
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawChar(char c, Font font, Color color, float x, float y) { DrawChar(c, font, color, (int)x, (int)y); }

        /// <summary>
        /// Draw character at position with specified font and color
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="color"></param>
        /// <param name="pos"></param>
        public void DrawChar(char c, Font font, Color color, Point pos) { DrawChar(c, font, color, pos.X, pos.Y); }

        /// <summary>
        /// Draw character at position with specified font and pen
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawChar(char c, Font font, Pen pen, int x, int y) { DrawChar(c, font, pen.Color, x, y); }

        /// <summary>
        /// Draw character at position with specified font and pen
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="pen"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawChar(char c, Font font, Pen pen, float x, float y) { DrawChar(c, font, pen.Color, (int)x, (int)y); }

        /// <summary>
        /// Draw character at position with specified font and pen
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="pen"></param>
        /// <param name="pos"></param>
        public void DrawChar(char c, Font font, Pen pen, Point pos) { DrawChar(c, font, pen.Color, pos.X, pos.Y); }

        /// <summary>
        /// Trim line to fit inside canvas bounds
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        protected void TrimLine(ref int x1, ref int y1, ref int x2, ref int y2)
        {
            // in case of vertical lines, no need to perform complex operations
            if (x1 == x2)
            {
                x1 = Math.Min(Mode.Width - 1, Math.Max(0, x1));
                x2 = x1;
                y1 = Math.Min(Mode.Height - 1, Math.Max(0, y1));
                y2 = Math.Min(Mode.Height - 1, Math.Max(0, y2));

                return;
            }

            // never attempt to remove this part,
            // if we didn't calculate our new values as floats, we would end up with inaccurate output
            float x1_out = x1, y1_out = y1;
            float x2_out = x2, y2_out = y2;

            // calculate the line slope, and the entercepted part of the y axis
            float m = (y2_out - y1_out) / (x2_out - x1_out);
            float c = y1_out - m * x1_out;

            // handle x1
            if (x1_out < 0)
            {
                x1_out = 0;
                y1_out = c;
            }
            else if (x1_out >= Mode.Width)
            {
                x1_out = Mode.Width - 1;
                y1_out = (Mode.Width - 1) * m + c;
            }

            // handle x2
            if (x2_out < 0)
            {
                x2_out = 0;
                y2_out = c;
            }
            else if (x2_out >= Mode.Width)
            {
                x2_out = Mode.Width - 1;
                y2_out = (Mode.Width - 1) * m + c;
            }

            // handle y1
            if (y1_out < 0)
            {
                x1_out = -c / m;
                y1_out = 0;
            }
            else if (y1_out >= Mode.Height)
            {
                x1_out = (Mode.Height - 1 - c) / m;
                y1_out = Mode.Height - 1;
            }

            // handle y2
            if (y2_out < 0)
            {
                x2_out = -c / m;
                y2_out = 0;
            }
            else if (y2_out >= Mode.Height)
            {
                x2_out = (Mode.Height - 1 - c) / m;
                y2_out = Mode.Height - 1;
            }

            // final check, to avoid lines that are totally outside bounds
            if (x1_out < 0 || x1_out >= Mode.Width || y1_out < 0 || y1_out >= Mode.Height)
            {
                x1_out = 0; x2_out = 0;
                y1_out = 0; y2_out = 0;
            }

            if (x2_out < 0 || x2_out >= Mode.Width || y2_out < 0 || y2_out >= Mode.Height)
            {
                x1_out = 0; x2_out = 0;
                y1_out = 0; y2_out = 0;
            }

            // replace inputs with new values
            x1 = (int)x1_out; y1 = (int)y1_out;
            x2 = (int)x2_out; y2 = (int)y2_out;
        }

        /// <summary>
        /// Get color at specified position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract Color GetPointColor(int x, int y);

        /// <summary>
        /// Blend 2 colors together by amount
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Color AlphaBlend(Color to, Color from, byte alpha)
        {
            byte R = (byte)((to.R * alpha + from.R * (255 - alpha)) >> 8);
            byte G = (byte)((to.G * alpha + from.G * (255 - alpha)) >> 8);
            byte B = (byte)((to.B * alpha + from.B * (255 - alpha)) >> 8);
            return Color.FromArgb(R, G, B);
        }

    }
}
