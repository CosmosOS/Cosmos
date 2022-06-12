using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.MemoryGroup;

namespace Cosmos.Core
{
    /// <summary>
    /// Local APIC class.
    /// </summary>
    public static unsafe class LocalAPIC
    {
        public const ushort LAPIC_ID = 0x0020;
        public const ushort LAPIC_VER = 0x0030;
        public const ushort LAPIC_TPR = 0x0080;
        public const ushort LAPIC_APR = 0x0090;
        public const ushort LAPIC_PPR = 0x00a0;
        public const ushort LAPIC_EOI = 0x00b0;
        public const ushort LAPIC_RRD = 0x00c0;
        public const ushort LAPIC_LDR = 0x00d0;
        public const ushort LAPIC_DFR = 0x00e0;
        public const ushort LAPIC_SVR = 0x00f0;
        public const ushort LAPIC_ISR = 0x0100;
        public const ushort LAPIC_TMR = 0x0180;
        public const ushort LAPIC_IRR = 0x0200;
        public const ushort LAPIC_ESR = 0x0280;
        public const ushort LAPIC_ICRLO = 0x0300;
        public const ushort LAPIC_ICRHI = 0x0310;
        public const ushort LAPIC_TIMER = 0x0320;
        public const ushort LAPIC_THERMAL = 0x0330;
        public const ushort LAPIC_PERF = 0x0340;
        public const ushort LAPIC_LINT0 = 0x0350;
        public const ushort LAPIC_LINT1 = 0x0360;
        public const ushort LAPIC_ERROR = 0x0370;
        public const ushort LAPIC_TICR = 0x0380;
        public const ushort LAPIC_TCCR = 0x0390;
        public const ushort LAPIC_TDCR = 0x03e0;

        public const ushort ICR_FIXED = 0x00000000;
        public const ushort ICR_LOWEST = 0x00000100;
        public const ushort ICR_SMI = 0x00000200;
        public const ushort ICR_NMI = 0x00000400;
        public const ushort ICR_INIT = 0x00000500;
        public const ushort ICR_STARTUP = 0x00000600;

        public const ushort ICR_PHYSICAL = 0x00000000;
        public const ushort ICR_LOGICAL = 0x00000800;

        public const ushort ICR_IDLE = 0x00000000;
        public const ushort ICR_SEND_PENDING = 0x00001000;

        public const ushort ICR_DEASSERT = 0x00000000;
        public const ushort ICR_ASSERT = 0x00004000;

        public const ushort ICR_EDGE = 0x00000000;
        public const ushort ICR_LEVEL = 0x00008000;

        public const int ICR_NO_SHORTHAND = 0x00000000;
        public const int ICR_SELF = 0x00040000;
        public const int ICR_ALL_INCLUDING_SELF = 0x00080000;
        public const int ICR_ALL_EXCLUDING_SELF = 0x000c0000;

        public const int ICR_DESTINATION_SHIFT = 24;

        /// <summary>
        /// Local APIC Base Address.
        /// </summary>
        private static uint Address = 0;

        /// <summary>
        /// Initialize local APIC.
        /// </summary>
        public static void Initialize()
        {
            if (ACPI.MADT == null)
            {
                Global.mDebugger.Send("Can't initialize Local APIC");
                return;
            }

            Global.PIC.Disable();

            Address = ACPI.MADT->LocalAPICAddress;

            Global.mDebugger.Send("Local APIC address:0x" + Address.ToString("X"));

            //Enable All Interrupts
            Out(LAPIC_TPR, 0);

            // Logical Destination Mode
            Out(LAPIC_DFR, 0xffffffff); // Flat mode
            Out(LAPIC_LDR, 0x01000000); // All cpus use logical id 1

            // Configure Spurious Interrupt Vector Register
            Out(LAPIC_SVR, 0x100 | 0xFF);

            Global.mDebugger.Send("Local APIC " + GetId() + " initialized");
        }

        /// <summary>
        /// IO APIC MMIO Out.
        /// </summary>
        /// <param name="reg">IO APIC Register.</param>
        /// <param name="val">Data.</param>
        public static void Out(uint reg, uint val)
        {
            MMIOBase.Write32(Address + reg, val);
        }

        /// <summary>
        /// IO APIC MMIO In.
        /// </summary>
        /// <param name="reg">IO APIC Register.</param>
        public static uint In(uint reg)
        {
            return MMIOBase.Read32(Address + reg);
        }

        /// <summary>
        /// End of Interrupt.
        /// </summary>
        public static void EndOfInterrupt()
        {
            Out(LAPIC_EOI, 0);
        }

        /// <summary>
        /// Get Local APIC ID.
        /// </summary>
        /// <returns>integer value.</returns>
        public static uint GetId()
        {
            return In(LAPIC_ID) >> 24;
        }
    }
}
