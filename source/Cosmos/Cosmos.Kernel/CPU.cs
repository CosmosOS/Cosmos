using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class CPU {
		public static unsafe void Init() {
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
			Console.Write("Initializing ATA...");
			Hardware.Storage.ATA.Initialize(Sleep);
			Console.WriteLine("Done");
			byte[] xBytes = new byte[512];
			byte xValue = 0;
			for (int i = 0; i < 512; i++) {
				xBytes[i] = xValue;
				if (xValue == 255) {
					xValue = 0;
				} else {
					xValue += 1;
				}
			}
			// C-drive is at 0 - 0
			Console.WriteLine("Trying to read file...");
			if (FileSystem.Ext2.ReadFileContents(0, 0, new string[] { "test.txt" }) == null) {
				Console.WriteLine("Failed or not fully implemented!");
			} else {
				Console.WriteLine("Done");
			}
			//if (FileSystem.Ext2.ReadFileContents(1, 0, new string[] { "test.txt" }) == null) {
			//    Console.WriteLine("Failed or not fully implemented!");
			//} else {
			//    Console.WriteLine("Done");
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