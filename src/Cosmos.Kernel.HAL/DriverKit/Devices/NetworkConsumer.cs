// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>The ring's consumer of network interfaces: receives frames and link changes.</summary>
internal abstract class NetworkConsumer : DeviceConsumer
{
    /// <summary>A frame arrived. Caller's context, possibly an interrupt; the span is valid for the call only.</summary>
    /// <param name="device">The interface reporting.</param>
    /// <param name="frame">The received frame, without checksum.</param>
    public abstract void OnReceive(PublishedDevice device, ReadOnlySpan<byte> frame);

    /// <summary>The link came up or went down. Caller's context, possibly an interrupt.</summary>
    /// <param name="device">The interface reporting.</param>
    /// <param name="up">True when the link is up.</param>
    public abstract void OnLinkChanged(PublishedDevice device, bool up);
}
