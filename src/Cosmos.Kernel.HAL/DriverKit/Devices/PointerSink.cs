// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Where a pointer driver reports movement, returned by
/// <see cref="DeviceBinding.PublishPointer"/>. Kit-owned; see <see cref="KeyboardSink"/>.
/// Allocation-free; any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class PointerSink
{
    private readonly PublishedDevice _device;

    internal PointerSink(PublishedDevice device)
    {
        _device = device;
    }

    /// <summary>Reports relative movement.</summary>
    /// <param name="deltaX">Horizontal movement since the last report.</param>
    /// <param name="deltaY">Vertical movement since the last report.</param>
    /// <param name="buttons">Buttons held down.</param>
    /// <param name="wheel">Wheel movement since the last report.</param>
    public void ReportRelative(int deltaX, int deltaY, PointerButtons buttons, int wheel)
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Pointer) is PointerConsumer consumer)
        {
            consumer.OnRelative(_device, deltaX, deltaY, buttons, wheel);
        }
    }

    /// <summary>Reports an absolute position.</summary>
    /// <param name="x">Horizontal position in the device's range.</param>
    /// <param name="y">Vertical position in the device's range.</param>
    /// <param name="buttons">Buttons held down.</param>
    public void ReportAbsolute(int x, int y, PointerButtons buttons)
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Pointer) is PointerConsumer consumer)
        {
            consumer.OnAbsolute(_device, x, y, buttons);
        }
    }
}
