using Cosmos.Core;

namespace Cosmos.Core
{
    // PIC is not in hardware becuase its a special core piece like CPU that is not interacted with by anything except Core.
    //
    // Remaps the IRQ's to INT20-INT2F
    /// <summary>
    /// PIC class. Used to manage PIC interrupts.
    /// </summary>
    public class PIC
    {
        // This is here and not in BaseGroups for 2 reasons.
        // 1) Its needed before the other Basegroups are created.
        // 2) Its only used by this class, and it also exists in Core.
        /// <summary>
        /// Master PIC.
        /// </summary>
        protected IOGroup.PIC Master = new IOGroup.PIC(false);
        /// <summary>
        /// Slave PIC.
        /// </summary>
        protected IOGroup.PIC Slave = new IOGroup.PIC(true);

        /// <summary>
        /// Commands.
        /// </summary>
        protected enum Cmd
        {
            /// <summary>
            /// Initialize.
            /// </summary>
            Init = 0x10,
            /// <summary>
            /// End of interrupt.
            /// </summary>
            EOI = 0x20
        }

        /*
        /// <summary>
        /// Master end of interrupt.
        /// </summary>
        public void EoiMaster()
        {
            Master.Cmd.Byte = (byte) Cmd.EOI;
        }

        /// <summary>
        /// Slave end of interrupt.
        /// </summary>
        public void EoiSlave()
        {
            Master.Cmd.Byte = (byte) Cmd.EOI;
            Slave.Cmd.Byte = (byte) Cmd.EOI;
        }*/

        /// <summary>
        /// Create new instance of the <see cref="PIC"/> class.
        /// </summary>
        public PIC()
        {
            // MTW: to disable PIT, send 0x01 to Master mask
            // Right now we mask all IRQs. We enable them as we add
            // support for them. The 0x08 on master MUST remain. IRQ7
            // has a problem of "spurious requests" and is therefore unreliable.
            // It used for LPT2 (and sometimes old 8 bit sound blasters). We likely
            // won't need either of those for a very long time, so we just mask it
            // completely. For more info:
            // http://en.wikipedia.org/wiki/Intel_8259#Spurious_Interrupts

            //Init(Master, 0x20, 4, 0xFD | 0x08);
            //Init(Slave, 0x28, 2, 0xFF);
            //for now enable keyboard, mouse(ps2)
            //Remap(0x20, 0xF8 | 0x08, 0x28, 0xEB);
        }

        /// <summary>
        /// Remap master and slave relationship.
        /// </summary>
        /// <param name="masterStart">Master start.</param>
        /// <param name="masterMask">Master mask.</param>
        /// <param name="slaveStart">Slave start.</param>
        /// <param name="slaveMask">Slave mask.</param>
        private void Remap(byte masterStart, byte masterMask, byte slaveStart, byte slaveMask)
        {
            #region consts

            // source: osdev.org

#pragma warning disable
            const byte ICW1_ICW4 = 0x01; // ICW4 (not) needed
            const byte ICW1_SINGLE = 0x02; // Single (cascade) mode
            const byte ICW1_INTERVAL4 = 0x04; // Call address interval 4 (8)
            const byte ICW1_LEVEL = 0x08; // Level triggered (edge) mode
            const byte ICW1_INIT = 0x10;

            const byte ICW4_8086 = 0x01; // 8086/88 mode
            const byte ICW4_AUTO = 0x02; // auto (normal) EOI
            const byte ICW4_BUF_SLAVE = 0x08; // buffered mode/slave
            const byte ICW4_BUF_MASTER = 0x0C; // buffered mode/master
            const byte ICW4_SFNM = 0x10; // special fully nested (not)
#pragma warning restore

            #endregion

            var xOldMasterMask = Master.Data.Byte;
            var xOldSlaveMask = Slave.Data.Byte;
            Master.Cmd.Byte = ICW1_INIT + ICW1_ICW4;
            IOPort.Wait();
            Slave.Cmd.Byte = ICW1_INIT + ICW1_ICW4;
            IOPort.Wait();
            Master.Data.Byte = masterStart;
            IOPort.Wait();
            Slave.Data.Byte = slaveStart;
            IOPort.Wait();

            // magic:
            Master.Data.Byte = 4;
            IOPort.Wait();
            Slave.Data.Byte = 2;
            IOPort.Wait();

            // set modes:
            Master.Data.Byte = ICW4_8086;
            IOPort.Wait();
            Slave.Data.Byte = ICW4_8086;
            IOPort.Wait();

            // set masks:
            Master.Data.Byte = masterMask;
            IOPort.Wait();
            //Slave.Data.Byte = slaveMask;
            //IOPort.Wait();
        }

        /// <summary>
        /// Disable PIC.
        /// </summary>
        public void Disable()
        {
            //disable
            Master.Data.Byte = 0xFF;
            IOPort.Wait();
            Slave.Data.Byte = 0xFF;
            IOPort.Wait();
        }
    }
}
