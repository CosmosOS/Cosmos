using System;
using System.Collections.Generic;
using System.Text;

namespace Cosmos.Hardware {
	public class PIC : Hardware {
		/// <summary>
		/// Remaps the IRQ's to INT20-INT2F
		/// </summary>
        const ushort CmdPort1 = 0x20;
        const ushort DataPort1 = 0x21;

        const ushort CmdPort2 = 0xA0;
        const ushort DataPort2 = 0xA1;

        public static void SignalPrimary() {
			IOWriteByte(CmdPort1, 0x20);
        }

        public static void SignalSecondary() {
			IOWriteByte(CmdPort2, 0x20);
			IOWriteByte(CmdPort1, 0x20);
        }

        public static void Init() {
            // Init
			IOWriteByte(CmdPort1, 0x11);
			IOWriteByte(CmdPort2, 0x11);
            // Offsets
			IOWriteByte(DataPort1, 0x20);
			IOWriteByte(DataPort2, 0x28);
            // More Init
			IOWriteByte(DataPort1, 0x04);
			IOWriteByte(DataPort2, 0x02);
            // 8086 mode
			IOWriteByte(DataPort1, 0x01);
			IOWriteByte(DataPort2, 0x01);
            // Masks - 0 = receive all IRQ's
			// MTW, to disable PIT, send 0x1 to DataPort1
			IOWriteByte(DataPort1, 0x00); 
			IOWriteByte(DataPort2, 0x00);
		}
	}
}