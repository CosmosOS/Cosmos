using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL {
    public abstract class Keyboard : Device {
        protected Keyboard(ScanMapBase scanMap)
        {
            if (mQueuedKeys != null)
            {
                Debugger.DoSend("Skipping creation of key queue!");
            }
            mQueuedKeys = new Queue<KeyEvent>();
            Debugger.DoSend("mQueuedKeys created");
            SetKeyLayout(scanMap);
            Debugger.DoSend("Keylayout set");
            Initialize();
            Debugger.DoSend("Initialized");
            UpdateLeds();
            Debugger.DoSend("Leds updated");
        }

        /// <summary>
        /// Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
        /// </summary>
        protected abstract void Initialize();

        public ScanMapBase KeyLayout { get; private set; }

        public virtual void SetKeyLayout(ScanMapBase layout)
        {
            KeyLayout = layout;
        }

        /// <summary>
        /// Allow faking scancodes. Used for test kernels
        /// </summary>
        internal void HandleFakeScanCode(byte aScancode, bool aReleased)
        {
            HandleScancode(aScancode, aReleased);
        }

        public abstract void UpdateLeds();

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
    }
}
