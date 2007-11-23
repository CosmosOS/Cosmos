using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class Interrupts {

        //TODO: Remove
		public static void DoTest() {
			Hardware.Interrupts.IncludeAllHandlers();
			bool xTheval = false;
			if(xTheval) {
				Hardware.Interrupts.HandleInterrupt_Default(0, 0);
			}
		}
    }
}
