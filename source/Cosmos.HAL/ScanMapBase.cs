using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public abstract class ScanMapBase
    {
        protected List<KeyMapping> _keys;

        protected ScanMapBase()
        {
            InitKeys();
        }

        protected abstract void InitKeys();

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
            if (alt) keyev.Modifiers |= ConsoleModifiers.Alt;
            if (ctrl) keyev.Modifiers |= ConsoleModifiers.Control;
            if (shift) keyev.Modifiers |= ConsoleModifiers.Shift;

            keyev.Type = (scan & 0x80) != 0 ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make;
            if ((scan & 0x80) != 0) scan = (byte)(scan ^ 0x80);
            Debugger.DoSend("Number of keys: ");
            Debugger.DoSendNumber((uint) _keys.Count);
            for (var index = 0; index < _keys.Count; index++)
            {
                var t = _keys[index];
                if (t == null)
                {
                    Debugger.DoSend("Key received but item is NULL");
                    continue;
                }
                if (t.Scancode == scan)
                {
                    found = true;
                    var map = t;
                    var key = '\0';

                    if (shift)
                    {
                        if (caps) key = map.ShiftCaps;
                        else if (num) key = map.ShiftNum;
                        else key = map.Shift;
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
    }
}
