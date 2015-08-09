using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.HAL.ScanMaps
{
    public class US_Standard : ScanMapBase
    {
        private List<KeyMapping> _keys;
        
        public US_Standard()
        {
            _keys = new List<KeyMapping>();

            #region Keys
            /*     Scan  Norm Shift Ctrl Alt     Num  Caps ShCaps ShNum ConsoleKeyEx */
            _keys.Add(new KeyMapping(0x00, ConsoleKeyEx.NoName));
            _keys.Add(new KeyMapping(0x01, ConsoleKeyEx.Escape));
            /* 1 -> 9 */
            _keys.Add(new KeyMapping(0x02, '1', '!', '\0', '\0', '1', '1', '!', '1', ConsoleKeyEx.D1));
            _keys.Add(new KeyMapping(0x03, '2', '@', '\0', '\0', '2', '2', '@', '2', ConsoleKeyEx.D2));
            _keys.Add(new KeyMapping(0x04, '3', '#', '\0', '\0', '3', '3', '#', '3', ConsoleKeyEx.D3));
            _keys.Add(new KeyMapping(0x05, '4', '$', '\0', '\0', '4', '4', '$', '4', ConsoleKeyEx.D4));
            _keys.Add(new KeyMapping(0x06, '5', '%', '\0', '\0', '5', '5', '%', '5', ConsoleKeyEx.D5));
            _keys.Add(new KeyMapping(0x07, '6', '^', '\0', '\0', '6', '6', '^', '6', ConsoleKeyEx.D6));
            _keys.Add(new KeyMapping(0x08, '7', '&', '\0', '\0', '7', '7', '&', '7', ConsoleKeyEx.D7));
            _keys.Add(new KeyMapping(0x09, '8', '*', '\0', '\0', '8', '8', '*', '8', ConsoleKeyEx.D8));
            _keys.Add(new KeyMapping(0x0A, '9', '(', '\0', '\0', '9', '9', '(', '9', ConsoleKeyEx.D9));
            _keys.Add(new KeyMapping(0x0B, '0', ')', '\0', '\0', '0', '0', ')', '0', ConsoleKeyEx.D0));
            /* -, =, Bksp, Tab */
            _keys.Add(new KeyMapping(0x0C, '-', '_', '\0', '\0', '-', '-', '_', '-', ConsoleKeyEx.Minus));
            _keys.Add(new KeyMapping(0x0D, '=', '+', '\0', '\0', '=', '=', '+', '=', ConsoleKeyEx.Equal));
            _keys.Add(new KeyMapping(0x0E, ConsoleKeyEx.Backspace));
            _keys.Add(new KeyMapping(0x0F, ConsoleKeyEx.Tab));
            /*      QWERTYUIOP[] */
            _keys.Add(new KeyMapping(0x10, 'q', 'Q', '\0', '\0', 'q', 'Q', 'q', 'Q', ConsoleKeyEx.Q));
            _keys.Add(new KeyMapping(0x11, 'w', 'W', '\0', '\0', 'w', 'W', 'w', 'W', ConsoleKeyEx.W));
            _keys.Add(new KeyMapping(0x12, 'e', 'E', '\0', '\0', 'e', 'E', 'e', 'E', ConsoleKeyEx.E));
            _keys.Add(new KeyMapping(0x13, 'r', 'R', '\0', '\0', 'r', 'R', 'r', 'R', ConsoleKeyEx.R));
            _keys.Add(new KeyMapping(0x14, 't', 'T', '\0', '\0', 't', 'T', 't', 'T', ConsoleKeyEx.T));
            _keys.Add(new KeyMapping(0x15, 'y', 'Y', '\0', '\0', 'y', 'Y', 'y', 'Y', ConsoleKeyEx.Y));
            _keys.Add(new KeyMapping(0x16, 'u', 'U', '\0', '\0', 'u', 'U', 'u', 'U', ConsoleKeyEx.U));
            _keys.Add(new KeyMapping(0x17, 'i', 'I', '\0', '\0', 'i', 'I', 'i', 'I', ConsoleKeyEx.I));
            _keys.Add(new KeyMapping(0x18, 'o', 'O', '\0', '\0', 'o', 'O', 'o', 'O', ConsoleKeyEx.O));
            _keys.Add(new KeyMapping(0x19, 'p', 'P', '\0', '\0', 'p', 'P', 'p', 'P', ConsoleKeyEx.P));
            _keys.Add(new KeyMapping(0x1A, '[', '{', '\0', '\0', '[', '{', '[', '{', ConsoleKeyEx.LBracket));
            _keys.Add(new KeyMapping(0x1B, ']', '}', '\0', '\0', ']', '}', ']', '}', ConsoleKeyEx.RBracket));
            /* ENTER, CTRL */
            _keys.Add(new KeyMapping(0x1C, ConsoleKeyEx.Enter));
            _keys.Add(new KeyMapping(0x1D, ConsoleKeyEx.Ctrl));
            /* ASDFGHJKL;'` */
            _keys.Add(new KeyMapping(0x1E, 'a', 'A', '\0', '\0', 'a', 'A', 'a', 'A', ConsoleKeyEx.A));
            _keys.Add(new KeyMapping(0x1F, 's', 'S', '\0', '\0', 's', 'S', 's', 'S', ConsoleKeyEx.S));
            _keys.Add(new KeyMapping(0x20, 'd', 'D', '\0', '\0', 'd', 'D', 'd', 'D', ConsoleKeyEx.D));
            _keys.Add(new KeyMapping(0x21, 'f', 'F', '\0', '\0', 'f', 'F', 'f', 'F', ConsoleKeyEx.F));
            _keys.Add(new KeyMapping(0x22, 'g', 'G', '\0', '\0', 'g', 'G', 'g', 'G', ConsoleKeyEx.G));
            _keys.Add(new KeyMapping(0x23, 'h', 'H', '\0', '\0', 'h', 'H', 'h', 'H', ConsoleKeyEx.H));
            _keys.Add(new KeyMapping(0x24, 'j', 'J', '\0', '\0', 'j', 'J', 'j', 'J', ConsoleKeyEx.J));
            _keys.Add(new KeyMapping(0x25, 'k', 'K', '\0', '\0', 'k', 'K', 'k', 'K', ConsoleKeyEx.K));
            _keys.Add(new KeyMapping(0x26, 'l', 'L', '\0', '\0', 'l', 'L', 'l', 'L', ConsoleKeyEx.L));
            _keys.Add(new KeyMapping(0x27, ';', ':', '\0', '\0', ';', ';', ':', ':', ConsoleKeyEx.Semicolon));
            _keys.Add(new KeyMapping(0x28, '\'', '"', '\0', '\0', '\'', '\'', '"', '"', ConsoleKeyEx.Apostrophe));
            _keys.Add(new KeyMapping(0x29, '`', '~', '\0', '\0', '`', '`', '~', '~', ConsoleKeyEx.Backquote));
            /* Left Shift*/
            _keys.Add(new KeyMapping(0x2A, ConsoleKeyEx.Shift));
            /* \ZXCVBNM,./ */
            _keys.Add(new KeyMapping(0x2B, '\\', '|', '\0', '\0', '\\', '\\', '|', '|', ConsoleKeyEx.Backslash));
            _keys.Add(new KeyMapping(0x2C, 'z', 'Z', '\0', '\0', 'z', 'Z', 'z', 'Z', ConsoleKeyEx.Z));
            _keys.Add(new KeyMapping(0x2D, 'x', 'X', '\0', '\0', 'x', 'X', 'x', 'X', ConsoleKeyEx.X));
            _keys.Add(new KeyMapping(0x2E, 'c', 'C', '\0', '\0', 'c', 'C', 'c', 'C', ConsoleKeyEx.C));
            _keys.Add(new KeyMapping(0x2F, 'v', 'V', '\0', '\0', 'v', 'V', 'v', 'V', ConsoleKeyEx.V));
            _keys.Add(new KeyMapping(0x30, 'b', 'B', '\0', '\0', 'b', 'B', 'b', 'B', ConsoleKeyEx.B));
            _keys.Add(new KeyMapping(0x31, 'n', 'N', '\0', '\0', 'n', 'N', 'n', 'N', ConsoleKeyEx.N));
            _keys.Add(new KeyMapping(0x32, 'm', 'M', '\0', '\0', 'm', 'M', 'm', 'M', ConsoleKeyEx.M));
            _keys.Add(new KeyMapping(0x33, ',', '<', '\0', '\0', ',', ',', '<', '<', ConsoleKeyEx.Comma));
            _keys.Add(new KeyMapping(0x34, '.', '>', '\0', '\0', '.', '.', '>', '>', ConsoleKeyEx.Period));
            _keys.Add(new KeyMapping(0x35, '/', '?', '\0', '\0', '/', '/', '?', '/', ConsoleKeyEx.Slash)); // also numpad divide
                                                                                                           /* Right Shift */
            _keys.Add(new KeyMapping(0x36, ConsoleKeyEx.Shift));
            /* Print Screen */
            _keys.Add(new KeyMapping(0x37, '*', '*', '\0', '\0', '*', '*', '*', '*', ConsoleKeyEx.PrintScreen)); // also numpad multiply
                                                                                                                 /* Alt  */
            _keys.Add(new KeyMapping(0x38, ConsoleKeyEx.Alt));
            /* Space */
            _keys.Add(new KeyMapping(0x39, ' ', ConsoleKeyEx.Spacebar));
            /* Caps */
            _keys.Add(new KeyMapping(0x3A, ConsoleKeyEx.CapsLock));
            /* F1-F12 */
            _keys.Add(new KeyMapping(0x3B, ConsoleKeyEx.F1));
            _keys.Add(new KeyMapping(0x3C, ConsoleKeyEx.F2));
            _keys.Add(new KeyMapping(0x3D, ConsoleKeyEx.F3));
            _keys.Add(new KeyMapping(0x3E, ConsoleKeyEx.F4));
            _keys.Add(new KeyMapping(0x3F, ConsoleKeyEx.F5));
            _keys.Add(new KeyMapping(0x40, ConsoleKeyEx.F6));
            _keys.Add(new KeyMapping(0x41, ConsoleKeyEx.F7));
            _keys.Add(new KeyMapping(0x42, ConsoleKeyEx.F8));
            _keys.Add(new KeyMapping(0x43, ConsoleKeyEx.F9));
            _keys.Add(new KeyMapping(0x44, ConsoleKeyEx.F10));
            _keys.Add(new KeyMapping(0x57, ConsoleKeyEx.F11));
            _keys.Add(new KeyMapping(0x58, ConsoleKeyEx.F12));
            /* Num Lock, Scrl Lock */
            _keys.Add(new KeyMapping(0x45, ConsoleKeyEx.NumLock));
            _keys.Add(new KeyMapping(0x46, ConsoleKeyEx.ScrollLock));
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            _keys.Add(new KeyMapping(0x47, '\0', '\0', '\0', '\0', '7', '\0', '\0', '\0', ConsoleKeyEx.Home, ConsoleKeyEx.Num7));
            _keys.Add(new KeyMapping(0x48, '\0', '\0', '\0', '\0', '8', '\0', '\0', '\0', ConsoleKeyEx.UpArrow, ConsoleKeyEx.Num8));
            _keys.Add(new KeyMapping(0x49, '\0', '\0', '\0', '\0', '9', '\0', '\0', '\0', ConsoleKeyEx.PageUp, ConsoleKeyEx.Num9));
            _keys.Add(new KeyMapping(0x4A, '-', '-', 0, 0, '-', '-', '-', '-', ConsoleKeyEx.NumMinus));
            _keys.Add(new KeyMapping(0x4B, '\0', '\0', '\0', '\0', '4', '\0', '\0', '\0', ConsoleKeyEx.LeftArrow, ConsoleKeyEx.Num4));
            _keys.Add(new KeyMapping(0x4C, '\0', '\0', '\0', '\0', '5', '\0', '\0', '\0', ConsoleKeyEx.Num5));
            _keys.Add(new KeyMapping(0x4D, '\0', '\0', '\0', '\0', '6', '\0', '\0', '\0', ConsoleKeyEx.RightArrow, ConsoleKeyEx.Num6));
            _keys.Add(new KeyMapping(0x4E, '+', '+', 0, 0, '+', '+', '+', '+', ConsoleKeyEx.NumPlus));
            _keys.Add(new KeyMapping(0x4F, '\0', '\0', '\0', '\0', '1', '\0', '\0', '\0', ConsoleKeyEx.End, ConsoleKeyEx.Num1));
            _keys.Add(new KeyMapping(0x50, '\0', '\0', '\0', '\0', '2', '\0', '\0', '\0', ConsoleKeyEx.DownArrow, ConsoleKeyEx.Num2));
            _keys.Add(new KeyMapping(0x51, '\0', '\0', '\0', '\0', '3', '\0', '\0', '\0', ConsoleKeyEx.PageDown, ConsoleKeyEx.Num3));
            _keys.Add(new KeyMapping(0x52, '\0', '\0', '\0', '\0', '0', '\0', '\0', '\0', ConsoleKeyEx.Insert, ConsoleKeyEx.Num0));
            _keys.Add(new KeyMapping(0x53, '\0', '\0', '\0', '\0', '.', '\0', '\0', '\0', ConsoleKeyEx.Delete, ConsoleKeyEx.NumPeriod));

            _keys.Add(new KeyMapping(0x5b, ConsoleKeyEx.LWin));
            _keys.Add(new KeyMapping(0x5c, ConsoleKeyEx.RWin));
            #endregion
        } 

        public override KeyEvent ConvertScanCode(byte scan2, bool ctrl, bool shift, bool alt, bool num, bool caps, bool scroll)
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
            if((scan & 0x80) != 0) scan = (byte) (scan ^ 0x80);          

            for (var index = 0; index < _keys.Count; index++)
            {
                var t = _keys[index];
                if (t.Scancode == scan)
                {
                    found = true;
                    var map = t;
                    var key = '\0';

                    if(shift)
                    {
                        if (caps) key = map.ShiftCaps;
                        else if (num) key = map.ShiftNum;
                        else key = map.Shift;
                    }
                    else if (ctrl)
                    {
                        key = map.Ctrl;
                    }
                    else if (alt)
                    {
                        key = map.Alt;
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
