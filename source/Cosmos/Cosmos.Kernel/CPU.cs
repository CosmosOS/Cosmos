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
        protected static uint GetAmountOfRAM() {
            return 0;
        }

        protected static uint GetEndOfKernel() {
            return 0;
        }
        
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