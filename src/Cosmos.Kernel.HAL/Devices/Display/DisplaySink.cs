// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Devices.Input;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Devices.Display;

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
