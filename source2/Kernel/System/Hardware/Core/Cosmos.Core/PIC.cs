using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cosmos.Core {
    // PIC is not in hardware becuase its a special core piece like CPU that is not interacted with by anything except Core.
    //
    // Remaps the IRQ's to INT20-INT2F
    public class PIC {
        // This is here and not in BaseGroups for 2 reasons.
        // 1) Its needed before the other Basegroups are created.
        // 2) Its only used by this class, and it also exists in Core.
        protected IOGroup.PIC Master = new IOGroup.PIC(false);
        protected IOGroup.PIC Slave = new IOGroup.PIC(true);

        protected enum Cmd {
            Init = 0x11,
            EOI = 0x20
        }

        public void EoiMaster() {
            Master.Cmd.Byte = (byte)Cmd.EOI;
        }

        public void EoiSlave() {
            Master.Cmd.Byte = (byte)Cmd.EOI;
            Slave.Cmd.Byte = (byte)Cmd.EOI;
        }

        public PIC() {
            Init(Master, 0x20, 4);
            Init(Slave, 0x28, 2);
        }

        protected void Init(IOGroup.PIC aPIC, byte aBase, byte aIDunno){
            // We need to remap the PIC interrupt lines to the CPU. The BIOS sets
            // them in a way compatible for 16 bit mode, but in a way that causes problems
            // for 32 bit mode.
            // The only way to remap them however is to completely reinitialize the PICs.

            //#define ICW1_ICW4	0x01		/* ICW4 (not) needed */
            //#define ICW1_SINGLE	0x02		/* Single (cascade) mode */
            //#define ICW1_INTERVAL4	0x04		/* Call address interval 4 (8) */
            //#define ICW1_LEVEL	0x08		/* Level triggered (edge) mode */
            //#define ICW1_INIT	0x10		/* Initialization - required! */

            //#define ICW4_8086	0x01		/* 8086/88 (MCS-80/85) mode */
            //#define ICW4_AUTO	0x02		/* Auto (normal) EOI */
            //#define ICW4_BUF_SLAVE	0x08		/* Buffered mode/slave */
            //#define ICW4_BUF_MASTER	0x0C		/* Buffered mode/master */
            //#define ICW4_SFNM	0x10		/* Special fully nested (not) */

            byte xOldMask = Master.Data.Byte;
            
            //    outb(PIC1_COMMAND, ICW1_INIT+ICW1_ICW4);  // starts the initialization sequence
            Master.Cmd.Byte = (byte)Cmd.Init | 0x01;
            IOPort.Wait();

            Master.Data.Byte = aBase;
            IOPort.Wait();

            Master.Data.Byte = aIDunno;
            IOPort.Wait();

            // 8086/88 (MCS-80/85) mode
            Master.Data.Byte = 0x01;
            IOPort.Wait();

            //// Masks - 0 = receive all IRQ's
            //// MTW: to disable PIT, send 0x01 to DataPort1
            //IO.PortData1.Byte = 0x01;
            //IO.PortData2.Byte = 0x00;
            // Restore saved masks.
            Master.Data.Byte = xOldMask;
            IOPort.Wait();
		}
    }
}
