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
        public readonly int Port60 = 0x60;
    }
}
