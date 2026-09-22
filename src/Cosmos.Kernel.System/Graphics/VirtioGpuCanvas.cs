// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.Core.Memory;
using Cosmos.Kernel.HAL.Devices.Graphic.Virtio;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// virtio-gpu canvas implementation, 2D path. Backed by a host 2D resource
/// attached to a physically-contiguous guest framebuffer; every primitive
/// writes through to the framebuffer, and Display() pushes the dirty rect to
/// the host via TRANSFER_TO_HOST_2D + RESOURCE_FLUSH.
/// Not a hardware-passthrough canvas: there is no virgl / OpenGL path.
/// </summary>
internal unsafe class VirtioGpuCanvas : Canvas
{
    private static readonly Mode s_defaultMode = new(1024, 768, ColorDepth.ColorDepth32);

    private readonly VirtioGpu _driver;
    private Mode _mode;

    /// <summary>
    /// Creates a canvas on the given virtio-gpu driver. The driver must already
    /// be initialized; the canvas adopts the scanout geometry the driver
    /// negotiated (which the device reported via GET_DISPLAY_INFO).
    /// </summary>
    internal VirtioGpuCanvas(VirtioGpu driver)
        : this(driver, new Mode((int)driver.Width, (int)driver.Height, ColorDepth.ColorDepth32))
    {
    }

    internal VirtioGpuCanvas(VirtioGpu driver, Mode mode)
        : base(mode)
    {
        _driver = driver;
        // The driver's framebuffer is sized to the real scanout, so adopt it
        // instead of the requested mode (mirrors GopCanvas's stance).
        _mode = new Mode((int)driver.Width, (int)driver.Height, mode.ColorDepth);
    }

    public override string Name => "VirtioGpu";

    public override Mode Mode
    {
        get => _mode;
        protected internal set
        {
            // virtio-gpu cannot reprogram the scanout after init (SET_SCANOUT
            // is sent once during Initialize); adopt the driver's geometry.
            _mode = new Mode((int)_driver.Width, (int)_driver.Height, value.ColorDepth);
        }
    }

    public override Mode DefaultGraphicsMode => s_defaultMode;

    public override IReadOnlyList<Mode> AvailableModes { get; } = new List<Mode>
    {
        new(320, 240, ColorDepth.ColorDepth32),
        new(640, 480, ColorDepth.ColorDepth32),
        new(800, 600, ColorDepth.ColorDepth32),
        new(1024, 768, ColorDepth.ColorDepth32),
        new(1280, 720, ColorDepth.ColorDepth32),
        new(1280, 1024, ColorDepth.ColorDepth32),
        new(1920, 1080, ColorDepth.ColorDepth32),
    };

    internal override void Disable()
    {
        _driver.Disable();
    }

    // --- Drawing ---

    public override void Clear(int aColor)
    {
        _driver.ClearScreen((uint)aColor);
    }

    public override void Clear(Color aColor)
    {
        _driver.ClearScreen((uint)aColor.ToArgb());
    }

    public override void DrawPoint(Color aColor, int aX, int aY)
    {
        if (aX < 0 || aX >= Width || aY < 0 || aY >= Height)
        {
            return;
        }

        if (aColor.A < 255)
        {
            if (aColor.A == 0)
            {
                return;
            }
            aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
        }

        _driver.DrawPixel((uint)aColor.ToArgb(), aX, aY);
    }

    public override void DrawPoint(uint aColor, int aX, int aY)
    {
        if (aX < 0 || aX >= Width || aY < 0 || aY >= Height)
        {
            return;
        }
        _driver.DrawPixel(aColor, aX, aY);
    }

    public override void DrawPoint(int aColor, int aX, int aY)
    {
        DrawPoint((uint)aColor, aX, aY);
    }

    public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
    {
        var pixels = new uint[aColors.Length];
        for (int i = 0; i < aColors.Length; i++)
        {
            pixels[i] = (uint)aColors[i].ToArgb();
        }
        _driver.CopyBuffer(pixels.AsMemory(), aX, aY, aWidth, aHeight);
    }

    public override void DrawArray(int[] aColors, int aX, int aY, int aWidth, int aHeight)
    {
        _driver.CopyBuffer(aColors.AsMemory(), aX, aY, aWidth, aHeight);
    }

    public override void DrawArray(int[] aColors, int aX, int aY, int aWidth, int aHeight, int startIndex)
    {
        _driver.CopyBuffer(aColors.AsMemory(startIndex), aX, aY, aWidth, aHeight);
    }

    public override void DrawFilledRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight)
    {
        if (aX < 0) { aWidth += aX; aX = 0; }
        if (aY < 0) { aHeight += aY; aY = 0; }
        if (aX >= Mode.Width || aY >= Mode.Height)
        {
            return;
        }

        aWidth = Math.Min(aWidth, Mode.Width - aX);
        aHeight = Math.Min(aHeight, Mode.Height - aY);
        if (aWidth <= 0 || aHeight <= 0)
        {
            return;
        }

        uint raw = (uint)aColor.ToArgb();
        // Fill row by row; the framebuffer pitch is width*4.
        for (int row = 0; row < aHeight; row++)
        {
            int dstByteOffset = (aY + row) * (int)_driver.Pitch + aX * 4;
            byte* fb = _driver.Framebuffer + dstByteOffset;
            MemoryOp.MemSet((uint*)fb, raw, aWidth);
        }
        // Bypasses ClearScreen/DrawPixel/CopyBuffer, so mark the rect dirty by hand.
        _driver.MarkDirty(aX, aY, aWidth, aHeight);
    }

    public override void DrawRectangle(Color color, int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return;
        }

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

        uint rawColor = (uint)color.ToArgb();
        for (int posX = x; posX <= right; posX++)
        {
            DrawPoint(rawColor, posX, y);
            DrawPoint(rawColor, posX, bottom);
        }
        for (int posY = y; posY <= bottom; posY++)
        {
            DrawPoint(rawColor, x, posY);
            DrawPoint(rawColor, right, posY);
        }
    }

    public override void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        int width = image.Width;
        int height = image.Height;
        var data = image.RawData;

        if (preventOffBoundPixels)
        {
            int maxWidth = Math.Min(width, (int)_mode.Width - x);
            int maxHeight = Math.Min(height, (int)_mode.Height - y);
            int startX = Math.Max(0, x);
            int startY = Math.Max(0, y);
            int sourceX = Math.Max(0, -x);
            int sourceY = Math.Max(0, -y);

            maxWidth -= startX - x;
            maxHeight -= startY - y;

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                return;
            }

            if (sourceX == 0 && sourceY == 0 && maxWidth == width && maxHeight == height)
            {
                _driver.CopyBuffer(data.AsMemory(), startX, startY, width, height);
            }
            else
            {
                for (int i = 0; i < maxHeight; i++)
                {
                    int sourceIndex = (sourceY + i) * width + sourceX;
                    _driver.CopyBuffer(data.AsMemory(sourceIndex, maxWidth), startX, startY + i, maxWidth, 1);
                }
            }
        }
        else
        {
            _driver.CopyBuffer(data.AsMemory(), x, y, width, height);
        }
    }

    public override void CroppedDrawImage(Image aImage, int aX, int aY, int aWidth, int aHeight, bool preventOffBoundPixels = true)
    {
        var xBitmap = aImage.RawData;
        var xWidth = aWidth;
        var xHeight = aHeight;

        if (preventOffBoundPixels)
        {
            int maxWidth = Math.Min(xWidth, Mode.Width - aX);
            int maxHeight = Math.Min(xHeight, Mode.Height - aY);
            int startX = Math.Max(0, aX);
            int startY = Math.Max(0, aY);
            int sourceX = Math.Max(0, -aX);
            int sourceY = Math.Max(0, -aY);

            maxWidth -= startX - aX;
            maxHeight -= startY - aY;

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                return;
            }

            if (sourceX == 0 && sourceY == 0 && maxWidth == xWidth && maxHeight == xHeight)
            {
                _driver.CopyBuffer(xBitmap.AsMemory(), startX, startY, xWidth, xHeight);
            }
            else
            {
                for (int i = 0; i < maxHeight; i++)
                {
                    int sourceIndex = (sourceY + i) * xWidth + sourceX;
                    _driver.CopyBuffer(xBitmap.AsMemory(sourceIndex, maxWidth), startX, startY + i, maxWidth, 1);
                }
            }
        }
        else
        {
            _driver.CopyBuffer(xBitmap.AsMemory(), aX, aY, xWidth, xHeight);
        }
    }

    public override void DrawCanvas(Canvas canvas, int x, int y)
    {
        var srcBuffer = canvas.GetBuffer();
        if (srcBuffer is not null)
        {
            _driver.CopyBuffer(srcBuffer.AsMemory(), x, y, canvas.Width, canvas.Height);
        }
        else
        {
            base.DrawCanvas(canvas, x, y);
        }
    }

    public override void Display()
    {
        _driver.Swap();
    }

    // --- Reading ---

    public override Color GetPointColor(int aX, int aY)
    {
        return Color.FromArgb((int)_driver.GetPixel(aX, aY));
    }

    public override int GetRawPointColor(int aX, int aY)
    {
        return (int)_driver.GetPixel(aX, aY);
    }

    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        Bitmap bitmap = new(width, height, ColorDepth.ColorDepth32);

        int startX = Math.Max(0, x);
        int startY = Math.Max(0, y);
        int endX = Math.Min(x + width, Mode.Width);
        int endY = Math.Min(y + height, Mode.Height);
        int offsetX = Math.Max(0, -x);
        int offsetY = Math.Max(0, -y);

        int[] rawData = new int[width * height];

        for (int posy = startY; posy < endY; posy++)
        {
            int srcOffset = (int)(posy * _driver.Pitch + startX * 4);
            int destOffset = (posy - startY + offsetY) * width + offsetX;
            _driver.GetVRAM(srcOffset, rawData, destOffset, endX - startX);
        }

        bitmap.RawData = rawData;
        return bitmap;
    }
}
