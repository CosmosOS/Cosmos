// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>A display that can switch modes. Optional facet on an <see cref="IDisplay"/>, found by a type test on the published object. Thread context.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IDisplayModes
{
    /// <summary>The modes the display accepts, in the driver's order. Pitch and refresh rate are informational; the driver sets the real pitch.</summary>
    ReadOnlySpan<DisplayMode> Modes { get; }

    /// <summary>Switches to the mode with the given width, height and depth; the pitch is the driver's. Reports through <see cref="DisplaySink.ModeChanged"/> on success.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="bitsPerPixel">Bits per pixel.</param>
    /// <returns>False when the display cannot switch to that mode; nothing changed.</returns>
    bool TrySetMode(int width, int height, int bitsPerPixel);
}
