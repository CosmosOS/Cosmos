// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Devices;

/// <summary>
/// What the ring installs to learn about devices of one kind. Abstract
/// classes rather than interfaces on purpose: the report methods of the
/// derived consumers are called by sinks from the driver's context, which
/// may be an interrupt, and a virtual call is a table load that can neither
/// allocate nor throw, where interface dispatch has cold paths that do both.
/// <para>
/// Contexts: <see cref="OnPublished"/> and <see cref="OnWithdrawn"/> run in
/// thread context, never concurrently: on the kit worker inside the
/// publishing driver's probe or the binding's teardown, or, for a device of
/// firmware provenance, on the boot thread before the engine started
/// (<see cref="DeviceRegistry.PublishFirmware"/>) and from the offer that
/// retires it. The report methods of the derived classes run in
/// the sink caller's context, interrupt context included, and must follow
/// the handler rules: no allocation, no blocking.
/// </para>
/// </summary>
internal abstract class DeviceConsumer
{
    /// <summary>A device of this consumer's kind is available.</summary>
    /// <param name="device">The device; the consumer may keep the reference until <see cref="OnWithdrawn"/>.</param>
    public abstract void OnPublished(PublishedDevice device);

    /// <summary>A device this consumer received is going away; its contract object must not be used afterwards.</summary>
    /// <param name="device">The device being withdrawn.</param>
    public abstract void OnWithdrawn(PublishedDevice device);
}
