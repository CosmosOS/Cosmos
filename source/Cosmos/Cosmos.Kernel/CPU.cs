using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class CPU {
        public static void Init() {
			Hardware.CPU.CreateGDT();
			Hardware.PIC.Init();
        	Hardware.Serial.InitSerial(0);
        	Hardware.DebugUtil.Initialize();
        	Hardware.DebugUtil.SendMessage("Logging", "Initialized!");
        	Hardware.PIT.SetSlowest();
			Hardware.CPU.CreateIDT();
        }
    }
}
