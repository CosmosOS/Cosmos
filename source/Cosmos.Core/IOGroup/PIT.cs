using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    /// <summary>
    /// Programmable Interval Timer (PIT) class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class PIT : IOGroup {
        /// <summary>
        /// Channel 0 data port.
        /// </summary>
        public readonly ushort Data0 = 0x40;
        /// <summary>
        /// Channel 1 data port.
        /// </summary>
        public readonly ushort Data1 = 0x41;
        /// <summary>
        /// Channel 2 data port.
        /// </summary>
        public readonly ushort Data2 = 0x42;
        /// <summary>
        /// Command register port.
        /// </summary>
        public readonly ushort Command = 0x43;
    }
}
