using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class CPU: Hardware {
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

		public static uint GetCurrentESP() {
			return 0;
		}

		public static uint GetEndOfStack() {
			return 0;
		}

		public static void DoTest() {
		}
	}
}