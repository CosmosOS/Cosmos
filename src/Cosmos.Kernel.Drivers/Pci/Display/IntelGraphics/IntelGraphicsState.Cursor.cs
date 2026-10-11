// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.IntelGraphics;

/// <summary>
/// The hardware cursor: each pipe's cursor plane fetches a square
/// premultiplied ARGB image of 64, 128 or 256 pixels a side from stolen
/// memory and blends it over the plane. An image smaller than the square is
/// padded with transparent pixels. Two images alternate, so a new image is
/// written while the pipe still fetches the old one and takes effect with
/// its base at the next vertical blank. The hardware takes a position off
/// the top or left edge as it is, sign and magnitude, and clips the image;
/// a cursor wholly off screen is turned off. The display buffer and the
/// watermarks the cursor needs are set up on the first image, and when
/// that fails the display reports no cursor from then on.
/// </summary>
internal sealed partial class IntelGraphicsState
{
    /// <summary>The largest cursor side the display engine fetches, in pixels.</summary>
    internal const int MaxCursorSize = 256;

    /// <summary>Bytes of one cursor image at the largest size.</summary>
    internal const ulong CursorImageBytes = MaxCursorSize * MaxCursorSize * sizeof(uint);

    /// <summary>The smallest cursor side, in pixels; the others are 128 and 256.</summary>
    private const int MinCursorSize = 64;

    private readonly DeviceRegion[] _cursorImages;
    private readonly ulong[] _cursorAddresses;
    private bool _cursorPrepared;
    private bool _cursorFailed;
    private bool _cursorDefined;
    private int _cursorImage;
    private int _cursorSize;
    private int _cursorWidth;
    private int _cursorHeight;
    private int _cursorHotSpotX;
    private int _cursorHotSpotY;
    private int _cursorX;
    private int _cursorY;
    private bool _cursorVisible;

    /// <summary>Whether stolen memory had room for the cursor images and the display buffer took a cursor. Any context.</summary>
    internal bool HasCursor => _cursorImages.Length > 0 && !_cursorFailed;

    /// <summary>
    /// Writes the image into the cursor image the pipes are not fetching and
    /// places the cursor again with the new hot spot. The first image readies
    /// the cursor's display buffer and watermarks. Thread context.
    /// </summary>
    /// <param name="hotspotX">The hotspot's column within the image.</param>
    /// <param name="hotspotY">The hotspot's row within the image.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="pixels">The pixels, width times height of them, premultiplied ARGB.</param>
    /// <returns>False without room for the cursor, when the display buffer could not take one, for an empty image, one larger than <see cref="MaxCursorSize"/> per side, a hotspot outside the image, or too few pixels; the previous image stays.</returns>
    public bool TryDefine(int hotspotX, int hotspotY, int width, int height, ReadOnlySpan<uint> pixels)
    {
        if (!HasCursor)
        {
            return false;
        }

        if (width <= 0 || height <= 0 || width > MaxCursorSize || height > MaxCursorSize)
        {
            return false;
        }

        if (hotspotX < 0 || hotspotY < 0 || hotspotX >= width || hotspotY >= height)
        {
            return false;
        }

        if ((long)width * height > pixels.Length || !TryPrepareCursor())
        {
            return false;
        }

        int size = CursorSize(Math.Max(width, height));
        int image = _cursorDefined ? _cursorImage ^ 1 : 0;
        Span<uint> target = _cursorImages[image].As<uint>();
        target.Slice(0, size * size).Clear();
        for (int row = 0; row < height; row++)
        {
            pixels.Slice(row * width, width).CopyTo(target.Slice(row * size, width));
        }

        _cursorImage = image;
        _cursorSize = size;
        _cursorWidth = width;
        _cursorHeight = height;
        _cursorHotSpotX = hotspotX;
        _cursorHotSpotY = hotspotY;
        _cursorDefined = true;
        PlaceCursor();
        return true;
    }

    /// <summary>Moves the cursor and shows or hides it; remembered until an image is defined. Thread context.</summary>
    /// <param name="x">The hotspot's column on the display.</param>
    /// <param name="y">The hotspot's row on the display.</param>
    /// <param name="visible">True to show the cursor, false to hide it.</param>
    public void Set(int x, int y, bool visible)
    {
        _cursorX = x;
        _cursorY = y;
        _cursorVisible = visible;
        PlaceCursor();
    }

    /// <summary>Turns the cursor off on every pipe, when an image was ever shown. Thread context, from the detach.</summary>
    private void HideCursor()
    {
        if (!_cursorDefined)
        {
            return;
        }

        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i].ProgramCursor(0, 0, 0, _cursorAddresses[_cursorImage]);
        }
    }

    /// <summary>Readies every pipe's display buffer and watermarks for the cursor, once; on failure the cursor stays off for good.</summary>
    private bool TryPrepareCursor()
    {
        if (_cursorPrepared)
        {
            return true;
        }

        for (int i = 0; i < _pipes.Length; i++)
        {
            if (!_pipes[i].TryPrepareCursorBuffer(out string problem))
            {
                _cursorFailed = true;
                _binding.Log($"no hardware cursor: {problem}");
                return false;
            }
        }

        _cursorPrepared = true;
        return true;
    }

    /// <summary>
    /// Programs the cursor on every pipe: shown, at the image's top left
    /// corner in the pipe's coordinates, on the pipes whose plane the image
    /// overlaps, and off everywhere once it is wholly off screen.
    /// </summary>
    private void PlaceCursor()
    {
        if (!_cursorDefined)
        {
            return;
        }

        int left = _cursorX - _cursorHotSpotX;
        int top = _cursorY - _cursorHotSpotY;
        bool shown = _cursorVisible
            && left > -_cursorWidth && top > -_cursorHeight
            && left < Mode.Width && top < Mode.Height;

        uint mode = _cursorSize switch
        {
            MinCursorSize => IntelGraphicsRegisters.CursorMode64,
            MinCursorSize * 2 => IntelGraphicsRegisters.CursorMode128,
            _ => IntelGraphicsRegisters.CursorMode256,
        };

        ulong address = _cursorAddresses[_cursorImage];
        for (int i = 0; i < _pipes.Length; i++)
        {
            IntelGraphicsPipe pipe = _pipes[i];
            bool onPipe = shown && left < pipe.Width && top < pipe.Height;
            uint control = onPipe ? pipe.CursorControlFor(mode) : 0;
            pipe.ProgramCursor(control, left + pipe.X, top + pipe.Y, address);
        }
    }

    /// <summary>The smallest cursor side the image fits: 64, 128 or 256 pixels.</summary>
    private static int CursorSize(int side)
    {
        int size = MinCursorSize;
        while (size < side)
        {
            size *= 2;
        }

        return size;
    }
}
