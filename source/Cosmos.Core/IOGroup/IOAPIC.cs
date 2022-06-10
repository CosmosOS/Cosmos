using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// IO APIC IOGroup class.
    /// </summary>
    public class IOAPICIOGroup : IOGroup
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
        public readonly IOPort Ver;
        /// <summary>
        /// RegSel port.
        /// </summary>
        public readonly IOPort RegSel;
        /// <summary>
        /// Win port.
        /// </summary>
        public readonly IOPort Win;
        /// <summary>
        /// RedTbl port.
        /// </summary>
        public readonly IOPort RedTbl;

        /// <summary>
        /// Create new instance of the <see cref="IOAPIC"/> class.
        /// </summary>
        /// <param name="baseAddress">IO APIC Base Address.</param>
        internal IOAPICIOGroup(ushort baseAddress)
        {
            Ver = new IOPort(baseAddress, IOAPICVER);
            RegSel = new IOPort(baseAddress, IOREGSEL);
            Win = new IOPort(baseAddress, IOWIN);
            RedTbl = new IOPort(baseAddress, IOREDTBL);
        }
    }
}
