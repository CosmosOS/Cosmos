// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Boot.Limine;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Firmware;

/// <summary>
/// The framebuffer the bootloader handed the kernel, as an
/// <see cref="IDisplay"/> the engine publishes with firmware provenance and
/// no binding. The memory is already mapped by Limine, so the region is
/// built over the virtual address as given; <see cref="PhysicalAddress"/>
/// is what the retirement rule compares with a bound function's memory
/// windows. Built on the boot thread; read by the ring in thread context.
/// </summary>
internal sealed unsafe class FirmwareDisplay : IDisplay
{
    /// <summary>The refresh rate reported when the EDID carries none.</summary>
    private const int DefaultRefreshRate = 60;

    /// <summary>The name every firmware framebuffer is published under.</summary>
    private const string FramebufferName = "framebuffer";

    /// <summary>The region over the framebuffer, invalidated by <see cref="Retire"/>.</summary>
    private readonly DeviceRegion _framebuffer;

    /// <summary>Builds the display from Limine's description of the framebuffer. Boot thread.</summary>
    /// <param name="framebuffer">Limine's framebuffer entry.</param>
    /// <param name="hhdmOffset">The higher-half direct map offset, or 0 without one.</param>
    internal FirmwareDisplay(LimineFramebuffer* framebuffer, ulong hhdmOffset)
    {
        ulong address = (ulong)framebuffer->Address;
        int width = (int)framebuffer->Width;
        int height = (int)framebuffer->Height;
        int pitch = (int)framebuffer->Pitch;
        Mode = new DisplayMode(width, height, pitch, framebuffer->BitsPerPixel, ParseEdidRefreshRate(framebuffer));
        Length = (ulong)height * (ulong)pitch;
        PhysicalAddress = hhdmOffset != 0 && address >= hhdmOffset ? address - hhdmOffset : address;
        _framebuffer = new DeviceRegion(address, Length, RegionCaching.WriteCombining);
    }

    /// <inheritdoc/>
    public string Name => FramebufferName;

    /// <inheritdoc/>
    public DisplayMode Mode { get; }

    /// <summary>The framebuffer as Limine mapped it, one frame long; every access throws once <see cref="Retire"/> ran.</summary>
    public DeviceRegion? Framebuffer => _framebuffer;

    /// <summary>The physical address of the framebuffer's first byte, for the retirement rule. Any context; allocation-free.</summary>
    internal ulong PhysicalAddress { get; }

    /// <summary>The length of one frame in bytes: height times pitch. Any context; allocation-free.</summary>
    internal ulong Length { get; }

    /// <summary>
    /// Invalidates the framebuffer region: the scanout now belongs to the
    /// driver that bound the function holding it, and a ring still drawing
    /// through this display gets the region's exception instead of writing
    /// into the driver's memory. Called by the registry's retirement rule,
    /// from the offer that bound the node.
    /// </summary>
    internal void Retire() => _framebuffer.Invalidate();

    /// <summary>A no-op: a linear framebuffer is scanned out as it is written.</summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height)
    {
    }

    /// <summary>
    /// The refresh rate of the first detailed timing descriptor of the EDID
    /// Limine passed on, or <see cref="DefaultRefreshRate"/> when the EDID is
    /// absent, malformed, or the descriptor is not a timing.
    /// </summary>
    private static int ParseEdidRefreshRate(LimineFramebuffer* framebuffer)
    {
        if (framebuffer->EdidSize < 128 || framebuffer->Edid == null)
        {
            return DefaultRefreshRate;
        }

        byte* edid = (byte*)framebuffer->Edid;

        // The EDID header: 00 FF FF FF FF FF FF 00.
        if (edid[0] != 0x00 || edid[1] != 0xFF || edid[7] != 0x00)
        {
            return DefaultRefreshRate;
        }

        // The first detailed timing descriptor starts at byte 54.
        byte* dtd = edid + 54;

        // Pixel clock in 10 kHz units (bytes 0 and 1, little-endian). Zero
        // means the descriptor is not a timing.
        uint pixelClock = (uint)(dtd[0] | (dtd[1] << 8));
        if (pixelClock == 0)
        {
            return DefaultRefreshRate;
        }

        uint hActive = (uint)(dtd[2] | ((dtd[4] >> 4) << 8));
        uint hBlank = (uint)(dtd[3] | ((dtd[4] & 0xF) << 8));
        uint vActive = (uint)(dtd[5] | ((dtd[7] >> 4) << 8));
        uint vBlank = (uint)(dtd[6] | ((dtd[7] & 0xF) << 8));

        uint hTotal = hActive + hBlank;
        uint vTotal = vActive + vBlank;
        if (hTotal == 0 || vTotal == 0)
        {
            return DefaultRefreshRate;
        }

        int hz = (int)((pixelClock * 10000) / (hTotal * vTotal));
        if (hz < 24 || hz > 360)
        {
            return DefaultRefreshRate;
        }

        return hz;
    }
}
