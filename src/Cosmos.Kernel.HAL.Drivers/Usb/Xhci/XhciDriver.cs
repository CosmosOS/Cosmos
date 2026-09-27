// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.Drivers.Pci;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.Xhci;

/// <summary>
/// The built-in xHCI driver: takes an eXtensible Host Controller from
/// firmware, resets and starts it, and publishes it to the kit's USB core,
/// which enumerates the devices on its root ports as the kit delivers it,
/// binds HAL's hub and keyboard drivers to them and leaves the rest of their
/// interfaces to the kit's USB drivers, the built-in mass storage one
/// among them. Its interrupts come through MSI-X where the platform routes
/// it, else from the timer; with neither, the USB hot-plug thread polls it.
/// </summary>
/// <remarks>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreateUsbHostRegistrations"/>, in a kernel
/// built with USB support. Its name, <c>xhci</c>, is the owner the
/// controller's PCI function records, and a name the kit refuses to a
/// kernel's own registration.
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class XhciDriver : PciDriver
{
    private const string DriverName = "xhci";

    // A serial bus controller (class 0Ch) of the USB subclass (03h),
    // programmed for xHCI (interface 30h): the UHCI, OHCI and EHCI
    // functions of the same subclass are never offered to it.
    private const byte SerialBusClass = 0x0C;
    private const byte UsbSubclass = 0x03;
    private const byte XhciProgrammingInterface = 0x30;

    /// <summary>PCI BAR index of the xHCI register block (xHCI 1.2 §5.2.1).</summary>
    private const int RegisterBarIndex = 0;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private XhciDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>xhci</c> and one match, the xHCI class, subclass and programming
    /// interface, so every xHCI controller is offered to it and no other
    /// function is. Any context: it only allocates the registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per controller it binds through it.</returns>
    public static PciDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new XhciDriver(), PciMatch.Class(SerialBusClass, UsbSubclass, XhciProgrammingInterface));

    /// <inheritdoc />
    protected override ProbeResult Probe(PciDeviceContext context)
    {
        if (!context.TryMapBar(RegisterBarIndex, out MmioRegion? registers))
        {
            context.WriteLog("BAR 0 is not a memory BAR");
            return ProbeResult.Failed;
        }

        XhciController controller;
        try
        {
            controller = XhciController.Create(context, registers);
        }
        catch (InvalidOperationException exception)
        {
            context.WriteLog($"controller setup failed: {exception.Message}");
            return ProbeResult.Failed;
        }

        try
        {
            controller.Start();
        }
        catch (InvalidOperationException exception)
        {
            // Stopped before the kit frees the rings the controller was
            // handed, which only bus mastering being off would keep it from
            // fetching.
            context.WriteLog($"controller init failed: {exception.Message}");
            controller.Halt();
            return ProbeResult.Failed;
        }

        context.PublishUsbHostController(controller);
        return ProbeResult.Bound;
    }
}
