using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.HAL
{
    public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);
    public class Keyboard : Device
    {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        private HandleKeyboardDelegate mHandleKeyboardKey;
        public void Initialize(HandleKeyboardDelegate aHandleKeyboardKeyDelegate)
        {
            mHandleKeyboardKey = aHandleKeyboardKeyDelegate;
        }

        public Keyboard()
        {
            Global.Dbg.Send("Keyboard ctor");
            mBuffer = new Queue<uint>(BufferSize);

            Initialize(HandleScancode);
            INTs.SetIrqHandler(0x01, HandleIRQ);
            // TODO: Need to add support for mult keyboards. ie one in PS2 and one in USB, or even more
            if (mKeys == null)
            {
                CreateDefaultKeymap();
            }
        }

        public void HandleIRQ(ref INTs.IRQContext aContext)
        {
            if (mHandleKeyboardKey != null)
            {
                byte xScanCode = IO.Port60.Byte;
                mHandleKeyboardKey(xScanCode, false);
            }
        }

        private Queue<uint> mBuffer;
        private const int BufferSize = 64;
        private List<KeyMapping> mKeys;
        private byte led_status;
        private bool _ctrl;
        private bool _shift;
        private bool _alt;

        public enum KeyboardLed : byte
        {
            ScrollLock = 1,
            NumLock = 2,
            CapsLock = 4
        }

        public enum ControlKey : byte
        {
            Shift = 1,
            Alt = 2,
            Control = 4
        }

        public bool ShiftPressed
        {
            get { return _shift; }
        }

        public bool CtrlPressed
        {
            get { return _ctrl; }
        }

        public bool AltPressed
        {
            get { return _alt; }
        }

        public bool NumLock
        {
            get { return (led_status & 2) != 0; }
        }

        public bool CapsLock
        {
            get { return (led_status & 4) != 0; }
        }

        public bool ScrollLock
        {
            get { return (led_status & 1) != 0; }
        }

        private void updateLed()
        {
            IO.Port60.Byte = 0xED;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
            IO.Port60.Byte = led_status;
            while ((new IOPort(0x64).Byte & 2) != 0)
            {
            }
        }


        protected void HandleScancode(byte aScancode, bool aReleased)
        {
            uint key = aScancode;
            if (key == 0x3A)
            {
                // caps lock
                led_status ^= 4;
                updateLed();
            }
            else if (key == 0x45)
            {
                // num lock
                led_status ^= 2;
                updateLed();
            }
            else if (key == 0x46)
            {
                // scroll lock
                led_status ^= 1;
                updateLed();
            }

            else if (key == 0x1D && !CtrlPressed) // Ctrl key
            {
                _ctrl = true;
            }
            else if (key == 0x80 + 0x1D) // Ctrl key released
            {
                _ctrl = false;
            }

            else if ((key == 0x2A || key == 0x36) && !ShiftPressed) // Shift key
            {
                _shift = true;
            }
            else if ((key == 0x80 + 0x2A) || (key == 0x80 + 0x36)) // Shift key released
            {
                _shift = false;
            }

            else if (key == 0x38 && !AltPressed) // Alt key
            {
                _alt = true;
            }
            else if (key == 0x80 + 0x38) // Alt key released
            {
                _alt = false;
            }
            else
            {
                if (CtrlPressed && AltPressed && (key == 0x53))
                {
                    Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                    Core.Global.CPU.Reboot();
                }
                if (mBuffer.Count < BufferSize)
                {

                    char xTheChar;
                    if (!GetCharValue(key, out xTheChar))
                    {
                        //DebugUtil.SendError("Keyboard", "error while getting scancode character!");
                    }
                    else
                    {
                        //DebugUtil.SendDoubleNumber("Keyboard", "Scancode and Char", xTheScancode, 32, xTheChar, 16);
                    }
                    mBuffer.Enqueue(key);

                }
            }
        }


        // Can merge HandleScancode after we remove old code
        // Remove the static.. Make it a real class
        protected void ByteReceived(byte aValue)
        {
            mHandleKeyboardKey(aValue, false);
        }

        private void CreateDefaultKeymap()
        {
            /*      ASCII - Shift - Ctrl -  Alt -   Num -   Caps -  Shift Caps -    Shift Num -   ConsoleKey */
            AddKey(0x00000, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            AddKey(0x01, 0x001B, 0x001B, 0x001B, 0x00, 0x001B, 0x1B, 0x1B, 0x1B, ConsoleKey.Escape);
            /* 1 -> 9 */
            AddKey(0x02, 0x31, 0x21, 0, 0x7800, 0x31, 0x31, 0x21, 0x21, ConsoleKey.D1);
            AddKey(0x03, 0x32, 0x40, 0x0300, 0x7900, 0x32, 0x32, 0x40, 0x40, ConsoleKey.D2);
            AddKey(0x04, 0x33, 0x23, 0, 0x7A00, 0x33, 0x33, 0x23, 0x23, ConsoleKey.D3);
            AddKey(0x05, 0x34, 0x24, 0, 0x7B00, 0x34, 0x34, 0x24, 0x24, ConsoleKey.D4);
            AddKey(0x06, 0x35, 0x25, 0, 0x7C00, 0x35, 0x35, 0x25, 0x25, ConsoleKey.D5);
            AddKey(0x07, 0x36, 0x5E, 0x1E, 0x7D00, 0x36, 0x36, 0x5E, 0x5E, ConsoleKey.D6);
            AddKey(0x08, 0x37, 0x26, 0, 0x7E00, 0x37, 0x37, 0x26, 0x26, ConsoleKey.D7);
            AddKey(0x09, 0x38, 0x2A, 0, 0x7F00, 0x38, 0x38, 0x2A, 0x2A, ConsoleKey.D8);
            AddKey(0x0A, 0x39, 0x28, 0, 0x8000, 0x39, 0x39, 0x28, 0x28, ConsoleKey.D9);
            AddKey(0x0B, 0x30, 0x29, 0, 0x8100, 0x30, 0x30, 0x29, 0x29, ConsoleKey.D0);
            /* -, =, Bksp, Tab */
            AddKey(0x0C, 0x2D, 0x5F, 0x1F, 0x8200, 0x2D, 0x2D, 0x5F, 0x5F, ConsoleKey.OemMinus);
            AddKey(0x0D, 0x3D, 0x2B, 0, 0x8300, 0x3D, 0x3D, 0x2B, 0x2B, ConsoleKey.NoName);
            AddKey(0x0E, 0x08, 0x08, 0x7F, 0, 0x08, 0x08, 0x08, 0x08, ConsoleKey.Backspace);
            AddKey(0x0F, 0x09, 0x0F00, 0, 0, 0x09, 0x09, 0x0F00, 0x0F00, ConsoleKey.Tab);
            /*      QWERTYUIOP[] */
            AddKey(0x10, 0x71, 0x51, 0x11, 0x1000, 0x71, 0x51, 0x71, 0x51, ConsoleKey.Q);
            AddKey(0x11, 0x77, 0x57, 0x17, 0x1100, 0x77, 0x57, 0x77, 0x57, ConsoleKey.W);
            AddKey(0x12, 0x65, 0x45, 0x05, 0x1200, 0x65, 0x45, 0x65, 0x45, ConsoleKey.E);
            AddKey(0x13, 0x72, 0x52, 0x12, 0x1300, 0x72, 0x52, 0x72, 0x52, ConsoleKey.R);
            AddKey(0x14, 0x74, 0x54, 0x14, 0x1400, 0x74, 0x54, 0x74, 0x54, ConsoleKey.T);
            AddKey(0x15, 0x79, 0x59, 0x19, 0x1500, 0x79, 0x59, 0x79, 0x59, ConsoleKey.Y);
            AddKey(0x16, 0x75, 0x55, 0x15, 0x1600, 0x75, 0x55, 0x75, 0x55, ConsoleKey.U);
            AddKey(0x17, 0x69, 0x49, 0x09, 0x1700, 0x69, 0x49, 0x69, 0x49, ConsoleKey.I);
            AddKey(0x18, 0x6F, 0x4F, 0x0F, 0x1800, 0x6F, 0x4F, 0x6F, 0x4F, ConsoleKey.O);
            AddKey(0x19, 0x70, 0x50, 0x10, 0x1900, 0x70, 0x50, 0x70, 0x50, ConsoleKey.P);
            AddKey(0x1A, 0x5B, 0x7B, 0x1B, 0x0, 0x5B, 0x5B, 0x7B, 0x7B, ConsoleKey.NoName);
            AddKey(0x1B, 0x5D, 0x7D, 0x1D, 0, 0x5D, 0x5D, 0x7D, 0x7D, ConsoleKey.NoName);
            /* ENTER, CTRL */
            AddKey(0x1C, 0x0A, 0x0A, 0x0D, 0, 0x0A, 0x0A, 0x0D, 0x0D, ConsoleKey.Enter);
            AddKey(0x1D, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* ASDFGHJKL;'~ */
            AddKey(0x1E, 0x61, 0x41, 0x01, 0x1E00, 0x61, 0x41, 0x61, 0x41, ConsoleKey.A);
            AddKey(0x1F, 0x73, 0x53, 0x13, 0x1F00, 0x73, 0x53, 0x73, 0x53, ConsoleKey.S);
            AddKey(0x20, 0x64, 0x44, 0x04, 0x2000, 0x64, 0x44, 0x64, 0x44, ConsoleKey.D);
            AddKey(0x21, 0x66, 0x46, 0x06, 0x2100, 0x66, 0x46, 0x66, 0x46, ConsoleKey.F);
            AddKey(0x22, 0x67, 0x47, 0x07, 0x2200, 0x67, 0x47, 0x67, 0x47, ConsoleKey.G);
            AddKey(0x23, 0x68, 0x48, 0x08, 0x2300, 0x68, 0x48, 0x68, 0x48, ConsoleKey.H);
            AddKey(0x24, 0x6A, 0x4A, 0x0A, 0x2400, 0x6A, 0x4A, 0x6A, 0x4A, ConsoleKey.J);
            AddKey(0x25, 0x6B, 0x4B, 0x0B, 0x3500, 0x6B, 0x4B, 0x6B, 0x4B, ConsoleKey.K);
            AddKey(0x26, 0x6C, 0x4C, 0x0C, 0x2600, 0x6C, 0x4C, 0x6C, 0x4C, ConsoleKey.L);
            AddKey(0x27, 0x3B, 0x3A, 0, 0, 0x3B, 0x3B, 0x3A, 0x3A, ConsoleKey.NoName);
            AddKey(0x28, 0x27, 0x22, 0, 0, 0x27, 0x27, 0x22, 0x22, ConsoleKey.NoName);
            AddKey(0x29, 0x60, 0x7E, 0, 0, 0x60, 0x60, 0x7E, 0x7E, ConsoleKey.NoName);
            /* Left Shift*/
            AddKey(0x2A, 0x2A, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* \ZXCVBNM,./ */
            AddKey(0x2B, 0x5C, 0x7C, 0x1C, 0, 0x5C, 0x5C, 0x7C, 0x7C, ConsoleKey.NoName);
            AddKey(0x2C, 0x7A, 0x5A, 0x1A, 0x2C00, 0x7A, 0x5A, 0x7A, 0x5A, ConsoleKey.Z);
            AddKey(0x2D, 0x78, 0x58, 0x18, 0x2D00, 0x78, 0x58, 0x78, 0x58, ConsoleKey.X);
            AddKey(0x2E, 0x63, 0x43, 0x03, 0x2E00, 0x63, 0x43, 0x63, 0x43, ConsoleKey.C);
            AddKey(0x2F, 0x76, 0x56, 0x16, 0x2F00, 0x76, 0x56, 0x76, 0x56, ConsoleKey.V);
            AddKey(0x30, 0x62, 0x42, 0x02, 0x3000, 0x62, 0x42, 0x62, 0x42, ConsoleKey.B);
            AddKey(0x31, 0x6E, 0x4E, 0x0E, 0x3100, 0x6E, 0x4E, 0x6E, 0x4E, ConsoleKey.N);
            AddKey(0x32, 0x6D, 0x4D, 0x0D, 0x3200, 0x6D, 0x4D, 0x6D, 0x4D, ConsoleKey.M);
            AddKey(0x33, 0x2C, 0x3C, 0, 0, 0x2C, 0x2C, 0x3C, 0x3C, ConsoleKey.OemComma);
            AddKey(0x34, 0x2E, 0x3E, 0, 0, 0x2E, 0x2E, 0x3E, 0x3E, ConsoleKey.OemPeriod);
            AddKey(0x35, 0x2F, 0x3F, 0, 0, 0x2F, 0x2F, 0x3F, 0x3F, ConsoleKey.Divide);
            /* Right Shift */
            AddKey(0x36, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* Print Screen */
            AddKey(0x37, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.PrintScreen);
            /* Alt  */
            AddKey(0x38, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* Space */
            AddKey(0x39, 0x20, 0x20, 0x20, 0, 0x20, 0x20, 0x20, 0x20, ConsoleKey.Spacebar);
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
            AddKey(0x57, 0x5700, 0x5700, 0x5700, 0x5700, 0x5700, 0x5700, 0x5700, 0x5700, ConsoleKey.F11);
            AddKey(0x58, 0x5800, 0x5800, 0x5800, 0x5800, 0x5800, 0x5800, 0x5800, 0x5800, ConsoleKey.F12);
            /* Num Lock, Scrl Lock */
            AddKey(0x45, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            AddKey(0x46, 0, 0, 0, 0, 0, 0, 0, 0, ConsoleKey.NoName);
            /* HOME, Up, Pgup, -kpad, left, center, right, +keypad, end, down, pgdn, ins, del */
            AddKey(0x47, 0x4700, 0x37, 0x7700, 0, 0x37, 0x4700, 0x37, 0x4700, ConsoleKey.Home);
            AddKey(0x48, 0x4800, 0x38, 0, 0, 0x38, 0x4800, 0x38, 0x4800, ConsoleKey.UpArrow);
            AddKey(0x49, 0x4900, 0x39, 0x8400, 0, 0x39, 0x4900, 0x39, 0x4900, ConsoleKey.PageUp);
            AddKey(0x4A, 0x2D, 0x2D, 0, 0, 0x2D, 0x2D, 0x2D, 0x2D, ConsoleKey.OemMinus);
            AddKey(0x4B, 0x4B00, 0x34, 0x7300, 0, 0x34, 0x4B00, 0x34, 0x4B00, ConsoleKey.LeftArrow);
            AddKey(0x4C, 0x4C00, 0x35, 0, 0, 0x35, 0x4C00, 0x35, 0x4C00, ConsoleKey.NumPad5);
            AddKey(0x4D, 0x4D00, 0x36, 0x7400, 0, 0x36, 0x4D00, 0x36, 0x4D00, ConsoleKey.RightArrow);
            AddKey(0x4E, 0x2B, 0x2B, 0, 0, 0x2B, 0x2B, 0x2B, 0x2B, ConsoleKey.OemPlus);
            AddKey(0x4F, 0x4F00, 0x31, 0x7500, 0, 0x31, 0x4F00, 0x31, 0x4F00, ConsoleKey.End);
            AddKey(0x50, 0x5000, 0x32, 0, 0, 0x32, 0x5000, 0x32, 0x5000, ConsoleKey.DownArrow);
            AddKey(0x51, 0x5100, 0x33, 0x7600, 0, 0x33, 0x5100, 0x33, 0x5100, ConsoleKey.PageDown);
            AddKey(0x52, 0x5200, 0x30, 0, 0, 0x30, 0x5200, 0x30, 0x5200, ConsoleKey.Insert);
            AddKey(0x53, 0x5300, 0x2E, 0, 0, 0x2E, 0x5300, 0x2E, 0x5300, ConsoleKey.Delete);

            Global.Dbg.Send("Keyboard default key mapping");
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
            for (int i = 0; i < mKeys.Count; i++)
            {
                //if (i == 0) {
                //  Console.Write("ScanCode in KeyMapping: ");
                //  Interrupts.WriteNumber(mKeys[i].Scancode, 32);
                //  Console.WriteLine("");
                //}
                if (mKeys[i].Scancode == aScanCode)
                {
                    if (mKeys[i].Value != '\0')
                    {
                        var map = mKeys[i];
                        char key = '\0';

                        if (ShiftPressed && CapsLock) key = map.ShiftCaps;
                        else if (ShiftPressed) key = map.Shift;
                        else if (CtrlPressed) key = map.Ctrl;
                        else if (AltPressed) key = map.Alt;
                        else if (ShiftPressed && NumLock) key = map.ShiftNum;
                        else if (CapsLock) key = map.Caps;
                        else if (NumLock) key = map.Num;
                        else if (!CtrlPressed && !AltPressed && !ShiftPressed) key = map.Value;

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
            for (int i = 0; i < mKeys.Count; i++)
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

        public char ReadChar()
        {
            char xResult = '\0';
            while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult))
            {
                //Global.Sleep(10); //ToDo optimize value 
                if (Core.Global.CPU == null)
                {
                    return '\0';
                }
                Core.Global.CPU.Halt();
            }
            return xResult;
        }

        public bool GetChar(out char c)
        {
            c = '\0';

            if (mBuffer.Count > 0)
            {
                GetCharValue(mBuffer.Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }

        public ConsoleKey ReadKey()
        {
            ConsoleKey xResult = ConsoleKey.NoName;
            while (mBuffer.Count == 0 || !GetKeyValue(mBuffer.Dequeue(), out xResult))
            {
                //Global.Sleep(10); //ToDo optimize value 
                Core.Global.CPU.Halt();
            }
            return xResult;
        }
        public bool GetKey(out ConsoleKey c)
        {
            c = ConsoleKey.NoName;

            if (mBuffer.Count > 0)
            {
                GetKeyValue(mBuffer.Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }

        public KeyMapping ReadMapping()
        {
            KeyMapping xResult = null;
            while (mBuffer.Count == 0 || !GetKeyMapping(mBuffer.Dequeue(), out xResult))
            {
                //Global.Sleep(10); //ToDo optimize value 
                Core.Global.CPU.Halt();
            }
            return xResult;
        }
        public bool GetMapping(out KeyMapping c)
        {
            c = null;

            if (mBuffer.Count > 0)
            {
                GetKeyMapping(mBuffer.Dequeue(), out c);
                return true;
            }
            else
            {
                return false;
            }
        }

        public uint ReadScancode()
        {
            while (mBuffer.Count == 0)
            {
                Core.Global.CPU.Halt();
            }

            return mBuffer.Dequeue();
        }
        public bool GetScancode(out uint c)
        {
            if (mBuffer.Count > 0)
            {
                c = mBuffer.Dequeue();
                return true;
            }
            else
            {
                c = 0;
                return false;
            }
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
