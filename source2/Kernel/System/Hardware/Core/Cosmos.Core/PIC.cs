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
            Init = 0x10,
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
            // MTW: to disable PIT, send 0x01 to Master mask
            Init(Master, 0x20, 4, 0xFF);
            Init(Slave, 0x28, 2, 0xFF);
        }

        protected void Init(IOGroup.PIC aPIC, byte aBase, byte aIDunno, byte aMask){
            // We need to remap the PIC interrupt lines to the CPU. The BIOS sets
            // them in a way compatible for 16 bit mode, but in a way that causes problems
            // for 32 bit mode.
            // The only way to remap them however is to completely reinitialize the PICs.

            byte xOldMask = Master.Data.Byte;

            //#define ICW1_ICW4	0x01		/* ICW4 (not) needed */
            //#define ICW1_SINGLE	0x02		/* Single (cascade) mode */
            //#define ICW1_INTERVAL4	0x04		/* Call address interval 4 (8) */
            //#define ICW1_LEVEL	0x08		/* Level triggered (edge) mode */
            Master.Cmd.Byte = (byte)Cmd.Init | 0x01;
            IOPort.Wait();

            // ICW2
            Master.Data.Byte = aBase;
            IOPort.Wait();

            // ICW3
            // Somehow tells them about master/slave relationship
            Master.Data.Byte = aIDunno;
            IOPort.Wait();

            //#define ICW4_AUTO	0x02		/* Auto (normal) EOI */
            //#define ICW4_BUF_SLAVE	0x08		/* Buffered mode/slave */
            //#define ICW4_BUF_MASTER	0x0C		/* Buffered mode/master */
            //#define ICW4_SFNM	0x10		/* Special fully nested (not) */
            //0x01 8086/88 (MCS-80/85) mode
            Master.Data.Byte = 0x01;
            IOPort.Wait();

            // Set mask
            Master.Data.Byte = aMask;
            IOPort.Wait();
		}
    }
}
