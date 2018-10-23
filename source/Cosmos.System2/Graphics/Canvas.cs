//#define COSMOSDEBUG
using System;
using System.Drawing;
using System.Collections.Generic;

namespace Cosmos.System.Graphics
{
    public abstract class Canvas
    {
        protected Canvas(Mode mode)
        {
            //Global.mDebugger.SendInternal($"Creating a new Canvas with Mode ${mode}");
            Mode = mode;
        }

        public abstract IReadOnlyList<Mode> AvailableModes { get; }

        public abstract Mode DefaultGraphicMode { get; }

        public abstract Mode Mode { get; set; }
        
        /// <summary>
        /// Clear all the Canvas with the Black color.
        /// </summary>
        public void Clear()
        {
            Clear(Color.Black);
        }

        /*
         * Clear all the Canvas with the specified color. Please note that it is a very naïve implementation and any
         * driver should replace it (or with an hardware command or if not possible with a block copy on the IoMemoryBlock)
         */

        public virtual void Clear(Color color)
        {
            Global.mDebugger.SendInternal($"Clearing the Screen with Color {color}");
            //if (color == null)
            //    throw new ArgumentNullException(nameof(color));

            Pen pen = new Pen(color);

            for (int x = 0; x < Mode.Rows; x++)
            {
                for (int y = 0; y < Mode.Columns; y++)
                {
                    DrawPoint(pen, x, y);
                }
            }
        }

        public void DrawPoint(Pen pen, Point point)
        {
            DrawPoint(pen, point.X, point.Y);
        }

        public abstract void DrawPoint(Pen pen, int x, int y);

        public abstract void DrawPoint(Pen pen, float x, float y);

        public abstract Color GetPointColor(int x, int y);

        public virtual void DrawArray(Color[] colors, Point point, int width, int height)
        {
            DrawArray(colors, point.X, point.Y, width, height);
        }

        public abstract void DrawArray(Color[] colors, int x, int y, int width, int height);

        private void DrawHorizontalLine(Pen pen, int dx, int x1, int y1)
        {
            int i;

            for (i = 0; i < dx; i++)
                DrawPoint(pen, x1 + i, y1);
        }

        private void DrawVerticalLine(Pen pen, int dy, int x1, int y1)
        {
            int i;

            for (i = 0; i < dy; i++)
                DrawPoint(pen, x1, y1 + i);
        }

        /*
         * To draw a diagonal line we use the fast version of the Bresenham's algorithm.
         * See http://www.brackeen.com/vga/shapes.html#4 for more informations.
         */

        private void DrawDiagonalLine(Pen pen, int dx, int dy, int x1, int y1)
        {
            int i, sdx, sdy, dxabs, dyabs, x, y, px, py;

            dxabs = Math.Abs(dx);
            dyabs = Math.Abs(dy);
            sdx = Math.Sign(dx);
            sdy = Math.Sign(dy);
            x = dyabs >> 1;
            y = dxabs >> 1;
            px = x1;
            py = y1;

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
                    DrawPoint(pen, px, py);
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
                    DrawPoint(pen, px, py);
                }
            }
        }

        /*
         * DrawLine throw if the line goes out of the boundary of the Canvas, probably will be better to draw only the part
         * of line visibile. This is too "smart" to do here better do it in a future Window Manager.
         */

        public virtual void DrawLine(Pen pen, int x1, int y1, int x2, int y2)
        {
            if (pen == null)
                throw new ArgumentOutOfRangeException(nameof(pen));

            ThrowIfCoordNotValid(x1, y1);

            ThrowIfCoordNotValid(x2, y2);

            int dx, dy;

            dx = x2 - x1;      /* the horizontal distance of the line */
            dy = y2 - y1;      /* the vertical distance of the line */

            if (dy == 0) /* The line is horizontal */
            {
                DrawHorizontalLine(pen, dx, x1, y1);
                return;
            }

            if (dx == 0) /* the line is vertical */
            {
                DrawVerticalLine(pen, dy, x1, y1);
                return;
            }

            /* the line is neither horizontal neither vertical, is diagonal then! */
            DrawDiagonalLine(pen, dx, dy, x1, y1);
        }

        public void DrawLine(Pen pen, Point p1, Point p2)
        {
            DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }

        public void DrawLine(Pen pen, float x1, float y1, float x2, float y2)
        {
            throw new NotImplementedException();
        }

        //https://en.wikipedia.org/wiki/Midpoint_circle_algorithm
        public virtual void DrawCircle(Pen pen, int x_center, int y_center, int radius)
        {
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));
            ThrowIfCoordNotValid(x_center + radius, y_center);
            ThrowIfCoordNotValid(x_center - radius, y_center);
            ThrowIfCoordNotValid(x_center, y_center + radius);
            ThrowIfCoordNotValid(x_center, y_center - radius);
            int x = radius;
            int y = 0;
            int e = 0;

            while (x >= y)
            {
                DrawPoint(pen, x_center + x, y_center + y);
                DrawPoint(pen, x_center + y, y_center + x);
                DrawPoint(pen, x_center - y, y_center + x);
                DrawPoint(pen, x_center - x, y_center + y);
                DrawPoint(pen, x_center - x, y_center - y);
                DrawPoint(pen, x_center - y, y_center - x);
                DrawPoint(pen, x_center + y, y_center - x);
                DrawPoint(pen, x_center + x, y_center - y);

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

        //
        public virtual void DrawCircle(Pen pen, Point point, int radius)
        {
            DrawCircle(pen, point.X, point.Y, radius);
        }

        //http://members.chello.at/~easyfilter/bresenham.html
        public virtual void DrawEllipse(Pen pen, int x_center, int y_center, int x_radius, int y_radius)
        {
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));
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
                DrawPoint(pen, x_center + x, y_center + y);
                DrawPoint(pen, x_center - x, y_center + y);
                DrawPoint(pen, x_center - x, y_center - y);
                DrawPoint(pen, x_center + x, y_center - y);
                e2 = 2 * err;
                if (e2 <= dy) { y++; err += dy += a; }
                if (e2 >= dx || 2 * err > dy) { x--; err += dx += b1; }
            }
        }

        public virtual void DrawEllipse(Pen pen, Point point, int x_radius, int y_radius)
        {
            DrawEllipse(pen, point.X, point.Y, x_radius, y_radius);
        }

        public virtual void DrawPolygon(Pen pen, params Point[] points)
        {
            if (points.Length < 3)
                throw new ArgumentException("A polygon requires more than 3 points.");
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

            for (int i = 0; i < points.Length - 1; i++)
            {
                DrawLine(pen, points[i], points[i + 1]);
            }
            DrawLine(pen, points[0], points[points.Length - 1]);
        }

        public virtual void DrawSquare(Pen pen, Point point, int size)
        {
            DrawRectangle(pen, point.X, point.Y, size, size);
        }

        public virtual void DrawSquare(Pen pen, int x, int y, int size)
        {
            DrawRectangle(pen, x, y, size, size);
        }

        public virtual void DrawRectangle(Pen pen, Point point, int width, int height)
        {
            DrawRectangle(pen, point.X, point.Y, width, height);
        }

        public virtual void DrawRectangle(Pen pen, int x, int y, int width, int height)
        {
            /*
             * we must draw four lines connecting any vertex of our rectangle to do this we first obtain the position of these
             * vertex (we call these vertexes A, B, C, D as for geometric convention)
             */
            if (pen == null)
                throw new ArgumentNullException(nameof(pen));

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
            DrawLine(pen, xa, ya, xb, yb);

            /* Draw a line between A and C */
            DrawLine(pen, xa, ya, xc, yc);

            /* Draw a line between B and D */
            DrawLine(pen, xb, yb, xd, yd);

            /* Draw a line between C and D */
            DrawLine(pen, xc, yc, xd, yd);
        }

        public virtual void DrawFilledRectangle(Pen pen, Point point, int width, int height)
        {
            DrawFilledRectangle(pen, point.X, point.Y, width, height);
        }

        public virtual void DrawFilledRectangle(Pen pen, int x_start, int y_start, int width, int height)
        {
            if (height == -1)
            {
                height = width;
            }

            for (int y = y_start; y < y_start + height; y++)
            {
                DrawLine(pen, x_start, y, x_start + width - 1, y);
            }
        }

        public virtual void DrawTriangle(Pen pen, Point point0, Point point1, Point point2)
        {
            DrawTriangle(pen, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
        }

        public virtual void DrawTriangle(Pen pen, int v1x, int v1y, int v2x, int v2y, int v3x, int v3y)
        {
            DrawLine(pen, v1x, v1y, v2x, v2y);
            DrawLine(pen, v1x, v1y, v3x, v3y);
            DrawLine(pen, v2x, v2y, v3x, v3y);
        }

        public virtual void DrawRectangle(Pen pen, float x_start, float y_start, float width, float height)
        {
            throw new NotImplementedException();
        }

        //Image and Font will be available in .NET Core 2.1
        // dot net core does not have Image
        //We are using a short term solution for bitmap
        public virtual void DrawImage(Image image, int x, int y)
        {
            for (int _x = 0; _x < image.Width; _x++)
            {
                for (int _y = 0; _y < image.Height; _y++)
                {
                    DrawPoint(new Pen(Color.FromArgb(image.rawData[_x + _y * image.Width])), x + _x, y + _y);
                }
            }
        }

        public void DrawImage(Image image, Point point)
        {
            DrawImage(image, point.X, point.Y);
        }

        //public void DrawString(String str, Font aFont, Brush brush, Point point)
        //{
        //    DrawString(str, aFont, brush, point.X, point.Y);
        //}

        //public void DrawString(String str, Font aFont, Brush brush, int x, int y)
        //{
        //    throw new NotImplementedException();
        //}

        protected bool CheckIfModeIsValid(Mode mode)
        {
            Global.mDebugger.SendInternal($"CheckIfModeIsValid");

            /* This would have been the more "modern" version but LINQ is not working

            if (!availableModes.Exists(element => element == mode))
                return true;

            */

            foreach (var elem in AvailableModes)
            {
                if (elem == mode)
                {
                    return true; // All OK mode does exists in availableModes
                }
            }

            return false;
        }

        protected void ThrowIfModeIsNotValid(Mode mode)
        {
            if (CheckIfModeIsValid(mode))
            {
                return;
            }

            Global.mDebugger.SendInternal($"mode is not found! Raising exception...");
            /* 'mode' was not in the 'availableModes' List ==> 'mode' in NOT Valid */
            throw new ArgumentOutOfRangeException(nameof(mode), $"Mode {mode} is not supported by this Driver");
        }

        protected void ThrowIfCoordNotValid(Point point)
        {
            ThrowIfCoordNotValid(point.X, point.Y);
        }

        protected void ThrowIfCoordNotValid(int x, int y)
        {
            if (x < 0 || x >= Mode.Columns)
            {
                throw new ArgumentOutOfRangeException(nameof(x), $"x ({x}) is not between 0 and {Mode.Columns}");
            }

            if (y < 0 || y >= Mode.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(y), $"y ({y}) is not between 0 and {Mode.Rows}");
            }
        }
    }
}
