//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.Collections.Generic;
using Cosmos.System.Graphics.Fonts;

namespace Cosmos.System.Graphics
{
    /// <summary>
    /// Canvas abstract class.
    /// </summary>
    public abstract class Canvas
    {
        /*
         * IReadOnlyList<T> is not working, the Modes inside it become corrupted and then you get Stack Overflow
         */
        //public abstract IReadOnlyList<Mode> AvailableModes { get; }

        /// <summary>
        /// Available graphics modes.
        /// </summary>
        public abstract List<Mode> AvailableModes { get; }

        /// <summary>
        /// Get default graphics mode.
        /// </summary>
        public abstract Mode DefaultGraphicMode { get; }

        /// <summary>
        /// Get and set graphics mode.
        /// </summary>
        public abstract Mode Mode { get; set; }

        /// <summary>
        /// Clear all the Canvas with the Black color.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public void Clear()
        {
            Clear(Color.Black);
        }

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */
        /// <summary>
        /// Clear all the Canvas with the specified color.
        /// </summary>
        /// <param name="color">Color in ARGB.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public abstract void Clear(int color);

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */
        /// <summary>
        /// Clear all the Canvas with the specified color.
        /// </summary>
        /// <param name="color">Color.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public virtual void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");

            for (int x = 0; x < Mode.Width; x++)
            {
                for (int y = 0; y < Mode.Height; y++)
                {
                    DrawPoint(color, x, y);
                }
            }
        }

        /// <summary>
        /// Display graphic mode
        /// </summary>
        public abstract void Disable();

        /// <summary>
        /// Draw point.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public abstract void DrawPoint(Color color, int x, int y);

        /// <summary>
        /// Draw point to the screen.
        /// Not implemented.
        /// </summary>
        /// <param name="color">Color to draw the point with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always (only int coordinats supported).</exception>
        public abstract void DrawPoint(Color color, float x, float y);

        /// <summary>
        /// Name of the backend
        /// </summary>
        public abstract string Name();

        /// <summary>
        /// Display screen
        /// </summary>
        public abstract void Display();

        /// <summary>
        /// Get point color.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>Color value.</returns>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public abstract Color GetPointColor(int x, int y);

        /// <summary>
        /// Get point offset.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <returns>int value.</returns>
        internal int GetPointOffset(int aX, int aY)
        {
            int xBytePerPixel = (int)Mode.ColorDepth / 8;
            int stride = (int)Mode.ColorDepth / 8;
            int pitch = Mode.Width * xBytePerPixel;

            return aX * stride + aY * pitch;
        }

        /// <summary>
        /// Draw array of colors.
        /// </summary>
        /// <param name="colors">Colors array.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">unused.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid, or width is less than 0.</exception>
        /// <exception cref="NotImplementedException">Thrown if color depth is not supported.</exception>
        public abstract void DrawArray(Color[] colors, int x, int y, int width, int height);

        /// <summary>
        /// Draw horizontal line.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="dx">Line lenght.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        internal void DrawHorizontalLine(Color color, int dx, int x1, int y1)
        {
            int i;

            for (i = 0; i < dx; i++)
            {
                DrawPoint(color, x1 + i, y1);
            }
        }

        /// <summary>
        /// Draw vertical line.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="dy">Line lenght.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
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
        /// Draw diagonal line.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="dx">Line lenght on X axis.</param>
        /// <param name="dy">Line lenght on Y axis.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <exception cref="OverflowException">Thrown if dx or dy equal to Int32.MinValue.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
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
        /// Draw line.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <param name="x2">End point X coordinate.</param>
        /// <param name="y2">End point Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if x1-x2 or y1-y2 equal to Int32.MinValue.</exception>
        public virtual void DrawLine(Color color, int x1, int y1, int x2, int y2)
        {
            // trim the given line to fit inside the canvas boundries
            TrimLine(ref x1, ref y1, ref x2, ref y2);

            var dx = x2 - x1 /* the horizontal distance of the line */;
            var dy = y2 - y1 /* the vertical distance of the line */;

            if (dy == 0) /* The line is horizontal */
            {
                DrawHorizontalLine(color, dx, x1, y1);
                return;
            }

            if (dx == 0) /* the line is vertical */
            {
                DrawVerticalLine(color, dy, x1, y1);
                return;
            }

            /* the line is neither horizontal neither vertical, is diagonal then! */
            DrawDiagonalLine(color, dx, dy, x1, y1);
        }

        /// <summary>
        /// Draw line.
        /// Not implemented.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="x1">Staring point X coordinate.</param>
        /// <param name="y1">Staring point Y coordinate.</param>
        /// <param name="x2">End point X coordinate.</param>
        /// <param name="y2">End point Y coordinate.</param>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public void DrawLine(Color color, float x1, float y1, float x2, float y2)
        {
            throw new NotImplementedException();
        }

        //https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        /// <summary>
        /// Draw Circle.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public virtual void DrawCircle(Color color, int x_center, int y_center, int radius)
        {
            ThrowIfCoordNotValid(x_center + radius, y_center);
            ThrowIfCoordNotValid(x_center - radius, y_center);
            ThrowIfCoordNotValid(x_center, y_center + radius);
            ThrowIfCoordNotValid(x_center, y_center - radius);
            int x = radius;
            int y = 0;
            int e = 0;

            while (x >= y)
            {
                DrawPoint(color, x_center + x, y_center + y);
                DrawPoint(color, x_center + y, y_center + x);
                DrawPoint(color, x_center - y, y_center + x);
                DrawPoint(color, x_center - x, y_center + y);
                DrawPoint(color, x_center - x, y_center - y);
                DrawPoint(color, x_center - y, y_center - x);
                DrawPoint(color, x_center + y, y_center - x);
                DrawPoint(color, x_center + x, y_center - y);

                y++;
                if (e <= 0)
                {
                    e += 2 * y + 1;
                }
                if (e > 0)
                {
                    x--;
                    e -= 2 * x + 1;
                }
            }
        }

        /// <summary>
        /// Draw Filled Circle.
        /// </summary>
        /// <param name="color">Color to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="radius">Radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
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

            /*
            for (int y = -radius; y <= radius; y++)
                for (int x = -radius; x <= radius; x++)
                    if (x * x + y * y <= radius * radius)
                        (origin.x + x, origin.y + y);


            ThrowIfCoordNotValid(x_center + radius, y_center);
            ThrowIfCoordNotValid(x_center - radius, y_center);
            ThrowIfCoordNotValid(x_center, y_center + radius);
            ThrowIfCoordNotValid(x_center, y_center - radius);
            int x = radius;
            int y = 0;
            int e = 0;

            while (x >= y)
            {
                DrawLine(color, x_center - x, y_center + y, x_center + x, y_center + y);
                y++;
                if (e <= 0)
                {
                    e += 2 * y + 1;
                }
                if (e > 0)
                {
                    x--;
                    e -= 2 * x + 1;
                }
            }*/
        }

        //http://members.chello.at/~easyfilter/bresenham.html
        /// <summary>
        /// Draw ellipse.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x_center">X center coordinate.</param>
        /// <param name="y_center">Y center coordinate.</param>
        /// <param name="x_radius">X radius.</param>
        /// <param name="y_radius">Y radius.</param>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public virtual void DrawEllipse(Color color, int x_center, int y_center, int x_radius, int y_radius)
        {
            ThrowIfCoordNotValid(x_center + x_radius, y_center);
            ThrowIfCoordNotValid(x_center - x_radius, y_center);
            ThrowIfCoordNotValid(x_center, y_center + y_radius);
            ThrowIfCoordNotValid(x_center, y_center - y_radius);
            int a = 2 * x_radius;
            int b = 2 * y_radius;
            int b1 = b & 1;
            int dx = 4 * (1 - a) * b * b;
            int dy = 4 * (b1 + 1) * a * a;
            int err = dx + dy + b1 * a * a;
            int e2;
            int y = 0;
            int x = x_radius;
            a *= 8 * a;
            b1 = 8 * b * b;

            while (x >= 0)
            {
                DrawPoint(color, x_center + x, y_center + y);
                DrawPoint(color, x_center - x, y_center + y);
                DrawPoint(color, x_center - x, y_center - y);
                DrawPoint(color, x_center + x, y_center - y);
                e2 = 2 * err;
                if (e2 <= dy) { y++; err += dy += a; }
                if (e2 >= dx || 2 * err > dy) { x--; err += dx += b1; }
            }
        }

        /// <summary>
        /// Draw Filled Ellipse.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="point">Center point.</param>
        /// <param name="height">Height.</param>
        /// <param name="width">Width.</param>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coorinates invalid.</exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        public virtual void DrawFilledEllipse(Color color, int xx, int yy, int height, int width)
        {
            for (int y = -height; y <= height; y++)
            {
                for (int x = -width; x <= width; x++)
                {
                    if (x * x * height * height + y * y * width * width <= height * height * width * width)
                    {
                        DrawPoint(color, xx + x, yy + y);
                    }
                }
            }
        }

        /// <summary>
		/// Draws an arc.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="color"></param>
		/// <param name="StartAngle"></param>
		/// <param name="EndAngle"></param>
        public virtual void DrawArc(int x, int y, int width, int height, Color color, int StartAngle = 0, int EndAngle = 360)
        {
            if (width == 0 || height == 0)
            {
                return;
            }

            for (double Angle = StartAngle; Angle < EndAngle; Angle += 0.5)
            {
                double Angle1 = Math.PI * Angle / 180;
                int IX = (int)(width * Math.Cos(Angle1));
                int IY = (int)(height * Math.Sin(Angle1));
                DrawPoint(color, x + IX, y + IY);
            }
        }

        /// <summary>
        /// Draw polygon.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="points">Points array.</param>
        /// <exception cref="ArgumentException">Thrown if point array is smaller then 3.</exception>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
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
        /// Draw square.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="size">size.</param>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public virtual void DrawSquare(Color color, int x, int y, int size)
        {
            DrawRectangle(color, x, y, size, size);
        }

        /// <summary>
        /// Draw rectangle.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentNullException">Thrown if color is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
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
        /// Draw filled rectangle.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x_start">Starting point X coordinate.</param>
        /// <param name="y_start">Starting point Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines length are invalid.</exception>
        public virtual void DrawFilledRectangle(Color color, int x_start, int y_start, int width, int height)
        {
            if (height == -1)
            {
                height = width;
            }

            for (int y = y_start; y < y_start + height; y++)
            {
                DrawLine(color, x_start, y, x_start + width - 1, y);
            }
        }

        /// <summary>
        /// Draw triangle.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="v1x">First point X coordinate.</param>
        /// <param name="v1y">First point Y coordinate.</param>
        /// <param name="v2x">Second point X coordinate.</param>
        /// <param name="v2y">Second point Y coordinate.</param>
        /// <param name="v3x">Third point X coordinate.</param>
        /// <param name="v3y">Third point Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown if color is null.</item>
        /// <item>Coordinates invalid.</item>
        /// </list>
        /// </exception>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="OverflowException">Thrown if lines lengths are invalid.</exception>
        public virtual void DrawTriangle(Color color, int v1x, int v1y, int v2x, int v2y, int v3x, int v3y)
        {
            DrawLine(color, v1x, v1y, v2x, v2y);
            DrawLine(color, v1x, v1y, v3x, v3y);
            DrawLine(color, v2x, v2y, v3x, v3y);
        }

        /// <summary>
        /// Draw rectangle.
        /// Not implemented.
        /// </summary>
        /// <param name="color">color to draw with.</param>
        /// <param name="x_start">starting X coordinate.</param>
        /// <param name="y_start">starting Y coordinate.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <exception cref="NotImplementedException">Thrown always.</exception>
        public virtual void DrawRectangle(Color color, float x_start, float y_start, float width, float height)
        {
            throw new NotImplementedException();
        }

        //Image and Font will be available in .NET Core 2.1
        // dot net core does not have Image
        //We are using a short term solution for bitmap
        /// <summary>
        /// Draw image.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public virtual void DrawImage(Image image, int x, int y)
        {
            Color _color;

            for (int _x = 0; _x < image.Width; _x++)
            {
                for (int _y = 0; _y < image.Height; _y++)
                {
                    Global.mDebugger.SendInternal(image.rawData[_x + _y * image.Width]);
                    _color = Color.FromArgb(image.rawData[_x + _y * image.Width]);
                    DrawPoint(_color, x + _x, y + _y);
                }
            }
        }

        private int[] scaleImage(Image image, int newWidth, int newHeight)
        {
            int[] pixels = image.rawData;
            int w1 = (int)image.Width;
            int h1 = (int)image.Height;
            int[] temp = new int[newWidth * newHeight];
            int x_ratio = (int)((w1 << 16) / newWidth) + 1;
            int y_ratio = (int)((h1 << 16) / newHeight) + 1;
            int x2, y2;
            for (int i = 0; i < newHeight; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    x2 = (j * x_ratio) >> 16;
                    y2 = (i * y_ratio) >> 16;
                    temp[i * newWidth + j] = pixels[y2 * w1 + x2];
                }
            }
            return temp;
        }
        /// <summary>
        /// Draw a Scaled Bitmap.
        /// </summary>
        /// <param name="image">Image to Scale.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <param name="w">Desired Width.</param>
        /// <param name="h">Desired Height.</param>
        public virtual void DrawImage(Image image, int x, int y, int w, int h)
        {
            Color _color = Color.White;

            int[] pixels = scaleImage(image, w, h);
            for (int _x = 0; _x < w; _x++)
            {
                for (int _y = 0; _y < h; _y++)
                {
                    Global.mDebugger.SendInternal(pixels[_x + _y * w]);
                    _color = Color.FromArgb(pixels[_x + _y * w]);
                    DrawPoint(_color, x + _x, y + _y);
                }
            }
        }

        /// <summary>
        /// Draw image with alpha channel.
        /// </summary>
        /// <param name="image">Image to draw.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="Exception">Thrown on memory access violation.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown on fatal error.</exception>
        public void DrawImageAlpha(Image image, int x, int y)
        {
            Color _color;

            for (int _x = 0; _x < image.Width; _x++)
            {
                for (int _y = 0; _y < image.Height; _y++)
                {
                    Global.mDebugger.SendInternal(image.rawData[_x + _y * image.Width]);
                    _color = Color.FromArgb(image.rawData[_x + _y * image.Width]);
                    DrawPoint(_color, x + _x, y + _y);
                }
            }
        }

        /// <summary>
        /// Draw string.
        /// </summary>
        /// <param name="str">string to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="color">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public virtual void DrawString(string str, Font aFont, Color color, int x, int y)
        {
            for (int i = 0; i < str.Length; i++)
            {
                DrawChar(str[i], aFont, color, x, y);
                x += aFont.Width;
            }
        }

        /// <summary>
        /// Draw char.
        /// </summary>
        /// <param name="str">char to draw.</param>
        /// <param name="aFont">Font used.</param>
        /// <param name="color">Color.</param>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        public virtual void DrawChar(char c, Font aFont, Color color, int x, int y)
        {
            int p = aFont.Height * (byte)c;

            for (int cy = 0; cy < aFont.Height; cy++)
            {
                for (byte cx = 0; cx < aFont.Width; cx++)
                {
                    if (aFont.ConvertByteToBitAddres(aFont.Data[p + cy], cx + 1))
                    {
                        DrawPoint(color, (ushort)(x + (aFont.Width - cx)), (ushort)(y + cy));
                    }
                }
            }
        }

        /// <summary>
        /// Check if video mode is valid.
        /// </summary>
        /// <param name="mode">Video mode.</param>
        /// <returns>bool value.</returns>
        protected bool CheckIfModeIsValid(Mode mode)
        {
            Global.mDebugger.SendInternal($"CheckIfModeIsValid");

            /* To keep or not to keep, that is the question~
            if (mode == null)
            {
                return false;
            }
            */

            /* This would have been the more "modern" version but LINQ is not working

            if (!availableModes.Exists(element => element == mode))
                return true;

            */

            foreach (var elem in AvailableModes)
            {
                Global.mDebugger.SendInternal($"elem is {elem} mode is {mode}");
                if (elem == mode)
                {
                    Global.mDebugger.SendInternal($"Mode {mode} found");
                    return true; // All OK mode does exists in availableModes
                }
            }

            Global.mDebugger.SendInternal($"Mode {mode} found");
            return false;
        }

        /// <summary>
        /// Check if video mode is valid. Throw exception if not.
        /// </summary>
        /// <param name="mode">Video mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if mode is not suppoted.</exception>
        protected void ThrowIfModeIsNotValid(Mode mode)
        {
            if (CheckIfModeIsValid(mode))
            {
                return;
            }

            Global.mDebugger.SendInternal($"Mode {mode} is not found! Raising exception...");
            /* 'mode' was not in the 'availableModes' List ==> 'mode' in NOT Valid */
            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by this Driver");
        }

        /// <summary>
        /// Check if coordinats are valid. Throw exception if not.
        /// </summary>
        /// <param name="x">X coordinate.</param>
        /// <param name="y">Y coordinate.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if coordinates are invalid.</exception>
        protected void ThrowIfCoordNotValid(int x, int y)
        {
            if (x < 0 || x >= Mode.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"x ({x}) is not between 0 and {Mode.Width}");
            }

            if (y < 0 || y >= Mode.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y ({y}) is not between 0 and {Mode.Height}");
            }
        }

        /// <summary>
        /// TrimLine
        /// </summary>
        /// <param name="x1">X coordinate.</param>
        /// <param name="y1">Y coordinate.</param>
        /// <param name="x2">X coordinate.</param>
        /// <param name="y2">Y coordinate.</param>
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
        /// Calculate new Color from back Color with alpha
        /// </summary>
        /// <param name="to">Color to calculate.</param>
        /// <param name="from">Color used to calculate.</param>
        /// <param name="alpha">Alpha amount.</param>
        public Color AlphaBlend(Color to, Color from, byte alpha)
        {
            byte R = (byte)((to.R * alpha + from.R * (255 - alpha)) >> 8);
            byte G = (byte)((to.G * alpha + from.G * (255 - alpha)) >> 8);
            byte B = (byte)((to.B * alpha + from.B * (255 - alpha)) >> 8);
            return Color.FromArgb(R, G, B);
        }
    }
}
