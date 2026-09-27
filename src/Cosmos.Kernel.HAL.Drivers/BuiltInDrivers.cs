// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Ahci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Storage.Nvme;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn;

/// <summary>
/// The catalogue of the built-in drivers written against the driver kit,
/// one method per subsystem. <c>Global.StartKernel</c> registers what it
/// returns before the kernel's own <c>RegisterDrivers</c> runs, each
/// subsystem behind that subsystem's feature switch: this assembly cannot
/// read the switches itself, so the caller guards, and a kernel built
/// without the subsystem never calls the method and trims the drivers it
/// names. A kernel does not call it: these drivers are registered by the
/// time the kernel's RegisterDrivers runs, and a kernel's registration of
/// a built-in name is refused.
/// </summary>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public static class BuiltInDrivers
{
    /// <summary>
    /// Creates the registrations of the built-in storage drivers: the AHCI
    /// driver, which binds SATA host bus adapters and publishes their disks,
    /// and the NVMe driver, which binds NVM Express controllers and publishes
    /// their namespaces. Any context: it only allocates the registrations.
    /// </summary>
    /// <returns>New registrations, in the order they are to be registered.</returns>
    public static IReadOnlyList<PciDriverRegistration> CreateStorageRegistrations()
    {
        PciDriverRegistration[] registrations = [AhciDriver.CreateRegistration(), NvmeDriver.CreateRegistration()];
        return registrations;
    }
}
