// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio;

/// <summary>
/// What one of a virtio PCI function's vendor-specific capabilities points
/// at, its <c>cfg_type</c> (virtio 1.2 §4.1.4): each names a BAR, an offset
/// into it and a length, and together they are how a modern virtio device
/// says where its registers are. The driver uses the first capability of
/// each type, as the specification requires.
/// </summary>
internal enum VirtioConfigType : byte
{
    /// <summary>VIRTIO_PCI_CAP_COMMON_CFG: the common configuration structure, which every device has.</summary>
    Common = 1,

    /// <summary>VIRTIO_PCI_CAP_NOTIFY_CFG: the doorbells, one per queue, spaced by the capability's multiplier.</summary>
    Notify = 2,

    /// <summary>VIRTIO_PCI_CAP_ISR_CFG: the INTx status byte, which no driver here reads; the kit disables INTx.</summary>
    Isr = 3,

    /// <summary>VIRTIO_PCI_CAP_DEVICE_CFG: the device-specific configuration, whose layout each device type sets; virtio-net keeps its MAC address and link status there.</summary>
    Device = 4
}
