// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using System.Diagnostics.CodeAnalysis;
using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.BootKeyboard;

/// <summary>
/// The built-in USB keyboard driver: binds every HID interface that
/// declares the boot keyboard protocol (HID 1.11 §4.2-§4.3), which every PC
/// keyboard does so firmware can use it, and publishes each as a keyboard
/// the kernel reads keys from. One instance per bound interface, and the
/// kit takes a keyboard back out of the keyboard manager when its device
/// leaves the bus.
/// </summary>
/// <remarks>
/// <para>
/// The interface is driven in boot protocol (HID 1.11 appendix B.1): each
/// report is 8 bytes, a modifier bitmap, a reserved byte and the usages of
/// up to six keys held down. Consecutive reports are diffed and every
/// change is reported as the PS/2 set 1 make code the kernel's scan maps
/// expect. The keyboard only reports changes, so holding a key does not
/// repeat it: USB leaves typematic repeat to the host, and no repeat timer
/// exists yet.
/// </para>
/// <para>
/// <c>Global.StartKernel</c> registers it, through
/// <see cref="BuiltInDrivers.CreateUsbInputRegistrations"/>, in a kernel
/// built with keyboard and USB support. Its name,
/// <c>HID boot keyboard</c>, is the owner the interfaces it binds record,
/// and a name the kit refuses to a kernel's own registration.
/// </para>
/// </remarks>
// The ID is spelled out: the kit's own constant for it is internal to
// Cosmos.Kernel.HAL, and this assembly takes no grant from it.
[Experimental("COSMOS0003")]
public sealed class BootKeyboardDriver : UsbDriver
{
    private const string DriverName = "HID boot keyboard";

    /// <summary>bInterfaceClass of HID (HID 1.11 §4.1).</summary>
    private const byte HidClass = 0x03;

    /// <summary>bInterfaceSubClass of a boot interface (HID 1.11 §4.2).</summary>
    private const byte BootSubclass = 0x01;

    /// <summary>bInterfaceProtocol of a boot keyboard (HID 1.11 §4.3).</summary>
    private const byte KeyboardProtocol = 0x01;

    // HID class requests (HID 1.11 §7.2).
    private const byte SetReportRequest = 0x09;
    private const byte SetIdleRequest = 0x0A;
    private const byte SetProtocolRequest = 0x0B;

    /// <summary>SET_PROTOCOL's value selecting the boot protocol.</summary>
    private const ushort BootProtocol = 0;

    /// <summary>SET_IDLE's value for reports on change only: duration 0 in the high byte.</summary>
    private const ushort ReportOnChange = 0;

    /// <summary>SET_REPORT's value: report type Output (2) in the high byte, report ID 0.</summary>
    private const ushort OutputReport = 0x02 << 8;

    private const int BootReportLength = 8;
    private const int ModifiersOffset = 0;
    private const int FirstKeyOffset = 2;
    private const int ModifierCount = 8;

    /// <summary>Usages below this are error codes, not keys (HID usage tables §10: 0x01 is ErrorRollOver).</summary>
    private const byte FirstKeyUsage = 0x04;
    private const byte UsageErrorRollOver = 0x01;

    /// <summary>The report before the one being handled, which every change is worked out against.</summary>
    private readonly byte[] _previousReport = new byte[BootReportLength];

    // Set by Probe. The kit hands a report to the handler and asks for the
    // lamps only once Probe returned Bound, so by then they are set; the
    // null checks below are for the compiler.
    private UsbDeviceContext? _context;
    private KeyboardReporter? _keyboard;
    private DeviceWorkItem? _lampWork;
    private byte _interfaceNumber;

    /// <summary>
    /// The lamps to send, as an output report's first byte: written from the
    /// report handler, in interrupt context, and read on the driver-work
    /// thread, which is why it is volatile.
    /// </summary>
    private volatile byte _lamps;

    // Only the registration's factory creates one: a kernel gets the driver
    // through the registration, never an instance of its own.
    private BootKeyboardDriver()
    {
    }

    /// <summary>
    /// Creates the registration the kit binds the driver through: the name
    /// <c>HID boot keyboard</c> and one match, the HID class with the boot
    /// subclass and the keyboard protocol, so every boot keyboard interface
    /// is offered to it and no other is. Any context: it only allocates the
    /// registration.
    /// </summary>
    /// <returns>A new registration; the kit creates one driver instance per interface it binds through it.</returns>
    public static UsbDriverRegistration CreateRegistration() =>
        new(DriverName, static () => new BootKeyboardDriver(),
            UsbMatch.Interface(HidClass, BootSubclass, KeyboardProtocol));

    /// <inheritdoc />
    protected override ProbeResult Probe(UsbDeviceContext context)
    {
        UsbInterfaceInfo usbInterface = context.Interface;
        if (!usbInterface.TryFindEndpoint(UsbEndpointType.Interrupt, UsbDirection.In, out UsbEndpointInfo endpoint))
        {
            context.WriteLog("no interrupt IN endpoint to read reports from");
            return ProbeResult.Declined;
        }

        if (context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetProtocolRequest, BootProtocol,
                usbInterface.Number) != UsbTransferStatus.Success)
        {
            context.WriteLog("SET_PROTOCOL(boot) failed");
            return ProbeResult.Failed;
        }

        // Report on change only. A keyboard may stall this optional request
        // and still work, so its status is not checked.
        _ = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetIdleRequest, ReportOnChange,
            usbInterface.Number);

        _context = context;
        _interfaceNumber = usbInterface.Number;

        // An output report goes out over the default pipe, which only a
        // thread may use, while the kit asks for the lamps from inside the
        // report handler: the handler records them and the work item sends
        // them. Without the scheduler there is no work item, so the keyboard
        // is published with no lamp path rather than one that could not
        // answer; its keys still arrive.
        if (context.TryCreateWorkItem(SendLampReport, out DeviceWorkItem? lampWork))
        {
            _lampWork = lampWork;
            _keyboard = context.PublishKeyboard(OnUpdateLamps);
        }
        else
        {
            _keyboard = context.PublishKeyboard();
        }

        // The endpoint starts transferring as it opens, but the kit drops the
        // reports until Probe returns Bound, then hands the keyboard to the
        // keyboard manager before it lets the first one through to OnReport.
        return context.OpenInterruptIn(endpoint, OnReport) ? ProbeResult.Bound : ProbeResult.Failed;
    }

    /// <summary>
    /// The report handler. Interrupt context: no allocation, no throw, no
    /// string, no lock.
    /// </summary>
    /// <param name="report">The 8 bytes the keyboard sent.</param>
    private void OnReport(ReadOnlySpan<byte> report)
    {
        // A report full of ErrorRollOver means too many keys are down to
        // tell which: keep the last known state (HID usage tables §10).
        if (report.Length < BootReportLength || report[FirstKeyOffset] == UsageErrorRollOver
            || _keyboard is not { } keyboard)
        {
            return;
        }

        ForwardModifierChanges(keyboard, _previousReport[ModifiersOffset], report[ModifiersOffset]);

        // Releases first, so a quick roll from one key to the next arrives in
        // the order it was typed.
        for (int i = FirstKeyOffset; i < BootReportLength; i++)
        {
            byte usage = _previousReport[i];
            if (usage >= FirstKeyUsage && !HoldsKey(report, usage))
            {
                Forward(keyboard, usage, released: true);
            }
        }

        for (int i = FirstKeyOffset; i < BootReportLength; i++)
        {
            byte usage = report[i];
            if (usage >= FirstKeyUsage && !HoldsKey(_previousReport, usage))
            {
                Forward(keyboard, usage, released: false);
            }
        }

        report[..BootReportLength].CopyTo(_previousReport);
    }

    /// <summary>
    /// Records the lamps the kit worked out from the lock keys reported.
    /// Runs in the context the lock key arrived in, an interrupt handler:
    /// the transfer itself belongs to the work item.
    /// </summary>
    /// <param name="leds">The lamps to show; their bits are an output report's.</param>
    private void OnUpdateLamps(KeyboardLeds leds)
    {
        _lamps = (byte)leds;
        _lampWork?.Schedule();
    }

    /// <summary>
    /// Sends the lamps as an output report, on the driver-work thread. It
    /// reads <see cref="_lamps"/> as it runs, so several toggles that came
    /// in before it ran send one report with the latest state.
    /// </summary>
    private void SendLampReport()
    {
        if (_context is not { } context)
        {
            return;
        }

        ReadOnlySpan<byte> report = [_lamps];
        _ = context.ControlOut(UsbRequestKind.Class, UsbRecipient.Interface, SetReportRequest, OutputReport,
            _interfaceNumber, report);
    }

    /// <summary>Reports every modifier whose bit changed between two reports.</summary>
    private static void ForwardModifierChanges(KeyboardReporter keyboard, byte previous, byte current)
    {
        int changed = previous ^ current;
        for (int bit = 0; bit < ModifierCount; bit++)
        {
            int mask = 1 << bit;
            if ((changed & mask) != 0)
            {
                keyboard.Report(BootKeyboardScanCodes.Modifiers[bit], (current & mask) == 0);
            }
        }
    }

    /// <summary>Reports one key, unless it has no set 1 make code.</summary>
    private static void Forward(KeyboardReporter keyboard, byte usage, bool released)
    {
        byte scanCode = BootKeyboardScanCodes.ToScanCode(usage);
        if (scanCode != 0)
        {
            keyboard.Report(scanCode, released);
        }
    }

    /// <summary>True when <paramref name="report"/> holds <paramref name="usage"/> in one of its key slots.</summary>
    private static bool HoldsKey(ReadOnlySpan<byte> report, byte usage)
    {
        for (int i = FirstKeyOffset; i < BootReportLength; i++)
        {
            if (report[i] == usage)
            {
                return true;
            }
        }

        return false;
    }
}
