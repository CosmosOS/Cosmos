using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Hardware2 {
    //TODO: Change this to be an instance like other drivers
    public abstract class PIC : Cosmos.Hardware2.Device {
		/// <summary>
		/// Remaps the IRQ's to INT20-INT2F
		/// </summary>
        const ushort CmdPort1 = 0x20;
        const ushort DataPort1 = 0x21;

        const ushort CmdPort2 = 0xA0;
        const ushort DataPort2 = 0xA1;

        public static void SignalPrimary() {
			Kernel.CPUBus.Write8(CmdPort1, 0x20);
        }

        public static void SignalSecondary() {
            Kernel.CPUBus.Write8(CmdPort2, 0x20);
            Kernel.CPUBus.Write8(CmdPort1, 0x20);
        }

        public static void Init() {
            // Init
            Kernel.CPUBus.Write8(CmdPort1, 0x11);
            Kernel.CPUBus.Write8(CmdPort2, 0x11);
            // Offsets
            Kernel.CPUBus.Write8(DataPort1, 0x20);
            Kernel.CPUBus.Write8(DataPort2, 0x28);
            // More Init
            Kernel.CPUBus.Write8(DataPort1, 0x04);
            Kernel.CPUBus.Write8(DataPort2, 0x02);
            // 8086 mode
            Kernel.CPUBus.Write8(DataPort1, 0x01);
            Kernel.CPUBus.Write8(DataPort2, 0x01);
            // Masks - 0 = receive all IRQ's
			// MTW, to disable PIT, send 0x1 to DataPort1
            Kernel.CPUBus.Write8(DataPort1, 0x01);
            Kernel.CPUBus.Write8(DataPort2, 0x00);
		}
    }
}
