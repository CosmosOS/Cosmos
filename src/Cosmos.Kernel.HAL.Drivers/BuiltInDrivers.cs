// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Net;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.MassStorage;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;
using Cosmos.Kernel.HAL.Drivers.Pci;
using Cosmos.Kernel.HAL.Drivers.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn;

/// <summary>
/// The catalogue of the built-in drivers written against the driver kit,
/// one method per bus and subsystem, named as the folder the drivers it
/// returns live in. <c>Global.StartKernel</c> registers what it returns
/// before the kernel's own <c>RegisterDrivers</c> runs, each behind the
/// feature switches of what it needs: this assembly cannot
/// read the switches itself, so the caller guards, and a kernel built
/// without them never calls the method and trims the drivers it names. A
/// kernel does not call it: these drivers are registered by the
/// time the kernel's RegisterDrivers runs, and a kernel's registration of
/// a built-in name is refused.
/// </summary>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public static class BuiltInDrivers
{
    /// <summary>
    /// Creates the registrations of the built-in USB host controller drivers:
    /// the xHCI driver, which binds eXtensible Host Controllers and publishes
    /// each to the kit's USB core, which then enumerates the devices behind
    /// it. Any context: it only allocates the registrations.
    /// </summary>
    /// <returns>New registrations, in the order they are to be registered.</returns>
    public static IReadOnlyList<PciDriverRegistration> CreateUsbHostRegistrations()
    {
        PciDriverRegistration[] registrations = [XhciDriver.CreateRegistration()];
        return registrations;
    }

    /// <summary>
    /// Creates the registrations of the built-in storage drivers that bind a
    /// PCI function: the AHCI driver, which binds SATA host bus adapters and
    /// publishes their disks, and the NVMe driver, which binds NVM Express
    /// controllers and publishes their namespaces. Any context: it only
    /// allocates the registrations.
    /// </summary>
    /// <returns>New registrations, in the order they are to be registered.</returns>
    public static IReadOnlyList<PciDriverRegistration> CreatePciStorageRegistrations()
    {
        PciDriverRegistration[] registrations = [AhciDriver.CreateRegistration(), NvmeDriver.CreateRegistration()];
        return registrations;
    }

    /// <summary>
    /// Creates the registrations of the built-in network drivers that bind a
    /// PCI function: the E1000E driver, which binds Intel's gigabit Ethernet
    /// controllers, and the virtio-net driver, which binds the virtio network
    /// devices on the PCI bus. Each publishes its device's link to the
    /// kernel's network stack. Any context: it only allocates the
    /// registrations.
    /// </summary>
    /// <returns>New registrations, in the order they are to be registered.</returns>
    public static IReadOnlyList<PciDriverRegistration> CreatePciNetworkRegistrations()
    {
        PciDriverRegistration[] registrations = [E1000eDriver.CreateRegistration(), VirtioNetDriver.CreateRegistration()];
        return registrations;
    }

    /// <summary>
    /// Creates the registrations of the built-in storage drivers that bind a
    /// USB interface: the mass storage driver, which binds the SCSI over
    /// Bulk-Only interfaces of USB sticks, card readers and USB disks and
    /// publishes their logical units. Any context: it only allocates the
    /// registrations.
    /// </summary>
    /// <returns>New registrations, in the order they are to be registered.</returns>
    public static IReadOnlyList<UsbDriverRegistration> CreateUsbMassStorageRegistrations()
    {
        UsbDriverRegistration[] registrations = [MassStorageDriver.CreateRegistration()];
        return registrations;
    }
}
