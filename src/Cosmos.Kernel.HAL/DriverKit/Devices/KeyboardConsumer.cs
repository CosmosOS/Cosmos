// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>The ring's consumer of keyboards: receives raw scan codes from every published keyboard.</summary>
internal abstract class KeyboardConsumer : DeviceConsumer
{
    /// <summary>A key went down or up. Caller's context, possibly an interrupt.</summary>
    /// <param name="device">The keyboard reporting.</param>
    /// <param name="scanCode">The raw scan code as the device reported it.</param>
    /// <param name="released">True for a key release.</param>
    public abstract void OnKey(PublishedDevice device, byte scanCode, bool released);
}
