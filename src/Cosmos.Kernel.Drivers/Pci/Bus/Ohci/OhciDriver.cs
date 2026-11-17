// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ohci;

/// <summary>
/// The OHCI host controller driver (OpenHCI 1.0a, the full and low-speed
/// host of USB 1.1 and the companion of EHCI) over the driver kit: binds
/// the PCI function, maps BAR0, takes the controller from the firmware,
/// builds the HCCA and the list heads, resets and programs it, scans the
/// root ports so every boot-time device's interface nodes are published
/// during the driver stage, connects the legacy line interrupt when the
/// platform routes it and polls otherwise, creates the kit's
/// <see cref="UsbBus"/> over its <see cref="OhciState"/>, and starts the
/// <c>ohci-hotplug</c> thread for the plugs after it. Device-class
/// agnostic: it addresses devices, runs control and bulk transfers and
/// opens interrupt endpoints for the class drivers bound to the interface
/// nodes, and never publishes a node itself. Everything it holds for one
/// controller lives on an <see cref="OhciState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker. Not
/// implemented: isochronous endpoints, interrupt OUT endpoints and the
/// recovery of a controller that stopped on an unrecoverable error.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class OhciDriver : Driver
{
    // --- Constants ---

    /// <summary>Serial bus controller, the class code of a USB host controller.</summary>
    private const byte SerialBusControllerClass = 0x0C;

    /// <summary>USB controller, the subclass.</summary>
    private const byte UsbControllerSubclass = 0x03;

    /// <summary>OHCI, the programming interface.</summary>
    private const byte OhciProgIf = 0x10;

    /// <summary>The base address register holding the controller's registers.</summary>
    private const int RegisterBar = 0;

    /// <summary>The legacy line is the first entry of the node's interrupt list; OHCI functions carry no message interrupt.</summary>
    private const int LineInterruptSource = 0;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: SerialBusControllerClass, subclass: UsbControllerSubclass, progIf: OhciProgIf),
    ];

    /// <summary>The next controller number; probes are serialized on the kit worker, so no lock.</summary>
    private int _nextIndex;

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(OhciDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up, enumerates the devices on its root ports
    /// and starts the hot-plug thread. Thread context on the kit worker; a
    /// declined or failed result makes the kit release everything acquired
    /// here and quiet the function again. The root port scan runs here with
    /// waits of up to 500 ms per port reset, 200 ms for the connect settle
    /// and up to 5 s per control transfer, so the driver stage lengthens by
    /// that.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the bus created, with or without devices; declined when BAR0 is not a memory window; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The wire structs, then BAR0 as the register window.
        if (Unsafe.SizeOf<OhciEndpointDescriptor>() != OhciEndpointDescriptor.Size
            || Unsafe.SizeOf<OhciTransferDescriptor>() != OhciTransferDescriptor.Size
            || Unsafe.SizeOf<OhciHcca>() != OhciHcca.Size)
        {
            return ProbeResult.Failed("the wire structs have the wrong size");
        }

        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar0 = pci.Bars[RegisterBar];
        if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < OhciProtocol.MinimumRegisterWindowBytes)
        {
            return ProbeResult.Declined("BAR0 is not a memory window");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on: the controller reads the HCCA every frame
        //    and dies with an unrecoverable error when bus mastering is off.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. The revision and the root hub descriptor, and whether the port
        //    registers fit the window.
        OhciState state = new(binding, registers);
        if (state.Revision != OhciProtocol.SupportedRevision)
        {
            return ProbeResult.Failed("unsupported OHCI revision");
        }

        if (state.RequiredRegisterLength > registers.Length)
        {
            return ProbeResult.Failed("the register block runs past BAR0");
        }

        // 4. The firmware hand-off.
        state.TakeOwnershipFromFirmware();

        // 5. The HCCA, the list heads, the interrupt tree and the pool,
        //    built now so the window after the reset holds register writes
        //    only; then the lock.
        string? failure = state.AllocateStructures();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        state.Lock = binding.CreateLock();

        // 6. The reset, then at once 7. the registers, read back (a
        //    controller that dropped the frame interval is reset and
        //    programmed once more), and the operational state.
        failure = state.Reset();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        failure = state.ProgramRegisters();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        // 8..11 run with the controller walking the lists: a failure here
        // stops it before the kit frees the pages it would keep reading.
        try
        {
            // 8, 9. The state and the bus, then the root ports (power, the
            //       connect settle, every change bit written back, the
            //       probes): every boot-time device's interface nodes are
            //       published from here, queued behind this probe, so they
            //       are offered during the driver stage.
            state.Index = _nextIndex++;
            state.Bus = new UsbBus(binding, state);
            binding.DriverState = state;
            state.ProbeRootPorts();

            // 10. The interrupt: the legacy line when the platform routes it
            //     and nobody holds it, polling otherwise. The handler is
            //     connected before the enables are written, and the enable
            //     first writes back the RHSC the boot scan's own port
            //     resets latched, so no change the scan handled raises the
            //     line.
            bool connected = binding.Node.Interrupts.Count > LineInterruptSource
                && binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptSource], state.OnInterrupt, out _);
            state.HasInterrupt = connected;
            state.IsPolling = !connected;
            if (connected)
            {
                state.EnableInterrupts();
            }

            // 11. The hot-plug thread; without a scheduler, boot-time
            //     enumeration only.
            state.HotPlugRunning = binding.TryStartThread("ohci-hotplug", state.HotPlugMain, out _);
            if (!state.HotPlugRunning)
            {
                binding.Log("hot-plug off (no scheduler)");
            }

            binding.Log($"revision 0x{state.Revision:x}, {state.PortCount} ports, {(connected ? "events via line interrupt" : "events polled")}, {state.Bus.DeviceCount} device(s)");
            return ProbeResult.Bound;
        }
        catch (Exception exception)
        {
            state.Stop();
            return ProbeResult.Failed(exception.Message);
        }
    }

    /// <summary>
    /// Releases every device the bus still holds (its EDs taken out of the
    /// lists when the hardware is present; the waits complete through the
    /// polled drain, since the kit cancelled the binding's events and
    /// disconnected its interrupt before this), then, with the hardware
    /// present, stops the controller (interrupts off, lists disabled, HCFS
    /// reset) and turns bus mastering off. Thread context on the kit
    /// worker; the kit tore the interface nodes down and joined the
    /// hot-plug thread before this, and frees every DMA page afterwards.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not OhciState state)
        {
            return;
        }

        state.Bus.ReleaseAll(reason.HardwarePresent);
        if (reason.HardwarePresent)
        {
            state.Stop();
            binding.Node.Access<PciAccess>().EnableBusMastering(false);
        }

        state.Running = false;
    }
}
