using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Debug.Kernel;

namespace Cosmos.Core
{
    // Non hardware class, only used by core and hardware drivers for ports etc.
    public class CPU
    {
        // Amount of RAM in MB's.
        // needs to be static, as Heap needs it before we can instantiate objects
        public static uint GetAmountOfRAM()
        {
            return 0;
        } // Plugged
        // needs to be static, as Heap needs it before we can instantiate objects
        public static uint GetEndOfKernel()
        {
            return 0;
        } // Plugged

        public void UpdateIDT(bool aEnableInterruptsImmediately)
        {
        } // Plugged

        public void InitFloat()
        {
        } // Plugged

        public static void ZeroFill(uint aStartAddress, uint aLength)
        {
        } // Plugged

        internal static void DoHalt()
        {
        } // Plugged

        public void Halt()
        {
            DoHalt();
        }

        public void Reboot()
        {
            // Disable all interrupts
            //DisableInterrupts();

            //byte temp;

            //// Clear all keyboard buffers
            //do {
            //    temp = CPUBus.Read8(0x64); // Empty user data
            //    if ((temp & 0x01) != 0) {
            //        CPUBus.Read8(0x60); // Empty keyboard data
            //    }
            //} while ((temp & 0x02) != 0);

            //CPUBus.Write8(0x64, 0xFE); // Pulse CPU Reset line
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
            Debugger.DoSend("Enabling interrupts");
            mInterruptsEnabled = true;
            DoEnableInterrupts();
        }

        public static void DisableInterrupts()
        {
            mInterruptsEnabled = false;
            DoDisableInterrupts();
            Debugger.DoSend("Disable interrupts");
        }
    }
}
