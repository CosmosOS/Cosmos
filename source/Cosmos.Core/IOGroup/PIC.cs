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
        public readonly ushort Cmd;
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly ushort Data;

        /// <summary>
        /// Create new instance of the <see cref="PIC"/> class.
        /// </summary>
        /// <param name="aSlave">True if slave.</param>
        internal PIC(bool aSlave)
        {
            ushort aBase = (ushort) (aSlave ? 0xA0 : 0x20);
            Cmd = aBase;
            Data = (ushort)(aBase + 1);
        }
    }
}
