using System;
using System.Collections.Generic;
using Cosmos.Core;

namespace Cosmos.HAL
{
    //public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);

    public class DefaultKeyboard: Keyboard
    {

        protected override void Initialize()
        {
            if (mKeys == null)
            {
                CreateDefaultKeymap();
            }
        }

        private bool mEscaped;
        private List<KeyMapping> mKeys;
        private bool mShiftState;
        private bool mCtrlState;
        private bool mAltState;
        private bool _num;
        private bool _scroll;
        private bool _caps;

        public bool ShiftPressed
        {
            get
            {
                return mShiftState;
            }
        }

        public bool CtrlPressed
        {
            get
            {
                return mCtrlState;
            }
        }

        public bool AltPressed
        {
            get
            {
                return mAltState;
            }
        }
        public bool NumLock
        {
            get { return _num; }
        }

        public bool CapsLock
        {
            get { return _caps; }
        }

        public bool ScrollLock
        {
            get { return _scroll; }
        }

        private void updateLed()
        {
            IO.Port60.Byte = 0xED;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
            var led_status = (_scroll ? 1 : 0) | ((_num ? 1 : 0) << 1) | ((_caps ? 1 : 0) << 2);
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
                _caps = !_caps;
                updateLed();
            }
            else if (key == 0x45 && !aReleased)
            {
                // num lock
                _num = !_num;
                updateLed();
            }
            else if (key == 0x46 && !aReleased)
            {
                // scroll lock
                _scroll = !_scroll;
                updateLed();
            }
            else
                switch (key)
                {
                    case 0x1D:
                        mCtrlState = !aReleased;
                        break;
                    case 0x2A:
                    case 0x36:
                        mShiftState = !aReleased;
                        break;
                    case 0x38:
                        mAltState = !aReleased;
                        break;
                    default:
                        if (CtrlPressed && AltPressed && (key == 0x53))
                        {
                            Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                            Core.Global.CPU.Reboot();
                        }

                        if (!aReleased)
                        {
                            ConsoleKeyInfoEx keyInfo;
                            GetKey(key, out keyInfo);
                            Enqueue(keyInfo);
                        }


                        break;
                }
        }

        private void CreateDefaultKeymap()
        {
            mKeys = new List<KeyMapping>();

            /*     Scan  Norm Shift Ctrl Alt     Num  Caps ShCaps ShNum ConsoleKey */
            AddKey(0x00000, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            AddKey(0x01, 0x001B, 0x001B, 0x001B, 0x00, 0x001B, 0x1B, 0x1B, 0x1B, ConsoleKey.Escape);
            /* 1 -> 9 */
            AddKey(0x02, '1', '!', '\0', 0x7800, '1', '1', '!', '1', ConsoleKey.D1);
            AddKey(0x03, '2', '@', '\0', 0x7900, '2', '2', '@', '2', ConsoleKey.D2);
            AddKey(0x04, '3', '#', '\0', 0x7A00, '3', '3', '#', '3', ConsoleKey.D3);
            AddKey(0x05, '4', '$', '\0', 0x7B00, '4', '4', '$', '4', ConsoleKey.D4);
            AddKey(0x06, '5', '%', '\0', 0x7C00, '5', '5', '%', '5', ConsoleKey.D5);
            AddKey(0x07, '6', '^', '\0', 0x7D00, '6', '6', '^', '6', ConsoleKey.D6);
            AddKey(0x08, '7', '&', '\0', 0x7E00, '7', '7', '&', '7', ConsoleKey.D7);
            AddKey(0x09, '8', '*', '\0', 0x7F00, '8', '8', '*', '8', ConsoleKey.D8);
            AddKey(0x0A, '9', '(', '\0', 0x8000, '9', '9', '(', '9', ConsoleKey.D9);
            AddKey(0x0B, '0', ')', '\0', 0x8100, '0', '0', ')', '0', ConsoleKey.D0);
            /* -, =, Bksp, Tab */
            AddKey(0x0C, '-', '_', '\0', 0x8200, '-', '-', '_', '-', ConsoleKey.OemMinus);
            AddKey(0x0D, '=', '+', '\0', 0x8300, '=', '=', '+', '=', ConsoleKey.NoName);
            AddKey(0x0E, '\u0968', '\u0968', '\u0968', '\u0968', '\u0968', '\u0968', '\u0968', '\u0968', ConsoleKey.Backspace);
            AddKey(0x0F, '\t', '\t', '\t', '\t', '\t', '\t', '\t', '\t', ConsoleKey.Tab);
            /*      QWERTYUIOP[] */
            AddKey(0x10, 'q', 'Q', '\0', 0x1000, 'q', 'Q', 'q', 'Q', ConsoleKey.Q);
            AddKey(0x11, 'w', 'W', '\0', 0x1100, 'w', 'W', 'w', 'W', ConsoleKey.W);
            AddKey(0x12, 'e', 'E', '\0', 0x1200, 'e', 'E', 'e', 'E', ConsoleKey.E);
            AddKey(0x13, 'r', 'R', '\0', 0x1300, 'r', 'R', 'r', 'R', ConsoleKey.R);
            AddKey(0x14, 't', 'T', '\0', 0x1400, 't', 'T', 't', 'T', ConsoleKey.T);
            AddKey(0x15, 'y', 'Y', '\0', 0x1500, 'y', 'Y', 'y', 'Y', ConsoleKey.Y);
            AddKey(0x16, 'u', 'U', '\0', 0x1600, 'u', 'U', 'u', 'U', ConsoleKey.U);
            AddKey(0x17, 'i', 'I', '\0', 0x1700, 'i', 'I', 'i', 'I', ConsoleKey.I);
            AddKey(0x18, 'o', 'O', '\0', 0x1800, 'o', 'O', 'o', 'O', ConsoleKey.O);
            AddKey(0x19, 'p', 'P', '\0', 0x1900, 'p', 'P', 'p', 'P', ConsoleKey.P);
            AddKey(0x1A, '[', '{', '\0', 0x0000, '[', '{', '[', '{', ConsoleKey.NoName);
            AddKey(0x1B, ']', '}', '\0', 0x0000, ']', '}', ']', '}', ConsoleKey.NoName);
            /* ENTER, CTRL */
            AddKey(0x1C, '\n', '\n', '\n', '\n', '\n', '\n', '\n', '\n', ConsoleKey.Enter);
            AddKey(0x1D, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* ASDFGHJKL;'` */
            AddKey(0x1E, 'a', 'A', '\0', 0x1E00, 'a', 'A', 'a', 'A', ConsoleKey.A);
            AddKey(0x1F, 's', 'S', '\0', 0x1F00, 's', 'S', 's', 'S', ConsoleKey.S);
            AddKey(0x20, 'd', 'D', '\0', 0x2000, 'd', 'D', 'd', 'D', ConsoleKey.D);
            AddKey(0x21, 'f', 'F', '\0', 0x2100, 'f', 'F', 'f', 'F', ConsoleKey.F);
            AddKey(0x22, 'g', 'G', '\0', 0x2200, 'g', 'G', 'g', 'G', ConsoleKey.G);
            AddKey(0x23, 'h', 'H', '\0', 0x2300, 'h', 'H', 'h', 'H', ConsoleKey.H);
            AddKey(0x24, 'j', 'J', '\0', 0x2400, 'j', 'J', 'j', 'J', ConsoleKey.J);
            AddKey(0x25, 'k', 'K', '\0', 0x3500, 'k', 'K', 'k', 'K', ConsoleKey.K);
            AddKey(0x26, 'l', 'L', '\0', 0x2600, 'l', 'L', 'l', 'L', ConsoleKey.L);
            AddKey(0x27, ';', ':', '\0', 0x0000, ';', ';', ':', ':', ConsoleKey.NoName);
            AddKey(0x28, '\'', '"', '\0', 0x0000, '\'', '\'', '"', '"', ConsoleKey.NoName);
            AddKey(0x29, '`', '~', '\0', 0x0000, '`', '`', '~', '~', ConsoleKey.NoName);
            /* Left Shift*/
            AddKey(0x2A, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* \ZXCVBNM,./ */
            AddKey(0x2B, '\\', '|', '\0', 0x0000, '\\', '\\', '|', '|', ConsoleKey.NoName);
            AddKey(0x2C, 'z', 'Z', '\0', 0x2C00, 'z', 'Z', 'z', 'Z', ConsoleKey.Z);
            AddKey(0x2D, 'x', 'X', '\0', 0x2D00, 'x', 'X', 'x', 'X', ConsoleKey.X);
            AddKey(0x2E, 'c', 'C', '\0', 0x2E00, 'c', 'C', 'c', 'C', ConsoleKey.C);
            AddKey(0x2F, 'v', 'V', '\0', 0x2F00, 'v', 'V', 'v', 'V', ConsoleKey.V);
            AddKey(0x30, 'b', 'B', '\0', 0x3000, 'b', 'B', 'b', 'B', ConsoleKey.B);
            AddKey(0x31, 'n', 'N', '\0', 0x3100, 'n', 'N', 'n', 'N', ConsoleKey.N);
            AddKey(0x32, 'm', 'M', '\0', 0x3200, 'm', 'M', 'm', 'M', ConsoleKey.M);
            AddKey(0x33, ',', '<', '\0', 0x0000, ',', ',', '<', '<', ConsoleKey.OemComma);
            AddKey(0x34, '.', '>', '\0', 0x0000, '.', '.', '>', '>', ConsoleKey.OemPeriod);
            AddKey(0x35, '/', '?', '\0', 0x0000, '/', '/', '?', '?', ConsoleKey.Divide);
            /* Right Shift */
            AddKey(0x36, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* Print Screen */
            AddKey(0x37, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.PrintScreen);
            /* Alt  */
            AddKey(0x38, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* Space */
            AddKey(0x39, ' ', ' ', ' ', ' ', ' ', ' ', ' ', ' ', ConsoleKey.Spacebar);
            /* Caps */
            AddKey(0x3A, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* F1-F12 */
            AddKey(0x3B, 0x3B00, 0x5400, 0x5E00, 0x6800, 0x3B00, 0x3B00, 0x5400, 0x5400, ConsoleKey.F1);
            AddKey(0x3C, 0x3C00, 0x5500, 0x5F00, 0x6900, 0x3C00, 0x3C00, 0x5500, 0x5500, ConsoleKey.F2);
            AddKey(0x3D, 0x3D00, 0x5600, 0x6000, 0x6A00, 0x3D00, 0x3D00, 0x5600, 0x5600, ConsoleKey.F3);
            AddKey(0x3E, 0x3E00, 0x5700, 0x6100, 0x6B00, 0x3E00, 0x3E00, 0x5700, 0x5700, ConsoleKey.F4);
            AddKey(0x3F, 0x3F00, 0x5800, 0x6200, 0x6C00, 0x3F00, 0x3F00, 0x5800, 0x5800, ConsoleKey.F5);
            AddKey(0x40, 0x4000, 0x5900, 0x6300, 0x6D00, 0x4000, 0x4000, 0x5900, 0x5900, ConsoleKey.F6);
            AddKey(0x41, 0x4100, 0x5A00, 0x6400, 0x6E00, 0x4100, 0x4100, 0x5A00, 0x5A00, ConsoleKey.F7);
            AddKey(0x42, 0x4200, 0x5B00, 0x6500, 0x6F00, 0x4200, 0x4200, 0x5B00, 0x5B00, ConsoleKey.F8);
            AddKey(0x43, 0x4300, 0x5C00, 0x6600, 0x7000, 0x4300, 0x4300, 0x5C00, 0x5C00, ConsoleKey.F9);
            AddKey(0x44, 0x4400, 0x5D00, 0x6700, 0x7100, 0x4400, 0x4400, 0x5D00, 0x5D00, ConsoleKey.F10);
            AddKey(0x57, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, ConsoleKey.F11); // Todo: Add the chars
            AddKey(0x58, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, ConsoleKey.F12);
            /* Num Lock, Scrl Lock */
            AddKey(0x45, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            AddKey(0x46, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            AddKey(0x47, 0x4700, '7', 0x7700, 0, '7', 0x4700, '7', 0x4700, ConsoleKey.Home);
            AddKey(0x48, '\u2191', '8', '\u2191', '\u2191', '8', '\u2191', '8', '\u2191', ConsoleKey.UpArrow);
            AddKey(0x49, 0x4900, '9', 0x8400, 0, '9', 0x4900, '9', 0x4900, ConsoleKey.PageUp);
            AddKey(0x4A, '-', '-', 0, 0, '-', '-', '-', '-', ConsoleKey.OemMinus);
            AddKey(0x4B, '\u2190', '4', '\u2190', '\u2190', '4', '\u2190', '4', '\u2190', ConsoleKey.LeftArrow);
            AddKey(0x4C, 0x4C00, '5', 0, 0, '5', 0x4C00, '5', 0x4C00, ConsoleKey.NumPad5);
            AddKey(0x4D, '\u2192', '6', '\u2192', '\u2192', '6', '\u2192', '6', '\u2192', ConsoleKey.RightArrow);
            AddKey(0x4E, '+', '+', 0, 0, '+', '+', '+', '+', ConsoleKey.OemPlus);
            AddKey(0x4F, 0x4F00, '1', 0x7500, 0, '1', 0x4F00, '1', 0x4F00, ConsoleKey.End);
            AddKey(0x50, '\u2193', '2', '\u2193', '\u2193', '2', '\u2193', '2', '\u2193', ConsoleKey.DownArrow);
            AddKey(0x51, 0x5100, '3', 0x7600, 0, '3', 0x5100, '3', 0x5100, ConsoleKey.PageDown);
            AddKey(0x52, 0x5200, '0', 0, 0, '0', 0x5200, '0', 0x5200, ConsoleKey.Insert);
            AddKey(0x53, 0x5300, '.', 0, 0, '.', 0x5300, '.', 0x5300, ConsoleKey.Delete);



            AddKey(0x5b, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.LeftWindows);
            AddKey(0x5c, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.RightWindows);
        }


        private void AddKey(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }
        private void AddKey(uint aScanCode, int norm, int shift, int ctrl, int alt, int num, int caps, int shiftcaps, int shiftnum, ConsoleKey aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }

        public void ChangeKeyMap(List<KeyMapping> aKeys)
        {
            mKeys = aKeys;
        }

        public bool GetCharValue(uint aScanCode, out char aValue)
        {
            for (var i = 0; i < mKeys.Count; i++)
            {
                if (mKeys[i].Scancode == aScanCode)
                {
                    if (mKeys[i].Value != '\0')
                    {
                        var map = mKeys[i];
                        var key = '\0';

                        if (ShiftPressed && CapsLock) key = map.ShiftCaps;
                        else if (ShiftPressed) key = map.Shift;
                        else if (CtrlPressed) key = map.Ctrl;
                        else if (AltPressed) key = map.Alt;
                        else if (ShiftPressed && NumLock) key = map.ShiftNum;
                        else if (CapsLock) key = map.Caps;
                        else if (NumLock) key = map.Num;
                        else key = map.Value;

                        aValue = key;
                        return true;
                    }
                    break;
                }
            }

            aValue = '\0';
            return false;
        }

        public bool GetKeyValue(uint aScanCode, out ConsoleKey aValue)
        {
            for (var i = 0; i < mKeys.Count; i++)
            {
                if (mKeys[i].Scancode == aScanCode)
                {
                    aValue = mKeys[i].Key;
                    return true;
                }
            }

            aValue = ConsoleKey.NoName;
            return false;
        }

        public bool GetKeyMapping(uint aScanCode, out KeyMapping aValue)
        {
            for (int i = 0; i < mKeys.Count; i++)
            {

                if (mKeys[i].Scancode == aScanCode)
                {
                    aValue = mKeys[i];
                    return true;
                }
            }

            aValue = null;
            return false;
        }

        //public char ReadChar()
        //{
        //    char xResult = '\0';
        //    while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult))
        //    {
        //        //Global.Sleep(10); //ToDo optimize value
        //        if (Core.Global.CPU == null)
        //        {
        //            return '\0';
        //        }
        //        Core.Global.CPU.Halt();
        //    }
        //    return xResult;
        //}

        //public bool GetChar(out char c)
        //{
        //    c = '\0';

        //    if (mBuffer.Count > 0)
        //    {
        //        GetCharValue(mBuffer.Dequeue(), out c);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public ConsoleKey ReadKey()
        //{
        //    ConsoleKey xResult = ConsoleKey.NoName;
        //    while (mBuffer.Count == 0 || !GetKeyValue(mBuffer.Dequeue(), out xResult))
        //    {
        //        //Global.Sleep(10); //ToDo optimize value
        //        Core.Global.CPU.Halt();
        //    }
        //    return xResult;
        //}

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

        //public bool GetMapping(out KeyMapping c)
        //{
        //    c = null;

        //    if (mBuffer.Count > 0)
        //    {
        //        GetKeyMapping(mBuffer.Dequeue(), out c);
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public uint ReadScancode()
        //{
        //    while (mBuffer.Count == 0)
        //    {
        //        Core.Global.CPU.Halt();
        //    }

        //    return mBuffer.Dequeue();
        //}

        //public bool GetScancode(out uint c)
        //{
        //    if (mBuffer.Count > 0)
        //    {
        //        c = mBuffer.Dequeue();
        //        return true;
        //    }
        //    else
        //    {
        //        c = 0;
        //        return false;
        //    }
        //}

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
            }
        }
    }
}
