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
        public readonly IOPort Data0 = new IOPort(0x40);
        /// <summary>
        /// Channel 1 data port.
        /// </summary>
        public readonly IOPort Data1 = new IOPort(0x41);
        /// <summary>
        /// Channel 2 data port.
        /// </summary>
        public readonly IOPort Data2 = new IOPort(0x42);
        /// <summary>
        /// Command register port.
        /// </summary>
        public readonly IOPortWrite Command = new IOPortWrite(0x43);
    }
}
