// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e.Registers;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.E1000e;

/// <summary>
/// The built-in Intel E1000E driver: brings an Intel gigabit Ethernet
/// controller up and publishes its link to the kernel's network stack, which
/// sends through it and receives the frames the driver delivers. One instance
/// per bound function, and the link of the first one to bind becomes the
/// primary adapter unless the platform already registered a network device.
/// </summary>
/// <remarks>
/// <para>
/// It matches every Ethernet controller and takes the Intel ones, which is
/// the set it claimed when it was <c>Cosmos.Kernel.HAL.X64</c>'s and HAL
/// brought it up itself, rather than the 82574 family alone. That is the
/// weakest match the kit offers, so a kernel's own driver, named to a vendor
/// and device ID, is offered the function first and takes it: a board this
/// driver only half fits is a driver away from being driven properly.
/// </para>
/// <para>
/// Nothing in it is architecture specific: the registers are in a memory BAR,
/// the rings are DMA memory the kit allocates, and the interrupt comes
/// through MSI-X where the platform routes it and from the timer where it
/// does not. So it binds a controller on either architecture, which it could
/// not while it lived in the x64 HAL.
/// </para>
/// <para>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreatePciNetworkRegistrations"/>, in a kernel
/// built with network support. Its name, <c>e1000e</c>, is the owner the
/// function records, and a name the kit refuses to a kernel's own
/// registration.
/// </para>
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class E1000eDriver : PciDriver
{
    private const string DriverName = "e1000e";

    /// <summary>Intel's PCI vendor ID, the one this driver's register layout belongs to.</summary>
    private const ushort IntelVendorId = 0x8086;

    // A network controller (class 02h) of the Ethernet subclass (00h) with
    // the one programming interface the subclass defines (00h).
    private const byte NetworkControllerClass = 0x02;
    private const byte EthernetSubclass = 0x00;
    private const byte EthernetProgrammingInterface = 0x00;

    /// <summary>PCI BAR index of the controller's registers.</summary>
    private const int RegisterBarIndex = 0;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private E1000eDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>e1000e</c> and one match, the Ethernet class, subclass and
    /// programming interface, with the vendor checked in the probe, since the
    /// kit's match table names either a class or a device, not both. Any
    /// context: it only allocates the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per controller it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new E1000eDriver(),
            PciMatch.Class(NetworkControllerClass, EthernetSubclass, EthernetProgrammingInterface));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        // Declined rather than failed: the function is someone else's NIC, and
        // it has to stay free for the driver that speaks its registers.
        if (context.Function.VendorId != IntelVendorId)
        {
            return ProbeResult.Declined;
        }

        if (!context.TryMapBar(RegisterBarIndex, out MmioRegion? registers))
        {
            context.WriteLog("BAR 0 is not a memory BAR");
            return ProbeResult.Declined;
        }

        if (!ControllerRegisters.Covers(registers))
        {
            context.WriteLog($"BAR 0 is {registers.Length} bytes, too short for the register block");
            return ProbeResult.Declined;
        }

        E1000eController controller = new(context, new ControllerRegisters(registers));
        return controller.TryStart() ? ProbeResult.Bound : ProbeResult.Failed;
    }
}
