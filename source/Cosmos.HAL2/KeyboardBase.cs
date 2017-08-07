using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.HAL
{
    public abstract class KeyboardBase : Device
    {
        protected KeyboardBase()
        {
            Initialize();
            Global.mDebugger.Send("Initialized");
            UpdateLeds();
            Global.mDebugger.Send("Leds updated");
        }

        /// <summary>
        /// Initialize the device. Happens before the interrupt is registered, ie before the class is being used.
        /// </summary>
        protected abstract void Initialize();

        public abstract void UpdateLeds();

        public delegate void KeyPressedEventHandler(byte ScanCode, bool Released);
        public KeyPressedEventHandler OnKeyPressed;

        public static void WaitForKey()
        {
            Core.Global.CPU.Halt();
        }
    }
}
