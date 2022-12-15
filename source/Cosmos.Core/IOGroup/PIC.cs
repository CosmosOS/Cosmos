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
        public readonly ushort Cmd = 0x20;
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly ushort Data = 0x21;

        /// <summary>
        /// Create new instance of the <see cref="PIC"/> class.
        /// </summary>
        /// <param name="aSlave">True if slave.</param>
        internal PIC(bool aSlave)
        {
            if (aSlave)
            {
                Cmd = 0xA0;
                Data = 0xA1;
            }
        }
    }
}
