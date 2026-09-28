// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// Where a network driver reports frames and link changes, returned by
/// <see cref="DeviceBinding.PublishNetwork"/>. Kit-owned; see <see cref="KeyboardSink"/>.
/// Allocation-free; any context.
/// </summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public sealed class NetworkSink
{
    private readonly PublishedDevice _device;

    internal NetworkSink(PublishedDevice device)
    {
        _device = device;
    }

    /// <summary>
    /// Reports a received frame; the span is valid for the call only. The
    /// ring's consumer needs thread context (it copies the frame), so a
    /// network driver delivers from a work item; a driver that must deliver
    /// from its handler waits for a preallocated pool in the ring.
    /// </summary>
    /// <param name="frame">The frame, without checksum.</param>
    public void Receive(ReadOnlySpan<byte> frame)
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Network) is NetworkConsumer consumer)
        {
            consumer.OnReceive(_device, frame);
        }
    }

    /// <summary>Reports the link coming up or going down.</summary>
    /// <param name="up">True when the link is up.</param>
    public void LinkChanged(bool up)
    {
        if (_device.IsWithdrawn)
        {
            return;
        }

        if (DeviceRegistry.ConsumerOf(DeviceKind.Network) is NetworkConsumer consumer)
        {
            consumer.OnLinkChanged(_device, up);
        }
    }
}
