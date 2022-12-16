namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// PIC class. Represent PIC.
    /// </summary>
    public class PIC
    {
        /// <summary>
        /// Command port.
        /// </summary>
        public readonly int Cmd;
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly int Data;

        /// <summary>
        /// Create new instance of the <see cref="PIC"/> class.
        /// </summary>
        /// <param name="aSlave">True if slave.</param>
        internal PIC(bool aSlave)
        {
            int aBase = aSlave ? 0xA0 : 0x20;
            Cmd = aBase;
            Data = aBase + 1;
        }
    }
}
