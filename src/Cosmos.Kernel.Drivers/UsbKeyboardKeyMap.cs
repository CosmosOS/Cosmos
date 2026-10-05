// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.Drivers;

/// <summary>
/// Maps the HID keyboard usages a boot protocol keyboard reports (HID usage
/// tables section 10) to the PS/2 scan code set 1 values the ring's keyboard
/// manager decodes, with the E0 prefix dropped as the PS/2 driver reports
/// them. A usage the table does not know maps to 0. Any context;
/// allocation-free.
/// </summary>
internal static class UsbKeyboardKeyMap
{
    /// <summary>
    /// The scan code every keyboard device reports for the right Alt key,
    /// the value the ring's <c>ScanMapBase.RightAltScanCode</c> keeps and
    /// every keyboard driver reports, so the manager's AltGr handling applies
    /// to this keyboard as to the others.
    /// </summary>
    internal const byte RightAltScanCode = 0x60;

    /// <summary>
    /// PS/2 set 1 make code per HID keyboard usage, 0 where there is none.
    /// PrintScreen and Pause have no single code (and PrintScreen would
    /// read as the keypad star), so they are dropped.
    /// </summary>
    internal static ReadOnlySpan<byte> UsageScanCodes =>
    [
        0x00, 0x00, 0x00, 0x00, 0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26, // 0x00: -, A-L
        0x32, 0x31, 0x18, 0x19, 0x10, 0x13, 0x1F, 0x14, 0x16, 0x2F, 0x11, 0x2D, 0x15, 0x2C, 0x02, 0x03, // 0x10: M-Z, 1, 2
        0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x1C, 0x01, 0x0E, 0x0F, 0x39, 0x0C, 0x0D, 0x1A, // 0x20: 3-0, Enter, Esc, Backspace, Tab, Space, - = [
        0x1B, 0x2B, 0x2B, 0x27, 0x28, 0x29, 0x33, 0x34, 0x35, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, // 0x30: ] \ #, ; ' ` , . /, CapsLock, F1-F6
        0x41, 0x42, 0x43, 0x44, 0x57, 0x58, 0x00, 0x46, 0x00, 0x52, 0x47, 0x49, 0x53, 0x4F, 0x51, 0x4D, // 0x40: F7-F12, PrtSc, ScrLk, Pause, Ins Home PgUp Del End PgDn Right
        0x4B, 0x50, 0x48, 0x45, 0x35, 0x37, 0x4A, 0x4E, 0x1C, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47, // 0x50: Left Down Up, NumLock, keypad / * - + Enter 1-7
        0x48, 0x49, 0x52, 0x53, 0x56, 0x5D                                                              // 0x60: keypad 8 9 0 ., ISO \, Application
    ];

    /// <summary>Scan code per modifier bit of the report's first byte: LCtrl, LShift, LAlt, LGUI, RCtrl, RShift, RAlt, RGUI.</summary>
    internal static ReadOnlySpan<byte> ModifierScanCodes =>
        [0x1D, 0x2A, 0x38, 0x5B, 0x1D, 0x36, RightAltScanCode, 0x5C];
}
