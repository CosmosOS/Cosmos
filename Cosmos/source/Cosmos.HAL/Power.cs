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
            Core.Global.CPU.Reboot();
        }
    }
}
