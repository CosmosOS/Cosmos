using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
    public class Hardware {
        //Dont want non hardware members to be able to call this - but need to expose it for future hardware..... 
        protected static void IOWriteByte(ushort aPort, byte aData) { }
        protected static byte IOReadByte(ushort aPort) { return 0; }
		protected static void IOWriteWord(ushort aPort, ushort aData) {
		}
		protected static ushort IOReadWord(ushort aPort) {
			return 0;
		}
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
    }
}
