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
            mQueuedKeys = new Queue<ConsoleKeyInfo>();

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

        private readonly Queue<ConsoleKeyInfo> mQueuedKeys;

        protected void Enqueue(ConsoleKeyInfo aKey)
        {
            mQueuedKeys.Enqueue(aKey);
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
