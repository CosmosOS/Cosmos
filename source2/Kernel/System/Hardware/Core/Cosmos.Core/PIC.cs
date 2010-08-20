using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    // PIC is not in hardware becuase its a special core piece like CPU that is not interacted with by anything except Core.
    //
    // Remaps the IRQ's to INT20-INT2F
    public class PIC {
        protected IOGroup.PIC IO = Global.BaseIOGroups.PIC;

        public void SignalPrimary() {
            IO.PortCmd1.Byte = 0x20;
        }

        public void SignalSecondary() {
            IO.PortCmd2.Byte = 0x20;
            IO.PortCmd1.Byte = 0x20;
        }

        public PIC() {
            // Init
            IO.PortCmd1.Byte = 0x11;
            IO.PortCmd2.Byte = 0x11;
            // Offsets
            IO.PortData1.Byte = 0x20;
            IO.PortData2.Byte = 0x28;
            // More Init
            IO.PortData1.Byte = 0x04;
            IO.PortData2.Byte = 0x02;
            // 8086 mode
            IO.PortData1.Byte = 0x01;
            IO.PortData2.Byte = 0x01;
            // Masks - 0 = receive all IRQ's
			// MTW, to disable PIT, send 0x1 to DataPort1
            IO.PortData1.Byte = 0x01;
            IO.PortData2.Byte = 0x00;
		}
    }
}
