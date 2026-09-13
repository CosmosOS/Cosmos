// This code is licensed under the BSD 3-Clause license (see LICENSE for details)
// Ported from Cosmos.System2/Keyboard/ScanMapBase.cs

namespace Cosmos.Kernel.System.Keyboard;

/// <summary>
/// Represents the base class for keyboard layout scan-maps.
/// </summary>
public abstract class ScanMapBase
{
    /// <summary>Mappings a layout is expected to declare, reserved up front.</summary>
    private const int InitialKeyCapacity = 105;

    private bool _keysInitialized;

    /// <summary>
    /// The available key mappings. The list is created here and stays the
    /// same instance for the life of the layout; <see cref="InitializeKeys"/>
    /// adds to it.
    /// </summary>
    protected List<KeyMapping> Keys { get; } = new(InitialKeyCapacity);

    /// <summary>
    /// Adds this layout's mappings to <see cref="Keys"/>. Called once, the
    /// first time a scan code reaches the layout, so a derived layout's own
    /// field initializers and constructor have already run by then.
    /// </summary>
    protected abstract void InitializeKeys();

    /// <summary>
    /// Runs <see cref="InitializeKeys"/> the first time the mappings are
    /// needed. The base constructor deliberately does not call it: a virtual
    /// call from a constructor reaches a derived layout before its own state
    /// exists.
    /// </summary>
    private void EnsureKeysInitialized()
    {
        if (_keysInitialized)
        {
            return;
        }

        _keysInitialized = true;
        InitializeKeys();
    }

    /// <summary>
    /// Converts the given scan code to a <see cref="KeyEvent"/> instance.
    /// </summary>
    /// <param name="scanKey">The scanned (pressed) key.</param>
    /// <param name="ctrl">Whether the Control (Ctrl) key is pressed.</param>
    /// <param name="shift">Whether the Shift key is pressed.</param>
    /// <param name="alt">Whether the Alt key is pressed.</param>
    /// <param name="numLock">Whether num-lock is active.</param>
    /// <param name="capsLock">Whether caps-lock is active.</param>
    /// <param name="scrollLock">Whether scroll-lock is active.</param>
    /// <returns>The translated <see cref="KeyEvent"/>.</returns>
    internal KeyEvent? ConvertScanCode(byte scanKey, bool ctrl, bool shift, bool alt, bool numLock, bool capsLock, bool scrollLock)
    {
        EnsureKeysInitialized();

        var keyEvent = new KeyEvent();
        bool found = false;

        if (scanKey == 0)
        {
            return keyEvent;
        }

        byte scan = scanKey;

        if (alt)
        {
            keyEvent.Modifiers |= ConsoleModifiers.Alt;
        }

        if (ctrl)
        {
            keyEvent.Modifiers |= ConsoleModifiers.Control;
        }

        if (shift)
        {
            keyEvent.Modifiers |= ConsoleModifiers.Shift;
        }

        keyEvent.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;

        if ((scan & 0x80) != 0)
        {
            scan = (byte)(scan ^ 0x80);
        }

        for (int index = 0; index < Keys.Count; index++)
        {
            KeyMapping t = Keys[index];

            if (t == null)
            {
                continue;
            }
            else if (t.ScanCode == scan)
            {
                found = true;
                KeyMapping map = t;
                char key;

                if (ctrl)
                {
                    if (alt)
                    {
                        key = shift ^ capsLock ? map.ControlAltShift : map.ControlAlt;
                    }
                    else
                    {
                        key = shift ^ capsLock ? map.ControlShift : map.Control;
                    }
                }
                else if (shift)
                {
                    key = capsLock ? map.ShiftCapsLock
                        : numLock ? map.ShiftNumLock
                        : map.Shift;
                }
                else if (capsLock)
                {
                    key = map.CapsLock;
                }
                else
                {
                    key = numLock ? map.NumLock : map.Value;
                }

                keyEvent.KeyChar = key;
                keyEvent.Key = numLock ? t.NumLockKey : t.Key;
                break;
            }
        }

        return found ? keyEvent : null;
    }

    /// <summary>
    /// Checks if the given scan code matches the specified key.
    /// </summary>
    /// <param name="scanCode">The physical keyboard scan-code.</param>
    /// <param name="key">The virtual mapping key.</param>
    internal bool ScanCodeMatchesKey(byte scanCode, ConsoleKeyEx key)
    {
        EnsureKeysInitialized();

        for (int i = 0; i < Keys.Count; i++)
        {
            if (Keys[i].ScanCode == scanCode && Keys[i].Key == key)
            {
                return true;
            }
        }

        return false;
    }
}
