using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// Local APIC Timer IOGroup class.
    /// </summary>
    public class APICTimerIOGroup : IOGroup
    {
        /// <summary>
        /// Id port.
        /// </summary>
        public readonly IOPort Timer;
        /// <summary>
        /// Id port.
        /// </summary>
        public readonly IOPort TimerDiv;
        /// <summary>
        /// Id port.
        /// </summary>
        public readonly IOPort TimerInitCount;
        /// <summary>
        /// Id port.
        /// </summary>
        public readonly IOPort TimerCurrentCount;

        /// <summary>
        /// Create new instance of the <see cref="APIC"/> class.
        /// </summary>
        internal APICTimerIOGroup()
        {
            Timer = new IOPort(0x320);
            TimerDiv = new IOPort(0x3e0);
            TimerInitCount = new IOPort(0x380);
            TimerCurrentCount = new IOPort(0x390);
        }
    }
}
