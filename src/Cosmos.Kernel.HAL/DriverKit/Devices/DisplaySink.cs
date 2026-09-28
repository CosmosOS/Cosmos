// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Where a display driver reports a mode change, returned by
/// <see cref="DeviceBinding.PublishDisplay"/>. Kit-owned; see <see cref="KeyboardSink"/>.
/// Allocation-free; any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class DisplaySink
{
    private readonly PublishedDevice _device;

    internal DisplaySink(PublishedDevice device)
    {
        _device = device;
    }

    /// <summary>Reports that <see cref="IDisplay.Mode"/> changed.</summary>
    public void ModeChanged()
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Display) is DisplayConsumer consumer)
        {
            consumer.OnModeChanged(_device);
        }
    }
}
