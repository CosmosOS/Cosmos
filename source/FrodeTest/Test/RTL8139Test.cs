using System;
using System.Collections.Generic;
using System.Text;
using Cosmos.Hardware.Network.Devices.RTL8139;
using Cosmos.Sys.Network;

namespace FrodeTest.Test
{
    public class RTL8139Test
    {
        public static void RunTest()
        {
            // Testing RTL8139 PCI networkcard
            //Load card
            var xNics = Cosmos.Hardware.Network.NetworkDevice.NetworkDevices;
            if (xNics.Count == 0)
            {
                Console.WriteLine("No network cards found");
                return;
            }

            var xNic = xNics[0];

            xNic.Enable();

            byte[] xNetworkData = { (byte)'a' };
            //var xPingOut = new ICMPPacket(1, 2, ICMPPacket.ICMPType.EchoRequest, xPingData, 0);

            xNic.QueueBytes(xNetworkData);
        }
    }
}
