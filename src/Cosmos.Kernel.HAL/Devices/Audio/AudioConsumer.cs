// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Devices.Audio;

/// <summary>The ring's consumer of audio outputs: learns when the device freed room or changed format.</summary>
internal abstract class AudioConsumer : DeviceConsumer
{
    /// <summary>The device drained a buffer; <see cref="IAudioOutput.WritableBytes"/> grew. Caller's context, an interrupt included.</summary>
    /// <param name="device">The output reporting.</param>
    public abstract void OnBufferCompleted(PublishedDevice device);

    /// <summary>The device's format or rate changed; re-read them. Caller's context.</summary>
    /// <param name="device">The output reporting.</param>
    public abstract void OnFormatChanged(PublishedDevice device);
}
