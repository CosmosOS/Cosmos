using System;
using System.Collections.Generic;

namespace Cosmos.System;

/// <summary>
///     ScanMapBase abstract class.
/// </summary>
public abstract class ScanMapBase
{
    /// <summary>
    ///     Keys list.
    /// </summary>
    protected List<KeyMapping> _keys;

    /// <summary>
    ///     Create new instance of the <see cref="ScanMapBase" /> class.
    /// </summary>
    protected ScanMapBase()
    {
        InitKeys();
    }

    /// <summary>
    ///     Init keys list.
    /// </summary>
    protected abstract void InitKeys();

    /// <summary>
    ///     Convert scan code to KeyEvent.
    /// </summary>
    /// <param name="scan2">Scaned key.</param>
    /// <param name="ctrl">Ctrl pressed.</param>
    /// <param name="shift">Shift pressed.</param>
    /// <param name="alt">Alt pressed.</param>
    /// <param name="num">Num pressed.</param>
    /// <param name="caps">Caps pressed.</param>
    /// <param name="scroll">Scroll pressed.</param>
    /// <returns>KeyEvent value.</returns>
    public KeyEvent ConvertScanCode(byte scan2, bool ctrl, bool shift, bool alt, bool num, bool caps, bool scroll)
    {
        var keyev = new KeyEvent();
        var found = false;

        if (scan2 == 0)
        {
            found = true;
            return keyev;
        }

        var scan = scan2;

        if (alt)
        {
            keyev.Modifiers |= ConsoleModifiers.Alt;
        }

        if (ctrl)
        {
            keyev.Modifiers |= ConsoleModifiers.Control;
        }

        if (shift)
        {
            keyev.Modifiers |= ConsoleModifiers.Shift;
        }

        keyev.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;

        if ((scan & 0x80) != 0)
        {
            scan = (byte)(scan ^ 0x80);
        }

        Global.mDebugger.Send("Number of keys: ");
        Global.mDebugger.SendNumber((uint)_keys.Count);

        for (var index = 0; index < _keys.Count; index++)
        {
            var t = _keys[index];

            if (t == null)
            {
                Global.mDebugger.Send("Key received but item is NULL");
                continue;
            }

            if (t.Scancode == scan)
            {
                found = true;
                var map = t;
                var key = '\0';

                if (ctrl)
                {
                    if (alt)
                    {
                        key = shift ^ caps ? map.ControlAltShift : map.ControlAlt;
                    }
                    else
                    {
                        key = shift ^ caps ? map.ControlShift : map.Control;
                    }
                }
                else if (shift)
                {
                    key = caps ? map.ShiftCaps
                        : num ? map.ShiftNum
                        : map.Shift;
                }
                else if (caps)
                {
                    key = map.Caps;
                }
                else if (num)
                {
                    key = map.Num;
                }
                else
                {
                    key = map.Value;
                }

                keyev.KeyChar = key;
                keyev.Key = num ? t.NumLockKey : t.Key;

                break;
            }
        }

        return found ? keyev : null;
    }

    /// <summary>
    ///     Check if scan code matches key.
    /// </summary>
    /// <param name="ScanCode">Scan code.</param>
    /// <param name="Key">Key.</param>
    /// <returns>bool value.</returns>
    public bool ScanCodeMatchesKey(byte ScanCode, ConsoleKeyEx Key)
    {
        for (var i = 0; i < _keys.Count; i++)
        {
            if (_keys[i].Scancode == ScanCode && _keys[i].Key == Key)
            {
                return true;
            }
        }

        return false;
    }
}
