// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Nvme;

/// <summary>
/// The built-in NVMe driver: brings an NVM Express controller up and
/// publishes each namespace it can drive to the storage manager, named
/// <c>nvme0n1</c>, <c>nvme1n1</c>, ... by the order the controllers bind.
/// Completions interrupt through MSI-X where the platform routes it. The
/// kit delivers the namespaces, and the storage manager scans their
/// partition tables, before it arms that interrupt, so until the handler
/// first runs each command is completed by the thread that issued it,
/// polling the completion queue; so is every command where the kit could
/// only poll the handler from the timer, or grant no interrupt at all.
/// </summary>
/// <remarks>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreatePciStorageRegistrations"/>, in a kernel
/// built with storage support. Its name, <c>nvme</c>, is the owner the
/// controller's PCI function records, and a name the kit refuses to a
/// kernel's own registration.
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class NvmeDriver : PciDriver
{
    private const string DriverName = "nvme";

    // A mass storage controller (class 01h) of the non-volatile memory
    // subclass (08h) with the NVM Express interface (02h): an NVMe
    // administrative controller (03h) has no I/O queues to create, and a
    // legacy NVMHCI one (01h) speaks another register set.
    private const byte MassStorageClass = 0x01;
    private const byte NonVolatileMemorySubclass = 0x08;
    private const byte NvmExpressProgrammingInterface = 0x02;

    /// <summary>PCI BAR index of the controller's registers (NVMe 1.4 s2.1.10, MLBAR/MUBAR).</summary>
    private const int RegisterBarIndex = 0;

    /// <summary>Config offsets of BAR 0 and BAR 1, the halves of the register BAR's address, for the log.</summary>
    private const ushort Bar0ConfigOffset = 0x10;
    private const ushort Bar1ConfigOffset = 0x14;

    /// <summary>Address bits of a memory BAR's low half; the low four hold its type flags.</summary>
    private const uint MemoryBarAddressMask = 0xFFFFFFF0;

    private const int BarUpperHalfShift = 32;

    // Across every controller, so each namespace gets a name of its own
    // ("nvme0n1", "nvme1n1", ...). Probes run one at a time.
    private static int s_nextIndex;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private NvmeDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>nvme</c> and one match, the NVM Express class, subclass and
    /// programming interface, so every NVMe controller is offered to it and
    /// no other function is. Any context: it only allocates the
    /// registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per controller it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new NvmeDriver(), PciMatch.Class(MassStorageClass, NonVolatileMemorySubclass, NvmExpressProgrammingInterface));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        int index = s_nextIndex++;

        if (!context.TryMapBar(RegisterBarIndex, out MmioRegion? registers))
        {
            context.WriteLog("BAR 0 is not a memory BAR");
            return ProbeResult.Failed;
        }

        ulong registerAddress = ((ulong)context.Function.ReadConfig32(Bar1ConfigOffset) << BarUpperHalfShift)
            | (context.Function.ReadConfig32(Bar0ConfigOffset) & MemoryBarAddressMask);
        context.WriteLog($"registers at 0x{registerAddress:X}");

        // Bus mastering stays off until the controller is reset:
        // Initialize turns it on once the admin queues are programmed.
        if (!NvmeController.TryCreate(context, registers, index, out NvmeController? controller))
        {
            return ProbeResult.Failed;
        }

        try
        {
            if (!controller.Initialize())
            {
                controller.Quiesce();
                return ProbeResult.Failed;
            }
        }
        catch (IOException exception)
        {
            context.WriteLog($"controller init failed: {exception.Message}");
            controller.Quiesce();
            return ProbeResult.Failed;
        }

        // In namespace ID order, the order the storage manager registers
        // them in. A controller with no namespace it can drive binds all
        // the same, as it did before the driver moved to the kit.
        IReadOnlyList<NvmeNamespace> namespaces = controller.Namespaces;
        for (int i = 0; i < namespaces.Count; i++)
        {
            context.PublishBlockDevice(namespaces[i]);
        }

        return ProbeResult.Bound;
    }
}
