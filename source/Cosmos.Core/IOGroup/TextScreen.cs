using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    /// <summary>
    /// IOGroup text screen.
    /// </summary>
    public class TextScreen : IOGroup {
        /// <summary>
        /// Memory.
        /// </summary>
        public MemoryBlock Memory = new MemoryBlock(0xB8000, 80 * 25 * 2);
        // These should probably move to a VGA class later, or this class should be remade into a VGA class
        /// <summary>
        /// Misc. output.
        /// </summary>
        public readonly ushort MiscOutput = 0x03C2;
        /// <summary>
        /// First IOPort index.
        /// </summary>
        public readonly ushort Idx1 = 0x03C4;
        /// <summary>
        /// First IOPort data.
        /// </summary>
        public readonly ushort Data1 = 0x03C5;
        /// <summary>
        /// Second IOPort index.
        /// </summary>
        public readonly ushort Idx2 = 0x03CE;
        /// <summary>
        /// Second IOPort data.
        /// </summary>
        public readonly ushort Data2 = 0x03CF;
        /// <summary>
        /// Third IOPort index.
        /// </summary>
        public readonly ushort Idx3 = 0x03D4;
        /// <summary>
        /// Third IOPort data.
        /// </summary>
        public readonly ushort Data3 = 0x03D5;
    }
}
