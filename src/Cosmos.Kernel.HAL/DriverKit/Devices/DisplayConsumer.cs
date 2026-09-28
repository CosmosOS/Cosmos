// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>The ring's consumer of displays: learns when a display's mode changed.</summary>
internal abstract class DisplayConsumer : DeviceConsumer
{
    /// <summary>The display's mode changed; re-read <see cref="IDisplay.Mode"/>. Caller's context.</summary>
    /// <param name="device">The display reporting.</param>
    public abstract void OnModeChanged(PublishedDevice device);
}
