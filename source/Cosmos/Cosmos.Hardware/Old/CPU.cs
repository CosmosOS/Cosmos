using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class CPUOld: Hardware {
        // Plugged
		public static void CreateGDT() { }
        // Plugged
        public static void CreateIDT() { }

		public static uint AmountOfMemory {
			get {
				return GetAmountOfRAM();
			}
		}

		public static uint EndOfKernel {
			get {
				return GetEndOfKernel();
			}
		}

		public static void ZeroFill(uint aStartAddress, uint aLength) {
		}
	}
}