using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup
{
    /// <summary>
    /// PC Speaker.
    /// </summary>
    public class PCSpeaker : IOGroup
    {
        // IO Port 61, channel 2 gate
        /// <summary>
        /// Gate IO port.
        /// </summary>
        public readonly int Gate = 0x61;
        // These two ports are shared with the PIT, so names are the same
        // IO Port 43
        /// <summary>
        /// Command register IO port.
        /// </summary>
        public readonly int CommandRegister = 0x43;
        // IO Port 42
        /// <summary>
        /// Channel to data IO port.
        /// </summary>
        public readonly int Channel2Data = 0x42;
    }
}
