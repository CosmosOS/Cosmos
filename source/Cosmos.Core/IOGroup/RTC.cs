namespace Cosmos.Core.IOGroup
{
    public class RTC : IOGroup
    {
        public readonly IOPort Address = new IOPort(0x70);
        public readonly IOPort Data = new IOPort(0x71);
    }
}