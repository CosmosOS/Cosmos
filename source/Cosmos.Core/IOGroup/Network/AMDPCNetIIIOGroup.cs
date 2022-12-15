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
        public readonly ushort RegisterAddress;
        /// <summary>
        /// Register data port.
        /// </summary>
        public readonly ushort RegisterData;
        /// <summary>
        /// Bus data port.
        /// </summary>
        public readonly ushort BusData;
        /// <summary>
        /// MAC1 port.
        /// </summary>
        public readonly ushort MAC1;
        /// <summary>
        /// MAC2 port.
        /// </summary>
        public readonly ushort MAC2;

        /// <summary>
        /// Create new instance of the AMDPCNetIIIOGroup class.
        /// </summary>
        /// <param name="baseAddress">Chip base address.</param>
        public AMDPCNetIIIOGroup(ushort baseAddress)
        {
            RegisterAddress = (ushort)(baseAddress + 0x14);
            RegisterData = (ushort)(baseAddress + 0x10);
            BusData = (ushort)(baseAddress + 0x1C);
            MAC1 = baseAddress;
            MAC2 = (ushort)(baseAddress + 0x04);
        }
    }
}
