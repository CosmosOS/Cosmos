using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Graphic;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Defines a EFI canvas implementation. Please note
/// that this implementation of <see cref="Canvas"/> only works on UEFI
/// implementations, meaning that it is not available on BIOS systems.
/// </summary>
internal class GopCanvas : Canvas
{
    static readonly Mode defaultMode = new(1024, 768, ColorDepth.ColorDepth32);
    Mode mode;
    int _refreshRate = 60;

    public override int RefreshRate => _refreshRate;

    /// <summary>
    /// Initializes a new instance of the <see cref="GopCanvas"/> class.
    /// </summary>
    internal GopCanvas() : this(defaultMode)
    {
    }

    GopDriver driver;

    /// <summary>
    /// Initializes a new instance of the <see cref="GopCanvas"/> class.
    /// </summary>
    /// <param name="mode">The display mode to use.</param>
    internal unsafe GopCanvas(Mode mode) : base(mode)
    {
        // The requested mode is only validated when there is no Limine framebuffer
        // to adopt (see Mode setter): with a live framebuffer the request is
        // ignored anyway, and the AvailableModes list must not reject callers who
        // pass the framebuffer's true resolution (which may not be listed).
        if (Limine.Framebuffer.Response != null && Limine.Framebuffer.Response->FramebufferCount > 0)
        {
            LimineFramebuffer* fb = Limine.Framebuffer.Response->Framebuffers[0];
            driver = new GopDriver((uint*)fb->Address, (uint)fb->Width, (uint)fb->Height, (uint)fb->Pitch);

            // Update mode to match actual framebuffer resolution
            this.mode = new Mode((uint)fb->Width, (uint)fb->Height, mode.ColorDepth);

            _refreshRate = ParseEdidRefreshRate(fb);
        }
        else
        {
            Mode = mode;
        }
    }

    private static unsafe int ParseEdidRefreshRate(LimineFramebuffer* fb)
    {
        if (fb->EdidSize < 128 || fb->Edid == null)
        {
            return 60;
        }

        byte* edid = (byte*)fb->Edid;

        // Validate EDID header: 00 FF FF FF FF FF FF 00
        if (edid[0] != 0x00 || edid[1] != 0xFF || edid[7] != 0x00)
        {
            return 60;
        }

        // First detailed timing descriptor starts at byte 54
        byte* dtd = edid + 54;

        // Pixel clock in 10 kHz units (bytes 0-1, little-endian). Zero means not a timing descriptor.
        uint pixelClock = (uint)(dtd[0] | (dtd[1] << 8));
        if (pixelClock == 0)
        {
            return 60;
        }

        uint hActive = (uint)(dtd[2] | ((dtd[4] >> 4) << 8));
        uint hBlank = (uint)(dtd[3] | ((dtd[4] & 0xF) << 8));
        uint vActive = (uint)(dtd[5] | ((dtd[7] >> 4) << 8));
        uint vBlank = (uint)(dtd[6] | ((dtd[7] & 0xF) << 8));

        uint hTotal = hActive + hBlank;
        uint vTotal = vActive + vBlank;

        if (hTotal == 0 || vTotal == 0)
        {
            return 60;
        }

        int hz = (int)((pixelClock * 10000) / (hTotal * vTotal));

        // Sanity check
        if (hz < 24 || hz > 360)
        {
            return 60;
        }

        return hz;
    }

    public override void Disable()
    {
        //driver.DisableDisplay();
    }

    public override string Name() => "GopCanvas";

    public override Mode Mode
    {
        get => mode;
        set
        {
            // GOP/Limine cannot change the framebuffer mode after boot (SetMode is
            // a no-op), so a requested mode must not shadow the real framebuffer
            // resolution: callers read Mode back to learn the actual screen size,
            // and adopting the request would make every consumer lay out its UI
            // for a resolution the hardware never switched to. The real
            // framebuffer size is also deliberately NOT checked against
            // AvailableModes — that legacy VBE list doesn't contain every mode
            // firmware can hand us (e.g. 1280x800).
            if (driver != null)
            {
                mode = new Mode(driver.Width, driver.Height, value.ColorDepth);
            }
            else
            {
                SetMode(value);
                mode = value;
            }
        }
    }

    #region Display
    /// <summary>
    /// Available EFI supported video modes.
    /// <para>
    /// Low res:
    /// <list type="bullet">
    /// <item>320x240x32.</item>
    /// <item>640x480x32.</item>
    /// <item>800x600x32.</item>
    /// <item>1024x768x32.</item>
    /// </list>
    /// </para>
    /// <para>
    /// HD:
    /// <list type="bullet">
    /// <item>1280x720x32.</item>
    /// <item>1280x1024x32.</item>
    /// </list>
    /// </para>
    /// <para>
    /// HDR:
    /// <list type="bullet">
    /// <item>1366x768x32.</item>
    /// <item>1680x1050x32.</item>
    /// </list>
    /// </para>
    /// <para>
    /// HDTV:
    /// <list type="bullet">
    /// <item>1920x1080x32.</item>
    /// <item>1920x1200x32.</item>
    /// </list>
    /// </para>
    /// </summary>
    public override List<Mode> AvailableModes { get; } = new()
    {
        new Mode(320, 240, ColorDepth.ColorDepth32),
        new Mode(640, 480, ColorDepth.ColorDepth32),
        new Mode(800, 600, ColorDepth.ColorDepth32),
        new Mode(1024, 768, ColorDepth.ColorDepth32),
        /* The so called HD-Ready resolution */
        new Mode(1280, 720, ColorDepth.ColorDepth32),
        new Mode(1280, 768, ColorDepth.ColorDepth32),
        new Mode(1280, 1024, ColorDepth.ColorDepth32),
        /* A lot of HD-Ready screen uses this instead of 1280x720 */
        new Mode(1366, 768, ColorDepth.ColorDepth32),
        new Mode(1680, 1050, ColorDepth.ColorDepth32),
        /* HDTV resolution */
        new Mode(1920, 1080, ColorDepth.ColorDepth32),
        /* HDTV resolution (16:10 AR) */
        new Mode(1920, 1200, ColorDepth.ColorDepth32),
    };

    public override Mode DefaultGraphicsMode => defaultMode;

    /// <summary>
    /// Sets the used display mode, disabling text mode if it is active.
    /// </summary>
    private void SetMode(Mode mode)
    {
        ThrowIfModeIsNotValid(mode);

        ushort xres = (ushort)Mode.Width;
        ushort yres = (ushort)Mode.Height;
        ushort bpp = (ushort)Mode.ColorDepth;

        //driver.VBESet(xres, yres, bpp);
    }
    #endregion

    #region Drawing

    public override void Clear(int aColor)
    {
        /*
            * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
            * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
            * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
            * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
            */
        switch (mode.ColorDepth)
        {
            case ColorDepth.ColorDepth4:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth8:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth16:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth24:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth32:
                driver.ClearScreen((uint)aColor);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public override void Clear(Color aColor)
    {
        /*
            * TODO this version of Clear() works only when mode.ColorDepth == ColorDepth.ColorDepth32
            * in the other cases you should before convert color and then call the opportune ClearVRAM() overload
            * (the one that takes ushort for ColorDepth.ColorDepth16 and the one that takes byte for ColorDepth.ColorDepth8)
            * For ColorDepth.ColorDepth24 you should mask the Alpha byte.
            */
        switch (mode.ColorDepth)
        {
            case ColorDepth.ColorDepth4:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth8:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth16:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth24:
                throw new NotImplementedException();
            case ColorDepth.ColorDepth32:
                driver.ClearScreen((uint)aColor.ToArgb());
                break;
            default:
                throw new NotImplementedException();
        }
    }

    /*
        * As DrawPoint() is the basic block of DrawLine() and DrawRect() and in theory of all the future other methods that will
        * be implemented is better to not check the validity of the arguments here or it will repeat the check for any point
        * to be drawn slowing down all.
        */
    public override void DrawPoint(Color aColor, int aX, int aY)
    {
        //uint offset;

        /*
            * For now we can Draw only if the ColorDepth is 32 bit, we will throw otherwise.
            *
            * How to support other ColorDepth? The offset calculation should be the same (and so could be done out of the switch)
            * ColorDepth.ColorDepth16 and ColorDepth.ColorDepth8 need a conversion from color (an ARGB32 color) to the RGB16 and RGB8
            * how to do this conversion faster maybe using pre-computed tables? What happens if the color cannot be converted? We will throw?
            */
        switch (Mode.ColorDepth)
        {
            case ColorDepth.ColorDepth32:

                //offset = (uint)GetPointOffset(aX, aY);

                if (aColor.A < 255)
                {
                    if (aColor.A == 0)
                    {
                        return;
                    }

                    aColor = AlphaBlend(aColor, GetPointColor(aX, aY), aColor.A);
                }

                driver.DrawPixel((uint)aColor.ToArgb(), aX, aY);

                break;
            case ColorDepth.ColorDepth24:

                //offset = (uint)GetPointOffset(aX, aY);

                driver.DrawPixel((uint)aColor.ToArgb(), aX, aY);

                break;
            default:
                throw new NotImplementedException("Drawing pixels with color depth " + (int)Mode.ColorDepth + "is not yet supported.");
        }
    }

    public override void DrawPoint(uint aColor, int aX, int aY)
    {
        //uint offset;

        switch (Mode.ColorDepth)
        {
            case ColorDepth.ColorDepth32:
                //offset = (uint)GetPointOffset(aX, aY);

                driver.DrawPixel(aColor, aX, aY);

                break;
            case ColorDepth.ColorDepth24:
                //offset = (uint)GetPointOffset(aX, aY);

                driver.DrawPixel(aColor, aX, aY);

                break;
            default:
                throw new NotImplementedException("Drawing pixels with color depth " + (int)Mode.ColorDepth + " is not yet supported.");
        }
    }

    public override void DrawPoint(int aColor, int aX, int aY)
    {
        DrawPoint((uint)aColor, aX, aY);
    }

    public override void DrawArray(Color[] aColors, int aX, int aY, int aWidth, int aHeight)
    {
        ThrowIfCoordNotValid(aX, aY);
        ThrowIfCoordNotValid(aX + aWidth, aY + aHeight);

        // Convert Color[] to uint[] for bulk copy
        var pixels = new uint[aColors.Length];
        for (int i = 0; i < aColors.Length; i++)
        {
            pixels[i] = (uint)aColors[i].ToArgb();
        }

        driver.CopyBuffer(pixels.AsMemory(), aX, aY, aWidth, aHeight);
    }

    public override void DrawArray(int[] aColors, int aX, int aY, int aWidth, int aHeight)
    {
        driver.CopyBuffer(aColors.AsMemory(), aX, aY, aWidth, aHeight);
    }

    public override void DrawArray(int[] aColors, int aX, int aY, int aWidth, int aHeight, int startIndex)
    {
        driver.CopyBuffer(aColors.AsMemory(startIndex), aX, aY, aWidth, aHeight);
    }

    public override void DrawFilledRectangle(Color aColor, int aX, int aY, int aWidth, int aHeight, bool preventOffBoundPixels = true)
    {
        // ClearVRAM clears one uint at a time. So we clear pixelwise not byte wise. That's why we divide by 32 and not 8.
        if (preventOffBoundPixels)
        {
            // Clamp to screen bounds
            if (aX < 0) { aWidth += aX; aX = 0; }
            if (aY < 0) { aHeight += aY; aY = 0; }
            if (aX >= (int)Mode.Width || aY >= (int)Mode.Height)
            {
                return;
            }

            aWidth = Math.Min(aWidth, (int)Mode.Width - aX);
            aHeight = Math.Min(aHeight, (int)Mode.Height - aY);

            if (aWidth <= 0 || aHeight <= 0)
            {
                return;
            }
        }

        var color = aColor.ToArgb();
        for (int i = aY; i < aY + aHeight; i++)
        {
            driver.ClearVRAM((int)((i * driver.Pitch) + (aX * 4)), aWidth, color);
        }
    }

    public override void DrawRectangle(Color color, int x, int y, int width, int height)
    {
        if (color.A < 255)
        {
            // Draw top edge from (x, y) to (x + width, y)
            DrawLine(color, x, y, x + width, y);
            // Draw left edge from (x, y) to (x, y + height)
            DrawLine(color, x, y, x, y + height);
            // Draw bottom edge from (x, y + height) to (x + width, y + height)
            DrawLine(color, x, y + height, x + width, y + height);
            // Draw right edge from (x + width, y) to (x + width, y + height)
            DrawLine(color, x + width, y, x + width, y + height);
        }
        else
        {
            int rawColor = color.ToArgb();
            // Draw top edge from (x, y) to (x + width, y)
            for (int posX = x; posX < x + width; posX++)
            {
                DrawPoint((uint)rawColor, posX, y);
            }
            // Draw left edge from (x, y) to (x, y + height)
            int newY = y + height;
            for (int posX = x; posX < x + width; posX++)
            {
                DrawPoint((uint)rawColor, posX, newY);
            }
            // Draw bottom edge from (x, y + height) to (x + width, y + height)
            for (int posY = y; posY < y + height; posY++)
            {
                DrawPoint((uint)rawColor, x, posY);
            }
            // Draw right edge from (x + width, y) to (x + width, y + height)
            int newX = x + width;
            for (int posY = y; posY < y + height; posY++)
            {
                DrawPoint((uint)rawColor, newX, posY);
            }
        }
    }

    public override void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        var width = (int)image.Width;
        var height = (int)image.Height;
        var data = image.RawData;

        if (preventOffBoundPixels)
        {
            var maxWidth = Math.Min(width, (int)mode.Width - x);
            var maxHeight = Math.Min(height, (int)mode.Height - y);
            var startX = Math.Max(0, x);
            var startY = Math.Max(0, y);

            var sourceX = Math.Max(0, -x);
            var sourceY = Math.Max(0, -y);

            // Adjust maxWidth and maxHeight if startX or startY were changed
            maxWidth -= startX - x;
            maxHeight -= startY - y;

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                return;
            }

            // If no cropping needed, use CopyBuffer directly
            if (sourceX == 0 && sourceY == 0 && maxWidth == width && maxHeight == height)
            {
                driver.CopyBuffer(data.AsMemory(), startX, startY, width, height);
            }
            else
            {
                // Need to copy row by row due to source offset
                for (int i = 0; i < maxHeight; i++)
                {
                    int sourceIndex = (sourceY + i) * width + sourceX;
                    driver.CopyBuffer(data.AsMemory(sourceIndex, maxWidth), startX, startY + i, maxWidth, 1);
                }
            }
        }
        else
        {
            driver.CopyBuffer(data.AsMemory(), x, y, width, height);
        }
    }

    public override void CroppedDrawImage(Image aImage, int aX, int aY, int aWidth, int aHeight, bool preventOffBoundPixels = true)
    {
        var xBitmap = aImage.RawData;
        var xWidth = aWidth;
        var xHeight = aHeight;

        if (preventOffBoundPixels)
        {
            var maxWidth = Math.Min(xWidth, (int)Mode.Width - aX);
            var maxHeight = Math.Min(xHeight, (int)Mode.Height - aY);

            var startX = Math.Max(0, aX);
            var startY = Math.Max(0, aY);

            var sourceX = Math.Max(0, -aX);
            var sourceY = Math.Max(0, -aY);

            maxWidth -= startX - aX;
            maxHeight -= startY - aY;

            if (maxWidth <= 0 || maxHeight <= 0)
            {
                return;
            }

            // If no cropping needed, use CopyBuffer directly
            if (sourceX == 0 && sourceY == 0 && maxWidth == xWidth && maxHeight == xHeight)
            {
                driver.CopyBuffer(xBitmap.AsMemory(), startX, startY, xWidth, xHeight);
            }
            else
            {
                // Need to copy row by row due to source offset
                for (int i = 0; i < maxHeight; i++)
                {
                    int sourceIndex = (sourceY + i) * xWidth + sourceX;
                    driver.CopyBuffer(xBitmap.AsMemory(sourceIndex, maxWidth), startX, startY + i, maxWidth, 1);
                }
            }
        }
        else
        {
            driver.CopyBuffer(xBitmap.AsMemory(), aX, aY, xWidth, xHeight);
        }
    }

    public override void DrawCanvas(Canvas canvas, int x, int y)
    {
        var srcBuffer = canvas.GetBuffer();
        if (srcBuffer != null)
        {
            driver.CopyBuffer(srcBuffer.AsMemory(), x, y, canvas.Width, canvas.Height);
        }
        else
        {
            base.DrawCanvas(canvas, x, y);
        }
    }

    #endregion

    public override void Display()
    {
        driver.Swap();
    }

    public override void Display(int x, int y, int width, int height)
    {
        driver.SwapRect(x, y, width, height);
    }

    #region Reading

    public override Color GetPointColor(int aX, int aY)
    {
        return Color.FromArgb((int)driver.GetPixel(aX, aY));
    }

    public override int GetRawPointColor(int aX, int aY)
    {
        return (int)driver.GetPixel(aX, aY);
    }

    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        Bitmap bitmap = new((uint)width, (uint)height, ColorDepth.ColorDepth32);

        int startX = Math.Max(0, x);
        int startY = Math.Max(0, y);
        int endX = Math.Min(x + width, (int)Mode.Width);
        int endY = Math.Min(y + height, (int)Mode.Height);

        int offsetX = Math.Max(0, -x);
        int offsetY = Math.Max(0, -y);

        int[] rawData = new int[width * height];

        for (int posy = startY; posy < endY; posy++)
        {
            int srcOffset = (int)(posy * driver.Pitch + startX * 4);
            int destOffset = (posy - startY + offsetY) * width + offsetX;

            driver.GetVRAM(srcOffset, rawData, destOffset, endX - startX);
        }

        bitmap.RawData = rawData;
        return bitmap;
    }

    #endregion

}
