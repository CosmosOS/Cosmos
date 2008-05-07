using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Kernel {
	public class CPU {
        // Plugged
        public static void CreateIDT() { }

        /// <summary>
        /// Gets the amount of RAM in MB's.
        /// </summary>
        /// <returns></returns>
        // Plugged
        protected static uint GetAmountOfRAM() {
            return 0;
        }

        // Plugged
        protected static uint GetEndOfKernel() {
            return 0;
        }

        // Plugged
        public static void CreateGDT() { }

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

        // Plugged
        public static void ZeroFill(uint aStartAddress, uint aLength) {
		}

        // Plugged
        public static uint GetCurrentESP() {
			return 0;
		}

        // Plugged
        public static uint GetEndOfStack() {
			return 0;
		}

        // Plugged
		public static void DoTest() {
		}
	}
}