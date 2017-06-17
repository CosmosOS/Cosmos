using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmos.System
{
    public class Power
    {
        public static void CPUReboot()
        {
            HAL.Power.CPUReboot();
        }
        public static void Reboot()
        {
            HAL.Power.Reboot();
        }
        public static void Shutdown()
        {
            HAL.Power.Shutdown();
        }
    }
}
