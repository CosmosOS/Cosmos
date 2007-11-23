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
            IOWrite(CmdPort1, 0x20);
        }

        public static void SignalSecondary() {
            IOWrite(CmdPort2, 0x20);
            IOWrite(CmdPort1, 0x20);
        }

        public static void Init() {
            // Init
            IOWrite(CmdPort1, 0x11);
            IOWrite(CmdPort2, 0x11);
            // Offsets
            IOWrite(DataPort1, 0x20);
            IOWrite(DataPort2, 0x28);
            // More Init
            IOWrite(DataPort1, 0x04);
            IOWrite(DataPort2, 0x02);
            // 8086 mode
            IOWrite(DataPort1, 0x01);
            IOWrite(DataPort2, 0x01);
            // Masks - 0 = receive all IRQ's
			// MTW, disabling PIT
            IOWrite(DataPort1, 0x01);
            IOWrite(DataPort2, 0x00);
		}
	}
}