using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class PIT: Hardware {
		public const int TicksPerSecond = 1000;
		public static void SetDivisor(ushort aDivisor) {
			IOWriteByte(0x43, 0x36);
			IOWriteByte(0x40, (byte)(aDivisor & 0xFF));
			IOWriteByte(0x40, (byte)(aDivisor >> 8));
		}

		private static void SetInterval(ushort hz) {
			ushort xDivisor = (ushort)(1193180 / hz);       /* Calculate our divisor */
			SetDivisor(xDivisor);
		}

		public static void Initialize(EventHandler aTick) {
			mTick = aTick;
			SetInterval(1);
			//SetInterval(1193);
		}

		private static EventHandler mTick;

		internal static void HandleInterrupt() {
			mTick(null, null);
		}
	}
}