// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Ahci;

/// <summary>
/// The built-in AHCI driver: brings a SATA host bus adapter up in AHCI
/// mode and publishes every SATA disk behind it to the storage manager,
/// named <c>sata0</c>, <c>sata1</c>, ... across every controller in the
/// order they bind. The controller is polled, never interrupt-driven, so a
/// disk answers I/O the moment it is published: the storage manager scans
/// its partition table while the kit delivers it, before any interrupt
/// would be armed. SATAPI (optical), enclosure-management and
/// port-multiplier ports are recognised and left alone.
/// </summary>
/// <remarks>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreatePciStorageRegistrations"/>, in a kernel
/// built with storage support. Its name, <c>ahci</c>, is the owner the
/// controller's PCI function records, and a name the kit refuses to a
/// kernel's own registration.
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class AhciDriver : PciDriver
{
    private const string DriverName = "ahci";

    // A mass storage controller (class 01h) of the SATA subclass (06h),
    // programmed for AHCI 1.0 (interface 01h): the IDE-compatible SATA
    // functions q35 and many chipsets also expose are never offered to it.
    private const byte MassStorageClass = 0x01;
    private const byte SataSubclass = 0x06;
    private const byte AhciProgrammingInterface = 0x01;

    /// <summary>PCI BAR index of ABAR, the AHCI MMIO region (AHCI 1.3.1 s2.1.11).</summary>
    private const int AbarBarIndex = 5;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private AhciDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>ahci</c> and one match, the AHCI class, subclass and programming
    /// interface, so every AHCI controller is offered to it and no other
    /// function is. Any context: it only allocates the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per controller it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new AhciDriver(), PciMatch.Class(MassStorageClass, SataSubclass, AhciProgrammingInterface));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        if (!context.TryMapBar(AbarBarIndex, out MmioRegion? abar))
        {
            context.WriteLog("BAR5 is not a memory BAR");
            return ProbeResult.Failed;
        }

        // First, as early as the kit allows: firmware may have left the HBA's
        // engines running on its own command lists, and the kit turned
        // mastering off when it built this context. Before the driver moved
        // to the kit mastering was never off, and turning it back on before
        // any port is stopped is the closest to that order real machines
        // ran; an engine that tries to master meanwhile reports a host bus
        // error, which the port rebase clears.
        context.EnableBusMastering();

        AhciController controller = new(context, abar);
        if (!controller.Initialize())
        {
            context.WriteLog("controller init failed");
            return ProbeResult.Failed;
        }

        // In port order, so the names the disks were given and the order the
        // storage manager registers them in agree. A controller that came up
        // with no SATA disk binds all the same, as it did before the driver
        // moved to the kit: q35's on-board one carries only the SATAPI boot
        // CD, and it stays owned by this driver.
        IReadOnlyList<Sata> disks = controller.Ports;
        for (int i = 0; i < disks.Count; i++)
        {
            context.PublishBlockDevice(disks[i]);
        }

        return ProbeResult.Bound;
    }
}
