using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup.Network
{
    /// <summary>
    /// AMD PCNET II PCI (AM79C970A) chip IOGroup class.
    /// </summary>
    public class AMDPCNetIIIOGroup
    {
        /// <summary>
        /// Register address port.
        /// </summary>
        public readonly IOPort RegisterAddress;
        /// <summary>
        /// Register data port.
        /// </summary>
        public readonly IOPort RegisterData;
        /// <summary>
        /// Bus data port.
        /// </summary>
        public readonly IOPort BusData;
        /// <summary>
        /// MAC1 port.
        /// </summary>
        public readonly IOPortRead MAC1;
        /// <summary>
        /// MAC2 port.
        /// </summary>
        public readonly IOPortRead MAC2;

        /// <summary>
        /// Create new instance of the AMDPCNetIIIOGroup class.
        /// </summary>
        /// <param name="baseAddress">Chip base address.</param>
        public AMDPCNetIIIOGroup(ushort baseAddress)
        {
            RegisterAddress = new IOPort(baseAddress, 0x14);
            RegisterData = new IOPort(baseAddress, 0x10);
            BusData = new IOPort(baseAddress, 0x1C);
            MAC1 = new IOPortRead(baseAddress, 0x00);
            MAC2 = new IOPortRead(baseAddress, 0x04);
        }
    }
}
