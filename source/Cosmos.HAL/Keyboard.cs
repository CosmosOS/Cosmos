using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.HAL.ScanMaps;

namespace Cosmos.HAL {
    public abstract class Keyboard : Device {
        protected Keyboard()
        {
            if (mQueuedKeys != null)
            {
                Console.WriteLine("Skipping creation of key queue!");
            }
            mQueuedKeys = new Queue<KeyEvent>(32);     
            SetKeyLayout(new US_Standard());   
        }

        /// <summary>
        /// Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
        /// </summary>
        protected abstract void Initialize();

        public ScanMapBase KeyLayout { get; private set; }

        public void SetKeyLayout(ScanMapBase layout)
        {
            KeyLayout = layout;
        }

        protected abstract void HandleScancode(byte aScancode, bool aReleased);

        private static Queue<KeyEvent> mQueuedKeys;

        protected void Enqueue(KeyEvent aKey)
        {
            mQueuedKeys.Enqueue(aKey);
        }

        public bool TryReadKey(out KeyEvent oKey)
        {
            if (mQueuedKeys.Count > 0)
            {
                oKey = mQueuedKeys.Dequeue();
                return true;
            }
            oKey = default(KeyEvent);
            return false;
        }

        public KeyEvent ReadKey()
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
