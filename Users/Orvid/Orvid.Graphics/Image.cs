#define DebugDraw // If this is uncommented, all triangles will have a set-color border drawn. This is useful to debug polygon drawing code.
//#define SqrtWorks // Only uncomment this when the square-root works 

using System;
using System.Collections.Generic;
using System.Text;

namespace Orvid.Graphics
{
    /// <summary>
    /// This class is used to describe an image.
    /// </summary>
    public class Image : Shapes.Shape, IDisposable
    {
        /// <summary>
        /// The raw data in the image.
        /// </summary>
        public Pixel[] Data;
        /// <summary>
        /// The height of the image.
        /// </summary>
        public int Height;
        /// <summary>
        /// The width of the image.
        /// </summary>
        public int Width;

        ///// <summary>
        ///// Creates an image from a bitmap.
        ///// </summary>
        ///// <param name="b">The input bitmap.</param>
        //public Image(System.Drawing.Bitmap b)
        //{
        //    this.Width = b.Width;
        //    this.Height = b.Height;
        //    this.Data = new Pixel[(Height + 1) * (Width + 1)];
        //    for (int height = 0; height < this.Height; height++)
        //    {
        //        for (int width = 0; width < this.Width; width++)
        //        {
        //            System.Drawing.Color pc = b.GetPixel(width, height);
        //            Pixel p = new Pixel(pc.R, pc.G, pc.B, pc.A);
        //            Data[((height * Width) + width)] = p;
        //        }
        //    }
        //}

        /// <summary>
        /// Creates a new image with the specified height and width.
        /// </summary>
        /// <param name="height">The height of the new image.</param>
        /// <param name="width">The width of the new image.</param>
        public Image(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Data = new Pixel[(Height + 1) * (Width + 1)];
        }

        public Image(Vec2 v)
            : this(v.X, v.Y)
        {
        }

        /// <summary>
        /// The default Destructor.
        /// </summary>
        ~Image()
        {
            this.Data = null;
        }

        public void Dispose()
        {
            this.Data = null;
            System.GC.Collect();
        }

        #region Clear
        public virtual void Clear(Pixel color)
        {
            for (uint i = 0; i < Data.Length; i++)
            {
                Data[i] = color;
            }
        }
        #endregion



        #region DON'T WORK!!!

        #region DrawElipse
        /// <summary>
        /// Draws and fills an elipse.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipse(Vec2 CenterPoint, int height, int width, Pixel color)
        {
            double angle = 0;
            double range = (360 * (Math.PI / 180));
            double x = (width * Math.Cos(angle));
            double y = (height * Math.Sin(angle));
            do
            {
                int X = (int)(CenterPoint.X + x + 0.5);
                int Y = (int)(CenterPoint.Y - y + 0.5);
                SetPixel((uint)(X), (uint)(Y), color);
                DrawLine(CenterPoint, new Vec2(X, Y), color);
                angle += 0.001;
                x = (width * Math.Cos(angle));
                y = (height * Math.Sin(angle));
            }
            while (angle <= range);
        }

        /// <summary>
        /// Draws and fills an elipse.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="fillColor">The color to fill in.</param>
        /// <param name="borderColor">The color to draw the border in.</param>
        public void DrawElipse(Vec2 CenterPoint, int height, int width, Pixel fillColor, Pixel borderColor)
        {
            DrawElipse(CenterPoint, height, width, fillColor);
            DrawElipseOutline(CenterPoint, height, width, borderColor);
        }
        #endregion

        #region DrawElipseOutline
        /// <summary>
        /// Draws an elipse outline.
        /// </summary>
        /// <param name="CenterPoint">The center of the elipse</param>
        /// <param name="height">The height of the elipse.</param>
        /// <param name="width">The width of the elipse.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipseOutline(Vec2 CenterPoint, int height, int width, Pixel color)
        {
            DrawElipticalArc(CenterPoint, height, width, 0, 360, color);
        }
        #endregion

        #region DrawElipticalArc
        /// <summary>
        /// Draws an eliptical arc.
        /// </summary>
        /// <param name="CenterPoint">The center-point of the elipse to use.</param>
        /// <param name="height">The height of the elipse to use.</param>
        /// <param name="width">The width of the elipse to use.</param>
        /// <param name="startAngle">The angle to start drawing at.</param>
        /// <param name="endAngle">The angle to stop drawing at.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawElipticalArc(Vec2 CenterPoint, int height, int width, int startAngle, int endAngle, Pixel color)
        {
            double angle = (((startAngle <= endAngle) ? startAngle : endAngle) * (Math.PI / 180));
            double range = (((endAngle > startAngle) ? endAngle : startAngle) * (Math.PI / 180));
            double x = (width * Math.Cos(angle));
            double y = (height * Math.Sin(angle));
            do
            {
                SetPixel((uint)((int)(CenterPoint.X + x + 0.5)), (uint)((int)(CenterPoint.Y - y + 0.5)), color);
                angle += 0.001;
                x = (width * Math.Cos(angle));
                y = (height * Math.Sin(angle));
            }
            while (angle <= range);
        }
        #endregion

        #region DrawQuad
        /// <summary>
        /// Draws a quadrilateral with the specified points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="p4">The fourth point.</param>
        /// <param name="FillColor">The color to fill in the rectangle with.</param>
        /// <param name="BorderColor">The color to draw the border in.</param>
        public void DrawQuad(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, Pixel FillColor, Pixel BorderColor)
        {
            DrawPolygon(new Vec2[] { p1, p2, p3, p4 }, FillColor);
            DrawPolygonOutline(new Vec2[] { p1, p2, p3, p4 }, BorderColor);
        }

        /// <summary>
        /// Draws a quadrilateral with the specified points.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="p4">The fourth point.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawQuad(Vec2 p1, Vec2 p2, Vec2 p3, Vec2 p4, Pixel color)
        {
            DrawPolygon(new Vec2[] { p1, p2, p3, p4 }, color);
        }
        #endregion

        #endregion


        // This SHOULD work. (I can't guarantee it though.)
        #region DrawCircle
        /// <summary>
        /// Draws a filled circle.
        /// </summary>
        /// <param name="Center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawCircle(Vec2 Center, int radius, Pixel color)
        {
#if SqrtWorks
            int x1;
            int x2;
            int rSquared = radius * radius;
            int counter = (Center.Y + radius);
            for (int count = (Center.Y - radius); count <= counter; count++)
            {
                int ySquared = (count - Center.Y) * (count - Center.Y);
                double sqrt = Math.Sqrt(rSquared - ySquared) + 0.5;
                x1 = (int)(Center.X + sqrt);
                x2 = (int)(Center.X - sqrt);
                Vec2 p1 = new Vec2(x1, count);
                Vec2 p2 = new Vec2(x2, count);
                DrawLine(p1, p2, color);
            }
            DrawCircleOutline(Center, radius, color);
#else
            Vec2 tlcorn = new Vec2(Center.X - radius, Center.Y - radius);
            Image i = new Image(radius * 2 + 2, radius * 2 + 2);
            i.DrawCircleOutline(Center - tlcorn, radius, color);
            i.FloodFill(Center - tlcorn, new Pixel(true), color);
            DrawImage(tlcorn, i);
#endif
        }
        #endregion


        #region DrawCircleOutline
        /// <summary>
        /// Draws the outline of a circle.
        /// </summary>
        /// <param name="Center">The center of the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="color">The color to draw with.</param>
        public void DrawCircleOutline(Vec2 Center, int radius, Pixel color)
        {
            int x = 0;
            int y = radius;
            int p = (5 - radius * 4) / 4;

            SetPixel((uint)(Center.X), (uint)(Center.Y + y), color);
            SetPixel((uint)(Center.X), (uint)(Center.Y - y), color);
            SetPixel((uint)(Center.X + y), (uint)(Center.Y), color);
            SetPixel((uint)(Center.X - y), (uint)(Center.Y), color);
            while (x < y)
            {
                x++;
                if (p < 0)
                {
                    p += 2 * x + 1;
                }
                else
                {
                    y--;
                    p += 2 * (x - y) + 1;
                }
                if (x == y)
                {
                    SetPixel((uint)(Center.X + x), (uint)(Center.Y + y), color);
                    SetPixel((uint)(Center.X - x), (uint)(Center.Y + y), color);
                    SetPixel((uint)(Center.X + x), (uint)(Center.Y - y), color);
                    SetPixel((uint)(Center.X - x), (uint)(Center.Y - y), color);
                }
                else
                {
                    if (x < y)
                    {
                        SetPixel((uint)(Center.X + x), (uint)(Center.Y + y), color);
                        SetPixel((uint)(Center.X - x), (uint)(Center.Y + y), color);
                        SetPixel((uint)(Center.X + x), (uint)(Center.Y - y), color);
                        SetPixel((uint)(Center.X - x), (uint)(Center.Y - y), color);
                        SetPixel((uint)(Center.X + y), (uint)(Center.Y + x), color);
                        SetPixel((uint)(Center.X - y), (uint)(Center.Y + x), color);
                        SetPixel((uint)(Center.X + y), (uint)(Center.Y - x), color);
                        SetPixel((uint)(Center.X - y), (uint)(Center.Y - x), color);
                    }
                }
            }
        }
        #endregion

        #region DrawImage
        /// <summary>
        /// Draws the specified image, at the specified point.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="i"></param>
        public void DrawImage(Vec2 loc, Image i)
        {
            Pixel value;
            if (loc.X < 0)
            {
                if (loc.Y < 0) // Both X and Y are negative
                {
                    for (uint y = (uint)i.Height; y >= 0 && y + loc.Y >= 0; y--)
                    {
                        for (uint x = (uint)i.Width; x >= 0 && x + loc.X >= 0; x--)
                        {
                            value = i.GetPixel(x, y);
                            if (!value.Empty)
                                SetPixel((uint)(x + loc.X), (uint)(y + loc.Y), value);
                        }
                    }
                }
                else // Only X is negative.
                {
                    for (uint y = 0; y < i.Height && y + loc.Y < this.Height; y++)
                    {
                        for (uint x = (uint)i.Width; x >= 0 && x + loc.X >= 0; x--)
                        {
                            value = i.GetPixel(x, y);
                            if (!value.Empty)
                                SetPixel((uint)(x + loc.X), (uint)(y + loc.Y), value);
                        }
                    }
                }
            }
            else
            {
                if (loc.Y < 0) // Only X Positive
                {
                    for (uint y = (uint)i.Height; y >= 0 && y + loc.Y >= 0; y--)
                    {
                        for (uint x = 0; x < i.Width && x + loc.X < this.Width; x++)
                        {
                            value = i.GetPixel(x, y);
                            if (!value.Empty)
                                SetPixel((uint)(x + loc.X), (uint)(y + loc.Y), value);
                        }
                    }
                }
                else // Both positive
                {
                    for (uint y = 0; y < i.Height && y + loc.Y < this.Height; y++)
                    {
                        for (uint x = 0; x < i.Width && x + loc.X < this.Width; x++)
                        {
                            value = i.GetPixel(x, y);
                            if (!value.Empty)
                                SetPixel((uint)(x + loc.X), (uint)(y + loc.Y), value);
                        }
                    }
                }
            }
        }
        #endregion

        #region DrawLine
        /// <summary>
        /// Draws a line between 2 points.
        /// </summary>
        /// <param name="Point1">The first point.</param>
        /// <param name="Point2">The first point.</param>
        /// <param name="color">The color to draw.</param>
        public void DrawLine(Vec2 Point1, Vec2 Point2, Pixel color)
        {
            int x0 = Point1.X;
            int x1 = Point2.X;
            int y0 = Point1.Y;
            int y1 = Point2.Y;
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int x3 = y0;
                y0 = x0;
                x0 = x3;
                int x4 = y1;
                y1 = x1;
                x1 = x4;
            }
            if (x0 > x1)
            {
                int x5 = x0;
                x0 = x1;
                x1 = x5;
                int x6 = y0;
                y0 = y1;
                y1 = x6;
            }
            int deltax = x1 - x0;
            int deltay = Math.Abs(y1 - y0);
            int error = deltax / 2;
            int ystep;
            int y = y0;
            if (y0 < y1)
            {
                ystep = 1;
            }
            else
            {
                ystep = -1;
            }
            for (int x = x0; x <= x1; x++)
            {
                if (steep)
                {
                    SetPixel((uint)y, (uint)x, color);
                }
                else
                {
                    SetPixel((uint)x, (uint)y, color);
                }
                error = error - deltay;
                if (error < 0)
                {
                    y = y + ystep;
                    error = error + deltax;
                }
            }
        }
        #endregion

        #region DrawLines
        /// <summary>
        /// Draws a set of connected lines.
        /// </summary>
        /// <param name="Points">
        /// An array of points to draw, in the order they need drawing.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void DrawLines(Vec2[] Points, Pixel color)
        {
            int n = Points.Length;
            if (n >= 2)
            {
                DrawLine(Points[0], Points[1], color);

                for (int count = 1; count < (n - 1); count++)
                {
                    DrawLine(Points[count], Points[count + 1], color);
                }
            }
            else
            {
                throw new Exception("Not enough points provided!");
            }
        }
        #endregion

        #region DrawPolygon
        /// <summary>
        /// Draws a polygon with the specified points.
        /// </summary>
        /// <param name="points">The points of the polygon.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawPolygon(Vec2[] points, Pixel color)
        {
            Vec2[] contour = points;
            List<Vec2> result = new List<Vec2>();
            int cnt = 0;
            #region Process
            {
#warning TODO: Change these back to floats when their implemented fully.
                double EPSILON = 0.0000000001f;
                int n = contour.Length;
                if (n < 3)
                {
                    throw new Exception("Not enough Verticies!");
                }
                int[] V = new int[n];
                int Area;
                {
                    int n2 = contour.Length;
                    double A = 0.0f;
                    for (int p = n2 - 1, q = 0; q < n2; p = q++)
                    {
                        A += contour[p].X * contour[q].Y - contour[q].X * contour[p].Y;
                    }
                    Area = (Int32)(A * 0.5f);
                }
                if (0.0f < Area)
                {
                    for (int v = 0; v < n; v++)
                    {
                        V[v] = v;
                    }
                }
                else
                {
                    for (int v = 0; v < n; v++)
                    {
                        V[v] = (n - 1) - v;
                    }
                }
                int nv = n;
                int count = 2 * nv;
                for (int m = 0, v = nv - 1; nv > 2; )
                {
                    if (0 >= (count--))
                    {
                        throw new Exception("Invalid polygon!");
                    }
                    int u = v;
                    if (nv <= u)
                    {
                        u = 0;
                    }
                    v = u + 1;
                    if (nv <= v)
                    {
                        v = 0;
                    }
                    int w = v + 1;
                    if (nv <= w)
                    {
                        w = 0;
                    }
                    bool Snip;
                    {
                        int p;
                        double Ax, Ay, Bx, By, Cx, Cy, Px, Py;
                        Ax = contour[V[u]].X;
                        Ay = contour[V[u]].Y;
                        Bx = contour[V[v]].X;
                        By = contour[V[v]].Y;
                        Cx = contour[V[w]].X;
                        Cy = contour[V[w]].Y;
                        if (EPSILON > (((Bx - Ax) * (Cy - Ay)) - ((By - Ay) * (Cx - Ax))))
                        {
                            Snip = false;
                        }
                        for (p = 0; p < nv; p++)
                        {
                            if ((p == u) || (p == v) || (p == w))
                            {
                                continue;
                            }
                            Px = contour[V[p]].X;
                            Py = contour[V[p]].Y;
                            bool InsideTriangle;
                            {
                                double ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
                                double cCROSSap, bCROSScp, aCROSSbp;
                                ax = Cx - Bx;
                                ay = Cy - By;
                                bx = Ax - Cx;
                                by = Ay - Cy;
                                cx = Bx - Ax;
                                cy = By - Ay;
                                apx = Px - Ax;
                                apy = Py - Ay;
                                bpx = Px - Bx;
                                bpy = Py - By;
                                cpx = Px - Cx;
                                cpy = Py - Cy;
                                aCROSSbp = ax * bpy - ay * bpx;
                                cCROSSap = cx * apy - cy * apx;
                                bCROSScp = bx * cpy - by * cpx;
                                InsideTriangle = ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
                            }
                            if (InsideTriangle)
                            {
                                Snip = false;
                            }
                        }
                        Snip = true;
                    }
                    if (Snip)
                    {
                        int a, b, c, s, t;
                        a = V[u];
                        b = V[v];
                        c = V[w];
                        result.Add(contour[a]);
                        result.Add(contour[b]);
                        result.Add(contour[c]);
                        m++;
                        for (s = v, t = v + 1; t < nv; s++, t++)
                        {
                            V[s] = V[t];
                        }
                        nv--;
                        count = 2 * nv;
                    }
                }
            }
            #endregion
            while (cnt < (result.Count / 3))
            {
                DrawTriangle(result[(cnt * 3)], result[(cnt * 3) + 1], result[(cnt * 3) + 2], color);
                cnt++;
            }
        }
        #endregion

        #region DrawPolygonOutline
        /// <summary>
        /// Draws the outline of a polygon.
        /// The last point connects to the first point.
        /// </summary>
        /// <param name="points">An array containing the points to draw.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawPolygonOutline(Vec2[] points, Pixel color)
        {
            Vec2[] pts = new Vec2[points.Length + 1];
            points.CopyTo(pts, 0);
            pts[pts.Length - 1] = points[0];
            DrawLines(pts, color);
        }
        #endregion

        #region DrawRectangle
        /// <summary>
        /// This method fills in the space between
        /// the 2 points specified in a rectangle.
        /// </summary>
        /// <param name="TopLeftCorner">
        /// The point that specifies the top left 
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="BottomRightCorner">
        /// The point that specifies the bottom right
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void DrawRectangle(Vec2 TopLeftCorner, Vec2 BottomRightCorner, Pixel color)
        {
            int height = BottomRightCorner.Y - TopLeftCorner.Y;
            int width = BottomRightCorner.X - TopLeftCorner.X;
            Image i = new Image(width, height);
            i.Clear(color);
            DrawImage(TopLeftCorner, i);
            //DrawQuad(TopLeftCorner, new Vec2(BottomRightCorner.X, TopLeftCorner.Y), BottomRightCorner, new Vec2(TopLeftCorner.X, BottomRightCorner.Y), color);
        }
        #endregion

        #region DrawReverseRectangle
        /// <summary>
        /// This method fills in the space between
        /// the 2 points specified in a rectangle.
        /// </summary>
        /// <param name="TopRightCorner">
        /// The point that specifies the top right 
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="BottomLeftCorner">
        /// The point that specifies the bottom left
        /// corner of the rectangle being drawn.
        /// </param>
        /// <param name="color">The color to draw.</param>
        public void ReverseDrawRectangle(Vec2 TopRightCorner, Vec2 BottomLeftCorner, Pixel color)
        {
            int height = BottomLeftCorner.Y - TopRightCorner.Y;
            int width = TopRightCorner.X - BottomLeftCorner.X;
            Image i = new Image(width, height);
            i.Clear(color);
            Vec2 TopLeftCorner = new Vec2(BottomLeftCorner.X, TopRightCorner.Y);
            DrawImage(TopLeftCorner, i);
            //DrawQuad(new Vec2(BottomLeftCorner.X, TopRightCorner.Y), TopRightCorner, new Vec2(TopRightCorner.X, BottomLeftCorner.Y), BottomLeftCorner, color);
        }
        #endregion

        #region DrawTriangle
        /// <summary>
        /// Draws a triangle and fills it in.
        /// </summary>
        /// <param name="p1">The first point of the triangle.</param>
        /// <param name="p2">The second point of the triangle.</param>
        /// <param name="p3">The third point of the triangle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawTriangle(Vec2 p1, Vec2 p2, Vec2 p3, Pixel color)
        {

            int l; // The farthest left 
            int r;
            int t;
            int b;
            #region Organize points
            if (p1.Y > p2.Y && p1.Y > p3.Y)
            {
                t = p1.Y;
                if (p2.Y < p3.Y)
                {
                    b = p2.Y;
                }
                else
                {
                    b = p3.Y;
                }
            }
            else if (p2.Y > p1.Y && p2.Y > p3.Y)
            {
                t = p2.Y;
                if (p1.Y < p3.Y)
                {
                    b = p1.Y;
                }
                else
                {
                    b = p3.Y;
                }
            }
            else // This means p3 is the highest point
            {
                t = p3.Y;
                if (p1.Y < p2.Y)
                {
                    b = p1.Y;
                }
                else
                {
                    b = p2.Y;
                }
            }

            // Now organize in the width direction.
            if (p1.X > p3.X && p1.X > p2.X)
            {
                l = p1.X;
                if (p2.X < p3.X)
                {
                    r = p2.X;
                }
                else
                {
                    r = p3.X;
                }
            }
            else if (p2.X > p3.X && p2.X > p1.X)
            {
                l = p2.X;
                if (p1.X < p3.X)
                {
                    r = p1.X;
                }
                else
                {
                    r = p3.X;
                }
            }
            else
            {
                l = p3.X;
                if (p2.X < p1.X)
                {
                    r = p2.X;
                }
                else
                {
                    r = p1.X;
                }
            }
            #endregion
            int h;
            int w;
            if (t - b + 1 > 0)
            {
                h = t - b + 1;
            }
            else
            {
                h = b - t + 1;
            }
            if (l - r + 1 > 0)
            {
                w = l - r + 1;
            }
            else
            {
                w = r - l + 1;
            }
            Image i = new Image(w, h);
            // Get the location of the points within the image.
            Vec2 p4 = new Vec2(p1.X - r, p1.Y - b);
            Vec2 p5 = new Vec2(p2.X - r, p2.Y - b);
            Vec2 p6 = new Vec2(p3.X - r, p3.Y - b);
#if DebugDraw
            Pixel p9 = new Pixel();
            p9.B = 200;
            i.DrawLine(p4, p5, p9);
            i.DrawLine(p5, p6, p9);
            i.DrawLine(p6, p4, p9);
#else
            i.DrawLine(p4, p5, color);
            i.DrawLine(p5, p6, color);
            i.DrawLine(p6, p4, color);
#endif
            Vec2 center = (p4 + p5 + p6) / 3;
            i.FloodFill(center, new Pixel(true), color);

            /*

            NOTE TO SELF: Draw a line from each corner to the middle of the opposite line,
                          so as to fill in the holes that this flood fill method might leave,
                          when dealing with small angles. Some acute triangles have this problem.
            
            */
            Vec2 m1 = (p4 + p5) / 2;
            Vec2 m2 = (p5 + p6) / 2;
            Vec2 m3 = (p4 + p6) / 2;
#if DebugDraw
            Pixel p50 = new Pixel();
            p50.B = 200;
            i.DrawLine(p4, p5, p50);
            i.DrawLine(p5, p6, p50);
            i.DrawLine(p6, p4, p50);
#else
            i.DrawLine(p6, m1, color);
            i.DrawLine(p4, m2, color);
            i.DrawLine(p5, m3, color);
#endif

            Vec2 v = new Vec2(r, b);
            DrawImage(v, i);
        }

        /// <summary>
        /// Draws a triangle with the specified fill color, and the specified border color.
        /// </summary>
        /// <param name="p1">The first point.</param>
        /// <param name="p2">The second point.</param>
        /// <param name="p3">The third point.</param>
        /// <param name="FillColor">The color to fill the triangle with.</param>
        /// <param name="BorderColor">The color to draw the border of the triangle.</param>
        public void DrawTriangle(Vec2 p1, Vec2 p2, Vec2 p3, Pixel FillColor, Pixel BorderColor)
        {
            DrawTriangle(p1, p2, p3, FillColor);
            DrawTriangleOutline(p1, p2, p3, BorderColor);
        }
        #endregion

        #region DrawTriangleOutline
        /// <summary>
        /// Draw a triangle's outline.
        /// </summary>
        /// <param name="p1">The first point of the triangle.</param>
        /// <param name="p2">The second point of the triangle.</param>
        /// <param name="p3">The third point of the triangle.</param>
        /// <param name="color">The color to draw in.</param>
        public void DrawTriangleOutline(Vec2 p1, Vec2 p2, Vec2 p3, Pixel color)
        {
            DrawLines(new Vec2[] { p1, p2, p3, p1 }, color);
        }
        #endregion

        #region FloodFill
        private bool[] Visiteds;
        /// <summary>
        /// Returns true if the given pixel has
        /// been visited with the flood-fill algorithm.
        /// </summary>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool Visited(int y, int x)
        {
            if (x >= 0 && y >= 0 && y <= Height && x <= Width)
                return Visiteds[((y * Width) + x)];
            return true;
        }

        /// <summary>
        /// Implements a flood fill algorithm.
        /// </summary>
        /// <param name="startPoint">The point to start filling from.</param>
        /// <param name="sColor">The color to search for.</param>
        /// <param name="dColor">The color to change it to.</param>
        public void FloodFill(Vec2 startPoint, Pixel sColor, Pixel dColor)
        {
            int x = startPoint.X;
            int y = startPoint.Y;
            if ((GetPixel((uint)x, (uint)y).Empty) || GetPixel((uint)x, (uint)y) == sColor)
            {
                SetPixel((uint)x, (uint)y, dColor);
            }
            Stack<Vec2> q = new Stack<Vec2>();
            Visiteds = new bool[(Height + 1) * (Width + 1)];

            #region Setup Start of queue
            Pixel p;
            Vec2 v;
            p = GetPixel((uint)x + 1, (uint)y);
            if ((p.Empty || p == sColor) && p != dColor)
            {
                v = new Vec2(x + 1, y);
                q.Push(v);
                Visiteds[((y * Width) + x + 1)] = true;
                SetPixel((uint)x + 1, (uint)y, dColor);
            }
            p = GetPixel((uint)x, (uint)y + 1);
            if ((p.Empty || p == sColor) && p != dColor)
            {
                v = new Vec2(x, y + 1);
                q.Push(v);
                Visiteds[(((y + 1) * Width) + x)] = true;
                SetPixel((uint)x, (uint)y + 1, dColor);
            }
            p = GetPixel((uint)x - 1, (uint)y);
            if ((p.Empty || p == sColor) && p != dColor)
            {
                v = new Vec2(x - 1, y);
                q.Push(v);
                Visiteds[((y * Width) + x - 1)] = true;
                SetPixel((uint)(x - 1), (uint)y, dColor);
            }
            p = GetPixel((uint)x, (uint)y - 1);
            if ((p.Empty || p == sColor) && p != dColor)
            {
                v = new Vec2(x, y - 1);
                q.Push(v);
                Visiteds[(((y - 1) * Width) + x)] = true;
                SetPixel((uint)x, (uint)(y - 1), dColor);
            }
            #endregion

            while (q.Count > 0)
            {
                v = q.Pop();
                x = v.X;
                y = v.Y;
                #region Check

                p = GetPixel((uint)x + 1, (uint)y);
                if (p != dColor && (p.Empty || p == sColor))
                {
                    v = new Vec2(x + 1, y);
                    if (x >= 0 && !(Visited(y, x + 1)))
                    {
                        q.Push(v);
                        Visiteds[((y * Width) + x + 1)] = true;
                        SetPixel((uint)x + 1, (uint)y, dColor);
                    }
                }
                p = GetPixel((uint)x, (uint)y + 1);
                if (p != dColor && (p.Empty || p == sColor))
                {
                    v = new Vec2(x, y + 1);
                    if (x >= 0 && !(Visited(y + 1, x)))
                    {
                        q.Push(v);
                        Visiteds[(((y + 1) * Width) + x)] = true;
                        SetPixel((uint)x, (uint)y + 1, dColor);
                    }
                }
                p = GetPixel((uint)x - 1, (uint)y);
                if (p != dColor && (p.Empty || p == sColor))
                {
                    v = new Vec2(x - 1, y);
                    if (x >= 0 && !(Visited(y, x - 1)))
                    {
                        q.Push(v);
                        Visiteds[((y * Width) + x - 1)] = true;
                        SetPixel((uint)x - 1, (uint)y, dColor);
                    }
                }
                p = GetPixel((uint)x, (uint)y - 1);
                if (p != dColor && (p.Empty || p == sColor))
                {
                    v = new Vec2(x, y - 1);
                    if (!(Visited(y - 1, x)))
                    {
                        q.Push(v);
                        Visiteds[(((y - 1) * Width) + x)] = true;
                        SetPixel((uint)x, (uint)y - 1, dColor);
                    }
                }
                #endregion
            }
            // And finally empty the visiteds array.
            Visiteds = new bool[0];
        }
        #endregion

        #region SubImage
        /// <summary>
        /// Gets a sub-image of this image,
        /// from the specified location,
        /// of the specified size.
        /// </summary>
        /// <param name="loc">The location of the image to pull.</param>
        /// <param name="size">The size of the image to pull.</param>
        /// <returns>The sub-image obtained.</returns>
        public Image SubImage(Vec2 loc, Vec2 size)
        {
            Image i = new Image(size);
            for (int y = loc.Y; y < (loc.Y + size.Y); y++)
            {
                for (int x = loc.X; x < (loc.X + size.X); x++)
                {
                    i.SetPixel((uint)(x - loc.X), (uint)(y - loc.Y), this.GetPixel((uint)x, (uint)y));
                }
            }
            return i;
        }
        #endregion


        /// <summary>
        /// Get's the pixel a the specified location.
        /// </summary>
        /// <param name="width">The width position of the pixel.</param>
        /// <param name="height">The height position of the pixel.</param>
        /// <returns>The Pixel at the specified position.</returns>
        public virtual Pixel GetPixel(uint x, uint y)
        {
            if (/*x > 0 &&*/ x < Width /*&& y > 0 */&& y < Height)
                return Data[(y * Width) + x];
            else
                return new Pixel(true);
        }

        //public List<Vec2> Modified = new List<Vec2>();

        /// <summary>
        /// Set's the pixel at the specified location, 
        /// to the specified pixel.
        /// </summary>
        /// <param name="width">The width position of the pixel.</param>
        /// <param name="height">The height position of the pixel.</param>
        /// <param name="p">The pixel to set to.</param>
        public virtual void SetPixel(uint x, uint y, Pixel p)
        {
            //Modified.Add(new Vec2(width, height));
            if (p.A != 255)
            {
                if (p.A != 0)
                {
                    double r1 = ((double)p.A / 255);
                    double r2 = 1.0d - r1;
                    Pixel cur = Data[((y * Width) + x)];
                    //throw new Exception();

                    Data[((y * Width) + x)] = new Pixel((byte)((p.R * r1) + (cur.R * r2)), (byte)((p.G * r1) + (cur.G * r2)), (byte)((p.B * r1) + (cur.B * r2)), 255);
                }
                // else nothing gets drawn.
            }
            else
            {
                Data[((y * Width) + x)] = p;
            }
        }

        public void DrawString(Vec2 loc, String s, FontSupport.Font f, int stringHeight, FontSupport.FontStyle flags, Pixel color)
        {
            FontSupport.FontManager.Instance.DrawText(this, null, null, s, f, loc, color);
        }

        public override void Draw()
        {
            Parent.DrawImage(new Vec2(base.X, base.Y), this);
        }

        #region Operators
        public static explicit operator System.Drawing.Bitmap(Image i)
        {
            System.Drawing.Bitmap b = new System.Drawing.Bitmap(i.Width, i.Height);
            System.Drawing.Imaging.BitmapData bd = b.LockBits(new System.Drawing.Rectangle(0, 0, i.Width, i.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            int bytes  = Math.Abs(bd.Stride) * bd.Height;
            byte[] rgbValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, rgbValues, 0, bytes);
            uint len = (uint)(i.Height * i.Width);
            for (uint d = 0, ind = 0; d < len; d++)
            {
                rgbValues[ind] = i.Data[d].B;
                ind++;
                rgbValues[ind] = i.Data[d].G;
                ind++;
                rgbValues[ind] = i.Data[d].R;
                ind++;
                rgbValues[ind] = i.Data[d].A;
                ind++;
            }
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, bd.Scan0, bytes);
            rgbValues = null;
            b.UnlockBits(bd);
            return b;
        }

        public static explicit operator Image(System.Drawing.Bitmap b)
        {
            Image i = new Image(b.Width, b.Height);
            for (uint x = 0; x < b.Width; x++)
            {
                for (uint y = 0; y < b.Height; y++)
                {
                    i.SetPixel(x, y, b.GetPixel((int)x, (int)y));
                }
            }
            return i;
        }
        #endregion
    }
}
