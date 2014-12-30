using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos.Debug.Kernel;
using Cosmos.HAL;
using Cosmos.HAL.Drivers.PCI.Network;
using Cosmos.System.Network;
using Cosmos.System.Network.IPv4;
using PlaygroundHAL;

namespace PlaygroundSystem
{
    public class SystemGlobal
    {
        public static void Execute()
        {
            new Debugger("test", "test").Break();
            new Debugger("test", "test").SendMessageBox("");
            HALGlobal.Test();
            //Console.WriteLine("Finding PCI device");
            //var xNicDev = PCI.GetDevice(0x1022, 0x2000);
            //if (xNicDev == null)
            //{
            //    Console.WriteLine("  Not found!!");
            //    return;
            //}

            //var xNicDevNormal = xNicDev as PCIDeviceNormal;
            //if (xNicDevNormal == null)
            //{
            //    Console.WriteLine("  Unable to cast as PCIDeviceNormal!");
            //    return;
            //}
            //var xNic = new AMDPCNetII(xNicDevNormal);
            //NetworkStack.Init();
            //xNic.Enable();
            //NetworkStack.ConfigIP(xNic, new Config(new Address(192, 168, 17, 100), new Address(255, 255, 255, 0)));

            //var xClient = new UdpClient(15);
            //xClient.Connect(new Address(192, 168, 17, 1), 25);
            //xClient.Send(new byte[]
            //             {
            //                 1,
            //                 2,
            //                 3,
            //                 4,
            //                 5,
            //                 6,
            //                 7,
            //                 8,
            //                 9,
            //                 0xAA,
            //                 0xBB,
            //                 0xCC,
            //                 0xDD,
            //                 0xEE,
            //                 0xFF
            //             });

            //while (true)
            //{
            //    NetworkStack.Update();

            //    Console.WriteLine("Done");
            //    Console.ReadLine();
            //}
        }
    }
}
