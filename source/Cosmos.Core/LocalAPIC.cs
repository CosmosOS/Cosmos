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
        /// <summary>
        /// Local APIC IO group.
        /// </summary>
        internal static APICMMIOGroup IO;

        /// <summary>
        /// Initialize local APIC.
        /// </summary>
        public static void Initialize()
        {
            Global.PIC.Disable();

            IO = new APICMMIOGroup(ACPI.MADT->LocalAPICAddress);

            //Enable All Interrupts
            IO.Tpr.DWord = 0;

            // Logical Destination Mode
            IO.Tpr.DWord = 0xffffffff; // Flat mode
            IO.Ldr.DWord = 0 << 24; // All cpus use logical id 0

            // Configure Spurious Interrupt Vector Register
            IO.Svr.DWord = 0x100 | 0xff;

            Console.WriteLine("Local APIC initialized");
        }

        /// <summary>
        /// End of Interrupt.
        /// </summary>
        public static void EndOfInterrupt()
        {
            IO.EndOfInterrupts.DWord = 0;
        }

        /// <summary>
        /// Get Local APIC ID.
        /// </summary>
        /// <returns>integer value.</returns>
        public static uint GetId()
        {
            return IO.Id.DWord >> 24;
        }
    }
}
