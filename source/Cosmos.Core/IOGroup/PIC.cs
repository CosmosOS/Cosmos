using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// PIC class. Represent PIC.
    /// </summary>
    public class PIC : IOGroup
    {
        /// <summary>
        /// Command port.
        /// </summary>
        public readonly IOPort Cmd = new IOPort(0x20);
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly IOPort Data = new IOPort(0x21);

        /// <summary>
        /// Create new instance of the <see cref="PIC"/> class.
        /// </summary>
        /// <param name="aSlave">True if slave.</param>
        internal PIC(bool aSlave)
        {
            byte aBase = (byte) (aSlave ? 0xA0 : 0x20);
            Cmd = new IOPort(aBase);
            Data = new IOPort((byte) (aBase + 1));
        }
    }
}
