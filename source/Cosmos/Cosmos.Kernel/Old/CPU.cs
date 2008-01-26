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
			Hardware.PIC.Init();
			Hardware.Serial.InitSerial(0);
			Hardware.DebugUtil.Initialize();
			Hardware.DebugUtil.SendMessage("Logging", "Initialized!");
			Hardware.PIT.Initialize(Tick);
			Kernel.Interrupts.DoTest();
			Hardware.Storage.ATA.Initialize(Sleep);
            Hardware.CPU.CreateIDT();
			Keyboard.Initialize();
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
			uint xStart = TickCount;
			uint xEnd = xStart + aMSec;
			while (TickCount < xEnd) {
				;
			}
		}
	}
}
