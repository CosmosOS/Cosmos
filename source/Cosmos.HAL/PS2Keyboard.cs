using System;
using System.Collections.Generic;
using Cosmos.Common.Extensions;
using Cosmos.Core;

namespace Cosmos.HAL
{
    //public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);

    public class PS2Keyboard : Keyboard
    {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        public PS2Keyboard() : base()
        {
            Initialize();
            Core.INTs.SetIrqHandler(0x01, HandleIRQ);
        }

        protected override void Initialize()
        {
            if (mKeys == null)
            {
                CreateDefaultKeymap();
            }
        }

        private List<KeyMapping> mKeys;

        private void HandleIRQ(ref Core.INTs.IRQContext aContext)
        {
            byte xScanCode = IO.Port60.Byte;
            bool xReleased = (xScanCode & 0x80) == 0x80;
            if (xReleased)
            {
                xScanCode = (byte)(xScanCode ^ 0x80);
            }
            HandleScancode(xScanCode, xReleased);
        }

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
            byte key = aScancode;
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
                                KeyEvent keyInfo;
                                if (GetKey(key, aReleased, out keyInfo))
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

            /*     Scan  Norm Shift Ctrl Alt     Num  Caps ShCaps ShNum ConsoleKeyEx */
            AddKey(0x00, ConsoleKeyEx.NoName);
            AddKey(0x01, ConsoleKeyEx.Escape);
            /* 1 -> 9 */
            AddKey(0x02, '1', '!', '\0', '\0', '1', '1', '!', '1', ConsoleKeyEx.D1);
            AddKey(0x03, '2', '@', '\0', '\0', '2', '2', '@', '2', ConsoleKeyEx.D2);
            AddKey(0x04, '3', '#', '\0', '\0', '3', '3', '#', '3', ConsoleKeyEx.D3);
            AddKey(0x05, '4', '$', '\0', '\0', '4', '4', '$', '4', ConsoleKeyEx.D4);
            AddKey(0x06, '5', '%', '\0', '\0', '5', '5', '%', '5', ConsoleKeyEx.D5);
            AddKey(0x07, '6', '^', '\0', '\0', '6', '6', '^', '6', ConsoleKeyEx.D6);
            AddKey(0x08, '7', '&', '\0', '\0', '7', '7', '&', '7', ConsoleKeyEx.D7);
            AddKey(0x09, '8', '*', '\0', '\0', '8', '8', '*', '8', ConsoleKeyEx.D8);
            AddKey(0x0A, '9', '(', '\0', '\0', '9', '9', '(', '9', ConsoleKeyEx.D9);
            AddKey(0x0B, '0', ')', '\0', '\0', '0', '0', ')', '0', ConsoleKeyEx.D0);
            /* -, =, Bksp, Tab */
            AddKey(0x0C, '-', '_', '\0', '\0', '-', '-', '_', '-', ConsoleKeyEx.Minus);
            AddKey(0x0D, '=', '+', '\0', '\0', '=', '=', '+', '=', ConsoleKeyEx.Equal);
            AddKey(0x0E, ConsoleKeyEx.Backspace);
            AddKey(0x0F, '\t', ConsoleKeyEx.Tab);
            /*      QWERTYUIOP[] */
            AddKey(0x10, 'q', 'Q', '\0', '\0', 'q', 'Q', 'q', 'Q', ConsoleKeyEx.Q);
            AddKey(0x11, 'w', 'W', '\0', '\0', 'w', 'W', 'w', 'W', ConsoleKeyEx.W);
            AddKey(0x12, 'e', 'E', '\0', '\0', 'e', 'E', 'e', 'E', ConsoleKeyEx.E);
            AddKey(0x13, 'r', 'R', '\0', '\0', 'r', 'R', 'r', 'R', ConsoleKeyEx.R);
            AddKey(0x14, 't', 'T', '\0', '\0', 't', 'T', 't', 'T', ConsoleKeyEx.T);
            AddKey(0x15, 'y', 'Y', '\0', '\0', 'y', 'Y', 'y', 'Y', ConsoleKeyEx.Y);
            AddKey(0x16, 'u', 'U', '\0', '\0', 'u', 'U', 'u', 'U', ConsoleKeyEx.U);
            AddKey(0x17, 'i', 'I', '\0', '\0', 'i', 'I', 'i', 'I', ConsoleKeyEx.I);
            AddKey(0x18, 'o', 'O', '\0', '\0', 'o', 'O', 'o', 'O', ConsoleKeyEx.O);
            AddKey(0x19, 'p', 'P', '\0', '\0', 'p', 'P', 'p', 'P', ConsoleKeyEx.P);
            AddKey(0x1A, '[', '{', '\0', '\0', '[', '{', '[', '{', ConsoleKeyEx.LBracket);
            AddKey(0x1B, ']', '}', '\0', '\0', ']', '}', ']', '}', ConsoleKeyEx.RBracket);
            /* ENTER, CTRL */
            AddKey(0x1C, '\n', ConsoleKeyEx.Enter);
            AddKey(0x1D, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKeyEx.Ctrl);
            /* ASDFGHJKL;'` */
            AddKey(0x1E, 'a', 'A', '\0', '\0', 'a', 'A', 'a', 'A', ConsoleKeyEx.A);
            AddKey(0x1F, 's', 'S', '\0', '\0', 's', 'S', 's', 'S', ConsoleKeyEx.S);
            AddKey(0x20, 'd', 'D', '\0', '\0', 'd', 'D', 'd', 'D', ConsoleKeyEx.D);
            AddKey(0x21, 'f', 'F', '\0', '\0', 'f', 'F', 'f', 'F', ConsoleKeyEx.F);
            AddKey(0x22, 'g', 'G', '\0', '\0', 'g', 'G', 'g', 'G', ConsoleKeyEx.G);
            AddKey(0x23, 'h', 'H', '\0', '\0', 'h', 'H', 'h', 'H', ConsoleKeyEx.H);
            AddKey(0x24, 'j', 'J', '\0', '\0', 'j', 'J', 'j', 'J', ConsoleKeyEx.J);
            AddKey(0x25, 'k', 'K', '\0', '\0', 'k', 'K', 'k', 'K', ConsoleKeyEx.K);
            AddKey(0x26, 'l', 'L', '\0', '\0', 'l', 'L', 'l', 'L', ConsoleKeyEx.L);
            AddKey(0x27, ';', ':', '\0', '\0', ';', ';', ':', ':', ConsoleKeyEx.Semicolon);
            AddKey(0x28, '\'', '"', '\0', '\0', '\'', '\'', '"', '"', ConsoleKeyEx.Apostrophe);
            AddKey(0x29, '`', '~', '\0', '\0', '`', '`', '~', '~', ConsoleKeyEx.Backquote);
            /* Left Shift*/
            AddKey(0x2A, ConsoleKeyEx.Shift);
            /* \ZXCVBNM,./ */
            AddKey(0x2B, '\\', '|', '\0', '\0', '\\', '\\', '|', '|', ConsoleKeyEx.Backslash);
            AddKey(0x2C, 'z', 'Z', '\0', '\0', 'z', 'Z', 'z', 'Z', ConsoleKeyEx.Z);
            AddKey(0x2D, 'x', 'X', '\0', '\0', 'x', 'X', 'x', 'X', ConsoleKeyEx.X);
            AddKey(0x2E, 'c', 'C', '\0', '\0', 'c', 'C', 'c', 'C', ConsoleKeyEx.C);
            AddKey(0x2F, 'v', 'V', '\0', '\0', 'v', 'V', 'v', 'V', ConsoleKeyEx.V);
            AddKey(0x30, 'b', 'B', '\0', '\0', 'b', 'B', 'b', 'B', ConsoleKeyEx.B);
            AddKey(0x31, 'n', 'N', '\0', '\0', 'n', 'N', 'n', 'N', ConsoleKeyEx.N);
            AddKey(0x32, 'm', 'M', '\0', '\0', 'm', 'M', 'm', 'M', ConsoleKeyEx.M);
            AddKey(0x33, ',', '<', '\0', '\0', ',', ',', '<', '<', ConsoleKeyEx.Comma);
            AddKey(0x34, '.', '>', '\0', '\0', '.', '.', '>', '>', ConsoleKeyEx.Period);
            AddKey(0x35, '/', '?', '\0', '\0', '/', '/', '?', '/', ConsoleKeyEx.Slash); // also numpad divide
            /* Right Shift */
            AddKey(0x36, ConsoleKeyEx.Shift);
            /* Print Screen */
            AddKey(0x37, '*', '*', '\0', '\0', '*', '*', '*', '*', ConsoleKeyEx.PrintScreen); // also numpad multiply
            /* Alt  */
            AddKey(0x38, ConsoleKeyEx.Alt);
            /* Space */
            AddKey(0x39, ' ', ConsoleKeyEx.Spacebar);
            /* Caps */
            AddKey(0x3A, ConsoleKeyEx.CapsLock);
            /* F1-F12 */
            AddKey(0x3B, ConsoleKeyEx.F1);
            AddKey(0x3C, ConsoleKeyEx.F2);
            AddKey(0x3D, ConsoleKeyEx.F3);
            AddKey(0x3E, ConsoleKeyEx.F4);
            AddKey(0x3F, ConsoleKeyEx.F5);
            AddKey(0x40, ConsoleKeyEx.F6);
            AddKey(0x41, ConsoleKeyEx.F7);
            AddKey(0x42, ConsoleKeyEx.F8);
            AddKey(0x43, ConsoleKeyEx.F9);
            AddKey(0x44, ConsoleKeyEx.F10);
            AddKey(0x57, ConsoleKeyEx.F11);
            AddKey(0x58, ConsoleKeyEx.F12);
            /* Num Lock, Scrl Lock */
            AddKey(0x45, ConsoleKeyEx.NumLock);
            AddKey(0x46, ConsoleKeyEx.ScrollLock);
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            AddKey(0x47, '\0', '\0', '\0', '\0', '7', '\0', '\0', '\0', ConsoleKeyEx.Home, ConsoleKeyEx.Num7);
            AddKey(0x48, '\0', '\0', '\0', '\0', '8', '\0', '\0', '\0', ConsoleKeyEx.UpArrow, ConsoleKeyEx.Num8);
            AddKey(0x49, '\0', '\0', '\0', '\0', '9', '\0', '\0', '\0', ConsoleKeyEx.PageUp, ConsoleKeyEx.Num9);
            AddKey(0x4A, '-', '-', 0, 0, '-', '-', '-', '-', ConsoleKeyEx.NumMinus);
            AddKey(0x4B, '\0', '\0', '\0', '\0', '4', '\0', '\0', '\0', ConsoleKeyEx.LeftArrow, ConsoleKeyEx.Num4);
            AddKey(0x4C, '\0', '\0', '\0', '\0', '5', '\0', '\0', '\0', ConsoleKeyEx.Num5);
            AddKey(0x4D, '\0', '\0', '\0', '\0', '6', '\0', '\0', '\0', ConsoleKeyEx.RightArrow, ConsoleKeyEx.Num6);
            AddKey(0x4E, '+', '+', 0, 0, '+', '+', '+', '+', ConsoleKeyEx.NumPlus);
            AddKey(0x4F, '\0', '\0', '\0', '\0', '1', '\0', '\0', '\0', ConsoleKeyEx.End, ConsoleKeyEx.Num1);
            AddKey(0x50, '\0', '\0', '\0', '\0', '2', '\0', '\0', '\0', ConsoleKeyEx.DownArrow, ConsoleKeyEx.Num2);
            AddKey(0x51, '\0', '\0', '\0', '\0', '3', '\0', '\0', '\0', ConsoleKeyEx.PageDown, ConsoleKeyEx.Num3);
            AddKey(0x52, '\0', '\0', '\0', '\0', '0', '\0', '\0', '\0', ConsoleKeyEx.Insert, ConsoleKeyEx.Num0);
            AddKey(0x53, '\0', '\0', '\0', '\0', '.', '\0', '\0', '\0', ConsoleKeyEx.Delete, ConsoleKeyEx.NumPeriod);



            AddKey(0x5b, ConsoleKeyEx.LWin);
            AddKey(0x5c, ConsoleKeyEx.RWin);

        }


        private void AddKey(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }
        private void AddKey(uint aScanCode, char norm, char shift, char ctrl, char alt, char num, char caps, char shiftcaps, char shiftnum, ConsoleKeyEx aKey, ConsoleKeyEx numKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey, numKey));
        }
        private void AddKey(uint aScanCode, int norm, int shift, int ctrl, int alt, int num, int caps, int shiftcaps, int shiftnum, ConsoleKeyEx aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, shift, ctrl, alt, num, caps, shiftcaps, shiftnum, aKey));
        }
        private void AddKey(uint aScanCode, char norm, ConsoleKeyEx aKey)
        {
            mKeys.Add(new KeyMapping(aScanCode, norm, aKey));
        }
        private void AddKey(uint aScanCode, ConsoleKeyEx aKey)
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

        public bool GetKeyValue(uint aScanCode, out ConsoleKeyEx aValue)
        {
            for (var index = 0; index < mKeys.Count; index++)
            {
                var t = mKeys[index];
                if (t.Scancode == aScanCode)
                {
                    if (NumLock)
                    {
                        if (ShiftPressed && aScanCode >= 0x47 && aScanCode != 0x4A && aScanCode != 0x4E && aScanCode <= 0x52)
                        {
                            aValue = t.Key; // ex: Shift+NumPad4=LeftArrow (extended keyset not supported yet)
                        }
                        else
                        {
                            aValue = t.NumLockKey;
                        }
                    }
                    else
                    {
                        aValue = t.Key;
                    }
                    //aValue = NumLock ? t.NumLockKey : t.Key;
                    return true;
                }
            }

            aValue = ConsoleKeyEx.NoName;
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


        public bool GetKey(byte aScancode, bool released, out KeyEvent keyInfo)
        {
            /*ConsoleKeyEx xKey;

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
            keyInfo = new KeyEvent(xChar, xKey, ShiftPressed, AltPressed, ControlPressed, released ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make);*/
            //var ev = KeyLayout.ConvertScanCode(aScancode);
            //keyInfo = new KeyEvent(ev.KeyChar, ev.Key, ShiftPressed, AltPressed, ControlPressed, released ? KeyEvent.KeyEventType.Break : KeyEvent.KeyEventType.Make);
            //return true;
            keyInfo = KeyLayout.ConvertScanCode(aScancode, ControlPressed, ShiftPressed, AltPressed, NumLock, CapsLock, ScrollLock);
            return keyInfo != null;
        }
    }
}
