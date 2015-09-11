using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    // Non hardware class, only used by core and hardware drivers for ports etc.
    public class CPU {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        public static uint GetAmountOfRAM() { return 0; } // Plugged
        // needs to be static, as Heap needs it before we can instantiate objects
        public static uint GetEndOfKernel() { return 0; } // Plugged
        public void UpdateIDT(bool aEnableInterruptsImmediately) { } // Plugged
        public void InitFloat() { } // Plugged
        public static void ZeroFill(uint aStartAddress, uint aLength) { } // Plugged
        public void Halt() { } // Plugged

        public static void DisableInterrupts()
        {
            // plugged
        }

        public static void EnableInterrupts()
        {
            // plugged
        }

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
    }
}
