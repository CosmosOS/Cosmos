using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
    public class CPU {
        public static void Init() {
			Console.Write("Creating GDT...");
			Hardware.CPU.CreateGDT();
			Console.WriteLine("Done");
			Console.Write("Initializing PIC...");
			Hardware.PIC.Init();
			Console.WriteLine("Done");
			Console.Write("Initializing Serial 0...");
			Hardware.Serial.InitSerial(0);
			Console.WriteLine("Done");
			Console.Write("Initializing Debug Utility...");
			Hardware.DebugUtil.Initialize();
			//System.Diagnostics.Debugger.Break();
			Hardware.DebugUtil.SendMessage("Logging", "Initialized!");
			//System.Diagnostics.Debugger.Break();
			Console.WriteLine("Done");
			Console.Write("Configuring PIT...");
			Hardware.PIT.SetSlowest();
			Console.WriteLine("Done");
			//System.Diagnostics.Debugger.Break();
			Console.Write("Creating IDT...");
			Hardware.CPU.CreateIDT();
			Console.WriteLine("Done");
		}
    }
}
