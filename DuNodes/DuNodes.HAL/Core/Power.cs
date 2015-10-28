using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuNodes.HAL.Core
{
    public class Power
    {
        public static void Halt()
        {
            Cosmos.Core.Global.CPU.Halt();
        }
    }
}
