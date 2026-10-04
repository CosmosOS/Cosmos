// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;
using Cosmos.Kernel.System;

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// The HID boot keyboard driver over the driver kit: binds every HID
/// interface that declares the boot keyboard protocol (HID 1.11 sections
/// 4.2 and 4.3), which every PC keyboard does so firmware can use it,
/// switches it to the boot protocol, publishes it to the ring as
/// <c>usb-keyboard</c> and opens its interrupt IN pipe, whose reports the
/// state diffs into scan codes. Everything it holds for one interface lives
/// on a <see cref="UsbKeyboardState"/> in
/// <see cref="DeviceBinding.DriverState"/>. <see cref="Probe"/> runs in
/// thread context on the kit worker; the detach hook does nothing, since
/// the pipe is closed by the ledger and the keyboard withdrawn by the kit.
/// </summary>
[Driver(Feature = DriverFeature.Usb)]
public sealed class UsbKeyboardDriver : Driver
{
    // --- Constants ---

    /// <summary>bInterfaceSubClass: the boot interface subclass.</summary>
    private const byte BootInterfaceSubclass = 0x01;

    /// <summary>bInterfaceProtocol: keyboard.</summary>
    private const byte KeyboardProtocol = 0x01;

    // --- Private fields ---

    private readonly DeviceMatch[] _matches =
    [
        new UsbMatch(interfaceClass: UsbClassCode.Hid, interfaceSubclass: BootInterfaceSubclass, interfaceProtocol: KeyboardProtocol),
    ];

    // --- Properties ---

    /// <inheritdoc/>
    public override string Name => nameof(UsbKeyboardDriver);

    /// <inheritdoc/>
    public override ReadOnlySpan<DeviceMatch> Matches => _matches;

    // --- Public methods ---

    /// <summary>
    /// Switches the interface to the boot protocol, publishes the keyboard
    /// and opens its report pipe. Thread context on the kit worker; a
    /// declined or failed result makes the kit release everything acquired
    /// here, the published keyboard included.
    /// </summary>
    /// <param name="binding">The keyboard interface's node and the kit facilities for it.</param>
    /// <returns>Bound with the keyboard published and its pipe open; declined when keyboard support is compiled out or the interface has no interrupt IN endpoint; failed when the keyboard refused the boot protocol or the pipe could not be opened.</returns>
    public override ProbeResult Probe(DeviceBinding binding)
    {
        // 1. The ring's keyboard manager has to exist for the keyboard to
        //    go anywhere.
        if (!KernelFeatures.Keyboard)
        {
            return ProbeResult.Declined("keyboard support is compiled out");
        }

        // 2. The access and the report endpoint.
        UsbAccess usb = binding.Node.Access<UsbAccess>();
        UsbEndpoint? endpoint = usb.FindEndpoint(UsbEndpointType.Interrupt, isIn: true);
        if (endpoint is null)
        {
            return ProbeResult.Declined("no interrupt IN endpoint");
        }

        // 3. The boot protocol: 8-byte reports, no report descriptor to parse.
        UsbTransferStatus status = usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface,
            UsbKeyboardState.SetProtocolRequest, UsbKeyboardState.BootProtocol, usb.InterfaceNumber);
        if (status != UsbTransferStatus.Success)
        {
            return ProbeResult.Failed("SET_PROTOCOL(boot) failed");
        }

        // 4. Report on change only (duration 0). A keyboard may stall this
        //    optional request and still work, so the result is not checked.
        usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, UsbKeyboardState.SetIdleRequest, 0, usb.InterfaceNumber);

        // 5. The state and the ring, before the pipe opens, so a report never
        //    finds a null sink; the sink drops reports until the consumer
        //    registered it, which happens inside the publish.
        UsbKeyboardState state = new(binding, usb);
        binding.DriverState = state;
        state.Sink = binding.PublishKeyboard(state);

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
