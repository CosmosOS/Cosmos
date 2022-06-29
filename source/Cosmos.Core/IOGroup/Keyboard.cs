using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    /// <summary>
    /// Keyboard class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class Keyboard : IOGroup {
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly IOPort Port60 = new IOPort(0x60);
    }
}
