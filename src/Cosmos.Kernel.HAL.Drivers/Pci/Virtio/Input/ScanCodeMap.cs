// This code is licensed under the BSD 3-Clause license (see LICENSE for details)

namespace Cosmos.Kernel.HAL.Drivers.BuiltIn.Pci.Virtio.Input;

/// <summary>
/// The Linux key codes a virtio-input keyboard reports
/// (linux/input-event-codes.h), as the PS/2 set 1 make codes the kit's
/// keyboards report: the 0xE0 prefix of an extended key dropped, and the
/// right Alt key as <see cref="KeyboardReporter.RightAltScanCode"/>, which
/// is what tells it from the left one.
/// </summary>
/// <remarks>
/// The two sets agree on nothing but the ordering of the main block, so
/// every key is listed. A code with no set 1 equivalent, and every key
/// beyond the ones here, maps to 0, which the driver reports as nothing:
/// the keypad, the Windows keys, Print Screen and Pause are still missing,
/// as they were before this driver moved onto the kit.
/// </remarks>
internal static class ScanCodeMap
{
    /// <summary>
    /// The set 1 make code of <paramref name="keyCode"/>, or 0 when the key
    /// has none here. Any context: a jump table, no allocation.
    /// </summary>
    /// <param name="keyCode">A KEY_* code from an EV_KEY event.</param>
    /// <returns>The make code, or 0 for a key this map does not carry.</returns>
    internal static byte ToScanCode(ushort keyCode) => keyCode switch
    {
        // Function row
        1 => 0x01,   // KEY_ESC
        59 => 0x3B,  // KEY_F1
        60 => 0x3C,  // KEY_F2
        61 => 0x3D,  // KEY_F3
        62 => 0x3E,  // KEY_F4
        63 => 0x3F,  // KEY_F5
        64 => 0x40,  // KEY_F6
        65 => 0x41,  // KEY_F7
        66 => 0x42,  // KEY_F8
        67 => 0x43,  // KEY_F9
        68 => 0x44,  // KEY_F10

        // Number row
        2 => 0x02,   // KEY_1
        3 => 0x03,   // KEY_2
        4 => 0x04,   // KEY_3
        5 => 0x05,   // KEY_4
        6 => 0x06,   // KEY_5
        7 => 0x07,   // KEY_6
        8 => 0x08,   // KEY_7
        9 => 0x09,   // KEY_8
        10 => 0x0A,  // KEY_9
        11 => 0x0B,  // KEY_0
        12 => 0x0C,  // KEY_MINUS
        13 => 0x0D,  // KEY_EQUAL
        14 => 0x0E,  // KEY_BACKSPACE

        // Top letter row
        15 => 0x0F,  // KEY_TAB
        16 => 0x10,  // KEY_Q
        17 => 0x11,  // KEY_W
        18 => 0x12,  // KEY_E
        19 => 0x13,  // KEY_R
        20 => 0x14,  // KEY_T
        21 => 0x15,  // KEY_Y
        22 => 0x16,  // KEY_U
        23 => 0x17,  // KEY_I
        24 => 0x18,  // KEY_O
        25 => 0x19,  // KEY_P
        26 => 0x1A,  // KEY_LEFTBRACE
        27 => 0x1B,  // KEY_RIGHTBRACE
        28 => 0x1C,  // KEY_ENTER

        // Middle letter row
        29 => 0x1D,  // KEY_LEFTCTRL
        30 => 0x1E,  // KEY_A
        31 => 0x1F,  // KEY_S
        32 => 0x20,  // KEY_D
        33 => 0x21,  // KEY_F
        34 => 0x22,  // KEY_G
        35 => 0x23,  // KEY_H
        36 => 0x24,  // KEY_J
        37 => 0x25,  // KEY_K
        38 => 0x26,  // KEY_L
        39 => 0x27,  // KEY_SEMICOLON
        40 => 0x28,  // KEY_APOSTROPHE
        41 => 0x29,  // KEY_GRAVE

        // Bottom letter row
        42 => 0x2A,  // KEY_LEFTSHIFT
        43 => 0x2B,  // KEY_BACKSLASH
        44 => 0x2C,  // KEY_Z
        45 => 0x2D,  // KEY_X
        46 => 0x2E,  // KEY_C
        47 => 0x2F,  // KEY_V
        48 => 0x30,  // KEY_B
        49 => 0x31,  // KEY_N
        50 => 0x32,  // KEY_M
        51 => 0x33,  // KEY_COMMA
        52 => 0x34,  // KEY_DOT
        53 => 0x35,  // KEY_SLASH
        54 => 0x36,  // KEY_RIGHTSHIFT

        // Bottom row
        56 => 0x38,  // KEY_LEFTALT
        57 => 0x39,  // KEY_SPACE
        58 => 0x3A,  // KEY_CAPSLOCK
        100 => KeyboardReporter.RightAltScanCode, // KEY_RIGHTALT

        // Arrow keys, extended on the wire and reported without the prefix
        103 => 0x48, // KEY_UP
        105 => 0x4B, // KEY_LEFT
        106 => 0x4D, // KEY_RIGHT
        108 => 0x50, // KEY_DOWN

        _ => 0x00
    };
}
