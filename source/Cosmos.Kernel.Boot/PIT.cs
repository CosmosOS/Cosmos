using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.Boot {
	public static class PIT {
		public static void PIT_SetDivisor(ushort aDivisor) {
			IO.WriteToPort(0x43, 0x36);
			IO.WriteToPort(0x40, (byte)(aDivisor & 0xFF));
			IO.WriteToPort(0x40, (byte)(aDivisor >> 8));
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