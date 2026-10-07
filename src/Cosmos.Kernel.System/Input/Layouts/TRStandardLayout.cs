// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/ScanMaps/TRStandardLayout.cs

namespace Cosmos.Kernel.System.Input.Layouts;

/// <summary>
/// Represents the standard Turkish (TR) keyboard layout.
/// </summary>
public sealed class TRStandardLayout : KeyboardLayout
{
    /// <inheritdoc />
    protected override void InitializeKeys()
    {
        #region Keys

        /*                       Scan Norm Shift Num Caps SCaps SNum AltGr AltGrShift Key */
        Keys.Add(new KeyMapping(0x00, Key.NoName));
        /* F1-F12 */
        Keys.Add(new KeyMapping(0x01, Key.Escape));
        Keys.Add(new KeyMapping(0x3B, Key.F1));
        Keys.Add(new KeyMapping(0x3C, Key.F2));
        Keys.Add(new KeyMapping(0x3D, Key.F3));
        Keys.Add(new KeyMapping(0x3E, Key.F4));
        Keys.Add(new KeyMapping(0x3F, Key.F5));
        Keys.Add(new KeyMapping(0x40, Key.F6));
        Keys.Add(new KeyMapping(0x41, Key.F7));
        Keys.Add(new KeyMapping(0x42, Key.F8));
        Keys.Add(new KeyMapping(0x43, Key.F9));
        Keys.Add(new KeyMapping(0x44, Key.F10));
        Keys.Add(new KeyMapping(0x57, Key.F11));
        Keys.Add(new KeyMapping(0x58, Key.F12));
        /* 1 -> 9 */
        Keys.Add(new KeyMapping(0x29, '"', 'é', '"', '"', 'é', '"', '<', Key.NoName));
        Keys.Add(new KeyMapping(0x02, '1', '!', '1', '1', '!', '1', '>', Key.D1));
        Keys.Add(new KeyMapping(0x03, '2', '\'', '2', '2', '\'', '2', '£', Key.D2));
        Keys.Add(new KeyMapping(0x04, '3', '^', '3', '3', '^', '3', '#', Key.D3));
        Keys.Add(new KeyMapping(0x05, '4', '+', '4', '4', '+', '4', '$', Key.D4));
        Keys.Add(new KeyMapping(0x06, '5', '%', '5', '5', '%', '5', '½', Key.D5));
        Keys.Add(new KeyMapping(0x07, '6', '&', '6', '6', '&', '6', Key.D6));
        Keys.Add(new KeyMapping(0x08, '7', '/', '7', '7', '/', '7', '{', Key.D7));
        Keys.Add(new KeyMapping(0x09, '8', '(', '8', '8', '(', '8', '[', Key.D8));
        Keys.Add(new KeyMapping(0x0A, '9', ')', '9', '9', ')', '9', ']', Key.D9));
        Keys.Add(new KeyMapping(0x0B, '0', '=', '0', '0', '=', '0', '}', Key.D0));
        /* -, =, Bksp, Tab */
        Keys.Add(new KeyMapping(0x0C, '*', '?', '*', '*', '?', '*', '\\', Key.NoName));
        Keys.Add(new KeyMapping(0x0D, '-', '_', '-', '-', '_', '-', '|', Key.Minus));
        Keys.Add(new KeyMapping(0x0E, Key.Backspace));
        Keys.Add(new KeyMapping(0x0F, '\t', Key.Tab));
        /*      QWERTYUIOP[] */
        Keys.Add(new KeyMapping(0x10, 'q', 'Q', 'q', 'Q', 'q', 'Q', '@', Key.Q));
        Keys.Add(new KeyMapping(0x11, 'w', 'W', 'w', 'W', 'w', 'W', Key.W));
        Keys.Add(new KeyMapping(0x12, 'e', 'E', 'e', 'E', 'e', 'E', '€', Key.E));
        Keys.Add(new KeyMapping(0x13, 'r', 'R', 'r', 'R', 'r', 'R', Key.R));
        Keys.Add(new KeyMapping(0x14, 't', 'T', 't', 'T', 't', 'T', '₺', Key.T));
        Keys.Add(new KeyMapping(0x15, 'y', 'Y', 'y', 'Y', 'y', 'Y', Key.Y));
        Keys.Add(new KeyMapping(0x16, 'u', 'U', 'u', 'U', 'u', 'U', Key.U));
        Keys.Add(new KeyMapping(0x17, 'ı', 'I', 'ı', 'I', 'ı', 'I', 'i', 'İ', Key.I));
        Keys.Add(new KeyMapping(0x18, 'o', 'O', 'o', 'O', 'o', 'O', Key.O));
        Keys.Add(new KeyMapping(0x19, 'p', 'P', 'p', 'P', 'p', 'P', Key.P));
        Keys.Add(new KeyMapping(0x1A, 'ğ', 'Ğ', 'ğ', 'Ğ', 'ğ', 'Ğ', '¨', Key.LBracket));
        Keys.Add(new KeyMapping(0x1B, 'ü', 'Ü', 'ü', 'Ü', 'ü', 'Ü', '~', Key.RBracket));
        /* ASDFGHJKL;'` */
        Keys.Add(new KeyMapping(0x1E, 'a', 'A', 'a', 'A', 'a', 'A', 'æ', 'Æ', Key.A));
        Keys.Add(new KeyMapping(0x1F, 's', 'S', 's', 'S', 's', 'S', 'ß', Key.S));
        Keys.Add(new KeyMapping(0x20, 'd', 'D', 'd', 'D', 'd', 'D', Key.D));
        Keys.Add(new KeyMapping(0x21, 'f', 'F', 'f', 'F', 'f', 'F', Key.F));
        Keys.Add(new KeyMapping(0x22, 'g', 'G', 'g', 'G', 'g', 'G', Key.G));
        Keys.Add(new KeyMapping(0x23, 'h', 'H', 'h', 'H', 'h', 'H', Key.H));
        Keys.Add(new KeyMapping(0x24, 'j', 'J', 'j', 'J', 'j', 'J', Key.J));
        Keys.Add(new KeyMapping(0x25, 'k', 'K', 'k', 'K', 'k', 'K', Key.K));
        Keys.Add(new KeyMapping(0x26, 'l', 'L', 'l', 'L', 'l', 'L', Key.L));
        Keys.Add(new KeyMapping(0x27, 'ş', 'Ş', 'ş', 'Ş', 'ş', 'Ş', '´', Key.NoName));
        Keys.Add(new KeyMapping(0x28, 'i', 'İ', 'i', 'İ', 'i', 'İ', Key.NoName));
        Keys.Add(new KeyMapping(0x2B, ',', ';', ',', ',', ';', ';', '`', Key.Comma));
        /* Left Shift*/
        Keys.Add(new KeyMapping(0x2A, Key.LShift));
        Keys.Add(new KeyMapping(0x56, '<', '>', '<', '<', '>', '>', '|', Key.NoName));
        /* \ZXCVBNM,./ */
        Keys.Add(new KeyMapping(0x2C, 'z', 'Z', 'z', 'Z', 'z', 'Z', Key.Z));
        Keys.Add(new KeyMapping(0x2D, 'x', 'X', 'x', 'X', 'x', 'X', Key.X));
        Keys.Add(new KeyMapping(0x2E, 'c', 'C', 'c', 'C', 'c', 'C', Key.C));
        Keys.Add(new KeyMapping(0x2F, 'v', 'V', 'v', 'V', 'v', 'V', Key.V));
        Keys.Add(new KeyMapping(0x30, 'b', 'B', 'b', 'B', 'b', 'B', Key.B));
        Keys.Add(new KeyMapping(0x31, 'n', 'N', 'n', 'N', 'n', 'N', Key.N));
        Keys.Add(new KeyMapping(0x32, 'm', 'M', 'm', 'M', 'm', 'M', Key.M));
        Keys.Add(new KeyMapping(0x33, 'ö', 'Ö', 'ö', 'Ö', 'ö', 'Ö', Key.NoName));
        Keys.Add(new KeyMapping(0x34, 'ç', 'Ç', 'ç', 'Ç', 'ç', 'Ç', Key.NoName));
        Keys.Add(new KeyMapping(0x35, '.', ':', '.', '.', ':', '.', Key.NoName)); // also numpad divide
        /* Right Shift */
        Keys.Add(new KeyMapping(0x1C, Key.Enter));
        Keys.Add(new KeyMapping(0x36, Key.RShift));
        Keys.Add(new KeyMapping(0x1D, Key.LCtrl));
        /* Print Screen */
        Keys.Add(new KeyMapping(0x37, '*', '*', '*', '*', '*', '*', Key.NumMultiply)); // also numpad multiply
        /* Alt  */
        Keys.Add(new KeyMapping(0x38, Key.LAlt));
        /* Right Alt: the third-level modifier on this layout */
        Keys.Add(new KeyMapping(RightAltScanCode, Key.AltGr));
        /* Space */
        Keys.Add(new KeyMapping(0x39, ' ', Key.Spacebar));
        /* Caps */
        Keys.Add(new KeyMapping(0x3A, Key.CapsLock));
        /* Num Lock, Scrl Lock */
        Keys.Add(new KeyMapping(0x45, Key.NumLock));
        Keys.Add(new KeyMapping(0x46, Key.ScrollLock));
        /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
        Keys.Add(new KeyMapping(0x47, '\0', '\0', '7', '\0', '\0', '\0', Key.Home, Key.Num7));
        Keys.Add(new KeyMapping(0x48, '\0', '\0', '8', '\0', '\0', '\0', Key.UpArrow, Key.Num8));
        Keys.Add(new KeyMapping(0x49, '\0', '\0', '9', '\0', '\0', '\0', Key.PageUp, Key.Num9));
        Keys.Add(new KeyMapping(0x4A, '-', '-', '-', '-', '-', '-', Key.NumMinus));
        Keys.Add(new KeyMapping(0x4B, '\0', '\0', '4', '\0', '\0', '\0', Key.LeftArrow, Key.Num4));
        Keys.Add(new KeyMapping(0x4C, '\0', '\0', '5', '\0', '\0', '\0', Key.Num5));
        Keys.Add(new KeyMapping(0x4D, '\0', '\0', '6', '\0', '\0', '\0', Key.RightArrow, Key.Num6));
        Keys.Add(new KeyMapping(0x4E, '+', '+', '+', '+', '+', '+', Key.NumPlus));
        Keys.Add(new KeyMapping(0x4F, '\0', '\0', '1', '\0', '\0', '\0', Key.End, Key.Num1));
        Keys.Add(new KeyMapping(0x50, '\0', '\0', '2', '\0', '\0', '\0', Key.DownArrow, Key.Num2));
        Keys.Add(new KeyMapping(0x51, '\0', '\0', '3', '\0', '\0', '\0', Key.PageDown, Key.Num3));
        Keys.Add(new KeyMapping(0x52, '\0', '\0', '0', '\0', '\0', '\0', Key.Insert, Key.Num0));
        Keys.Add(new KeyMapping(0x53, '\0', '\0', '.', '\0', '\0', '\0', Key.Delete, Key.NumPeriod));

        Keys.Add(new KeyMapping(0x5b, Key.LWin));
        Keys.Add(new KeyMapping(0x5c, Key.RWin));

        #endregion
    }
}
