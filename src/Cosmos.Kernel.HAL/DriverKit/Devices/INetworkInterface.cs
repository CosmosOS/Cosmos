// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>What a network driver implements and hands to <see cref="DeviceBinding.PublishNetwork"/>. Called by the ring in thread context.</summary>
[Experimental(Experimentals.DriverKitSeamDiagId)]
public interface INetworkInterface
{
    /// <summary>The interface's name.</summary>
    string Name { get; }

    /// <summary>The hardware address.</summary>
    MACAddress MacAddress { get; }

    /// <summary>True while the link is up.</summary>
    bool LinkUp { get; }

    /// <summary>Queues a frame for transmission.</summary>
    /// <param name="frame">The frame, without checksum.</param>
    /// <returns>False when the device could not take it (queue full, link down).</returns>
    bool Transmit(ReadOnlySpan<byte> frame);
}
