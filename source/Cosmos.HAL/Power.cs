using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Core;

namespace Cosmos.HAL
{
    public class Power
    {
        public static void Reboot()
        {
            byte good = 0x02;
            while ((good & 0x02) != 0)
                good = new IOPort(0x64).Byte;
            new IOPort(0x64).Byte = 0xFE;
            Core.Global.CPU.Halt();
        }
    }
}
