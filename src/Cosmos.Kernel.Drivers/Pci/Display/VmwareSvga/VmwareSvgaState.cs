// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System;
using Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga.Enums;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.System.Graphics;

namespace Cosmos.Kernel.Drivers.Pci.Display.VmwareSvga;

/// <summary>
/// Everything <see cref="VmwareSvgaDriver"/> holds for one bound adapter,
/// hung off <see cref="DeviceBinding.DriverState"/>, and the display it
/// publishes: the registers and FIFO, the VRAM, the mode the scanout is
/// programmed to, and the facets. The display reports an empty mode until
/// a mode is programmed (on QEMU the firmware leaves the scanout off, and
/// the width, height and pitch registers echo the VGA surface of the
/// firmware mode, which the guest has not programmed), and reports the
/// programmed geometry from then on, while the scanout is held off by
/// <see cref="SetEnabled"/> included. <see cref="Flush"/> is an UPDATE
/// and a sync, gated on a programmed mode and an enabled scanout: a
/// disabled adapter consumes nothing, and a console flush during the
/// FIFO wire tests must not put UPDATE commands in the FIFO the tests
/// pin. The ring's canvas is the back buffer; the driver keeps none.
/// <see cref="VmwareSvga3DState"/> is the variant published when the host
/// negotiated 3D. Thread context; one caller at a time, as the ring's
/// canvas is.
/// </summary>
public class VmwareSvgaState : IDisplay, IDisplayModes, IHardwareCursor, ISvgaAdapter
{
    // --- Constants ---

    /// <summary>Bits per pixel of every listed mode; the canvas draws 32-bit pixels only.</summary>
    internal const int BitsPerPixel = 32;

    /// <summary>Bytes per pixel of every listed mode.</summary>
    internal const int BytesPerPixel = 4;

    /// <summary>The largest cursor image accepted, per side: the bound QEMU's cursor definition enforces, and a bound that keeps the pixel count in an int.</summary>
    internal const int MaxCursorDimension = 256;

    // --- Private fields ---

    /// <summary>The modes the adapter accepts, the classic VMware list at 32 bits per pixel. The pitch is informational; the adapter sets the real one.</summary>
    private static readonly DisplayMode[] s_modes =
    [
        new(320, 200, 320 * BytesPerPixel, BitsPerPixel),
        new(320, 240, 320 * BytesPerPixel, BitsPerPixel),
        new(640, 480, 640 * BytesPerPixel, BitsPerPixel),
        new(720, 480, 720 * BytesPerPixel, BitsPerPixel),
        new(800, 600, 800 * BytesPerPixel, BitsPerPixel),
        new(1024, 768, 1024 * BytesPerPixel, BitsPerPixel),
        new(1152, 768, 1152 * BytesPerPixel, BitsPerPixel),
        new(1280, 720, 1280 * BytesPerPixel, BitsPerPixel),
        new(1280, 768, 1280 * BytesPerPixel, BitsPerPixel),
        new(1280, 800, 1280 * BytesPerPixel, BitsPerPixel),
        new(1280, 1024, 1280 * BytesPerPixel, BitsPerPixel),
        new(1360, 768, 1360 * BytesPerPixel, BitsPerPixel),
        new(1440, 900, 1440 * BytesPerPixel, BitsPerPixel),
        new(1400, 1050, 1400 * BytesPerPixel, BitsPerPixel),
        new(1600, 1200, 1600 * BytesPerPixel, BitsPerPixel),
        new(1680, 1050, 1680 * BytesPerPixel, BitsPerPixel),
        new(1920, 1080, 1920 * BytesPerPixel, BitsPerPixel),
        new(1920, 1200, 1920 * BytesPerPixel, BitsPerPixel),
        new(2048, 1536, 2048 * BytesPerPixel, BitsPerPixel),
        new(2560, 1080, 2560 * BytesPerPixel, BitsPerPixel),
        new(2560, 1600, 2560 * BytesPerPixel, BitsPerPixel),
        new(2560, 2048, 2560 * BytesPerPixel, BitsPerPixel),
        new(3200, 2048, 3200 * BytesPerPixel, BitsPerPixel),
        new(3200, 2400, 3200 * BytesPerPixel, BitsPerPixel),
        new(3840, 2400, 3840 * BytesPerPixel, BitsPerPixel),
    ];

    private readonly DeviceBinding _binding;
    private readonly VmwareSvgaFifo _fifo;
    private readonly DeviceRegion _vram;
    private DisplaySink? _sink;
    private DeviceRegion? _framebuffer;
    private bool _modeProgrammed;
    private int _width;
    private int _height;
    private int _bitsPerPixel;
    private int _pitch;
    private uint _frameOffset;

    // --- Constructor ---

    /// <summary>
    /// Takes the registers, the FIFO and the VRAM the probe mapped. The
    /// found scanout comes through <see cref="RecordFoundMode"/>. Thread
    /// context, from the probe.
    /// </summary>
    /// <param name="binding">The device's binding, for the log.</param>
    /// <param name="fifo">The adapter's registers and FIFO, initialised.</param>
    /// <param name="vram">The adapter's VRAM, BAR 1.</param>
    internal VmwareSvgaState(DeviceBinding binding, VmwareSvgaFifo fifo, DeviceRegion vram)
    {
        _binding = binding;
        _fifo = fifo;
        _vram = vram;
    }

    // --- IDisplay ---

    /// <inheritdoc/>
    public string Name => "vmware-svga";

    /// <summary>The programmed geometry, with the adapter's pitch; empty until a mode is programmed. Any context.</summary>
    public DisplayMode Mode => _modeProgrammed ? new DisplayMode(_width, _height, _pitch, _bitsPerPixel) : default;

    /// <summary>One frame of VRAM at the adapter's frame offset; null until a mode is programmed. Any context.</summary>
    public DeviceRegion? Framebuffer => _framebuffer;

    /// <summary>
    /// UPDATE of the rectangle, clipped to the mode, then a sync. Returns
    /// at once while no mode is programmed or the scanout is disabled: a
    /// disabled adapter consumes nothing. Thread context.
    /// </summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height)
    {
        if (!_modeProgrammed || !_fifo.IsEnabled)
        {
            return;
        }

        if (x < 0)
        {
            width += x;
            x = 0;
        }

        if (y < 0)
        {
            height += y;
            y = 0;
        }

        if (x >= _width || y >= _height)
        {
            return;
        }

        width = Math.Min(width, _width - x);
        height = Math.Min(height, _height - y);
        if (width <= 0 || height <= 0)
        {
            return;
        }

        _fifo.Update((uint)x, (uint)y, (uint)width, (uint)height);
    }

    // --- IDisplayModes ---

    /// <inheritdoc/>
    public ReadOnlySpan<DisplayMode> Modes => s_modes;

    /// <summary>
    /// Programs a listed mode: the width, height and depth registers, the
    /// scanout enabled, the FIFO initialised again, the pitch and the frame
    /// offset read back, the frame sliced out of VRAM, and the change
    /// reported to the ring. Thread context.
    /// </summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="bitsPerPixel">Bits per pixel, 32.</param>
    /// <returns>False for a mode not in <see cref="Modes"/> or one VRAM cannot hold; nothing changed then.</returns>
    public bool TrySetMode(int width, int height, int bitsPerPixel)
    {
        if (!IsListed(width, height, bitsPerPixel))
        {
            return false;
        }

        if ((ulong)width * (ulong)height * BytesPerPixel > _vram.Length)
        {
            return false;
        }

        _fifo.SetMode((uint)width, (uint)height, (uint)bitsPerPixel);
        int pitch = (int)_fifo.ReadRegister(Register.BytesPerLine);
        if (pitch < width * BytesPerPixel)
        {
            pitch = width * BytesPerPixel;
        }

        uint frameOffset = _fifo.ReadRegister(Register.FrameBufferOffset);
        RecordMode(width, height, bitsPerPixel, pitch, frameOffset);
        _sink?.ModeChanged();
        return true;
    }

    // --- IHardwareCursor ---

    /// <summary>DEFINE_ALPHA_CURSOR when the adapter composes an alpha cursor; false otherwise, and QEMU does not. Thread context.</summary>
    /// <param name="hotspotX">The hotspot's column within the image.</param>
    /// <param name="hotspotY">The hotspot's row within the image.</param>
    /// <param name="width">Width of the image in pixels.</param>
    /// <param name="height">Height of the image in pixels.</param>
    /// <param name="pixels">The pixels, width times height of them, premultiplied ARGB.</param>
    /// <returns>False without the alpha cursor capability, for an empty image, one larger than <see cref="MaxCursorDimension"/> per side, or too few pixels; the previous image stays.</returns>
    public bool TryDefine(int hotspotX, int hotspotY, int width, int height, ReadOnlySpan<uint> pixels)
    {
        if (!_fifo.HasAlphaCursor)
        {
            return false;
        }

        if (width <= 0 || height <= 0 || hotspotX < 0 || hotspotY < 0)
        {
            return false;
        }

        if (width > MaxCursorDimension || height > MaxCursorDimension)
        {
            return false;
        }

        long count = (long)width * height;
        if (count > pixels.Length)
        {
            return false;
        }

        _fifo.DefineAlphaCursor((uint)hotspotX, (uint)hotspotY, (uint)width, (uint)height, pixels.Slice(0, (int)count));
        return true;
    }

    /// <summary>The cursor registers: position, then the on bit that latches them. Thread context.</summary>
    /// <param name="x">The hotspot's column on the display.</param>
    /// <param name="y">The hotspot's row on the display.</param>
    /// <param name="visible">True to show the cursor, false to hide it.</param>
    public void Set(int x, int y, bool visible)
    {
        _fifo.SetCursor(visible, (uint)Math.Max(0, x), (uint)Math.Max(0, y));
    }

    // --- ISvgaAdapter ---

    /// <inheritdoc/>
    public uint Capabilities => _fifo.Capabilities;

    /// <inheritdoc/>
    public bool Is3DNegotiated => _fifo.Is3DNegotiated;

    /// <inheritdoc/>
    public uint Svga3DVersion => _fifo.Svga3DVersion;

    /// <inheritdoc/>
    public bool IsEnabled => _fifo.IsEnabled;

    /// <summary>Writes the Enable register; ignored for an enable before a mode is programmed. Thread context.</summary>
    /// <param name="enabled">True to enable the scanout, false to disable it.</param>
    public void SetEnabled(bool enabled)
    {
        if (enabled && !_modeProgrammed)
        {
            return;
        }

        _fifo.SetEnabled(enabled);
    }

    /// <inheritdoc/>
    public uint FifoMin => _fifo.GetFifo(FIFO.Min);

    /// <inheritdoc/>
    public uint FifoMax => _fifo.GetFifo(FIFO.Max);

    /// <inheritdoc/>
    public uint NextCommand
    {
        get => _fifo.GetFifo(FIFO.NextCmd);
        set => _fifo.SetFifo(FIFO.NextCmd, value);
    }

    /// <inheritdoc/>
    public uint ReadFifo(uint byteOffset) => _fifo.ReadFifoDword(byteOffset);

    /// <summary>
    /// The SVGA3D canvas over <paramref name="display"/>, regardless of the
    /// negotiation, in the mode the registers hold; the adapter is neither
    /// re-enabled nor its FIFO re-initialised. Thread context.
    /// </summary>
    /// <param name="display">The display this adapter publishes.</param>
    /// <returns>The 3D canvas.</returns>
    public Canvas3D CreateCanvas3D(DisplayDevice display)
    {
        ArgumentNullException.ThrowIfNull(display);

        return new VmwareSvgaCanvas3D(this, display);
    }

    // --- Internal properties ---

    /// <summary>The adapter's registers and FIFO, for the 3D canvas. Any context.</summary>
    internal VmwareSvgaFifo Fifo => _fifo;

    /// <summary>The adapter's VRAM, for the 3D canvas's DMA scratch. Any context.</summary>
    internal DeviceRegion Vram => _vram;

    /// <summary>The device's binding, for the log. Any context.</summary>
    internal DeviceBinding Binding => _binding;

    /// <summary>The sink mode changes go to; set by the probe when the display is published.</summary>
    internal DisplaySink? Sink
    {
        get => _sink;
        set => _sink = value;
    }

    /// <summary>The frame's byte offset in VRAM, as the FrameBufferOffset register read after the last mode set. Any context.</summary>
    internal uint FrameOffset => _frameOffset;

    // --- Internal methods for the probe ---

    /// <summary>
    /// Records the scanout the probe found. When the firmware enabled the
    /// scanout the geometry is the display's mode; when it did not, the
    /// mode stays empty until <see cref="TrySetMode"/>. Thread context,
    /// from the probe, before the display is published.
    /// </summary>
    /// <param name="width">The Width register.</param>
    /// <param name="height">The Height register.</param>
    /// <param name="bitsPerPixel">The BitsPerPixel register.</param>
    /// <param name="pitch">The BytesPerLine register.</param>
    /// <param name="frameOffset">The FrameBufferOffset register.</param>
    /// <param name="enabled">Whether the Enable register read 1.</param>
    internal void RecordFoundMode(int width, int height, int bitsPerPixel, int pitch, uint frameOffset, bool enabled)
    {
        if (!enabled)
        {
            return;
        }

        if (width <= 0 || height <= 0 || pitch <= 0)
        {
            return;
        }

        RecordMode(width, height, bitsPerPixel, pitch, frameOffset);
    }

    // --- Private methods ---

    /// <summary>Whether the mode is in the list.</summary>
    private static bool IsListed(int width, int height, int bitsPerPixel)
    {
        if (bitsPerPixel != BitsPerPixel)
        {
            return false;
        }

        for (int i = 0; i < s_modes.Length; i++)
        {
            if (s_modes[i].Width == width && s_modes[i].Height == height)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Records a programmed geometry and slices one frame out of VRAM at the frame offset; the frame is null when VRAM cannot hold it.</summary>
    private void RecordMode(int width, int height, int bitsPerPixel, int pitch, uint frameOffset)
    {
        _width = width;
        _height = height;
        _bitsPerPixel = bitsPerPixel;
        _pitch = pitch;
        _frameOffset = frameOffset;

        ulong frameBytes = (ulong)height * (ulong)pitch;
        if (frameOffset > _vram.Length || _vram.Length - frameOffset < frameBytes)
        {
            _binding.Log($"frame of {frameBytes} bytes at offset {frameOffset} does not fit the {_vram.Length} byte VRAM; nothing is drawn");
            _framebuffer = null;
        }
        else
        {
            _framebuffer = _vram.Slice(frameOffset, frameBytes);
        }

        _modeProgrammed = true;
    }
}
