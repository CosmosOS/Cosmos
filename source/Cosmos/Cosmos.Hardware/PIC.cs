using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class PIC : Hardware {
		/// <summary>
		/// Remaps the IRQ's to INT20-INT2F
		/// </summary>
		public static void Init() {
            IOWrite(0x20, 0x11);
            IOWrite(0xA0, 0x11);
            IOWrite(0x21, 0x20);
            IOWrite(0xA1, 0x28);
            IOWrite(0x21, 0x04);
            IOWrite(0xA1, 0x02);
            IOWrite(0x21, 0x01);
            IOWrite(0xA1, 0x01);
            IOWrite(0x21, 0x0);
            IOWrite(0xA1, 0x0);
		}
	}
}