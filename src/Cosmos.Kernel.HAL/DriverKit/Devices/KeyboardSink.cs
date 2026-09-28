// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Where a keyboard driver reports keys, returned by
/// <see cref="DeviceBinding.PublishKeyboard"/>. Kit-owned: it finds the
/// ring's keyboard consumer at call time and drops the report when the device
/// was withdrawn or nobody listens. Allocation-free; any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class KeyboardSink
{
    private readonly PublishedDevice _device;

    internal KeyboardSink(PublishedDevice device)
    {
        _device = device;
    }

    /// <summary>Reports a key going down or up.</summary>
    /// <param name="scanCode">The raw scan code.</param>
    /// <param name="released">True for a release.</param>
    public void Report(byte scanCode, bool released)
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Keyboard) is KeyboardConsumer consumer)
        {
            consumer.OnKey(_device, scanCode, released);
        }
    }
}
