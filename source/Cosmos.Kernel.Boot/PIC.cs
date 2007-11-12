using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Kernel.Boot {
	public static class PIC {
		public static void Initialize() {
			RemapIRQs();
		}

		/// <summary>
		/// This method remaps IRQ0-IRQ16 to ISR32-ISR47 (0x20 - 0x2F)
		/// </summary>
		/// <remarks>
		/// When changing the range to which it's mapped, also change NativeOpCodeMap!!
		/// </remarks>
		private static void RemapIRQs() {
			IO.WriteToPort(0x20, 0x11);
			IO.WriteToPort(0xA0, 0x11);
			IO.WriteToPort(0x21, 0x20);
			IO.WriteToPort(0xA1, 0x28);
			IO.WriteToPort(0x21, 0x04);
			IO.WriteToPort(0xA1, 0x02);
			IO.WriteToPort(0x21, 0x01);
			IO.WriteToPort(0xA1, 0x01);
			IO.WriteToPort(0x21, 0x0);
			IO.WriteToPort(0xA1, 0x0);
		}
	}
}