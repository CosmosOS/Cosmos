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
        /// <summary>
        /// IO APIC IO group.
        /// </summary>
        internal static IOAPICIOGroup IO;

        /// <summary>
        /// Initialize local APIC.
        /// </summary>
        public static void Initialize()
        {
            IO = new IOAPICIOGroup(ACPI.IOAPIC->IOApicAddress);

            if (ACPI.IOAPIC == null)
            {
                Console.WriteLine("Can't initialize IO APIC");

                return;
            }
            uint value = IO.Ver.DWord;
            uint count = ((value >> 16) & 0xFF) + 1;

            //Disable All Entries
            for (uint i = 0; i < count; ++i)
            {
                SetEntry((byte)i, 1 << 16);
            }
            Console.WriteLine("IO APIC Initialized");
        }

        public static void SetEntry(byte index, ulong data)
        {
            var mmio = new MMIO((uint)(IOAPICIOGroup.IOREDTBL + index * 2));
            mmio.DWord = (uint)data;
            var mmio2 = new MMIO((uint)(IOAPICIOGroup.IOREDTBL + index * 2 + 1));
            mmio2.DWord = (uint)(data >> 32);
        }

        public static void SetEntry(uint irq)
        {
            SetEntry((byte)ACPI.RemapIRQ(irq - 0x20), irq);
        }
    }
}
