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
			Hardware.PIT.Initialize(Tick);
			Console.WriteLine("Done");
			//System.Diagnostics.Debugger.Break();
			Console.Write("Creating IDT...");
			Hardware.CPU.CreateIDT();
			Console.WriteLine("Done");
			Console.Write("Initializing Keyboard...");
			Keyboard.Initialize();
			Console.WriteLine("Done");
			Console.WriteLine("Initializing ATA");
			Hardware.Storage.ATA.Initialize(Sleep);
			Console.WriteLine("Done");
			byte[] xTempBytes = new byte[512];
			byte xCurrentValue = 0;
			for (int i = 0; i < 512; i++) {
				xTempBytes[i] = xCurrentValue;
				if (xCurrentValue == 255) {
					xCurrentValue = 0;
				} else {
					xCurrentValue++;
				}
			}
			Console.Write("Writing Diagnostics to Primary Master device...");
			if (Hardware.Storage.ATA.WriteBlock(0, 0, 1, xTempBytes)) {
				Console.WriteLine("Done");
			} else {
				Console.WriteLine("Failed");
			}
			//Console.Write("Flushing caches...");
			//if (Hardware.Storage.ATA.FlushCaches(0, 0)) {
			//    Console.WriteLine("Done");
			//} else {
			//    Console.WriteLine("Failed");
			//}
		}

		public static uint TickCount {
			get;
			private set;
		}
		private static void Tick(object aSender, EventArgs aEventArgs) {
			TickCount += 1;
		}

		public static void Sleep(uint aMSec) {
			uint xStart = TickCount;
			while (TickCount < (xStart + aMSec))
				;
		}
	}
}