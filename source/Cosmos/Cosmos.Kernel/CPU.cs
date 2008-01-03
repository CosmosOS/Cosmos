using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace Cosmos.Kernel {
	public class CPU {
		private static void WriteBool(bool aValue) {
			if (aValue) {
				Console.Write("true");
			} else {
				Console.Write("false");
			}
		}
		public static unsafe void Init() {
			Heap.CheckInit();
			//Console.Write("Creating GDT...");
			//Hardware.CPU.CreateGDT();
			//Console.WriteLine("Done");
			//Console.Write("Initializing PIC...");
			//Hardware.PIC.Init();
			//Console.WriteLine("Done");
			//Console.Write("Initializing Serial 0...");
			//Hardware.Serial.InitSerial(0);
			//Console.WriteLine("Done");
			//Console.Write("Initializing Debug Utility...");
			//Hardware.DebugUtil.Initialize();
			//Hardware.DebugUtil.SendMessage("Logging", "Initialized!");
			//Console.WriteLine("Done");
			//Console.Write("Configuring PIT...");
			//Hardware.PIT.Initialize(Tick);
			//Console.WriteLine("Done");
			//Console.Write("Creating IDT...");
			//Kernel.Interrupts.DoTest();
			//Hardware.CPU.CreateIDT();
			//Console.WriteLine("Done");
			//Keyboard.Initialize();
			//TestATA();
		}

		public static void PrintTime() {
			Console.Write("Time: ");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetHours(), 8);
			Console.Write(":");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetMinutes(), 8);
			Console.Write(":");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetSeconds(), 8);
			Console.WriteLine("");
		}

		public static uint TickCount {
			get;
			private set;
		}
		private static void Tick(object aSender, EventArgs aEventArgs) {
			TickCount += 1;
		}

		public static void Sleep(uint aMSec) {
			uint xEnd = TickCount + aMSec;
			while (TickCount < xEnd)
				;
		}


	}
}
