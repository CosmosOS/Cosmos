using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Kernel.Boot.Glue;

namespace Cosmos.Kernel.Boot {
	public static class DebugUtil {
		private static bool mInitialized;
		private static void CheckInitialized() {
			if(!mInitialized) {
				mInitialized = true;
				Console.Write("Initializing COM1...");
				IO.InitIO();
				IO.InitSerial(0);
				IO.WriteSerialString(0, "Comport initialized!\r\n");
			}
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.Debug_Write)]
		public static void Write(string aData) {
			CheckInitialized();
			IO.WriteSerialString(0, aData);
		}

		[GlueMethod(MethodType = GlueMethodTypeEnum.Debug_WriteLine)]
		public static void WriteLine(string aData) {
			Write(aData);
			Write("\r\n");
		}
	}
}
