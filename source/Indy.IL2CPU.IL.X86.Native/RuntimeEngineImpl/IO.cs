using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	partial class RuntimeEngineImpl {

		private static void WriteToPort(ushort aPort, byte aData) {
			// implemented by OpCodeMap
		}
		private static byte ReadFromPort(ushort aPort) {
			// implemented by OpCodeMap
			return 0;
		}
	}
}
