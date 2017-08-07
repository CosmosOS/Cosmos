using System;
using System.Collections.Generic;

using Cosmos.Debug.Kernel;
using Cosmos.HAL;

namespace Cosmos.System
{
    public abstract class ScanMapBase
    {
        protected List<KeyMapping> _keys;


        protected abstract void InitKeys();

        protected ScanMapBase()
        {
            InitKeys();
        }

        public KeyEvent ConvertScanCode(byte scan2, bool ctrl, bool shift, bool alt, bool num, bool caps, bool scroll)
        {
            KeyEvent keyev = new KeyEvent();
            bool found = false;

            if (scan2 == 0)
            {
                found = true;
                return keyev;
            }

            byte scan = scan2;

            if (alt) keyev.Modifiers |= ConsoleModifiers.Alt;
            if (ctrl) keyev.Modifiers |= ConsoleModifiers.Control;
            if (shift) keyev.Modifiers |= ConsoleModifiers.Shift;

            keyev.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;

            if ((scan & 0x80) != 0)
                scan = (byte)(scan ^ 0x80);

            Global.mDebugger.Send("Number of keys: ");
            Global.mDebugger.SendNumber((uint) _keys.Count);

            for (int index = 0; index < _keys.Count; index++)
            {
                KeyMapping t = _keys[index];

                if (t == null)
                {
                    Global.mDebugger.Send("Key received but item is NULL");
                    continue;
                }
                else if (t.Scancode == scan)
                {
                    found = true;
                    KeyMapping map = t;
                    char key = '\0';

                    if (ctrl)
                        if (alt)
                            key = shift ^ caps ? map.ControlAltShift : map.ControlAlt;
                        else
                            key = shift ^ caps ? map.ControlShift : map.Control;
                    else if (shift)
                        key = caps ? map.ShiftCaps
                            : num ? map.ShiftNum
                            : map.Shift;
                    else if (caps)
                        key = map.Caps;
                    else if (num)
                        key = map.Num;
                    else
                        key = map.Value;

                    keyev.KeyChar = key;
                    keyev.Key = num ? t.NumLockKey : t.Key;

                    break;
                }
            }

            return found ? keyev : null;
        }

        public bool ScanCodeMatchesKey(byte ScanCode, ConsoleKeyEx Key)
        {
            for (int i = 0; i < _keys.Count; i++)
            {
                if (_keys[i].Scancode == ScanCode && _keys[i].Key == Key)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
