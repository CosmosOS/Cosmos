using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL {
    public class ConsoleKeyInfoEx
    {
        // once Github issue #137 is fixed, replace this class with ConsoleKeyInfo struct.

        public char KeyChar
        {
            get;
            set;
        }

        public ConsoleKey Key
        {
            get;
            set;
        }

        public ConsoleModifiers Modifiers
        {
            get;
            set;
        }

        public ConsoleKeyInfoEx(char keyChar, ConsoleKey key, bool shift, bool alt, bool control)
        {
            this.KeyChar = keyChar;
            this.Key = key;
            this.Modifiers = (ConsoleModifiers)0;
            if (shift)
            {
                this.Modifiers |= ConsoleModifiers.Shift;
            }
            if (alt)
            {
                this.Modifiers |= ConsoleModifiers.Alt;
            }
            if (control)
            {
                this.Modifiers |= ConsoleModifiers.Control;
            }
        }
    }

    public abstract class Keyboard : Device {
        // TODO: MtW: I don't like the following line in the baseclass, but for now, lets keep it here.
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;
        protected Keyboard()
        {
            if (mQueuedKeys != null)
            {
                Console.WriteLine("Skipping creation of key queue!");
            }
            mQueuedKeys = new Queue<ConsoleKeyInfoEx>(32);

            Initialize();
            Core.INTs.SetIrqHandler(0x01, HandleIRQ);
        }

        /// <summary>
        /// Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
        /// </summary>
        protected abstract void Initialize();

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

        protected abstract void HandleScancode(byte aScancode, bool aReleased);

        private static Queue<ConsoleKeyInfoEx> mQueuedKeys;

        protected void Enqueue(ConsoleKeyInfoEx aKey)
        {
            mQueuedKeys.Enqueue(aKey);
        }

        public bool TryReadKey(out ConsoleKeyInfoEx oKey)
        {
            if (mQueuedKeys.Count > 0)
            {
                oKey = mQueuedKeys.Dequeue();
                return true;
            }
            oKey = default(ConsoleKeyInfoEx);
            return false;
        }

        public ConsoleKeyInfoEx ReadKey()
        {
            while (mQueuedKeys.Count == 0)
            {
                Core.Global.CPU.Halt();
            }
            return mQueuedKeys.Dequeue();
        }

        public bool ShiftPressed
        {
            get;
            protected set;
        }

        public bool ControlPressed
        {
            get;
            protected set;
        }

        public bool AltPressed
        {
            get;
            protected set;
        }

        public bool NumLock { get; protected set; }

        public bool CapsLock { get; protected set; }

        public bool ScrollLock { get; protected set; }
    }
}