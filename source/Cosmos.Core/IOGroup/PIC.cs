using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    public class PIC : IOGroup
    {
        public readonly IOPort Cmd = new IOPort(0x20);
        public readonly IOPort Data = new IOPort(0x21);

        internal PIC(bool aSlave)
        {
            byte aBase = (byte) (aSlave ? 0xA0 : 0x20);
            Cmd = new IOPort(aBase);
            Data = new IOPort((byte) (aBase + 1));
        }
    }
}
