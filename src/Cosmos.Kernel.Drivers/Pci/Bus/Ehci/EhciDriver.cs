// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Pci;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.HAL.DriverKit.Resources;

namespace Cosmos.Kernel.Drivers.Pci.Bus.Ehci;

/// <summary>
/// The EHCI host controller driver (Enhanced Host Controller Interface
/// 1.0, the USB 2.0 host) over the driver kit: binds the PCI function,
/// maps BAR0, takes the controller from the firmware, builds the periodic
/// and async schedules, resets and programs it, connects the legacy line
/// interrupt when the platform routes it and polls otherwise, creates the
/// kit's <see cref="UsbBus"/> over its <see cref="EhciState"/>, scans the
/// root ports so every boot-time high-speed device's interface nodes are
/// published during the driver stage (full and low-speed devices go to a
/// companion controller, whose own driver enumerates them), and starts
/// the <c>ehci-hotplug</c> thread for the plugs after it. Device-class
/// agnostic: it addresses devices, runs control and bulk transfers and
/// opens interrupt endpoints for the class drivers bound to the interface
/// nodes, and never publishes a node itself. Everything it holds for one
/// controller lives on an <see cref="EhciState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> and
/// <see cref="OnDetach"/> run in thread context on the kit worker. Not
/// implemented: isochronous endpoints, interrupt OUT endpoints, 64-bit
/// data structures above 4 GiB, FSTNs and the recovery of a controller
/// that halted on a host system error.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class EhciDriver : Driver
{
    // --- Constants ---

    /// <summary>Serial bus controller, the class code of a USB host controller.</summary>
    private const byte SerialBusControllerClass = 0x0C;

    /// <summary>USB controller, the subclass.</summary>
    private const byte UsbControllerSubclass = 0x03;

    /// <summary>EHCI, the programming interface.</summary>
    private const byte EhciProgIf = 0x20;

    /// <summary>The base address register holding the controller's registers.</summary>
    private const int RegisterBar = 0;

    /// <summary>The least BAR0 may span: the capability registers through HCSP-PORTROUTE.</summary>
    private const ulong MinimumRegisterWindowBytes = 0x10;

    /// <summary>The legacy line is the first entry of the node's interrupt list; EHCI functions carry no message interrupt the kit models.</summary>
    private const int LineInterruptSource = 0;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: SerialBusControllerClass, subclass: UsbControllerSubclass, progIf: EhciProgIf),
    ];

    /// <summary>The next controller number; probes are serialized on the kit worker, so no lock.</summary>
    private int _nextIndex;

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(EhciDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up, enumerates the high-speed devices on its
    /// root ports and starts the hot-plug thread. Thread context on the
    /// kit worker; a declined or failed result makes the kit release
    /// everything acquired here and quiet the function again. The root
    /// port scan runs here with waits of about 70 ms per connected port,
    /// 200 ms for the connect settle and up to 5 s per transfer, so the
    /// driver stage lengthens by that.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the bus created, with or without devices; declined when BAR0 is not a memory window; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The wire structs, then BAR0 as the register window.
        if (Unsafe.SizeOf<EhciQueueHead>() != EhciQueueHead.Size || Unsafe.SizeOf<EhciTransferDescriptor>() != EhciTransferDescriptor.Size)
        {
            return ProbeResult.Failed("the wire structs have the wrong size");
        }

        PciAccess pci = binding.Node.Access<PciAccess>();
        PciBar bar0 = pci.Bars[RegisterBar];
        if (!bar0.IsAssigned || bar0.IsIo || bar0.Length < MinimumRegisterWindowBytes)
        {
            return ProbeResult.Declined("BAR0 is not a memory window");
        }

        RegisterWindow registers = binding.MapRegisters(RegisterBar);

        // 2. Decoding and DMA on.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. The capability registers, whether the port registers fit the
        //    window, and the lock every pool access runs under.
        EhciState state = new(binding, registers);
        if (state.RequiredRegisterLength > registers.Length)
        {
            return ProbeResult.Failed("the register block runs past BAR0");
        }

        state.Lock = binding.CreateLock();

        // 4. The firmware hand-off.
        state.TakeOwnershipFromFirmware(pci);

        // 5. The frame list, the pool, the async head and the interval
        //    heads, both schedules built: nothing below allocates, so the
        //    window between the reset and CONFIGFLAG, during which every
        //    port is routed to the companions, holds register writes only.
        string? failure = state.AllocateStructures();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        // 6. The reset.
        failure = state.Reset();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        // 7. The operational registers, then run.
        if (!state.Start())
        {
            return ProbeResult.Failed("the controller did not start");
        }

        // 8 to 10 run against a running controller: any failure stops it
        //    first, since the kit runs no OnDetach for a failed probe but
        //    frees the DMA pages, which a running controller would walk.
        try
        {
            // 8. Every port routed here.
            state.Configure();

            // 9. The interrupt: the legacy line when the platform routes it
            //    and nobody else holds it, polling otherwise; only then are
            //    the controller's enables turned on, so a polled
            //    controller's pin never asserts. The handler is live from
            //    here.
            bool connected = binding.Node.Interrupts.Count > LineInterruptSource
                && binding.TryRequestInterrupt(binding.Node.Interrupts[LineInterruptSource], state.OnInterrupt, out _);
            state.HasInterrupt = connected;
            state.IsPolling = !connected;
            if (connected)
            {
                state.EnableInterrupts();
            }

            // 10. The state and the bus, then the root ports: every
            //     boot-time device's interface nodes are published from
            //     here, queued behind this probe, so they are offered
            //     during the driver stage; then the hot-plug thread,
            //     without a scheduler boot-time enumeration only.
            state.Index = _nextIndex++;
            state.Bus = new UsbBus(binding, state);
            binding.DriverState = state;
            state.ProbeRootPorts();

            state.HotPlugRunning = binding.TryStartThread("ehci-hotplug", state.HotPlugMain, out _);
            if (!state.HotPlugRunning)
            {
                binding.Log("hot-plug off (no scheduler)");
            }

            binding.Log($"version 0x{state.Version:x}, {state.PortCount} ports, {state.CompanionCount} companion(s) over {state.CompanionPortCount} ports, {(connected ? "events via line interrupt" : "events polled")}, {state.Bus.DeviceCount} device(s)");
            return ProbeResult.Bound;
        }
        catch (Exception exception)
        {
            state.Stop();
            state.Running = false;
            return ProbeResult.Failed(exception.Message);
        }
    }

    /// <summary>
    /// Releases every device the bus still holds (their queue heads
    /// unlinked through the doorbell when the hardware is present; the
    /// waits complete through the polled path, since the kit cancelled
    /// the binding's events and disconnected its interrupt before this),
    /// then, with the hardware present, stops the controller (the
    /// enables off, RS cleared, HCH awaited within 100 ms, a miss
    /// ignored) and turns bus mastering off. With the hardware gone the
    /// controller is marked not running first, so nothing is written to
    /// a function that is no longer there. Thread context on the kit
    /// worker; the kit tore the interface nodes down and joined the
    /// hot-plug thread before this, and frees every DMA page afterwards.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not EhciState state)
        {
            return;
        }

        if (!reason.HardwarePresent)
        {
            state.Running = false;
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
