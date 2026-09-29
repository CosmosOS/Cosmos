//#define COSMOSDEBUG
using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.System.Graphics.Fonts;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// A drawing surface: a buffer of raw ARGB pixels, either off-screen or
/// attached to a display the driver kit published. Every primitive draws
/// into the buffer; a display-backed canvas copies it to the display's
/// framebuffer in <see cref="Display"/>, so it is double buffered and its
/// pixels read back exactly as drawn. Subclassed by <see cref="Canvas3D"/>
/// for a display that renders 3D.
/// </summary>
/// <remarks>
/// Every drawing primitive clips. A pixel outside 0..<see cref="Width"/>-1 by
/// 0..<see cref="Height"/>-1 is dropped, a shape that straddles an edge is drawn
/// up to it, and nothing throws for a coordinate, so coordinates never need
/// clamping before a call. A canvas whose display has no mode is zero-sized:
/// every primitive clips everything, and nothing throws.
/// </remarks>
public class Canvas
{
    /// <summary>The width of the mode a display is switched to when it reports none and lists this one.</summary>
    private const int DefaultWidth = 1024;

    /// <summary>The height of the mode a display is switched to when it reports none and lists this one.</summary>
    private const int DefaultHeight = 768;

    /// <summary>The only pixel depth the buffer is copied out at.</summary>
    private const int SupportedBitsPerPixel = 32;

    /// <summary>The refresh rate an off-screen canvas reports.</summary>
    private const int DefaultRefreshRate = 60;

    /// <summary>The name of an off-screen canvas.</summary>
    private const string OffScreenName = "Canvas";

    /// <summary>
    /// The pixel buffer, <see cref="Width"/> times <see cref="Height"/> raw
    /// ARGB values, always allocated and possibly empty.
    /// </summary>
    private int[] _buffer;

    /// <summary>The display this canvas draws on; null for an off-screen canvas.</summary>
    private readonly DisplayDevice? _display;

    /// <summary>The name reported by <see cref="Name"/>, fixed at construction.</summary>
    private readonly string _name;

    /// <summary>The mode the buffer is sized for.</summary>
    private Mode _mode;

    /// <summary>
    /// The modes this canvas accepts, built on first read and dropped when
    /// <see cref="Mode"/> is assigned, so reading it in a loop costs nothing.
    /// </summary>
    private Mode[]? _availableModes;

    /// <summary>Set once <see cref="Display"/> logged that the display's depth is not supported.</summary>
    private bool _unsupportedDepthLogged;

    /// <summary>
    /// Creates an off-screen (buffer-backed) canvas of the given size.
    /// </summary>
    /// <param name="width">The width of the canvas in pixels.</param>
    /// <param name="height">The height of the canvas in pixels.</param>
    /// <param name="colorDepth">The color depth (default 32-bit).</param>
    public Canvas(int width, int height, ColorDepth colorDepth = ColorDepth.ColorDepth32)
    {
        _mode = new Mode(width, height, colorDepth);
        _buffer = new int[width * height];
        _name = OffScreenName;
    }

    /// <summary>
    /// Attaches a canvas to a published display, switching it to
    /// <paramref name="requested"/> when the display can switch modes, or to
    /// its default mode when it reports none. The constructor calls no
    /// virtual member, so a subclass sees its own fields set before anything
    /// virtual runs; when the display's mode is still empty afterwards the
    /// canvas is zero-sized. Thread context.
    /// </summary>
    /// <param name="display">The display to draw on.</param>
    /// <param name="requested">The mode to switch to, or null to keep or default the display's mode.</param>
    /// <exception cref="ArgumentOutOfRangeException">The display can switch modes and refused <paramref name="requested"/>.</exception>
    internal Canvas(DisplayDevice display, Mode? requested)
    {
        ArgumentNullException.ThrowIfNull(display);

        _display = display;
        _name = display.DriverName + " " + display.Name;
        ApplyInitialMode(display, requested);
        _mode = ReadMode(display);
        _buffer = new int[_mode.Width * _mode.Height];
    }

    /// <summary>
    /// Attaches a canvas to a published display in the display's current
    /// mode, or its default mode when it reports none. For a
    /// <see cref="Canvas3D"/> in a driver package: the base has sized the
    /// buffer when the subclass constructor runs, and calls no virtual
    /// member. Thread context.
    /// </summary>
    /// <param name="display">The display to draw on.</param>
    protected Canvas(DisplayDevice display)
        : this(display, null)
    {
    }

    /// <summary>
    /// The graphics modes this canvas accepts: the display's list when it
    /// can switch modes (32-bit modes only), otherwise the one mode it is in.
    /// </summary>
    public virtual IReadOnlyList<Mode> AvailableModes => _availableModes ??= ListModes();

    /// <summary>
    /// The default graphics mode: 1024x768x32 when the display lists it, else
    /// the first listed mode, else the current mode.
    /// </summary>
    public virtual Mode DefaultGraphicsMode
    {
        get
        {
            IReadOnlyList<Mode> modes = AvailableModes;
            Mode preferred = new(DefaultWidth, DefaultHeight, ColorDepth.ColorDepth32);
            for (int i = 0; i < modes.Count; i++)
            {
                if (modes[i] == preferred)
                {
                    return preferred;
                }
            }

            return modes.Count > 0 ? modes[0] : Mode;
        }
    }

    /// <summary>
    /// The current mode. Setting it on an off-screen canvas resizes the
    /// buffer, discarding its contents. On a display-backed canvas the
    /// display is switched through its <see cref="IDisplayModes"/> facet when
    /// it has one, and the request is ignored otherwise; the canvas then
    /// re-reads the display's mode, resizes the buffer and raises
    /// <see cref="OnModeChanged"/>. Thread context.
    /// </summary>
    /// <remarks>
    /// The setter is not part of the ring: a kernel picks its mode when it
    /// acquires the canvas, through <see cref="GetFullScreen(Mode)"/> or the
    /// off-screen constructor.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The display can switch modes and refused the one assigned.</exception>
    public virtual Mode Mode
    {
        get => _mode;
        protected internal set
        {
            if (_display is null)
            {
                _mode = value;
            }
            else
            {
                if (_display.TryGetFacet(out IDisplayModes? modes))
                {
                    if (!modes.TrySetMode(value.Width, value.Height, (int)value.ColorDepth))
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), value, $"Mode {value} is not supported by display {_display.Name}");
                    }
                }

                _mode = ReadMode(_display);
            }

            _availableModes = null;
            ResizeBuffer();
            OnModeChanged();
        }
    }

    /// <summary>
    /// The width of this canvas in pixels.
    /// </summary>
    public int Width => _mode.Width;

    /// <summary>
    /// The height of this canvas in pixels.
    /// </summary>
    public int Height => _mode.Height;

    /// <summary>
    /// The refresh rate of the display in Hz, 60 when the display does not
    /// report one and for an off-screen canvas, 0 once the display is withdrawn.
    /// </summary>
    public virtual int RefreshRate => _display?.RefreshRate ?? DefaultRefreshRate;

    /// <summary>
    /// The name of the canvas: <c>Canvas</c> off-screen, otherwise the
    /// display's driver and name, as in <c>firmware framebuffer</c>.
    /// </summary>
    public virtual string Name => _name;

    /// <summary>
    /// Gets the full-screen canvas on the primary display, in the display's
    /// current mode or its default mode when it reports none. The first call
    /// builds it; later calls hand back the same canvas without resetting the
    /// mode, so <see cref="Mode"/> always reports the real screen size.
    /// <para>
    /// The display decides the canvas's type: a display that implements
    /// <see cref="ICanvas3DFactory"/> hands out its own <see cref="Canvas3D"/>,
    /// so a kernel that wants to render 3D tests the canvas with
    /// <c>is <see cref="Canvas3D"/></c>; on the firmware framebuffer every
    /// documented setup uses, it never is.
    /// </para>
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Graphics support is compiled out with CosmosEnableGraphics=false (test
    /// <see cref="KernelFeatures.Graphics"/> first to avoid it), or no display
    /// is published: the kernel has no framebuffer from the bootloader and no
    /// display driver bound a device.
    /// </exception>
    public static Canvas GetFullScreen()
    {
        return FullScreenCanvas.Get();
    }

    /// <summary>
    /// Gets the full-screen canvas on the primary display, switching the
    /// display to <paramref name="mode"/>. A display that cannot switch
    /// modes, the firmware framebuffer among them, ignores the request and
    /// the canvas keeps reporting the resolution the display is in.
    /// </summary>
    /// <param name="mode">
    /// The display mode to switch to; must be one of the canvas's
    /// <see cref="AvailableModes"/>.
    /// </param>
    /// <exception cref="InvalidOperationException">Graphics support is compiled out with CosmosEnableGraphics=false, or no display is published.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The display can switch modes and does not support <paramref name="mode"/>.</exception>
    public static Canvas GetFullScreen(Mode mode)
    {
        return FullScreenCanvas.Get(mode);
    }

    /// <summary>
    /// Drops the full-screen canvas, after its <see cref="Disable"/> released
    /// the device resources a 3D canvas holds, so a later
    /// <see cref="GetFullScreen()"/> builds a fresh canvas on the primary
    /// display. Any canvas already acquired is dead after this call. The
    /// display itself stays in its mode: there is no text mode to return to.
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
    /// Releases what the canvas holds on its device; the canvas is dead
    /// afterwards. The device half of <see cref="DisableFullScreen"/>, which
    /// is what a kernel calls. A no-op here: the buffer needs no release; a
    /// <see cref="Canvas3D"/> releases its contexts and surfaces in an
    /// override. Thread context.
    /// </summary>
    protected internal virtual void Disable()
    {
    }

    /// <summary>
    /// True when this canvas is attached to a display the kit has since
    /// withdrawn: <see cref="Display"/> then copies nothing, and the
    /// full-screen cache replaces the canvas on the next acquisition.
    /// </summary>
    internal bool IsDisplayWithdrawn => _display is not null && _display.IsWithdrawn;

    /// <summary>
    /// Called after <see cref="Mode"/> was assigned and the buffer resized,
    /// never from a constructor. A <see cref="Canvas3D"/> recreates its
    /// render targets here. Thread context.
    /// </summary>
    protected virtual void OnModeChanged()
    {
    }

    /// <summary>
    /// Sets the pixel at the given coordinates to the specified <paramref name="color"/>,
    /// blending it over the existing pixel when it is translucent.
    /// </summary>
    /// <param name="color">The color to draw with.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual void DrawPoint(Color color, int x, int y)
    {
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
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return;
        }

        _buffer[y * Width + x] = color;
    }

    /// <summary>
    /// Copies the buffer to the display and flushes it, so the frame becomes
    /// visible. A no-op for an off-screen canvas. The copy is clipped to the
    /// display's live mode and to its framebuffer's length, so a canvas whose
    /// buffer is larger than the mode the display is in now is clipped, never
    /// overrun; nothing is copied while the display is withdrawn, reports no
    /// mode, publishes no framebuffer, or is not 32 bits per pixel (logged
    /// once). Thread context.
    /// </summary>
    public virtual void Display()
    {
        if (_display is null || _display.IsWithdrawn)
        {
            return;
        }

        IDisplay display = _display.Display;
        DisplayMode mode = display.Mode;
        DeviceRegion? framebuffer = display.Framebuffer;
        if (mode.IsEmpty || framebuffer is null || framebuffer.Length < (ulong)mode.Height * (ulong)mode.Pitch)
        {
            return;
        }

        if (mode.BitsPerPixel != SupportedBitsPerPixel)
        {
            if (!_unsupportedDepthLogged)
            {
                _unsupportedDepthLogged = true;
                Serial.WriteString($"[Display] {Name}: {mode.BitsPerPixel} bits per pixel is not supported, nothing is drawn\n");
            }

            return;
        }

        int rowInts = mode.Pitch / sizeof(int);
        int rows = Math.Min(Height, mode.Height);
        int columns = Math.Min(Math.Min(Width, mode.Width), rowInts);
        if (rows <= 0 || columns <= 0)
        {
            return;
        }

        try
        {
            Span<int> target = framebuffer.As<int>();
            for (int row = 0; row < rows; row++)
            {
                _buffer.AsSpan(row * Width, columns).CopyTo(target.Slice(row * rowInts, columns));
            }

            if (_display.IsWithdrawn)
            {
                return;
            }

            display.Flush(0, 0, columns, rows);
        }
        catch (InvalidOperationException)
        {
            // The region, or a resource the driver's flush touches, was torn
            // down between the check and the copy: the display is going away,
            // and the next call sees it withdrawn.
            return;
        }
    }

    /// <summary>
    /// Gets the color of the pixel at the given coordinates, black outside the canvas.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual Color GetPointColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Color.Black;
        }

        return Color.FromArgb(_buffer[y * Width + x]);
    }

    /// <summary>
    /// Gets the color of the pixel at the given coordinates in ARGB, 0 outside the canvas.
    /// </summary>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    public virtual int GetRawPointColor(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return 0;
        }

        return _buffer[y * Width + x];
    }

    /// <summary>
    /// Gets the raw pixel buffer of this canvas, <see cref="Width"/> times
    /// <see cref="Height"/> ARGB values, row-major. Never null: a
    /// display-backed canvas is buffered too. The return type stays nullable
    /// for callers written against the earlier contract.
    /// </summary>
    public int[]? GetBuffer() => _buffer;

    /// <summary>
    /// Draws an array of pixels to the canvas, starting at the given
    /// coordinates, converting each color to ARGB and copying it raw: no blending.
    /// </summary>
    /// <param name="colors">The pixels to draw, row-major.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width of the drawn bitmap.</param>
    /// <param name="height">The height of the drawn bitmap.</param>
    public virtual void DrawArray(Color[] colors, int x, int y, int width, int height)
    {
        if (!ClipRectangle(x, y, width, height, out int left, out int top, out int columns, out int rows))
        {
            return;
        }

        for (int row = 0; row < rows; row++)
        {
            int sourceIndex = (top - y + row) * width + (left - x);
            int targetIndex = (top + row) * Width + left;
            for (int column = 0; column < columns; column++)
            {
                _buffer[targetIndex + column] = colors[sourceIndex + column].ToArgb();
            }
        }
    }

    /// <summary>
    /// Draws an array of raw ARGB pixels to the canvas, starting at the given
    /// coordinates, as row copies: no blending.
    /// </summary>
    /// <param name="colors">The pixels to draw, row-major.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width of the drawn bitmap.</param>
    /// <param name="height">The height of the drawn bitmap.</param>
    public virtual void DrawArray(int[] colors, int x, int y, int width, int height)
    {
        CopyRows(colors, 0, width, x, y, width, height);
    }

    /// <summary>
    /// Draws an array of raw ARGB pixels to the canvas, starting at the given
    /// coordinates, as row copies: no blending.
    /// </summary>
    /// <param name="colors">The pixels to draw, row-major.</param>
    /// <param name="x">The X coordinate.</param>
    /// <param name="y">The Y coordinate.</param>
    /// <param name="width">The width of the drawn bitmap.</param>
    /// <param name="height">The height of the drawn bitmap.</param>
    /// <param name="startIndex">The index in <paramref name="colors"/> of the first pixel.</param>
    public virtual void DrawArray(int[] colors, int x, int y, int width, int height, int startIndex)
    {
        CopyRows(colors, startIndex, width, x, y, width, height);
    }

    /// <summary>
    /// Draws another canvas onto this one at the specified position, as row
    /// copies of its buffer: no blending.
    /// </summary>
    /// <param name="canvas">The source canvas to draw.</param>
    /// <param name="x">The X coordinate on this canvas.</param>
    /// <param name="y">The Y coordinate on this canvas.</param>
    public virtual void DrawCanvas(Canvas canvas, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(canvas);

        CopyRows(canvas._buffer, 0, canvas.Width, x, y, canvas.Width, canvas.Height);
    }

    /// <summary>
    /// Copies a rectangle of pixels from one position on the canvas to
    /// another. The rectangle is clipped so that both the source and the
    /// destination stay within the canvas bounds. Overlapping regions copy
    /// correctly and without an intermediate buffer: the rows are copied in
    /// the order that walks away from the overlap, and within a row the copy
    /// follows the same rule as <c>memmove</c>.
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
        // bottom-up. Within a row Array.Copy handles the overlap itself.
        bool rowsBackward = destinationY > sourceY;

        for (int r = 0; r < copyHeight; r++)
        {
            int row = rowsBackward ? copyHeight - 1 - r : r;
            Array.Copy(_buffer, (sourceY + row) * Width + sourceX, _buffer, (destinationY + row) * Width + destinationX, copyWidth);
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

        int dxabs = Math.Abs(dx);
        int dyabs = Math.Abs(dy);
        int sdx = Math.Sign(dx);
        int sdy = Math.Sign(dy);
        int x = dyabs >> 1;
        int y = dxabs >> 1;
        int px = x1;
        int py = y1;

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

        int dx = x2 - x1; // The horizontal distance of the line
        int dy = y2 - y1; // The vertical distance of the line

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
            throw new ArgumentException("A polygon requires at least 3 points.", nameof(points));
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            Point pointA = points[i];
            Point pointB = points[i + 1];
            DrawLine(color, pointA.X, pointA.Y, pointB.X, pointB.Y);
        }

        Point firstPoint = points[0];
        Point lastPoint = points[^1];
        DrawLine(color, firstPoint.X, firstPoint.Y, lastPoint.X, lastPoint.Y);
    }

    /// <summary>
    /// Draws the outline of a rectangle, covering the same area
    /// <see cref="DrawFilledRectangle"/> fills for the same arguments. An
    /// edge outside the canvas is dropped, not moved onto the border.
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

        if (color.A < 255)
        {
            DrawLine(color, x, y, right, y);
            DrawLine(color, x, y, x, bottom);
            DrawLine(color, x, bottom, right, bottom);
            DrawLine(color, right, y, right, bottom);
            return;
        }

        // An opaque outline goes through the clipping point primitive, which
        // drops a pixel whose row or column is outside the canvas; DrawLine
        // would clamp an off-screen edge onto the border instead. The loop
        // bounds only avoid walking far off-screen.
        int argb = color.ToArgb();
        int firstX = Math.Max(x, 0);
        int lastX = Math.Min(right, Width - 1);
        for (int posX = firstX; posX <= lastX; posX++)
        {
            DrawPoint(argb, posX, y);
            DrawPoint(argb, posX, bottom);
        }

        int firstY = Math.Max(y, 0);
        int lastY = Math.Min(bottom, Height - 1);
        for (int posY = firstY; posY <= lastY; posY++)
        {
            DrawPoint(argb, x, posY);
            DrawPoint(argb, right, posY);
        }
    }

    /// <summary>
    /// Draws a filled rectangle, as a raw fill of every clipped row: no blending.
    /// </summary>
    /// <param name="color">The color to draw the rectangle with.</param>
    /// <param name="xStart">The X coordinate of the top-left corner.</param>
    /// <param name="yStart">The Y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle in pixels.</param>
    /// <param name="height">The height of the rectangle in pixels, or -1 to
    /// reuse <paramref name="width"/> and draw a square.</param>
    /// <remarks>
    /// The shape is always clipped to the canvas, under the contract every
    /// primitive follows.
    /// </remarks>
    public virtual void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
    {
        if (height == -1)
        {
            height = width;
        }

        if (!ClipRectangle(xStart, yStart, width, height, out int left, out int top, out int columns, out int rows))
        {
            return;
        }

        int argb = color.ToArgb();
        for (int row = 0; row < rows; row++)
        {
            Array.Fill(_buffer, argb, (top + row) * Width + left, columns);
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
    /// Draws the given image at the specified coordinates, as row copies of
    /// its pixels: no blending. <see cref="DrawImageAlpha"/> blends.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    /// <param name="x">The origin X coordinate.</param>
    /// <param name="y">The origin Y coordinate.</param>
    /// <param name="preventOffBoundPixels">Kept for callers written against the earlier contract; the canvas always clips.</param>
    public virtual void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        ArgumentNullException.ThrowIfNull(image);

        CopyRows(image.RawData, 0, image.Width, x, y, image.Width, image.Height);
    }

    /// <summary>
    /// Creates a bitmap by copying a portion of your canvas from the specified coordinates and dimensions. Pixels outside the canvas read as 0.
    /// </summary>
    /// <param name="x">The starting X coordinate of the region to copy.</param>
    /// <param name="y">The starting Y coordinate of the region to copy.</param>
    /// <param name="width">The width of the region to copy.</param>
    /// <param name="height">The height of the region to copy.</param>
    /// <returns>A new <see cref="Bitmap"/> containing the copied region.</returns>
    public virtual Bitmap GetImage(int x, int y, int width, int height)
    {
        Bitmap bitmap = new(width, height, ColorDepth.ColorDepth32);

        if (!ClipRectangle(x, y, width, height, out int left, out int top, out int columns, out int rows))
        {
            return bitmap;
        }

        int[] target = bitmap.RawData;
        for (int row = 0; row < rows; row++)
        {
            Array.Copy(_buffer, (top + row) * Width + left, target, (top - y + row) * width + (left - x), columns);
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
    private static int[] ScaleImage(Image image, int newWidth, int newHeight)
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
    /// Draws a bitmap, applying scaling to the given image. The scaled pixels
    /// are drawn through <see cref="DrawPoint(Color, int, int)"/>, so a
    /// translucent pixel blends over what is there.
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
            int maxWidth = Math.Min(w, Width - x);
            int maxHeight = Math.Min(h, Height - y);
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
    /// Draws the given image at the specified coordinates, cropping the image
    /// to fit within the maximum width and height, as row copies of its
    /// pixels: no blending.
    /// </summary>
    /// <param name="image">The image to draw.</param>
    /// <param name="x">The X coordinate where the image will be drawn.</param>
    /// <param name="y">The Y coordinate where the image will be drawn.</param>
    /// <param name="maxWidth">The maximum width to display the image. If the image exceeds this width, it will be cropped.</param>
    /// <param name="maxHeight">The maximum height to display the image. If the image exceeds this height, it will be cropped.</param>
    /// <param name="preventOffBoundPixels">Kept for callers written against the earlier contract; the canvas always clips.</param>
    public virtual void CroppedDrawImage(Image image, int x, int y, int maxWidth, int maxHeight, bool preventOffBoundPixels = true)
    {
        ArgumentNullException.ThrowIfNull(image);

        int width = Math.Min(image.Width, maxWidth);
        int height = Math.Min(image.Height, maxHeight);
        CopyRows(image.RawData, 0, image.Width, x, y, width, height);
    }

    /// <summary>
    /// Draws an image with alpha blending: every pixel goes through
    /// <see cref="DrawPoint(Color, int, int)"/>, so a translucent pixel
    /// blends over what is there and a transparent one leaves it alone.
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

        int len = str.Length;
        int width = font.Width;

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
            if (glyph is null)
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
        if (coverage is null)
        {
            return;
        }

        for (int gy = 0; gy < glyph.Height; gy++)
        {
            int py = y + gy;
            if (py < 0 || py >= Height)
            {
                continue;
            }

            for (int gx = 0; gx < glyph.Width; gx++)
            {
                int px = x + gx;
                if (px < 0 || px >= Width)
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

        int height = font.Height;
        int width = font.Width;
        byte[] data = font.Data;
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
        foreach (Mode elem in AvailableModes)
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
            x1 = Math.Min(Width - 1, Math.Max(0, x1));
            x2 = x1;
            y1 = Math.Min(Height - 1, Math.Max(0, y1));
            y2 = Math.Min(Height - 1, Math.Max(0, y2));

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
        else if (x1Out >= Width)
        {
            x1Out = Width - 1;
            y1Out = ((Width - 1) * m) + c;
        }

        // handle x2
        if (x2Out < 0)
        {
            x2Out = 0;
            y2Out = c;
        }
        else if (x2Out >= Width)
        {
            x2Out = Width - 1;
            y2Out = ((Width - 1) * m) + c;
        }

        // handle y1
        if (y1Out < 0)
        {
            x1Out = -c / m;
            y1Out = 0;
        }
        else if (y1Out >= Height)
        {
            x1Out = (Height - 1 - c) / m;
            y1Out = Height - 1;
        }

        // handle y2
        if (y2Out < 0)
        {
            x2Out = -c / m;
            y2Out = 0;
        }
        else if (y2Out >= Height)
        {
            x2Out = (Height - 1 - c) / m;
            y2Out = Height - 1;
        }

        // final check, to avoid lines that are totally outside bounds
        if (x1Out < 0 || x1Out >= Width || y1Out < 0 || y1Out >= Height)
        {
            x1Out = 0; x2Out = 0;
            y1Out = 0; y2Out = 0;
        }

        if (x2Out < 0 || x2Out >= Width || y2Out < 0 || y2Out >= Height)
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

    /// <summary>
    /// Switches the display for a new canvas: to <paramref name="requested"/>
    /// when given, or to the default mode when the display reports none, in
    /// both cases only through the <see cref="IDisplayModes"/> facet; a
    /// display without it keeps its mode and a request is ignored, the
    /// documented behaviour of the firmware framebuffer. Static, so the
    /// constructor calls nothing virtual. Thread context.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The facet refused <paramref name="requested"/>.</exception>
    private static void ApplyInitialMode(DisplayDevice display, Mode? requested)
    {
        if (!display.TryGetFacet(out IDisplayModes? modes))
        {
            return;
        }

        if (requested is Mode mode)
        {
            if (!modes.TrySetMode(mode.Width, mode.Height, (int)mode.ColorDepth))
            {
                throw new ArgumentOutOfRangeException(nameof(requested), mode, $"Mode {mode} is not supported by display {display.Name}");
            }

            return;
        }

        if (display.Width == 0 || display.Height == 0)
        {
            DisplayMode fallback = DefaultModeOf(modes);
            if (!fallback.IsEmpty)
            {
                // A refusal leaves the canvas zero-sized, which is valid.
                modes.TrySetMode(fallback.Width, fallback.Height, fallback.BitsPerPixel);
            }
        }
    }

    /// <summary>
    /// The mode a display that reports none is switched to: 1024x768x32 when
    /// listed, else the first listed mode, else an empty mode.
    /// </summary>
    private static DisplayMode DefaultModeOf(IDisplayModes modes)
    {
        ReadOnlySpan<DisplayMode> listed = modes.Modes;
        for (int i = 0; i < listed.Length; i++)
        {
            DisplayMode candidate = listed[i];
            if (candidate.Width == DefaultWidth && candidate.Height == DefaultHeight && candidate.BitsPerPixel == SupportedBitsPerPixel)
            {
                return candidate;
            }
        }

        return listed.Length > 0 ? listed[0] : default;
    }

    /// <summary>
    /// The display's live mode as a canvas mode, read once from the contract
    /// object; 0x0 once the display is withdrawn or while it reports no mode.
    /// </summary>
    private static Mode ReadMode(DisplayDevice display)
    {
        if (display.IsWithdrawn)
        {
            return new Mode(0, 0, ColorDepth.ColorDepth32);
        }

        DisplayMode mode = display.Display.Mode;
        ColorDepth depth = mode.BitsPerPixel == 0 ? ColorDepth.ColorDepth32 : (ColorDepth)mode.BitsPerPixel;
        return mode.IsEmpty ? new Mode(0, 0, depth) : new Mode(mode.Width, mode.Height, depth);
    }

    /// <summary>
    /// The modes for <see cref="AvailableModes"/>: the 32-bit entries of the
    /// display's <see cref="IDisplayModes"/> list, or the current mode alone
    /// when there is no facet or the list holds no 32-bit mode.
    /// </summary>
    private Mode[] ListModes()
    {
        if (_display is not null && _display.TryGetFacet(out IDisplayModes? modes))
        {
            ReadOnlySpan<DisplayMode> listed = modes.Modes;
            int count = 0;
            for (int i = 0; i < listed.Length; i++)
            {
                if (listed[i].BitsPerPixel == SupportedBitsPerPixel)
                {
                    count++;
                }
            }

            if (count > 0)
            {
                Mode[] result = new Mode[count];
                int next = 0;
                for (int i = 0; i < listed.Length; i++)
                {
                    if (listed[i].BitsPerPixel == SupportedBitsPerPixel)
                    {
                        result[next++] = new Mode(listed[i].Width, listed[i].Height, ColorDepth.ColorDepth32);
                    }
                }

                return result;
            }
        }

        return [_mode];
    }

    /// <summary>
    /// Sizes the buffer to the current mode, discarding its contents: a new
    /// array when the pixel count changed, a clear when two modes of the same
    /// area differ only in shape (the rows would otherwise be read with a
    /// different stride).
    /// </summary>
    private void ResizeBuffer()
    {
        int length = _mode.Width * _mode.Height;
        if (_buffer.Length != length)
        {
            _buffer = new int[length];
            return;
        }

        Array.Clear(_buffer);
    }

    /// <summary>
    /// Clips a rectangle to the canvas. Returns false when nothing is left.
    /// </summary>
    /// <param name="x">The X coordinate of the top-left corner.</param>
    /// <param name="y">The Y coordinate of the top-left corner.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="left">The X coordinate of the clipped rectangle's first column.</param>
    /// <param name="top">The Y coordinate of the clipped rectangle's first row.</param>
    /// <param name="columns">The clipped width in pixels.</param>
    /// <param name="rows">The clipped height in pixels.</param>
    private bool ClipRectangle(int x, int y, int width, int height, out int left, out int top, out int columns, out int rows)
    {
        left = Math.Max(x, 0);
        top = Math.Max(y, 0);
        int right = Math.Min(x + width, Width);
        int bottom = Math.Min(y + height, Height);
        columns = right - left;
        rows = bottom - top;
        return columns > 0 && rows > 0;
    }

    /// <summary>
    /// Copies rows of raw ARGB pixels into the buffer at the given position,
    /// clipped to the canvas: one <see cref="Array.Copy(Array, int, Array, int, int)"/>
    /// per row, no blending.
    /// </summary>
    /// <param name="source">The pixels, row-major.</param>
    /// <param name="sourceIndex">The index in <paramref name="source"/> of the first pixel of the first row.</param>
    /// <param name="sourceStride">The number of pixels from one row of <paramref name="source"/> to the next.</param>
    /// <param name="x">The X coordinate the first column lands on.</param>
    /// <param name="y">The Y coordinate the first row lands on.</param>
    /// <param name="width">The number of pixels per row to copy.</param>
    /// <param name="height">The number of rows to copy.</param>
    private void CopyRows(int[] source, int sourceIndex, int sourceStride, int x, int y, int width, int height)
    {
        if (!ClipRectangle(x, y, width, height, out int left, out int top, out int columns, out int rows))
        {
            return;
        }

        int sourceStart = sourceIndex + ((top - y) * sourceStride) + (left - x);
        int targetStart = (top * Width) + left;
        for (int row = 0; row < rows; row++)
        {
            Array.Copy(source, sourceStart + (row * sourceStride), _buffer, targetStart + (row * Width), columns);
        }
    }
}
