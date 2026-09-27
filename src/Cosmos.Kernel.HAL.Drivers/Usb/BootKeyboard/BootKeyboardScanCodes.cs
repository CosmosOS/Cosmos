// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Usb.BootKeyboard;

/// <summary>
/// The HID keyboard usages a boot report carries (HID usage tables §10), as
/// the PS/2 set 1 make codes the kit's keyboards report: the 0xE0 prefix of
/// an extended key dropped, and the right Alt key as
/// <see cref="KeyboardReporter.RightAltScanCode"/>, which is what tells it
/// from the left one.
/// </summary>
/// <remarks>
/// Print Screen and Pause have no single set 1 make code, and Print
/// Screen's last byte would read as the keypad's asterisk, so both map to
/// 0, which the driver reports as nothing.
/// </remarks>
internal static class BootKeyboardScanCodes
{
    /// <summary>Make code per usage, 0 where there is none; usages past the end have none either.</summary>
    private static ReadOnlySpan<byte> Usages =>
    [
        0x00, 0x00, 0x00, 0x00, 0x1E, 0x30, 0x2E, 0x20, 0x12, 0x21, 0x22, 0x23, 0x17, 0x24, 0x25, 0x26, // 0x00: -, A-L
        0x32, 0x31, 0x18, 0x19, 0x10, 0x13, 0x1F, 0x14, 0x16, 0x2F, 0x11, 0x2D, 0x15, 0x2C, 0x02, 0x03, // 0x10: M-Z, 1, 2
        0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x1C, 0x01, 0x0E, 0x0F, 0x39, 0x0C, 0x0D, 0x1A, // 0x20: 3-0, Enter, Esc, Backspace, Tab, Space, - = [
        0x1B, 0x2B, 0x2B, 0x27, 0x28, 0x29, 0x33, 0x34, 0x35, 0x3A, 0x3B, 0x3C, 0x3D, 0x3E, 0x3F, 0x40, // 0x30: ] \ #, ; ' ` , . /, CapsLock, F1-F6
        0x41, 0x42, 0x43, 0x44, 0x57, 0x58, 0x00, 0x46, 0x00, 0x52, 0x47, 0x49, 0x53, 0x4F, 0x51, 0x4D, // 0x40: F7-F12, PrtSc, ScrLk, Pause, Ins Home PgUp Del End PgDn Right
        0x4B, 0x50, 0x48, 0x45, 0x35, 0x37, 0x4A, 0x4E, 0x1C, 0x4F, 0x50, 0x51, 0x4B, 0x4C, 0x4D, 0x47, // 0x50: Left Down Up, NumLock, keypad / * - + Enter 1-7
        0x48, 0x49, 0x52, 0x53, 0x56, 0x5D                                                              // 0x60: keypad 8 9 0 ., ISO \, Application
    ];

    /// <summary>Make code per modifier bit of the report's first byte: LCtrl, LShift, LAlt, LGUI, RCtrl, RShift, RAlt, RGUI.</summary>
    internal static ReadOnlySpan<byte> Modifiers =>
        [0x1D, 0x2A, 0x38, 0x5B, 0x1D, 0x36, KeyboardReporter.RightAltScanCode, 0x5C];

    /// <summary>
    /// The make code of <paramref name="usage"/>, or 0 when the key has none
    /// here. Any context: two loads out of a data section, no allocation.
    /// </summary>
    /// <param name="usage">A usage from one of the report's key slots.</param>
    /// <returns>The make code, or 0 for a key this map does not carry.</returns>
    internal static byte ToScanCode(byte usage) => usage < Usages.Length ? Usages[usage] : (byte)0;
}
