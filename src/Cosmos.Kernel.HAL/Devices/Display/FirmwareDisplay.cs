// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.Firmware;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.HAL.Devices.Display;

/// <summary>
/// The framebuffer the bootloader handed the kernel, as an
/// <see cref="IDisplay"/> the engine publishes with firmware provenance and
/// no binding. The memory is already mapped by Limine, so the region is
/// built over the virtual address as given; <see cref="PhysicalAddress"/>
/// is what the retirement rule compares with a bound function's memory
/// windows. Built on the boot thread; read by the ring in thread context.
/// </summary>
internal sealed class FirmwareDisplay : IDisplay
{
    /// <summary>The name every firmware framebuffer is published under.</summary>
    private const string FramebufferName = "framebuffer";

    /// <summary>The region over the framebuffer, invalidated by <see cref="Retire"/>.</summary>
    private readonly DeviceRegion _framebuffer;

    /// <summary>The display over the framebuffer the bootloader handed over, or null when it answered with none. Written once by <see cref="DiscoverBoot"/>; published by the engine when it starts.</summary>
    internal static FirmwareDisplay? Boot { get; private set; }

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

    /// <summary>Builds the display from the bootloader's description of the framebuffer. Boot thread.</summary>
    /// <param name="framebuffer">The framebuffer as Core read it.</param>
    private FirmwareDisplay(in BootFramebuffer framebuffer)
    {
        Mode = new DisplayMode(framebuffer.Width, framebuffer.Height, framebuffer.Pitch, framebuffer.BitsPerPixel, framebuffer.RefreshRate);
        Length = (ulong)framebuffer.Height * (ulong)framebuffer.Pitch;
        PhysicalAddress = framebuffer.PhysicalAddress;
        _framebuffer = new DeviceRegion(framebuffer.Address, Length, RegionCaching.WriteCombining);
    }

    /// <summary>
    /// Records the framebuffer the bootloader handed over as <see cref="Boot"/>,
    /// or nothing when there is none. Boot thread, from the HAL initializer,
    /// once.
    /// </summary>
    internal static void DiscoverBoot()
    {
        if (BootFirmware.TryGetFramebuffer(out BootFramebuffer framebuffer))
        {
            Boot = new FirmwareDisplay(framebuffer);
        }
    }

    /// <summary>
    /// Invalidates the framebuffer region: the scanout now belongs to the
    /// driver that bound the function holding it, and a ring still drawing
    /// through this display gets the region's exception instead of writing
    /// into the driver's memory. Called by the registry's retirement rule,
    /// from the offer that bound the node.
    /// </summary>
    internal void Retire()
    {
        _framebuffer.Invalidate();
    }

    /// <summary>A no-op: a linear framebuffer is scanned out as it is written.</summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height)
    {
    }
}
