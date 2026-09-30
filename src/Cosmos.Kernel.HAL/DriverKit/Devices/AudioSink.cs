// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Where an audio driver reports that it drained a buffer or changed
/// format, returned by <see cref="DeviceBinding.PublishAudio"/>. Kit-owned;
/// see <see cref="KeyboardSink"/>. Allocation-free; any context, so the
/// driver may report a completion straight from its interrupt handler.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class AudioSink
{
    private readonly PublishedDevice _device;

    internal AudioSink(PublishedDevice device)
    {
        _device = device;
    }

    /// <summary>Reports that the device finished a buffer, so the ring has room the writer can use.</summary>
    public void BufferCompleted()
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Audio) is AudioConsumer consumer)
        {
            consumer.OnBufferCompleted(_device);
        }
    }

    /// <summary>Reports that <see cref="IAudioOutput.Format"/> or <see cref="IAudioOutput.SampleRate"/> changed.</summary>
    public void FormatChanged()
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Audio) is AudioConsumer consumer)
        {
            consumer.OnFormatChanged(_device);
        }
    }
}
