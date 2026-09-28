// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit.Engine;

namespace Cosmos.Kernel.HAL.DriverKit.Platform;

/// <summary>
/// The publisher of the platform bus: the root nodes a machine description
/// seeds because nothing enumerates them (a PCI host, a memory-mapped
/// device at a fixed address). Called by the arch HAL assemblies from the
/// platform initializer; the nodes wait in the engine's queue until the
/// driver stage offers them.
/// </summary>
internal static class PlatformBus
{
    /// <summary>
    /// Publishes a root node. Thread context; callable before the engine
    /// starts, when the job only queues, and afterwards, when the offer is
    /// queued behind everything pending.
    /// </summary>
    /// <param name="identity">The node's identity.</param>
    /// <param name="resources">Its windows and port ranges.</param>
    /// <param name="interrupts">Its interrupt sources.</param>
    /// <param name="access">The bus's access object for it, or null.</param>
    /// <exception cref="InvalidOperationException">The caller is an interrupt handler.</exception>
    public static DeviceNode Publish(PlatformIdentity identity, DeviceResource[] resources, InterruptSource[] interrupts, object? access)
    {
        InterruptContextGuard.ThrowIfInHandler(nameof(Publish));
        DeviceNode node = new(identity, resources, interrupts, access, null);
        DriverEngine.PublishNode(node);
        return node;
    }
}
