// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.HAL.DriverKit;
using Cosmos.Kernel.HAL.DriverKit.Devices;
using Cosmos.Kernel.HAL.DriverKit.Usb;

namespace Cosmos.Kernel.Drivers.Usb.Input.UsbKeyboard;

/// <summary>
/// Everything <see cref="UsbKeyboardDriver"/> holds for one bound keyboard
/// interface, hung off <see cref="DeviceBinding.DriverState"/>, and the
/// keyboard it publishes: a HID keyboard driven in boot protocol (HID 1.11
/// appendix B.1), whose 8-byte reports (a modifier bitmap, a reserved byte
/// and the usages of up to six keys held down) are diffed against the
/// previous one and forwarded to the sink as PS/2 set 1 scan codes. The
/// keyboard only reports changes, so holding a key does not repeat it: USB
/// leaves typematic repeat to the host, and no repeat timer exists yet.
/// <see cref="OnReport"/> runs in the <see cref="UsbReportHandler"/>
/// contexts and is allocation-free; <see cref="SetLeds"/> is thread
/// context, the ring's work item on the kit worker.
/// </summary>
public sealed class UsbKeyboardState : IKeyboard
{
    // --- Constants ---

    /// <summary>SET_REPORT (HID 1.11 section 7.2.2).</summary>
    internal const byte SetReportRequest = 0x09;

    /// <summary>SET_IDLE (HID 1.11 section 7.2.4).</summary>
    internal const byte SetIdleRequest = 0x0A;

    /// <summary>SET_PROTOCOL (HID 1.11 section 7.2.6).</summary>
    internal const byte SetProtocolRequest = 0x0B;

    /// <summary>SET_PROTOCOL wValue for the boot protocol.</summary>
    internal const ushort BootProtocol = 0;

    /// <summary>SET_REPORT wValue: report type Output (2) in the high byte, report ID 0.</summary>
    internal const ushort OutputReport = 0x02 << 8;

    /// <summary>Bytes of a boot protocol input report.</summary>
    internal const int BootReportLength = 8;

    /// <summary>The modifier bitmap's offset in the report.</summary>
    internal const int ModifiersOffset = 0;

    /// <summary>The first key usage's offset in the report; six follow.</summary>
    internal const int FirstKeyOffset = 2;

    /// <summary>Modifier bits in the bitmap.</summary>
    internal const int ModifierCount = 8;

    /// <summary>Usages below this are error codes, not keys (HID usage tables section 10).</summary>
    internal const byte FirstKeyUsage = 0x04;

    /// <summary>ErrorRollOver: too many keys are down to tell which.</summary>
    internal const byte UsageErrorRollOver = 0x01;

    /// <summary>Output report bit for Num Lock (HID 1.11 appendix B.1).</summary>
    internal const byte LedNumLock = 1 << 0;

    /// <summary>Output report bit for Caps Lock.</summary>
    internal const byte LedCapsLock = 1 << 1;

    /// <summary>Output report bit for Scroll Lock.</summary>
    internal const byte LedScrollLock = 1 << 2;

    // --- Private fields ---

    private readonly DeviceBinding _binding;
    private readonly UsbAccess _usb;
    private readonly byte _interfaceNumber;
    private readonly byte[] _previousReport = new byte[BootReportLength];
    private KeyboardSink? _sink;
    private volatile int _reportCount;
    private volatile int _keyEvents;
    private volatile int _ledWrites;
    private volatile byte _lastLedReport;
    private volatile UsbTransferStatus _lastLedStatus;

    // --- Constructor ---

    /// <summary>Takes the access and the interface the probe bound. Thread context, from the probe.</summary>
    /// <param name="binding">The keyboard interface's binding.</param>
    /// <param name="usb">The kit's access to the keyboard interface.</param>
    internal UsbKeyboardState(DeviceBinding binding, UsbAccess usb)
    {
        _binding = binding;
        _usb = usb;
        _interfaceNumber = usb.InterfaceNumber;
    }

    // --- IKeyboard ---

    /// <inheritdoc/>
    public string Name => "usb-keyboard";

    /// <summary>
    /// Lights the indicators the ring asks for: one SET_REPORT output report
    /// with the HID bits (Num Lock 0, Caps Lock 1, Scroll Lock 2) built from
    /// the kit's flags. Thread context, the ring's work item on the kit
    /// worker; a failed write is recorded in <see cref="LastLedStatus"/>,
    /// not logged.
    /// </summary>
    /// <param name="leds">The indicators to light.</param>
    public void SetLeds(KeyboardLeds leds)
    {
        byte report = (byte)(((leds & KeyboardLeds.NumLock) != 0 ? LedNumLock : 0)
            | ((leds & KeyboardLeds.CapsLock) != 0 ? LedCapsLock : 0)
            | ((leds & KeyboardLeds.ScrollLock) != 0 ? LedScrollLock : 0));
        _lastLedStatus = _usb.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, SetReportRequest, OutputReport, _interfaceNumber, [report]);
        _lastLedReport = report;
        _ledWrites++;
    }

    // --- Properties the suites read ---

    /// <summary>The sink keys go to, set by the probe before the report pipe opens; a report before the consumer registered it is dropped by the sink. Any context.</summary>
    public KeyboardSink? Sink
    {
        get => _sink;
        internal set => _sink = value;
    }

    /// <summary>How many whole reports the handler took. Any context.</summary>
    public int ReportCount => _reportCount;

    /// <summary>How many key presses and releases the handler forwarded to the sink. Any context.</summary>
    public int KeyEvents => _keyEvents;

    /// <summary>How many times <see cref="SetLeds"/> wrote an output report. Any context.</summary>
    public int LedWrites => _ledWrites;

    /// <summary>The last output report written by <see cref="SetLeds"/>. Any context.</summary>
    public byte LastLedReport => _lastLedReport;

    /// <summary>The status of the last output report written by <see cref="SetLeds"/>. Any context.</summary>
    public UsbTransferStatus LastLedStatus => _lastLedStatus;

    // --- The report handler ---

    /// <summary>
    /// The report pipe's handler: a short report or one full of
    /// ErrorRollOver keeps the last known state; otherwise the modifier
    /// changes go out first, then the releases (every usage of the previous
    /// report not held now, so a quick roll from one key to the next arrives
    /// in the order it was typed), then the presses; a usage with no scan
    /// code is dropped; the report becomes the previous one. The
    /// <see cref="UsbReportHandler"/> contexts: interrupt context on a
    /// controller with a message interrupt, otherwise whichever thread drains
    /// the controller's events under its lock. Allocation-free; calls only
    /// the sink.
    /// </summary>
    /// <param name="report">The bytes the keyboard sent in this transfer.</param>
    internal void OnReport(ReadOnlySpan<byte> report)
    {
        if (report.Length < BootReportLength || report[FirstKeyOffset] == UsageErrorRollOver)
        {
            return;
        }

        _reportCount++;
        ForwardModifierChanges(_previousReport[ModifiersOffset], report[ModifiersOffset]);

        for (int i = FirstKeyOffset; i < BootReportLength; i++)
        {
            byte usage = _previousReport[i];
            if (usage >= FirstKeyUsage && !HoldsKey(report, usage))
            {
                Forward(usage, released: true);
            }
        }

        for (int i = FirstKeyOffset; i < BootReportLength; i++)
        {
            byte usage = report[i];
            if (usage >= FirstKeyUsage && !HoldsKey(_previousReport, usage))
            {
                Forward(usage, released: false);
            }
        }

        report.Slice(0, BootReportLength).CopyTo(_previousReport);
    }

    /// <summary>Reports every modifier bit that changed, released when the bit is now clear. Handler context.</summary>
    /// <param name="previous">The previous report's modifier bitmap.</param>
    /// <param name="current">This report's modifier bitmap.</param>
    private void ForwardModifierChanges(byte previous, byte current)
    {
        int changed = previous ^ current;
        for (int bit = 0; bit < ModifierCount; bit++)
        {
            int mask = 1 << bit;
            if ((changed & mask) != 0)
            {
                Report(UsbKeyboardKeyMap.ModifierScanCodes[bit], (current & mask) == 0);
            }
        }
    }

    /// <summary>Reports one key usage through the map; a usage with no scan code is dropped. Handler context.</summary>
    /// <param name="usage">The HID usage.</param>
    /// <param name="released">True for a release.</param>
    private void Forward(byte usage, bool released)
    {
        ReadOnlySpan<byte> scanCodes = UsbKeyboardKeyMap.UsageScanCodes;
        byte scanCode = usage < scanCodes.Length ? scanCodes[usage] : (byte)0;
        if (scanCode != 0)
        {
            Report(scanCode, released);
        }
    }

    /// <summary>Hands one scan code to the sink and counts it. Handler context.</summary>
    /// <param name="scanCode">The PS/2 set 1 scan code.</param>
    /// <param name="released">True for a release.</param>
    private void Report(byte scanCode, bool released)
    {
        _keyEvents++;
        KeyboardSink? sink = _sink;
        sink?.Report(scanCode, released);
    }

    /// <summary>True when <paramref name="usage"/> is among the six key slots of <paramref name="report"/>. Handler context.</summary>
    /// <param name="report">A boot protocol report.</param>
    /// <param name="usage">The HID usage looked for.</param>
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
