using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL {
    public delegate void HandleKeyboardDelegate(byte aScanCode, bool aReleased);
    public class Keyboard : Device {
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;

        private HandleKeyboardDelegate mHandleKeyboardKey;
        public void Initialize(HandleKeyboardDelegate aHandleKeyboardKeyDelegate) {
            mHandleKeyboardKey = aHandleKeyboardKeyDelegate;
        }

        public Keyboard() {
            mBuffer = new Queue<uint>(BufferSize);

            Initialize(HandleScancode);
            Core.INTs.SetIrqHandler(0x01, HandleIRQ);
            // TODO: Need to add support for mult keyboards. ie one in PS2 and one in USB, or even more

            if (mKeys == null) {
                CreateDefaultKeymap();
            }
        }

        public void HandleIRQ(ref Core.INTs.IRQContext aContext) {
            if (mHandleKeyboardKey != null) {
                byte xScanCode = IO.Port60.Byte;
                bool xReleased = (xScanCode & 0x80) == 0x80;
                if (xReleased) {
                    xScanCode = (byte)(xScanCode ^ 0x80);
                }
                mHandleKeyboardKey(xScanCode, xReleased);
            }
        }

        private Queue<uint> mBuffer;
        private const int BufferSize = 64;
        private bool mEscaped;
        private List<KeyMapping> mKeys;
        private bool mShiftState;
        private bool mCtrlState;
        private bool mAltState;

        public bool ShiftPressed {
            get {
                return mShiftState;
            }
        }
        public bool CtrlPressed {
            get {
                return mCtrlState;
            }
        }
        public bool AltPressed {
            get {
                return mAltState;
            }
        }

        protected void HandleScancode(byte aScancode, bool aReleased) {
            uint xTheScancode = aScancode;
            if (mEscaped) {
                xTheScancode = (ushort)(xTheScancode << 8);
                mEscaped = false;
            }
            switch (xTheScancode) {
                case 0x36:
                case 0x2A: {
                        mShiftState = !aReleased;
                        break;
                    }
                case 0x1D: {
                        mCtrlState = !aReleased;
                        break;
                    }
                case 0x38: {
                        mAltState = !aReleased;
                        break;
                    }
                default: {
                        if ((mCtrlState) && (mAltState) && (xTheScancode == 0x53)) {
                            Console.WriteLine("Detected Ctrl-Alt-Delete! Rebooting System...");
                            Core.Global.CPU.Reboot();
                        }
                        if (mShiftState) {
                            xTheScancode = xTheScancode << 16;
                        }
                        if (mBuffer.Count < BufferSize) {
                            if (!aReleased) {
                                char xTheChar;
                                if (!GetCharValue(xTheScancode, out xTheChar)) {
                                    //DebugUtil.SendError("Keyboard", "error while getting scancode character!");
                                } else {
                                    //DebugUtil.SendDoubleNumber("Keyboard", "Scancode and Char", xTheScancode, 32, xTheChar, 16);
                                }
                                mBuffer.Enqueue(xTheScancode);
                            }
                        }
                        break;
                    }
            }
        }

        // Can merge HandleScancode after we remove old code
        // Remove the static.. Make it a real class
        protected void ByteReceived(byte aValue) {
            bool xReleased = (aValue & 0x80) == 0x80;
            if (xReleased) {
                aValue = (byte)(aValue ^ 0x80);
            }
            mHandleKeyboardKey(aValue, xReleased);
        }

        private void CreateDefaultKeymap() {
            mKeys = new List<KeyMapping>(164);

            //TODO: fn (for laptops)

            #region Letters
            AddKey(0x10, 'q', ConsoleKey.Q);
            AddKey(0x100000, 'Q', ConsoleKey.Q);
            AddKey(0x11, 'w', ConsoleKey.W);
            AddKey(0x110000, 'W', ConsoleKey.W);
            AddKey(0x12, 'e', ConsoleKey.E);
            AddKey(0x120000, 'E', ConsoleKey.E);
            AddKey(0x13, 'r', ConsoleKey.R);
            AddKey(0x130000, 'R', ConsoleKey.R);
            AddKey(0x14, 't', ConsoleKey.T);
            AddKey(0x140000, 'T', ConsoleKey.T);
            AddKey(0x15, 'y', ConsoleKey.Y);
            AddKey(0x150000, 'Y', ConsoleKey.Y);
            AddKey(0x16, 'u', ConsoleKey.U);
            AddKey(0x160000, 'U', ConsoleKey.U);
            AddKey(0x17, 'i', ConsoleKey.I);
            AddKey(0x170000, 'I', ConsoleKey.I);
            AddKey(0x18, 'o', ConsoleKey.O);
            AddKey(0x180000, 'O', ConsoleKey.O);
            AddKey(0x19, 'p', ConsoleKey.P);
            AddKey(0x190000, 'P', ConsoleKey.P);

            AddKey(0x1E, 'a', ConsoleKey.A);
            AddKey(0x1E0000, 'A', ConsoleKey.A);
            AddKey(0x1F, 's', ConsoleKey.S);
            AddKey(0x1F0000, 'S', ConsoleKey.S);
            AddKey(0x20, 'd', ConsoleKey.D);
            AddKey(0x200000, 'D', ConsoleKey.D);
            AddKey(0x21, 'f', ConsoleKey.F);
            AddKey(0x210000, 'F', ConsoleKey.F);
            AddKey(0x22, 'g', ConsoleKey.G);
            AddKey(0x220000, 'G', ConsoleKey.G);
            AddKey(0x23, 'h', ConsoleKey.H);
            AddKey(0x230000, 'H', ConsoleKey.H);
            AddKey(0x24, 'j', ConsoleKey.J);
            AddKey(0x240000, 'J', ConsoleKey.J);
            AddKey(0x25, 'k', ConsoleKey.K);
            AddKey(0x250000, 'K', ConsoleKey.K);
            AddKey(0x26, 'l', ConsoleKey.L);
            AddKey(0x260000, 'L', ConsoleKey.L);

            AddKey(0x2C, 'z', ConsoleKey.Z);
            AddKey(0x2C0000, 'Z', ConsoleKey.Z);
            AddKey(0x2D, 'x', ConsoleKey.X);
            AddKey(0x2D0000, 'X', ConsoleKey.X);
            AddKey(0x2E, 'c', ConsoleKey.C);
            AddKey(0x2E0000, 'C', ConsoleKey.C);
            AddKey(0x2F, 'v', ConsoleKey.V);
            AddKey(0x2F0000, 'V', ConsoleKey.V);
            AddKey(0x30, 'b', ConsoleKey.B);
            AddKey(0x300000, 'B', ConsoleKey.B);
            AddKey(0x31, 'n', ConsoleKey.N);
            AddKey(0x310000, 'N', ConsoleKey.N);
            AddKey(0x32, 'm', ConsoleKey.M);
            AddKey(0x320000, 'M', ConsoleKey.M);
            #endregion

            #region digits
            //AddKey(0x1, '`');
            //AddKey(0x10000, '~');
            AddKey(0x29, '`', ConsoleKey.NoName);
            AddKey(0x290000, '~', ConsoleKey.NoName);
            AddKey(0x2, '1', ConsoleKey.D1);
            AddKey(0x20000, '!', ConsoleKey.D1);
            AddKey(0x3, '2', ConsoleKey.D2);
            AddKey(0x30000, '@', ConsoleKey.D2);
            AddKey(0x4, '3', ConsoleKey.D3);
            AddKey(0x40000, '#', ConsoleKey.D3);
            AddKey(0x5, '4', ConsoleKey.D4);
            AddKey(0x50000, '$', ConsoleKey.D5);
            AddKey(0x6, '5', ConsoleKey.D5);
            AddKey(0x60000, '%', ConsoleKey.D5);
            AddKey(0x7, '6', ConsoleKey.D6);
            AddKey(0x70000, '^', ConsoleKey.D6);
            AddKey(0x8, '7', ConsoleKey.D7);
            AddKey(0x80000, '&', ConsoleKey.D7);
            AddKey(0x9, '8', ConsoleKey.D8);
            AddKey(0x90000, '*', ConsoleKey.D8);
            AddKey(0xA, '9', ConsoleKey.D9);
            AddKey(0xA0000, '(', ConsoleKey.D9);
            AddKey(0xB, '0', ConsoleKey.D0);
            AddKey(0xB0000, ')', ConsoleKey.D0);

            #endregion

            #region Special
            AddKeyWithShift(0x0E, '\u0968', ConsoleKey.Backspace);               //Backspace
            AddKeyWithShift(0x0F, '\t', ConsoleKey.Tab);                         //Tabulator
            AddKeyWithShift(0x1C, '\n', ConsoleKey.Enter);                       //Enter
            AddKeyWithShift(0x39, ' ', ConsoleKey.Spacebar);                     //Space
            AddKeyWithShift(0x4b, '\u2190', ConsoleKey.LeftArrow);               //Left arrow
            AddKeyWithShift(0x48, '\u2191', ConsoleKey.UpArrow);                 //Up arrow
            AddKeyWithShift(0x4d, '\u2192', ConsoleKey.RightArrow);              //Right arrow
            AddKeyWithShift(0x50, '\u2193', ConsoleKey.DownArrow);               //Down arrow

            AddKeyWithShift(0x5b, ConsoleKey.LeftWindows);
            AddKeyWithShift(0x5c, ConsoleKey.RightWindows);
            //AddKey(0x5d, ConsoleKey.NoName);                                   //Context Menu  

            AddKeyWithShift(0x52, ConsoleKey.Insert);
            AddKeyWithShift(0x47, ConsoleKey.Home);
            AddKeyWithShift(0x49, ConsoleKey.PageUp);
            AddKeyWithShift(0x53, ConsoleKey.Delete);
            AddKeyWithShift(0x4f, ConsoleKey.End);
            AddKeyWithShift(0x51, ConsoleKey.PageDown);

            AddKeyWithShift(0x37, ConsoleKey.PrintScreen);
            //AddKeyWithShift(0x46, ConsoleKey.NoName);                          //Scroll Lock
            //AddKeyWithShift(0x3a, ConsoleKey.NoName);                          //Caps Lock
            AddKeyWithShift(0x45, ConsoleKey.Pause);

            AddKeyWithShift(0x3b, ConsoleKey.F1);
            AddKeyWithShift(0x3c, ConsoleKey.F2);
            AddKeyWithShift(0x3d, ConsoleKey.F3);
            AddKeyWithShift(0x3e, ConsoleKey.F4);
            AddKeyWithShift(0x3f, ConsoleKey.F5);
            AddKeyWithShift(0x40, ConsoleKey.F6);
            AddKeyWithShift(0x41, ConsoleKey.F7);
            AddKeyWithShift(0x42, ConsoleKey.F8);
            AddKeyWithShift(0x43, ConsoleKey.F9);
            AddKeyWithShift(0x44, ConsoleKey.F10);
            AddKeyWithShift(0x57, ConsoleKey.F11);
            AddKeyWithShift(0x58, ConsoleKey.F12);

            AddKeyWithShift(0x1, ConsoleKey.Escape);
            #endregion

            #region Punctuation and Signs
            AddKey(0x27, ';', ConsoleKey.NoName);
            AddKey(0x270000, ':', ConsoleKey.NoName);
            AddKey(0x28, '\'', ConsoleKey.NoName);
            AddKey(0x280000, '"', ConsoleKey.NoName);
            AddKey(0x2B, '\\', ConsoleKey.NoName);
            AddKey(0x2B0000, '|', ConsoleKey.NoName);
            AddKey(0x33, ',', ConsoleKey.OemComma);
            AddKey(0x330000, '<', ConsoleKey.OemComma);
            AddKey(0x34, '.', ConsoleKey.OemPeriod);
            AddKey(0x340000, '>', ConsoleKey.OemPeriod);
            AddKey(0x35, '/', ConsoleKey.Divide);
            AddKey(0x350000, '?', ConsoleKey.Divide);
            //AddKey(0x4A, '-');
            AddKey(0x0C, '-', ConsoleKey.Subtract);
            AddKey(0x0C0000, '_', ConsoleKey.Subtract);
            AddKey(0x0D, '=', ConsoleKey.OemPlus);
            AddKey(0x0D0000, '+', ConsoleKey.OemPlus);
            //AddKey(0x4E, '+');
            AddKey(0x1A, '[', ConsoleKey.NoName);
            AddKey(0x1A0000, '{', ConsoleKey.NoName);
            AddKey(0x1B, ']', ConsoleKey.NoName);
            AddKey(0x1B0000, '}', ConsoleKey.NoName);

            AddKeyWithShift(0x4c, '5', ConsoleKey.NumPad5);

            AddKeyWithShift(0x4a, '-', ConsoleKey.OemMinus);
            AddKeyWithShift(0x4e, '+', ConsoleKey.OemPlus);

            AddKeyWithShift(0x37, '*', ConsoleKey.Multiply);
            #endregion
        }

        private uint KeyCount = 0;

        private void AddKey(uint p, char p_2, ConsoleKey p_3) {
            mKeys.Add(new KeyMapping(p, p_2, p_3));
            KeyCount++;
        }
        private void AddKeyWithShift(uint p, char p_2, ConsoleKey p_3) {
            AddKey(p, p_2, p_3);
            AddKey(p << 16, p_2, p_3);
        }
        private void AddKey(uint p, ConsoleKey p_3) {
            AddKey(p, '\0', p_3);
        }
        private void AddKeyWithShift(uint p, ConsoleKey p_3) {
            AddKeyWithShift(p, '\0', p_3);
        }

        public void ChangeKeyMap(List<KeyMapping> aKeys) {
            mKeys = aKeys;
        }

        public bool GetCharValue(uint aScanCode, out char aValue) {
            for (int i = 0; i < mKeys.Count; i++) {
                //if (i == 0) {
                //  Console.Write("ScanCode in KeyMapping: ");
                //  Interrupts.WriteNumber(mKeys[i].Scancode, 32);
                //  Console.WriteLine("");
                //}
                if (mKeys[i].Scancode == aScanCode) {
                    if (mKeys[i].Value != '\0') {
                        aValue = mKeys[i].Value;
                        return true;
                    }
                    break;
                }
            }

            aValue = '\0';
            return false;
        }
        public bool GetKeyValue(uint aScanCode, out ConsoleKey aValue) {
            for (int i = 0; i < mKeys.Count; i++) {
                if (mKeys[i].Scancode == aScanCode) {
                    aValue = mKeys[i].Key;
                    return true;
                }
            }

            aValue = ConsoleKey.NoName;
            return false;
        }
        public bool GetKeyMapping(uint aScanCode, out KeyMapping aValue) {
            for (int i = 0; i < mKeys.Count; i++) {

                if (mKeys[i].Scancode == aScanCode) {
                    aValue = mKeys[i];
                    return true;
                }
            }

            aValue = null;
            return false;
        }

        public char ReadChar() {
            char xResult = '\0';
            while (mBuffer.Count == 0 || !GetCharValue(mBuffer.Dequeue(), out xResult)) {
                //Global.Sleep(10); //ToDo optimize value 
                if (Core.Global.CPU == null)
                {
                    return '\0';
                }
                Core.Global.CPU.Halt();
            }
            return xResult;
        }

        public bool GetChar(out char c) {
            c = '\0';

            if (mBuffer.Count > 0) {
                GetCharValue(mBuffer.Dequeue(), out c);
                return true;
            } else {
                return false;
            }
        }

        public ConsoleKey ReadKey() {
            ConsoleKey xResult = ConsoleKey.NoName;
            while (mBuffer.Count == 0 || !GetKeyValue(mBuffer.Dequeue(), out xResult)) {
                //Global.Sleep(10); //ToDo optimize value 
                Core.Global.CPU.Halt();
            }
            return xResult;
        }
        public bool GetKey(out ConsoleKey c) {
            c = ConsoleKey.NoName;

            if (mBuffer.Count > 0) {
                GetKeyValue(mBuffer.Dequeue(), out c);
                return true;
            } else {
                return false;
            }
        }

        public KeyMapping ReadMapping() {
            KeyMapping xResult = null;
            while (mBuffer.Count == 0 || !GetKeyMapping(mBuffer.Dequeue(), out xResult)) {
                //Global.Sleep(10); //ToDo optimize value 
                Core.Global.CPU.Halt();
            }
            return xResult;
        }
        public bool GetMapping(out KeyMapping c) {
            c = null;

            if (mBuffer.Count > 0) {
                GetKeyMapping(mBuffer.Dequeue(), out c);
                return true;
            } else {
                return false;
            }
        }

        public uint ReadScancode() {
            while (mBuffer.Count == 0) {
                Core.Global.CPU.Halt();
            }

            return mBuffer.Dequeue();
        }
        public bool GetScancode(out uint c) {
            if (mBuffer.Count > 0) {
                c = mBuffer.Dequeue();
                return true;
            } else {
                c = 0;
                return false;
            }
        }

        public class KeyMapping {
            public uint Scancode;
            public char Value;
            public ConsoleKey Key;

            public KeyMapping(uint aScanCode, char aValue, ConsoleKey aKey) {
                Scancode = aScanCode;
                Value = aValue;
                Key = aKey;
            }
            public KeyMapping(uint aScanCode, ConsoleKey aKey) {
                Scancode = aScanCode;
                Value = '\0';
                Key = aKey;
            }
        }
    }
}
