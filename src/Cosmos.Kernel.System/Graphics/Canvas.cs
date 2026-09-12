//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Represents a drawing surface. Can be used directly as a virtual (buffer-backed)
/// canvas, or subclassed for hardware-backed canvases.
/// </summary>
/// <remarks>
/// Every drawing primitive clips. A pixel outside 0..<see cref="Width"/>-1 by
/// 0..<see cref="Height"/>-1 is dropped, a shape that straddles an edge is drawn
/// up to it, and nothing throws for a coordinate, so coordinates never need
/// clamping before a call.
/// </remarks>
public unsafe class Canvas
{
    /// <summary>
    /// Pixel buffer for virtual canvases. Null for hardware-backed subclasses,
    /// which draw straight through their device. Each element is a raw ARGB
    /// pixel value.
    /// </summary>
    private int[]? _buffer;

    /// <summary>
    /// The graphics modes this canvas accepts, in the order the driver reports
    /// them. <see cref="Mode"/> only accepts a mode from this list.
    /// </summary>
    public virtual IReadOnlyList<Mode> AvailableModes => new Mode[] { Mode };

    /// <summary>
    /// The default graphics mode.
    /// </summary>
    public virtual Mode DefaultGraphicsMode => Mode;

    private Mode _mode;

    /// <summary>
    /// The currently used display mode. Setting it resizes a virtual canvas'
    /// buffer, discarding its contents, and recomputes the pixel metrics.
    /// Hardware-backed canvases reprogram the device in their override.
    /// </summary>
    /// <remarks>
    /// The setter is not part of the ring: a kernel picks its mode when it
    /// acquires the canvas, through <see cref="GetFullScreen(Mode)"/> or the
    /// virtual-canvas constructor.
    /// </remarks>
    public virtual Mode Mode
    {
        get => _mode;
        protected internal set
        {
            _mode = value;
            _bytesPerPixel = (int)value.ColorDepth / 8;
            _stride = (int)value.ColorDepth / 8;
            _pitch = value.Width * _bytesPerPixel;

            int length = value.Width * value.Height;
            if (_buffer != null && _buffer.Length != length)
            {
                _buffer = new int[length];
            }
        }
    }

    /// <summary>
    /// The width of this canvas in pixels.
    /// </summary>
    public int Width => Mode.Width;

    /// <summary>
    /// The height of this canvas in pixels.
    /// </summary>
    public int Height => Mode.Height;

    /// <summary>
    /// Screen refresh rate in Hz as reported by EDID. Defaults to 60 if unavailable.
    /// </summary>
    public virtual int RefreshRate => 60;

    /// <summary>
    /// The name of the Canvas implementation.
    /// </summary>
    public virtual string Name => "Canvas";

    /// <summary>
    /// Bytes per pixel (4 in 32bit, 3 in 24bit).
    /// </summary>
    internal int _bytesPerPixel;

    /// <summary>
    /// _stride.
    /// </summary>
    internal int _stride;

    /// <summary>
    /// _pitch.
    /// </summary>
    internal int _pitch;

    /// <summary>
    /// Initializes a new instance of the <see cref="Canvas"/> class.
    /// Used by subclasses.
    /// </summary>
    protected Canvas()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Canvas"/> class with a mode.
    /// Used by subclasses.
    /// </summary>
    protected Canvas(Mode mode)
    {
        _mode = mode;
        _bytesPerPixel = (int)mode.ColorDepth / 8;
        _stride = (int)mode.ColorDepth / 8;
        _pitch = mode.Width * _bytesPerPixel;
    }

    /// <summary>
    /// Creates a virtual (buffer-backed) canvas of the given size.
    /// </summary>
    /// <param name="width">The width of the canvas in pixels.</param>
    /// <param name="height">The height of the canvas in pixels.</param>
    /// <param name="colorDepth">The color depth (default 32-bit).</param>
    public Canvas(int width, int height, ColorDepth colorDepth = ColorDepth.ColorDepth32)
    {
        _mode = new Mode(width, height, colorDepth);
        _bytesPerPixel = (int)colorDepth / 8;
        _stride = (int)colorDepth / 8;
        _pitch = width * _bytesPerPixel;
        _buffer = new int[width * height];
    }

    /// <summary>
    /// Gets the hardware-backed full-screen canvas using the default graphics
    /// mode. The first call builds it against the detected display device;
    /// later calls hand back the same canvas without resetting the mode, so
    /// <see cref="Mode"/> always reports the real screen size.
    /// <para>
    /// The display device decides the canvas's type. Only the VMware SVGA II
    /// adapter negotiates 3D, so a kernel that wants to render 3D tests the
    /// canvas with <c>is <see cref="Canvas3D"/></c>; on the UEFI framebuffer
    /// every documented setup uses, it never is.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Graphics support is compiled out with CosmosEnableGraphics=false. Test
    /// <see cref="KernelFeatures.Graphics"/> first to avoid it.
    /// </exception>
    public static Canvas GetFullScreen()
    {
        return FullScreenCanvas.Get();
    }

    /// <summary>
    /// Gets the hardware-backed full-screen canvas, switching the display to
    /// <paramref name="mode"/>. The UEFI framebuffer cannot change mode after
    /// boot, so on the GOP path the request is ignored and the canvas keeps
    /// reporting the resolution the bootloader set.
    /// </summary>
    /// <param name="mode">
    /// The display mode to switch to; must be one of the canvas's
    /// <see cref="AvailableModes"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out with CosmosEnableGraphics=false.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The display device does not support <paramref name="mode"/>.</exception>
    public static Canvas GetFullScreen(Mode mode)
    {
        return FullScreenCanvas.Get(mode);
    }

    /// <summary>
    /// Returns the display device to text mode and gives the screen back, so a
    /// later <see cref="GetFullScreen()"/> builds a fresh canvas against a
    /// re-enabled device. Any canvas already acquired is dead after this call.
    /// There is no VGA text mode to fall back to on UEFI machines, where this
    /// is a no-op.
    /// </summary>
    public static void DisableFullScreen()
    {
        FullScreenCanvas.Disable();
    }

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
    public virtual void Clear(int color)
    {
        if (_buffer == null)
        {
            return;
        }

        Array.Fill(_buffer, color);
    }

    /// <summary>
    /// Clears the entire canvas with the specified color.
    /// </summary>
    /// <param name="color">The color to clear the screen with.</param>
    public virtual void Clear(Color color)
    {
        Clear(color.ToArgb());
    }

    /// <summary>
    /// Turns the display device off. The device half of
    /// <see cref="DisableFullScreen"/>, which is what a kernel calls: a
    /// virtual canvas has no device to turn off at all.
    /// </summary>
    internal virtual void Disable()
    {
    }

    /// <summary>
    /// Sets the pixel at the given coordinates to the specified <paramref name="color"/>.
    /// </summary>
    /// <param name="color">The color to draw with.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual void DrawPoint(Color color, int x, int y)
    {
        if (_buffer == null)
        {
            return;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        if (color.A < 255)
        {
            if (color.A == 0)
            {
                return;
            }

            color = AlphaBlend(color, GetPointColor(x, y), color.A);
        }

        _buffer[y * Width + x] = color.ToArgb();
    }

    /// <summary>
    /// Sets the pixel at the given coordinates to the specified <paramref name="color"/>, without unnecessary color operations.
    /// </summary>
    /// <param name="color">The color to draw with (raw argb).</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual void DrawPoint(uint color, int x, int y)
    {
        if (_buffer == null)
        {
            return;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _buffer[y * Width + x] = (int)color;
    }

    /// <summary>
    /// Sets the pixel at the given coordinates to the specified <paramref name="color"/>. without ToArgb()
    /// </summary>
    /// <param name="color">The color to draw with (raw argb).</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual void DrawPoint(int color, int x, int y)
    {
        if (_buffer == null)
        {
            return;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _buffer[y * Width + x] = color;
    }

    /// <summary>
    /// Updates the screen to display the underlying frame-buffer.
    /// For virtual canvases, this is a no-op.
    /// </summary>
    public virtual void Display()
    {
    }

    /// <summary>
    /// Gets the color of the pixel at the given coordinates.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual Color GetPointColor(int x, int y)
    {
        if (_buffer == null)
        {
            return Color.Black;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Color.Black;
        }

        return Color.FromArgb(_buffer[y * Width + x]);
    }

    /// <summary>
    /// Gets the color of the pixel at the given coordinates in ARGB.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual int GetRawPointColor(int x, int y)
    {
        if (_buffer == null)
        {
            return 0;
        }

        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return 0;
        }

        return _buffer[y * Width + x];
    }

    /// <summary>
    /// Gets the raw pixel buffer of this canvas. Returns null for hardware-backed canvases.
    /// </summary>
    public int[]? GetBuffer() => _buffer;

    internal int GetPointOffset(int x, int y)
    {
        return (x * _stride) + (y * _pitch);
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
    /// Draws an array of pixels to the canvas, starting at the given coordinates,
    /// using the given width.
    /// </summary>
    /// <param name="colors">The pixels to draw.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width of the drawn bitmap.</param>
    /// <param name="height">The height of the drawn bitmap.</param>
    public virtual void DrawArray(int[] colors, int x, int y, int width, int height)
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
    /// Draws an array of pixels to the canvas, starting at the given coordinates,
    /// using the given width.
    /// </summary>
    /// <param name="colors">The pixels to draw.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width of the drawn bitmap.</param>
    /// <param name="height">The height of the drawn bitmap.</param>
    /// <param name="startIndex">int[] colors starting position</param>
    public virtual void DrawArray(int[] colors, int x, int y, int width, int height, int startIndex)
    {
        for (int X = 0; X < width; X++)
        {
            for (int Y = 0; Y < height; Y++)
            {
                DrawPoint(colors[Y * width + X + startIndex], x + X, y + Y);
            }
        }
    }

    /// <summary>
    /// Draws another canvas onto this one at the specified position.
    /// </summary>
    /// <param name="canvas">The source canvas to draw.</param>
    /// <param name="x">The X coordinate on this canvas.</param>
    /// <param name="y">The Y coordinate on this canvas.</param>
    public virtual void DrawCanvas(Canvas canvas, int x, int y)
    {
        var srcBuffer = canvas.GetBuffer();
        int srcWidth = canvas.Width;
        int srcHeight = canvas.Height;

        if (srcBuffer != null)
        {
            DrawArray(srcBuffer, x, y, srcWidth, srcHeight);
        }
        else
        {
            for (int sx = 0; sx < srcWidth; sx++)
            {
                for (int sy = 0; sy < srcHeight; sy++)
                {
                    DrawPoint(canvas.GetRawPointColor(sx, sy), x + sx, y + sy);
                }
            }
        }
    }

    /// <summary>
    /// Copies a rectangle of pixels from one position on the canvas to
    /// another. The rectangle is clipped so that both the source and the
    /// destination stay within the canvas bounds. Overlapping regions copy
    /// correctly and without an intermediate buffer: the copy walks away
    /// from the overlap, the same rule as <c>memmove</c>.
    /// </summary>
    /// <param name="srcX">The X coordinate of the source rectangle.</param>
    /// <param name="srcY">The Y coordinate of the source rectangle.</param>
    /// <param name="dstX">The X coordinate of the destination rectangle.</param>
    /// <param name="dstY">The Y coordinate of the destination rectangle.</param>
    /// <param name="width">The width of the rectangle in pixels.</param>
    /// <param name="height">The height of the rectangle in pixels.</param>
    public virtual void CopyPixels(int srcX, int srcY, int dstX, int dstY, int width, int height)
    {
        int left = Math.Max(0, Math.Max(-srcX, -dstX));
        int top = Math.Max(0, Math.Max(-srcY, -dstY));
        int right = Math.Min(width, Math.Min(Width - srcX, Width - dstX));
        int bottom = Math.Min(height, Math.Min(Height - srcY, Height - dstY));

        if (left >= right || top >= bottom)
        {
            return;
        }

        int copyWidth = right - left;
        int copyHeight = bottom - top;
        int sourceX = srcX + left;
        int sourceY = srcY + top;
        int destinationX = dstX + left;
        int destinationY = dstY + top;

        // When the destination sits below the source, writing row r of the
        // destination lands on a source row not yet read, so rows go
        // bottom-up. Columns only interfere when both rectangles share their
        // rows; then a destination to the right is written right-to-left.
        bool rowsBackward = destinationY > sourceY;
        bool columnsBackward = destinationY == sourceY && destinationX > sourceX;

        for (int r = 0; r < copyHeight; r++)
        {
            int row = rowsBackward ? copyHeight - 1 - r : r;

            for (int c = 0; c < copyWidth; c++)
            {
                int column = columnsBackward ? copyWidth - 1 - c : c;
                DrawPoint(GetRawPointColor(sourceX + column, sourceY + row), destinationX + column, destinationY + row);
            }
        }
    }

    /// <summary>
    /// Moves a single pixel to a new position and clears its old position to
    /// black.
    /// </summary>
    /// <param name="x">The X coordinate of the pixel.</param>
    /// <param name="y">The Y coordinate of the pixel.</param>
    /// <param name="newX">The X coordinate to move the pixel to.</param>
    /// <param name="newY">The Y coordinate to move the pixel to.</param>
    public virtual void MovePixel(int x, int y, int newX, int newY)
    {
        CopyPixels(x, y, newX, newY, 1, 1);
        DrawPoint(0, x, y);
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

        // The loops below step before they draw, so they end on (x2, y2) and
        // would skip the start. Paint it here to keep both endpoints, matching
        // the axis-aligned paths.
        DrawPoint(color, px, py);

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
    /// <remarks>
    /// Both endpoints are painted, so a line from x to x is one pixel.
    /// </remarks>
    public virtual void DrawLine(Color color, int x1, int y1, int x2, int y2)
    {
        // Trim the given line to fit inside the canvas boundaries
        TrimLine(ref x1, ref y1, ref x2, ref y2);

        var dx = x2 - x1; // The horizontal distance of the line
        var dy = y2 - y1; // The vertical distance of the line

        if (dy == 0) // The line is horizontal
        {
            // Both endpoints are painted, so the run is the distance plus one.
            // DrawHorizontalLine only walks in the positive direction; start
            // from the leftmost point so right-to-left lines are not dropped.
            DrawHorizontalLine(color, Math.Abs(dx) + 1, Math.Min(x1, x2), y1);
            return;
        }

        if (dx == 0) // The line is vertical
        {
            // Same as above: start from the topmost point.
            DrawVerticalLine(color, Math.Abs(dy) + 1, x1, Math.Min(y1, y2));
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
    public virtual void DrawFilledEllipse(Color color, int xCenter, int yCenter, int xR, int yR)
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
    /// <param name="color">The color of the arc.</param>
    /// <param name="xCenter">The X coordinate of the arc's center.</param>
    /// <param name="yCenter">The Y coordinate of the arc's center.</param>
    /// <param name="xR">The X radius of the arc.</param>
    /// <param name="yR">The Y radius of the arc.</param>
    /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
    /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
    public virtual void DrawArc(Color color, int xCenter, int yCenter, int xR, int yR, int startAngle = 0, int endAngle = 360)
    {
        if (xR == 0 || yR == 0)
        {
            return;
        }

        for (double angle = startAngle; angle < endAngle; angle += 0.5)
        {
            double angleRadians = Math.PI * angle / 180;
            int IX = (int)(xR * Math.Cos(angleRadians));
            int IY = (int)(yR * Math.Sin(angleRadians));
            DrawPoint(color, xCenter + IX, yCenter + IY);
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
    /// Draws the outline of a rectangle, covering the same area
    /// <see cref="DrawFilledRectangle"/> fills for the same arguments.
    /// </summary>
    /// <param name="color">The color to draw with.</param>
    /// <param name="x">The X coordinate of the top-left corner.</param>
    /// <param name="y">The Y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle in pixels.</param>
    /// <param name="height">The height of the rectangle in pixels.</param>
    public virtual void DrawRectangle(Color color, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

        // The far edges sit on the last covered pixel, not one past it, so the
        // outline covers the same width x height area DrawFilledRectangle fills.
        int right = x + width - 1;
        int bottom = y + height - 1;

        DrawLine(color, x, y, right, y);
        DrawLine(color, x, y, x, bottom);
        DrawLine(color, x, bottom, right, bottom);
        DrawLine(color, right, y, right, bottom);
    }

    /// <summary>
    /// Draws a filled rectangle.
    /// </summary>
    /// <param name="color">The color to draw the rectangle with.</param>
    /// <param name="xStart">The X coordinate of the top-left corner.</param>
    /// <param name="yStart">The Y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle in pixels.</param>
    /// <param name="height">The height of the rectangle in pixels, or -1 to
    /// reuse <paramref name="width"/> and draw a square.</param>
    /// <remarks>
    /// The shape is always clipped to the canvas. There is no opt-out, because
    /// the hardware canvases fill straight into video memory and an unclipped
    /// origin there is a write outside the framebuffer, not a stray pixel.
    /// </remarks>
    public virtual void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
    {
        if (height == -1)
        {
            height = width;
        }

        // Clip both corners, not just the far one: a negative origin used to
        // walk rows above the canvas and draw each one from a negative X.
        if (xStart < 0)
        {
            width += xStart;
            xStart = 0;
        }

        if (yStart < 0)
        {
            height += yStart;
            yStart = 0;
        }

        width = Math.Min(width, Mode.Width - xStart);
        height = Math.Min(height, Mode.Height - yStart);

        if (width <= 0 || height <= 0)
        {
            return;
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
    /// <param name="preventOffBoundPixels">Prevents drawing outside the bounds of the canvas.</param>
    public virtual void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        Color color;
        if (preventOffBoundPixels)
        {
            int maxWidth = Math.Min(image.Width, Width - x);
            int maxHeight = Math.Min(image.Height, Height - y);
            for (int xi = 0; xi < maxWidth; xi++)
            {
                for (int yi = 0; yi < maxHeight; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }
        else
        {
            for (int xi = 0; xi < image.Width; xi++)
            {
                for (int yi = 0; yi < image.Height; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }
    }

    /// <summary>
    /// Creates a bitmap by copying a portion of your canvas from the specified coordinates and dimensions.
    /// </summary>
    /// <param name="x">The starting X coordinate of the region to copy.</param>
    /// <param name="y">The starting Y coordinate of the region to copy.</param>
    /// <param name="width">The width of the region to copy.</param>
    /// <param name="height">The height of the region to copy.</param>
    /// <returns>A new <see cref="Bitmap"/> containing the copied region.</returns>
    public virtual Bitmap GetImage(int x, int y, int width, int height)
    {
        Bitmap bitmap = new Bitmap(width, height, ColorDepth.ColorDepth32);

        for (int posy = 0; posy < height; posy++)
        {
            for (int posx = 0; posx < width; posx++)
            {
                bitmap.RawData[posy * width + posx] = GetRawPointColor(x + posx, y + posy);
            }
        }
        return bitmap;
    }

    /// <summary>
    /// Scales an image to the specified new width and height.
    /// </summary>
    /// <param name="image">The image to be scaled.</param>
    /// <param name="newWidth">The width of the scaled image.</param>
    /// <param name="newHeight">The height of the scaled image.</param>
    /// <returns>An array of integers representing the scaled image's pixel data. (Raw bitmap data)</returns>
    static int[] ScaleImage(Image image, int newWidth, int newHeight)
    {
        int[] pixels = image.RawData;
        int w1 = image.Width;
        int h1 = image.Height;
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
    /// <param name="preventOffBoundPixels">Prevents drawing outside the bounds of the canvas.</param>
    public virtual void DrawImage(Image image, int x, int y, int w, int h, bool preventOffBoundPixels = true)
    {
        Color color;

        int[] pixels = ScaleImage(image, w, h);
        if (preventOffBoundPixels)
        {
            var maxWidth = Math.Min(w, Mode.Width - x);
            var maxHeight = Math.Min(h, Mode.Height - y);
            for (int xi = 0; xi < maxWidth; xi++)
            {
                for (int yi = 0; yi < maxHeight; yi++)
                {
                    color = Color.FromArgb(pixels[xi + (yi * w)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }
        else
        {
            for (int xi = 0; xi < w; xi++)
            {
                for (int yi = 0; yi < h; yi++)
                {
                    color = Color.FromArgb(pixels[xi + (yi * w)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }
    }

    /// <summary>
    /// Draws the given image at the specified coordinates, cropping the image to fit within the maximum width and height.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    /// <param name="x">The X coordinate where the image will be drawn.</param>
    /// <param name="y">The Y coordinate where the image will be drawn.</param>
    /// <param name="maxWidth">The maximum width to display the image. If the image exceeds this width, it will be cropped.</param>
    /// <param name="maxHeight">The maximum height to display the image. If the image exceeds this height, it will be cropped.</param>
    /// <param name="preventOffBoundPixels">Prevents drawing outside the bounds of the canvas.</param>
    public virtual void CroppedDrawImage(Image image, int x, int y, int maxWidth, int maxHeight, bool preventOffBoundPixels = true)
    {
        Color color;
        int width = Math.Min(image.Width, maxWidth);
        int height = Math.Min(image.Height, maxHeight);
        int[] pixels = image.RawData;

        for (int xi = 0; xi < width; xi++)
        {
            for (int yi = 0; yi < height; yi++)
            {
                color = Color.FromArgb(pixels[xi + (yi * image.Width)]);
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
    /// <param name="preventOffBoundPixels">Prevents drawing outside the bounds of the canvas.</param>
    public void DrawImageAlpha(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        Color color;
        if (preventOffBoundPixels)
        {
            int maxWidth = Math.Min(image.Width, Width - x);
            int maxHeight = Math.Min(image.Height, Height - y);
            for (int xi = 0; xi < maxWidth; xi++)
            {
                for (int yi = 0; yi < maxHeight; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
            }
        }
        else
        {
            for (int xi = 0; xi < image.Width; xi++)
            {
                for (int yi = 0; yi < image.Height; yi++)
                {
                    color = Color.FromArgb(image.RawData[xi + (yi * image.Width)]);
                    DrawPoint(color, x + xi, y + yi);
                }
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
        ArgumentNullException.ThrowIfNull(font);

        if (font is TrueTypeFont trueType)
        {
            DrawString(str, trueType, trueType.SizePx, color, x, y);
            return;
        }

        var len = str.Length;
        var width = font.Width;

        for (int i = 0; i < len; i++)
        {
            DrawChar(str[i], font, color, x, y);
            x += width;
        }
    }

    /// <summary>
    /// Draws a string using the given TrueType font, with per-glyph advances,
    /// kerning and anti-aliased edges blended onto the existing pixels.
    /// </summary>
    /// <param name="str">The string to draw.</param>
    /// <param name="font">The TrueType font to use.</param>
    /// <param name="sizePx">The text size in pixels.</param>
    /// <param name="color">The color to write the string with.</param>
    /// <param name="x">The origin X coordinate (left edge of the text).</param>
    /// <param name="y">The origin Y coordinate (top of the text line).</param>
    public virtual void DrawString(string str, TrueTypeFont font, int sizePx, Color color, int x, int y)
    {
        int ascent = font.GetAscent(sizePx);
        int penX = x;
        char previous = '\0';

        for (int i = 0; i < str.Length; i++)
        {
            char c = str[i];
            TrueTypeGlyph? glyph = font.GetGlyph(c, sizePx);
            if (glyph == null)
            {
                continue;
            }

            if (previous != '\0')
            {
                penX += font.GetKerning(previous, c, sizePx);
            }

            DrawGlyph(glyph, color, penX + glyph.OffsetX, y + ascent + glyph.OffsetY);
            penX += glyph.Advance;
            previous = c;
        }
    }

    /// <summary>
    /// Blends a rasterized TrueType glyph onto the canvas, tinting its
    /// coverage with the given color.
    /// </summary>
    /// <param name="glyph">The rasterized glyph to draw.</param>
    /// <param name="color">The color to tint the glyph with.</param>
    /// <param name="x">The X coordinate of the left edge of the glyph bitmap.</param>
    /// <param name="y">The Y coordinate of the top edge of the glyph bitmap.</param>
    private void DrawGlyph(TrueTypeGlyph glyph, Color color, int x, int y)
    {
        byte[]? coverage = glyph.Coverage;
        if (coverage == null)
        {
            return;
        }

        for (int gy = 0; gy < glyph.Height; gy++)
        {
            int py = y + gy;
            if (py < 0 || py >= Mode.Height)
            {
                continue;
            }

            for (int gx = 0; gx < glyph.Width; gx++)
            {
                int px = x + gx;
                if (px < 0 || px >= Mode.Width)
                {
                    continue;
                }

                byte alpha = (byte)(coverage[(gy * glyph.Width) + gx] * color.A / 255);
                if (alpha == 0)
                {
                    continue;
                }

                DrawPoint(Color.FromArgb(alpha, color.R, color.G, color.B), px, py);
            }
        }
    }

    /// <summary>
    /// Draws a single character using the given bitmap font.
    /// </summary>
    /// <param name="c">The character to draw.</param>
    /// <param name="font">The bitmap font to use.</param>
    /// <param name="color">The color to write the character with.</param>
    /// <param name="x">The origin X coordinate.</param>
    /// <param name="y">The origin Y coordinate.</param>
    public virtual void DrawChar(char c, Font font, Color color, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(font);

        if (font is TrueTypeFont trueType)
        {
            DrawString(c.ToString(), trueType, trueType.SizePx, color, x, y);
            return;
        }

        var height = font.Height;
        var width = font.Width;
        var data = font.Data;
        int bytesPerRow = (width + 7) / 8;
        int p = height * bytesPerRow * (byte)c;

        for (int cy = 0; cy < height; cy++)
        {
            for (byte cx = 0; cx < width; cx++)
            {
                byte byteValue = data[p + (cy * bytesPerRow) + (cx / 8)];
                if (font.ConvertByteToBitAddress(byteValue, (cx % 8) + 1))
                {
                    DrawPoint(color, x + cx, y + cy);
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
    /// Clamps the line's two endpoints so the segment fits inside the canvas
    /// boundaries, preserving its slope.
    /// </summary>
    /// <param name="x1">The start point X coordinate.</param>
    /// <param name="y1">The start point Y coordinate.</param>
    /// <param name="x2">The end point X coordinate.</param>
    /// <param name="y2">The end point Y coordinate.</param>
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
    /// Blends <paramref name="to"/> over <paramref name="from"/> at the given
    /// <paramref name="alpha"/>. At alpha 255 the result is
    /// <paramref name="to"/>, at 0 it is <paramref name="from"/>.
    /// </summary>
    /// <param name="to">The color being laid on, weighted by <paramref name="alpha"/>.</param>
    /// <param name="from">The color already there, weighted by the remainder.</param>
    /// <param name="alpha">The opacity of <paramref name="to"/>, 0 to 255.</param>
    public static Color AlphaBlend(Color to, Color from, byte alpha)
    {
        byte R = (byte)(((to.R * alpha) + (from.R * (255 - alpha))) >> 8);
        byte G = (byte)(((to.G * alpha) + (from.G * (255 - alpha))) >> 8);
        byte B = (byte)(((to.B * alpha) + (from.B * (255 - alpha))) >> 8);
        return Color.FromArgb(R, G, B);
    }
}
