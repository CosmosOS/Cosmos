// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;

namespace Cosmos.Kernel.Tests.Drivers.Library;

/// <summary>
/// What <see cref="DisplayDriver"/> holds for one bound device, hung off
/// <see cref="DeviceBinding.DriverState"/>: the display contract it
/// publishes, in one fixed mode, with no framebuffer behind it. A display
/// without a framebuffer receives no pixels and no flush from the ring's
/// canvas, so <see cref="Flush"/> only counts the calls that would reach
/// it. The test reads the state back through the node's binding and the
/// ring's display manager. Written over the public seam only.
/// </summary>
public sealed class DisplayState : IDisplay
{
    /// <summary>The name the display is published under.</summary>
    public const string DisplayName = "synthetic-display";

    /// <summary>Width in pixels of the one mode the display is in.</summary>
    public const int ModeWidth = 64;

    /// <summary>Height in pixels of that mode.</summary>
    public const int ModeHeight = 32;

    /// <summary>Bytes per row of that mode: the width at four bytes per pixel.</summary>
    public const int ModePitch = 256;

    /// <summary>Bits per pixel of that mode.</summary>
    public const int ModeBitsPerPixel = 32;

    private readonly DeviceBinding _binding;
    private volatile int _flushCount;

    /// <summary>Keeps the binding the driver was probed with. Thread context, from the probe.</summary>
    /// <param name="binding">The device and the kit facilities for it.</param>
    internal DisplayState(DeviceBinding binding)
    {
        _binding = binding;
    }

    /// <inheritdoc/>
    public string Name => DisplayName;

    /// <inheritdoc/>
    public DisplayMode Mode => new(ModeWidth, ModeHeight, ModePitch, ModeBitsPerPixel);

    /// <summary>Null: the display exposes no region the CPU can draw into.</summary>
    public DeviceRegion? Framebuffer => null;

    /// <summary>The binding this state belongs to.</summary>
    public DeviceBinding Binding => _binding;

    /// <summary>The sink mode changes would be reported to; set once the display is published.</summary>
    public DisplaySink? Sink { get; internal set; }

    /// <summary>How many times <see cref="Flush"/> was called; zero while no canvas draws on the display.</summary>
    public int FlushCount => _flushCount;

    /// <summary>Counts the call; there is no framebuffer to make visible. Any context.</summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public void Flush(int x, int y, int width, int height) => _flushCount++;
}
