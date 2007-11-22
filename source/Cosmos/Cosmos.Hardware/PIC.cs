using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public static class PIC {
		/// <summary>
		/// Remaps the IRQ's to INT20-INT2F
		/// </summary>
		public static void Remap() {
			CPU.WriteByteToPort(0x20, 0x11);
			CPU.WriteByteToPort(0xA0, 0x11);
			CPU.WriteByteToPort(0x21, 0x20);
			CPU.WriteByteToPort(0xA1, 0x28);
			CPU.WriteByteToPort(0x21, 0x04);
			CPU.WriteByteToPort(0xA1, 0x02);
			CPU.WriteByteToPort(0x21, 0x01);
			CPU.WriteByteToPort(0xA1, 0x01);
			CPU.WriteByteToPort(0x21, 0x0);
			CPU.WriteByteToPort(0xA1, 0x0);
		}
	}
}