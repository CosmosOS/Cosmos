using System;
using System.Collections.Generic;

namespace Cosmos.System
{
    /// <summary>
    /// Represents the base class for keyboard layout scan-maps.
    /// </summary>
    public abstract class ScanMapBase
    {
        /// <summary>
        /// The available key mappings.
        /// </summary>
        protected List<KeyMapping> Keys;

        /// <summary>
        /// Initializes the key list.
        /// </summary>
        protected abstract void InitKeys();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScanMapBase"/> class.
        /// </summary>
        protected ScanMapBase()
        {
            InitKeys();
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
        public KeyEvent ConvertScanCode(byte scanKey, bool ctrl, bool shift, bool alt, bool numLock, bool capsLock, bool scrollLock)
        {
            var keyEvent = new KeyEvent();
            bool found = false;

            if (scanKey == 0)
            {
                return keyEvent;
            }

            byte scan = scanKey;

            if (alt)   keyEvent.Modifiers |= ConsoleModifiers.Alt;
            if (ctrl)  keyEvent.Modifiers |= ConsoleModifiers.Control;
            if (shift) keyEvent.Modifiers |= ConsoleModifiers.Shift;

            keyEvent.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;

            if ((scan & 0x80) != 0) {
                scan = (byte)(scan ^ 0x80);
            }

            for (int index = 0; index < Keys.Count; index++)
            {
                KeyMapping t = Keys[index];

                if (t == null)
                {
                    Global.Debugger.Send("An unmapped key input has been received; ignoring.");
                    continue;
                }
                else if (t.ScanCode == scan)
                {
                    found = true;
                    KeyMapping map = t;
                    char key;

                    if (ctrl) {
                        if (alt) {
                            key = shift ^ capsLock ? map.ControlAltShift : map.ControlAlt;
                        } else {
                            key = shift ^ capsLock ? map.ControlShift : map.Control;
                        }
                    } else if (shift) {
                        key = capsLock ? map.ShiftCapsLock
                            : numLock ? map.ShiftNumLock
                            : map.Shift;
                    } else if (capsLock) {
                        key = map.CapsLock;
                    } else {
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
        public bool ScanCodeMatchesKey(byte scanCode, ConsoleKeyEx key)
        {
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
}
