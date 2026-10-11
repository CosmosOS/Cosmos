// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Display;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Display.AmdDcn;

/// <summary>
/// The hardware cursor: the HUBP fetches a premultiplied ARGB image from
/// VRAM and the DPP blends it over the surface. Two images alternate, so a
/// new image is written while the pipe still fetches the old one and takes
/// effect with its address at the next vertical update. A cursor partly off
/// the top or left edge is placed at the edge with the hot spot moved into
/// the image by the clipped amount, the way amdgpu places it.
/// </summary>
internal sealed partial class AmdDcnState
{
    /// <summary>The largest cursor side DCN 3.1.5 fetches, in pixels.</summary>
    internal const int MaxCursorSize = 256;

    /// <summary>Bytes of one cursor image at the largest pitch.</summary>
    internal const ulong CursorImageBytes = MaxCursorSize * MaxCursorSize * sizeof(uint);

    /// <summary>The narrowest cursor pitch, in pixels; the others are 128 and 256.</summary>
    private const int MinCursorPitch = 64;

    private readonly DeviceRegion[] _cursorImages;
    private readonly ulong[] _cursorAddresses;
    private bool _cursorDefined;
    private int _cursorImage;
    private int _cursorWidth;
    private int _cursorHeight;
    private int _cursorHotSpotX;
    private int _cursorHotSpotY;
    private int _cursorX;
    private int _cursorY;
    private bool _cursorVisible;

    /// <summary>Whether VRAM had room for the cursor images. Any context.</summary>
    internal bool HasCursor => _cursorImages.Length > 0;

    /// <summary>
    /// Writes the image into the cursor image the pipe is not fetching and
    /// programs it, then places the cursor again with the new hot spot.
    /// Thread context.
    /// </summary>
    /// <param name="hotspotX">The hotspot's column within the image.</param>
    /// <param name="hotspotY">The hotspot's row within the image.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="pixels">The pixels, width times height of them, premultiplied ARGB.</param>
    /// <returns>False without room for the cursor in VRAM, for an empty image, one larger than <see cref="MaxCursorSize"/> per side, a hotspot outside the image, or too few pixels; the previous image stays.</returns>
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

        if ((long)width * height > pixels.Length)
        {
            return false;
        }

        int pitch = CursorPitch(width);
        int image = _cursorDefined ? _cursorImage ^ 1 : 0;
        Span<uint> target = _cursorImages[image].As<uint>();
        for (int row = 0; row < height; row++)
        {
            pixels.Slice(row * width, width).CopyTo(target.Slice(row * pitch, width));
        }

        uint pitchCode = pitch switch
        {
            MinCursorPitch => 0,
            MinCursorPitch * 2 => 1,
            _ => 2,
        };

        for (int i = 0; i < _pipes.Length; i++)
        {
            _pipes[i].ProgramCursorImage(_cursorAddresses[image], width, height, pitchCode, LinesPerChunk(width));
        }

        _cursorImage = image;
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

    /// <summary>
    /// Programs the position on every pipe, shown on the pipes whose viewport
    /// the image overlaps and hidden everywhere once it is fully off screen.
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

        int x = Math.Max(left, 0);
        int y = Math.Max(top, 0);
        int hotSpotX = left < 0 ? -left : 0;
        int hotSpotY = top < 0 ? -top : 0;
        for (int i = 0; i < _pipes.Length; i++)
        {
            AmdDcnPipe pipe = _pipes[i];
            bool onPipe = shown
                && left < pipe.ViewportX + pipe.ViewportWidth && left + _cursorWidth > pipe.ViewportX
                && top < pipe.ViewportY + pipe.ViewportHeight && top + _cursorHeight > pipe.ViewportY;
            pipe.ProgramCursorPosition(x, y, hotSpotX, hotSpotY, onPipe);
        }
    }

    /// <summary>The narrowest cursor pitch the width fits: 64, 128 or 256 pixels.</summary>
    private static int CursorPitch(int width)
    {
        int pitch = MinCursorPitch;
        while (pitch < width)
        {
            pitch *= 2;
        }

        return pitch;
    }

    /// <summary>CURSOR_LINES_PER_CHUNK for a 32-bit cursor of the width, as amdgpu's <c>hubp2_get_lines_per_chunk</c> picks it: 16, 8, 4 or 2 lines.</summary>
    private static uint LinesPerChunk(int width)
    {
        if (width <= 32)
        {
            return 4;
        }

        if (width <= 64)
        {
            return 3;
        }

        return width <= 128 ? 2u : 1u;
    }
}
