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
    public unsafe partial class IOAPIC
    {
        public const byte IOREGSEL = 0x00;
        public const byte IOWIN = 0x10;

        //IO APIC Registers
        public const byte IOAPICID = 0x00;
        public const byte IOAPICVER = 0x01;
        public const byte IOAPICARB = 0x02;
        public const byte IOREDTBL = 0x10;

        /// <summary>
        /// IO APIC Base Address.
        /// </summary>
        private static uint Address = 0;

        /// <summary>
        /// Initialize local APIC.
        /// </summary>
        public static void Initialize()
        {
            if (ACPI.IOAPIC == null)
            {
                Global.mDebugger.Send("Can't initialize IO APIC");
                return;
            }

            Address = ACPI.IOAPIC->IOApicAddress;

            Global.mDebugger.Send("IO APIC address:0x" + Address.ToString("X"));

            uint x = In(IOAPICVER);
            uint count = ((x >> 16) & 0xFF) + 1;

            Global.mDebugger.Send("IO APIC pins:" + count);

            //Disable All Entries
            for (uint i = 0; i < count; ++i)
            {
                SetEntry((byte)i, 1 << 16);
            }

            Global.mDebugger.Send("IO APIC " + GetId() + " Initialized");
        }

        /// <summary>
        /// IO APIC MMIO Out.
        /// </summary>
        /// <param name="reg">IO APIC Register.</param>
        /// <param name="val">Data.</param>
        public static void Out(byte reg, uint val)
        {
            MMIOBase.Write32(Address + IOREGSEL, reg);
            MMIOBase.Write32(Address + IOWIN, val);
        }

        /// <summary>
        /// IO APIC MMIO In.
        /// </summary>
        /// <param name="reg">IO APIC Register.</param>
        public static uint In(byte reg)
        {
            MMIOBase.Write32(Address + IOREGSEL, reg);
            return MMIOBase.Read32(Address + IOWIN);
        }

        /// <summary>
        /// Set IO APIC Entry.
        /// </summary>
        /// <param name="index">Entry index.</param>
        /// <param name="data">Data.</param>
        public static void SetEntry(byte index, ulong data)
        {
            Out((byte)(IOREDTBL + index * 2), (uint)data);
            Out((byte)(IOREDTBL + index * 2 + 1), (uint)(data >> 32));
        }

        /// <summary>
        /// Set IO APIC Entry.
        /// </summary>
        /// <param name="data">Irq ID.</param>
        public static void SetEntry(uint irq)
        {
            SetEntry((byte)ACPI.RemapIRQ(irq), 0x20 + irq);
        }

        /// <summary>
        /// Get IO APIC ID.
        /// </summary>
        /// <returns>byte value.</returns>
        public static byte GetId()
        {
            return (byte)((In(IOAPICID) >> 24) & 0xF0);
        }
    }
}
