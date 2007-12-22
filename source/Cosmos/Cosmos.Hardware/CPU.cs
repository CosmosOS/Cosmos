using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class CPU: Hardware {
		public static void CreateGDT() { }
		public static void CreateIDT() { }
		public static uint AmountOfMemory {
			get {
				return GetAmountOfRAM();
			}
		}
	}
}