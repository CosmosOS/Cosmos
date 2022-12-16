using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup {
    /// <summary>
    /// Peripheral Component Interconnect (PCI) class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class PCI : IOGroup {
        /// <summary>
        /// Configuration address port.
        /// </summary>
        public readonly int ConfigAddressPort = 0xCF8;
        /// <summary>
        /// Configuration data port.
        /// </summary>
        public readonly int ConfigDataPort = 0xCFC;
    }
}
