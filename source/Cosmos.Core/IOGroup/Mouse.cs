using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// Mouse class. See also: <seealso cref="IOGroup"/>.
    /// </summary>
    public class Mouse : IOGroup
    {
        /// <summary>
        /// Data port.
        /// </summary>
        public readonly IOPort p60 = new IOPort(0x60);
        /// <summary>
        /// Indicator port, used to tell if data came from keyboard or mouse.
        /// </summary>
        public readonly IOPort p64 = new IOPort(0x64);
    }
}
