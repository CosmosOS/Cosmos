// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio;

/// <summary>
/// The device status bits a driver drives a virtio device through
/// (virtio 1.2 §2.1), written to the common configuration's
/// <c>device_status</c> byte. The driver adds them in order, and a device
/// that will not take one has refused the step.
/// </summary>
[Flags]
internal enum VirtioDeviceStatus : byte
{
    /// <summary>Nothing set, which is also what a reset leaves behind.</summary>
    Reset = 0,

    /// <summary>ACKNOWLEDGE: the driver has found the device.</summary>
    Acknowledge = 1,

    /// <summary>DRIVER: a driver that knows how to drive it is coming up.</summary>
    Driver = 2,

    /// <summary>DRIVER_OK: the driver is done setting up, and the device may start.</summary>
    DriverOk = 4,

    /// <summary>FEATURES_OK: the driver has chosen its features, which the device accepts by leaving the bit set.</summary>
    FeaturesOk = 8,

    /// <summary>DEVICE_NEEDS_RESET: the device has given up, and only a reset brings it back.</summary>
    NeedsReset = 64,

    /// <summary>FAILED: the driver has given up on the device.</summary>
    Failed = 128
}
