using System;
using System.Collections.Generic;
using System.Drawing;
using Cosmos.Kernel.HAL.Devices.Graphic.SVGAII;

namespace Cosmos.Kernel.System.Graphics;

/// <summary>
/// Defines a VMWare SVGAII canvas implementation, used when the device did
/// not negotiate 3D support (QEMU's vmware-svga never does; devices that do
/// get <see cref="SvgaII3DCanvas"/> instead). Please note that this
/// implementation of <see cref="Canvas"/> can only be used with virtualizers
/// that implement SVGAII. This class will not work on regular hardware.
/// </summary>
internal class SvgaIICanvas : Canvas
{
    /// <summary>
    /// The 2D display driver, bound to the SVGA II PCI device.
    /// </summary>
    public SvgaIIDriver Driver { get; }

    /// <summary>
    /// Creates a canvas on the given SVGA II driver in the given mode.
    /// </summary>
    /// <param name="driver">The initialized VMware SVGA II display driver.</param>
    /// <param name="mode">The graphics mode to set; must be one of <see cref="AvailableModes"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">The mode is not supported by this driver.</exception>
    public SvgaIICanvas(SvgaIIDriver driver, Mode mode)
        : base(mode)
    {
        Driver = driver;
        ThrowIfModeIsNotValid(mode);
        SvgaIIRender.ApplyMode(this, driver, mode);
    }

    /// <inheritdoc />
    public override string Name => "VMWareSVGAII";

    /// <summary>
    /// Gets or sets the current graphics mode.
    /// </summary>
    public override Mode Mode
    {
        get => base.Mode;
        protected internal set
        {
            ThrowIfModeIsNotValid(value);
            base.Mode = value;
            SvgaIIRender.ApplyMode(this, Driver, value);
        }
    }

    /// <inheritdoc />
    public override Mode DefaultGraphicsMode => SvgaIIRender.DefaultMode;

    /// <inheritdoc />
    public override IReadOnlyList<Mode> AvailableModes { get; } = SvgaIIRender.CreateAvailableModes();

    /// <inheritdoc />
    internal override void Disable()
    {
        Driver.Disable();
    }

    /// <inheritdoc />
    public override void DrawPoint(Color color, int x, int y)
    {
        SvgaIIRender.DrawPoint(this, Driver, color, x, y);
    }

    /// <inheritdoc />
    public override void DrawPoint(uint color, int x, int y)
    {
        SvgaIIRender.DrawRawPoint(this, Driver, color, x, y);
    }

    /// <inheritdoc />
    public override void DrawPoint(int color, int x, int y)
    {
        SvgaIIRender.DrawRawPoint(this, Driver, (uint)color, x, y);
    }

    /// <inheritdoc />
    public override void DrawArray(Color[] colors, int x, int y, int width, int height)
    {
        SvgaIIRender.DrawArray(this, colors, x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawArray(int[] colors, int x, int y, int width, int height)
    {
        Driver.CopyBuffer(colors.AsMemory(), x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawArray(int[] colors, int x, int y, int width, int height, int startIndex)
    {
        Driver.CopyBuffer(colors.AsMemory(startIndex), x, y, width, height);
    }

    /// <inheritdoc />
    public override void DrawFilledRectangle(Color color, int xStart, int yStart, int width, int height)
    {
        SvgaIIRender.DrawFilledRectangle(this, Driver, color, xStart, yStart, width, height);
    }

    /// <inheritdoc />
    public override void DrawRectangle(Color color, int x, int y, int width, int height)
    {
        SvgaIIRender.DrawRectangle(this, color, x, y, width, height);
    }

    /// <inheritdoc />
    public override void Clear(int color)
    {
        Driver.ClearScreen((uint)color);
    }

    /// <inheritdoc />
    public override void Clear(Color color)
    {
        Driver.ClearScreen((uint)color.ToArgb());
    }

    /// <summary>
    /// Whether the device composes a 32-bit alpha hardware cursor on the host
    /// side. When true, callers can define a shape once with
    /// <see cref="DefineAlphaCursor"/> and move it with <see cref="SetCursor"/>
    /// instead of blitting a software cursor every frame.
    /// </summary>
    public bool HasHardwareCursor => Driver.HasAlphaCursor;

    /// <summary>
    /// Moves the hardware cursor and toggles its visibility.
    /// </summary>
    /// <param name="visible">Whether the cursor is shown.</param>
    /// <param name="x">The X coordinate of the cursor.</param>
    /// <param name="y">The Y coordinate of the cursor.</param>
    public void SetCursor(bool visible, int x, int y)
    {
        Driver.SetCursor(visible, (uint)x, (uint)y);
    }

    /// <summary>
    /// Define the hardware cursor shape. <paramref name="data"/> is
    /// width×height premultiplied 32-bit BGRA pixels.
    /// </summary>
    public void DefineAlphaCursor(int hotspotX, int hotspotY, int width, int height, int[] data)
    {
        Driver.DefineAlphaCursor((uint)hotspotX, (uint)hotspotY, (uint)width, (uint)height, data);
    }

    /// <summary>
    /// Defines the default hardware cursor shape on the device.
    /// </summary>
    public void CreateCursor()
    {
        Driver.DefineCursor();
    }

    /// <inheritdoc />
    public override void CopyPixels(int srcX, int srcY, int dstX, int dstY, int width, int height)
    {
        SvgaIIRender.CopyPixels(this, Driver, srcX, srcY, dstX, dstY, width, height);
    }

    /// <inheritdoc />
    public override Color GetPointColor(int x, int y)
    {
        return Color.FromArgb((int)Driver.GetPixel(x, y));
    }

    /// <inheritdoc />
    public override int GetRawPointColor(int x, int y)
    {
        return (int)Driver.GetPixel(x, y);
    }

    /// <inheritdoc />
    public override Bitmap GetImage(int x, int y, int width, int height)
    {
        return SvgaIIRender.GetImage(this, Driver, x, y, width, height);
    }

    /// <inheritdoc />
    public override void Display()
    {
        Driver.Swap();
    }

    /// <inheritdoc />
    public override void DrawImage(Image image, int x, int y, bool preventOffBoundPixels = true)
    {
        SvgaIIRender.DrawImage(this, Driver, image, x, y, preventOffBoundPixels);
    }

    /// <inheritdoc />
    public override void CroppedDrawImage(Image image, int x, int y, int width, int height, bool preventOffBoundPixels = true)
    {
        SvgaIIRender.CroppedDrawImage(this, Driver, image, x, y, width, height, preventOffBoundPixels);
    }
}
