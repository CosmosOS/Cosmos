using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Indy.IL2CPU.IL.X86.Native {
	partial class RuntimeEngineImpl {

		public static void PIT_SetDivisor(ushort aDivisor) {
			WriteToPort(0x43, 0x36);
			WriteToPort(0x40, (byte)(aDivisor & 0xFF));
			WriteToPort(0x40, (byte)(aDivisor >> 8));
		}

		public static void PIT_SetSlowest() {
			PIT_SetDivisor(UInt16.MaxValue);
		}

		public static void PIT_SetInterval(ushort hz) {
			ushort xDivisor = (ushort)(1193180 / hz);       /* Calculate our divisor */
			PIT_SetDivisor(xDivisor);
		}
	}
}
