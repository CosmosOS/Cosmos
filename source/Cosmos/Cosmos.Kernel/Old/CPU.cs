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

		public static void PrintTime() {
			Console.Write("Time: ");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetHours(), 8);
			Console.Write(":");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetMinutes(), 8);
			Console.Write(":");
			Hardware.Storage.ATAOld.WriteNumber(Hardware.RTC.GetSeconds(), 8);
			Console.WriteLine("");
		}


	}
}
