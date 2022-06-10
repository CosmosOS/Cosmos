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
    public unsafe partial class LocalAPIC
    {
        /// <summary>
        /// Local APIC IO group.
        /// </summary>
        protected APICIOGroup IO;

        /// <summary>
        /// End of Interrupt.
        /// </summary>
        public void EndOfInterrupt()
        {
            IO.EndOfInterrupts.DWord = 0;
        }

        /// <summary>
        /// Initialize local APIC.
        /// </summary>
        public void Initialize()
        {
            //PIC.Disable(); TODO

            IO = new APICIOGroup((ushort)ACPI.MADT->LocalAPICAddress);

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
        /// Get Local APIC ID.
        /// </summary>
        /// <returns>integer value.</returns>
        public uint GetId()
        {
            return IO.Id.DWord >> 24;
        }

        /// <summary>
        /// Send Init Command to Local APIC.
        /// </summary>
        /// <param name="apic_id">APIC ID.</param>
        public void SendInit(uint apic_id)
        {
            IO.ICRHI.DWord = apic_id << IO.ICR_DESTINATION_SHIFT;
            IO.ICRLO.DWord = (uint)(IO.ICR_INIT | IO.ICR_PHYSICAL | IO.ICR_ASSERT | IO.ICR_EDGE | IO.ICR_NO_SHORTHAND);

            while ((IO.ICRLO.DWord & IO.ICR_SEND_PENDING) != 0) { }
        }


        /// <summary>
        /// Send Startup Command to Local APIC.
        /// </summary>
        /// <param name="apic_id">APIC ID.</param>
        /// <param name="vector">Vector.</param>
        public void SendStartup(uint apic_id, ushort vector)
        {
            IO.ICRHI.DWord = apic_id << IO.ICR_DESTINATION_SHIFT;
            IO.ICRLO.DWord = (uint)(vector | IO.ICR_STARTUP | IO.ICR_PHYSICAL | IO.ICR_ASSERT | IO.ICR_EDGE | IO.ICR_NO_SHORTHAND);

            while ((IO.ICRLO.DWord & IO.ICR_SEND_PENDING) != 0) { }
        }
    }
}
