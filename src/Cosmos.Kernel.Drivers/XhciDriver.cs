// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Runtime.CompilerServices;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Pci;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The xHCI host controller driver (eXtensible Host Controller Interface
/// 1.2) over the driver kit: binds the PCI function, maps BAR0, takes the
/// controller from the firmware, resets and programs it, connects message
/// interrupt 0 when the platform routes it and polls otherwise, creates
/// the kit's <see cref="UsbBus"/> over its <see cref="XhciState"/>, scans
/// the root ports so every boot-time device's interface nodes are
/// published during the driver stage, and starts the <c>xhci-hotplug</c>
/// thread for the plugs after it. Device-class agnostic: it addresses
/// devices, runs control and bulk transfers and opens interrupt endpoints
/// for the class drivers bound to the interface nodes, and never publishes
/// a node itself. Everything it holds for one controller lives on an
/// <see cref="XhciState"/> in <see cref="DeviceBinding.DriverState"/>.
/// <see cref="Probe"/> and <see cref="OnDetach"/> run in thread context on
/// the kit worker. Not implemented: isochronous endpoints, interrupt OUT
/// endpoints and streams.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class XhciDriver : Driver
{
    // --- Constants ---

    /// <summary>Serial bus controller, the class code of a USB host controller.</summary>
    private const byte SerialBusControllerClass = 0x0C;

    /// <summary>USB controller, the subclass.</summary>
    private const byte UsbControllerSubclass = 0x03;

    /// <summary>xHCI, the programming interface.</summary>
    private const byte XhciProgIf = 0x30;

    /// <summary>The base address register holding the controller's registers.</summary>
    private const int RegisterBar = 0;

    /// <summary>The least BAR0 may span: the capability registers.</summary>
    private const ulong MinimumRegisterWindowBytes = 0x20;

    /// <summary>Interrupter 0 is routed to message 0, the second entry of the node's interrupt list after the legacy line.</summary>
    private const int MessageInterruptSource = 1;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new PciMatch(classCode: SerialBusControllerClass, subclass: UsbControllerSubclass, progIf: XhciProgIf),
    ];

    /// <summary>The next controller number; probes are serialized on the kit worker, so no lock.</summary>
    private int _nextIndex;

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(XhciDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Brings the controller up, enumerates the devices on its root ports
    /// and starts the hot-plug thread. Thread context on the kit worker; a
    /// declined or failed result makes the kit release everything acquired
    /// here and quiet the function again. The root port scan runs here with
    /// waits of up to 500 ms per port reset, 200 ms for the connect settle
    /// and up to 5 s per command, so the driver stage lengthens by that.
    /// </summary>
    /// <param name="binding">The function's node and the kit facilities for it.</param>
    /// <returns>Bound with the bus created, with or without devices; declined when BAR0 is not a memory window; failed when the controller did not come up.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The wire struct, then BAR0 as the register window.
        if (Unsafe.SizeOf<XhciTrb>() != XhciTrb.Size)
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

        // 2. Decoding and DMA on; memory space before the message request,
        //    since the kit refuses a message while decode is off.
        pci.EnableMemorySpace(true);
        pci.EnableBusMastering(true);

        // 3. The capability registers, and whether the four register sets
        //    fit the window.
        XhciState state = new(binding, registers);
        if (state.RequiredRegisterLength > registers.Length)
        {
            return ProbeResult.Failed("the register block runs past BAR0");
        }

        if (!state.Supports4KiBPages)
        {
            return ProbeResult.Failed("the controller does not support 4 KiB pages");
        }

        // 4. The command ring, the event ring, the device context base
        //    address array and the scratchpad; the events and the lock.
        string? failure = state.AllocateStructures();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        state.Lock = binding.CreateLock();

        // 5. The firmware hand-off, the port protocols, the reset.
        state.TakeOwnershipFromFirmware();
        state.ReadSupportedProtocols();
        failure = state.Reset();
        if (failure is not null)
        {
            return ProbeResult.Failed(failure);
        }

        // 6. The operational and interrupter registers, ERSTBA last.
        state.ProgramRegisters();

        // 7. The interrupt: message 0 when the platform routes it, polling
        //    otherwise; the legacy line is never requested. The handler is
        //    live from here.
        int messages = pci.MessageInterruptCount;
        bool connected = false;
        if (messages >= 1 && binding.Node.Interrupts.Count > MessageInterruptSource)
        {
            connected = binding.TryRequestInterrupt(binding.Node.Interrupts[MessageInterruptSource], state.OnInterrupt, out _);
        }

        state.HasInterrupt = connected;
        state.IsPolling = !connected;
        if (connected)
        {
            state.EnableInterrupter();
        }

        // 8. Run.
        if (!state.Start(connected))
        {
            return ProbeResult.Failed("the controller did not start");
        }

        // 9. The state and the bus, then the root ports: every boot-time
        //    device's interface nodes are published from here, queued behind
        //    this probe, so they are offered during the driver stage.
        state.Index = _nextIndex++;
        state.Bus = new UsbBus(binding, state);
        binding.DriverState = state;
        state.ProbeRootPorts();

        // 10. The hot-plug thread; without a scheduler, boot-time
        //     enumeration only.
        state.HotPlugRunning = binding.TryStartThread("xhci-hotplug", state.HotPlugMain, out _);
        if (!state.HotPlugRunning)
        {
            binding.Log("hot-plug off (no scheduler)");
        }

        // 11.
        binding.Log($"version 0x{state.Version:x}, {state.MaxSlots} slots, {state.MaxPorts} ports, {state.ContextSize}-byte contexts, {state.ScratchpadBuffers} scratchpad buffers, {(connected ? "events via message interrupt" : "events polled")}, {state.Bus.DeviceCount} device(s)");
        return ProbeResult.Bound;
    }

    /// <summary>
    /// Releases every device the bus still holds (its slots disabled when
    /// the hardware is present; the commands complete through the polled
    /// wait, since the kit cancelled the binding's events and disconnected
    /// its interrupt before this), then, with the hardware present, stops
    /// the controller (Run and INTE cleared, HCH awaited within 100 ms, a
    /// miss ignored) and turns bus mastering off. Thread context on the kit
    /// worker; the kit tore the interface nodes down and joined the
    /// hot-plug thread before this, and frees every DMA page afterwards.
    /// </summary>
    /// <param name="binding">The binding being torn down; its window is still valid.</param>
    /// <param name="reason">Why, and whether the hardware is still there.</param>
    public override void OnDetach(DeviceBinding binding, DetachReason reason)
    {
        if (binding.DriverState is not XhciState state)
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
