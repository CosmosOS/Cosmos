using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;

namespace Cosmos.Core.IOGroup.Network
{
    public class AMDPCNetIIIOGroup
    {
        public readonly IOPort RegisterAddress;
        public readonly IOPort RegisterData;
        public readonly IOPort BusData;
        public readonly IOPortRead MAC1;
        public readonly IOPortRead MAC2;

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
