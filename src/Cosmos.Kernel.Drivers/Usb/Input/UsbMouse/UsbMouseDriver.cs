// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Buses.Usb;
using Cosmos.Kernel.System;

namespace Cosmos.Kernel.Drivers.Usb.Input.UsbMouse;

/// <summary>
/// The HID boot mouse driver over the driver kit: binds every HID
/// interface that declares the boot mouse protocol (HID 1.11 sections 4.2
/// and 4.3), which every PC mouse does so firmware can use it, switches it
/// to the boot protocol, publishes it to the ring as <c>usb-mouse</c> and
/// opens its interrupt IN pipe, whose reports the state forwards as
/// relative movements. Everything it holds for one interface lives on a
/// <see cref="UsbMouseState"/> in <see cref="DeviceBinding.DriverState"/>.
/// <see cref="Probe"/> runs in thread context on the kit worker; the detach
/// hook does nothing, since the pipe is closed by the ledger and the
/// pointer withdrawn by the kit.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class UsbMouseDriver : Driver
{
    // --- Constants ---

    /// <summary>bInterfaceSubClass: the boot interface subclass.</summary>
    private const byte BootInterfaceSubclass = 0x01;

    /// <summary>bInterfaceProtocol: mouse.</summary>
    private const byte MouseProtocol = 0x02;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new UsbMatch(interfaceClass: UsbClassCode.Hid, interfaceSubclass: BootInterfaceSubclass, interfaceProtocol: MouseProtocol),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(UsbMouseDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Switches the interface to the boot protocol, publishes the mouse and
    /// opens its report pipe. Thread context on the kit worker; a declined
    /// or failed result makes the kit release everything acquired here, the
    /// published pointer included.
    /// </summary>
    /// <param name="binding">The mouse interface's node and the kit facilities for it.</param>
    /// <returns>Bound with the pointer published and its pipe open; declined when mouse support is compiled out or the interface has no interrupt IN endpoint; failed when the mouse refused the boot protocol or the pipe could not be opened.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The ring's mouse manager has to exist for the movements to go
        //    anywhere.
        if (!KernelFeatures.Mouse)
        {
            return ProbeResult.Declined("mouse support is compiled out");
        }

        // 2. The access and the report endpoint.
        UsbAccess usb = binding.Node.Access<UsbAccess>();
        UsbEndpoint? endpoint = usb.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        if (endpoint is null)
        {
            return ProbeResult.Declined("no interrupt IN endpoint");
        }

        // 3. The boot protocol: buttons and 8-bit deltas in the first three
        //    bytes, no report descriptor to parse.
        UsbTransferStatus status = usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface,
            UsbMouseState.SetProtocolRequest, UsbMouseState.BootProtocol, usb.InterfaceNumber);
        if (status != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed("SET_PROTOCOL(boot) failed");
        }

        // 4. Report on change only (duration 0), the idle rate HID 1.11
        //    section 7.2.4 recommends for a mouse. A mouse may stall this
        //    optional request and still work, so the result is not checked.
        usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, UsbMouseState.SetIdleRequest, 0, usb.InterfaceNumber);

        // 5. The state and the ring, before the pipe opens, so a report never
        //    finds a null sink; the sink drops reports until the consumer
        //    registered it, which happens inside the publish.
        UsbMouseState state = new();
        binding.DriverState = state;
        state.Sink = binding.PublishPointer(state);

        // 6. The report pipe.
        if (!usb.OpenInterruptPipe(binding, endpoint, state.OnReport, out _))
        {
            return ProbeResult.Failed("could not open the report pipe");
        }

        // 7.
        binding.Log("ready");
        return ProbeResult.Bound;
    }
}
