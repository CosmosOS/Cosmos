// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.DriverKit.Virtio;

/// <summary>
/// The bits of the virtio device status register (specification section
/// 2.1) and the one feature bit the kit negotiates by itself. Any context.
/// </summary>
internal static class VirtioStatus
{
    /// <summary>ACKNOWLEDGE: the guest has noticed the device.</summary>
    internal const byte Acknowledge = 1;

    /// <summary>DRIVER: the guest knows how to drive the device.</summary>
    internal const byte Driver = 2;

    /// <summary>DRIVER_OK: the driver is set up and ready to drive the device.</summary>
    internal const byte DriverOk = 4;

    /// <summary>FEATURES_OK: the driver has acknowledged the features it understands; negotiation is over.</summary>
    internal const byte FeaturesOk = 8;

    /// <summary>DEVICE_NEEDS_RESET: the device has experienced an error it cannot recover from.</summary>
    internal const byte DeviceNeedsReset = 64;

    /// <summary>FAILED: the guest gave up on the device.</summary>
    internal const byte Failed = 128;

    /// <summary>VIRTIO_F_VERSION_1, bit 32: the device is a virtio 1.x device; the kit takes it whenever it is offered.</summary>
    internal const ulong Version1 = 1UL << 32;
}
