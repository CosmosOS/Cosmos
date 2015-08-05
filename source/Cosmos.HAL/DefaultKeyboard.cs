using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;

namespace Cosmos.HAL
{
    //public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);

    public class DefaultKeyboard : Keyboard
    {
        public DefaultKeyboard()
        {
        }

        protected override void Initialize()
        {
            if (mKeys == null)
            {
                CreateDefaultKeymap();
            }
        }

        private List<KeyMapping> mKeys;

        private void updateLed()
        {
            IO.Port60.Byte = 0xED;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
            var led_status = (ScrollLock ? 1 : 0) | ((NumLock ? 1 : 0) << 1) | ((CapsLock ? 1 : 0) << 2);
            IO.Port60.Byte = (byte)led_status;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
        }

        protected override void HandleScancode(byte aScancode, bool aReleased)
        {
            uint key = aScancode;
            if (key == 0x3A && !aReleased)
            {
                // caps lock
                CapsLock = !CapsLock;
                updateLed();
            }
            else if (key == 0x45 && !aReleased)
            {
                // num lock
                NumLock = !NumLock;
                updateLed();
            }
            else if (key == 0x46 && !aReleased)
            {
                // scroll lock
                ScrollLock = !ScrollLock;
                updateLed();
            }
            else
                switch (key)
                {
                    case 0x1D:
                        {
                            ControlPressed = !aReleased;
                            break;
                        }
                    case 0x2A:
                    case 0x36:
                        {
                            ShiftPressed = !aReleased;
                            break;
                        }
                    case 0x38:
                        {
                            AltPressed = !aReleased;
                            break;
                        }
                    default:
                        {
                            if (ControlPressed && AltPressed && (key == 0x53))
                            {
                                Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                                Core.Global.CPU.Reboot();
                            }

                            if (!aReleased)
                            {
                                ConsoleKeyInfoEx keyInfo;
                                if (GetKey(key, out keyInfo))
                                {
                                    Enqueue(keyInfo);
                                }
                            }


                            break;
                        }
                }
        }

        private void CreateDefaultKeymap()
        {
            mKeys = new List<KeyMapping>();

            /*     Scan  Norm Shift Ctrl Alt     Num  Caps ShCaps ShNum ConsoleKey */
            AddKey(0x00, ConsoleKey.NoName);
            AddKey(0x01, ConsoleKey.Escape);
            /* 1 -> 9 */
            AddKey(0x02, '1', '!', '\0', '\0', '1', '1', '!', '1', ConsoleKey.D1);
            AddKey(0x03, '2', '@', '\0', '\0', '2', '2', '@', '2', ConsoleKey.D2);
            AddKey(0x04, '3', '#', '\0', '\0', '3', '3', '#', '3', ConsoleKey.D3);
            AddKey(0x05, '4', '$', '\0', '\0', '4', '4', '$', '4', ConsoleKey.D4);
            AddKey(0x06, '5', '%', '\0', '\0', '5', '5', '%', '5', ConsoleKey.D5);
            AddKey(0x07, '6', '^', '\0', '\0', '6', '6', '^', '6', ConsoleKey.D6);
            AddKey(0x08, '7', '&', '\0', '\0', '7', '7', '&', '7', ConsoleKey.D7);
            AddKey(0x09, '8', '*', '\0', '\0', '8', '8', '*', '8', ConsoleKey.D8);
            AddKey(0x0A, '9', '(', '\0', '\0', '9', '9', '(', '9', ConsoleKey.D9);
            AddKey(0x0B, '0', ')', '\0', '\0', '0', '0', ')', '0', ConsoleKey.D0);
            /* -, =, Bksp, Tab */
            AddKey(0x0C, '-', '_', '\0', '\0', '-', '-', '_', '-', ConsoleKey.OemMinus);
            AddKey(0x0D, '=', '+', '\0', '\0', '=', '=', '+', '=', ConsoleKey.NoName);
            AddKey(0x0E, ConsoleKey.Backspace);
            AddKey(0x0F, '\t', ConsoleKey.Tab);
            /*      QWERTYUIOP[] */
            AddKey(0x10, 'q', 'Q', '\0', '\0', 'q', 'Q', 'q', 'Q', ConsoleKey.Q);
            AddKey(0x11, 'w', 'W', '\0', '\0', 'w', 'W', 'w', 'W', ConsoleKey.W);
            AddKey(0x12, 'e', 'E', '\0', '\0', 'e', 'E', 'e', 'E', ConsoleKey.E);
            AddKey(0x13, 'r', 'R', '\0', '\0', 'r', 'R', 'r', 'R', ConsoleKey.R);
            AddKey(0x14, 't', 'T', '\0', '\0', 't', 'T', 't', 'T', ConsoleKey.T);
            AddKey(0x15, 'y', 'Y', '\0', '\0', 'y', 'Y', 'y', 'Y', ConsoleKey.Y);
            AddKey(0x16, 'u', 'U', '\0', '\0', 'u', 'U', 'u', 'U', ConsoleKey.U);
            AddKey(0x17, 'i', 'I', '\0', '\0', 'i', 'I', 'i', 'I', ConsoleKey.I);
            AddKey(0x18, 'o', 'O', '\0', '\0', 'o', 'O', 'o', 'O', ConsoleKey.O);
            AddKey(0x19, 'p', 'P', '\0', '\0', 'p', 'P', 'p', 'P', ConsoleKey.P);
            AddKey(0x1A, '[', '{', '\0', '\0', '[', '{', '[', '{', ConsoleKey.NoName);
            AddKey(0x1B, ']', '}', '\0', '\0', ']', '}', ']', '}', ConsoleKey.NoName);
            /* ENTER, CTRL */
            AddKey(0x1C, '\n', ConsoleKey.Enter);
            AddKey(0x1D, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* ASDFGHJKL;'` */
            AddKey(0x1E, 'a', 'A', '\0', '\0', 'a', 'A', 'a', 'A', ConsoleKey.A);
            AddKey(0x1F, 's', 'S', '\0', '\0', 's', 'S', 's', 'S', ConsoleKey.S);
            AddKey(0x20, 'd', 'D', '\0', '\0', 'd', 'D', 'd', 'D', ConsoleKey.D);
            AddKey(0x21, 'f', 'F', '\0', '\0', 'f', 'F', 'f', 'F', ConsoleKey.F);
            AddKey(0x22, 'g', 'G', '\0', '\0', 'g', 'G', 'g', 'G', ConsoleKey.G);
            AddKey(0x23, 'h', 'H', '\0', '\0', 'h', 'H', 'h', 'H', ConsoleKey.H);
            AddKey(0x24, 'j', 'J', '\0', '\0', 'j', 'J', 'j', 'J', ConsoleKey.J);
            AddKey(0x25, 'k', 'K', '\0', '\0', 'k', 'K', 'k', 'K', ConsoleKey.K);
            AddKey(0x26, 'l', 'L', '\0', '\0', 'l', 'L', 'l', 'L', ConsoleKey.L);
            AddKey(0x27, ';', ':', '\0', '\0', ';', ';', ':', ':', ConsoleKey.NoName);
            AddKey(0x28, '\'', '"', '\0', '\0', '\'', '\'', '"', '"', ConsoleKey.NoName);
            AddKey(0x29, '`', '~', '\0', '\0', '`', '`', '~', '~', ConsoleKey.NoName);
            /* Left Shift*/
            AddKey(0x2A, ConsoleKey.NoName);
            /* \ZXCVBNM,./ */
            AddKey(0x2B, '\\', '|', '\0', '\0', '\\', '\\', '|', '|', ConsoleKey.NoName);
            AddKey(0x2C, 'z', 'Z', '\0', '\0', 'z', 'Z', 'z', 'Z', ConsoleKey.Z);
            AddKey(0x2D, 'x', 'X', '\0', '\0', 'x', 'X', 'x', 'X', ConsoleKey.X);
            AddKey(0x2E, 'c', 'C', '\0', '\0', 'c', 'C', 'c', 'C', ConsoleKey.C);
            AddKey(0x2F, 'v', 'V', '\0', '\0', 'v', 'V', 'v', 'V', ConsoleKey.V);
            AddKey(0x30, 'b', 'B', '\0', '\0', 'b', 'B', 'b', 'B', ConsoleKey.B);
            AddKey(0x31, 'n', 'N', '\0', '\0', 'n', 'N', 'n', 'N', ConsoleKey.N);
            AddKey(0x32, 'm', 'M', '\0', '\0', 'm', 'M', 'm', 'M', ConsoleKey.M);
            AddKey(0x33, ',', '<', '\0', '\0', ',', ',', '<', '<', ConsoleKey.OemComma);
            AddKey(0x34, '.', '>', '\0', '\0', '.', '.', '>', '>', ConsoleKey.OemPeriod);
            AddKey(0x35, '/', '?', '\0', '\0', '/', '/', '?', '/', ConsoleKey.Divide); // also numpad divide
            /* Right Shift */
            AddKey(0x36, ConsoleKey.NoName);
            /* Print Screen */
            AddKey(0x37, '*', '*', '\0', '\0', '*', '*', '*', '*', ConsoleKey.PrintScreen); // also numpad multiply
            /* Alt  */
            AddKey(0x38, ConsoleKey.NoName);
            /* Space */
            AddKey(0x39, ' ', ConsoleKey.Spacebar);
            /* Caps */
            AddKey(0x3A, ConsoleKey.NoName);
            /* F1-F12 */
            AddKey(0x3B, ConsoleKey.F1);
            AddKey(0x3C, ConsoleKey.F2);
            AddKey(0x3D, ConsoleKey.F3);
            AddKey(0x3E, ConsoleKey.F4);
            AddKey(0x3F, ConsoleKey.F5);
            AddKey(0x40, ConsoleKey.F6);
            AddKey(0x41, ConsoleKey.F7);
            AddKey(0x42, ConsoleKey.F8);
            AddKey(0x43, ConsoleKey.F9);
            AddKey(0x44, ConsoleKey.F10);
            AddKey(0x57, ConsoleKey.F11);
            AddKey(0x58, ConsoleKey.F12);
            /* Num Lock, Scrl Lock */
            AddKey(0x45, ConsoleKey.NoName);
            AddKey(0x46, ConsoleKey.NoName);
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            AddKey(0x47, '\0', '\0', '\0', '\0', '7', '\0', '\0', '\0', ConsoleKey.Home, ConsoleKey.NumPad7);
            AddKey(0x48, '\0', '\0', '\0', '\0', '8', '\0', '\0', '\0', ConsoleKey.UpArrow, ConsoleKey.NumPad8);
            AddKey(0x49, '\0', '\0', '\0', '\0', '9', '\0', '\0', '\0', ConsoleKey.PageUp, ConsoleKey.NumPad9);
            AddKey(0x4A, '-', '-', 0, 0, '-', '-', '-', '-', ConsoleKey.OemMinus);
            AddKey(0x4B, '\0', '\0', '\0', '\0', '4', '\0', '\0', '\0', ConsoleKey.LeftArrow, ConsoleKey.NumPad4);
            AddKey(0x4C, '\0', '\0', '\0', '\0', '5', '\0', '\0', '\0', ConsoleKey.NumPad5);
            AddKey(0x4D, '\0', '\0', '\0', '\0', '6', '\0', '\0', '\0', ConsoleKey.RightArrow, ConsoleKey.NumPad6);
            AddKey(0x4E, '+', '+', 0, 0, '+', '+', '+', '+', ConsoleKey.OemPlus);
            AddKey(0x4F, '\0', '\0', '\0', '\0', '1', '\0', '\0', '\0', ConsoleKey.End, ConsoleKey.NumPad1);
            AddKey(0x50, '\0', '\0', '\0', '\0', '2', '\0', '\0', '\0', ConsoleKey.DownArrow, ConsoleKey.NumPad2);
            AddKey(0x51, '\0', '\0', '\0', '\0', '3', '\0', '\0', '\0', ConsoleKey.PageDown, ConsoleKey.NumPad3);
            AddKey(0x52, '\0', '\0', '\0', '\0', '0', '\0', '\0', '\0', ConsoleKey.Insert, ConsoleKey.NumPad0);
            AddKey(0x53, '\0', '\0', '\0', '\0', '.', '\0', '\0', '\0', ConsoleKey.Delete, ConsoleKey.OemPeriod);



            AddKey(0x5b, ConsoleKey.LeftWindows);
            AddKey(0x5c, ConsoleKey.RightWindows);

        }


        private void AddKey(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }
        private void AddKey(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKey aKey, ConsoleKey numKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey, numKey));
        }
        private void AddKey(uint aScanCode, int norm, int shift, int ctrl, int alt, int num, int caps, int shiftcaps, int shiftnum, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }
        private void AddKey(uint aScanCode, char norm, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, aKey));
        }
        private void AddKey(uint aScanCode, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, aKey));
        }
        public void ChangeKeyMap(List<KeyMapping> aKeys)
        {
            mKeys = aKeys;
        }

        public bool GetCharValue(uint aScanCode, out char aValue)
        {
            for (var index = 0; index < mKeys.Count; index++)
            {
                KeyMapping t = mKeys[index];
                if (t.Scancode == aScanCode)
                {
                    var map = t;
                    var key = '\0';

                    if (ShiftPressed && CapsLock)
                    {
                        key = map.ShiftCaps;
                    }
                    else if (ShiftPressed)
                    {
                        key = map.Shift;
                    }
                    else if (ControlPressed)
                    {
                        key = map.Ctrl;
                    }
                    else if (AltPressed)
                    {
                        key = map.Alt;
                    }
                    else if (ShiftPressed && NumLock)
                    {
                        key = map.ShiftNum;
                    }
                    else if (CapsLock)
                    {
                        key = map.Caps;
                    }
                    else if (NumLock)
                    {
                        key = map.Num;
                    }
                    else
                    {
                        key = map.Value;
                    }

                    aValue = key;
                    return true;
                }
            }

            aValue = '\0';
            return false;
        }

        public bool GetKeyValue(uint aScanCode, out ConsoleKey aValue)
        {
            for (var index = 0; index < mKeys.Count; index++)
            {
                var t = mKeys[index];
                if (t.Scancode == aScanCode)
                {
                    aValue = NumLock ? t.NumLockKey : t.Key;
                    return true;
                }
            }

            aValue = ConsoleKey.NoName;
            return false;
        }

        public bool GetKeyMapping(uint aScanCode, out KeyMapping aValue)
        {
            for (var index = 0; index < mKeys.Count; index++)
            {
                var t = mKeys[index];
                if (t.Scancode == aScanCode)
                {
                    aValue = t;
                    return true;
                }
            }

            aValue = null;
            return false;
        }


        public bool GetKey(uint aScancode, out ConsoleKeyInfoEx keyInfo)
        {
            ConsoleKey xKey;

            if (!GetKeyValue(aScancode, out xKey))
            {
                keyInfo = null;
                return false;
            }

            char xChar;
            if (!GetCharValue(aScancode, out xChar))
            {
                keyInfo = null;
                return false;
            }
            keyInfo = new ConsoleKeyInfoEx(xChar, xKey, ShiftPressed, AltPressed, ControlPressed);
            return true;
        }
        public class KeyMapping
        {
            public uint Scancode;
            public char Value;
            public char Shift;
            public char Ctrl;
            public char Alt;
            public char Num;
            public char Caps;
            public char ShiftCaps;
            public char ShiftNum;
            public ConsoleKey Key;
            public ConsoleKey NumLockKey;

            public KeyMapping(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKey aKey)
            {
                Scancode = aScanCode;
                Value = norm;
                Shift = shift;
                Ctrl = ctrl;
                Alt = alt;
                Num = num;
                Caps = caps;
                ShiftCaps = shiftcaps;
                ShiftNum = shiftnum;
                Key = aKey;
                NumLockKey = aKey;
            }
            public KeyMapping(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKey aKey, ConsoleKey numKey)
            {
                Scancode = aScanCode;
                Value = norm;
                Shift = shift;
                Ctrl = ctrl;
                Alt = alt;
                Num = num;
                Caps = caps;
                ShiftCaps = shiftcaps;
                ShiftNum = shiftnum;
                Key = aKey;
                NumLockKey = numKey;
            }
            public KeyMapping(uint aScanCode, int norm, int shift, int ctrl, int alt, int num, int caps, int shiftcaps, int shiftnum, ConsoleKey aKey)
            {
                Scancode = aScanCode;
                Value = (char)norm;
                Shift = (char)shift;
                Ctrl = (char)ctrl;
                Alt = (char)alt;
                Num = (char)num;
                Caps = (char)caps;
                ShiftCaps = (char)shiftcaps;
                ShiftNum = (char)shiftnum;
                Key = aKey;
                NumLockKey = aKey;
            }
            public KeyMapping(uint aScanCode, byte norm, byte shift, byte ctrl, byte alt, byte num, byte caps, byte shiftcaps, byte shiftnum, ConsoleKey aKey)
            {
                Scancode = aScanCode;
                Value = (char)norm;
                Shift = (char)shift;
                Ctrl = (char)ctrl;
                Alt = (char)alt;
                Num = (char)num;
                Caps = (char)caps;
                ShiftCaps = (char)shiftcaps;
                ShiftNum = (char)shiftnum;
                Key = aKey;
                NumLockKey = aKey;
            }
            public KeyMapping(uint aScanCode, char n, ConsoleKey aKey)
            {
                Scancode = aScanCode;
                Value = n;
                Shift = n;
                Ctrl = n;
                Alt = n;
                Num = n;
                Caps = n;
                ShiftCaps = n;
                ShiftNum = n;
                Key = aKey;
                NumLockKey = aKey;
            }
            public KeyMapping(uint aScanCode, ConsoleKey aKey)
            {
                Scancode = aScanCode;
                Value = '\0';
                Shift = '\0';
                Ctrl = '\0';
                Alt = '\0';
                Num = '\0';
                Caps = '\0';
                ShiftCaps = '\0';
                ShiftNum = '\0';
                Key = aKey;
                NumLockKey = aKey;
            }
        }
    }
}
