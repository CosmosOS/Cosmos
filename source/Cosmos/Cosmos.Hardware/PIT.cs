using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class PIT: Hardware {
		public static void SetDivisor(ushort aDivisor) {
			IOWrite(0x43, 0x36);
			IOWrite(0x40, (byte)(aDivisor & 0xFF));
			IOWrite(0x40, (byte)(aDivisor >> 8));
		}

		public static void SetSlowest() {
			SetDivisor(UInt16.MaxValue);
		}

		public static void SetInterval(ushort hz) {
			ushort xDivisor = (ushort)(1193180 / hz);       /* Calculate our divisor */
			SetDivisor(xDivisor);
		}

	}
}