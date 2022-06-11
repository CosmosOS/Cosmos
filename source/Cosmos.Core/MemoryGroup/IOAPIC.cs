namespace Cosmos.Core.MemoryGroup
{
    /// <summary>
    /// IO APIC IOGroup class.
    /// </summary>
    public class IOAPICMMIOGroup
    {
        public const ushort IOREGSEL = 0x00;
        public const ushort IOWIN = 0x10;
        public const ushort IOAPICID = 0x00;
        public const ushort IOAPICVER = 0x01;
        public const ushort IOAPICARB = 0x02;
        public const ushort IOREDTBL = 0x10;

        /// <summary>
        /// Ver port.
        /// </summary>
        public readonly MMIO Ver;
        /// <summary>
        /// RegSel port.
        /// </summary>
        public readonly MMIO RegSel;
        /// <summary>
        /// Win port.
        /// </summary>
        public readonly MMIO Win;
        /// <summary>
        /// RedTbl port.
        /// </summary>
        public readonly MMIO RedTbl;

        /// <summary>
        /// Create new instance of the <see cref="IOAPIC"/> class.
        /// </summary>
        /// <param name="baseAddress">IO APIC Base Address.</param>
        internal IOAPICMMIOGroup(uint baseAddress)
        {
            Ver = new MMIO(baseAddress, IOAPICVER);
            RegSel = new MMIO(baseAddress, IOREGSEL);
            Win = new MMIO(baseAddress, IOWIN);
            RedTbl = new MMIO(baseAddress, IOREDTBL);
        }
    }
}
