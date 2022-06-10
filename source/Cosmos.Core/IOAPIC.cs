using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core.IOGroup;

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

        public static uint In(byte reg)
        {
            IO.RegSel.Byte = reg;
            return IO.Win.DWord;
        }

        public static void Out(byte reg, uint value)
        {
            IO.RegSel.Byte = reg;
            IO.Win.DWord = value;
        }

        public static void SetEntry(byte index, ulong data)
        {
            Out((byte)(IOAPICIOGroup.IOREDTBL + index * 2), (uint)data);
            Out((byte)(IOAPICIOGroup.IOREDTBL + index * 2 + 1), (uint)(data >> 32));
        }

        public static void SetEntry(uint irq)
        {
            IOAPIC.SetEntry((byte)ACPI.RemapIRQ(irq - 0x20), irq);
        }
    }
}
