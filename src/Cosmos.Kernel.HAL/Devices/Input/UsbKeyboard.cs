// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

using Cosmos.Kernel.Core.IO;
using Cosmos.Kernel.HAL.Devices.Usb;
using Cosmos.Kernel.HAL.Drivers.Usb;
using Cosmos.Kernel.HAL.Interfaces.Devices;

namespace Cosmos.Kernel.HAL.Devices.Input;

/// <summary>
/// A USB HID keyboard driven in boot protocol (HID 1.11 appendix B.1): each
/// report is 8 bytes, a modifier bitmap, a reserved byte and the usages of
/// up to six keys held down. Consecutive reports are diffed and every
/// change is forwarded as the PS/2 set 1 scan code the scan maps expect,
/// with the E0 prefix dropped, as the PS/2 driver reports it (Right Alt
/// being <see cref="IKeyboardDevice.RightAltScanCode"/>).
///
/// <para>The keyboard only reports changes, so holding a key does not
/// repeat it: USB leaves typematic repeat to the host, and no repeat timer
/// exists yet.</para>
/// </summary>
internal sealed class UsbKeyboard : KeyboardDevice
{
    // HID class requests (HID 1.11 §7.2).
    private const byte SetReportRequest = 0x09;
    private const byte SetIdleRequest = 0x0A;
    private const byte SetProtocolRequest = 0x0B;
    private const ushort BootProtocol = 0;

    /// <summary>SET_REPORT wValue: report type Output (2) in the high byte, report ID 0.</summary>
    private const ushort OutputReport = 0x02 << 8;

    private const int BootReportLength = 8;
    private const int ModifiersOffset = 0;
    private const int FirstKeyOffset = 2;
    private const int ModifierCount = 8;

    /// <summary>Usages below this are error codes, not keys (HID usage tables §10: 0x01 is ErrorRollOver).</summary>
    private const byte FirstKeyUsage = 0x04;
    private const byte UsageErrorRollOver = 0x01;

    private const byte UsageCapsLock = 0x39;
    private const byte UsageScrollLock = 0x47;
    private const byte UsageNumLock = 0x53;

    // Output report LED bits (HID 1.11 appendix B.1).
    private const byte LedNumLock = 1 << 0;
    private const byte LedCapsLock = 1 << 1;
    private const byte LedScrollLock = 1 << 2;

    private readonly UsbEndpoint _endpoint;
    private readonly byte[] _previousReport = new byte[BootReportLength];
    private bool _enabled;
    private byte _leds;

    public bool IsInitialized { get; private set; }

    public UsbDevice Device { get; }

    /// <summary>bInterfaceNumber of the keyboard interface.</summary>
    public byte InterfaceNumber { get; }

    /// <summary>Always false: reports are pushed from the interrupt pipe, there is no buffer to query.</summary>
    public override bool KeyAvailable => false;

    /// <summary>
    /// PS/2 set 1 make code per HID keyboard usage (HID usage tables §10),
    /// 0 where there is none. PrintScreen and Pause have no single code (and
    /// PrintScreen's would read as keypad *), so they are dropped.
    /// </summary>
    private static ReadOnlySpan<byte> UsageScanCodes =>
    [
        0x00, 0x00, 0x00, 0x00, 0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26, // 0x00: -, A-L
        0x32, 0x31, 0x18, 0x19, 0x10, 0x13, 0x1F, 0x14, 0x16, 0x2F, 0x11, 0x2D, 0x15, 0x2C, 0x02, 0x03, // 0x10: M-Z, 1, 2
        0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x1C, 0x01, 0x0E, 0x0F, 0x39, 0x0C, 0x0D, 0x1A, // 0x20: 3-0, Enter, Esc, Backspace, Tab, Space, - = [
        0x1B, 0x2B, 0x2B, 0x27, 0x28, 0x29, 0x33, 0x34, 0x35, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, // 0x30: ] \ #, ; ' ` , . /, CapsLock, F1-F6
        0x41, 0x42, 0x43, 0x44, 0x57, 0x58, 0x00, 0x46, 0x00, 0x52, 0x47, 0x49, 0x53, 0x4F, 0x51, 0x4D, // 0x40: F7-F12, PrtSc, ScrLk, Pause, Ins Home PgUp Del End PgDn Right
        0x4B, 0x50, 0x48, 0x45, 0x35, 0x37, 0x4A, 0x4E, 0x1C, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47, // 0x50: Left Down Up, NumLock, keypad / * - + Enter 1-7
        0x48, 0x49, 0x52, 0x53, 0x56, 0x5D                                                              // 0x60: keypad 8 9 0 ., ISO \, Application
    ];

    /// <summary>Scan code per modifier bit: LCtrl, LShift, LAlt, LGUI, RCtrl, RShift, RAlt, RGUI.</summary>
    private static ReadOnlySpan<byte> ModifierScanCodes =>
        [0x1D, 0x2A, 0x38, 0x5B, 0x1D, 0x36, IKeyboardDevice.RightAltScanCode, 0x5C];

    public UsbKeyboard(UsbDevice device, byte interfaceNumber, UsbEndpoint endpoint)
    {
        Device = device;
        InterfaceNumber = interfaceNumber;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Switches the interface to boot protocol and starts polling its
    /// interrupt endpoint. <see cref="IsInitialized"/> reports the outcome.
    /// </summary>
    public override void Initialize()
    {
        if (Device.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, SetProtocolRequest, BootProtocol, InterfaceNumber) != UsbTransferStatus.Success)
        {
            Serial.WriteString("[UsbKeyboard] SET_PROTOCOL(boot) failed\n");
            return;
        }

        // Report on change only (duration 0). A keyboard may stall this
        // optional request and still work, so the result is not checked.
        Device.ControlOut(UsbRequestType.Class | UsbRequestType.Interface, SetIdleRequest, 0, InterfaceNumber);

        if (!Device.OpenInterruptPipe(_endpoint, OnReport))
        {
            Serial.WriteString("[UsbKeyboard] Could not open the report endpoint\n");
            return;
        }

        IsInitialized = true;
        Serial.WriteString("[UsbKeyboard] Ready\n");
    }

    /// <summary>Starts forwarding key changes to <see cref="KeyboardDevice.OnKeyPressed"/>.</summary>
    public override void Enable() => _enabled = true;

    /// <summary>Stops forwarding key changes; reports keep being tracked so no key is seen twice.</summary>
    public override void Disable() => _enabled = false;

    /// <summary>
    /// Sends the lock state this keyboard tracked from its own lock keys as
    /// an output report. Queued without waiting: this runs from the report
    /// handler, in interrupt context.
    /// </summary>
    public override void UpdateLeds()
    {
        ReadOnlySpan<byte> report = [_leds];
        Device.SubmitControlTransfer(
            new UsbSetupPacket(UsbRequestType.Class | UsbRequestType.Interface, SetReportRequest, OutputReport, InterfaceNumber, (ushort)report.Length),
            report);
    }

    /// <summary>Processes pending reports where interrupts are not delivered.</summary>
    public override void Poll() => Device.HostController.Poll();

    /// <summary>Report handler of the interrupt pipe; runs in interrupt context.</summary>
    private void OnReport(ReadOnlySpan<byte> report)
    {
        // A report full of ErrorRollOver means too many keys are down to
        // tell which: keep the last known state (HID usage tables §10).
        if (report.Length < BootReportLength || report[FirstKeyOffset] == UsageErrorRollOver)
        {
            return;
        }

        if (_enabled)
        {
            ForwardModifierChanges(_previousReport[ModifiersOffset], report[ModifiersOffset]);

            // Releases first, so a quick roll from one key to the next
            // arrives in the order it was typed.
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
                    TrackLockKey(usage);
                    Forward(usage, released: false);
                }
            }
        }

        report.Slice(0, BootReportLength).CopyTo(_previousReport);
    }

    private void ForwardModifierChanges(byte previous, byte current)
    {
        int changed = previous ^ current;
        for (int bit = 0; bit < ModifierCount; bit++)
        {
            int mask = 1 << bit;
            if ((changed & mask) != 0)
            {
                OnKeyPressed?.Invoke(ModifierScanCodes[bit], (current & mask) == 0);
            }
        }
    }

    private void Forward(byte usage, bool released)
    {
        byte scanCode = usage < UsageScanCodes.Length ? UsageScanCodes[usage] : (byte)0;
        if (scanCode != 0)
        {
            OnKeyPressed?.Invoke(scanCode, released);
        }
    }

    /// <summary>
    /// Mirrors the lock toggles the keyboard manager makes on the same key
    /// press, before the manager asks for <see cref="UpdateLeds"/>.
    /// </summary>
    private void TrackLockKey(byte usage)
    {
        switch (usage)
        {
            case UsageCapsLock:
                _leds ^= LedCapsLock;
                break;
            case UsageNumLock:
                _leds ^= LedNumLock;
                break;
            case UsageScrollLock:
                _leds ^= LedScrollLock;
                break;
        }
    }

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
