using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{
    public abstract class Canvas
    {
        /// <summary>
        /// List of available graphics modes
        /// </summary>
        public abstract List<Mode> AvailableModes { get; }

        /// <summary>
        /// Default graphics mode
        /// </summary>
        public abstract Mode DefaultGraphicMode { get; }

        /// <summary>
        /// Active graphics mode
        /// </summary>
        public abstract Mode Mode { get; set; }

        /// <summary>
        /// Fill the screen black
        /// </summary>
        public void Clear() { Clear(Color.Black); }

        /// <summary>
        /// Fill the screen with specified color
        /// </summary>
        /// <param name="color"></param>
        public virtual void Clear(Color color)
        {
            for (int i = 0; i < Mode.Width * Mode.Height; i++) { DrawPoint(i % Mode.Width, i / Mode.Width, color); }
        }

        /// <summary>
        /// Draw point at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public abstract void DrawPoint(int x, int y, Color color);

        /// <summary>
        /// Draw point at specified location using floating point
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public void DrawPoint(float x, float y, Color color) { DrawPoint((int)x, (int)y, color); }

        /// <summary>
        /// Draw point at specified location
        /// </summary>
        /// <param name="point"></param>
        /// <param name="color"></param>
        public void DrawPoint(Point point, Color color) { DrawPoint(point.X, point.Y, color); }

        /// <summary>
        /// Draw a filled rectangle with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public virtual void DrawFilledRectangle(int x, int y, int width, int height, Color color) { }

        /// <summary>
        /// Draw a filled rectangle with floating point dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void DrawFilledRectangle(float x, float y, float width, float height, Color color) { DrawFilledRectangle((int)x, (int)y, (int)width, (int)height, color); }

        /// <summary>
        /// Draw a filled rectangle with dimensions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public void DrawFilledRectangle(Point pos, Point size, Color color) { DrawFilledRectangle(pos.X, pos.Y, size.X, size.Y, color); }

        /// <summary>
        /// Draw a filled rectangle with dimensions
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="color"></param>
        public void DrawFilledRectangle(Rectangle bounds, Color color) { DrawFilledRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, color); }

        /// <summary>
        /// Draw a filled square with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public void DrawFilledSquare(int x, int y, int size, Color color) { DrawFilledRectangle(x, y, size, size, color); }

        /// <summary>
        /// Draw a filled square with floating point dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public void DrawFilledSquare(float x, float y, float size, Color color) { DrawFilledRectangle((int)x, (int)y, (int)size, (int)size, color); }

        /// <summary>
        /// Draw a filled square with dimensions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        public void DrawFilledSquare(Point pos, int size, Color color) { DrawFilledRectangle(pos.X, pos.Y, size, size, color); }

        private int cirX, cirY, cirE, cirXChg, cirYChg;
        /// <summary>
        /// Draw a filled circle with dimensions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawFilledCircle(Point pos, int radius, Color color) { DrawFilledCircle(pos.X, pos.Y, radius, color); }

        /// <summary>
        /// Draw a filled circle with dimensions
        /// </summary>
        /// <param name="xx"></param>
        /// <param name="yy"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawFilledCircle(int xx, int yy, int radius, Color color)
        {
            cirX = radius;
            cirY = 0;
            cirXChg = 1 - (radius << 1);
            cirYChg = 0;
            cirE = 0;

            while (cirX >= cirY)
            {
                for (int i = xx - cirX; i <= xx + cirX; i++)
                {
                    DrawPoint(i, yy + cirY, color);
                    DrawPoint(i, yy - cirY, color);
                }
                for (int i = xx - cirY; i <= xx + cirY; i++)
                {
                    DrawPoint(i, yy + cirX, color);
                    DrawPoint(i, yy - cirX, color);
                }

                cirY++;
                cirE += cirYChg;
                cirYChg += 2;
                if (((cirE << 1) + cirXChg) > 0)
                {
                    cirX--;
                    cirE += cirXChg;
                    cirXChg += 2;
                }
            }
        }

        /// <summary>
        /// Draw a filled ellipse with dimensions
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawFilledEllipse(Point center, Point radius, Color color) { DrawFilledEllipse(center.X, center.Y, radius.X, radius.Y, color); }

        /// <summary>
        /// Draw a filled ellipse with dimensions
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <param name="color"></param>
        public void DrawFilledEllipse(float centerX, float centerY, float radiusX, float radiusY, Color color) { DrawFilledEllipse((int)centerX, (int)centerY, (int)radiusX, (int)radiusY, color); }

        /// <summary>
        /// Draw a filled ellipse with dimensions
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <param name="color"></param>
        public void DrawFilledEllipse(int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            for (int y = -radiusY; y <= radiusY; y++)
            {
                for (int x = -radiusX; x <= radiusX; x++)
                {
                    if (x * x * radiusY * radiusY + y * y * radiusX * radiusX <= radiusY * radiusY * radiusX * radiusX)
                    {
                        DrawPoint(centerX + x, centerY + y, color);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a line from point to point
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="color"></param>
        public void DrawLine(Point start, Point end, Color color) { DrawLine(start.X, start.Y, end.X, end.Y, color); }

        /// <summary>
        /// Draw a line from point to point
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="color"></param>
        public void DrawLine(float startX, float startY, float endX, float endY, Color color) { DrawLine((int)startX, (int)startY, (int)endX, (int)endY, color); }

        /// <summary>
        /// Draw a line from point to point
        /// </summary>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="endX"></param>
        /// <param name="endY"></param>
        /// <param name="color"></param>
        public void DrawLine(int startX, int startY, int endX, int endY, Color color)
        {
            int x0 = startX,  y0 = startY, x1 = endX, y1 = endY;

            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawPoint(x0, y0, color);

                if ((x0 == x1) && (y0 == y1)) { break; }
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }

        /// <summary>
        /// Draw a polygon - must have at least 3 points
        /// </summary>
        /// <param name="points"></param>
        /// <param name="color"></param>
        public void DrawPolygon(Point[] points, Color color)
        {
            if (points.Length < 3) { return; }
            for (int i = 0; i < points.Length - 1; i++) { DrawLine(points[i], points[i + 1], color); }
            DrawLine(points[0], points[points.Length - 1], color);
        }

        /// <summary>
        /// Draw a triangle with specified points
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="color"></param>
        public void DrawTriangle(Point point1, Point point2, Point point3, Color color) { DrawTriangle(point1.X, point1.Y, point2.X, point2.Y, point3.X, point3.Y, color); }

        /// <summary>
        /// Draw a triangle with specified points
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="x3"></param>
        /// <param name="y3"></param>
        /// <param name="color"></param>
        public void DrawTriangle(int x1, int y1, int x2, int y2, int x3, int y3, Color color)
        {
            DrawLine(x1, y1, x2, y2, color);
            DrawLine(x1, y1, x3, y3, color);
            DrawLine(x2, y2, x3, y3, color);
        }

        /// <summary>
        /// Draw outline of rectangle with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawRectangle(float x, float y, float width, float height, float thickness, Color color) { DrawRectangle((int)x, (int)y, (int)width, (int)height, (int)thickness, color); }

        /// <summary>
        /// Draw outline of rectangle with dimensions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawRectangle(Point pos, Point size, int thickness, Color color) { DrawRectangle(pos.X, pos.Y, size.X, size.Y, thickness, color); }

        /// <summary>
        /// Draw outline of rectangle with dimensions
        /// </summary>
        /// <param name="bounds"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawRectangle(Rectangle bounds, int thickness, Color color) { DrawRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, thickness, color); }

        /// <summary>
        /// Draw outline of rectangle with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawRectangle(int x, int y, int width, int height, int thickness, Color color)
        {
            // horizontal
            DrawFilledRectangle(x, y, width, thickness, color);
            DrawFilledRectangle(x, y + height - thickness, width, thickness, color);
            // vertical
            DrawFilledRectangle(x, y + thickness, thickness, height - (thickness * 2), color);
            DrawFilledRectangle(x + width - thickness, y + thickness, thickness, height - (thickness * 2), color);
        }

        /// <summary>
        /// Draw outline of square with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawSquare(int x, int y, int size, int thickness, Color color) { DrawRectangle(x, y, size, size, thickness, color); }

        /// <summary>
        /// Draw outline of square with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawSquare(float x, float y, float size, float thickness, Color color) { DrawRectangle((int)x, (int)y, (int)size, (int)size, (int)thickness, color); }

        /// <summary>
        /// Draw outline of square with dimensions
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public void DrawSquare(Point pos, Point size, int thickness, Color color) { DrawRectangle(pos.X, pos.Y, size.X, size.Y, thickness, color); }

        /// <summary>
        /// Draw outline of square with dimensions
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        public void DrawSquare(Rectangle bounds, int thickness, Color color) { DrawRectangle(bounds.X, bounds.Y, bounds.Width, bounds.Height, thickness, color); }

        /// <summary>
        /// Draw outline of circle with dimensions
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawCircle(Point center, int radius, Color color) { DrawCircle(center.X, center.Y, radius, color); }

        /// <summary>
        /// Draw outline of circle with dimensions
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawCircle(int centerX, int centerY, int radius, Color color)
        {
            cirX = radius;
            cirY = 0;
            cirE = 0;

            while (cirX >= cirY)
            {
                DrawPoint(centerX + cirX, centerY + cirY, color);
                DrawPoint(centerX + cirY, centerY + cirX, color);
                DrawPoint(centerX - cirY, centerY + cirX, color);
                DrawPoint(centerX - cirX, centerY + cirY, color);
                DrawPoint(centerX - cirX, centerY - cirY, color);
                DrawPoint(centerX - cirY, centerY - cirX, color);
                DrawPoint(centerX + cirY, centerY - cirX, color);
                DrawPoint(centerX + cirX, centerY - cirY, color);

                cirY++;
                if (cirE <= 0)
                {
                    cirE += 2 * cirY + 1;
                }
                if (cirE > 0)
                {
                    cirX--;
                    cirE -= 2 * cirX + 1;
                }
            }
        }

        int ellA, ellB, ellB1, ellDx, ellDy, ellErr, ellE2, ellX, ellY;
        /// <summary>
        /// Draw outline of ellipse with dimensions
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public void DrawEllipse(Point center, Point radius, Color color) { DrawEllipse(center.X, center.Y, radius.X, radius.Y, color); }

        /// <summary>
        /// Draw outline of ellipse with dimensions
        /// </summary>
        /// <param name="centerX"></param>
        /// <param name="centerY"></param>
        /// <param name="radiusX"></param>
        /// <param name="radiusY"></param>
        /// <param name="color"></param>
        public void DrawEllipse(int centerX, int centerY, int radiusX, int radiusY, Color color)
        {
            ellA = 2 * radiusX;
            ellB = 2 * radiusY;
            ellB1 = ellB & 1;
            ellDx = 4 * (1 - ellA) * ellB * ellB;
            ellDy = 4 * (ellB1 + 1) * ellA * ellA;
            ellErr = ellDx + ellDy + ellB1 * ellA * ellA;
            ellE2 = 0;
            ellX = 0;
            ellY = radiusX;
            ellA *= 8 * ellA;
            ellB1 = 8 * ellB * ellB;

            while (ellX >= 0)
            {
                DrawPoint(centerX + ellX, centerY + ellY, color);
                DrawPoint(centerX - ellX, centerY + ellY, color);
                DrawPoint(centerX - ellX, centerY - ellY, color);
                DrawPoint(centerX + ellX, centerY - ellY, color);
                ellE2 = 2 * ellErr;
                if (ellE2 <= ellDy) { ellX++; ellErr += ellDy += ellA; }
                if (ellE2 >= ellDx || 2 * ellErr > ellDy) { ellX--; ellErr += ellDx += ellB1; }
            }
        }

        /// <summary>
        /// Draw an image at specified location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="image"></param>
        public void DrawImage(Point pos, Image image) { DrawImage(pos.X, pos.Y, image); }

        /// <summary>
        /// Draw an image at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="image"></param>
        public void DrawImage(int x, int y, Image image)
        {
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    DrawPoint(x + i, y + j, Color.FromArgb((int)image.RawData[i + (y * image.Width)]));
                }
            }
        }

        /// <summary>
        /// Draw an character at specified location with no background
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public void DrawChar(Point pos, char c, Color color, Font font) { DrawChar(pos.X, pos.Y, c, color, font); }

        /// <summary>
        /// Draw an character at specified location with no background
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public void DrawChar(int x, int y, char c, Color color, Font font)
        {
            int p = font.Height * (byte)c;
            for (int cy = 0; cy < font.Height; cy++)
            {
                for (byte cx = 0; cx < font.Width; cx++)
                {
                    if (font.ConvertByteToBitAddress(font.Data[p + cy], cx + 1))
                    {
                        DrawPoint(x + (font.Width - cx), y + cy, color);
                    }
                }
            }
        }

        /// <summary>
        /// Draw an character at specified location with background color
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="backColor"></param>
        /// <param name="font"></param>
        public void DrawChar(Point pos, char c, Color color, Color backColor, Font font) { DrawChar(pos.X, pos.Y, c, color, backColor, font); }

        /// <summary>
        /// Draw an character at specified location with background color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="c"></param>
        /// <param name="color"></param>
        /// <param name="backColor"></param>
        /// <param name="font"></param>
        public void DrawChar(int x, int y, char c, Color color, Color backColor, Font font)
        {
            int p = font.Height * (byte)c;
            for (int cy = 0; cy < font.Height; cy++)
            {
                for (byte cx = 0; cx < font.Width; cx++)
                {
                    if (font.ConvertByteToBitAddress(font.Data[p + cy], cx + 1))
                    { DrawPoint(x + (font.Width - cx), y + cy, color); } else { DrawPoint(x + (font.Width - cx), y + cy, backColor); } 
                }
            }
        }

        private int strX, strY;
        /// <summary>
        /// Draw string at specified location with no background
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public void DrawString(Point pos, string text, Color color, Font font) { DrawString(pos.X, pos.Y, text, color, font); }

        /// <summary>
        /// Draw string at specified location with no background
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="font"></param>
        public void DrawString(int x, int y, string text, Color color, Font font)
        {
            strX = x; strY = y;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { strX = x; strY += font.Height; }
                else
                {
                    DrawChar(strX, strY, text[i], color, font);
                    strX += font.Width;
                }
            }
        }

        /// <summary>
        /// Draw string at specified location with background color
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="backColor"></param>
        /// <param name="font"></param>
        public void DrawString(Point pos, string text, Color color, Color backColor, Font font) { DrawString(pos.X, pos.Y, text, color, backColor, font); }

        /// <summary>
        /// Draw string at specified location with background color
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="text"></param>
        /// <param name="color"></param>
        /// <param name="backColor"></param>
        /// <param name="font"></param>
        public void DrawString(int x, int y, string text, Color color, Color backColor, Font font)
        {
            strX = x; strY = y;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n') { strX = x; strY += font.Height; }
                else
                {
                    DrawChar(strX, strY, text[i], color, backColor, font);
                    strX += font.Width;
                }
            }
        }

        private int arrX, arrY;
        /// <summary>
        /// Draw array of colors at specified location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="colors"></param>
        public void DrawArray(Point pos, Point size, Color[] colors) { DrawArray(pos.X, pos.Y, size.X, size.Y, colors); }

        /// <summary>
        /// Draw array of colors at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colors"></param>
        public void DrawArray(int x, int y, int width, int height, Color[] colors)
        {
            for (int i = 0; i < width * height; i++)
            {
                arrX = x + (i % width);
                arrY = y + (i / width);
                DrawPoint(arrX, arrY, colors[i]);
            }
        }

        /// <summary>
        ///  Draw array of packed colors at specified location
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="size"></param>
        /// <param name="colors"></param>
        public void DrawArray(Point pos, Point size, uint[] colors) { DrawArray(pos.X, pos.Y, size.X, size.Y, colors); }

        /// <summary>
        /// Draw array of packed colors at specified location
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colors"></param>
        public void DrawArray(int x, int y, int width, int height, uint[] colors)
        {
            for (int i = 0; i < width * height; i++)
            {
                arrX = x + (i % width);
                arrY = y + (i / width);
                DrawPoint(arrX, arrY, Color.FromArgb((int)colors[i]));
            }
        }

        /// <summary>
        /// Get color of point on canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public abstract Color GetPointColor(int x, int y);

        /// <summary>
        /// Get color of point on canvas
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Color GetPointColor(float x, float y) { return GetPointColor((int)x, (int)y); }

        /// <summary>
        /// Get color of point on canvas
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public Color GetPointColor(Point pos) { return GetPointColor(pos.X, pos.Y); }

        // check if mode is valid
        protected bool CheckIfModeIsValid(Mode mode)
        {
            Global.mDebugger.SendInternal($"CheckIfModeIsValid");

            for (int i = 0; i < AvailableModes.Count; i++)
            {
                if (mode.Width == AvailableModes[i].Width && mode.Height == AvailableModes[i].Height && mode.Depth == AvailableModes[i].Depth)
                {
                    Global.mDebugger.SendInternal($"Mode {mode} found");
                    return true;
                }
            }

            Global.mDebugger.SendInternal($"Mode {mode} found");
            return false;
        }

        /// <summary>
        /// Disable the canvas
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Display the canvas
        /// </summary>
        public abstract void Display();

        // blend alpha with color
        public Color AlphaBlend(Color to, Color from, byte alpha)
        {
            byte R = (byte)((to.R * alpha + from.R * (255 - alpha)) >> 8);
            byte G = (byte)((to.G * alpha + from.G * (255 - alpha)) >> 8);
            byte B = (byte)((to.B * alpha + from.B * (255 - alpha)) >> 8);
            return Color.FromArgb(R, G, B);
        }

        // scale image
        private uint[] scaleImage(Image image, int newWidth, int newHeight)
        {
            uint[] pixels = image.RawData;
            int w1 = (int)image.Width;
            int h1 = (int)image.Height;
            uint[] temp = new uint[newWidth * newHeight];
            int x_ratio = (int)((w1 << 16) / newWidth) + 1;
            int y_ratio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * newWidth) + j] = pixels[(y2 * w1) + x2];
                }
            }
            return temp;
        }

    }
}
