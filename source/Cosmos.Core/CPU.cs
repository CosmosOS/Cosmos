using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.IL2CPU.Plugs;
namespace Cosmos.Core {
    // Non hardware class, only used by core and hardware drivers for ports etc.
    public class CPU {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        [PlugMethod(PlugRequired = true)]
        public static uint GetAmountOfRAM() { return 0; } // Plugged
        // needs to be static, as Heap needs it before we can instantiate objects
        [PlugMethod(PlugRequired = true)]
        public static uint GetEndOfKernel() { return 0; } // Plugged
        [PlugMethod(PlugRequired = true)]
        public void UpdateIDT(bool aEnableInterruptsImmediately) { } // Plugged
        [PlugMethod(PlugRequired = true)]
        public void InitFloat() { } // Plugged
        [PlugMethod(PlugRequired = true)]
        public void InitSSE() { } // Plugged
        [PlugMethod(PlugRequired = true)]
        public static void ZeroFill(uint aStartAddress, uint aLength) { } // Plugged
        [PlugMethod(PlugRequired = true)]
        public void Halt() { } // Plugged
        public void Reboot() {
            // Disable all interrupts
            DisableInterrupts();

            var myPort = new IOPort(0x64);
            while ((myPort.Byte & 0x02) != 0)
            {
            }
            myPort.Byte = 0xFE;
            Halt(); // If it didn't work, Halt the CPU
        }

        private static void DoEnableInterrupts()
        {

        }

        private static void DoDisableInterrupts()
        {
        }

        public static bool mInterruptsEnabled;
        public static void EnableInterrupts()
        {
            mInterruptsEnabled = true;
            DoEnableInterrupts();
        }

        /// <summary>
        /// Returns if the interrupts were actually enabled
        /// </summary>
        /// <returns></returns>
        public static bool DisableInterrupts()
        {
            DoDisableInterrupts();
            var xResult = mInterruptsEnabled;
            mInterruptsEnabled = false;
            return xResult;
        }
    }
}
