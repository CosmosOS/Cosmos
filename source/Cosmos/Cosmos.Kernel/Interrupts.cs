using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class Interrupts {
        // Interrupt 15, AH=0
        //[X86.Int(0x15, 0x00)]
        public void ReadChar() {
        }

		public static void DoTest() {
			Hardware.CPU.EnableSimpleGDT();
			Hardware.PIC.Remap();
			Hardware.CPU.SetupAndEnableIDT();
			bool xTheval = false;
			if(xTheval) {
				Hardware.Interrupts.HandleInterrupt_Default(0, 0);
			}
		}
    }
}
