using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.HAL {
    public abstract class Keyboard : Device {
        // TODO: MtW: I don't like the following line in the baseclass, but for now, lets keep it here.
        protected Core.IOGroup.Keyboard IO = Core.Global.BaseIOGroups.Keyboard;
        protected Keyboard()
        {
            if (mQueuedKeys != null)
            {
                Console.WriteLine("Skippign creation on key queue!");
            }
            mQueuedKeys = new Queue<ConsoleKeyInfo>(32);

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

        private static Queue<ConsoleKeyInfo> mQueuedKeys;

        protected void Enqueue(ConsoleKeyInfo aKey)
        {
            mQueuedKeys.Enqueue(aKey);
            Global.Dbg.SendNumber("Keyboard", "Key enqueued. QueuedKeys.Count", (uint)mQueuedKeys.Count, 32);
            global::System.Console.WriteLine("Key enqueued!");
            global::System.Console.Write("Key char: " );
            global::System.Console.Write(aKey.KeyChar);
            global::System.Console.WriteLine();
        }

        public bool TryReadKey(out ConsoleKeyInfo oKey)
        {
            if (mQueuedKeys.Count > 0)
            {
                oKey = mQueuedKeys.Dequeue();
                return true;
            }
            oKey = default(ConsoleKeyInfo);
            return false;
        }

        public ConsoleKeyInfo ReadKey()
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
    }
}
