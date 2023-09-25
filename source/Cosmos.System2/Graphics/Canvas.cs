//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Represents a drawing surface.
    /// </summary>
    public abstract class Canvas
    {
        /// <summary>
        /// The available graphics modes.
        /// </summary>
        public abstract List<Mode> AvailableModes { get; }

        /// <summary>
        /// The default graphics mode.
        /// </summary>
        public abstract Mode DefaultGraphicsMode { get; }

        /// <summary>
        /// The currently used display mode.
        /// </summary>
        public abstract Mode Mode { get; set; }

        /// <summary>
        /// Clears the canvas with the default color.
        /// </summary>
        public void Clear()
        {
            Clear(Color.Black);
        }

        /// <summary>
        /// Clears the entire canvas with the specified color.
        /// </summary>
        /// <param name="color">The ARGB color to clear the screen with.</param>
        public abstract void Clear(int color);

        /// <summary>
        /// Clears the entire canvas with the specified color.
        /// </summary>
        /// <param name="color">The color to clear the screen with.</param>
        public virtual void Clear(Color color)
        {
            for (int x = 0; x < Mode.Width; x++)
            {
                for (int y = 0; y < Mode.Height; y++)
                {
                    DrawPoint(color, x, y);
                }
            }
        }

        /// <summary>
        /// Disables the canvas.
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Sets the pixel at the given coordinates to the specified <paramref name="color"/>.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public abstract void DrawPoint(Color color, int x, int y);

        /// <summary>
        /// The name of the Canvas implementation.
        /// </summary>
        public abstract string Name();

        /// <summary>
        /// Updates the screen to display the underlying frame-buffer.
        /// Call this method in order to synchronize the screen with the canvas.
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Gets the color of the pixel at the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public abstract Color GetPointColor(int x, int y);

        /// <summary>
        /// Gets the index of the pixel at the given coordinates.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        internal int GetPointOffset(int x, int y)
        {
            int xBytePerPixel = (int)Mode.ColorDepth / 8;
            int stride = (int)Mode.ColorDepth / 8;
            int pitch = (int)Mode.Width * xBytePerPixel;

            return (x * stride) + (y * pitch);
        }

        /// <summary>
        /// Draws an array of pixels to the canvas, starting at the given coordinates,
        /// using the given width.
        /// </summary>
        /// <param name="colors">The pixels to draw.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="width">The width of the drawn bitmap.</param>
        /// <param name="height">This parameter is unused.</param>
        public virtual void DrawArray(Color[] colors, int x, int y, int width, int height)
        {
            for (int X = 0; X < width; X++)
            {
                for (int Y = 0; Y < height; Y++)
                {
                    DrawPoint(colors[Y * width + X], x + X, y + Y);
                }
            }
        }

        /// <summary>
        /// Draws a horizontal line.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="dx">The length of the line.</param>
        /// <param name="x1">The starting point X coordinate.</param>
        /// <param name="y1">The starting point Y coordinate.</param>
        internal void DrawHorizontalLine(Color color, int dx, int x1, int y1)
        {
            int i;

            for (i = 0; i < dx; i++)
            {
                DrawPoint(color, x1 + i, y1);
            }
        }

        /// <summary>
        /// Draw a vertical line.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="dy">The line of the line.</param>
        /// <param name="x1">The starting point X coordinate.</param>
        /// <param name="y1">The starting point Y coordinate.</param>
        internal void DrawVerticalLine(Color color, int dy, int x1, int y1)
        {
            int i;

            for (i = 0; i < dy; i++)
            {
                DrawPoint(color, x1, y1 + i);
            }
        }

        /*
         * To draw a diagonal line we use the fast version of the Bresenham's algorithm.
         * See http://www.brackeen.com/vga/shapes.html#4 for more informations.
         */
        /// <summary>
        /// Draws a diagonal line.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="dx">The line length on the X axis.</param>
        /// <param name="dy">The line length on the Y axis.</param>
        /// <param name="x1">The starting point X coordinate.</param>
        /// <param name="y1">The starting point Y coordinate.</param>
        internal void DrawDiagonalLine(Color color, int dx, int dy, int x1, int y1)
        {
            int i;

            var dxabs = Math.Abs(dx);
            var dyabs = Math.Abs(dy);
            var sdx = Math.Sign(dx);
            var sdy = Math.Sign(dy);
            var x = dyabs >> 1;
            var y = dxabs >> 1;
            var px = x1;
            var py = y1;

            if (dxabs >= dyabs) // the line is more horizontal than vertical
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
            else // the line is more vertical than horizontal
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
        /// Draws a line between the given points.
        /// </summary>
        /// <param name="color">The color to draw the line with.</param>
        /// <param name="x1">The starting point X coordinate.</param>
        /// <param name="y1">The starting point Y coordinate.</param>
        /// <param name="x2">The end point X coordinate.</param>
        /// <param name="y2">The end point Y coordinate.</param>
        public virtual void DrawLine(Color color, int x1, int y1, int x2, int y2)
        {
            // Trim the given line to fit inside the canvas boundaries
            TrimLine(ref x1, ref y1, ref x2, ref y2);

            var dx = x2 - x1; // The horizontal distance of the line
            var dy = y2 - y1; // The vertical distance of the line

            if (dy == 0) // The line is horizontal
            {
                DrawHorizontalLine(color, dx, x1, y1);
                return;
            }

            if (dx == 0) // The line is vertical
            {
                DrawVerticalLine(color, dy, x1, y1);
                return;
            }

            // The line is neither horizontal neither vertical - it's diagonal.
            DrawDiagonalLine(color, dx, dy, x1, y1);
        }

        //https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// <summary>
        /// Draws a circle at the given coordinates with the given radius.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="xCenter">The X center coordinate.</param>
        /// <param name="yCenter">The Y center coordinate.</param>
        /// <param name="radius">The radius of the circle to draw.</param>
        public virtual void DrawCircle(Color color, int xCenter, int yCenter, int radius)
        {
            ThrowIfCoordNotValid(xCenter + radius, yCenter);
            ThrowIfCoordNotValid(xCenter - radius, yCenter);
            ThrowIfCoordNotValid(xCenter, yCenter + radius);
            ThrowIfCoordNotValid(xCenter, yCenter - radius);
            int x = radius;
            int y = 0;
            int e = 0;

            while (x >= y)
            {
                DrawPoint(color, xCenter + x, yCenter + y);
                DrawPoint(color, xCenter + y, yCenter + x);
                DrawPoint(color, xCenter - y, yCenter + x);
                DrawPoint(color, xCenter - x, yCenter + y);
                DrawPoint(color, xCenter - x, yCenter - y);
                DrawPoint(color, xCenter - y, yCenter - x);
                DrawPoint(color, xCenter + y, yCenter - x);
                DrawPoint(color, xCenter + x, yCenter - y);

                y++;
                if (e <= 0)
                {
                    e += (2 * y) + 1;
                }
                if (e > 0)
                {
                    x--;
                    e -= (2 * x) + 1;
                }
            }
        }

        /// <summary>
        /// Draws a filled circle at the given coordinates with the given radius.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x0">The X center coordinate.</param>
        /// <param name="y0">The Y center coordinate.</param>
        /// <param name="radius">The radius of the circle to draw.</param>
        public virtual void DrawFilledCircle(Color color, int x0, int y0, int radius)
        {
            int x = radius;
            int y = 0;
            int xChange = 1 - (radius << 1);
            int yChange = 0;
            int radiusError = 0;

            while (x >= y)
            {
                for (int i = x0 - x; i <= x0 + x; i++)
                {

                    DrawPoint(color, i, y0 + y);
                    DrawPoint(color, i, y0 - y);
                }
                for (int i = x0 - y; i <= x0 + y; i++)
                {
                    DrawPoint(color, i, y0 + x);
                    DrawPoint(color, i, y0 - x);
                }

                y++;
                radiusError += yChange;
                yChange += 2;
                if ((radiusError << 1) + xChange > 0)
                {
                    x--;
                    radiusError += xChange;
                    xChange += 2;
                }
            }
        }

        //http://members.chello.at/~easyfilter/bresenham.html
        /// <summary>
        /// Draws an ellipse.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="xCenter">The X center coordinate.</param>
        /// <param name="yCenter">The Y center coordinate.</param>
        /// <param name="xR">The X radius.</param>
        /// <param name="yR">The Y radius.</param>
        public virtual void DrawEllipse(Color color, int xCenter, int yCenter, int xR, int yR)
        {
            ThrowIfCoordNotValid(xCenter + xR, yCenter);
            ThrowIfCoordNotValid(xCenter - xR, yCenter);
            ThrowIfCoordNotValid(xCenter, yCenter + yR);
            ThrowIfCoordNotValid(xCenter, yCenter - yR);
            int a = 2 * xR;
            int b = 2 * yR;
            int b1 = b & 1;
            int dx = 4 * (1 - a) * b * b;
            int dy = 4 * (b1 + 1) * a * a;
            int err = dx + dy + (b1 * a * a);
            int e2;
            int y = 0;
            int x = xR;
            a *= 8 * a;
            b1 = 8 * b * b;

            while (x >= 0)
            {
                DrawPoint(color, xCenter + x, yCenter + y);
                DrawPoint(color, xCenter - x, yCenter + y);
                DrawPoint(color, xCenter - x, yCenter - y);
                DrawPoint(color, xCenter + x, yCenter - y);
                e2 = 2 * err;
                if (e2 <= dy) { y++; err += dy += a; }
                if (e2 >= dx || 2 * err > dy) { x--; err += dx += b1; }
            }
        }

        /// <summary>
        /// Draws a filled ellipse.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="xCenter">The X center coordinate.</param>
        /// <param name="yCenter">The Y center coordinate.</param>
        /// <param name="xR">The X radius.</param>
        /// <param name="yR">The Y radius.</param>
        public virtual void DrawFilledEllipse(Color color, int xCenter, int yCenter, int yR, int xR)
        {
            for (int y = -yR; y <= yR; y++)
            {
                for (int x = -xR; x <= xR; x++)
                {
                    if ((x * x * yR * yR) + (y * y * xR * xR) <= yR * yR * xR * xR)
                    {
                        DrawPoint(color, xCenter + x, yCenter + y);
                    }
                }
            }
        }

        /// <summary>
		/// Draws an arc.
		/// </summary>
		/// <param name="x">The starting X coordinate.</param>
		/// <param name="y">The ending X coordinate.</param>
		/// <param name="width">The width of the arc.</param>
		/// <param name="height">The height of the arc.</param>
		/// <param name="color">The color of the arc.</param>
		/// <param name="startAngle">The starting angle of the arc, in degrees.</param>
		/// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        public virtual void DrawArc(int x, int y, int width, int height, Color color, int startAngle = 0, int endAngle = 360)
        {
            if (width == 0 || height == 0)
            {
                return;
            }

            for (double angle = startAngle; angle < endAngle; angle += 0.5)
            {
                double angleRadians = Math.PI * angle / 180;
                int IX = (int)(width * Math.Cos(angleRadians));
                int IY = (int)(height * Math.Sin(angleRadians));
                DrawPoint(color, x + IX, y + IY);
            }
        }

        /// <summary>
        /// Draws a polygon.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="points">The vertices of the polygon.</param>
        public virtual void DrawPolygon(Color color, params Point[] points)
        {
            // Using an array of points here is better than using something like a Dictionary of ints.
            if (points.Length < 3)
            {
                throw new ArgumentException("A polygon requires more than 3 points.");
            }

            for (int i = 0; i < points.Length - 1; i++)
            {
                var pointA = points[i];
                var pointB = points[i + 1];
                DrawLine(color, pointA.X, pointA.Y, pointB.X, pointB.Y);
            }

            var firstPoint = points[0];
            var lastPoint = points[^1];
            DrawLine(color, firstPoint.X, firstPoint.Y, lastPoint.X, lastPoint.Y);
        }

        /// <summary>
        /// Draws a square.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="size">The size of the square.</param>
        public virtual void DrawSquare(Color color, int x, int y, int size)
        {
            DrawRectangle(color, x, y, size, size);
        }

        /// <summary>
        /// Draws a rectangle.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public virtual void DrawRectangle(Color color, int x, int y, int width, int height)
        {
            /*
             * we must draw four lines connecting any vertex of our rectangle to do this we first obtain the position of these
             * vertex (we call these vertexes A, B, C, D as for geometric convention)
             */

            /* The check of the validity of x and y are done in DrawLine() */

            /* The vertex A is where x,y are */
            int xa = x;
            int ya = y;

            /* The vertex B has the same y coordinate of A but x is moved of width pixels */
            int xb = x + width;
            int yb = y;

            /* The vertex C has the same x coordiate of A but this time is y that is moved of height pixels */
            int xc = x;
            int yc = y + height;

            /* The Vertex D has x moved of width pixels and y moved of height pixels */
            int xd = x + width;
            int yd = y + height;

            /* Draw a line betwen A and B */
            DrawLine(color, xa, ya, xb, yb);

            /* Draw a line between A and C */
            DrawLine(color, xa, ya, xc, yc);

            /* Draw a line between B and D */
            DrawLine(color, xb, yb, xd, yd);

            /* Draw a line between C and D */
            DrawLine(color, xc, yc, xd, yd);
        }

        /// <summary>
        /// Draws a filled rectangle.
        /// </summary>
        /// <param name="color">The color to draw the rectangle with.</param>
        /// <param name="xStart">The starting point X coordinate.</param>
        /// <param name="yStart">The starting point Y coordinate.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        public virtual void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
        {
            if (height == -1)
            {
                height = width;
            }

            for (int y = yStart; y < yStart + height; y++)
            {
                DrawLine(color, xStart, y, xStart + width - 1, y);
            }
        }

        /// <summary>
        /// Draws a triangle.
        /// </summary>
        /// <param name="color">The color to draw with.</param>
        /// <param name="v1x">The first points X coordinate.</param>
        /// <param name="v1y">The first points Y coordinate.</param>
        /// <param name="v2x">The second points X coordinate.</param>
        /// <param name="v2y">The second points Y coordinate.</param>
        /// <param name="v3x">The third points X coordinate.</param>
        /// <param name="v3y">The third points Y coordinate.</param>
        public virtual void DrawTriangle(Color color, int v1x, int v1y, int v2x, int v2y, int v3x, int v3y)
        {
            DrawLine(color, v1x, v1y, v2x, v2y);
            DrawLine(color, v1x, v1y, v3x, v3y);
            DrawLine(color, v2x, v2y, v3x, v3y);
        }

        /// <summary>
        /// Draws the given image at the specified coordinates.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="x">The origin X coordinate.</param>
        /// <param name="y">The origin Y coordinate.</param>
        public virtual void DrawImage(Image image, int x, int y)
        {
            Color color;

            for (int xi = 0; xi < image.Width; xi++)
            {
                for (int yi = 0; yi < image.Height; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }

        static int[] ScaleImage(Image image, int newWidth, int newHeight)
        {
            int[] pixels = image.RawData;
            int w1 = (int)image.Width;
            int h1 = (int)image.Height;
            int[] temp = new int[newWidth * newHeight];
            int xRatio = (int)((w1 << 16) / newWidth) + 1;
            int yRatio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;

            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = (j * xRatio) >> 16;
                    y2 = (i * yRatio) >> 16;
                    temp[(i * newWidth) + j] = pixels[(y2 * w1) + x2];
                }
            }

            return temp;
        }

        /// <summary>
        /// Draws a bitmap, applying scaling to the given image.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <param name="w">The desired width to scale the image to before drawing.</param>
        /// <param name="h">The desired height to scale the image to before drawing</param>
        public virtual void DrawImage(Image image, int x, int y, int w, int h)
        {
            Color color;

            int[] pixels = ScaleImage(image, w, h);
            for (int xi = 0; xi < w; xi++)
            {
                for (int yi = 0; yi < h; yi++)
                {
                    color = Color.FromArgb(pixels[xi + (yi * w)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }

        /// <summary>
        /// Draws an image with alpha blending.
        /// </summary>
        /// <param name="image">The image to draw.</param>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        public void DrawImageAlpha(Image image, int x, int y)
        {
            Color color;

            for (int xi = 0; xi < image.Width; xi++)
            {
                for (int yi = 0; yi < image.Height; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }

        /// <summary>
        /// Draws a string using the given bitmap font.
        /// </summary>
        /// <param name="str">The string to draw.</param>
        /// <param name="font">The bitmap font to use.</param>
        /// <param name="color">The color to write the string with.</param>
        /// <param name="x">The origin X coordinate.</param>
        /// <param name="y">The origin Y coordinate.</param>
        public virtual void DrawString(string str, Font font, Color color, int x, int y)
        {
            var len = str.Length;
            var width = font.Width;

            for (int i = 0; i < len; i++)
            {
                DrawChar(str[i], font, color, x, y);
                x += width;
            }
        }

        /// <summary>
        /// Draws a single character using the given bitmap font.
        /// </summary>
        /// <param name="c">The character to draw.</param>
        /// <inheritdoc cref="DrawString(string, Font, Color, int, int)"/>
        public virtual void DrawChar(char c, Font font, Color color, int x, int y)
        {
            var height = font.Height;
            var width = font.Width;
            var data = font.Data;
            int p = height * (byte)c;

            for (int cy = 0; cy < height; cy++)
            {
                for (byte cx = 0; cx < width; cx++)
                {
                    if (font.ConvertByteToBitAddress(data[p + cy], cx + 1))
                    {
                        DrawPoint(color, (ushort)(x + cx), (ushort)(y + cy));
                    }
                }
            }
        }

        /// <summary>
        /// Checks if the given video mode is valid.
        /// </summary>
        /// <param name="mode">The target video mode.</param>
        protected bool CheckIfModeIsValid(Mode mode)
        {
            foreach (var elem in AvailableModes)
            {
                if (elem == mode)
                {
                    return true; // All OK mode does exists in availableModes
                }
            }

            return false;
        }

        /// <summary>
        /// Validates the given video mode, and throws an exception if the
        /// given value is invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the mode is not supported.</exception>
        protected void ThrowIfModeIsNotValid(Mode mode)
        {
            if (CheckIfModeIsValid(mode))
            {
                return;
            }

            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by this driver");
        }

        /// <summary>
        /// Validates that the given coordinates are in-range of the canvas, and
        /// throws an exception if the coordinates are out-of-bounds.
        /// </summary>
        /// <param name="x">The X coordinate.</param>
        /// <param name="y">The Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the coordinates are invalid.</exception>
        protected void ThrowIfCoordNotValid(int x, int y)
        {
            if (x < 0 || x >= Mode.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"X coordinate ({x}) is not between 0 and {Mode.Width}");
            }

            if (y < 0 || y >= Mode.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"Y coordinate ({y}) is not between 0 and {Mode.Height}");
            }
        }

        protected void TrimLine(ref int x1, ref int y1, ref int x2, ref int y2)
        {
            // in case of vertical lines, no need to perform complex operations
            if (x1 == x2)
            {
                x1 = Math.Min((int)Mode.Width - 1, Math.Max(0, x1));
                x2 = x1;
                y1 = Math.Min((int)Mode.Height - 1, Math.Max(0, y1));
                y2 = Math.Min((int)Mode.Height - 1, Math.Max(0, y2));

                return;
            }

            // never attempt to remove this part,
            // if we didn't calculate our new values as floats, we would end up with inaccurate output
            float x1Out = x1, y1Out = y1;
            float x2Out = x2, y2Out = y2;

            // calculate the line slope, and the entercepted part of the y axis
            float m = (y2Out - y1Out) / (x2Out - x1Out);
            float c = y1Out - (m * x1Out);

            // handle x1
            if (x1Out < 0)
            {
                x1Out = 0;
                y1Out = c;
            }
            else if (x1Out >= Mode.Width)
            {
                x1Out = Mode.Width - 1;
                y1Out = ((Mode.Width - 1) * m) + c;
            }

            // handle x2
            if (x2Out < 0)
            {
                x2Out = 0;
                y2Out = c;
            }
            else if (x2Out >= Mode.Width)
            {
                x2Out = Mode.Width - 1;
                y2Out = ((Mode.Width - 1) * m) + c;
            }

            // handle y1
            if (y1Out < 0)
            {
                x1Out = -c / m;
                y1Out = 0;
            }
            else if (y1Out >= Mode.Height)
            {
                x1Out = (Mode.Height - 1 - c) / m;
                y1Out = Mode.Height - 1;
            }

            // handle y2
            if (y2Out < 0)
            {
                x2Out = -c / m;
                y2Out = 0;
            }
            else if (y2Out >= Mode.Height)
            {
                x2Out = (Mode.Height - 1 - c) / m;
                y2Out = Mode.Height - 1;
            }

            // final check, to avoid lines that are totally outside bounds
            if (x1Out < 0 || x1Out >= Mode.Width || y1Out < 0 || y1Out >= Mode.Height)
            {
                x1Out = 0; x2Out = 0;
                y1Out = 0; y2Out = 0;
            }

            if (x2Out < 0 || x2Out >= Mode.Width || y2Out < 0 || y2Out >= Mode.Height)
            {
                x1Out = 0; x2Out = 0;
                y1Out = 0; y2Out = 0;
            }

            // replace inputs with new values
            x1 = (int)x1Out; y1 = (int)y1Out;
            x2 = (int)x2Out; y2 = (int)y2Out;
        }

        /// <summary>
        /// Blends between color <paramref name="from"/> and <paramref name="to"/>,
        /// using the given <paramref name="alpha"/> value.
        /// </summary>
        /// <param name="to">The background color.</param>
        /// <param name="from">The foreground color.</param>
        /// <param name="alpha">The alpha value.</param>
        public static Color AlphaBlend(Color to, Color from, byte alpha)
        {
            byte R = (byte)(((to.R * alpha) + (from.R * (255 - alpha))) >> 8);
            byte G = (byte)(((to.G * alpha) + (from.G * (255 - alpha))) >> 8);
            byte B = (byte)(((to.B * alpha) + (from.B * (255 - alpha))) >> 8);
            return Color.FromArgb(R, G, B);
        }
    }
}
