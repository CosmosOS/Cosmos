// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>The ring's consumer of pointers: receives movement and button reports.</summary>
internal abstract class PointerConsumer : DeviceConsumer
{
    /// <summary>A relative movement report (a mouse). Caller's context, possibly an interrupt.</summary>
    /// <param name="device">The pointer reporting.</param>
    /// <param name="deltaX">Horizontal movement since the last report.</param>
    /// <param name="deltaY">Vertical movement since the last report.</param>
    /// <param name="buttons">Buttons held down.</param>
    /// <param name="wheel">Wheel movement since the last report.</param>
    public abstract void OnRelative(PublishedDevice device, int deltaX, int deltaY, PointerButtons buttons, int wheel);

    /// <summary>An absolute position report (a tablet, a touch screen). Caller's context, possibly an interrupt.</summary>
    /// <param name="device">The pointer reporting.</param>
    /// <param name="x">Horizontal position in the device's own range.</param>
    /// <param name="y">Vertical position in the device's own range.</param>
    /// <param name="buttons">Buttons held down.</param>
    public abstract void OnAbsolute(PublishedDevice device, int x, int y, PointerButtons buttons);
}
