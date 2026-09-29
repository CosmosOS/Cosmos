// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.Devices.Virtio;
using Cosmos.Kernel.HAL.Pci;

namespace Cosmos.Kernel.HAL.Devices.Graphic.Virtio;

/// <summary>
/// Finds the virtio-gpu PCI function and brings the HAL's
/// <see cref="VirtioGpu"/> up over it. The one piece of the HAL's virtio
/// registry that stays until the display stage moves virtio-gpu into the
/// driver kit with the display kinds: every other virtio function is bound
/// by the kit's VirtioPciTransportDriver, which declines the GPU type so this
/// probe keeps it. Thread context, from the HAL library initializer, after
/// <see cref="Cosmos.Kernel.HAL.Interfaces.IPlatformInitializer.InitializeHardware"/>
/// installed the platform's MSI binder.
/// </summary>
internal static class VirtioGpuProbe
{
    /// <summary>The virtio device type of a GPU (virtio spec section 5.7).</summary>
    private const uint GpuDeviceType = VirtioTransport.DeviceTypeGpu;

    /// <summary>
    /// The GPU that came up, or null when no virtio-gpu function exists,
    /// the probe has not run, or the device failed to initialize. Read by
    /// the ring's canvas selection. Any context once the probe returned.
    /// </summary>
    public static VirtioGpu? Device { get; private set; }

    /// <summary>
    /// Walks the functions <see cref="PciManager.Setup"/> discovered and
    /// initializes the first virtio-gpu that comes up; a function another
    /// HAL driver already claimed is skipped. Returns at once when PCI was
    /// not enumerated. Thread context, interrupts disabled.
    /// </summary>
    public static void InitializePciBus()
    {
        if (PciManager.Devices is null)
        {
            return;
        }

        for (uint i = 0; i < PciManager.Count; i++)
        {
            PciDevice pci = PciManager.Devices[i];
            if (pci.VendorId != VirtioPciTransport.VirtioVendorId || pci.Claimed)
            {
                continue;
            }

            if (VirtioPciTransport.GetDeviceType(pci) != GpuDeviceType)
            {
                continue;
            }

            VirtioPciTransport? transport = VirtioPciTransport.TryCreate(pci);
            if (transport is null)
            {
                continue;
            }

            VirtioGpu gpu = new(transport);
            gpu.Initialize();
            if (!gpu.Ready)
            {
                continue;
            }

            pci.Claimed = true;
            Device = gpu;
            return;
        }
    }
}
