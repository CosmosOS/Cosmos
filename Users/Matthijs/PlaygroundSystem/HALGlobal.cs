using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Network;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;

namespace PlaygroundSystem
{
    public class HALGlobal
    {
        public static void Execute()
        {
            Console.WriteLine("Finding PCI device");
            var xNicDev = PCI.GetDevice(0x1022, 0x2000);
            if (xNicDev == null)
            {
                Console.WriteLine("  Not found!!");
                return;
            }

            var xNicDevNormal = xNicDev as PCIDeviceNormal;
            if (xNicDevNormal == null)
            {
                Console.WriteLine("  Unable to cast as PCIDeviceNormal!");
                return;
            }
            //var xNic = new AMDPCNetII(xNicDevNormal);
            //NetworkStack.ConfigIP(xNic, new Config(new Address(), ));
            //xNic.Enable()
        }
    }
}