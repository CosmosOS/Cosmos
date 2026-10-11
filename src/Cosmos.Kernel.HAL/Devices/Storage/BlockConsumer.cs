// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.Devices.Storage;

/// <summary>The ring's consumer of block devices. Block devices have no reports; the consumer only learns of arrivals and departures.</summary>
internal abstract class BlockConsumer : DeviceConsumer
{
}
