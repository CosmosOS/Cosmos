// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>What a display driver implements and hands to <see cref="DeviceBinding.PublishDisplay"/>. Called by the ring in thread context.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface IDisplay
{
    /// <summary>The name the driver gives the display, for the log and the ring: <c>virtio-gpu</c>, <c>vmware-svga</c>, <c>framebuffer</c>.</summary>
    string Name { get; }

    /// <summary>The current mode; <see cref="DisplayMode.IsEmpty"/> while the driver has not programmed a scanout.</summary>
    DisplayMode Mode { get; }

    /// <summary>The framebuffer, when the display exposes one the CPU can draw into; null otherwise.</summary>
    DeviceRegion? Framebuffer { get; }

    /// <summary>Makes the given rectangle of the framebuffer visible, for a display that needs telling.</summary>
    /// <param name="x">Left edge in pixels.</param>
    /// <param name="y">Top edge in pixels.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    void Flush(int x, int y, int width, int height);
}
